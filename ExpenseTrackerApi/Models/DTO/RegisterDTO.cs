using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Models.DTO
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } 

        public bool IsActive { get; set; }
    }
}
