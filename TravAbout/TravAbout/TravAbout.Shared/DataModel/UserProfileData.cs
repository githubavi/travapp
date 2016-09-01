using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TravAbout.Common;

namespace TravAbout.DataModel
{
    public class UserData
    {

        public UserData()
        {
            this.BasicInterests = new HashSet<string>();
            this.ProbableVisitedPlaces = new ConcurrentBag<Place>();
            //this.PreferredPlaces = new List<Place>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public string BirthDay { get; set; }

        public string Gender { get; set; }

        public HashSet<string> BasicInterests { get; set; }

        public List<InterestOption> FinalInterests { get; set; }

        public Place CurrentPlace { get; set; }

        public Place StartPlace { get; set; }

        public ConcurrentBag<Place> ProbableVisitedPlaces { get; set; }

        //public List<Place> PreferredPlaces { get; set; }

        public bool HasMoved { get; set; }
    }

    public class Place : BindableBase
    {
        string uniqueId = string.Empty;
        public string UniqueId
        {
            get
            {
                return string.IsNullOrEmpty(uniqueId) ? (uniqueId = string.Concat(this.Latitude, "|", this.Longitude)) : uniqueId;
            }
        }

        public string CityLocation { get; set; }

        public bool HasVisited { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Address { get; set; }

        public string DisplayName { get; set; }

        double rating;
        public double Rating 
        {
            get { return rating; }
            set
            {
                rating = value;
                OnPropertyChanged();
            }
        }

        string currComment;
        public string CurrentComment
        {
            get { return currComment; }
            set
            {
                currComment = value;
                OnPropertyChanged();
            }
        }
    }
}
