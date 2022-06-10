# Seq.App.Azure

Apps for the [Seq](http://getseq.net) event server. You can find installable versions of these by searching for the [seq-app tag on NuGet](http://www.nuget.org/packages?q=seq-app).

Currently in this repository you'll find:

 * **EventHub** - send Seq events to an Azure Event Hub.

## EventHub
The Seq.App.Azure.EventHub app can be used to send Seq events to an Azure Event Hub.  Once sent, you have many options regarding how you handle your log data.  One such option is Stream Analytics.  You can utilize Azure Stream Analytics to transport your event hub data to various ouputs like Microsoft's Power BI.  By sending your log data to Power BI, you can visualize your data in countless ways including dashboards, charts, cards, widgets, reports, and many more.

## Stream Analytics and Power BI
The following section walks you through setting up Stream Analytics with a Event Hub input and a Power BI output.

## 1. Create a Service Bus
If you don't already have one, you'll need to create an Azure Service bus to house the Event Hub.

![Create Service Bus](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/create_service_bus.png)

## 2. Event Hub
### 2a. Create Event Hub
Now you can create your Event Hub.  Name it whatever you like and is available.  It should be something meaningful to your project/application.

![Create Event Hub](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/create_event_hub.png)

### 2b. Configure a Shared Access Policy
Once the Event Hub is created, go over to the Configure tab and create a Share Access Policy.  This allows you to generate a connection string and send events to the hub.  You'll just need Send permissions.

![Configure Shared Access Policy](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/configure_shared_access_policy.png)

### 2c. Connection String
Now go back to the Event Hub Dashboard and get the connection string by clicking View Connection String then copy it to your clipboard.

![Event Hub Connection String](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/event_hub_connection_string.png)

## 3. Configure Seq App
Next you'll need to configure the EventHub app in Seq.  Go to Settings->Apps and click Start New Instance under Azure Event Hub and set the properties similar to the following:

![Configure Seq App](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/configure_seq_app.png)

Once created, go back into the app instance and click the View events raised by this instance link to verify that it's working properly.

![Verify Seq App](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/verify_app.png)

## 4. Stream Analytics
### 4a. Create Job
Now that you've verified that Seq is sending events to your Event Hub it's time to create a Stream Analytics job.

![Create Stream Analytics Job](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/create_stream_analytics_job.png)

### 4b. Add Input
Create an input for the Stream Analytics job.  The input will be the Event Hub you just created and are now sending Seq events to.

![Add Input - Step 1](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_input_job_1.png)

![Add Input - Step 2](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_input_job_2.png)

![Add Input - Step 3](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_input_job_3.png)

![Add Input - Step 4](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_input_job_4.png)

### 4c. Add Output
Add an output.  For this example you'll create a Power BI output like so:

![Add Output - Step 1](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_output_1.png)

![Add Output - Step 2](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_output_2.png)

![Add Output - Step 3](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/add_output_3.png)

### 4d. Create Query
Finally, create a Stream Analytics query to transfer data from the event hub to the Power BI output.

![Create Query](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/create_query.png)

## 5. Power BI
### 5a. Dataset
Once Stream Analytics starts sending data to Power BI, the dataset will be automatically created for you as you can see below:

![Dataset](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/pbi_dataset.png)

Click on the auto created data set to create a report:

![Create Report](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/pbi_create_report.png)

Click the thumbtack to save the report and pin it to your dashboard:

![Pin to Dashboard](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/pbi_pin_to_dashboard.png)

### 5b. Dashboard
Now you can view your report on the dashboard!

![Dashboard](https://raw.githubusercontent.com/mkaltner/Seq.App.Azure/master/docs/images/pbi_dashboard.png)
