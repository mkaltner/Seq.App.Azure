# Seq.App.Azure

Apps for the [Seq](http://getseq.net) event server. You can find installable versions of these by searching for the [seq-app tag on NuGet](http://www.nuget.org/packages?q=seq-app).

Currently in this repository you'll find:

 * **EventHub** - send Seq events to an Azure Event Hub.

## EventHub
The Seq.App.Azure.EventHub app can be used to send Seq events to an Azure Event Hub.  Once sent, you have many options regarding how you handle your log data.  One such option is Stream Analytics.  You can utilize Azure Stream Analytics to transport your event hub data to various ouputs.  One such useful output is Microsoft's Power BI.  By sending your log data to Power BI, you can visualize your data in countless ways including dashboards, charts, cards, widgets, reports, and many more.

## Stream Analytics and Power BI
The following section walks you through setting up Stream Analytics with a Event Hub input and a Power BI output.

##1. Create Service Bus
![Create Service Bus](docs/images/create_service_bus.png)

##2. Event Hub
###2a. Create Event Hub
![Create Event Hub](docs/images/create_event_hub.png)

###2b. Configure Shared Access Policy
![Configure Shared Access Policy](docs/images/configure_shared_access_policy.png)

###2c. Connection String
![Event Hub Connection String](docs/images/event_hub_connection_string.png)

##3. Configure Seq App
![Configure Seq App](docs/images/configure_seq_app.png)

Verify that the app is sending events to Azure:
![Verify Seq App](docs/images/verify_app.png)

##4. Stream Analytics
###4a. Create Job
![Create Stream Analytics Job](docs/images/create_stream_analytics_job.png)

###4b. Add Input
![Add Input - Step 1](docs/images/add_input_job_1.png)
![Add Input - Step 2](docs/images/add_input_job_2.png)
![Add Input - Step 3](docs/images/add_input_job_3.png)
![Add Input - Step 4](docs/images/add_input_job_4.png)

###4c. Add Output
![Add Output - Step 1](docs/images/add_output_1.png)
![Add Output - Step 2](docs/images/add_output_2.png)
![Add Output - Step 3](docs/images/add_output_3.png)

###4d. Create Query
![Create Query](docs/images/create_query.png)

##5. Power BI
###5a. Dataset
![Dataset](docs/images/pbi_dataset.png)

Click on the auto created data set to create a report:
![Create Report](docs/images/pbi_create_report.png)

Click the thumbtac to save the report and pin it to your dashboard:
![Pin to Dashboard](docs/images/pbi_pin_to_dashboard.png)

###5b. Dashboard
![Dashboard](docs/images/pbi_dashboard.png)
