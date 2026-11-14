using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MyHttpServer.Framework
{
    abstract class ControllerBase
    {
        protected HttpListenerContext Context { get; private set; }
        internal void SetContext(HttpListenerContext context)
        {
            Context = context;
        }
    }
}
