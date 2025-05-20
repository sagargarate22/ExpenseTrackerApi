namespace ExpenseTrackerApi.Services.Email.Interface
{
    public interface ICache
    {
        Task<TItem?> GetObjectAsync<TItem>(string key) where TItem : class;
        Task<TItem> SetItemAsync<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
    }
}
