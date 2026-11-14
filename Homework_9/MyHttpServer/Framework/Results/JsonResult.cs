using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    public sealed class JsonResult : IHttpResult
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly object? _data;
        private readonly int _statusCode;

        public JsonResult(object? data, int statusCode = 200)
        {
            _data = data;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(
            HttpListenerResponse response,
            CancellationToken cancellationToken = default)
        {
            if (response.OutputStream == null)
                return;

            string json = JsonSerializer.Serialize(_data, Options);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.StatusCode = _statusCode;
            response.ContentType = "application/json; charset=utf-8";
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
