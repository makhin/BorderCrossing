using System.Threading.Tasks;
using BorderCrossing.Components;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;

namespace BorderCrossing.Pages
{
    public class UploadFileBase: ComponentBase
    {
        [Inject]
        public IBorderCrossingService BorderCrossingService { get; set; }

        public string Status { get; set; }

        public DateRangePostRequest DateRangePostRequest { get; set; }
        
        public BorderCrossingResponse BorderCrossingResponse{ get; set; }

        protected async Task HandleValidSubmit()
        {
            BorderCrossingResponse = await BorderCrossingService.ParseLocationHistoryAsync(DateRangePostRequest);
        }

        protected async Task OnUploadComplete(FileUploaderBaseEventArgs e)
        {
            Status = "Preparing locations";
            DateRangePostRequest = await BorderCrossingService.PrepareLocationHistoryAsync(e.Stream);
        }
    }
}