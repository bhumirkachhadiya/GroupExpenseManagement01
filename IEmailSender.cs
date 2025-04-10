using System.Net.Mail;

namespace GroupExpenseManagement01
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message, List<Attachment> attachments);

        Task SendEmailAsync2(string message);

        Task SendEmailAsync3(string email, string subject, string message);
    }
}
