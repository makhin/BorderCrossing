using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BorderCrossing.Res;
using BorderCrossing.Services;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BorderCrossing.UWP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadPage : Page
    {
        private string ZipWarning { get; }

        public UploadPage()
        {
            this.InitializeComponent();
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ZipWarning = resourceLoader.GetString("ZipWarning");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add(".zip");

            var file = await picker.PickSingleFileAsync();

            if (file == null) return;

            if (Path.GetExtension(file.Name) != ".zip")
            {
                throw new Exception(ZipWarning);
            }

            using (var fileStream = await file.OpenStreamForReadAsync())
            {
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var locationHistory = await BorderCrossingHelper.ExtractJsonAsync(memoryStream, (s, progressChangedEventArgs) =>
                {
                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ProgressBarPrep.Value = progressChangedEventArgs.ProgressPercentage;
                    });
                });

                this.Frame.Navigate(typeof(QueryPage), locationHistory);
            }
        }
    }
}
