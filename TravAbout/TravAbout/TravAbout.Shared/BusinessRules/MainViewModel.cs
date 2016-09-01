using TravAbout.Common;
using System;
using System.Collections.Generic;
using System.Text;
using TravAbout.Data;
using Windows.UI.Popups;
using System.Net.Http;
using System.Threading.Tasks;


namespace TravAbout.BusinessRules
{
    public class MainViewModel : BindableBase
    {
        private bool isNotAuthenticated;

        /// <summary>
        /// Gets or sets the authentication status.
        /// </summary>
        public bool IsNotAuthenticated
        {
            get
            {
                return this.isNotAuthenticated;
            }

            set
            {
                this.isNotAuthenticated = value;
                this.OnPropertyChanged();
            }
        }

        

        public void LogInWithFaceBook(MainPage parentpage)
        {
            Action asyncExecute = async () =>
            {
                this.IsNotAuthenticated = true;
                string token = string.Empty;

                try
                {
                    token = await FacebookAutheticationType.Instance.Auth.AuthenticateAsync(new[] { "email", "user_likes", "user_birthday" });
                    await FacebookAutheticationType.Instance.Identity.SetUserInfo(token);
                    this.IsNotAuthenticated = false;
                    parentpage.NavigateToProfilePage();
                }
                catch (UnauthorizedAccessException e)
                {
                    //invalid token
                    this.IsNotAuthenticated = false;
                    FacebookAutheticationType.Instance.Auth.SignOut();
                    Util.HandleMessage("Authentication failed, try again",e.ToString(),"Info");
                }
                catch (HttpRequestException e)
                {
                    //invalid token
                    this.IsNotAuthenticated = false;
                    FacebookAutheticationType.Instance.Auth.SignOut();
                    Util.HandleMessage("Authentication failed, try again", e.ToString(), "Info");
                    
                }
                catch (Exception e)
                {
                    this.IsNotAuthenticated = false;
                    Util.HandleMessage("Something unusual happened", e.Message, "Error");
                }


            };

            asyncExecute();
        }

    }
}
