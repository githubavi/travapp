using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TravAbout.Common;

namespace TravAbout.DataModel
{
    public class InterestOption : BindableBase
    {
        bool isinterested;
        public bool IsInterested
        {
            get { return isinterested; }
            set
            {
                isinterested = value;
                OnPropertyChanged();
            }
        }

        public string GroupName { get; set; }

        public string Tag { get; set; }
    }

    public class QuoteObject
    {
        public string Quote { get; set; }
        public string ImageUrl { get; set; }
    }
}
