using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using ExpenseTrackerApi.Models.DTO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;

namespace ExpenseTrackerApi.Services.BlobStorage
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IOptions<BlobStorageOptions> _storageOptions;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _rootContainer;
        public BlobStorageService(IOptions<BlobStorageOptions> storageOptions, IConfiguration configuration)
        {
            this._storageOptions = storageOptions;
            this._configuration = configuration;
            this._connectionString = _configuration.GetConnectionString("AzureBlobStorage")!;
            this._rootContainer = _storageOptions.Value.RootContainer!;
        }

        public async Task DeleteRecord(string blobUrl)
        {
            BlobContainerClient blobContainer = GetBlobStorageContainer();

            BlobClient blobClient = blobContainer.GetBlobClient(blobUrl);

            await blobClient.DeleteIfExistsAsync();
        }

        public BlobContainerClient GetBlobStorageContainer()
        {
            var container = new BlobContainerClient(_connectionString, _rootContainer);
            return container;
        }

        public async Task<BlobDTO> GetRecord(string blobUrl)
        {
            BlobContainerClient blobContainer = GetBlobStorageContainer();

            BlobClient blobClient = blobContainer.GetBlobClient(blobUrl);

            Stream? stream = await blobClient.OpenReadAsync();

            //var result = await blobClient.DownloadAsync
            var result = await blobClient.DownloadContentAsync();
            return new BlobDTO { Content = stream, ContentType = result.Value.Details.ContentType, Name = blobUrl };
        }

        public async Task<BlobClient> UploadRecord(Stream fileStream, string fileName)
        {
            BlobContainerClient blobContainer = GetBlobStorageContainer();

            BlobClient blobClient = blobContainer.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream, true);

            return blobClient;
        }
    }
}
