namespace TravAbout.Auth
{
    using System.Threading.Tasks;

    /// <summary>
    /// The Authentication Helper interface.
    /// </summary>
    public interface IAuthHelper 
    {
        string Name { get; }

        Task<string> AuthenticateAsync(string[] claims);

        bool IsAlreadyAuthenticated();

        void SignOut();
    }
}