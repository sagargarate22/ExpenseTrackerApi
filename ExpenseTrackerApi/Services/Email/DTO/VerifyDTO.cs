namespace ExpenseTrackerApi.Services.Email.DTO
{
    public class VerifyDTO
    {
        public string Username { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;
    }
}
