using System;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CommonTools
{
    class Send
    {
        public static void Email(string from, List<string> to, string subject, string body, bool html = false)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("FromMailAddress");
                foreach (var emailAddr in to)
                {
                    message.To.Add(new MailAddress(emailAddr));
                }
                message.Subject = subject;
                if (html){
                    message.IsBodyHtml = true;
                }
                else { message.IsBodyHtml = false; }

                message.Body = body;

                SmtpClient smtp = new SmtpClient
                {
                    Port = 587,
                    Host = "smtp.gmail.com", //for gmail host  
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("FromMailAddress", "password"),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                smtp.Send(message);
            }
            catch (Exception) { }
        }

    }
}
