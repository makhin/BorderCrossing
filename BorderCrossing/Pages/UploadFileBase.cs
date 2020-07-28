using System;
using System.IO;
using System.Threading.Tasks;
using Blazorise;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace BorderCrossing.Pages
{
    public class UploadFileBase : ComponentBase
    {
        protected enum PageStatus
        {
            ReadyToUpload,
            Uploading,
            Deserializing,
        }

        [Inject] 
        public IBorderCrossingService BorderCrossingService { get; set; }

        [Inject]
        IHttpContextAccessor HttpContextAccessor { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected PageStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public int PercentageLoad { get; set; }

        public int PercentagePrep { get; set; }

        public ValidationStatus FileUploadStatus { get; set; }

        protected override void OnInitialized()
        {
            Status = PageStatus.ReadyToUpload;
        }

        public async Task OnChanged(FileChangedEventArgs e)
        {
            try
            {
                foreach (var file in e.Files)
                {
                    Status = PageStatus.Uploading;
                    FileUploadStatus = ValidationStatus.None;
                    StateHasChanged();

                    var ipAddress = HttpContextAccessor.HttpContext.Connection?.RemoteIpAddress.ToString();
                    var userAgent = HttpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

                    var request = await BorderCrossingService.AddNewRequestAsync(ipAddress, userAgent);

                    if (Path.GetExtension(file.Name) != ".zip")
                    {
                        throw new Exception("Файл должен быть .zip архивом");
                    }

                    await using (var memoryStream = new MemoryStream())
                    {
                        await file.WriteToStreamAsync(memoryStream);
                        PercentageLoad = 100;
                        StateHasChanged();
                        Status = PageStatus.Deserializing;
                        string requestId = await BorderCrossingService.PrepareLocationHistoryAsync(memoryStream, file.Name, request, (sender, progressChangedEventArgs) =>
                        {
                            PercentagePrep = progressChangedEventArgs.ProgressPercentage;
                            InvokeAsync(StateHasChanged);
                        });

                        NavigationManager.NavigateTo($"query/{requestId}");
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
                FileUploadStatus = ValidationStatus.Error;
                Status = PageStatus.ReadyToUpload;
                PercentageLoad = 0;
                PercentagePrep = 0;
            }
            finally
            {
                this.StateHasChanged();
            }
        }

        public void OnProgressed(FileProgressedEventArgs e)
        {
            PercentageLoad = (int)e.Percentage;
            this.StateHasChanged();
        }
    }
}