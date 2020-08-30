using ASP_Project_DHS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;


namespace ASP_Project_DHS.Helpers
{
    public class CurrentContext
    {
        public static bool IsLogged()
        {
            var flag = HttpContext.Current.Session["isLogin"];

            if (flag == null || (int)flag == 0)
            {
                if (HttpContext.Current.Request.Cookies["userIdCookie"] != null)
                {
                    int userIdCookie = Convert.ToInt32((HttpContext.Current.Request.Cookies["userIdCookie"].Value));
                    using (var ctx = new QLDauGiaEntities())
                    {
                        var user = ctx.TaiKhoan
                            .Where(u => u.ID == userIdCookie)
                            .FirstOrDefault();
                        HttpContext.Current.Session["isLogin"] = 1;
                        HttpContext.Current.Session["user"] = user;
                    }

                    return true;
                }
                return false;
            }
            return true;
        }

        public static TaiKhoan GetCurUser()
        {
            return (TaiKhoan)HttpContext.Current.Session["user"];
        }

        public static void Destroy()
        {
            HttpContext.Current.Session["isLogin"] = 0;
            HttpContext.Current.Session["user"] = null;
            HttpContext.Current.Response.Cookies["userIdCookie"].Expires = DateTime.Now.AddDays(-1);
        }

        public static string MaHoaEmail(string s)
        {
            int i = 0;
            var kq = new List<char>();
            while (true)
            {
                if (s[i + 3] == '@')
                    break;
                else
                    kq.Add('*');
                i++;
            }
            kq.AddRange(s.Skip(i)
                .Take(s.Length)
                .ToList());

            string kqq = "";

            foreach (var item in kq)
            {
                kqq += item;
            }

            return kqq;
        }
        public static string MaHoaTen(string s)
        {
            string kqq = "";

            for (int i = 0; i < s.Length; i++)
            {
                if (i % 2 != 0)
                    kqq += '*';
                else
                    kqq += s[i];
            }
            return kqq;
        }

        public static void SendMail(string to, string body)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.gmail.com";
                client.Port = 587;

                client.UseDefaultCredentials = false;
                //If you need to authenticate
                client.Credentials = new NetworkCredential("maildaugia123@gmail.com", "chungtoi");
                client.EnableSsl = true;

                MailMessage mailMessage = new MailMessage();
                MailAddress d = new MailAddress("maildaugia123@gmail.com");
                mailMessage.From = d;
                mailMessage.To.Add(to);
                mailMessage.Subject = "DHS - Thông báo!";
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                client.Send(mailMessage);
            }
            catch (Exception)
            {
            }

        }
    }
}