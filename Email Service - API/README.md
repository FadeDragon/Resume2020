# Contents
- Diagrams and a high level proposal of the system design

- Lambda for API gateway used

- Lambda for SQS message processing

# API - A new Lambda-based emailer
Orders from clients need to email certain parties to notify them to approve and/or take action at certain intervals. This service provides a highly performant, scalable and cost effective system while allowing easy tracing of email sending statuses.

## Currently

These emails are sent via calling third party libraries within the same Web Application added years ago. It has tight coupling with data structures and workflows in the application, with small changes sometimes breaking entire builds or breaking the emailer.

A common issue exists with certain parties claiming that they received no email or the email was never sent to their system and it is time consuming to trace these changes and prove when these emails were sent.

## Overview of proposal
From the business' point of view, the requirement is to improve the reliability of emails in the main product. From the development point of view, having to fix bugs and rebuild the application adds unneccessary delays to the development cycle.

Hence the idea to split off the emailing sub-system into its own project while addressing these concerns was started.

### Diagrams
Diagram for AWS resources

![AWS resources](https://github.com/FadeDragon/Resume2020/blob/master/Email%20Service%20-%20API/EmailService%20-%20Architecture%20Diagram.svg)

Diagram for API

![API](https://github.com/FadeDragon/Resume2020/blob/master/Email%20Service%20-%20API/EmailService%20-%20API%20Diagram.svg)

### Usage proposal.

POST - /send
Accepts a request to create an email, should contain the following

Application Id - Identify which product and brand is sending this request.
Notification Type - The email template to use.
Country Code - Determines language and market.
FromEmail - Address to display as the sender of the email.
Attributes - To be used in future to record the name or ID of the machine that created this request.
RequestData - Variable templating engine will take these values and output into text.
RecipientList - List of addresses under 'To', 'CC' and 'BCC'

Should validate the following

Application Id, Notification Type, FromEmail - required.
FromEmail - Is a valid email
RecipientList - Has at least one recipient

If the request is valid
- Request is recorded into database with a unique generated ID
- Put a message into SQS queue with ID in payload
- Return the ID in the response

GET - /check/{id:guid}
Accepts the ID created in a /send call and if ID is found in database, should return the following

Status - Currently if it is queued, has error or completed
Started - DateTime of SQS message being picked up
Completed - DateTime of successful dispatch to email provider

## Next steps.

Coordinate with development and support teams to migrate old data into the new service and retire the previous emailer function.


