namespace MainBot
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Dialogs;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;
    using Properties;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        // TODO: Replace with registration Web API so trusted bots can publish their commands and forwarding URLs dynamically.

        // Navigation commands and forwarding URLs for main and all sub bots.
        private static readonly Dictionary<string, string> NavCommandForwardingUrls = new Dictionary<string, string>()
        {
            { Resources.MainBot_Menu, ConfigurationManager.AppSettings[Resources.MainBot_Url] },
            { Resources.SubBot1_Menu, ConfigurationManager.AppSettings[Resources.SubBot1_Url] },
            { Resources.SubBot2_Menu, ConfigurationManager.AppSettings[Resources.SubBot2_Url] }
        };

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            string conversationID = activity.Conversation.Id;

            StateClient stateClient = activity.GetStateClient();
            BotData conversationData = await stateClient.BotState.GetConversationDataAsync(activity.ChannelId, activity.Conversation.Id);

            // The URL of the currently active bot, either main or one of the sub bots.
            var forwardingUrl = conversationData.GetProperty<string>(Resources.MainBot_Forwarding_Url_Key);

            // If the activity is a message, check to see if it's a navgation command.
            if (activity.Type == ActivityTypes.Message)
            {
                var message = activity as IMessageActivity;

                if (message != null && !string.IsNullOrWhiteSpace(message.Text))
                {
                    var commandUrl = (from cmd in NavCommandForwardingUrls
                                   where message.Text.Equals(cmd.Key, StringComparison.InvariantCultureIgnoreCase)
                                   select cmd.Value).FirstOrDefault();

                    if (commandUrl != null && !string.IsNullOrWhiteSpace(commandUrl))
                    {
                        // If the forwarding url has changed, save it to conversation state.
                        if (!commandUrl.Equals(forwardingUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            forwardingUrl = commandUrl;

                            conversationData.SetProperty<string>(Resources.MainBot_Forwarding_Url_Key, forwardingUrl);
                            await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                        }
                    }
                }
            }

            // If the forwarding URL is the main bot URL, send the message to the dialog stack.
            if ((forwardingUrl == null) || this.Request.RequestUri.ToString().Equals(forwardingUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new RootDialog());
                }
                else
                {
                    this.HandleSystemMessage(activity);
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            else
            {
                // Else forward the message to the sub bot at the forwarding URL.
                var client = new HttpClient();

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(forwardingUrl),
                    Method = HttpMethod.Post
                };

                // Copy the message headers to the forwarding request.
                foreach (var header in this.Request.Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                // Change the host for the request to be the forwarding URL.
                request.Headers.Host = request.RequestUri.Host;

                var json = JsonConvert.SerializeObject(activity);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                request.Content = content;

                return await client.SendAsync(request);
            }
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}