using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.CommonClasses;
using GroupExpenseManagement01.Models;
using Microsoft.AspNetCore.Mvc;

namespace GroupExpenseManagement01.Controllers
{
    [CheckAccess]
    public class ContactUsController : Controller
    {
        #region Configuration
        private IEmailSender emailSender;

        public ContactUsController(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }
        #endregion

        #region Contact Page
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region FeedBack Mails
        [HttpPost]
        public async Task<IActionResult> Mail(IFormCollection data)
        {

            if (!string.IsNullOrEmpty(data["Subject"].ToString()) && !string.IsNullOrEmpty(data["MSG"].ToString()))
            {
                #region To Manager
                try
                {
                    //To manager
                    await emailSender.SendEmailAsync2(HtmlTemplate.FeedBackToManagers(CV.UserName(), CV.Email(), data["Subject"], data["MSG"]));
                    // Redirect to the Portfolio page after successful email sending
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "We couldn't receive your message. Please try again later or check your input.";
                    return View("Index");
                }
                #endregion

                #region To User
                try
                {
                    //To user
                    await emailSender.SendEmailAsync3(CV.Email(), "Feedback Received", HtmlTemplate.AckFeedBack(CV.UserName()));
                }
                catch (Exception ex)
                {
                    //ACKFeedBack
                }
                #endregion

                TempData["Success"] = "Your message has been successfully received! Thank you for reaching out.";
                return View("Index");
            }
            else{
                TempData["Error"] = "Enter All Details";
                return View("Index");
            }
        }
        #endregion
    }
}
