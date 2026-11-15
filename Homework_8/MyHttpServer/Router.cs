using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer
{
    public class Router
    {
        private readonly List<IController> _controllers = new();

        public Router()
        {
           
        }

        public async Task<bool> TryHandleAsync(HttpListenerContext context)
        {
            string path = context.Request.Url?.LocalPath ?? "/";

            foreach (var controller in _controllers)
            {
                if (!controller.CanHandle(path))
                    continue;

                bool handled = await controller.HandleAsync(context);

                if (handled)
                    return true;
            }

            return false;
        }
    }
}
