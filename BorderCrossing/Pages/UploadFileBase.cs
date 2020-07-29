using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Blazorise;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using BorderCrossing.Models.Core;

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
        public NavigationManager NavigationManager { get; set; }

        protected PageStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public int PercentageLoad { get; set; }

        public int PercentagePrep { get; set; }

        public ValidationStatus FileUploadStatus { get; set; }

        [CascadingParameter]
        private ConnectionInfo ConnectionInfo { get; set; }

        protected override void OnInitialized()
        {
            Status = PageStatus.ReadyToUpload;
        }

        public async Task OnChanged(FileChangedEventArgs e)
        {
            try
            {
                ErrorMessage = string.Empty;
                Status = PageStatus.Uploading;
                FileUploadStatus = ValidationStatus.None;
                StateHasChanged();

                foreach (var file in e.Files)
                {
                    var request = await BorderCrossingService.AddNewRequestAsync(ConnectionInfo?.RemoteIpAddress, ConnectionInfo?.UserAgent);

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