using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Attributes;

namespace MyHttpServer.Controller
{
    [HttpController("steam")]
    public class SteamController
    {
        [Get("login")]
        public void LoginGet(HttpListenerContext context)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "static", "Steam.html");

            if (!File.Exists(filePath))
            {
                context.Response.StatusCode = 404;
                return;
            }

            byte[] buffer = File.ReadAllBytes(filePath);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer);
        }

        [Post("login")]
        public void Login(string login, string password, string remember, HttpListenerContext context)
        {
            Console.WriteLine();
            Console.WriteLine("-------------");
            Console.WriteLine($"Логин:  {login}");
            Console.WriteLine($"Пароль: {password}");
            Console.WriteLine($"Запомнить: {(string.IsNullOrEmpty(remember) ? "нет" : "да")}");
            Console.WriteLine($"Метод:  POST");
            Console.WriteLine($"Время:  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine("-------------");
            Console.WriteLine();

            string html = $@"<!DOCTYPE html>
<html lang='ru'>
<head>
    <meta charset='utf-8'>
    <title>Результат</title>
    <style>
        a {{
            color: #171d25;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        a:visited {{
            color: #171d25;
        }}
    </style>
</head>
<body>
    <h2>Данные получены</h2>
    <p>Логин: {WebUtility.HtmlEncode(login)}</p>
    <p>Пароль: {WebUtility.HtmlEncode(password)}</p>
    <p>Запомнить: {(string.IsNullOrEmpty(remember) ? "нет" : "да")}</p>
    <p><a href='/Steam.html'>← Назад к форме</a></p>
</body>
</html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer);
            context.Response.Close();
        }
    }
}
