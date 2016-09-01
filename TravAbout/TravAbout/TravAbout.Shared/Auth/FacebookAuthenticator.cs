using System;
using System.Security;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using TravAbout.Data;

namespace TravAbout.Auth
{
    

    /// <summary>
    /// The facebook authenticator.
    /// </summary>
    public class FacebookAuthenticator : IAuthHelper 
    {
        /// <summary>
        /// The face book id.
        /// </summary>
        private const string FacebookAppId = "379794725511184"; // App ID from https://developers.facebook.com/apps 

        /// <summary>
        /// The facebook redirect uri.
        /// </summary>
        private const string FacebookRedirectUri = "https://www.facebook.com/connect/login_success.html";

        /// <summary>
        /// The face book authentication.
        /// </summary>
        private const string FaceBookAuth =
            "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope={2}&response_type=token";
            //"https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}&response_type=token&client_secret={3}&grant_type=client_credentials";
        /// <summary>
        /// The data storage.
        /// </summary>
        private readonly ICredentialStorage dataStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthenticator"/> class.
        /// </summary>
        /// <param name="dataStorage">
        /// The data storage.
        /// </param>
        public FacebookAuthenticator(ICredentialStorage dataStorage)
        {
            this.dataStorage = dataStorage;
        }
        
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return "Facebook";
            }
        }

        public bool IsAlreadyAuthenticated()
        {   
            string savedToken;
            if (this.dataStorage.TryRestore(this.Name, out savedToken))
            {
                return true;
            }
            return false;
        }

        public void SignOut()
        {
            this.dataStorage.Signout(this.Name);
        }

        public async Task<string> AuthenticateAsync(string[] claims)
        {
            string savedToken;
            if (this.dataStorage.TryRestore(this.Name, out savedToken))
            {
                return savedToken;
            }

            var startUri = new Uri(string.Format(FaceBookAuth, FacebookAppId, FacebookRedirectUri, string.Join(",", claims)), UriKind.Absolute);
            var endUri = new Uri(FacebookRedirectUri, UriKind.Absolute);


            WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(
                                                    WebAuthenticationOptions.None,
                                                    startUri,
                                                    endUri);

            if (result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var data = result.ResponseData.Substring(result.ResponseData.IndexOf('#'));
                var values = data.Split('&');
                var token = values[0].Split('=')[1];
                var expirationSeconds = values[1].Split('=')[1];
                var expiration = //DateTime.UtcNow.AddSeconds(12 * 60 * 60); //12 h
                    DateTime.UtcNow.AddSeconds(int.Parse(expirationSeconds));
                this.dataStorage.Save(this.Name, expiration, token);
                return token;
            }


            throw new SecurityException(string.Format("Authentication failed: {0}", result.ResponseErrorDetail));
        }
    }
}
