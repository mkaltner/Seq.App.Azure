using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;

namespace Seq.App.Azure.EventHub
{
    [SeqApp("Azure Event Hub", Description = "Send Seq event properties to an Azure Event Hub.")]
    public class AzureEventHubReactor : BaseReactor, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
            DisplayName = "Connection string",
            HelpText = "Event hub connection string (must include the EntityPath).")]
        public string ConnectionString { get; set; }
        
        [SeqAppSetting(
            DisplayName = "Event properties",
            HelpText = "Event property name(s) (comma seperated for multiple).  If omitted, selecting a signal is highly recommended otherwise every single message logged to Seq will be sent to the Event hub.",
            IsOptional = true)]
        public string EventProperties { get; set; }

        private string EventHubName { get; set; }

        private static Lazy<EventHubClient> _lazyClient;

        private static EventHubClient Client
        {
            get { return _lazyClient.Value; }
        }

        public AzureEventHubReactor()
        {
            _lazyClient =
                new Lazy<EventHubClient>(() =>
                {
                    EventHubClient client;

                    try
                    {
                        client = EventHubClient.CreateFromConnectionString(ConnectionString);
                        EventHubName = client.Path;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error connecting to Event hub.");
                        throw;
                    }

                    return client;
                });
        }

        public void On(Event<LogEventData> evt)
        {
            Dictionary<string, object> propertyData;

            if (!string.IsNullOrEmpty(EventProperties))
            {
                // If EventProperties are defined, grab matching event properties.
                var propertyNames = EventProperties.Split(',');
                propertyData = evt.Data.Properties.Where(x => propertyNames.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => GetValue(x.Value.ToString()));
            }
            else
            {
                // If no EventProperties, get all properties for this event.
                // Hopefully the user specified a signal to match because this can/will send everything!
                propertyData = evt.Data.Properties.ToDictionary(x => x.Key, x => GetValue(x.Value.ToString()));
            }

            // If no properties were found (matching or otherwise), return
            if (!propertyData.Any())
                return;

            // Add a Timestamp to all messages
            propertyData.Add("Timestamp", DateTime.UtcNow);

            var message = JsonConvert.SerializeObject(propertyData);

            Log.Information("Sending {Message} to {EventHubName}", message, EventHubName);

            try
            {
                // Construct and send the event data
                var eventData = new EventData(Encoding.UTF8.GetBytes(message));
                Client.Send(eventData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send message to event hub - {Message}.", ex.Message);
                throw;
            }
        }
    }
}
