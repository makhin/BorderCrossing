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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BorderCrossing.UWP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueryPage : Page
    {
        public readonly string FromLabel = Strings.QueryStartDateLabel;
        public readonly string ToLabel = Strings.QueryEndDateLabel;

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
        public QueryPage()
        {
            this.InitializeComponent();
            this.ViewModelQueryRequest = new QueryRequest()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                IntervalType = IntervalType.Every12Hours,
            };
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
