using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using Tewr.Blazor.FileReader;

namespace BorderCrossing.Pages
{
    public class UploadFileBase : ComponentBase
    {
        [Inject] 
        public IBorderCrossingService BorderCrossingService { get; set; }

        [Inject] 
        public IFileReaderService FileReaderService { get; set; }

        public ElementReference InputTypeFileElement { get; set; }

        public int PercentageLoad { get; set; }

        public int PercentagePrep { get; set; }

        public int PercentageProc { get; set; }

        public string Status { get; set; }

        public DateRangePostRequest DateRangePostRequest { get; set; }

        public BorderCrossingResponse BorderCrossingResponse { get; set; }

        public async Task HandleValidSubmit()
        {
            BorderCrossingResponse = await BorderCrossingService.ParseLocationHistoryAsync(DateRangePostRequest, (sender, e) =>
            {
                PercentageProc = e.ProgressPercentage;
                this.InvokeAsync(this.StateHasChanged);
            });
        }

        public async Task ReadFile()
        {
            this.StateHasChanged();
            var files = await FileReaderService.CreateReference(InputTypeFileElement).EnumerateFilesAsync();
            foreach (var file in files)
            {
                var fileInfo = await file.ReadFileInfoAsync();
                this.StateHasChanged();
                const int onlyReportProgressAfterThisPercentDelta = 1;

                fileInfo.PositionInfo.PositionChanged += (s, e) =>
                {
                    if (e.PercentageDeltaSinceAcknowledge > onlyReportProgressAfterThisPercentDelta)
                    {
                        this.InvokeAsync(this.StateHasChanged);
                        e.Acknowledge();
                        PercentageLoad =  (int)e.Percentage;
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
                            this.StateHasChanged();
                            DateRangePostRequest =  await BorderCrossingService.PrepareLocationHistoryAsync(memoryStream, (sender, e) =>
                            {
                                PercentagePrep = e.ProgressPercentage;
                                this.InvokeAsync(this.StateHasChanged);
                            });
                        }
                    }

                    this.StateHasChanged();
                }
                catch (OperationCanceledException)
                {
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(1);
                }
            }
        }
    }
}