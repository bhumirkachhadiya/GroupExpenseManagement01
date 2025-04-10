using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.CommonClasses;
using GroupExpenseManagement01.Models;
using GroupExpenseManagement01.Services;

namespace GroupExpenseManagement01.Controllers
{
    [CheckAccess]
    public class GroupController : Controller
    {
        #region Configuration
        private IConfiguration configuration;
        private IEmailSender emailSender;
        private readonly IEncryptionService _encryptionService;

        public GroupController(IConfiguration configuration, IEmailSender emailSender, IEncryptionService encryptionService)
        {
            this.configuration = configuration;
            this.emailSender = emailSender;
            _encryptionService = encryptionService;
        }
        #endregion

        #region List of All Groups
        public IActionResult Index()
        {
            return View(CommonClass.SelectByPk("PR_Groups_SelectByUser", Convert.ToInt32(CV.UserID()), "UserID"));
        }
        #endregion

        #region Add Update Group(AddUpdateGroup)
        public IActionResult AddUpdateGroup(String? GroupIDString)
        {
            int? GroupID = null;
            var selectedMembers = new List<int>();
            if (GroupIDString != null)
            {
                GroupID = _encryptionService.DecryptInteger(GroupIDString);
            }


            GroupModel modelGroup = new GroupModel();
            TempData["PageTitle"] = "Add Group";
            #region Drop Down User
            ViewBag.UserList = DropdownClass.GetDropdownList<UserDropDownModel>(CommonClass.SelectData("PR_User_Selection"));
            #endregion

            #region Fetch Data

            if (GroupID != null)
            {string connectionString = this.configuration.GetConnectionString("ConnectionString");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("PR_Group_SelectByPK", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                        // First result set for group details
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                modelGroup.GroupID = Convert.ToInt32(reader["GroupID"]);
                                modelGroup.GroupName = reader["GroupName"].ToString();
                                modelGroup.Description = reader["Description"].ToString();
                                modelGroup.CreatedBy = Convert.ToInt32(reader["CreatedBy"]);
                                modelGroup.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);
                            }

                            // Move to the second result set for selected members
                            if (reader.NextResult())
                            {
                                //selectedMembers.Clear();
                                //var selectedMembers = new List<int>();
                                while (reader.Read())
                                {
                                    selectedMembers.Add(Convert.ToInt32(reader["UserID"]));
                                }

                                modelGroup.SelectedMembers = selectedMembers.ToArray();
                            }
                            TempData["PageTitle"] = "Update Group";
                        }
                    }
                }
            }
            else
            {

            }
            #endregion
            return View(modelGroup);

        }
        #endregion

        [HttpPost]
        public IActionResult DeleteGroup(int GroupID)
        {
            try
            {
                CommonClasses.CommonClass.DeleteRow("PR_Group_Delete", GroupID, "GroupID");
                TempData["Message"] = "Delete successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMSG"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        #region AddUpdateGroup(HttpPost)
        [HttpPost]
        public IActionResult AddUpdateGroup(GroupModel modelGroup)
        {
            if (ModelState.IsValid)
            {
                int newGroupId = 0;
                string connectionString = this.configuration.GetConnectionString("ConnectionString");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Determine if it's an insert or update
                        if (modelGroup.GroupID == 0)
                        {
                            command.CommandText = "PR_Group_Insert"; // Insert group
                            modelGroup.CreatedBy = Convert.ToInt32(CV.UserID());
                        }
                        else
                        {
                            command.CommandText = "PR_Group_Update"; // Update group
                            command.Parameters.Add("@GroupID", SqlDbType.Int).Value = modelGroup.GroupID;
                        }

                        // Assign CreatorID (ensure it's a valid integer)


                        // Add common parameters
                        command.Parameters.Add("@GroupName", SqlDbType.NVarChar).Value = modelGroup.GroupName;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = modelGroup.Description;
                        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = modelGroup.CreatedBy;

                        // If it's an insert, execute scalar to get the new GroupID
                        if (modelGroup.GroupID == 0)
                        {
                            newGroupId = Convert.ToInt32(command.ExecuteScalar());
                            if (newGroupId > 0)
                            {
                                TempData["InsertUpdateMSG"] = "Inserted successfully!";
                                AddOrUpdateGroupMembers(connection, newGroupId, modelGroup.CreatedBy, modelGroup.SelectedMembers);
                            }
                        }
                        else
                        {
                            // For update, execute non-query and check if rows were updated
                            if (command.ExecuteNonQuery() > 0)
                            {
                                TempData["InsertUpdateMSG"] = "Updated successfully!";
                                AddOrUpdateGroupMembers(connection, modelGroup.GroupID, modelGroup.CreatedBy, modelGroup.SelectedMembers);
                            }
                        }

                        return RedirectToAction("Index");
                    }
                }
            }

            return View(modelGroup);
        }
        #endregion

        #region Manipulation of Members
        private void AddOrUpdateGroupMembers(SqlConnection connection, int groupId, int creatorId, int[] selectedMembers)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;

                // Choose the appropriate stored procedure based on the groupId
                command.CommandText = "PR_GroupMember_BulkUpdate";

                // Add common parameters
                command.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupId;
                command.Parameters.Add("@CreatorID", SqlDbType.Int).Value = creatorId;

                // Create a table-valued parameter for the members
                DataTable membersTable = new DataTable();
                membersTable.Columns.Add("UserID", typeof(int));
                foreach (var member in selectedMembers)
                {
                    membersTable.Rows.Add(member);
                }

                SqlParameter tvpParam = command.Parameters.AddWithValue("@Members", membersTable);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = "dbo.UserIDTableType";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle or log the exception
                    // Example: Log the exception, rethrow it, or set an error message in TempData
                    TempData["ErrorMessage"] = "An error occurred while updating group members: " + ex.Message;
                    throw;
                }
            }
        }
        #endregion
    }
}
