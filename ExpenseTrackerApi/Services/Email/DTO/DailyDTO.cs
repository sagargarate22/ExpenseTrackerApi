namespace ExpenseTrackerApi.Services.Email.DTO
{
    public class DailyDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;  

        public bool IsExpensesAdded { get; set; } = false;

        public Decimal Amount { get; set; }
    }
}
