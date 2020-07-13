using Microsoft.AspNetCore.Http;

namespace BorderCrossing.Models
{
    public class LocationHistoryFilePostRequest
    {
        public IFormFile LocationHistoryFile { set; get; }
    }
}