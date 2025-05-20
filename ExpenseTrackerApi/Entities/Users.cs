using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace ExpenseTrackerApi.Entities
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [MaxLength(15)]
        public string Username { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string passwordSalt { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Expenses> Expenses { get; set; } = new List<Expenses>();
    }
}
