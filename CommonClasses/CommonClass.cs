using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Data;
using System.Data.SqlClient;

namespace GroupExpenseManagement01.CommonClasses
{
    public class CommonClass
    {
        #region Connection String
        private static string connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("ConnectionString");
        #endregion

        #region ExportToExcel
        public static Stream ExportToExcel(string sp)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                DataTable dataTable = SelectData(sp);

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    // Load data into the worksheet
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true, TableStyles.Medium9);

                    // Generate the Excel file
                    Stream stream = new MemoryStream(package.GetAsByteArray());

                    // Return the file as a downloadable response
                    return stream;
                }
            }
        }

        public static Stream ExportToExcel(string sp, int id, string pk)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                DataTable dataTable = SelectByPk(sp, id, pk);

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    // Load data into the worksheet
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true, TableStyles.Medium9);

                    // Generate the Excel file
                    Stream stream = new MemoryStream(package.GetAsByteArray());

                    // Return the file as a downloadable response
                    return stream;
                }
            }
        }
        #endregion

        #region Select All
        public static DataTable SelectData(string sp)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = sp;
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return table;
        }
        #endregion

        #region Select By Pk
        public static DataTable SelectByPk(string sp, int id, string pk)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = sp;
                command.Parameters.Add("@" + pk, SqlDbType.Int).Value = id;
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

        #region Delete Row
        public static void DeleteRow(string sp, int id, string pk)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = sp;
            command.Parameters.Add("@" + pk, SqlDbType.Int).Value = id;
            command.ExecuteNonQuery();
            connection.Close();
        }
        #endregion

        #region SavePic
        public static string? GetSavePic(IFormFile file)
        {
            if (file != null)
            {
                string FilePath = "wwwroot\\Upload";
                string path = Path.Combine(Directory.GetCurrentDirectory(), FilePath);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filenameWithPath = Path.Combine(path, file.FileName);

                using (var stream = new FileStream(filenameWithPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return "~" + FilePath.Replace("wwwroot\\", "/") + "/" + file.FileName;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region dbo.PR_SEC_User_SelectByUserNamePassword(GetUserInfo)
        public static DataTable dbo_PR_SEC_User_SelectByUserNamePassword(string UserName, string Password)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_SEC_User_SelectByUserNamePassword";
            command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = UserName;
            command.Parameters.Add("@Password", SqlDbType.VarChar).Value = Password;
            SqlDataReader reader = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            connection.Close();
            return table;
        }
        #endregion
    }
}
