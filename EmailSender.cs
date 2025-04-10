using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace GroupExpenseManagement01
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }


        //With Attachments
        public async Task SendEmailAsync(string email, string subject, string message, List<Attachment> attachments)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");

            using (var client = new System.Net.Mail.SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"])))
            {
                client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
                client.EnableSsl = true;

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpSettings["Username"]);
                    mailMessage.To.Add(email); // You can add multiple recipients by repeating this line
                    mailMessage.Subject = subject;
                    mailMessage.Body = message;
                    mailMessage.IsBodyHtml = true; // If sending an HTML email, set to true

                    foreach (var attachment in attachments)
                    {
                        mailMessage.Attachments.Add(attachment);
                    }
                    await client.SendMailAsync(mailMessage);
                }
            }
        }

        //To Admin
        public async Task SendEmailAsync2(string message)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");

            using (var client = new System.Net.Mail.SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"])))
            {
                client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
                client.EnableSsl = true;

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpSettings["Username"]);
                    mailMessage.To.Add(smtpSettings["ManagerEmail"]); // You can add multiple recipients by repeating this line
                    mailMessage.Subject = "FeedBack!!";
                    mailMessage.Body = message;
                    mailMessage.IsBodyHtml = true; // If sending an HTML email, set to true

                    /*foreach (var attachment in attachments)
                    {
                        mailMessage.Attachments.Add(attachment);
                    }*/
                    await client.SendMailAsync(mailMessage);
                }
            }
        }

        //To Users
        public async Task SendEmailAsync3(string email, string subject, string message)
        //, List<Attachment> attachments)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");

            using (var client = new System.Net.Mail.SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"])))
            {
                client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
                client.EnableSsl = true;

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpSettings["Username"]);
                    mailMessage.To.Add(email); // You can add multiple recipients by repeating this line
                    mailMessage.Subject = subject;
                    mailMessage.Body = message;
                    mailMessage.IsBodyHtml = true; // If sending an HTML email, set to true

                    await client.SendMailAsync(mailMessage);
                }
            }
        }
    }
}
