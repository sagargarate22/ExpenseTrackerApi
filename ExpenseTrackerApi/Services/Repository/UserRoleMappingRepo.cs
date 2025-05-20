using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.Repository.IRepository;

namespace ExpenseTrackerApi.Services.Repository
{
    public class UserRoleMappingRepo : CommonRepo<UserRoleMapping>, IUserRoleMappingServices
    {
        public UserRoleMappingRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
