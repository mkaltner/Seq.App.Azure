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
        private string _triggerProperties;
        private Subject<EventData> _messagesToBeSent;
        private HashSet<string> _splitEventProperties;
        private HashSet<string> _splitTagProperties;
        private HashSet<string> _splitTriggerProperties;
        private Dictionary<string, string> _splitStaticProperties;

        [SeqAppSetting(
            DisplayName = "Connection string",
            HelpText = "Event hub connection string (must include the EntityPath).")]
        public string ConnectionString { get; set; }

        [SeqAppSetting(
            DisplayName = "Event properties",
            HelpText = "Event property name(s) (comma seperated for multiple).  If omitted, selecting a signal is highly recommended otherwise every single message logged to Seq will be sent to the Event hub.",
            IsOptional = true,
            InputType = SettingInputType.LongText)]
        public string EventProperties
        {
            get { return _eventProperties; }
            set
            {
                _eventProperties = value;

                _splitEventProperties = new HashSet<string>();
                string[] parts = _eventProperties.Split(',');
                foreach (string part in parts)
                    _splitEventProperties.Add(part.Trim());
            }
        }

        [SeqAppSetting(
            DisplayName = "Static properties",
            HelpText = "Static properties to include in each Event Hub message.  Format: name1=value,name2=value",
            IsOptional = true,
            InputType = SettingInputType.LongText)]
        public string StaticProperties
        {
            get { return _staticProperties; }
            set
            {
                _staticProperties = value;

                _splitStaticProperties = new Dictionary<string, string>();

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
            IsOptional = true,
            InputType = SettingInputType.LongText)]
        public string TagProperties
        {
            get { return _tagProperties; }
            set
            {
                _tagProperties = value;

                _splitTagProperties = new HashSet<string>();

                string[] parts = (_tagProperties ?? string.Empty).Split(',');
                foreach (string part in parts)
                    _splitTagProperties.Add(part.Trim());
            }
        }

        [SeqAppSetting(
            DisplayName = "Trigger properties",
            HelpText = "Trigger property name(s) (comma seperated for multiple). If one of these properties exists on an event then all properties on the event are sent.",
            IsOptional = true,
            InputType = SettingInputType.LongText)]
        public string TriggerProperties
        {
            get { return _triggerProperties; }
            set
            {
                _triggerProperties = value;

                _splitTriggerProperties = new HashSet<string>();

                string[] parts = (_triggerProperties ?? string.Empty).Split(',');
                foreach (string part in parts)
                    _splitTriggerProperties.Add(part.Trim());
            }
        }

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
            try
            {
                Dictionary<string, object> propertyData = null;

                bool includeAllProperties = false;

                if (_splitEventProperties != null && _splitEventProperties.Any())
                {
                    // If EventProperties are defined, grab matching event properties.
                    propertyData = evt.Data.Properties.Where(x => _splitEventProperties.Contains(x.Key))
                        .ToDictionary(x => x.Key, x => GetValue(x.Value));
                }
                else
                {
                    // If no EventProperties, get all properties for this event.
                    // Hopefully the user specified a signal to match because this can/will send everything!
                    includeAllProperties = true;
                }

                if (_splitTriggerProperties != null)
                {
                    includeAllProperties = evt.Data.Properties.Any(x => _splitTriggerProperties.Contains(x.Key));
                }

                if (includeAllProperties)
                {
                    propertyData = evt.Data.Properties.ToDictionary(x => x.Key, x => GetValue(x.Value));
                }

                // If no properties were found (matching or otherwise), return
                if (propertyData == null || !propertyData.Any())
                    return;

                if (_splitTagProperties != null)
                {
                    // Get tag properties
                    foreach (var tagProp in evt.Data.Properties.Where(x => _splitTagProperties.Contains(x.Key)))
                        propertyData[tagProp.Key] = GetValue(tagProp.Value);

                }

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

                // Construct and queue the event data
                var eventData = new EventData(Encoding.UTF8.GetBytes(message));
                _messagesToBeSent.OnNext(eventData);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing event data. " + ex.Message);
                throw;
            }
        }
    }
}
