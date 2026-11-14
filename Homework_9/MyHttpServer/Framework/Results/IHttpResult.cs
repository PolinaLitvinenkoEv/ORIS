using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    /// <summary>
    /// Контракт HTTP-результата, который умеет самостоятельно
    /// записать себя в ответ.
    /// </summary>
    public interface IHttpResult
    {
        /// <summary>
        /// Записывает результат в HTTP-ответ.
        /// </summary>
        Task ExecuteAsync(
            HttpListenerResponse response,
            CancellationToken cancellationToken = default);
    }
}
