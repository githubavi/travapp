using System;
using System.Collections.Generic;
using System.Text;
using TravAbout.Auth;
using TravAbout.Identity;

namespace TravAbout.Data
{
    public class FacebookAutheticationType : IAuthenticationType
    {
        public FacebookAutheticationType()
        {
            this.Name = "FacebookAuthetication";
            this.Identity = new FacebookIdentity();
            this.Auth = new FacebookAuthenticator(new AppCredentialStorage());
        }

        static FacebookAutheticationType()
        {
            instance = new FacebookAutheticationType();
        }

        static FacebookAutheticationType instance;

        public static FacebookAutheticationType Instance {
            get
            {
                return instance ?? new FacebookAutheticationType();
            }
        }

        public string Name { get; set; }

        public Identity.IOnlineIdentity Identity { get; set; }

        public Auth.IAuthHelper Auth { get; set; }
          
    }
}
