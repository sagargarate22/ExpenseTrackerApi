using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerApi.Models.DTO
{
    public class ExpenseFilterDTO
    {
        [Required]
        [SwaggerSchema(Format = "date-time", Description = "The date and time of the expense.")]
        public DateTime StartDate { get; set; }  // Swagger will recognize this as a date-time picker

        [Required]
        [SwaggerSchema(Format = "date-time", Description = "The date and time of the expense.")]
        public DateTime EndDate { get; set; }  // Swagger will recognize this as a date-time picker
    }
}
