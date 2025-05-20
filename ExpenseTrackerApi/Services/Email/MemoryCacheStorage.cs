using ExpenseTrackerApi.Services.Email.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace ExpenseTrackerApi.Services.Email
{
    public class MemoryCacheStorage : ICache
    {
        private readonly IMemoryCache _cache;
        public MemoryCacheStorage(IMemoryCache cache)
        {
            _cache = cache;
        }
        public Task<TItem?> GetObjectAsync<TItem>(string key) where TItem : class =>
            Task.FromResult(_cache.Get<TItem?>(key));

        public Task<TItem> SetItemAsync<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow)=>
        Task.FromResult(_cache.Set(key, value, absoluteExpirationRelativeToNow)); 
    }
}
