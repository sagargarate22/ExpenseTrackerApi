using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.Repository.IRepository;

namespace ExpenseTrackerApi.Services.Repository
{
    public class ExpensesRepo : CommonRepo<Expenses>, IExpenseService
    {
        public ExpensesRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public DateTime GetStartOfWeek(DateTime currentDate)
        {
            int daysToSubtract = (int)currentDate.DayOfWeek - 1;
            if (daysToSubtract < 0)
            {
                daysToSubtract = 6; // If it's Sunday, subtract 6 days to get the previous Monday
            }
            DateTime startOfWeek = currentDate.AddDays(-daysToSubtract);
            return startOfWeek.Date;
        }

        public DateTime GetStartOfMonth(DateTime currentDate)
        {
            DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return startOfMonth;
        }

    }
}
