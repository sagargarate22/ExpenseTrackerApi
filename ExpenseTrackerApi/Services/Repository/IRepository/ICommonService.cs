using System.Linq.Expressions;

namespace ExpenseTrackerApi.Services.Repository.IRepository
{
    public interface ICommonService<T>
    {
        public Task<T> CreateAsync(T Record);
        public Task<T> DeleteAsync(T Record);
        public Task<T> UpdateAsync(T Record);

        public Task<T> GetRecordAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false);

        public Task<List<T>> GetRecordsAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false);
        public Task<List<T>> GetRecordsAsync();

    }
}
