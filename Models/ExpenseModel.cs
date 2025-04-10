using System.ComponentModel.DataAnnotations;

namespace GroupExpenseManagement01.Models
{
    public class ExpenseModel
    {
        [Key]
        public int ExpenseID { get; set; }
        public int GroupID { get; set; }
        public int UserID { get; set; }

        //[Range(0, 99999999.99, ErrorMessage = "The value must be between 0 and 99,999,999.99")]
        //public decimal Amount { get; set; }
        [Range(0.00000001, 999999999999.99999999, ErrorMessage = "The value must be greater than 0 and less than or equal to 999,999,999,999.99999999.")]
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public int CategoryID { get; set; }
        public int? CurrencyID { get; set; }

        public int[] SelectedMembers { get; set; } = Array.Empty<int>();

        //public int[] OldSelectedMembers { get; set; } = Array.Empty<int>();
    }

    public class ExpenseContributions
    {
        public int ContributionID { get; set; }
        public decimal ContributionAmount { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    
    public class CurrencyDropDownModel
    {
        public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
    }

    public class ExpenseDetails
    {
        public int ExpenseID { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string CategoryName { get; set; } // Matches 'CategoryName' from the procedure
        public string UserName { get; set; }     // Matches 'UserName' from the procedure
        public string CurrencyName { get; set; }     // Matches 'UserName' from the procedure
        public int UserID { get; set; }     // Matches 'UserName' from the procedure
    }

}