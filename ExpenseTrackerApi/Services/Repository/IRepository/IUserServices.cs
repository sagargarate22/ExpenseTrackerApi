using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Models.DTO;
using System.Linq.Expressions;

namespace ExpenseTrackerApi.Services.Repository.IRepository
{
    public interface IUserServices : ICommonService<Users>
    {
        public int GetCurrentUserId();
        public List<Users> GetUsers();
    }
}
