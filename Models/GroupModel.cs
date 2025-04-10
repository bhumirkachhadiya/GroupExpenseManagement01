using System.ComponentModel.DataAnnotations;

namespace GroupExpenseManagement01.Models
{
    public class GroupModel
    {
        [Key]
        public int GroupID { get; set; }

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int CreatedBy { get; set; }
        public int[] SelectedMembers { get; set; } = Array.Empty<int>();
    }
    
    public class GroupDropDownModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
    }

    public class GroupLog
    {
        public string? PhotoPath { get; set; }
        public string UserName { get; set; }
        public string MSG { get; set; }
        public string OtherInfo { get; set; }
        public DateTime DateOfEvent { get; set; }


    }
}
