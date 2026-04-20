using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        public string sid { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string city { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
        [Required]
        public string role { get; set; }
    }
}
