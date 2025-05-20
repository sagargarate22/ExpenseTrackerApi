using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace ExpenseTrackerApi.Entities
{
    public class ExpensesCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpenseCategoryId { get; set; }

        [MaxLength(30)]
        public string CategoryName { get; set; } = string.Empty;

        public virtual ICollection<Expenses> Expenses { get; set; } = new List<Expenses>();
    }
}
