using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TravAbout.Data;
using TravAbout.DataModel;
using Windows.UI.Popups;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.IO;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Text.RegularExpressions;
using Windows.Data.Json;

namespace TravAbout.Common
{
    public static class StringExtension
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }

    public class MessageArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public static class JSONValueExtension
    {
        public static string GetStringValue(this IJsonValue jsonvalue)
        {
            if (jsonvalue.ValueType == JsonValueType.Null)
                return string.Empty;

            return jsonvalue.GetString();
        }
    }

    public class Util
    {
        private Util() 
        { 
            
        }

        

        static Util()
        {
            SetUserProfile(new UserData());
        }

        public static UserData UserProfileData { get; private set; }

        public static void SetUserProfile(UserData data)
        {
            UserProfileData = data;
        }

        

        public async static Task<string> GetGeoLocationGMap()
        {
            string fileContent = "";
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Geo/CurrentLocation.html"));
                using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                    fileContent = await sRead.ReadToEndAsync();

               

                return fileContent;
            }
            catch (Exception ex)
            {
                Util.HandleMessage("Location can not be loaded", ex.ToString(), "Error");
                return string.Empty;
            }
        }

        public async static Task<string> GetGeodirectionGMap(string lat, string lng, string mapUrl)
        {
            string fileContent = "";
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(mapUrl));
                using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                    fileContent = await sRead.ReadToEndAsync();

                var fs = fileContent.Replace("#stlat#", lat)
                           .Replace("#stlng#", lng);

                return fs;
            }
            catch (Exception ex)
            {
                Util.HandleMessage("Map Direction can not be loaded", ex.ToString(), "Error");
                return string.Empty;
            }
        }


        public static List<string> Interests {
            get
            {
                return new List<string> { "mall", "movie", "theatre", "pizza", "food", "chicken", "multiplex",
                                   "restaurant", "travel", "cinema", "burger", "shopping",
                                 "park", "monument", "heritage", "govt", "museum", "pub", "nightclub",
                             "art", "art galleries", "gallery", "sport", "nature", "wildlife", "worship", "god", "spiritual",
                "concert", "music", "technology", "software", "IT", "startup", "seminar", "franchise", "comics", 
                "meetup", "business", "charity", "causes", "community", "film", "media", "food", "drink", "event"};
            }
        }

        public static async Task<bool> HasMatch(string text, IEnumerable<string> keywords)
        {
            MatchCollection matches = Regex.Matches(text, @"\p{L}([:']?\p{L})*",
                                        RegexOptions.IgnoreCase);
            
            foreach (Match match in matches)
            {
                var ismatched = keywords.Any(k => match.Value.Contains(k.Trim(), StringComparison.OrdinalIgnoreCase) || match.Value == k.Trim());
                if (ismatched)
                    return true;
            }

            return false;
        }

        public static async Task<string> GetMatch(string text, IEnumerable<string> keywords)
        {
            MatchCollection matches = Regex.Matches(text, @"\p{L}([:']?\p{L})*",
                                        RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var matchedword = keywords.Where(k => match.Value.Contains(k.Trim(), StringComparison.OrdinalIgnoreCase) || match.Value == k.Trim()).FirstOrDefault();
                return matchedword;
            }
            return string.Empty;
        }

        public static async Task<List<InterestOption>> LoadAllInterests()
        {
            var interestoptions = new List<InterestOption>
            {
                new InterestOption { GroupName = "Restaurants", Tag = "pizza, food, chicken, burger, restaurant" },
                new InterestOption { GroupName = "Malls", Tag = "mall, multiplex, shopping" },
                new InterestOption { GroupName = "Theatre", Tag="theatre, theater, cinema, movie" },
                new InterestOption { GroupName = "Park", Tag="park" },
                new InterestOption { GroupName = "Heritage", Tag = "monument, heritage" },
                new InterestOption { GroupName = "Art", Tag = "art, art gallery, art gallaries" },
                new InterestOption { GroupName = "Sports", Tag = "sports, sports club, gym" },
                new InterestOption { GroupName = "Museums", Tag = "museum" },
                new InterestOption { GroupName = "Nature", Tag = "park, nature" },
                new InterestOption { GroupName = "Wildlife", Tag = "park, nature, wildlife, wild" },
                //new InterestOption { GroupName = "Worship", Tag="god, worship, spiritual" },
                new InterestOption { GroupName = "Spiritual", Tag = "god, worship, spiritual" },
                new InterestOption { GroupName = "Pub", Tag="pub, nightclub" },
                new InterestOption { GroupName = "NightClub", Tag="pub, nightclub, salsa, tango, hiphop, drink, dance" },
                new InterestOption { GroupName = "Miscelleneous", Tag="" },
                new InterestOption { GroupName = "Events", Tag=@"event, concert, music, technology, software, IT, startup, seminar, franchise, comics, 
                                                                meetup, art, business, charity, causes, community, film, media, food,
                                                                drink, manager, management, finance, education, bank, marketing, hr, social, training, 
                                                                certification, certificate, concert, band, festival, rock, hackathaon, code, mobile, 
                                                                smac, entrepreneur, manager, meeting, lunch, dinner, design, science, medical, conference, presentation, workshop" },
                new InterestOption { GroupName = "Emergency Service", Tag="medical, police, fire, emergency", IsInterested = true },
            };

            foreach (var interest in Util.UserProfileData.BasicInterests)
            {
                var ios = interestoptions.Where(io => io.Tag.ToLower().Split(",".ToCharArray()).Contains(interest.ToLower()));
                foreach (var item in ios)
                {
                    item.IsInterested = true;
                }
            }

            return interestoptions;
        }

        public static bool IsAuthenticated()
        {
            return FacebookAutheticationType.Instance.Auth.IsAlreadyAuthenticated();
        }

        public async static Task<IUICommand> HandleMessage(string displaymessage, string actualmessage, string title)
        {
            //first log it to server with "actualmessage"
            await LogToServer(actualmessage);
            var dialog = new MessageDialog(displaymessage, title);
            return await dialog.ShowAsync();
        }

        public static async Task LogToServer(string message)
        {
            //call to server
        }
    }
}
