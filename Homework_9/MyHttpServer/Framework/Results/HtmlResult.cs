using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    public sealed class HtmlResult : IHttpResult
    {
        private readonly string _html;
        private readonly int _statusCode;

        public HtmlResult(string html, int statusCode = 200)
        {
            _html = html ?? string.Empty;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(
            HttpListenerResponse response,
            CancellationToken cancellationToken = default)
        {
            if (response.OutputStream == null)
                return;

            byte[] buffer = Encoding.UTF8.GetBytes(_html);

            response.StatusCode = _statusCode;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.ContentEncoding = Encoding.UTF8;

            try
            {
                await response.OutputStream.WriteAsync(
                    buffer.AsMemory(0, buffer.Length),
                    cancellationToken);
                await response.OutputStream.FlushAsync(cancellationToken);
            }
            finally
            {
                try { response.Close(); } catch { }
            }
        }
    }
}
