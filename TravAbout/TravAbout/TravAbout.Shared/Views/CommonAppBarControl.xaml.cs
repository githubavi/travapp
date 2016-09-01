using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TravAbout.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TravAbout.Views
{
    public sealed partial class CommonAppBarControl : Page
    {
        public CommonAppBarControl()
        {
            this.InitializeComponent();
            emergencyFlyout.Opened += emergencyFlyout_Opened;
        }

        void emergencyFlyout_Opened(object sender, object e)
        {
            txtMobNo.Focus(FocusState.Programmatic);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Notify TravAbout with highest priority about user details for any callback or further assistance
            //this.
            emergencyFlyout.Hide();
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToPage(typeof(MainPage));
        }

        private void pendingReviewButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to pending review page
        }
    }
}
