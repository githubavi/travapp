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

// The Split Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234234

namespace TravAbout.Views
{
    /// <summary>
    /// A page that displays a group title, a list of items within the group, and details for
    /// the currently selected item.
    /// </summary>
    public sealed partial class SplitPage : Page
    {
        private NavigationHelper navigationHelper;
        CoreDispatcher cd;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

       

        public SplitPage()
        {
            this.InitializeComponent();


            // Setup the navigation helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            // Setup the logical page navigation components that allow
            // the page to only show one pane at a time.
            this.navigationHelper.GoBackCommand = new RelayCommand((p) => this.GoBack(), (p) => this.CanGoBack());
            this.itemListView.SelectionChanged += ItemListView_SelectionChanged;

            // Start listening for Window size changes 
            // to change from showing two panes to showing a single pane
            Window.Current.SizeChanged += Window_SizeChanged;
            this.InvalidateVisualState();

            this.Unloaded += SplitPage_Unloaded;

            adContent.Content = AddDuplexControlWin8Phone.GetAdControl();
            mapFlyout.Opened += mapFlyout_Opened;
        }

        async void mapFlyout_Opened(object sender, object e)
        {
            var locarr = ((SampleDataItem)itemListView.SelectedItem).Position.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var loadedmap = await GetGeoLocationMap(locarr[0], locarr[1], ((SampleDataItem)itemListView.SelectedItem).Description);
            MapViewer.NavigateToString(loadedmap);
        }

        /// <summary>
        /// Unhook from the SizedChanged event when the SplitPage is Unloaded.
        /// </summary>
        private void SplitPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private ObservableDictionary itemDetailsViewModel = new ObservableDictionary();
        
        public ObservableDictionary ItemDetailsViewModel
        {
            get { return this.itemDetailsViewModel; }
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
            var group = await SampleDataSource.GetGroupAsync((String)e.NavigationParameter);
            this.ItemDetailsViewModel["Group"] = group;
            this.ItemDetailsViewModel["Items"] = group.Items;
            itemsViewSource.Source = this.ItemDetailsViewModel["Items"];

            if (e.PageState == null)
            {
                this.itemListView.SelectedItem = null;
                // When this is a new page, select the first item automatically unless logical page
                // navigation is being used (see the logical page navigation #region below.)
                if (!this.UsingLogicalPageNavigation() && this.itemsViewSource.View != null)
                {
                    this.itemsViewSource.View.MoveCurrentToFirst();
                }
            }
            else
            {
                // Restore the previously saved state associated with this page
                if (e.PageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null)
                {
                    var selectedItem = await SampleDataSource.GetItemAsync((String)e.PageState["SelectedItem"]);
                    this.itemsViewSource.View.MoveCurrentTo(selectedItem);
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (this.itemsViewSource.View != null)
            {
                var selectedItem = (Data.SampleDataItem)this.itemsViewSource.View.CurrentItem;
                if (selectedItem != null) e.PageState["SelectedItem"] = selectedItem.UniqueId;
            }
        }

        #region Logical page navigation

        // The split page isdesigned so that when the Window does have enough space to show
        // both the list and the dteails, only one pane will be shown at at time.
        //
        // This is all implemented with a single physical page that can represent two logical
        // pages.  The code below achieves this goal without making the user aware of the
        // distinction.

        private const int MinimumWidthForSupportingTwoPanes = 768;

        /// <summary>
        /// Invoked to determine whether the page should act as one logical page or two.
        /// </summary>
        /// <returns>True if the window should show act as one logical page, false
        /// otherwise.</returns>
        private bool UsingLogicalPageNavigation()
        {
            return Window.Current.Bounds.Width < MinimumWidthForSupportingTwoPanes;
        }

        /// <summary>
        /// Invoked with the Window changes size
        /// </summary>
        /// <param name="sender">The current Window</param>
        /// <param name="e">Event data that describes the new size of the Window</param>
        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary>
        /// Invoked when an item within the list is selected.
        /// </summary>
        /// <param name="sender">The GridView displaying the selected item.</param>
        /// <param name="e">Event data that describes how the selection was changed.</param>
        private async void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Invalidate the view state when logical page navigation is in effect, as a change
            // in selection may cause a corresponding change in the current logical page.  When
            // an item is selected this has the effect of changing from displaying the item list
            // to showing the selected item's details.  When the selection is cleared this has the
            // opposite effect.
            if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();

            if (itemListView.SelectedItem == null)
                return;
        }

        string currentUniqueId = string.Empty;

        async Task<string> GetGeoLocationMap(string lat, string lng, string add)
        {
            string fileContent = "";
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Geo/CurrentLocationStatic.html"));
                using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                    fileContent = await sRead.ReadToEndAsync();

                var fs = fileContent.Replace("#lat#", lat)
                            .Replace("#lng#", lng)
                            .Replace("#add#", add);

                return fs;
            }
            catch (Exception ex)
            {
                Util.HandleMessage("Location can not be loaded", ex.ToString(), "Error");
                return string.Empty;
            }
        }

        //public async Task ShowUserLocationOnMap(string lat, string lng, string description)
        //{
        //    try
        //    {
        //        await MapViewer.InvokeScriptAsync("setLocation", new string[] { lat, lng, description });
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.HandleMessage("Locatin couldn't be set in map", ex.ToString(), "Error");
        //    }
        //}

        private bool CanGoBack()
        {
            if (this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null)
            {
                return true;
            }
            else
            {
                return this.navigationHelper.CanGoBack();
            }
        }
        private void GoBack()
        {
            if (this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null)
            {
                // When logical page navigation is in effect and there's a selected item that
                // item's details are currently displayed.  Clearing the selection will return to
                // the item list.  From the user's point of view this is a logical backward
                // navigation.
                this.itemListView.SelectedItem = null;
            }
            else
            {
                this.navigationHelper.GoBack();
            }
        }

        private void InvalidateVisualState()
        {
            var visualState = DetermineVisualState();
            VisualStateManager.GoToState(this, visualState, false);
            this.navigationHelper.GoBackCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Invoked to determine the name of the visual state that corresponds to an application
        /// view state.
        /// </summary>
        /// <returns>The name of the desired visual state.  This is the same as the name of the
        /// view state except when there is a selected item in portrait and snapped views where
        /// this additional logical page is represented by adding a suffix of _Detail.</returns>
        private string DetermineVisualState()
        {
            if (!UsingLogicalPageNavigation())
                return "PrimaryView";

            // Update the back button's enabled state when the view state changes
            var logicalPageBack = this.UsingLogicalPageNavigation() && this.itemListView.SelectedItem != null;

            return logicalPageBack ? "SinglePane_Detail" : "SinglePane";
        }

        #endregion

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