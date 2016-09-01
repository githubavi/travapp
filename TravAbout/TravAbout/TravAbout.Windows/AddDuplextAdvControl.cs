using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TravAbout
{
    public sealed partial class AddDuplexControlWin8Phone
    {
        public static void Initialize()
        {
            AdDuplex.Universal.Controls.Win.XAML.Tracking.AdDuplexTrackingSDK.StartTracking("118623");
        }

        public static Control GetAdControl()
        {
            var addDuplexControl = new AdDuplex.Universal.Controls.Win.XAML.AdControl();
            addDuplexControl.Name = "add";
            addDuplexControl.AppId = "118623";
            addDuplexControl.Size = "292x60";
            //addDuplexControl.IsTest = true;
            return addDuplexControl;
        }
    }
}
