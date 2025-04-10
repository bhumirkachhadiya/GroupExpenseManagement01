using System.Globalization;

namespace GroupExpenseManagement01.CommonClasses
{
    public class HelperClass
    {
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd-MM-yyyy | HH:mm");
        }

        public static string FormatDateTime2(DateTime dateTime)
        {
            return dateTime.ToString("dd-MMM-yyyy h:mm tt");
        }

        public static string FormatToIndianCurrency(decimal amount)
        {
            var cultureInfo = new CultureInfo("en-IN");
            return string.Format(cultureInfo, "₹{0:N2}", amount); // Includes decimal points
        }

        public static string FormatToUSDCurrency(decimal amount)
        {
            var cultureInfo = new CultureInfo("en-US");
            return string.Format(cultureInfo, "${0:N2}", amount); // Includes decimal points
        }

        public static string FormatToEuroCurrency(decimal amount)
        {
            var cultureInfo = new CultureInfo("fr-FR"); // French culture for Euro formatting
            return string.Format(cultureInfo, "€{0:N2}", amount); // Includes two decimal places and the Euro symbol
        }
    }
}
