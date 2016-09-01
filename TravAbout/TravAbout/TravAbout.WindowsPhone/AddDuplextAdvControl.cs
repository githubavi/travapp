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
            AdDuplex.Universal.Controls.WinPhone.XAML.Tracking.AdDuplexTrackingSDK.StartTracking("118623");
        }

        public static Control GetAdControl()
        {
            var addDuplexControl = new AdDuplex.Universal.Controls.WinPhone.XAML.AdControl();
            addDuplexControl.Name = "add";
            addDuplexControl.AppId = "118623";
            //addDuplexControl.IsTest = true;
            return addDuplexControl;
        }
    }
}
