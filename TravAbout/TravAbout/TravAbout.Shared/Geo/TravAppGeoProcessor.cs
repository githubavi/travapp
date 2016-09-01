using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TravAbout.BusinessRules;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using System.Linq;
using System.Threading.Tasks;
using TravAbout.Common;
using TravAbout.Data;
using TravAbout.Views;
using System.Threading;
using System.Diagnostics;

namespace TravAbout.Geo
{
    internal class GeoPlace
    {
        public string Address { get; set; }
        public string Place { get; set; }

        public string DisplayName { get; set; }
    }
    public class TravAppGeoProcessor
    {
        Geolocator geo = null;
        CoreDispatcher cd;
        UserProfileViewModel userVM;
        HttpClient client;

        string googleplaceurl = "https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}";

        DispatcherTimer durationTimer;
        Stopwatch stopWatch = new Stopwatch();
        
        static TravAppGeoProcessor()
        {
            instance = new TravAppGeoProcessor();
        }

        private TravAppGeoProcessor()
        {
            cd = Window.Current.CoreWindow.Dispatcher;
            durationTimer = new DispatcherTimer();
            durationTimer.Stop();
            durationTimer.Interval = new TimeSpan(0, 1, 0); //check for duration every minute
            durationTimer.Tick += durationTimer_Tick;
            //geo.StatusChanged += geo_StatusChanged;
        }

        async void durationTimer_Tick(object sender, object e)
        {
            if (stopWatch.Elapsed.Seconds >= 300) //wait fo >=5 min
            {
                stopWatch.Stop();
                durationTimer.Stop();

                if (currentGeoPosition != null)
                {
                    //GeoPlace gp = await GetLocationDetails(currentGeoPosition.Coordinate.Point.Position.Latitude, currentGeoPosition.Coordinate.Point.Position.Longitude);
                    //if (gp != null)
                    //{
                        await SetLocationDetails(currentGeoPosition);
                    //}
                }
            }
        }

        static TravAppGeoProcessor instance;
        public static TravAppGeoProcessor Instance
        {
            get
            {
                return instance ?? new TravAppGeoProcessor();
            }
        }


        public async Task InitializeGeoPosition(UserProfileViewModel vm)
        {
            this.userVM = vm;
            try
            {
                geo = new Geolocator();
                geo.DesiredAccuracyInMeters = 100;
                geo.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.High;
                geo.PositionChanged -= geo_PositionChanged;
                geo.PositionChanged += geo_PositionChanged;

                Geoposition pos = await geo.GetGeopositionAsync();
                await SetLocationDetails(pos, true);
            }
            catch(UnauthorizedAccessException ux)
            {
                HandleGeoAuthException(ux, "Your curren location access denied");
            }
            catch(Exception ex)
            {
                HandleGeoAuthException(ex, "Oops some error has occured");
            }
        }

        private async void HandleGeoAuthException(Exception e, string displaymessage)
        {
            await NotificationManager.HandleErrorMessageWithToaster(displaymessage, e.ToString());
            NavigationHelper.NavigateToPage(typeof(MainPage));
        }

        private async Task<GeoPlace> GetLocationDetails(double lat, double lng)
        {
            string city = string.Empty;
            string place = string.Empty;
            string country = string.Empty;
            string displayname = string.Empty;

            try
            {
                client = new HttpClient();
                var jsonText = await client.GetStringAsync(string.Format(googleplaceurl, lat, lng));
                JsonObject jsonObject = JsonObject.Parse(jsonText);
                var add = jsonObject["results"].GetArray()[0].GetObject()["formatted_address"].GetString();
                var value = add.Split(",".ToCharArray());
                var valuecount = value.Count();
                var countryIndex = valuecount - 1;
                var cityIndex = valuecount - 3;

                if (countryIndex > 0)
                    country = value[countryIndex];
                if (cityIndex > 0)
                    place = value[cityIndex];

                if (!string.IsNullOrEmpty(city))
                    place = city;
                if (!string.IsNullOrEmpty(country))
                    displayname = place + "," + country;

                return new GeoPlace { Address = add, DisplayName = displayname, Place = place };
            }
            catch(Exception ex)
            {
                Util.LogToServer(ex.ToString());
                return null;
            }
        }

