using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GroupExpenseManagement01.BAL
{
    public static class CV
    {
        private static IHttpContextAccessor _contextAccessor;

        static CV()
        {
            _contextAccessor = new HttpContextAccessor();
        }

        public static string? UserName()
        {
            string? userName = null;

            if (_contextAccessor.HttpContext.Session.GetString("UserName") != null)
            {
                userName = _contextAccessor.HttpContext.Session.GetString("UserName").ToString();
            }
            return userName;
        }
        public static string? CurrencyName()
        {
            string? CurrencyName = null;

            if (_contextAccessor.HttpContext.Session.GetString("CurrencyName") != null)
            {
                CurrencyName = _contextAccessor.HttpContext.Session.GetString("CurrencyName").ToString();
            }
            return CurrencyName;
        }
        public static string? Password()
        {
            string? userPassword = null;

            if (_contextAccessor.HttpContext.Session.GetString("Password") != null)
            {
                userPassword = _contextAccessor.HttpContext.Session.GetString("Password").ToString();
            }
            return userPassword;
        }

        public static int? UserID()
        {
            int? userID = null;

            if (_contextAccessor.HttpContext.Session.GetString("UserID") != null)
            {
                userID = Convert.ToInt32(_contextAccessor.HttpContext.Session.GetString("UserID").ToString());
            }
            return userID;
        }
        
        public static int? CurrencyID()
        {
            int? CurrencyID = null;

            if (_contextAccessor.HttpContext.Session.GetString("CurrencyID") != null)
            {
                CurrencyID = Convert.ToInt32(_contextAccessor.HttpContext.Session.GetString("CurrencyID").ToString());
            }
            return CurrencyID;
        }

        public static string? PhotoPath()
        {
            string? userPhoto = null;

            if (_contextAccessor.HttpContext.Session.GetString("PhotoPath") != null)
            {
                userPhoto = _contextAccessor.HttpContext.Session.GetString("PhotoPath").ToString();
            }

            return userPhoto;
        }

        public static string? Email()
        {
            string? Email = null;

            if (_contextAccessor.HttpContext.Session.GetString("Email") != null)
            {
                Email = _contextAccessor.HttpContext.Session.GetString("Email").ToString();
            }

            return Email;
        }

        public static string? MobileNo()
        {
            string? MobileNo = null;

            if (_contextAccessor.HttpContext.Session.GetString("MobileNo") != null)
            {
                MobileNo = _contextAccessor.HttpContext.Session.GetString("MobileNo").ToString();
            }

            return MobileNo;
        }
        
        public static string? Address()
        {
            string? Address = null;

            if (_contextAccessor.HttpContext.Session.GetString("Address") != null)
            {
                Address = _contextAccessor.HttpContext.Session.GetString("Address").ToString();
            }

            return Address;
        }


    }
}