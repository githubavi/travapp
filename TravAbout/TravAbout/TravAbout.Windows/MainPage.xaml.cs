using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TravAbout.BusinessRules;
using TravAbout.Common;
using TravAbout.DataModel;
using TravAbout.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TravAbout
{    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainViewModel mainViewModel;
        NavigationHelper navigationHelper;
        
        public MainPage()
        {
            this.InitializeComponent();
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;
            navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            //ValidateAuthenticationToken();
        }

        void ValidateAuthenticationToken()
        {
            if (Util.IsAuthenticated())
            {
                this.txtLabel.Visibility = Visibility.Collapsed;
                this.btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                prAuthStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.txtLabel.Visibility = Visibility.Visible;
                this.btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                prAuthStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await NotificationManager.SetQuoteNotifications();
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void btnFacebookLogin_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.LogInWithFaceBook(this);
        }

        public void NavigateToProfilePage()
        {
            this.Frame.Navigate(typeof(UserProfile));
        }
        

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
