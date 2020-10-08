# Contents
* High level proposal of a system design with diagrams

* Lambda for API gateway proxy

* Lambda to process SQS message and send emails

# An emailer - API Gateway using lambda proxy
Orders from clients need to email certain parties to notify them of requests for action and/or approvals at certain intervals. This service provides a highly performant, secure and cost effective system while allowing easy tracing of email that were sent out.

## Currently
These emails are sent via third party libraries integrated into the same Web Application for handling orders.

The logic is tightly coupled with data structures and workflows from the application, with small changes sometimes breaking entire builds or breaking the emailer. A common operational issue is that certain parties claim they did not receive the emails they were expecting. This translates into risks for the business.

A few attempts to improve the emailing functions in the past has not decreased the amount of complaints. This has prompted management to review this system and request for a solution.

# Overview of proposal

## Idea from the team
From the business' point of view, the requirement is to improve the reliability of emails in the main product. From the development point of view, having to fix bugs and rebuild the application adds unneccessary delays to the development cycle of the application.

An idea was given, by the architect team, to split off the emailing sub-system into its own project and make use of AWS services.

## Discussion
We identified that the new system needs to immediately address the following : allow customer support teams to easily respond to cases of emails not being received; run as a separate service from the application; and handle the same volume of email requests as the current emailer system.

Although not stated in any communications, it is also expected that concerns of security and the costs of running this system will determine if it gets all the approvals needed to push it for production use.

To describe the proposed system, pictures have been drawn up and submitted with the proposal.

## Diagrams
Diagram for AWS resources

![AWS resources](https://github.com/FadeDragon/Resume2020/blob/master/Email%20Service%20-%20API/EmailService%20-%20Architecture%20Diagram.svg)

Diagram for request processing in EmailService.API

![API](https://github.com/FadeDragon/Resume2020/blob/master/Email%20Service%20-%20API/EmailService%20-%20API%20Diagram.svg)

Flow of logic for EmailService.Service

![Flow](https://github.com/FadeDragon/Resume2020/blob/master/Email%20Service%20-%20API/EmailService%20-%20Processor%20Flow%20Diagram.svg)

* Lambdas are used to keep up with traffic volume and achieve cost-savings when usage is low.
* Private VPC to keep the database safe from the public internet.
  * Lambda receives message from the SQS via VPC endpoint.
  * Lambda runs in the VPC to access the database.
  * Execution time is less of a concern as most emails are not seen as time sensitive.
* The email providers functionality allows the service to use alternative providers in case of availability issues identified during the 'Save email status' step.
  * Original project has support for AWS SES and SendGrid.
* Usage of SQS gives potential for future expansion to couple the email service with other lambda-based microservices.
* Email templates is an existing feature of the current emailer so as to support different products for different markets and clients.
* Rendering data into templates via a templating engine enables emails to provide details without customers having to log in.
* 'Save email status' provides easy tracing of emails that were sent (or not sent out) and ease of tracing.

## Usage proposal.

### POST - /send
Accepts a request to create an email, should contain the following

* Application Id - Identify which product and brand is sending this request.
* Notification Type - The email template to use.
* Country Code - Determines language and market.
* FromEmail - Address to display as the sender of the email.
* Attributes - To be used in future to record the name or ID of the machine that created this request.
* RequestData - Variable templating engine will take these values and output into text.
* RecipientList - List of addresses under 'To', 'CC' and 'BCC'

Should validate the following

* Application Id, Notification Type, FromEmail
  * Required.
* FromEmail
  * Valid email
* RecipientList
  * Has at least one recipient

Upon successful validation, insert the request into [notification_request] table with a new unique generated ID
1. Request is recorded into database with a Globally Unique ID (UUID)
1. Put a message into SQS queue with GuID in payload
1. Return the GuID in the response

### GET - /check/{id:guid}
Accepts a GuID created in another /send call and looks for information of the email request. Should return the following

* Status - Currently if it is queued, has error or completed
* Started - DateTime of SQS message being picked up
* Completed - DateTime of successful dispatch to email provider

## Next steps.

Coordinate with development and support teams to migrate old data into the new service and deprecate usages of previous emailer function.

A timeframe of a few weeks (at least one month, to be safe) will be expected, as certain clients in some markets have plenty of Notification Types that need to be migrated to this system.
