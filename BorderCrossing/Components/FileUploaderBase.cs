using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blazor.FileReader;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BorderCrossing.Components
{
    public class FileUploaderBase : ComponentBase
    {
        protected ElementReference InputElement;
        public long Max;
        public long Value;

        [Inject]
        public IFileReaderService FileReaderService { get; set; }

        [Inject]
        public IJSRuntime CurrentJsRuntime { get; set; }

        [Parameter]
        public EventCallback<FileUploaderBaseEventArgs> OnUploadCompleteCallback { get; set; }

        public CancellationTokenSource CancellationTokenSource;

        public bool CanCancel { get; set; }

        public bool IsCancelDisabled => !CanCancel;

        public async Task ClearFile()
        {
            await FileReaderService.CreateReference(InputElement).ClearValue();
        }

        public async Task ReadFile()
        {
            Max = 0;
            Value = 0;
            this.StateHasChanged();
            var files = await FileReaderService.CreateReference(InputElement).EnumerateFilesAsync();
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

                            await OnUploadCompleteCallback.InvokeAsync(new FileUploaderBaseEventArgs
                            {
                                Stream = memoryStream
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
                finally
                {
                    CanCancel = false;
                }
            }
        }

        public async Task CancelFile()
        {
            await InvokeAsync(StateHasChanged);
            await Task.Delay(1);
            CancellationTokenSource.Cancel();
        }
    }
}
