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
        private string eventProperties;
        private string staticProperties;
        private HashSet<string> splitEventProperties;
        private Dictionary<string, string> splitStaticProperties;

        [SeqAppSetting(
            DisplayName = "Connection string",
            HelpText = "Event hub connection string (must include the EntityPath).")]
        public string ConnectionString { get; set; }

        [SeqAppSetting(
            DisplayName = "Event properties",
            HelpText = "Event property name(s) (comma seperated for multiple).  If omitted, selecting a signal is highly recommended otherwise every single message logged to Seq will be sent to the Event hub.",
            IsOptional = true)]
        public string EventProperties
        {
            get { return this.eventProperties; }
            set
            {
                this.eventProperties = value;

                if (string.IsNullOrEmpty(this.eventProperties))
                    this.splitEventProperties = null;
                else
                {
                    string[] parts = this.eventProperties.Split(',');
                    foreach (string part in parts)
                        this.splitEventProperties.Add(part.Trim());
                }
            }
        }

        [SeqAppSetting(
            DisplayName = "Static properties",
            HelpText = "Static properties to include in each Event Hub message.  Format: name1=value,name2=value",
            IsOptional = true)]
        public string StaticProperties
        {
            get { return this.staticProperties; }
            set
            {
                this.staticProperties = value;

                string[] parts = (this.staticProperties ?? string.Empty).Split(',');
                foreach (string part in parts)
                {
                    string[] pair = part.Split('=');
                    this.splitStaticProperties.Add(pair[0].Trim(), pair[1].Trim());
                }
            }
        }

        [SeqAppSetting(
            DisplayName = "Log sent messages",
            HelpText = "If checked, will log sent messages as Information.",
            IsOptional = true)]
        public bool LogMessages { get; set; }

        private string EventHubName { get; set; }

        private static Lazy<EventHubClient> _lazyClient;

        private static EventHubClient Client
        {
            get { return _lazyClient.Value; }
        }

        public AzureEventHubReactor()
        {
            this.splitStaticProperties = new Dictionary<string, string>();

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

            if (this.splitEventProperties != null)
            {
                // If EventProperties are defined, grab matching event properties.
                propertyData = evt.Data.Properties.Where(x => this.splitEventProperties.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => GetValue(x.Value));
            }
            else
            {
                // If no EventProperties, get all properties for this event.
                // Hopefully the user specified a signal to match because this can/will send everything!
                propertyData = evt.Data.Properties.ToDictionary(x => x.Key, x => GetValue(x.Value));
            }

            // If no properties were found (matching or otherwise), return
            if (!propertyData.Any())
                return;

            try
            {
                // Parse and add static properties
                if (this.splitStaticProperties != null)
                {
                    foreach (var kvp in this.splitStaticProperties)
                        propertyData[kvp.Key] = GetValue(kvp.Value);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing static properties.");
                throw;
            }

            // Add a Timestamp to all messages
            propertyData.Add("Timestamp", evt.TimestampUtc);

            var message = JsonConvert.SerializeObject(propertyData);

            if (LogMessages)
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
