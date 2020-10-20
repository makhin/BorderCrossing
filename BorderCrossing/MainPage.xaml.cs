﻿using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BorderCrossing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public readonly string HomeLabel;
        public readonly string HowLabel;
        public readonly string UploadLabel;        

        /// <summary>
        /// Gets the navigation frame instance.
        /// </summary>
        public Frame AppFrame => frame;

        public MainPage()
        {
            this.InitializeComponent();

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            HomeLabel = resourceLoader.GetString("HomeNav");
            HowLabel = resourceLoader.GetString("HowNav");
            UploadLabel = resourceLoader.GetString("UploadNav");

            Loaded += (sender, args) =>
            {
                NavView.SelectedItem = HomeMenuItem;
                AppFrame.Navigate(typeof(HomePage));
            };
        }

        public void SetSelectedItem(string text)
        {
            if (text == "how")
            {
                NavView.SelectedItem = HowMenuItem;
            } 
            else if (text == "upload")
            {
                NavView.SelectedItem = UploadMenuItem;
            }
        }

        /// <summary>
        /// Navigates to the page corresponding to the tapped item.
        /// </summary>
        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            var label = args.InvokedItem as string;
            var pageType =
                label == HomeLabel ? typeof(HomePage) :
                label == UploadLabel ? typeof(UploadPage) :
                label == HowLabel ? typeof(HowPage) : null;
            if (pageType != null && pageType != AppFrame.CurrentSourcePageType)
            {
                AppFrame.Navigate(pageType);
            }
        }

        /// <summary>
        /// Ensures the nav menu reflects reality when navigation is triggered outside of
        /// the nav menu buttons.
        /// </summary>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (e.SourcePageType == typeof(HomePage))
                {
                    NavView.SelectedItem = HomeMenuItem;
                }
                else if (e.SourcePageType == typeof(HowPage))
                {
                    NavView.SelectedItem = HowMenuItem;
                }
                else if (e.SourcePageType == typeof(UploadPage))
                {
                    NavView.SelectedItem = UploadMenuItem;
                }
            }
        }



        /// <summary>
        /// Navigates the frame to the previous page.
        /// </summary>
        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            if (AppFrame.CanGoBack)
            {
                AppFrame.GoBack();
            }
        }
    }
}