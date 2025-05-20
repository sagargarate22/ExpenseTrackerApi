using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.Repository.IRepository;

namespace ExpenseTrackerApi.Services.Repository
{
    public class RoleRepo : CommonRepo<Role>, IRoleServices
    {
        public RoleRepo(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
