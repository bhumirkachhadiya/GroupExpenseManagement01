using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GroupExpenseManagement01.Controllers
{
    [CheckAccess]
    public class HomeController : Controller
    {
        private IConfiguration configuration;
        public HomeController(IConfiguration? _configuration)
        {
            configuration = _configuration;
        }

        public IActionResult Index()
        {
            ViewData["GroupCount"] = GetGroupCount();
            ViewData["ReceivableAmout"] = GetReceivableAmout();
            ViewData["PayableAmout"] = GetPayableAmout();
            ViewData["TotalExpense"] = GetTotalExpense();
            ViewData["Balance"] = Math.Abs(GetReceivableAmout() - GetPayableAmout());
            ViewData["WhichBalance"] = GetReceivableAmout() >= GetPayableAmout() ? true : false;
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public int GetGroupCount()
        {
            int count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_Group_Count";
            sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();
            count = (int)sqlCommand.ExecuteScalar();
            sqlConnection.Close();
            return count;
        }

        public decimal GetReceivableAmout()
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Contribution_Receivable";
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();

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
        public decimal GetPayableAmout()
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Contribution_Payable";
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();

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

        public decimal GetTotalExpense()
        {
            decimal count = 0;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Expense_TotalByUser";
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = CV.UserID();

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


    }
}
