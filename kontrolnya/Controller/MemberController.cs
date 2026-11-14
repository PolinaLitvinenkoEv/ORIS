using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MyHttpServer.Attributes;

namespace MyHttpServer.Controller
{
    [HttpController("members")]
    public class MemberController
    {
        public static List<Member> Members = new List<Member>();
        public List<Member> Members_541 = new List<Member>();
        public List<Member> Members_507 = new List<Member>();
        public List<Member> Members_other = new List<Member>();
        private static int next_id = 1;
        [Get("")]
        public void GetMembers(HttpListenerContext context)        
        {
            Members_541.Clear();
            Members_507.Clear();
            Members_other.Clear();
            foreach (var member in Members)
            {
                if (member.Group == "541")
                {
                    Members_541.Add(member);
                }
                else if (member.Group == "507")
                {
                    Members_507.Add(member);
                }
                else if (member.Group == "other")
                {
                    Members_other.Add(member);
                }
                else
                {
                    Console.WriteLine("Такой группы нет");
                    return;
                }
            }
            string html = $@"<!DOCTYPE html>
            <html lang='ru'>
            <head>
                <meta charset='utf-8'>
                <title>Участники студенческого клуба</title>
            </head>
            <body>
                <h2>Участники студенческого клуба:</h2><ul>
            ";
            if (Members.Count == 0)
            {
                html += "</ul>";
                html += "<p> Участники отсутствуют <p>";
            }
           
            if (Members_541.Count > 0)
            { 
                html += "<p> Участники из 541-й группы </p>";
                foreach (var member in Members_541)
                {
                    html += $"<li> Имя студента: {member.Name}, группа: {member.Group}, статус: {member.Status} </li>";
                }
            }
            if (Members_507.Count > 0)
            {
                html += "<p> Участники из 507-й группы </p>";
                foreach (var member in Members_507)
                {
                    html += $"<li> Имя студента: {member.Name}, группа: {member.Group}, статус: {member.Status} </li>";
                }
            }
            if (Members_other.Count > 0)
            {
                html += "<p> Участники из других групп </p>";
                foreach (var member in Members_other)
                {
                    html += $"<li> Имя студента: {member.Name}, группа: {member.Group}, статус: {member.Status} </li>";
                }
            }
            html += "</ul>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer);
            context.Response.Close();
        }
        [Post("create")]
        public void CreateMember(string name, string group, HttpListenerContext context)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 60)
            {
                byte[] err = Encoding.UTF8.GetBytes("<h3>Ошибка 400: Имя от 2 до 60 символов</h3>");
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = err.Length;
                context.Response.OutputStream.Write(err);
                context.Response.Close();
                return;
            }

            if (group != "541" && group != "507" && group != "other")
            {
                byte[] err = Encoding.UTF8.GetBytes("<h3>Ошибка 400: Неверная группа</h3>");
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = err.Length;
                context.Response.OutputStream.Write(err);
                context.Response.Close();
                return;
            }

            bool isDuplicate = Members.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && m.Group == group);
            if (isDuplicate)
            {
                byte[] err = Encoding.UTF8.GetBytes("<h3>Ошибка 409 Conflict: Такой участник уже есть</h3>");
                context.Response.StatusCode = 409;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = err.Length;
                context.Response.OutputStream.Write(err);
                context.Response.Close();
                return;
            }

            int newId = Members.Count > 0 ? Members.Max(m => m.Id) + 1 : 1;
            Members.Add(new Member(name, newId, group, "active"));

            context.Response.StatusCode = 303;
            context.Response.Headers.Add("Location", "/members");
            context.Response.Close();
        }
    }
}
