using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TravAbout.Common;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Web;

namespace TravAbout.Communication
{
    public class WebSocketCommunicator
    {
        public EventHandler<MessageArgs> WebSocketMessageReceived;

        WebSocketCommunicator()
        {

        }

        static WebSocketCommunicator instance;
        public static WebSocketCommunicator Instance
        {
            get
            {
                return instance ?? (instance = new WebSocketCommunicator());
            }
        }

        public MessageWebSocket CurrentSocket { get; private set; }
       

        public async Task Initialize()
        {
            try
            {

                if (this.CurrentSocket == null)
                {
                    CurrentResponse = string.Empty;

                    this.CurrentSocket = new MessageWebSocket();

                    Uri server = new Uri("ws://echo.websocket.org");

                    // MessageWebSocket supports both utf8 and binary messages.
                    // When utf8 is specified as the messageType, then the developer
                    // promises to only send utf8-encoded data.
                    CurrentSocket.Control.MessageType = SocketMessageType.Utf8;

                    // Set up callbacks
                    CurrentSocket.MessageReceived += MessageReceived;
                    CurrentSocket.Closed += Closed;

                    await CurrentSocket.ConnectAsync(server);
                }
            }
            catch (Exception ex) // For debugging
            {
                //WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                // Add your specific error-handling code here.
                Util.HandleMessage("Some problem happened with setting up Chat server", ex.ToString(), "Sorry");
            }
        }

        public string CurrentResponse { get; set; }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    var resp = reader.ReadString(reader.UnconsumedBufferLength);
                    var handler = WebSocketMessageReceived;
                    if (handler != null)
                        handler(this, new MessageArgs { Message = resp });
                }
            }
            catch (Exception ex) // For debugging
            {
                //WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                // Add your specific error-handling code here.
                Util.HandleMessage("Some problem happened with getting chat response", ex.ToString(), "Sorry");
            }
        }

        private async void Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            // You can add code to log or display the code and reason
            // for the closure (stored in args.Code and args.Reason)
            await NotificationManager.HandleErrorMessageWithToaster("Chat Server disconnected", args.Code + args.Reason);

            // This is invoked on another thread so use Interlocked 
            // to avoid races with the Start/Close/Reset methods.
            MessageWebSocket ws = CurrentSocket;
            MessageWebSocket webSocket = Interlocked.Exchange(ref ws, null);
            if (webSocket != null)
            {
                webSocket.Dispose();
                webSocket = null;
            }
        }

        public async Task SendMessage(string message)
        {
            try
            {
                if (this.CurrentSocket != null)
                {
                    DataWriter messageWriter = new DataWriter(CurrentSocket.OutputStream);
                    // Buffer any data we want to send.
                    messageWriter.WriteString(message);

                    // Send the data as one complete message.
                    await messageWriter.StoreAsync();
                }
            }
            catch (Exception e)
            {
                Util.HandleMessage("Can not send message, some problem with chat server", e.ToString(), "Sorry");
            }
        }

        public void CloseAndDispose()
        {
            if (this.CurrentSocket != null)
            {
                this.CurrentSocket.Close(1000, "Cleanup");
                this.CurrentSocket.Dispose();
            }
        }
}
}
