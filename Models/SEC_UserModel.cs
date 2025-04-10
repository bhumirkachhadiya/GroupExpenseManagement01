using System.ComponentModel.DataAnnotations;

namespace GroupExpenseManagement01.Models
{
    public class SEC_UserModel
    {
        [Key]
        public int UserID { get; set; }
        public int MainCurrencyID { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Enter Name")]
        public string UserName { get; set; }



        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long, contain at least one letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        //public bool IsActive { get; set; }

        public bool RememberMe { get; set; }

        public string? Otp { get; set; }

        public IFormFile? file { get; set; }

        public string? PhotoPath { get; set; }
    }

    public class Profile
    {
        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Enter Name")]
        public string UserName { get; set; }

        [Display(Name = "Main Currency")]
        [Required(ErrorMessage = "Enter Main Currency")]
        public int MainCurrencyID { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long, contain at least one letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        [DataType(DataType.PhoneNumber)]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        public IFormFile? file { get; set; }

        public string? PhotoPath { get; set; }
    }
        public class UserDropDownModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
