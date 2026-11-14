using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpControllerAttribute: Attribute
    {
        public string Prefix { get; }
        public HttpControllerAttribute(string prefix)
        {
            Prefix = prefix.Trim('/');
        }
    }
}
