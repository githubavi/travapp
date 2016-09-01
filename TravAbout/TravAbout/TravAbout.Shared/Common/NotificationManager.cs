using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TravAbout.DataModel;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace TravAbout.Common
{
    public class NotificationManager
    {
        private NotificationManager() { }

        static ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
        static XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);

        public async static Task SetQuoteNotifications()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText04);
            var tileImage = tileXml.GetElementsByTagName("image")[0] as XmlElement;

            var quotes = LoadQuotes();
            var i = 0;
            TileNotification tileNotification;

            foreach (var quote in quotes)
            {
                tileImage.SetAttribute("src", quote.ImageUrl);

                var tileText = tileXml.GetElementsByTagName("text");
                (tileText[0] as XmlElement).InnerText = quote.Quote;

                tileNotification = new TileNotification(tileXml);
                tileNotification.Tag = i.ToString();
                i++;

                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }
        }

        static List<QuoteObject> LoadQuotes()
        {
            List<QuoteObject> quotes = new List<QuoteObject>();

            quotes.Add(new QuoteObject { Quote = "Not all those who wander are lost. - J.R.R. Tolkien", ImageUrl = "http://media-cache-ak0.pinimg.com/736x/21/74/89/217489f8aafbae9de2000aafc738ea22.jpg" });
            quotes.Add(new QuoteObject { Quote = "The world is a book and those who do not travel read only one page. - Augustine of Hippo", ImageUrl = "http://media-cache-ec0.pinimg.com/236x/08/27/8e/08278e4b855e8dc938daf8f6c3a741e2.jpg" });
            quotes.Add(new QuoteObject { Quote = "The journey of a thousand miles begins with a single step. - Lao Tzu", ImageUrl = "ms-appx:///Assets/TravAboutLogo.scale-100.png" });
            quotes.Add(new QuoteObject { Quote = "Wherever you go becomes a part of you somehow. - Anita Desai", ImageUrl = "http://media-cache-ak0.pinimg.com/736x/fb/30/d0/fb30d04f1a47be9a64b3faac2e19d928.jpg" });
            quotes.Add(new QuoteObject { Quote = "I travel not to go anywhere, but to go. I travel for travel's sake. The great affair is to move.- Robert Louis Stevenson", ImageUrl = "http://media-cache-ak0.pinimg.com/236x/7b/fb/44/7bfb44ecf73fdf7ae091d06663068ce3.jpg" });
            quotes.Add(new QuoteObject { Quote = "The traveler sees what he sees. The tourist sees what he has come to see. - G.K. Chesterton", ImageUrl = "http://media-cache-ak0.pinimg.com/236x/94/54/05/945405f4b29b8bf60b23c385c1191911.jpg" });
            quotes.Add(new QuoteObject { Quote = "Travel brings power and love back into your life- Rumi", ImageUrl = "http://media-cache-ec0.pinimg.com/736x/eb/66/98/eb66980e13deadd4af3c556012d7d8c1.jpg" });
            quotes.Add(new QuoteObject { Quote = "I have found out that there ain't no surer way to find out whether you like people or hate them than to travel with them. - Mark Twain", ImageUrl = "ms-appx:///Assets/TravAboutLogo.scale-100.png" });
            quotes.Add(new QuoteObject { Quote = "It is good to have an end to journey toward; but it is the journey that matters, in the end. - Ernest Hemingway", ImageUrl = "http://media-cache-ak0.pinimg.com/736x/69/fe/c3/69fec363a4ed6af987068bd78159b351.jpg" });

            return quotes;
        }

        public static void ShowAddressAsToast(string address)
        {
            var toastText = toastXml.GetElementsByTagName("text");
            (toastText[0] as XmlElement).InnerText = "Your current Location:" + address;
            var toast = new ToastNotification(toastXml);
            toastNotifier.Show(toast);
        }

        public async static Task HandleErrorMessageWithToaster(string displaymessage, string actualmessage)
        {
            await Util.LogToServer(actualmessage);
            var toastText = toastXml.GetElementsByTagName("text");
            (toastText[0] as XmlElement).InnerText = displaymessage;
            var toast = new ToastNotification(toastXml);
            toastNotifier.Show(toast);
        }

    }
}
