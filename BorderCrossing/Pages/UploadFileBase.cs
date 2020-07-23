using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using Tewr.Blazor.FileReader;

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

        [Inject] 
        public IFileReaderService FileReaderService { get; set; }

        public Status Status { get; set; }

        public string ErrorMessage { get; set; }

        public ElementReference InputTypeFileElement { get; set; }

        public int PercentageLoad { get; set; }

        public int PercentagePrep { get; set; }

        public int PercentageProc { get; set; }

        public DateRangePostRequest DateRangePostRequest { get; set; }

        public BorderCrossingResponse BorderCrossingResponse { get; set; }

        protected override void OnInitialized()
        {
            Status = Status.ReadyToUpload;
        }

        public async Task HandleValidSubmit()
        {
            Status = Status.Processing;
            BorderCrossingResponse = await BorderCrossingService.ParseLocationHistoryAsync(DateRangePostRequest, (sender, e) =>
            {
                PercentageProc = e.ProgressPercentage;
                InvokeAsync(StateHasChanged);
            });
            Status = Status.Results;
        }

        public async Task ReadFile()
        {
            StateHasChanged();
            var files = await FileReaderService.CreateReference(InputTypeFileElement).EnumerateFilesAsync();
            try
            {
                foreach (var file in files)
                {
                    var fileInfo = await file.ReadFileInfoAsync();
                    if (Path.GetExtension(fileInfo.Name) != ".zip")
                    {
                        throw new Exception("Файл должен иметь расширение .zip");
                    }

                    Status = Status.Uploading;
                    StateHasChanged();
                    const int onlyReportProgressAfterThisPercentDelta = 1;

                    fileInfo.PositionInfo.PositionChanged += (s, e) =>
                    {
                        if (e.PercentageDeltaSinceAcknowledge > onlyReportProgressAfterThisPercentDelta)
                        {
                            InvokeAsync(StateHasChanged);
                            e.Acknowledge();
                            PercentageLoad = (int)e.Percentage;
                        }
                    };

                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var stream = await file.OpenReadAsync())
                            {
                                await stream.CopyToAsync(memoryStream);
                                PercentageLoad = 100;
                                StateHasChanged();
                                Status = Status.Deserializing;
                                DateRangePostRequest = await BorderCrossingService.PrepareLocationHistoryAsync(memoryStream, (sender, e) =>
                                {
                                    PercentagePrep = e.ProgressPercentage;
                                    InvokeAsync(StateHasChanged);
                                });
                            }
                        }

                        Status = Status.AskParameter;
                        StateHasChanged();
                    }
                    catch (OperationCanceledException)
                    {
                        await InvokeAsync(StateHasChanged);
                        await Task.Delay(1);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                Status = Status.ReadyToUpload;
                PercentageLoad = 0;
                PercentagePrep = 0;
                StateHasChanged();
            }
        }
    }
}