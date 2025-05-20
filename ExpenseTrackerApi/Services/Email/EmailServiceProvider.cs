
using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.Email.DTO;
using ExpenseTrackerApi.Services.Email.Interface;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Hangfire;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ExpenseTrackerApi.Services.Email
{
    public class EmailServiceProvider : IEmailServiceProvider
    {
        private readonly IEmailServices _emailServices;
        private readonly ICache _cache;
        private readonly IUserServices _userService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IExpenseService _expenseService;
        private readonly ICommonService<ExpensesCategory> _cateory;
        public EmailServiceProvider(IEmailServices emailServices, IUserServices userServices,
            IBackgroundJobClient backgroundJobClient, ICache cache,
            IExpenseService expenseService, ICommonService<ExpensesCategory> cateory)
        {
            _emailServices = emailServices;
            _userService = userServices;
            _backgroundJobClient = backgroundJobClient;
            _cache = cache;
            _expenseService = expenseService;
            _cateory = cateory;
        }

        public async Task<Action> SendVerifyEmail(string email)
        {
            var user = await _userService.GetRecordAsync(u => u.Email == email);
            if(user == null)
            {
                throw new Exception("User not found");
            };
            string otp = await GenerateOTP(6,email);
            VerifyDTO verifyDTO = new VerifyDTO()
            {
                Username = user.Username,
                Otp = otp,
            };
            //await _emailServices.Send(email, "Please Verify Your Email", EmailType.Verify, verifyDTO);
            return () =>
            {
                _backgroundJobClient.Enqueue<IEmailServices>(x => x.Send(email, "Please Verify Your Email", EmailType.Verify, verifyDTO));
            };
        }

        public async Task<string> GenerateOTP(int length,string email)
        {
            if (await _cache.GetObjectAsync<SignupCode>(SignupCode.CacheKey(email)) is SignupCode state)
            {
                return state.Code;
            }
            const string validCharacters = "0123456789";
            Random random = new Random();
            char[] otp = new char[length];

            for (int i = 0; i < length; i++)
            {
                otp[i] = validCharacters[random.Next(validCharacters.Length)];
            }
            var signupCode = new SignupCode { Code = new string(otp), Email = email };

            await _cache.SetItemAsync(SignupCode.CacheKey(email), signupCode, TimeSpan.FromMinutes(10));
            return new string(otp);
        }

        public async Task<bool> VerifyOTP(string email, string code)
        {
            if (String.IsNullOrEmpty(code)) return false;

            var state = await _cache.GetObjectAsync<SignupCode>(SignupCode.CacheKey(email));

            return state != null && state.Code == code;
        }

        public async Task SendDailyReport()
        {
            var users = await _userService.GetRecordsAsync();
            foreach (var user in users)
            {
                DailyDTO dailyReportDTO;
                var expenses = await _expenseService.GetRecordsAsync(e => e.ExpenseDate.Date == DateTime.Now.Date && e.UserId == user.UserId);
                if(expenses.Any())
                {
                    var total = expenses.Sum(e => e.Amount);
                    dailyReportDTO = new DailyDTO()
                    {
                        Username = user.Username,
                        Email = user.Email,
                        IsExpensesAdded = true,
                        Amount = total,
                    };
                    
                }
                else
                {
                    dailyReportDTO = new DailyDTO()
                    {
                        Username = user.Username,
                        Email = user.Email,
                        IsExpensesAdded = false,
                        Amount = 0,
                    };
                }
                _backgroundJobClient.Enqueue<IEmailServices>(x => x.Send(user.Email, "Daily Expense Report", EmailType.Daily, dailyReportDTO));
            }
        }

        public async Task SendMonthlyReport()
        {
            var users = await _userService.GetRecordsAsync();
            var currDate = DateTime.Now.Date;
            var startDate = _expenseService.GetStartOfMonth(currDate);
            foreach (var user in users)
            {
                
                var monthlyReport = await _expenseService.GetRecordsAsync(e => (e.ExpenseDate.Date >= startDate && e.ExpenseDate.Date <= currDate) && e.UserId == user.UserId,true);
                MonthlyDTO monthlyDTO;
                if (monthlyReport.Any())
                {
                    var monthlyReportByCategory = new List<ExpenseCategoryDTO>();
                    var toalExpenses = monthlyReport.Sum(e => e.Amount);
                    foreach (var group in monthlyReport.GroupBy(e => e.ExpenseCategoryId))
                    {
                        var category = await _cateory.GetRecordAsync(c => c.ExpenseCategoryId == group.Key);
                        var categoryName = category?.CategoryName ?? "Unknown";

                        monthlyReportByCategory.Add(new ExpenseCategoryDTO
                        {
                            CategoryName = categoryName,
                            Amount = group.Sum(e => e.Amount)
                        });
                    }
                    monthlyDTO = new MonthlyDTO()
                    {
                        Username = user.Username,
                        Email = user.Email,
                        TotalExpense = toalExpenses,
                        Month = DateTimeFormatInfo.CurrentInfo.GetMonthName(currDate.Month),
                        ExpenseCategory = monthlyReportByCategory
                    };
                }
                else
                {
                    monthlyDTO = new MonthlyDTO()
                    {
                        Username = user.Username,
                        Email = user.Email,
                        TotalExpense = 0,
                        Month = DateTimeFormatInfo.CurrentInfo.GetMonthName(currDate.Month)
                    };
                }
                _backgroundJobClient.Enqueue<IEmailServices>(x => x.Send(user.Email, "Monthly Expense Report", EmailType.Monthly, monthlyDTO));
            }
        }

        sealed class SignupCode
        {
            public string Code { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public static string CacheKey(string email) => $"signup_code:{email}";
        }
    }
}
