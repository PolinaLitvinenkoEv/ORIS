using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer
{
    public interface IController
    {
        bool CanHandle(string path);
        Task<bool> HandleAsync(HttpListenerContext context);
    }
}
