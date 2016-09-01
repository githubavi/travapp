using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TravAbout.Common;
using TravAbout.DataModel;
using TravAbout.Geo;
using TravAbout.Views;
using Windows.Data.Xml.Dom;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;


namespace TravAbout.BusinessRules
{
    public class UserProfileViewModel : BindableBase
    {
        TravAppGeoProcessor geoprocessor;
        UserProfile upage;

        public UserProfileViewModel(UserProfile up)
        {
            this.geoprocessor = TravAppGeoProcessor.Instance;
            this.upage = up;
        }

        public async Task ShowUserInformation(UserData userdata)
        {
            this.IsFetchingData = true;

            this.Id = userdata.Id;
            this.Name = userdata.Name;
            this.Email = userdata.Email;
            await this.geoprocessor.InitializeGeoPosition(this);
        }

        bool isfetchingdata;
        public bool IsFetchingData
        {
            get { return isfetchingdata; }
            set
            {
                isfetchingdata = value;
                OnPropertyChanged();
            }
        }

        Place currentplace;
        public Place CurrentPlace
        {
            get { return currentplace; }
            set
            {
                currentplace = value;
                Util.UserProfileData.CurrentPlace = currentplace;
                //show current place information
                upage.SetUserLocationOnMap(Util.UserProfileData.CurrentPlace);
                ShowUserData();
            }
        }

        async Task ShowUserData()
        {
            await Window.Current.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.CurrentPlaceName = Util.UserProfileData.CurrentPlace.DisplayName;
                this.IsFetchingData = false;
                NotificationManager.ShowAddressAsToast(Util.UserProfileData.CurrentPlace.Address);
            });
        }

        

        string currentplacename;
        public string CurrentPlaceName
        {
            get { return currentplacename; }
            set
            {
                currentplacename = value;
                OnPropertyChanged();
            }
        }

        string name = string.Empty;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        string email = string.Empty;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        public string Id { get; set; }
    
    }

    
}