        private async Task SetLocationDetails(Geoposition pos, bool firsttime = false)
        {
            if (pos != null)
            {
                try
                {
                    var lat = pos.Coordinate.Point.Position.Latitude;
                    var lng = pos.Coordinate.Point.Position.Longitude;

                    var status = GetStatusString(geo.LocationStatus);

                    if (string.IsNullOrEmpty(status))
                    {
                        GeoPlace gp = await GetLocationDetails(lat, lng);

                        if (gp == null)
                            return;

                        Util.UserProfileData.CurrentPlace = new DataModel.Place { Name = gp.Place, CityLocation = gp.Place, Address = gp.Address, Latitude = lat, Longitude = lng, DisplayName = gp.DisplayName };

                        if (firsttime)
                        {
                            Util.UserProfileData.StartPlace = new DataModel.Place { Name = gp.Place, CityLocation = gp.Place, Address = gp.Address, Latitude = lat, Longitude = lng, DisplayName = gp.DisplayName };
                            userVM.CurrentPlace = Util.UserProfileData.CurrentPlace;
                        }


                        foreach (var vp in Util.UserProfileData.ProbableVisitedPlaces)
                        {
                            var placesdistance = CalculateDistance(vp.Latitude, vp.Longitude, lat, lng);

                            if (placesdistance <= 100 && placesdistance > 30)
                            {
                                if (string.Compare(vp.Name, gp.Place, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    Util.UserProfileData.ProbableVisitedPlaces.Add(new DataModel.Place
                                    {
                                        Name = gp.Place,
                                        Address = gp.Address,
                                        DisplayName = gp.DisplayName,
                                        Latitude = lat,
                                        Longitude = lng
                                    });

                                    userVM.CurrentPlace = Util.UserProfileData.CurrentPlace;
                                }
                            }
                            else if(placesdistance <= 30) //approx. same place
                            {
                                vp.HasVisited = true;
                            }
                        }
                    }
                    else
                    {
                        userVM.IsFetchingData = false;
                        Util.HandleMessage(status, status, "Status");
                    }
                }
                catch (System.UnauthorizedAccessException ux)
                {
                    HandleError("UnAuthorized: No data found", ux);
                }
                catch (TaskCanceledException tx)
                {
                    HandleError("Request Cancelled", tx);
                }
                catch (Exception ex)
                {
                    HandleError("OOps Some error occured", ex);
                }
            }
        }

        private async Task HandleError(string displaymessage, Exception e)
        {
            await cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                userVM.IsFetchingData = false;
                Util.HandleMessage(displaymessage, e.ToString(), "Error");
            });
        }

        private string GetStatusString(PositionStatus status)
        {
            var strStatus = "";

            switch (status)
            {
                case PositionStatus.Ready:
                    //strStatus = "Location is available.";
                    break;

                case PositionStatus.Initializing:
                    //strStatus = "Geolocation service is initializing.";
                    break;

                case PositionStatus.NoData:
                    strStatus = "Location service data is not available.";
                    break;

                case PositionStatus.Disabled:
                    strStatus = "Location services are disabled. Use the " +
                                "Settings charm to enable them.";
                    break;

                case PositionStatus.NotInitialized:
                    strStatus = "Location status is not initialized because " +
                                "the app has not yet requested location data.";
                    break;

                case PositionStatus.NotAvailable:
                    strStatus = "Location services are not supported on your system.";
                    break;

                default:
                    strStatus = "Unknown PositionStatus value.";
                    break;
            }
            return (strStatus);
        }

        Geoposition currentGeoPosition;

        private async void geo_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            currentGeoPosition = args.Position;
            double updateDistance = 0;

            var lat = currentGeoPosition.Coordinate.Point.Position.Latitude;
            var lng = currentGeoPosition.Coordinate.Point.Position.Longitude;

            if (Util.UserProfileData.StartPlace == null)
            {
                await cd.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await SetLocationDetails(currentGeoPosition);
                });
            }
            else
            {
                if (Util.UserProfileData.StartPlace.Latitude == -1 
                    || Util.UserProfileData.StartPlace.Longitude == -1
                    || (Util.UserProfileData.StartPlace.Latitude == lat
                            && Util.UserProfileData.StartPlace.Longitude == lng))
                    updateDistance = 0;
                else
                    updateDistance = CalculateDistance(Util.UserProfileData.StartPlace.Latitude, Util.UserProfileData.StartPlace.Longitude, lat, lng);
            }

           if(updateDistance >= 200) //>=1km
           {
               await cd.RunAsync(CoreDispatcherPriority.Normal, async () =>
               {
                   if (string.Compare(Util.UserProfileData.CurrentPlace.CityLocation, Util.UserProfileData.StartPlace.CityLocation, StringComparison.OrdinalIgnoreCase) != 0) //User moved to different place
                   {
                       Util.UserProfileData.HasMoved = false;
                       SampleDataSource.ClearGroups();
                       //this will trigger to load new data items, not from cache
                       NavigationHelper.NavigateToPage(typeof(UserProfile));
                       await NotificationManager.HandleErrorMessageWithToaster("City or Region location has been changed, apps data has been reset", "City location has been changed");
                   }
                   else
                   {
                       Util.UserProfileData.HasMoved = true;
                       //call our service to get type/category details for this current lat, long position, if matched add to user visting places
                       //restart timer,
                        //calculate await time or duration
                       stopWatch.Restart();
                       durationTimer.Stop();
                       durationTimer.Start();
                   }
               });
           }
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
            return distance * 1000; // return results as Meter
        }

        internal void Unsubscribe()
        {
            if(geo != null)
                geo.PositionChanged -= geo_PositionChanged;
            if(durationTimer != null)
                durationTimer.Tick -= durationTimer_Tick;
        }
    }
}
