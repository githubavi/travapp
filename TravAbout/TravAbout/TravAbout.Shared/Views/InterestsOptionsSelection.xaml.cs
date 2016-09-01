using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using System.Collections.ObjectModel;
using TravAbout.DataModel;
using TravAbout.Communication;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TravAbout.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterestsOptionsSelection : Page
    {
        private NavigationHelper navigationHelper;
        CoreDispatcher cd;

        public InterestsOptionsSelection()
        {
            this.InitializeComponent();

            //if (!Util.IsAuthenticated())
            //    this.Frame.Navigate(typeof(MainPage));

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            WebSocketCommunicator.Instance.WebSocketMessageReceived += ChatMessageReceived;
            cd = Window.Current.CoreWindow.Dispatcher;
            PopulateOptions();
        }

        private async Task PopulateOptions()
        {
            Util.UserProfileData.FinalInterests = await Util.LoadAllInterests();
            Interests.ItemsSource = Util.UserProfileData.FinalInterests;
        }

        public List<InterestOption> InterestsColl { get; set; }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //if (InterestsColl.Any())
            //    Interests.ItemsSource = InterestsColl;
            //else
            //{
            //    InterestsColl = await Util.LoadAllInterests();
            //    Interests.ItemsSource = InterestsColl;
            //}
        }


        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
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

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ItemsPage), InterestsColl);
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
