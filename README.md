## Seq.App.Azure

Apps for the [Seq](http://getseq.net) event server. You can find installable versions of these by searching for the [seq-app tag on NuGet](http://www.nuget.org/packages?q=seq-app).

Currently in this repository you'll find:

 * **EventHub** - send Seq events to an Azure Event Hub.

## EventHub
The Seq.App.Azure.EventHub app can be used to send Seq events to an Azure Event Hub.  Once sent, you have many options regarding how you handle your log data.  One such option is Stream Analytics.  You can utilize Azure Stream Analytics to transport your event hub data to various ouputs.  One such useful output is Microsoft's Power BI.  By sending your log data to Power BI, you can visualize your data in countless ways including dashboards, charts, cards, widgets, reports, and many more.

### Stream Analytics and Power BI
The following section walks you through setting up Stream Analytics with a Event Hub input and a Power BI output.

####1. Create Service Bus
![Create Service Bus](docs/images/create_service_bus.png)

####2. Create Event Hub
![Create Event Hub](docs/images/create_event_hub.png)

####3. Configure Shared Access Policy
![Configure Shared Access Policy](docs/images/configure_shared_access_policy.png)

####4. Event Hub Connection String
![Event Hub Connection String](docs/images/event_hub_connection_string.png)

####5. Configure Seq App
![Configure Seq App](docs/images/configure_seq_app.png)

####5. Verify Seq App
![Configure Seq App](docs/images/configure_seq_app.png)
