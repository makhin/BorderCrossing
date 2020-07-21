using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BorderCrossing.Components;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using Tewr.Blazor.FileReader;

namespace BorderCrossing.Pages
{
    public class UploadFileBase : ComponentBase
    {
        public long Max;
        public long Value;
        public CancellationTokenSource CancellationTokenSource;

        [Inject] 
        public IBorderCrossingService BorderCrossingService { get; set; }

        [Inject] 
        public IFileReaderService FileReaderService { get; set; }

        public ElementReference InputTypeFileElement { get; set; }

        public string Status { get; set; }

        public bool CanCancel { get; set; }

        public EventCallback<FileUploaderBaseEventArgs> OnUploadCompleteCallback { get; set; }

        public DateRangePostRequest DateRangePostRequest { get; set; }

        public BorderCrossingResponse BorderCrossingResponse { get; set; }

        protected async Task HandleValidSubmit()
        {
            BorderCrossingResponse = await BorderCrossingService.ParseLocationHistoryAsync(DateRangePostRequest);
        }

        protected async Task OnUploadComplete(FileUploaderBaseEventArgs e)
        {
            Status = "Preparing locations";
            DateRangePostRequest = await BorderCrossingService.PrepareLocationHistoryAsync(e.Stream, null);
        }

        public async Task ReadFile()
        {
            Max = 0;
            Value = 0;
            this.StateHasChanged();
            var files = await FileReaderService.CreateReference(InputTypeFileElement).EnumerateFilesAsync();
            foreach (var file in files)
            {
                var fileInfo = await file.ReadFileInfoAsync();
                Max = fileInfo.Size;

                this.StateHasChanged();
                CancellationTokenSource?.Dispose();
                CancellationTokenSource = new System.Threading.CancellationTokenSource();
                CanCancel = true;

                const int onlyReportProgressAfterThisPercentDelta = 5;

                fileInfo.PositionInfo.PositionChanged += (s, e) =>
                {
                    if (e.PercentageDeltaSinceAcknowledge > onlyReportProgressAfterThisPercentDelta)
                    {
                        this.InvokeAsync(this.StateHasChanged);
                        e.Acknowledge();
                        Value = e.Position;
                    }
                };

                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var stream = await file.OpenReadAsync())
                        {
                            await stream.CopyToAsync(memoryStream, CancellationTokenSource.Token);
                            Value = Max;
                            DateRangePostRequest =  await BorderCrossingService.PrepareLocationHistoryAsync(memoryStream, null);
                        }
                    }

                    this.StateHasChanged();
                }
                catch (OperationCanceledException)
                {
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(1);
                }
                finally
                {
                    CanCancel = false;
                }
            }
        }
    }
}