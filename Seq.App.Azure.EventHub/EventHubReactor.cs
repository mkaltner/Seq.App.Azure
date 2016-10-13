using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Seq.App.Azure.EventHub
{
    [SeqApp("Azure Event Hub", Description = "Send Seq event properties to an Azure Event Hub.")]
    public class AzureEventHubReactor : BaseReactor, ISubscribeTo<LogEventData>
    {
        private string _eventProperties;
        private string _staticProperties;
        private string _tagProperties;
        private Subject<EventData> _messagesToBeSent;
        private HashSet<string> _splitEventProperties;
        private HashSet<string> _splitTagProperties;
        private Dictionary<string, string> _splitStaticProperties;

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
            get { return _eventProperties; }
            set
            {
                _eventProperties = value;

                if (string.IsNullOrEmpty(_eventProperties))
                    _splitEventProperties = null;
                else
                {
                    _splitEventProperties = new HashSet<string>();
                    string[] parts = _eventProperties.Split(',');
                    foreach (string part in parts)
                        _splitEventProperties.Add(part.Trim());
                }
            }
        }

        [SeqAppSetting(
            DisplayName = "Static properties",
            HelpText = "Static properties to include in each Event Hub message.  Format: name1=value,name2=value",
            IsOptional = true)]
        public string StaticProperties
        {
            get { return _staticProperties; }
            set
            {
                _staticProperties = value;

                string[] parts = (_staticProperties ?? string.Empty).Split(',');
                foreach (string part in parts)
                {
                    string[] pair = part.Split('=');
                    _splitStaticProperties.Add(pair[0].Trim(), pair[1].Trim());
                }
            }
        }

        [SeqAppSetting(
            DisplayName = "Log sent messages",
            HelpText = "If checked, will log sent messages as Information.",
            IsOptional = true)]
        public bool LogMessages { get; set; }

        [SeqAppSetting(
            DisplayName = "Tag properties",
            HelpText = "Tag property name(s) (comma seperated for multiple). These are added to messages that are sent, but if an event only has the tag properties (and none of the event ones) then nothing will be sent.",
            IsOptional = true)]
        public string TagProperties
        {
            get { return _tagProperties; }
            set
            {
                _tagProperties = value;

                string[] parts = (_tagProperties ?? string.Empty).Split(',');
                foreach (string part in parts)
                    _splitTagProperties.Add(part.Trim());
            }
        }

        private static Lazy<EventHubClient> _lazyClient;

        private static EventHubClient Client
        {
            get { return _lazyClient.Value; }
        }

        public AzureEventHubReactor()
        {
            _splitStaticProperties = new Dictionary<string, string>();
            _splitTagProperties = new HashSet<string>();

            _lazyClient =
                new Lazy<EventHubClient>(() =>
                {
                    EventHubClient client;

                    try
                    {
                        client = EventHubClient.CreateFromConnectionString(ConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error connecting to Event hub");
                        throw;
                    }

                    return client;
                });

            _messagesToBeSent = new Subject<EventData>();

            _messagesToBeSent
                .Buffer(TimeSpan.FromSeconds(3), 100)
                .Subscribe(async eventData =>
                    {
                        if (eventData.Any())
                        {
                            try
                            {
                                if (LogMessages)
                                    Log.Information("Sending {MessageCount} message(s)", eventData.Count);

                                await Client.SendBatchAsync(eventData);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to send message to event hub - {Message}.", ex.Message);
                            }
                        }
                    });
        }

        public void On(Event<LogEventData> evt)
        {
            Dictionary<string, object> propertyData;

            if (_splitEventProperties != null)
            {
                // If EventProperties are defined, grab matching event properties.
                propertyData = evt.Data.Properties.Where(x => _splitEventProperties.Contains(x.Key))
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

            // Get tag properties
            foreach (var tagProp in evt.Data.Properties.Where(x => _splitTagProperties.Contains(x.Key)))
                propertyData[tagProp.Key] = GetValue(tagProp.Value);

            try
            {
                // Parse and add static properties
                if (_splitStaticProperties != null)
                {
                    foreach (var kvp in _splitStaticProperties)
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
                Log.Information("Queuing {Message}", message);

            // Construct and send the event data
            var eventData = new EventData(Encoding.UTF8.GetBytes(message));
            _messagesToBeSent.OnNext(eventData);
        }
    }
}
