using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TravAbout.Data;
using TravAbout.DataModel;
using Windows.Data.Json;
using System.Linq;
using TravAbout.Common;

namespace TravAbout.Identity
{
    /// <summary>
    /// The facebook identity.
    /// </summary>
    public class FacebookIdentity : IOnlineIdentity
    {
        /// <summary>
        /// The facebook identity url.
        /// </summary>
        private const string FacebookIdentityUrl = "https://graph.facebook.com/v2.1/me"; 
        
        //profile: /me, scope:user_about_me
        //birthday: /me, scope:user_birthday
        // Likes: me/likes, scope:user_likes
        // Interests: me/interests, scope:user_interests
        //Activities: me/activities, scope:user_activities



        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookIdentity"/> class.
        /// </summary>
        public FacebookIdentity()
        {
            
        }
        

        /// <summary>
        /// The get email.
        /// </summary>
        /// <param name="accessToken">
        /// The access token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task SetUserInfo(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);

            try
            {

                var resultme = await client.GetStringAsync(FacebookIdentityUrl);
                var resultlikes = await client.GetStringAsync("https://graph.facebook.com/v2.1/me/likes");
                //var result3 = await client.GetStringAsync("https://graph.facebook.com/v2.1/me/activities");

                UserData userdata = new UserData();

                var profileInformation = JsonObject.Parse(resultme);

                userdata.Id = profileInformation["id"].GetString();
                userdata.Name = profileInformation["name"].GetString();
                userdata.BirthDay = profileInformation["birthday"].GetString();
                userdata.Gender = profileInformation["gender"].GetString();
                userdata.Email = profileInformation["email"].GetString();

                var profilelikes = JsonObject.Parse(resultlikes);
                var likes = profilelikes["data"].GetArray();
                var interests = Util.Interests;

                foreach (JsonValue like in likes)
                {
                    JsonObject category = like.GetObject();
                    var category_name = category["name"].GetString();

                    //var intrst = interests.Where(i => category_name.Contains(i, StringComparison.OrdinalIgnoreCase) || category_name == i).FirstOrDefault();

                    var intrst = await Util.GetMatch(category_name, interests);

                    if (!string.IsNullOrEmpty(intrst))
                        userdata.BasicInterests.Add(intrst);

                    if (category.ContainsKey("category_list"))
                    {
                        var categorylist = category["category_list"];

                        var categorylistarray = categorylist.GetArray();

                        foreach (JsonValue catlistitem in categorylistarray)
                        {
                            var catlistitemname = catlistitem.GetObject()["name"].GetString();
                            //var intrst2 = interests.Where(i => catlistitemname.Contains(i, StringComparison.OrdinalIgnoreCase) || catlistitemname == i).FirstOrDefault();
                            var intrst2 = await Util.GetMatch(catlistitemname, interests);

                            if (!string.IsNullOrEmpty(intrst2))
                                userdata.BasicInterests.Add(intrst2);
                        }
                    }
                }

                Util.SetUserProfile(userdata);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}