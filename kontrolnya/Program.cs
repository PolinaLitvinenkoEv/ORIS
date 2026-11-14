using System.Net;
using System.Text;
using System.Text.Json;
using MyHttpServer;

HttpServer httpServer = new HttpServer();
httpServer.Start().GetAwaiter().GetResult();
