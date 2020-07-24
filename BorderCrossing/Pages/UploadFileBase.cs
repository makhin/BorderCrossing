using System;
using System.IO;
using System.Threading.Tasks;
using Blazorise;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;

namespace BorderCrossing.Pages
{
    public enum Status{
        ReadyToUpload,
        Uploading,
        Deserializing,
        AskParameter,
        Processing,
        Results
    }

    public class UploadFileBase : ComponentBase
    {
        [Inject] 
        public IBorderCrossingService BorderCrossingService { get; set; }

        public Status Status { get; set; }

        public string ErrorMessage { get; set; }

        public ElementReference InputTypeFileElement { get; set; }

        public int PercentageLoad { get; set; }

        public int PercentagePrep { get; set; }

        public int PercentageProc { get; set; }

        public DateRangePostRequest DateRangePostRequest { get; set; }

        public BorderCrossingResponse BorderCrossingResponse { get; set; }

        public ValidationStatus FileUploadStatus { get; set; }

        protected override void OnInitialized()
        {
            Status = Status.ReadyToUpload;
        }

        public async Task Start()
        {
            Status = Status.Processing;
            BorderCrossingResponse = await BorderCrossingService.ParseLocationHistoryAsync(DateRangePostRequest, (sender, e) =>
            {
                PercentageProc = e.ProgressPercentage;
                InvokeAsync(StateHasChanged);
            });
            Status = Status.Results;
        }

        public async Task OnChanged(FileChangedEventArgs e)
        {
            try
            {
                foreach (var file in e.Files)
                {
                    Status = Status.Uploading;
                    FileUploadStatus = ValidationStatus.None;
                    StateHasChanged();

                    await using (var memoryStream = new MemoryStream())
                    {
                        await file.WriteToStreamAsync(memoryStream);
                        PercentageLoad = 100;
                        StateHasChanged();
                        Status = Status.Deserializing;
                        DateRangePostRequest = await BorderCrossingService.PrepareLocationHistoryAsync(memoryStream, (sender, progressChangedEventArgs) =>
                        {
                            PercentagePrep = progressChangedEventArgs.ProgressPercentage;
                            InvokeAsync(StateHasChanged);
                        });
                    }

                    Status = Status.AskParameter;
                }
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
                FileUploadStatus = ValidationStatus.Error;
                Status = Status.ReadyToUpload;
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