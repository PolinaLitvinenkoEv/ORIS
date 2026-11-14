using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniTemplateEngine;

namespace MyHttpServer.Framework.Results
{
    public class PageResult:IHttpResult
    {
        private readonly string _templatePath;
        private readonly object _model;
        private readonly int _statusCode;

        public PageResult(string templatePath, object model, int statusCode = 200)
        {
            _templatePath = templatePath;
            _model = model;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
        {
            
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", _templatePath);

            var renderer = new HtmlTemplateRenderer(); 

            string html = renderer.RenderFromFile(fullPath, _model);

            var htmlResult = new HtmlResult(html, _statusCode);
            await htmlResult.ExecuteAsync(response, cancellationToken);
        }
    }
}
