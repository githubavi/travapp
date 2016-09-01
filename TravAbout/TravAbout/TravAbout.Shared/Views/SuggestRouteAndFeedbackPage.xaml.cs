using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using TravAbout.Common;
using TravAbout.Communication;
using TravAbout.Data;
using TravAbout.DataModel;
using TravAbout.Geo;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TravAbout.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SuggestRouteAndFeedbackPage : Page
    {
        private NavigationHelper navigationHelper;
        DispatcherTimer locTimer;
        CoreDispatcher cd;

        public SuggestRouteAndFeedbackPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            ProbableVisitedPlaces = new ObservableCollection<Place>();
            VisitedPlaces = new ObservableDictionary();
            locTimer = new DispatcherTimer();
            locTimer.Interval = new TimeSpan(0, 1, 0);
            locTimer.Stop();
            locTimer.Tick += locTimer_Tick;
            this.DataContext = this;
            MapViewer.NavigationCompleted += MapViewer_NavigationCompleted;
            WebSocketCommunicator.Instance.WebSocketMessageReceived += ChatMessageReceived;
            cd = Window.Current.CoreWindow.Dispatcher;
            LoadCurrentMap();
        }


        private double CalculateDistance(double prevLat, double prevLong, double currLat, double currLong)
        {
            const double degreesToRadians = (Math.PI / 180.0);
            const double earthRadius = 6378.14; // kilometers

            // convert latitude and longitude values to radians
            var prevRadLat = prevLat * degreesToRadians;
            var prevRadLong = prevLong * degreesToRadians;
            var currRadLat = currLat * degreesToRadians;
            var currRadLong = currLong * degreesToRadians;

            // calculate radian delta between each position.
            var radDeltaLat = currRadLat - prevRadLat;
            var radDeltaLong = currRadLong - prevRadLong;

            // calculate distance
            var expr1 = (Math.Sin(radDeltaLat / 2.0) *
                         Math.Sin(radDeltaLat / 2.0)) +

                        (Math.Cos(prevRadLat) *
                         Math.Cos(currRadLat) *
                         Math.Sin(radDeltaLong / 2.0) *
                         Math.Sin(radDeltaLong / 2.0));

            var expr2 = 2.0 * Math.Atan2(Math.Sqrt(expr1),
                                         Math.Sqrt(1 - expr1));

            var distance = (earthRadius * expr2);
            return distance * 1000; // return results as meter
        }

        async void MapViewer_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (isfirsttime)
            {
                isfirsttime = false;
                await ShowBestPossibleRoutes();
            }
            else
            {
                //triggered from map button event
                await ShowSingleRoutedirections();
            }
        }

        private async Task ShowSingleRoutedirections()
        {
            try
            {
                if (Util.UserProfileData.CurrentPlace != null && selectedPlace != null)
                {
                    await MapViewer.InvokeScriptAsync("calcRoute", new string[] { Util.UserProfileData.CurrentPlace.Latitude.ToString(), 
                        Util.UserProfileData.CurrentPlace.Longitude.ToString(), 
                        selectedPlace.Latitude.ToString(), selectedPlace.Longitude.ToString(), 
                        Util.UserProfileData.CurrentPlace.Address, selectedPlace.DisplayName  });
                }
            }
            catch (Exception ex)
            {
                Util.HandleMessage("Map direction couldn't be loaded", ex.ToString(), "Error");
            }
        }

        private async Task ShowBestPossibleRoutes()
        {
            try
            {
                //Show best possible route path
                if (Util.UserProfileData.CurrentPlace != null)
                {
                    await MapViewer.InvokeScriptAsync("Reset", null);

                    List<KeyValuePair<double, string>> places = new List<KeyValuePair<double, string>>();

                    foreach (var item in ProbableVisitedPlaces)
                    {
                        places.Add(new KeyValuePair<double, string>(
                            CalculateDistance(Util.UserProfileData.StartPlace.Latitude, Util.UserProfileData.StartPlace.Longitude, item.Latitude, item.Longitude), item.UniqueId));
                    }

                    var maxdistance = places.Max(p => p.Key);
                    var destinationUniqueId = places.Where(p => p.Key == maxdistance).Select(p => p.Value).First();
                    var destination = ProbableVisitedPlaces.First(p => p.UniqueId == destinationUniqueId);

                    var waypoints = ProbableVisitedPlaces.Where(p => p.UniqueId != Util.UserProfileData.StartPlace.UniqueId && p.UniqueId != destinationUniqueId);
                    foreach (var point in waypoints)
                    {
                        await MapViewer.InvokeScriptAsync("createWayPoints", new string[] { point.Latitude.ToString(), point.Longitude.ToString(), point.DisplayName });
                    }

                    await MapViewer.InvokeScriptAsync("calcRoute", new string[] { Util.UserProfileData.StartPlace.Latitude.ToString(), 
                        Util.UserProfileData.StartPlace.Longitude.ToString(), destination.Latitude.ToString(), destination.Longitude.ToString(), 
                        Util.UserProfileData.StartPlace.Address, destination.DisplayName   });
                }
            }
            catch (Exception ex)
            {
                Util.HandleMessage("Map direction couldn't be loaded", ex.ToString(), "Error");
            }
        }

        void locTimer_Tick(object sender, object e)
        {
            LoadDataFromRuntimeLocationTrack();
        }

        bool isfirsttime = false;
        async void LoadCurrentMap()
        {
            isfirsttime = true;
            if (Util.UserProfileData.StartPlace != null)
            {
                var mapresponse = await Util.GetGeodirectionGMap(Util.UserProfileData.StartPlace.Latitude.ToString(),
                                            Util.UserProfileData.StartPlace.Longitude.ToString(), @"ms-appx:///Geo/BestRoute.html");
                MapViewer.NavigateToString(mapresponse);
            }
        }

        ICommand addCommentCmd;
        public ICommand AddCommentsCommand
        {
            get
            {
                return addCommentCmd ?? new RelayCommand(AddComments);
            }
        }

        ICommand deleteCommentCmd;
        public ICommand PlcaeDeleteCommand
        {
            get
            {
                return deleteCommentCmd ?? new RelayCommand(DeletePlace);
            }
        }

        ICommand showDirCmd;
        public ICommand ShowDirectionCommand
        {
            get
            {
                return showDirCmd ?? new RelayCommand(ShowDirectionInMap);
            }
        }

        async void ShowDirectionInMap(object parameter)
        {
            if (!isfirsttime)
            {
                var destination = parameter as Place;
                if (destination != null)
                {
                    selectedPlace = destination;

                    //load map
                    var mapresponse = await Util.GetGeodirectionGMap(Util.UserProfileData.CurrentPlace.Latitude.ToString(), Util.UserProfileData.CurrentPlace.Longitude.ToString(),
                        @"ms-appx:///Geo/SuggestedRoute.html");
                    MapViewer.NavigateToString(mapresponse);
                }
            }
        }

        Place selectedPlace;

        void AddComments(object parameter)
        {
            if (commentFlyout != null)
                commentFlyout.Hide();
        }

        void DeletePlace(object parameter)
        {
            this.ProbableVisitedPlaces.Remove(parameter as Place);
        }

        public ObservableCollection<Place> ProbableVisitedPlaces { get; set; }

        ObservableDictionary VisitedPlaces { get; set; }


        private async Task LoadDataFromRuntimeLocationTrack()
        {
            foreach (var vplace in Util.UserProfileData.ProbableVisitedPlaces)
            {
                if(!VisitedPlaces.ContainsKey(vplace.UniqueId))
                {
                    VisitedPlaces[vplace.UniqueId] = vplace;
                    (VisitedPlaces[vplace.UniqueId] as Place).DisplayName = (VisitedPlaces[vplace.UniqueId] as Place).Address;
                    ProbableVisitedPlaces.Insert(0, VisitedPlaces[vplace.UniqueId] as Place);

                    //refresh the map to show best possible route
                    await ShowBestPossibleRoutes();
                }
            }
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //Load desired places from user selection
            var grItems = e.NavigationParameter as IEnumerable<SampleDataItem>;
            if(grItems != null)
            {
                foreach (var item in grItems)
                {
                    var pos = item.Position.Split(",".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                    var key = pos[0].Trim() + pos[1].Trim();

                    if (!VisitedPlaces.ContainsKey(key))
                    {
                        Place p = new Place
                        {
                            Name = item.Title,
                            Address = item.Description,
                            DisplayName = item.Title + Environment.NewLine + item.Description,
                            Latitude = Convert.ToDouble(pos[0].Trim()),
                            Longitude = Convert.ToDouble(pos[1].Trim())
                        };
                        VisitedPlaces[key] = p;
                        ProbableVisitedPlaces.Add(p);
                    }
                }
            }
            locTimer.Stop();
            locTimer.Start();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }


        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserProfile));
        }

        Flyout commentFlyout = null;
        private void commentFlyout_Opened(object sender, object e)
        {
            commentFlyout = sender as Flyout;
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
