// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOnlineIdentity.cs" company="Jeremy Likness">
//   Copyright (c) Jeremy Liknss
// </copyright>
// <summary>
//   The OnlineIdentity interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TravAbout.Identity
{
    using System.Threading.Tasks;
    using TravAbout.DataModel;

    /// <summary>
    /// The OnlineIdentity interface.
    /// </summary>
    public interface IOnlineIdentity
    {
        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="accessToken">
        /// The access Token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task SetUserInfo(string accessToken);
    }
}