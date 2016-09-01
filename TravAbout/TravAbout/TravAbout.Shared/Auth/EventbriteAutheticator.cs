using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TravAbout.Common;
using TravAbout.Data;
using Windows.Data.Json;
using System.Linq;
using System.Text.RegularExpressions;

namespace TravAbout.Auth
{
    public class EventbriteAutheticator
    {
        HttpClient client;

        private EventbriteAutheticator()
        {
            
        }

        static EventbriteAutheticator instance;
        public static EventbriteAutheticator Instance
        {
            get
            {
                return instance ?? new EventbriteAutheticator();
            }
        }

        private const string EventBriteAuthToken = "EG3JEBLGORWM3FXNW6B7";

        private string EventQueryBaseUrl = "https://www.eventbriteapi.com/v3/events/search/?token={0}&venue.city={1}&date_modified.keyword=this_week";

        public async Task LoadEventInformation(SampleDataGroup eventGroup)
        {
            try
            {
                client = new HttpClient();
                var jsonText = await client.GetStringAsync(string.Format(EventQueryBaseUrl, EventBriteAuthToken, "Bangalore")); //Util.UserProfileData.StartPlace.Name
                JsonObject jsonObject = JsonObject.Parse(jsonText);

                if (jsonObject.ContainsKey("events"))
                {
                    var eventtags = eventGroup.Description.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    var events = jsonObject["events"].GetArray();
                    var i = 0;

                    foreach (var evtvalue in events)
                    {
                        if (i == 8)
                            break;

                        var evt = evtvalue.GetObject();
                        var content = evt["description"].ValueType == JsonValueType.Null  ? string.Empty : evt["description"].GetObject()["text"].GetStringValue();

                        if (evt["category"].ValueType == JsonValueType.Null)
                        {
                            if (!await Util.HasMatch(content, eventtags))
                                continue;
                        }
                        else
                        {
                            var categoryname = evt["category"].GetObject()["name"].GetStringValue();

                            if (!await Util.HasMatch(categoryname, eventtags))
                            {
                                if (!await Util.HasMatch(content, eventtags))
                                    continue;

                                continue;
                            }
                            
                        }
                        
                        i++;
                        var uniqueId = eventGroup.UniqueId + i;
                        var title = evt["name"].GetObject()["text"].GetStringValue();
                        var imagepath = evt["logo_url"].ValueType == JsonValueType.Null ? string.Empty : evt["logo_url"].GetStringValue();
                        string address = string.Empty;
                        string position = string.Empty;

                        if (!(evt["venue"].ValueType == JsonValueType.Null || evt["venue"].GetObject()["address"].ValueType == JsonValueType.Null))
                        {
                            var evtobject = evt["venue"].GetObject();

                            var addobj = evtobject["address"].GetObject();

                            address = evtobject.GetObject()["name"].GetStringValue();

                            if (addobj.ContainsKey("address_1"))
                                address = address + ", " + addobj["address_1"].GetStringValue();
                            if (addobj.ContainsKey("address_2"))
                                address = address + ", " + addobj["address_2"].GetStringValue();
                            if (addobj.ContainsKey("city"))
                                address = address + ", " + addobj["city"].GetStringValue();
                            if (addobj.ContainsKey("region"))
                                address = address + ", " + addobj["region"].GetStringValue();
                            if (addobj.ContainsKey("postal_code"))
                                address = address + ", " + addobj["postal_code"].GetStringValue();

                            if (evtobject.ContainsKey("latitude") && evtobject.ContainsKey("longitude"))
                                position = evtobject["latitude"].GetStringValue() + "," + evtobject["longitude"].GetStringValue();
                        }
                        

                        var groupItem = new SampleDataItem(uniqueId, title, string.Empty, imagepath, address, content,
                            position, false, string.Empty, string.Empty, string.Empty, evt["url"].ValueType == JsonValueType.Null ? string.Empty : evt["url"].GetStringValue());
                        eventGroup.Items.Add(groupItem);
                    }
                }//end if
                client.Dispose();
            } //end try
            catch (Exception ex)
            {
                Util.HandleMessage(ex.ToString(), ex.ToString(), "error"); //for testing
                //log error but continue with loading other data
                //Util.LogToServer(ex.ToString());
            }
        }
    }
}
