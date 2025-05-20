namespace ExpenseTrackerApi.Models.DTO
{
    public class BlobDTO
    {
        public Stream Content { get; set; }

        public string ContentType { get; set; }

        public string Name { get; set; }

        public string Uri { get; set; }
    }
}
