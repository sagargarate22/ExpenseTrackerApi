using ExpenseTrackerApi.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTrackerApi.Entities
{
    public class Expenses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpensesId { get; set; }

        [Required]
        public string ExpensesName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public DateTime ExpenseDate { get; set; } 

        public Decimal Amount { get; set; } = new Decimal(0);

        public DateTime CreatedAt { get; set; }

        public DateTime UpdateAt { get; set; }

        [ForeignKey("ExpensesCategory")]
        public int ExpenseCategoryId { get; set; }
        public virtual ExpensesCategory? ExpensesCategory { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }
        public virtual Users? Users { get; set; }

    }
}


//ExpenseId INT PRIMARY KEY IDENTITY(1,1),
//    UserId INT NOT NULL,
//    CategoryId INT NOT NULL,
//    Amount DECIMAL(18,2) NOT NULL,
//    Description NVARCHAR(255),
//    ExpenseDate DATE NOT NULL,
//    CreatedAt DATETIME DEFAULT GETDATE(),
//    UpdatedAt DATETIME,
//    FOREIGN KEY (UserId) REFERENCES Users(UserId),
//    FOREIGN KEY (CategoryId) REFERENCES ExpenseCategories(CategoryId)