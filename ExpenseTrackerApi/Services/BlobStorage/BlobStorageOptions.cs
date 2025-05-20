namespace ExpenseTrackerApi.Services.BlobStorage
{
    public class BlobStorageOptions
    {
        public string RootContainer { get; set; } = default!;

        public string BaseUrl { get; set; } = default!;
    }
}