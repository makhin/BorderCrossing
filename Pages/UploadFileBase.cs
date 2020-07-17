using System.Linq;
using System.Threading.Tasks;
using BlazorInputFile;
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
        
        protected async Task HandleSelection(IFileListEntry[] files)
        {
            Status = "Wait";
            
            var file = files.FirstOrDefault();
            if (file != null)
            {
                DateRangePostRequest = await BorderCrossingService.PrepareLocationHistoryAsync(file);
                 if (DateRangePostRequest != null)
                 {
                     Status = $"Finished loading {file.Size} bytes from {file.Name}";
                 }
                 else
                 {
                     Status = $"wrong file";
                 }
            }
        }

        protected async Task HandleValidSubmit()
        {
            BorderCrossingResponse = await BorderCrossingService.ParseLocationHistoryAsync(DateRangePostRequest);
        }
    }
}