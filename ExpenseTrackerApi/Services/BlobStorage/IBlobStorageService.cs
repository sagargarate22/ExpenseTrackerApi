using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ExpenseTrackerApi.Models.DTO;

namespace ExpenseTrackerApi.Services.BlobStorage
{
    public interface IBlobStorageService
    {
        Task<BlobDTO> GetRecord(string blobUrl);

        Task DeleteRecord(string blobUrl);

        Task<BlobClient> UploadRecord(Stream fileStream, string fileName);

        BlobContainerClient GetBlobStorageContainer();

    }
}
