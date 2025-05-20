using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Timeouts;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Models.DTO
{
    public class RoleDTO
    {
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
    }
}
