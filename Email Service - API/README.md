# Contents
- API sequence diagram and a high level proposal of the system design

- Lambda for API gateway used

- Lambda for SQS message processing

# API - A Lambda based emailer
Orders from clients need to send emails to notify certain parties to approve and/or take action at certain intervals. This service provides a highly performant, scalable and cost effective system while allowing easy tracing of email sending statuses.

## Currently

These emails are sent via calling third party libraries within the same Web Application added years ago.

This is causing tight coupling within data structures and workflows, with small changes sometimes breaking entire builds and making it is time consuming to trace these changes and prove when these emails were sent.

A common issue exists with certain parties claiming that they received no email or the email was never sent to their system.

## Overview of proposal
From the business' point of view, the requirement is to improve the reliability of emails in the main product. From the development point of view, having to fix bugs and rebuild the application adds unneccessary delays to the development cycle.

Hence the idea to split off the emailing sub-system into its own project while addressing these concerns was started.

### steps
Creation of API sequence and AWS resource diagrams along with a usage proposal of a new email service.

Coordinate with development and support teams to migrate old data into the new service and retire the previous emailer function.


