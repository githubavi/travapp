using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using TravAbout.Common;
using TravAbout.Communication;
using TravAbout.Data;
using TravAbout.DataModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace TravAbout.Views
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split App this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary itemsViewModel = new ObservableDictionary();
        CoreDispatcher cd;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary ItemsViewModel
        {
            get { return this.itemsViewModel; }
        }

        bool firstTimeInitialization;
        public ItemsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            WebSocketCommunicator.Instance.WebSocketMessageReceived += ChatMessageReceived;
            cd = Window.Current.CoreWindow.Dispatcher;
            firstTimeInitialization = true;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            isActiveProgressRing.IsActive = true;
            btnNext.IsEnabled = false;
            
            var finalgroups = new List<SampleDataGroup>();
            var interestoptions = e.NavigationParameter as List<InterestOption>;
            
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            
            if (interestoptions != null)
            {
                var positiveinterests = interestoptions.Where(io => io.IsInterested).Select(io => io.GroupName).ToList();
                sampleDataGroups = sampleDataGroups.Where(gr => positiveinterests.Contains(gr.Title));
            }

            this.ItemsViewModel["Items"] = sampleDataGroups;
            itemsViewSource.Source = this.ItemsViewModel["Items"];

            if (firstTimeInitialization)
            {
                //make a delay to load external images from web
                await Task.Delay(5000);
                isActiveProgressRing.IsActive = false;
                btnNext.IsEnabled = CanGoToNextPage();
                firstTimeInitialization = false;
            }
            else
            {
                isActiveProgressRing.IsActive = false;
                btnNext.IsEnabled = CanGoToNextPage();
            }
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(SplitPage), groupId);
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
            if(CanGoToNextPage())
                this.Frame.Navigate(typeof(SuggestRouteAndFeedbackPage), interestedItems);
        }

        List<SampleDataItem> interestedItems = new List<SampleDataItem>();

        private bool CanGoToNextPage()
        {
            var groups = itemsViewSource.Source as IEnumerable<SampleDataGroup>;
            interestedItems.Clear();

            if (groups != null)
            {
                var grItems = groups.Select(gr => gr.Items);

                foreach (var items in grItems)
                {
                    foreach (var item in items)
                    {
                        if (item.Interested)
                            interestedItems.Add(item);
                    }
                }
            }

            return interestedItems.Any();
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
