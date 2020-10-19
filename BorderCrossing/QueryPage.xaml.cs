using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BorderCrossing.Models;
using BorderCrossing.Models.Google;
using BorderCrossing.Services;
using Microsoft.Extensions.DependencyInjection;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BorderCrossing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueryPage : Page
    {
        private readonly IBorderCrossingService _borderCrossingService;

        public QueryPage() : this(App.Services.GetRequiredService<IBorderCrossingService>()) { }
        public LocationHistory LocationHistory { get; set; }

        private List<string> IntervalLabels
        {
            get
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                var result = new List<string>();
                foreach (IntervalType val in Enum.GetValues(typeof(IntervalType)))
                {
                    result.Add(resourceLoader.GetString(val.ToString()));
                }
                return result;
            }
        }

        public QueryRequest ViewModelQueryRequest;
        public QueryPage(IBorderCrossingService borderCrossingService)
        {
            _borderCrossingService = borderCrossingService;
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.LocationHistory = (LocationHistory)e.Parameter;
            this.ViewModelQueryRequest = await _borderCrossingService.GetQueryRequestAsync(this.LocationHistory);
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            List<CheckPoint> checkpoints = await _borderCrossingService.ParseLocationHistoryAsync(this.LocationHistory, this.ViewModelQueryRequest, (sender, e) =>
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ProgressBarProc.Value = e.ProgressPercentage;
                });
            });

            this.Frame.Navigate(typeof(ResultPage), checkpoints);
        }
    }
}
