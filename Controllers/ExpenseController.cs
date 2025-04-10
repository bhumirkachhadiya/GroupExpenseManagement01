using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.CommonClasses;
using GroupExpenseManagement01.Models;
using GroupExpenseManagement01.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace GroupExpenseManagement01.Controllers
{
    [CheckAccess]
    public class ExpenseController : Controller
    {
        #region Configuration
        private IConfiguration configuration;
        private IEmailSender emailSender;
        private readonly IEncryptionService _encryptionService;
        
        public ExpenseController(IEmailSender emailSender, IConfiguration configuration, IEncryptionService encryptionService)
        {
            this.configuration = configuration;
            this.emailSender = emailSender;
            this._encryptionService = encryptionService;
        }
        #endregion

        #region Expenses List
        public IActionResult Index()
        {
            return View(CommonClass.SelectByPk("PR_Expenses_SelectByUser", Convert.ToInt32(CV.UserID()), "UserID"));
        }
        #endregion

        #region Group Expense List
        [Route("~/Group/Expense")]
        public IActionResult Index2(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            ViewData["GroupName"] =  GetGroupName(GroupID);
            ViewData["GroupIDString"] = GroupIDString;
            ViewData["ReceivableAmout"] = GetReceivableAmout(GroupID);
            ViewData["PayableAmout"] = GetPayableAmout(GroupID);
            ViewData["TotalExpenseByUser"] = GetTotalExpense(GroupID);
            ViewData["TotalExpense"] = GetTotalExpenseOfGroup(GroupID);
            ViewData["TotalMembers"] = GetTotalMembers(GroupID);
            return View(CommonClass.SelectByPk("PR_Expenses_SelectByGroup", GroupID, "GroupID"));
        }

        [Route("~/GroupExpense")]
        public IActionResult GroupExpense(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            ViewData["GroupName"] = GetGroupName(GroupID);
            ViewData["GroupIDString"] = GroupIDString;
            ViewData["ReceivableAmout"] = GetReceivableAmout(GroupID);
            ViewData["PayableAmout"] = GetPayableAmout(GroupID);
            ViewData["TotalExpenseByUser"] = GetTotalExpense(GroupID);
            ViewData["TotalExpense"] = GetTotalExpenseOfGroup(GroupID);
            ViewData["TotalMembers"] = GetTotalMembers(GroupID);
            //ViewBag.ExpenseContribution = DropdownClass.GetDropdownList<ExpenseContributions>(CommonClass.SelectByPk("PR_ExpenseContributions_SelectByGroup", GroupID, "GroupID"));
            ViewBag.ExpenseContribution = DropdownClass.GetDropdownList<ExpenseContributions>(ExpenseRepository.SelectByPkWithCurrencyName("PR_ExpenseContributions_SelectByGroup", GroupID, "GroupID"));
            ViewBag.Expenses = DropdownClass.GetDropdownList<ExpenseDetails>(CommonClass.SelectByPk("PR_Expenses_SelectByGroup", GroupID, "GroupID"));
            ViewBag.Log = DropdownClass.GetDropdownList<GroupLog>(CommonClass.SelectByPk("PR_Group_Log", GroupID, "GroupID"));
            //return View(CommonClass.SelectByPk("PR_Settlements_SelectByGroupPK", GroupID, "GroupID"));
            return View(ExpenseRepository.SelectByPkWithCurrencyName("PR_Settlements_SelectByGroupPK", GroupID, "GroupID"));
        }
        #endregion

        #region AddEditExp(Get)
        public IActionResult AddUpdateExpense(String? ExpenseIDString) {
        
            int ? ExpenseID = null;
            if (ExpenseIDString != null)
            {
                ExpenseID = _encryptionService.DecryptInteger(ExpenseIDString);
            }
            ExpenseModel modelExpense = new ExpenseModel();
            TempData["PageTitle"] = "Add Expense";
            
            #region Drop Down Category
            ViewBag.CategoryList = DropdownClass.GetDropdownList<CategoryDropDownModel>(CommonClass.SelectData("PR_Category_Selection"));
            #endregion
            #region Fetch Data

            if (ExpenseID != null && ExpenseID != 0)
            {

                DataTable table = CommonClass.SelectByPk("PR_Expenses_SelectByPK", (int)ExpenseID, "ExpenseID");

                try
                {
                    modelExpense.ExpenseID = Convert.ToInt32(table.Rows[0]["ExpenseID"]);
                    modelExpense.Amount= Convert.ToDecimal(table.Rows[0]["Amount"]);
                    modelExpense.ExpenseDate = Convert.ToDateTime(table.Rows[0]["ExpenseDate"]);
                    modelExpense.Description = table.Rows[0]["Description"].ToString();
                    modelExpense.CategoryID = Convert.ToInt32(table.Rows[0]["CategoryID"]);
                    modelExpense.UserID = Convert.ToInt32(table.Rows[0]["UserID"]);
                    modelExpense.GroupID = Convert.ToInt32(table.Rows[0]["GroupID"]);
                    ViewData["GroupName"] = GetGroupName(modelExpense.GroupID);
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Expence Not Exists!";
                    return RedirectToAction("Index");
                }
                TempData["PageTitle"] = "Update Expence";
            }
            #endregion
            else
            {
                #region Drop Down Group 
                DataTable dataTable = CommonClass.SelectByPk("PR_Group_Selection", Convert.ToInt32(CV.UserID()), "UserID");

                List<GroupDropDownModel> groupList = new List<GroupDropDownModel>();
                foreach (DataRow data in dataTable.Rows)
                {
                    GroupDropDownModel groupDropDownModel = new GroupDropDownModel();
                    groupDropDownModel.GroupID = Convert.ToInt32(data["GroupID"]);
                    groupDropDownModel.GroupName = data["GroupName"].ToString();
                    groupList.Add(groupDropDownModel);
                }
                ViewBag.GroupList = groupList;
                #endregion
            }

            return View(modelExpense);

        }
        #endregion

        #region Mail (Group)
        public async Task<IActionResult> Mail(string GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            
            Stream stream = CommonClass.ExportToExcel("PR_Expenses_SelectByGroup", GroupID, "GroupID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("GroupExpense", new { GroupIDString = GroupIDString });
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{GetGroupName(GroupID)}'s Expense.xlsx", MediaTypeNames.Application.Octet);
        
                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(BAL.CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("GroupExpense", new { GroupIDString = GroupIDString });
        }
        #endregion

        #region Export To Excel (Group)
        public IActionResult ExportToExcel(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);

            Stream stream = CommonClass.ExportToExcel("PR_Expenses_SelectByGroup", GroupID, "GroupID");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", GetGroupName(GroupID) + "'s Expense.xlsx");
        }
        #endregion

        #region Export Log
        public IActionResult ExportToExcelActivity(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);

            Stream stream = CommonClass.ExportToExcel("PR_Group_Log_Excel", GroupID, "GroupID");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", GetGroupName(GroupID) + "'s Activity.xlsx");


        }
        public async Task<IActionResult> Mail2(String GroupIDString)
        {
            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            Stream stream = CommonClass.ExportToExcel("PR_Group_Log_Excel", GroupID, "GroupID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("Index");
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, GetGroupName(GroupID) + "'s Activity.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(BAL.CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("GroupExpense", new { GroupIDString = GroupIDString });
        }

        #endregion

        #region Export To Excel 1 (User)
        public IActionResult ExportToExcel1()
        {

            Stream stream = CommonClass.ExportToExcel("PR_Expenses_SelectByUser", Convert.ToInt32(CV.UserID()), "UserID");

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", CV.UserName() + "'s Expense.xlsx");

        }
        #endregion

        #region Mail 1 (User)
        public async Task<IActionResult> Mail1()
        {
            //int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            Stream stream = CommonClass.ExportToExcel("PR_Expenses_SelectByUser", Convert.ToInt32(CV.UserID()), "UserID");

            if (stream == null)
            {
                TempData["Error"] = "Failed to generate Excel file";
                return RedirectToAction("Index");
            }

            try
            {
                // Reset stream position before attaching
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, $"{CV.UserName()}'s Expense.xlsx", MediaTypeNames.Application.Octet);

                await emailSender.SendEmailAsync(BAL.CV.Email(), "Excel File", HtmlTemplate.ExportExcel(BAL.CV.UserName()), new List<Attachment> { attachment });

                TempData["Message"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        #endregion


        #region Add Edit Expense(AddEditExpense)From Exp
        [HttpGet]
        [Route("Group/Expense/AddEditExpense")]
        public IActionResult AddEditExpense(String GroupIDString,String? ExpenseIDString) {
            int? ExpenseID = null;
            var selectedMembers = new List<int>();
            if (ExpenseIDString != null)
            {
                ExpenseID = _encryptionService.DecryptInteger(ExpenseIDString);
            }

            int GroupID = _encryptionService.DecryptInteger(GroupIDString);
            ViewData["GroupName"] = GetGroupName(GroupID);

            ExpenseModel modelExpense = new ExpenseModel();
            TempData["PageTitle"] = "Add Expense";

            #region Drop Down User
            ViewBag.UserList = DropdownClass.GetDropdownList<UserDropDownModel>(CommonClass.SelectByPk("PR_User_DropDownByGID", GroupID, "GroupID"));
            #endregion

            #region Drop Down Currency
            ViewBag.CurrencyList = DropdownClass.GetDropdownList<CurrencyDropDownModel>(CommonClass.SelectData("PR_Currency_DropDown"));
            #endregion

            #region Drop Down Category
            ViewBag.CategoryList = DropdownClass.GetDropdownList<CategoryDropDownModel>(CommonClass.SelectData("PR_Category_Selection"));
            #endregion

            modelExpense.GroupID = GroupID;
            modelExpense.CurrencyID = CV.CurrencyID();
            //FETCH
            #region Fetch Data

            if (ExpenseID.HasValue)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                //var oldMembers = new List<int>();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("PR_Expense_SelectByPK", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = ExpenseID;

                        // First result set for expense details
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                modelExpense.ExpenseID = Convert.ToInt32(reader["ExpenseID"]);
                                modelExpense.GroupID = Convert.ToInt32(reader["GroupID"]);
                                modelExpense.UserID = Convert.ToInt32(reader["UserID"]);  // User who logged the expense
                                modelExpense.Amount = Convert.ToDecimal(reader["Amount"]);
                                modelExpense.Description = reader["Description"].ToString();
                                modelExpense.ExpenseDate = Convert.ToDateTime(reader["ExpenseDate"]);
                                modelExpense.CategoryID = Convert.ToInt32(reader["CategoryID"]);
                                modelExpense.CurrencyID = Convert.ToInt32(reader["CurrencyID"]);
                            }

                            // Move to the second result set for expense members
                            if (reader.NextResult())
                            {
                                selectedMembers.Clear();
                                while (reader.Read())
                                {
                                    selectedMembers.Add(Convert.ToInt32(reader["UserID"]));
                                }

                                
                                modelExpense.SelectedMembers = selectedMembers.ToArray();
                                //OldSelectedMembers = modelExpense.SelectedMembers;
                                //modelExpense.OldSelectedMembers = selectedMembers.ToArray();
                            }
                            TempData["PageTitle"] = "Update Expense";
                        }
                    }
                }
            }
            else
            {
                // Handle the case when ExpenseID is null or zero
            }
            #endregion

            return View(modelExpense);
        }
        #endregion

        #region AddEditExpense2 (HttpPost)
        [HttpPost]
        public IActionResult AddEditExpense2(ExpenseModel modelExpense)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Determine if it's an insert or update
                        if (modelExpense.ExpenseID == 0)
                        {
                            command.CommandText = "PR_Expense_Create"; // Insert expense
                            modelExpense.UserID = Convert.ToInt32(CV.UserID());
                            command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = modelExpense.Amount;
                        }
                        else
                        {
                            command.CommandText = "PR_Expense_Update"; // Update expense
                            command.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = modelExpense.ExpenseID;
                            command.Parameters.Add("@NewAmount", SqlDbType.Decimal).Value = modelExpense.Amount;
                        }

                        // Add common parameters
                        command.Parameters.Add("@GroupID", SqlDbType.Int).Value = modelExpense.GroupID;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = modelExpense.UserID;
                        
                        command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = modelExpense.Description;
                        command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = modelExpense.CategoryID;
                        command.Parameters.Add("@CurrencyID", SqlDbType.Int).Value = modelExpense.CurrencyID;

                        // Create table-valued parameter for members involved in the expense
                        SqlParameter tvpParam = command.Parameters.AddWithValue("@Members", GetUserTable(modelExpense.SelectedMembers));
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "dbo.UserIDTableType";

                        

                        try
                        {
                            command.ExecuteNonQuery();
                            TempData["InsertUpdateMSG"] = modelExpense.ExpenseID == 0 ? "Inserted successfully!" : "Updated successfully!";
                            return RedirectToAction("GroupExpense", new { GroupIDString = _encryptionService.EncryptInteger(modelExpense.GroupID) });
                        }
                        catch (Exception ex)
                        {
                            // Log the error here (e.g., using a logging framework)
                            TempData["ErrorMessage"] = ex.Message;
                            // Optionally log the exception details for further analysis
                            /*return View("AddEditExpense",modelExpense); // Return the view with model state errors*/
                        }
                    }
                }
            }

            return RedirectToAction("AddEditExpense", new { GroupIDString =_encryptionService.EncryptInteger(modelExpense.GroupID), ExpenseIDString =_encryptionService.EncryptInteger(modelExpense.ExpenseID)});
        }

        #region Convert Into Table
        private DataTable GetUserTable(int[] selectedMembers)
        {
            DataTable membersTable = new DataTable();
            membersTable.Columns.Add("UserID", typeof(int));
            if (selectedMembers != null)
            {
                foreach (var member in selectedMembers)
                {
                    membersTable.Rows.Add(member);
                }
            }
            return membersTable;
        }
        #endregion

        #endregion

        #region DeleteExpense
        [HttpPost]
        //public IActionResult DeleteExpense(IFormCollection fc)
        public IActionResult DeleteExpense(int ExpenseID,string? GroupIDString, int UserID,int? GroupID)
        {
            try
            {
                if (GroupID.HasValue) {
                    CommonClasses.ExpenseRepository.Delete(ExpenseID, Convert.ToInt32(GroupID), UserID);
                }
                else
                {
                    int GroupID1 = _encryptionService.DecryptInteger(GroupIDString);
                    //CommonClasses.ExpenseRepository.Delete(Convert.ToInt32(fc["ExpenseID"]), Convert.ToInt32(fc["GroupID"]));
                    CommonClasses.ExpenseRepository.Delete(ExpenseID, GroupID1, UserID);
                }
                
                TempData["Message"] = "Delete successfully!";
            }
            catch (Exception ex)
            {
                 TempData["ErrorMSG"] = ex.Message;
            }
            return GroupID.HasValue ? RedirectToAction("Index")/*RedirectToAction("GroupExpense", new { GroupIDString = _encryptionService.EncryptInteger(Convert.ToInt32(GroupID))})*/  : RedirectToAction("GroupExpense", new { GroupIDString = GroupIDString! });

        }
        #endregion

        #region Group selection page
        public IActionResult SelectGroup()
        {
            #region Drop Down Group
            ViewBag.GroupList = DropdownClass.GetDropdownList<GroupDropDownModel>(CommonClass.SelectByPk("PR_Group_Selection", Convert.ToInt32(CV.UserID()), "UserID"));
            #endregion
            return View();
        }

        [HttpPost]
        public IActionResult SelectGroup(IFormCollection c)
        {
            return RedirectToAction("AddEditExpense", new { GroupIDString = _encryptionService.EncryptInteger(Convert.ToInt32(c["GroupID"])) });
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

        public decimal GetReceivableAmout(int GroupID)
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Contribution_Receivable_Group";
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();
                    sqlCommand.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                    object result = sqlCommand.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToDecimal(result);
                    }
                    else
                    {
                        count = 0; // Or whatever default you want when nothing comes back
                    }
                }
            }

            return count;

        }
        public decimal GetPayableAmout(int GroupID)
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Contribution_Payable_Group";
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();
                    sqlCommand.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                    object result = sqlCommand.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToDecimal(result);
                    }
                    else
                    {
                        count = 0; // Handle null scenario by setting count to 0 or some default value
                    }
                }
            }
            return count;
        }

        public decimal GetTotalExpense(int GroupID)
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Expense_TotalByUser_Group";
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();
                    sqlCommand.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                    object result = sqlCommand.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToDecimal(result);
                    }
                    else
                    {
                        count = 0; // Handle null scenario by setting count to 0 or some default value
                    }
                }
            }
            return count;
        }
        public decimal GetTotalExpenseOfGroup(int GroupID)
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Expense_TotalOfGroup";
                    sqlCommand.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                    object result = sqlCommand.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToDecimal(result);
                    }
                    else
                    {
                        count = 0; // Handle null scenario by setting count to 0 or some default value
                    }
                }
            }
            return count;
        }

        public decimal GetTotalMembers(int GroupID)
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Group_MembersCount";
                    sqlCommand.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                    object result = sqlCommand.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToDecimal(result);
                    }
                    else
                    {
                        count = 0; // Handle null scenario by setting count to 0 or some default value
                    }
                }
            }
            return count;
        }
        #endregion
    }
}
