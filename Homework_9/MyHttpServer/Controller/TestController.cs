using MyHttpServer.Attributes;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Controller
{
    [HttpController("test")]
    public class TestController : BaseController
    {
        [Get("index")]
        public IHttpResult Index()
        {
            return Html("<h1>Hello from controller!</h1>");
        }

        [Get("info")]
        public IHttpResult Info()
        {
            return Json(new
            {
                Id = 1,
                Message = "Hello World!",
                Success = true
            });
        }
    }
}