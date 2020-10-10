using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BorderCrossing.Res;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BorderCrossing.UWP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public readonly string Title = Strings.HomeTitle;
        public readonly string HomeP1 = Strings.HomeP1;
        public readonly string HomeP21 = Strings.HomeP21;
        public readonly string HomeP22 = Strings.HomeP22;
        public readonly string HomeP3 = Strings.HomeP3;
        public readonly string HomeP4 = Strings.HomeP4;
        public readonly string HomeP5 = Strings.HomeP5;

        public HomePage()
        {
            this.InitializeComponent();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(HowPage));
            ((Window.Current.Content as Frame).Content as MainPage).SetSelectedItem("how");
        }
    }
}
