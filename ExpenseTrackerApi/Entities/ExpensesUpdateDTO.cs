using ExpenseTrackerApi.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Entities
{
    public class ExpensesUpdateDTO : ExpenseDTO
    {
        [Required]
        public int ExpensesId { get; set; }
    }
}
