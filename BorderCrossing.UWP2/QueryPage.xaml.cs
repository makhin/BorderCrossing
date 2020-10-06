using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BorderCrossing.Extensions;
using BorderCrossing.Models;
using BorderCrossing.Res;
using BorderCrossing.Services;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Core;
using BorderCrossing.DbContext;
using BorderCrossing.Models.Google;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BorderCrossing.UWP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueryPage : Page
    {
        private readonly IBorderCrossingService _borderCrossingService;

        public QueryPage() : this(App.Services.GetRequiredService<IBorderCrossingService>()) { }

        public readonly string FromLabel = Strings.QueryStartDateLabel;
        public readonly string ToLabel = Strings.QueryEndDateLabel;

        public LocationHistory LocationHistory { get; set; }

        private List<string> IntervalLabels
        {
            get
            {
                var result = new List<string>();
                foreach (IntervalType val in Enum.GetValues(typeof(IntervalType)))
                {
                    result.Add(val.GetDisplayName());
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
