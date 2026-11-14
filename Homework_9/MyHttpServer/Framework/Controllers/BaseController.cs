using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Framework.Controllers
{
    public abstract class BaseController
    {
        /// <summary>
        /// Контекст текущего HTTP-запроса. Не является статическим.
        /// </summary>
        protected internal HttpListenerContext Context { get; private set; } = null!;

        /// <summary>
        /// Устанавливает контекст конкретного HTTP-запроса.
        /// Вызывается обработчиком контроллеров перед вызовом метода действия.
        /// </summary>
        internal void SetContext(HttpListenerContext context)
        {
            Context = context;
        }

        /// <summary>Формирует HTML-результат.</summary>
        protected IHttpResult Html(string html, int statusCode = 200)
            => new HtmlResult(html, statusCode);

        /// <summary>Формирует JSON-результат.</summary>
        protected IHttpResult Json(object? data, int statusCode = 200)
            => new JsonResult(data, statusCode);
        protected IHttpResult Page(string templatePath, object model, int statusCode = 200)
            => new PageResult(templatePath, model, statusCode);
    }
}

