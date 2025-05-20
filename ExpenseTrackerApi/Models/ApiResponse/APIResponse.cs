using System.Net;

namespace ExpenseTrackerApi.Models.ApiResponse
{
    public class APIResponse
    {
        public bool Status {  get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public dynamic? Data { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

    }
}
