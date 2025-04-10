using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.CommonClasses;
using GroupExpenseManagement01.Models;
using GroupExpenseManagement01.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net.Mime;

namespace GroupExpenseManagement01.Controllers
{
    [CheckAccess]
    public class ExpenseContributionsController : Controller
    {
        #region Configuration
        private IConfiguration configuration;
        private IEmailSender emailSender;
        private readonly IEncryptionService _encryptionService;

        public ExpenseContributionsController(IEmailSender emailSender, IConfiguration configuration, IEncryptionService encryptionService)
        {
            this.configuration = configuration;
            this.emailSender = emailSender;
            this._encryptionService = encryptionService;
        }
        #endregion

        #region Contribution List
        [Route("~/Group/Splitting")]
        public IActionResult Index(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            ViewData["GroupName"] = GetGroupName(GroupID);
            ViewData["GroupIDString"] = GroupIDString;
            return View(ExpenseRepository.SelectByPkWithCurrencyName("PR_ExpenseContributions_SelectByGroup", GroupID, "GroupID"));
        }
        #endregion

        #region Group Settlements
        [Route("~/Group/Settlement")]
        public IActionResult Settlement(String GroupIDString) {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            ViewData["GroupName"] = GetGroupName(GroupID);
            ViewData["GroupIDString"] = GroupIDString;
            return View(ExpenseRepository.SelectByPkWithCurrencyName("PR_Settlements_SelectByGroupPK", GroupID, "GroupID"));
        }
        #endregion

       

        [Route("~/Creditors")]
        public IActionResult Creditors()
        {
            return View(CommonClass.SelectByPk("PR_Creditors_SelectByUserID", Convert.ToInt32(CV.UserID()), "UserID"));
        }

        [Route("~/Debtors")]
        public IActionResult Debtors()
        {
            return View(CommonClass.SelectByPk("PR_Debtors_SelectByUserID", Convert.ToInt32(CV.UserID()), "UserID"));
        }

        #region Export To Excel
        public IActionResult ExportToExcel(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);

            Stream stream = CommonClass.ExportToExcel("PR_ExpenseContributions_SelectByGroup", GroupID, "GroupID");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", GetGroupName(GroupID) + "'s Splitting List.xlsx");

        }

        public IActionResult ExportToExcelSettlement(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);

            Stream stream = CommonClass.ExportToExcel("PR_Settlements_SelectByGroupPK", GroupID, "GroupID");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", GetGroupName(GroupID) + "'s Settlements.xlsx");

        }

        public IActionResult ExportToExcelCreditors()
        {
            Stream stream = CommonClass.ExportToExcel("PR_Creditors_SelectByUserID", Convert.ToInt32(CV.UserID()), "UserID");
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", CV.UserName() + "'s Creditors List.xlsx");
        }

        public IActionResult ExportToExcelDebtors()
        {
            Stream stream = CommonClass.ExportToExcel("PR_Debtors_SelectByUserID", Convert.ToInt32(CV.UserID()), "UserID");
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", CV.UserName() + "'s Debtors List.xlsx");
        }
        #endregion

        #region Mail
        public async Task<IActionResult> Mail(string GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            
            Stream stream = CommonClass.ExportToExcel("PR_ExpenseContributions_SelectByGroup", GroupID, "GroupID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("GroupExpense", "Expense", new { GroupIDString = GroupIDString });
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{GetGroupName(GroupID)}'s Splitting List.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("GroupExpense", "Expense", new { GroupIDString = GroupIDString });
        }

        public async Task<IActionResult> MailSettlement(string GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);

            Stream stream = CommonClass.ExportToExcel("PR_Settlements_SelectByGroupPK", GroupID, "GroupID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("GroupExpense", "Expense", new { GroupIDString = GroupIDString });
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{GetGroupName(GroupID)}'s Settlements.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("GroupExpense", "Expense", new { GroupIDString = GroupIDString });
        }

        public async Task<IActionResult> MailDebtors()
        {
            
            Stream stream = CommonClass.ExportToExcel("PR_Debtors_SelectByUserID", Convert.ToInt32(CV.UserID()), "UserID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("Debtors");
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{CV.UserName()}'s Debtors List.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("Debtors");
        }

        public async Task<IActionResult> MailCreditors()
        {

            Stream stream = CommonClass.ExportToExcel("PR_Creditors_SelectByUserID", Convert.ToInt32(CV.UserID()), "UserID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("Creditors");
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{CV.UserName()}'s Creditors List.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("Creditors");
        }
        #endregion

        #region Helper Methods
        public string GetGroupName(int groupId)
        {
            string groupName = string.Empty;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Group_Name";

                    // Add the GroupID parameter
                    sqlCommand.Parameters.AddWithValue("@GroupID", groupId);

                    // Execute the query and retrieve the group name
                    object result = sqlCommand.ExecuteScalar();
                    if (result != null)
                    {
                        groupName = result.ToString();
                    }
                }
            }

            return groupName;
        }

        #endregion
    }
}
