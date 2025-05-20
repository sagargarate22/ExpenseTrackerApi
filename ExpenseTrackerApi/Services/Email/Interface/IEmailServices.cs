namespace ExpenseTrackerApi.Services.Email.Interface
{
    public interface IEmailServices
    {
        Task Send(string toEmail, string subject, EmailType emailType, object model);
    }
}
