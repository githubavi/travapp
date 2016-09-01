namespace TravAbout.Data
{
    using Auth;
    using Identity;

    /// <summary>
    /// Type of authentication to use 
    /// </summary>
    public interface IAuthenticationType
    {
        /// <summary>
        /// Gets or sets the name of the authenticator 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the identity service
        /// </summary>
        IOnlineIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets the authentication service
        /// </summary>
        IAuthHelper Auth { get; set; }
    }
}