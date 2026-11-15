using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Attributes;

namespace MyHttpServer.Controller
{
    [HttpController("home-work")]
    public class HomeWorkController
    {
        [Get("send")]
        public void Send(string to)
        {
            try
            {
                string group = "11-541";
                string fullName = "Литвиненко Полина Евгеньевна";

                string projectZipPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "static",
                    "Homework_5.zip");

                MailAddress from = new MailAddress("polina_lit00@mail.ru", "PolinaLitvinenko");
                MailAddress toAddress = new MailAddress(to);

                MailMessage message = new MailMessage(from, toAddress);

                message.Subject = $"Домашняя работа: {fullName}, {group}  ";

                message.Body =
                    $"<p>Домашняя работа</p>" +
                    $"<p>Группа: {group}</p>" +
                    $"<p>ФИО: {fullName}</p>";

                message.IsBodyHtml = true;

                if (File.Exists(projectZipPath))
                {
                    message.Attachments.Add(new Attachment(projectZipPath));
                }
                else
                {
                    Console.WriteLine($"Архив не найден: {projectZipPath}");
                }

                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 25);
                smtp.Credentials = new NetworkCredential("polina_lit00@mail.ru", "l3BHFOTRjDgDCgY1KXuO");
                smtp.EnableSsl = true;
                smtp.Send(message);

                Console.WriteLine($"Письмо отправлено на {toAddress}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
