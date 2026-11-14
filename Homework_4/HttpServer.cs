using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyHttpServer;

namespace MyHttpServer
{
    public class HttpServer
    {
        private HttpListener _server = new();
        private string _defaultPage = "bonx/index.html";
        private Router _router = new();
        private AttributeRouter _attrRouter = new();

        public async Task Start()
        {
            if (!File.Exists("settings.json"))
            {
                Console.WriteLine("Введите путь");
                File.WriteAllText("settings.json", Console.ReadLine() ?? "");
            }

            string settingsJson = File.ReadAllText("settings.json");
            Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson)
                ?? throw new Exception("Не удалось прочитать settings.json");

            string uriPrefix = setting.Prefixes[0];

            _server.Prefixes.Add(uriPrefix);
            _server.Start();
            Console.WriteLine("Сервер запущен и слушает: " + uriPrefix);

            await Listen();
        }

        public async Task Listen()
        {
            while (true)
            {
                Console.WriteLine("Введите 'stop', чтобы остановить выполнение");
                Task<string?> consoleTask = Task.Run(() => Console.ReadLine());
                Task<HttpListenerContext> requestTask = _server.GetContextAsync();

                Task completedTask = await Task.WhenAny(consoleTask, requestTask);

                if (completedTask == consoleTask)
                {
                    string? command = await consoleTask;
                    if (command?.ToLower().Trim() == "stop")
                    {
                        Stop();
                        break;
                    }
                }
                else
                {
                    HttpListenerContext context = await requestTask;
                    _ = HandleRequestAsync(context);
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                if (await _attrRouter.TryHandleAsync(context))
                    return;

                if (await _router.TryHandleAsync(context))
                {
                    return;
                }
                string path = request.Url!.LocalPath;

                if (path.EndsWith("/"))
                {
                    path += _defaultPage;
                }
                path = Uri.UnescapeDataString(path);

                string direct = Directory.GetCurrentDirectory() + $"/static{path}";
                if (!File.Exists(direct))
                {
                    string inBonx = Directory.GetCurrentDirectory() + $"/static/bonx{path}";
                    if (File.Exists(inBonx))
                        path = "/bonx" + path;
                }

                string filePath = Directory.GetCurrentDirectory() + $"/static{path}";
                if (!File.Exists(filePath))
                {
                    response.StatusCode = 404;
                    filePath = Directory.GetCurrentDirectory() + "/static/404.html";

                    if (!File.Exists(filePath))
                    {
                        await WriteText(response, 404, "404 Not Found");
                        return;
                    }
                }

                response.ContentType = ContentTypeHelper.GetContentType(filePath);

                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");

                try { response.StatusCode = 500; response.Close(); } catch { }
            }
        }

        private async Task WriteText(HttpListenerResponse response, int status, string text)
        {
            byte[] buf = Encoding.UTF8.GetBytes(text);
            response.StatusCode = status;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buf.Length;
            using Stream output = response.OutputStream;
            await output.WriteAsync(buf);
            response.Close();
        }

        public void Stop()
        {
            if (_server.IsListening)
                _server.Stop();
            Console.WriteLine("Работа сервера остановлена");
        }
    }
}