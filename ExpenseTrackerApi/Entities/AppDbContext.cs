using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace ExpenseTrackerApi.Entities

{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            
        }

        public DbSet<Users> Users { get; set; }

        public DbSet<Expenses> Expenses { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<UserRoleMapping> UserRoleMappings { get; set; }
        public DbSet<ExpensesCategory> ExpensesCategorys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expenses>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");



            modelBuilder.Entity<ExpensesCategory>().HasData(
                new ExpensesCategory()
                {
                    ExpenseCategoryId = 1,
                    CategoryName = "Groceries"
                },
                new ExpensesCategory()
                {
                    ExpenseCategoryId = 2,
                    CategoryName = "Leisure"
                },
                new ExpensesCategory()
                {
                    ExpenseCategoryId = 3,
                    CategoryName = "Electronics"
                },
                new ExpensesCategory()
                {
                    ExpenseCategoryId = 4,
                    CategoryName = "Clothing"
                },
                new ExpensesCategory()
                {
                    ExpenseCategoryId = 5,
                    CategoryName = "Health"
                },
                new ExpensesCategory()
                {
                    ExpenseCategoryId = 6,
                    CategoryName = "Others"
                }
                );
        }

    }
}
