using System.Data.SqlClient;
using System.Data;
using GroupExpenseManagement01.BAL;

namespace GroupExpenseManagement01.CommonClasses
{
    public class ExpenseRepository
    {
        #region Connection String
        private static string connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("ConnectionString");
        #endregion

        public static void Delete(int id, int groupID, int UserID)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Expense_Delete";
            command.Parameters.Add("@ExpenseID", SqlDbType.Int).Value = id;
            command.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupID;
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
            command.ExecuteNonQuery();
            connection.Close();
        }

        //PR_ExpenseContributions_SelectByGroup
        #region Select By Pk
        public static DataTable SelectByPkWithCurrencyName(string sp, int id, string pk)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = sp;
                command.Parameters.Add("@" + pk, SqlDbType.Int).Value = id;
                command.Parameters.Add("@CurrencyID", SqlDbType.VarChar).Value = CV.CurrencyID();
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);
                connection.Close();
                return table;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        #endregion

        //PR_Settlements_SelectByGroupPK
    }
}
