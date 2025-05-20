namespace ExpenseTrackerApi.Services.Email.Interface
{
    public interface IEmailServiceProvider
    {
        public Task<Action> SendVerifyEmail(string email);

        public Task<bool> VerifyOTP(string email, string otp);

        public Task SendDailyReport();

        public Task SendMonthlyReport();

    }
}
