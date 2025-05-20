using ExpenseTrackerApi.Entities;

namespace ExpenseTrackerApi.Services.Repository.IRepository
{
    public interface IExpenseService : ICommonService<Expenses>
    {
        public DateTime GetStartOfWeek(DateTime date);
        public DateTime GetStartOfMonth(DateTime currentDate);
    }
}
