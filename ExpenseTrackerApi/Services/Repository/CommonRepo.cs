using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExpenseTrackerApi.Services.Repository
{

    public class CommonRepo<T> : ICommonService<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        public CommonRepo(AppDbContext appDbContext)
        {
            _context = appDbContext;
            _dbSet = _context.Set<T>();
        }
        public async Task<T> CreateAsync(T Record)
        {
            await _dbSet.AddAsync(Record);
            await _context.SaveChangesAsync();
            return Record;

        }

        public async Task<T> DeleteAsync(T Record)
        {
            _dbSet.Remove(Record);
            await _context.SaveChangesAsync();
            return Record;
        }

        public async Task<T> GetRecordAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false)
        {
            if (useNoTracking)
            {
                return await _dbSet.AsNoTracking().Where(filter).FirstOrDefaultAsync();
            }
            else
            {
                return await _dbSet.Where(filter).FirstOrDefaultAsync();
            }
        }

        public async Task<List<T>> GetRecordsAsync(Expression<Func<T, bool>> filter, bool useNoTracking = false)
        {
            if (useNoTracking)
            {
                return await _dbSet.AsNoTracking().Where(filter).ToListAsync();
            }
            else
            {
                return await _dbSet.Where(filter).ToListAsync();
            }
        }

        public async Task<List<T>> GetRecordsAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> UpdateAsync(T Record)
        {
            _dbSet.Update(Record);
            await _context.SaveChangesAsync();
            return Record;
        }
    }
}
