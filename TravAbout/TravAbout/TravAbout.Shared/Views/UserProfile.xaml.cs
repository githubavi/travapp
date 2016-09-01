using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TravAbout.BusinessRules;
using TravAbout.Common;
using TravAbout.Communication;
using TravAbout.DataModel;
using TravAbout.Geo;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace TravAbout.Views
{
    public sealed partial class UserProfile : Page
    {
        NavigationHelper navigationHelper;
        UserProfileViewModel userVM;
        CoreDispatcher cd;

        public UserProfile()
        {
            this.InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            userVM = new UserProfileViewModel(this);
            this.DataContext = userVM;
            navigationHelper.LoadState += navigationHelper_LoadState;
            emergencyFlyout.Opened += emergencyFlyout_Opened;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            MapViewer.NavigationCompleted += MapViewer_NavigationCompleted;
            WebSocketCommunicator.Instance.WebSocketMessageReceived += ChatMessageReceived;
            cd = Window.Current.CoreWindow.Dispatcher;
        }

        async void MapViewer_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            navigationcompleted = true;

            var userdata = Util.UserProfileData;

            if (userdata != null)
            {
                await userVM.ShowUserInformation(userdata);
            }
            else
            {
                var message = "User data not found";
                Util.HandleMessage(message, "User data not found, service seems to be down", "Error");
            } 
        }

        void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            btnNext.IsEnabled = false;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            var resstr = await Util.GetGeoLocationGMap();
            MapViewer.NavigateToString(resstr);
        }

        Place currentLocation;
        bool navigationcompleted;

        public async Task SetUserLocationOnMap(Place userLocation)
        {
            currentLocation = userLocation;
            btnNext.IsEnabled = true;

            if (navigationcompleted)
            {    
                try
                {
                    await MapViewer.InvokeScriptAsync("setLocation", new string[] { currentLocation.Latitude.ToString(), currentLocation.Longitude.ToString(), currentLocation.Address });
                }
                catch (Exception ex)
                {
                    Util.HandleMessage("Locatin couldn't be set in map", ex.ToString(), "Error");
                }
            }
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(InterestsOptionsSelection));
        }

        private void homeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToPage(typeof(MainPage));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Notify TravAbout with highest priority about user details for any callback or further assistance
            //this.
            emergencyFlyout.Hide();
        }

        void emergencyFlyout_Opened(object sender, object e)
        {
            txtMobNo.Focus(FocusState.Programmatic);
        }

        private void pendingReviewButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to pending review page
        }

        private async void chatSendButton_Click(object sender, RoutedEventArgs e)
        {
            await WebSocketCommunicator.Instance.SendMessage(txtChatInput.Text.Trim());
        }

        private async void chatFlyout_Opened(object sender, object e)
        {
            txtChatInput.Focus(FocusState.Programmatic);

            //connect to websocket
            await WebSocketCommunicator.Instance.Initialize();
        }

        private async void ChatMessageReceived(object sender, MessageArgs e)
        {
            await cd.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    WebSocketCommunicator.Instance.CurrentResponse = e.Message + Environment.NewLine + WebSocketCommunicator.Instance.CurrentResponse;
                    txtChatDisplay.Text = WebSocketCommunicator.Instance.CurrentResponse;
                });
        }

        private void chatFlyout_Closed(object sender, object e)
        {
            //will do it here something if required
        }

        private async void exploreButton_Click(object sender, RoutedEventArgs e)
        {
           var success = await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.travabout.com"));
           if (!success)
               Util.HandleMessage("TarvAbout page can not be opened", "TarvAbout page can not be opened", "Sorry");
        }
    }
}
