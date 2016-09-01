using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TravAbout.Auth;
using TravAbout.Common;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace TravAbout.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : BindableBase
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, string position, 
            bool interested, string feedback, string textUpdate, string imageUpdate, string eventUrl = "")
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.Position = position;
            this.Interested = interested;

            this.Feedback = feedback;
            this.TextUpdate = textUpdate;
            this.ImageUpdate = imageUpdate;
            this.EventUrl = eventUrl;
        }

        public string EventUrl { get; set; }

        public string Feedback { get; set; }

        public string TextUpdate { get; set; }

        public string ImageUpdate { get; set; }

        public bool visited;

        public bool Visited
        {
            get { return visited; }
            set
            {
                visited = value;
                OnPropertyChanged();
            }
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public string Position { get; private set; }

        bool intrested;
        public bool Interested
        {
            get { return intrested; }
            set
            {
                intrested = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description, string location)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Location = location;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }

        public string Location { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static void ClearGroups()
        {
            _sampleDataSource.Groups.Clear();
        }
        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            ObservableCollection<SampleDataGroup> groups = new ObservableCollection<SampleDataGroup>();

            foreach (var gr in _sampleDataSource.Groups)
            {
                if (Util.UserProfileData.FinalInterests.Any(i => string.Compare(i.GroupName, gr.Title, StringComparison.OrdinalIgnoreCase) == 0 && i.IsInterested))
                    groups.Add(gr);
            }

            return groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) 
            {
                var grItem = matches.First();
                return grItem;
            }
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/PlacesData.json");

            
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);

                string jsonText = "";
                using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))
                    jsonText = await sRead.ReadToEndAsync();

                JsonObject jsonObject = JsonObject.Parse(jsonText);
                JsonArray jsonArray = jsonObject["Groups"].GetArray();

                foreach (JsonValue groupValue in jsonArray)
                {
                    JsonObject groupObject = groupValue.GetObject();

                    //var grouptitle = groupObject["Title"].GetString();

                   

                    SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                                groupObject["Title"].GetString(),
                                                                groupObject["Subtitle"].GetString(),
                                                                groupObject["ImagePath"].GetString(),
                                                                groupObject["Description"].GetString(),
                                                                groupObject["Location"].GetString());

                    //Add items for "Events" group

                    if (group.UniqueId == "Group-14")
                        //&& (Util.UserProfileData.FinalInterests.Any(i => string.Compare(i.GroupName, group.Title, StringComparison.OrdinalIgnoreCase) == 0 && i.IsInterested))) 
                    {
                        await EventbriteAutheticator.Instance.LoadEventInformation(group);
                        if(group.Items.Count <= 0)
                            continue;
                    }

                    foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                    {
                        JsonObject itemObject = itemValue.GetObject();

                        var feedback = itemObject.ContainsKey("Feedback") ? string.Format(itemObject["Feedback"].GetString(),Environment.NewLine) : string.Empty;
                        var textUpdate = itemObject.ContainsKey("TextUpdate") ? itemObject["TextUpdate"].GetString() : string.Empty;
                        var imageUpdate = itemObject.ContainsKey("ImageUpdateUrl") ? itemObject["ImageUpdateUrl"].GetString() : string.Empty;

                        group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                           itemObject["Title"].GetString(),
                                                           itemObject["Subtitle"].GetString(),
                                                           itemObject["ImagePath"].GetString(),
                                                           itemObject["Description"].GetString(),
                                                           itemObject["Content"].GetString(),
                                                           itemObject["Position"].GetString(),
                                                           itemObject["Interested"].GetBoolean(), feedback, textUpdate, imageUpdate));
                    }

                    this.Groups.Add(group);
                }
            }
            catch (Exception ex)
            {
                Util.HandleMessage("Some error happened in fetching data", ex.ToString(), "Error");
            }
        }
    }
}