namespace ExpenseTrackerApi.Services.Email.DTO
{
    public class MonthlyDTO
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;   

        public string Month { get; set; } = string.Empty;

        public Decimal TotalExpense { get; set; } = 0;

        public List<ExpenseCategoryDTO> ExpenseCategory { get; set; } = new List<ExpenseCategoryDTO>();
    }
}
