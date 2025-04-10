using GroupExpenseManagement01.BAL;
using GroupExpenseManagement01.CommonClasses;
using GroupExpenseManagement01.Models;
using GroupExpenseManagement01.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace GroupExpenseManagement01.Controllers
{
    public class SEC_UserController : Controller
    {
        #region Configuration
        private IConfiguration configuration;
        private IEmailSender emailSender;
        private readonly IEncryptionService _encryptionService;
        private readonly IHttpContextAccessor httpContextAccessor;
        public SEC_UserController(IHttpContextAccessor httpContextAccessor,IConfiguration configuration, IEmailSender emailSender, IEncryptionService encryptionService)
        {
            this.configuration = configuration;
            this.emailSender = emailSender;
            this._encryptionService = encryptionService;
            this.httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region Login(View)
        [Route("~/Login")]
        public IActionResult Index()
        {
            SEC_UserModel model = new SEC_UserModel();
            try
            {
                if (TempData.ContainsKey("UserName"))
                {
                    model.UserName = TempData["UserName"].ToString();
                }

                if (TempData.ContainsKey("Email"))
                {
                    model.Email = TempData["Email"].ToString();
                }

                if (TempData.ContainsKey("MobileNo"))
                {
                    model.MobileNo = TempData["MobileNo"].ToString();
                }
            }
            catch (Exception ex) { }
            return View();
        }
        #endregion

        #region Login(HttpPost)
        [HttpPost]
        public IActionResult Login(SEC_UserModel modelUser)
        {
            string error = "";

            // Validate required fields
            if (string.IsNullOrEmpty(modelUser.UserName))
            {
                error += "UserName is required!\n";
            }
            if (string.IsNullOrEmpty(modelUser.Password))
            {
                error += "Password is required!\n";
            }

            // If there are validation errors, store them in TempData and redirect to the login page
            if (!string.IsNullOrEmpty(error))
            {
                TempData["Error"] = error.TrimEnd('\n');
                return RedirectToAction("Index");
            }
            else
            {
                // Retrieve user data from the database based on username and password
                //DataTable dt = CommonClasses.CommonClass.dbo_PR_SEC_User_SelectByUserNamePassword(modelUser.UserName, modelUser.Password);
                DataTable dt = CommonClasses.CommonClass.dbo_PR_SEC_User_SelectByUserNamePassword(modelUser.UserName, _encryptionService.EncryptString(modelUser.Password));

                if (dt.Rows.Count > 0)
                {
                    var userCookieData = new UserCookieData();
                    // Store user information in the session
                    DataRow dr = dt.Rows[0]; // We assume only one row is returned since UserName is unique

                    userCookieData.UserID = dr["UserID"].ToString();
                    userCookieData.UserName = dr["UserName"].ToString();
                    userCookieData.Password = dr["Password"].ToString();
                    userCookieData.Email = dr["Email"].ToString();
                    userCookieData.MobileNo = dr["MobileNo"].ToString();
                    userCookieData.PhotoPath = dr["PhotoPath"].ToString();
                    userCookieData.Address = dr["Address"].ToString();
                    userCookieData.CurrencyName = dr["CurrencyName"].ToString();
                    userCookieData.CurrencyID = dr["CurrencyID"].ToString();

                    HttpContext.Session.SetString("UserName", userCookieData.UserName);
                    HttpContext.Session.SetString("UserID", userCookieData.UserID);
                    HttpContext.Session.SetString("Email", userCookieData.Email);
                    HttpContext.Session.SetString("MobileNo", userCookieData.MobileNo);
                    HttpContext.Session.SetString("Password", userCookieData.Password);
                    HttpContext.Session.SetString("PhotoPath", userCookieData.PhotoPath);
                    HttpContext.Session.SetString("Address", userCookieData.Address);
                    HttpContext.Session.SetString("CurrencyName", userCookieData.CurrencyName);
                    HttpContext.Session.SetString("CurrencyID", userCookieData.CurrencyID);

                    if (modelUser.RememberMe)
                    {
                        string cookieValue = _encryptionService.EncryptString(JsonConvert.SerializeObject(userCookieData));
                        // Set cookie with serialized object
                        CookieOptions cookieOptions = new CookieOptions
                        {
                            Expires = DateTimeOffset.Now.AddDays(5), // Cookie expiration (5 days)
                            HttpOnly = true,  // Prevent access via client-side scripts
                            Secure = true,    // Only send cookie over HTTPS
                            IsEssential = true // Ensure the cookie is not deleted due to GDPR rules
                        };

                        Response.Cookies.Append("UserCookie", cookieValue, cookieOptions);
                    }

                    // Redirect to Home if login is successful
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // If credentials are incorrect, show error
                    TempData["Error"] = "User name or Password is invalid!";
                    return RedirectToAction("Index");
                }
            }
        }

        #endregion

        #region Logout(HttpPost)
        [HttpPost]
        public IActionResult Logout()
        {
            // Clear the session data
            HttpContext.Session.Clear();

            CookieOptions cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(-1)
            };

            // Update the cookie with expired options, which will remove it
            Response.Cookies.Append("UserCookie", "", cookieOptions);

            // Optionally, clear authentication cookies if used
            // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 
            // (use the above if you are using cookie-based authentication)

            // Redirect to the login page after logging out
            return RedirectToAction("Index", "SEC_User");
        }
        #endregion

        #region Profile(View)
        [Route("~/Profile")]
        [CheckAccess]
        public IActionResult Profile()
        {
            if (TempData.ContainsKey("UpdateMSG"))
            {
                TempData["UpdateMSG"] = TempData["UpdateMSG"];
            }
            if (TempData.ContainsKey("ErrorMSG"))
            {
                TempData["ErrorMSG"] = TempData["ErrorMSG"];
            }
            
            #region Drop Down Currency
            ViewBag.CurrencyList = DropdownClass.GetDropdownList<CurrencyDropDownModel>(CommonClass.SelectData("PR_Currency_DropDown"));
            #endregion

            Profile model = new Profile
            {
                UserName = CV.UserName()!,
                Email = CV.Email()!,
                Address = CV.Address()!,
                PhotoPath = CV.PhotoPath(),
                MainCurrencyID = (int)CV.CurrencyID()!,
                MobileNo = CV.MobileNo()!                
            };
            return View(model);
        }
        #endregion

        #region PhotoUpdate(HTTPPost)
        [HttpPost]
        public IActionResult PhotoUpdate([Bind("PhotoPath, file")]Profile modelUser)
        {
            
                string connectionString = this.configuration.GetConnectionString("ConnectionString");

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_User_Update_Photo";
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = CV.UserID();
                
                //command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = modelUser.UserName;
                //command.Parameters.Add("@Email", SqlDbType.VarChar).Value = modelUser.Email;
                //command.Parameters.Add("@Password", SqlDbType.VarChar).Value = modelUser.Password;
                //command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = modelUser.MobileNo;
                //command.Parameters.Add("@Address", SqlDbType.VarChar).Value = modelUser.Address;
                //command.Parameters.Add("@IsActive", SqlDbType.VarChar).Value = modelUser.IsActive;



                if (modelUser.file != null)
                {
                    modelUser.PhotoPath = CommonClass.GetSavePic(modelUser.file);
                    command.Parameters.Add("@PhotoPath", SqlDbType.VarChar).Value = modelUser.PhotoPath;
                }
                else if (modelUser.PhotoPath != null)
                {
                    modelUser.PhotoPath = modelUser.PhotoPath;
                    command.Parameters.Add("@PhotoPath", SqlDbType.VarChar).Value = modelUser.PhotoPath;
                }
                
                if (Convert.ToBoolean(command.ExecuteNonQuery()))
                {
                    TempData["UpdateMSG"] = "Profile Photo Updated successfully!";
                    connection.Close();
                    
                    if (modelUser.PhotoPath != null)
                    {
                       HttpContext.Session.SetString("PhotoPath", modelUser.PhotoPath.ToString() ?? "~/assets/img/profile-img.jpg");
                    }
                    else
                    {
                       HttpContext.Session.SetString("PhotoPath", "~/assets/img/profile-img.jpg");
                    }
                    if (Request.Cookies["UserCookie"] != null)
                    {
                        var userCookieData = new UserCookieData();
                        userCookieData.UserID = CV.UserID().ToString();
                        userCookieData.CurrencyID = CV.CurrencyID().ToString();
                        userCookieData.UserName = CV.UserName();
                        userCookieData.CurrencyName = CV.CurrencyName();
                        userCookieData.Password = CV.Password();
                        userCookieData.Email = CV.Email();
                        userCookieData.MobileNo = CV.MobileNo();
                        userCookieData.PhotoPath = modelUser.PhotoPath != null ? modelUser.PhotoPath.ToString() : "~/assets/img/profile-img.jpg";
                        userCookieData.Address = CV.Address();

                        string updatedCookieValue = _encryptionService.EncryptString(JsonConvert.SerializeObject(userCookieData));

                        var existingCookie = httpContextAccessor.HttpContext?.Request.Cookies["UserCookie"];
                        var parts = existingCookie.Split('|');
                        var existingValue = parts[0];
                        var expiration = parts.Length > 1 ? DateTimeOffset.Parse(parts[1]) : DateTimeOffset.UtcNow.AddDays(5);


                        CookieOptions cookieOptions = new CookieOptions
                        {
                            Expires = expiration, // Use existing expiration or fallback
                            HttpOnly = true,
                            Secure = true,
                            IsEssential = true
                        };

                        Response.Cookies.Append("UserCookie", updatedCookieValue, cookieOptions);
                    }
                }
                else
                {
                    TempData["ErrorMSG"] = "Can't Update Profile Photo";
                }
            
            return RedirectToAction("Profile");
            //return RedirectToAction("Profile");
        }
        #endregion

        #region ProfileUpdate(HTTPPost)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProfileUpdate([Bind("UserName,MobileNo,Address,MainCurrencyID")] Profile modelUser)
        {

            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_Update";
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = CV.UserID();

            command.Parameters.Add("@MainCurrencyID", SqlDbType.Int).Value = modelUser.MainCurrencyID;
            command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = modelUser.UserName;
            command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = modelUser.MobileNo;
            command.Parameters.Add("@Address", SqlDbType.VarChar).Value = modelUser.Address;


            if (Convert.ToBoolean(command.ExecuteNonQuery()))
            {
                TempData["UpdateMSG"] = "Profile Updated successfully!";
                connection.Close();

                string cName = GetCurrencyName(modelUser.MainCurrencyID);
                HttpContext.Session.SetString("UserName", modelUser.UserName.Trim());
                HttpContext.Session.SetString("MobileNo", modelUser.MobileNo.Trim());
                HttpContext.Session.SetString("Address", modelUser.Address.Trim());
                HttpContext.Session.SetString("CurrencyID", modelUser.MainCurrencyID.ToString());
                HttpContext.Session.SetString("CurrencyName", cName);
                
                if (Request.Cookies["UserCookie"] != null)
                {
                    var userCookieData = new UserCookieData();
                    userCookieData.UserID = CV.UserID().ToString();
                    userCookieData.CurrencyID = modelUser.MainCurrencyID.ToString();
                    userCookieData.UserName = modelUser.UserName.Trim();
                    userCookieData.CurrencyName = cName;
                    userCookieData.Password = CV.Password();
                    userCookieData.Email = CV.Email();
                    userCookieData.MobileNo = modelUser.MobileNo.Trim();
                    userCookieData.PhotoPath = CV.PhotoPath();
                    userCookieData.Address = modelUser.Address.Trim();

                    string updatedCookieValue = _encryptionService.EncryptString(JsonConvert.SerializeObject(userCookieData));

                    var existingCookie = httpContextAccessor.HttpContext?.Request.Cookies["UserCookie"];
                    var parts = existingCookie.Split('|');
                    var existingValue = parts[0];
                    var expiration = parts.Length > 1 ? DateTimeOffset.Parse(parts[1]) : DateTimeOffset.UtcNow.AddDays(5);


                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Expires = expiration, // Use existing expiration or fallback
                        HttpOnly = true,
                        Secure = true,
                        IsEssential = true
                    };

                    Response.Cookies.Append("UserCookie", updatedCookieValue, cookieOptions);
                }
            }
            else
            {
                TempData["ErrorMSG"] = "Can't Update Profile";
            }

            return RedirectToAction("Profile");
            //return RedirectToAction("Profile");

        }
        #endregion


        #region PasswordUpdate(HTTPPost)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PasswordUpdate([Bind("OldPassword,Password")] Profile modelUser)
        {
            modelUser.OldPassword = _encryptionService.EncryptString(modelUser.OldPassword);

            if(CV.Password() == modelUser.OldPassword)
            {
                modelUser.Password = _encryptionService.EncryptString(modelUser.Password);
                string connectionString = this.configuration.GetConnectionString("ConnectionString");

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_User_Update_Password";
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = CV.UserID();

                command.Parameters.Add("@Password", SqlDbType.VarChar).Value = modelUser.Password.Trim();
                

                if (Convert.ToBoolean(command.ExecuteNonQuery()))
                {
                    TempData["UpdateMSG"] = "Password Updated successfully!";
                    connection.Close();

                    HttpContext.Session.SetString("Password", modelUser.Password.Trim());

                    if (Request.Cookies["UserCookie"] != null)
                    {
                        var userCookieData = new UserCookieData();
                        userCookieData.UserID = CV.UserID().ToString();
                        userCookieData.CurrencyID = CV.CurrencyID().ToString();
                        userCookieData.UserName = CV.UserName();
                        userCookieData.CurrencyName = CV.CurrencyName();
                        userCookieData.Password = modelUser.Password.Trim();
                        userCookieData.Email = CV.Email();
                        userCookieData.MobileNo = CV.MobileNo();
                        userCookieData.PhotoPath = CV.PhotoPath();
                        userCookieData.Address = CV.Address();

                        string updatedCookieValue = _encryptionService.EncryptString(JsonConvert.SerializeObject(userCookieData));

                        var existingCookie = httpContextAccessor.HttpContext?.Request.Cookies["UserCookie"];
                        var parts = existingCookie.Split('|');
                        var existingValue = parts[0];
                        var expiration = parts.Length > 1 ? DateTimeOffset.Parse(parts[1]) : DateTimeOffset.UtcNow.AddDays(5);


                        CookieOptions cookieOptions = new CookieOptions
                        {
                            Expires = expiration, // Use existing expiration or fallback
                            HttpOnly = true,
                            Secure = true,
                            IsEssential = true
                        };

                        Response.Cookies.Append("UserCookie", updatedCookieValue, cookieOptions);
                    }
                }
                else
                {
                    TempData["ErrorMSG"] = "Can't Update Password.";
                }
            }
            else {
                TempData["ErrorMSG"] = "Your old password was entered incorrectly. Please enter it again.";
            }
            return RedirectToAction("Profile");
        }
        #endregion

        #region Register
        public IActionResult Register()
        {
            #region Drop Down Currency
            ViewBag.CurrencyList = DropdownClass.GetDropdownList<CurrencyDropDownModel>(CommonClass.SelectData("PR_Currency_DropDown"));
            #endregion
            SEC_UserModel model = new SEC_UserModel();
            model.MainCurrencyID = 2;
            try
            {
                if (TempData.ContainsKey("UserName"))
                {
                    model.UserName = TempData["UserName"].ToString();
                }

                if (TempData.ContainsKey("Email"))
                {
                    model.Email = TempData["Email"].ToString();
                }

                if (TempData.ContainsKey("MobileNo"))
                {
                    model.MobileNo = TempData["MobileNo"].ToString();
                }
            }
            catch (Exception ex) { }
            return View(model);
        }

        #endregion

        #region Register(HttpPost)
        [HttpPost]
        public async Task<IActionResult> Register(SEC_UserModel modelUser)
        {
            // OTP validation
            if (modelUser.Otp != HttpContext.Session.GetString("otp"))
            {
                TempData["OTP"] = "OTP is invalid!!";
                ModelState.AddModelError("Otp", "OTP is invalid!!");
                return View(modelUser); // Early return if OTP is invalid
            }

            // Proceed if the model state is valid
            else if (ModelState.IsValid)
            {
                try
                {
                    // Using a 'using' block for the database connection to ensure proper resource disposal
                    string connectionString = configuration.GetConnectionString("ConnectionString");

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "PR_User_Insert";

                            // Add parameters
                            command.Parameters.AddWithValue("@UserName", modelUser.UserName.Trim());
                            command.Parameters.AddWithValue("@Email", modelUser.Email.Trim());
                            //command.Parameters.AddWithValue("@Password", modelUser.Password);
                            command.Parameters.AddWithValue("@Password", _encryptionService.EncryptString(modelUser.Password.Trim()));
                            command.Parameters.AddWithValue("@MobileNo", modelUser.MobileNo.Trim());
                            command.Parameters.AddWithValue("@Address", modelUser.Address.Trim());
                            command.Parameters.AddWithValue("@MainCurrencyID", modelUser.MainCurrencyID);

                            // Handle file upload if available
                            if (modelUser.file != null)
                            {
                                modelUser.PhotoPath = CommonClass.GetSavePic(modelUser.file);
                            }
                            command.Parameters.AddWithValue("@PhotoPath", modelUser.PhotoPath ?? string.Empty);

                            // Execute and check the result
                            var result = await command.ExecuteNonQueryAsync();

                            if (result > 0) // Registration successful
                            {
                                // Send email confirmation
                                try
                                {
                                    await emailSender.SendEmailAsync3(modelUser.Email, "Registration Successful", HtmlTemplate.RegistrationConfirmation(modelUser.UserName));
                                }
                                catch (Exception emailEx)
                                {
                                    // Log email error
                                    TempData["EmailError"] = "Failed to send confirmation email.";
                                }

                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMSG"] = "Registration failed.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMSG"] = "An error occurred: " + ex.Message;
                }
            }

            // If we got this far, something failed
            #region Drop Down Currency
            ViewBag.CurrencyList = DropdownClass.GetDropdownList<CurrencyDropDownModel>(CommonClass.SelectData("PR_Currency_DropDown"));
            #endregion
            return View(modelUser);
        }
        #endregion

        #region SendOtpEmail(ResetPassOtp)
        // The non-action method
        [NonAction]
        [ValidateAntiForgeryToken]
        public async Task SendOtpEmail(string UserName,string Email)
        {
            // Generate a random OTP (6 digits for example)
            Random random = new Random();
            int otp = random.Next(100000, 999999);

            // Store OTP in session for verification later
            HttpContext.Session.SetString("otp", otp.ToString());

            try
            {
                // Send the email
                await emailSender.SendEmailAsync3(Email, "Your Verification OTP Code", HtmlTemplate.ResetPassOtp(UserName, otp));
            }
            catch (Exception ex)
            {
                // Handle exception
                return;
            }
        }

        // Public action method that calls the non-action method
        [HttpPost]
        public async Task<IActionResult> SendOtpEmailAction()
        {
            await SendOtpEmail(CV.UserName()!, CV.Email()!); // Call the non-action method
            return Json(new { success = true }); // Respond with a success message
        }
        #endregion

        #region NewPass

        #region FromProf.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPasswordMail(IFormCollection fc)
        {
            if (string.IsNullOrEmpty(fc["otp"])) {
                TempData["ErrorMSG"] = "Enter the OTP. If it's incorrect, please try again with the new OTP.";
                return RedirectToAction("Profile"); 
            }
            if (fc["otp"] == HttpContext.Session.GetString("otp"))
            {

                // Generate a random OTP (6 digits for example)
                Random random = new Random();
                int newPassword = random.Next(100000, 999999);

                //// Store OTP in session for verification later
                //HttpContext.Session.SetString("otp", otp.ToString());

                try
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");

                    SqlConnection connection = new SqlConnection(connectionString);
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_User_Update_Password";
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = CV.UserID();
                    string pass = _encryptionService.EncryptString(newPassword.ToString());
                    command.Parameters.Add("@Password", SqlDbType.VarChar).Value = pass;


                    if (Convert.ToBoolean(command.ExecuteNonQuery()))
                    {

                        connection.Close();

                        HttpContext.Session.SetString("Password", pass);

                        if (Request.Cookies["UserCookie"] != null)
                        {
                            var userCookieData = new UserCookieData();
                            userCookieData.UserID = CV.UserID().ToString();
                            userCookieData.CurrencyID = CV.CurrencyID().ToString();
                            userCookieData.UserName = CV.UserName();
                            userCookieData.CurrencyName = CV.CurrencyName();
                            userCookieData.Password = pass;
                            userCookieData.Email = CV.Email();
                            userCookieData.MobileNo = CV.MobileNo();
                            userCookieData.PhotoPath = CV.PhotoPath();
                            userCookieData.Address = CV.Address();

                            string updatedCookieValue = _encryptionService.EncryptString(JsonConvert.SerializeObject(userCookieData));

                            var existingCookie = httpContextAccessor.HttpContext?.Request.Cookies["UserCookie"];
                            var parts = existingCookie.Split('|');
                            var existingValue = parts[0];
                            var expiration = parts.Length > 1 ? DateTimeOffset.Parse(parts[1]) : DateTimeOffset.UtcNow.AddDays(5);


                            CookieOptions cookieOptions = new CookieOptions
                            {
                                Expires = expiration, // Use existing expiration or fallback
                                HttpOnly = true,
                                Secure = true,
                                IsEssential = true
                            };

                            Response.Cookies.Append("UserCookie", updatedCookieValue, cookieOptions);
                        }

                        await emailSender.SendEmailAsync3(CV.Email()!, "Your New Password", HtmlTemplate.NewPassword(CV.UserName()!, newPassword));
                        TempData["UpdateMSG"] = "Password updated successfully! Please check your email for the new password.";
                    }
                    else
                    {
                        TempData["ErrorMSG"] = "Unable to update the password. Please try again.";
                    }
                }
                catch (Exception ex) { }
            }
            else
            {
                TempData["ErrorMSG"] = "The OTP is incorrect, please try again with the new OTP.";
            }
            return RedirectToAction("Profile");

        }
        #endregion

        #region Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPasswordMail2(IFormCollection fc)
        {
            if (string.IsNullOrEmpty(fc["otp"]))
            {
                TempData["ErrorMSG"] = "Enter the OTP. If it's incorrect, please try again with the new OTP.";
                TempData["UserID"] = fc["UserID"];
                TempData["UName"] = fc["UName"];
                return RedirectToAction("CheckVerification");
            }
            if (fc["otp"] == HttpContext.Session.GetString("otp"))
            {

                // Generate a random OTP (6 digits for example)
                Random random = new Random();
                int newPassword = random.Next(100000, 999999);

                //// Store OTP in session for verification later
                //HttpContext.Session.SetString("otp", otp.ToString());

                try
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");

                    SqlConnection connection = new SqlConnection(connectionString);
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_User_Update_Password";
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = Convert.ToInt32(fc["UserID"]);
                    string pass = _encryptionService.EncryptString(newPassword.ToString());
                    command.Parameters.Add("@Password", SqlDbType.VarChar).Value = pass;


                    if (Convert.ToBoolean(command.ExecuteNonQuery()))
                    {

                        connection.Close();

                        await emailSender.SendEmailAsync3(fc["Email"].ToString(), "Your New Password", HtmlTemplate.NewPassword(fc["UName"].ToString(), newPassword));
                        TempData["UpdateMSG"] = "Password updated successfully! Please check your email for the new password.";
                    }
                    else
                    {
                        TempData["ErrorMSG"] = "Unable to update the password. Please try again.";
                        return RedirectToAction("CheckVerification");
                    }
                }
                catch (Exception ex) { }
            }
            else
            {
                TempData["UserID"] = Convert.ToInt32(fc["UserID"]);
                TempData["UName"] = fc["UName"].ToString();
                TempData["Email"] = fc["Email"].ToString();
                TempData["ErrorMSG"] = "The OTP is incorrect, please try again with the new OTP.";
                return RedirectToAction("CheckVerification");
            }
            return View("Index");

        }
        #endregion
        #endregion

        #region OtpMail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OtpMail([Bind("UserName,Email")] SEC_UserModel UserDetails)
        {
            if (string.IsNullOrEmpty(UserDetails.UserName) || string.IsNullOrEmpty(UserDetails.Email)) {
                TempData["ErrorMSG"] = "Enter Mail Id And User Name!";
                return RedirectToAction("Register"); 
            }
            // Generate a random OTP (6 digits for example)
            Random random = new Random();
            int otp = random.Next(100000, 999999);

            // Store OTP in session for verification later
            HttpContext.Session.SetString("otp", otp.ToString());

            try
            {

                await emailSender.SendEmailAsync3(UserDetails.Email, "Your Registration OTP Code", HtmlTemplate.Otp(UserDetails.UserName, otp));

            }
            catch (Exception ex) { }

            TempData["Email"] = UserDetails.Email;
            TempData["UserName"] = UserDetails.UserName;
            return RedirectToAction("Register");

        }
        #endregion


        #region CheckEmailExists
        [Route("~/Exists")]
        public IActionResult CheckEmailExists()
        {
            return View();
        }
        #endregion

        #region Comment
        //public async Task<IActionResult> CheckEmailExistsPostAsync(IFormCollection fc)
        //{
        //    try
        //    {
        //        string userName = null;
        //        int result;
        //        string connectionString = this.configuration.GetConnectionString("ConnectionString");
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            using (SqlCommand command = new SqlCommand("PR_CheckEmailExists", connection))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;

        //                // Input parameter - explicitly convert StringValues to string
        //                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = fc["Email"].ToString() });

        //                // Output parameter
        //                SqlParameter userNameParam = new SqlParameter("@UserName", SqlDbType.NVarChar, 100)
        //                {
        //                    Direction = ParameterDirection.Output
        //                };
        //                command.Parameters.Add(userNameParam);

        //                // Return value
        //                SqlParameter returnValue = new SqlParameter
        //                {
        //                    Direction = ParameterDirection.ReturnValue
        //                };
        //                command.Parameters.Add(returnValue);

        //                connection.Open();
        //                command.ExecuteNonQuery();

        //                // Retrieve output values
        //                result = (int)returnValue.Value;
        //                if (result == 1)
        //                {
        //                    userName = userNameParam.Value as string;
        //                    TempData["UName"] = userName;
        //                    await SendOtpEmail(userName, fc["Email"].ToString());
        //                    return RedirectToAction("CheckVerification");
        //                }
        //                else
        //                {
        //                    TempData["ErrorMSG"] = "Sorry, we couldn't find an account associated with this email. Please check the email address and try again.";
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMSG"] = ex.Message;
        //    }

        //    return View("CheckEmailExists");
        //}
        #endregion

        #region CheckEmailExistsPost
        [HttpPost]
        public async Task<IActionResult> CheckEmailExistsPost(IFormCollection fc)
        {
            try
            {
                string userName = null;
                int userId = 0;
                int result;
                string connectionString = this.configuration.GetConnectionString("ConnectionString");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand("PR_CheckEmailExists", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Input parameter - explicitly convert StringValues to string
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = fc["Email"].ToString() });

                        // Output parameters
                        SqlParameter userNameParam = new SqlParameter("@UserName", SqlDbType.NVarChar, 100)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(userNameParam);

                        SqlParameter userIdParam = new SqlParameter("@UserID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(userIdParam);

                        // Return value
                        SqlParameter returnValue = new SqlParameter
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnValue);

                        connection.Open();
                        command.ExecuteNonQuery();

                        // Retrieve output values
                        result = (int)returnValue.Value;
                        if (result == 1)
                        {
                            userName = userNameParam.Value as string;
                            userId = (int)userIdParam.Value;

                            // Store user details
                            TempData["UName"] = userName;
                            TempData["UserID"] = userId;
                            TempData["Email"] = fc["Email"].ToString();
                            await SendOtpEmail(userName, fc["Email"].ToString());
                            return RedirectToAction("CheckVerification");
                        }
                        else
                        {
                            TempData["ErrorMSG"] = "Sorry, we couldn't find an account associated with this email. Please check the email address and try again.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMSG"] = ex.Message;
            }

            return View("CheckEmailExists");
        }
        #endregion

        #region CheckVerification
        public IActionResult CheckVerification() {
            if (TempData.ContainsKey("UName"))
            {
                TempData["UName"] = TempData["UName"];
            }
            if (TempData.ContainsKey("UserID"))
            {
                TempData["UserID"] = TempData["UserID"];
            }
            if (TempData.ContainsKey("Email"))
            {
                TempData["Email"] = TempData["Email"];
            }
            return View();
        }
        #endregion


        public string GetCurrencyName(int currencyID)
        {
            string currencyName = string.Empty;
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Currency_Name";

                    // Add the GroupID parameter
                    sqlCommand.Parameters.AddWithValue("@CurrencyID", currencyID);

                    // Execute the query and retrieve the group name
                    object result = sqlCommand.ExecuteScalar();
                    if (result != null)
                    {
                        currencyName = result.ToString();
                    }
                }
            }

            return currencyName;
        }

    }
}
