using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTrackerApi.Services.Repository
{
    public class UserRepository : CommonRepo<Users>, IUserServices
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext appDbContext,  IHttpContextAccessor httpContextAccessor) : base(appDbContext)
        {
            _context = appDbContext;
            _contextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        { 
            var userId = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value!;
            return Convert.ToInt32(userId);
        }

        public List<Users> GetUsers()
        {
            var user = _context.Users.Include(u=>u.Expenses).ThenInclude(e=>e.ExpensesCategory).ToList();
            return user;
        }

        //public int GetCurrentUserId(this ClaimsPrincipal user) 
        //{

        //    var claimsIdentity = this.User.Identity as ClaimsIdentity;
        //    var userId = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
        //    var userId = user?.Claims?.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value;
        //    return Convert.ToInt32(userId);
        //}
    }
}
