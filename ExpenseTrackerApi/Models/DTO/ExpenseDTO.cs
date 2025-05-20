using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Models.DTO
{


    public class ExpenseDTO
    {
        [Required]
        public string ExpensesName { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime ExpenseDate { get; set; }
        [Required]
        public Decimal? Amount { get; set; }
        [Required]
        public int ExpenseType { get; set; }

    }
       
}
