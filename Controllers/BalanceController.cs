using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.CommonClasses;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net.Mime;

namespace GroupExpenseManagement01.Controllers
{
    [CheckAccess]
    public class BalanceController : Controller
    {
        private IEmailSender emailSender;

        public BalanceController(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View(CommonClasses.CommonClass.SelectByPk("PR_Balance_ByUser", Convert.ToInt32(CV.UserID()), "UserID"));
        }

        public IActionResult ExportToExcel()
        {
            Stream stream = CommonClass.ExportToExcel("PR_Balance_ByUser", Convert.ToInt32(CV.UserID()), "UserID");
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", CV.UserName() + "'s Balance.xlsx");
        }

        public async Task<IActionResult> Mail()
        {
            Stream stream = CommonClass.ExportToExcel("PR_Balance_ByUser", Convert.ToInt32(CV.UserID()), "UserID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("Index");
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{CV.UserName()}'s Balance.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
