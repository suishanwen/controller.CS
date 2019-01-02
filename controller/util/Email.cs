using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace controller.util
{
    class Email
    {
        public static bool Send(string receiver, string title, string content)
        {
            //实例化一个发送邮件类。
            MailMessage mailMessage = new MailMessage();
            //发件人邮箱地址，方法重载不同，可以根据需求自行选择。
            mailMessage.From = new MailAddress("controlservice@sina.com");
            //收件人邮箱地址。
            mailMessage.To.Add(new MailAddress(receiver));
            //邮件标题。
            mailMessage.Subject = title;
            //邮件内容。
            mailMessage.Body = content;
            mailMessage.IsBodyHtml = true;
            //实例化一个SmtpClient类。
            SmtpClient client = new SmtpClient();
            //在这里我使用的是sina邮箱，所以是smtp.sina.com
            client.Host = "smtp.sina.com";
            //使用安全加密连接。
            client.EnableSsl = true;
            //不和请求一块发送。
            client.UseDefaultCredentials = false;
            //验证发件人身份(发件人的邮箱，邮箱里的生成授权码);
            client.Credentials = new NetworkCredential("controlservice@sina.com", "a123456");
            //发送
            try
            {
                client.Send(mailMessage);
                return true;
            }catch(Exception e){
                return false;
            }
        }
    }
}
