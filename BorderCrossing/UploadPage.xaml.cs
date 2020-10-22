using System;
using System.IO;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BorderCrossing.Services;

namespace BorderCrossing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadPage : Page
    {
        public UploadPage()
        {
            this.InitializeComponent();
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
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                throw new Exception(resourceLoader.GetString("ZipWarning"));
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
