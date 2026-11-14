using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Attributes;

namespace MyHttpServer
{
    public class AttributeRouter
    {
        private readonly List<RouteEntry> _routes = new();

        private class RouteEntry
        {
            public string Method = "";
            public string Path = "";
            public object Instance = null!;
            public MethodInfo MethodInfo = null!;
        }

        public AttributeRouter()
        {
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttribute<HttpControllerAttribute>() != null);

            foreach (var type in types)
            {
                var controllerAttr = type.GetCustomAttribute<HttpControllerAttribute>()!;
                string prefix = controllerAttr.Prefix;
                object instance = Activator.CreateInstance(type)!;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var getAttr = method.GetCustomAttribute<GetAttribute>();
                    var postAttr = method.GetCustomAttribute<PostAttribute>();

                    if (getAttr != null)
                    {
                        _routes.Add(new RouteEntry
                        {
                            Method = "GET",
                            Path = "/" + prefix + "/" + getAttr.Route,
                            Instance = instance,
                            MethodInfo = method
                        });
                    }

                    if (postAttr != null)
                    {
                        _routes.Add(new RouteEntry
                        {
                            Method = "POST",
                            Path = "/" + prefix + "/" + postAttr.Route,
                            Instance = instance,
                            MethodInfo = method
                        });
                    }
                }
            }
        }

        public async Task<bool> TryHandleAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url!.LocalPath.TrimEnd('/');
            if (path == "") path = "/";
            string method = request.HttpMethod;

            var route = _routes.FirstOrDefault(r =>
                r.Method == method &&
                r.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

            if (route == null)
                return false;
            Dictionary<string, string> formData = new();
            if (method == "POST")
            {
                string body;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    body = await reader.ReadToEndAsync();
                }

                foreach (var pair in body.Split('&'))
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length != 2) continue;
                    formData[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1].Replace('+', ' '));
                }
            }
            var parameters = route.MethodInfo.GetParameters();
            object?[] args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(HttpListenerContext))
                {
                    args[i] = context;
                    continue;
                }

                string name = parameters[i].Name ?? "";

                string? val = null;
                if (formData.TryGetValue(name, out var f)) val = f;
                else if (request.QueryString[name] is string q) val = q;

                if (val != null)
                    args[i] = Convert.ChangeType(val, parameters[i].ParameterType);
                else
                    args[i] = parameters[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(parameters[i].ParameterType)
                        : null;
            }

            var result = route.MethodInfo.Invoke(route.Instance, args);

            if (result is Task task)
                await task;

            try { response.Close(); } catch { }

            return true;
        }
    }
}
