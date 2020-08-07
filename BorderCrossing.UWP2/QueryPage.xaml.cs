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

    public class NamedIntervalType
    {
        public string Name { get; private set; }
        public IntervalType IntervalType { get; private set; }
        public NamedIntervalType(string name, IntervalType intervalType)
        {
            this.Name = name;
            this.IntervalType = intervalType;
        }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueryPage : Page
    {
        public readonly string FromLabel = Strings.QueryStartDateLabel;
        public readonly string ToLabel = Strings.QueryEndDateLabel;

        private List<NamedIntervalType> IntervalTypes { get; } = new List<NamedIntervalType>()
        {
            new NamedIntervalType(IntervalType.Day.GetDisplayName(), IntervalType.Day),
            new NamedIntervalType(IntervalType.Every12Hours.GetDisplayName(), IntervalType.Every12Hours),
            new NamedIntervalType(IntervalType.Hour.GetDisplayName(), IntervalType.Hour),
        };

        public QueryRequest ViewModelQueryRequest;
        public QueryPage()
        {
            this.InitializeComponent();
            this.ViewModelQueryRequest = new QueryRequest();
        }

        private void IntervalComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            IntervalComboBox.SelectedIndex = 1;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
