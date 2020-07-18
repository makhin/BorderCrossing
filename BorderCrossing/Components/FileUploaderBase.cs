using System;
using System.Diagnostics;
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
        IFileReaderService FileReaderService { get; set; }

        [Inject]
        IJSRuntime CurrentJsRuntime { get; set; }

        [Parameter]
        public int BufferSize { get; set; } = 20480;

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

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                this.StateHasChanged();
                CancellationTokenSource?.Dispose();
                CancellationTokenSource = new System.Threading.CancellationTokenSource();
                CanCancel = true;

                const int onlyReportProgressAfterThisPercentDelta = 10;

                fileInfo.PositionInfo.PositionChanged += (s, e) =>
                {
                    if (e.PercentageDeltaSinceAcknowledge > onlyReportProgressAfterThisPercentDelta)
                    {
                        stopwatch.Stop();
                        this.InvokeAsync(this.StateHasChanged);
                        e.Acknowledge();
                        Value = e.Position;
                        stopwatch.Start();
                    }
                };

                try
                {
                    var ps = new PositionStream();

                    using (var fs = await file.OpenReadAsync())
                    {
                        await fs.CopyToAsync(ps, BufferSize, CancellationTokenSource.Token);
                        stopwatch.Stop();
                    }

                    Value = Max;
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
