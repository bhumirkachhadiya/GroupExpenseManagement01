using GroupExpenseManagement01.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GroupExpenseManagement01.Services;

namespace GroupExpenseManagement01.BAL
{
    
        public class CheckAccess : ActionFilterAttribute, IAuthorizationFilter
        {

            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                // Access HttpContext via filterContext
                var request = filterContext.HttpContext.Request;
                var session = filterContext.HttpContext.Session;
                var response = filterContext.HttpContext.Response;


                // Retrieve the EncryptionService via the HttpContext
                var encryptionService = filterContext.HttpContext.RequestServices.GetService<IEncryptionService>();

                // Retrieve cookie data
                var cookieValue = request.Cookies["UserCookie"];

                if (!string.IsNullOrEmpty(cookieValue))
                {
                    
                    try
                    {
                        cookieValue = encryptionService?.DecryptString(cookieValue);
                        // Deserialize cookie data into the UserCookieData object
                        var userCookieData = JsonConvert.DeserializeObject<UserCookieData>(cookieValue);

                        if (userCookieData != null)
                        {
                            // Set session variables from cookie data
                            session.SetString("UserName", userCookieData.UserName);
                            session.SetString("UserID", userCookieData.UserID);
                            session.SetString("Email", userCookieData.Email);
                            session.SetString("MobileNo", userCookieData.MobileNo);
                            session.SetString("Password", userCookieData.Password);
                            session.SetString("PhotoPath", userCookieData.PhotoPath);
                            session.SetString("Address", userCookieData.Address);
                            session.SetString("CurrencyName", userCookieData.CurrencyName);
                            session.SetString("CurrencyID", userCookieData.CurrencyID);
                        }
                    }
                    catch (JsonException)
                    {
                        // Handle deserialization failure (malformed cookie)
                        session.Clear(); // Clear session in case of bad cookie data
                        filterContext.Result = new RedirectResult("~/Login");
                        return;
                    }
                }

                // Redirect to login page if no session exists for UserID
                if (session.GetString("UserID") == null)
                {
                    filterContext.Result = new RedirectResult("~/Login");
                }
            }

            public override void OnResultExecuting(ResultExecutingContext filterContext)
            {
                // Prevent caching in the browser
                var response = filterContext.HttpContext.Response;
                response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                response.Headers["Expires"] = "-1";
                response.Headers["Pragma"] = "no-cache";

                // Call the base method to allow any additional functionality
                base.OnResultExecuting(filterContext);
            }
        }
    
}
