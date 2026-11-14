using System;
using System.Collections.Generic;
using MyHttpServer.Attributes;
using MyHttpServer.Framework.Controllers;
using MyHttpServer.Framework.Results;
using MyHttpServer.Models;
using MiniHttpServer.ORM;
using System.Text.Json;

namespace MyHttpServer.Controller
{
    [HttpController("user")]
    public class UserController : BaseController
    {
        [Get("profile")]
        public IHttpResult Profile()
        {
            var user = new
            {
                Name = "Alex",
                Age = 20,
                Email = "alex@example.com"
            };

            return Page("User/Profile.html", user);
        }
        [Get("list")]
        public async Task<IHttpResult> List()
        {
            string settingsJson = File.ReadAllText("settings.json");
            var settings = JsonSerializer.Deserialize<Settings>(settingsJson);
            string connStr = settings?.ConnectionString ?? "";
            IOrmContext orm = new OrmContext(connStr);
            IReadOnlyList<User> users = await orm.GetAllAsync<User>("users");
            return Page("User/List.html", new { Users = users });
        }
    }
}
