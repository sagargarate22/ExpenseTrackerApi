namespace ExpenseTrackerApi.Services.Email.DTO
{
    public class ExpenseCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;

        public Decimal Amount { get; set; } = 0;
    }
}
