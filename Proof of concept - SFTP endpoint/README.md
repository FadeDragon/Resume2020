# Contents
Cognito user pool diagram and related scripts
Lambda for API gateway used as the custom identity provider

*Note that the above will be used by an instance in AWS transfer family*

# Proof of concept - Background
Orders from clients are usually done via a web interface. An FTP service allows clients to directly upload their orders without going through the web interface.

## Currently

Existing service was built in-house using a public facing EC2 instance with an FTP server installed on it. It stored the orders locally and sometimes ran out of disk space during periods of high traffic.

Sometime last year, one of the clients wanted to improve security for their content and has requested for SFTP to be made available, along with private key authentication.

## Overview of proposal
From the business' point of view, the requirement is to ensure all clients can use this service regardless of whether they are migrating to SFTP or not. From the development point of view, the FTP server can easily take on a new protocol which is SFTP.

But from the architecture point of view, this is an opportunity to push for a solution that has lower maintenance effort and does not run out of space. AWS Transfer Family is a great candidate as it is a managed service which seamlessly works with S3 storage. 

### steps
Discussion with stakeholders over deciding on a build-or-buy solution for the requirement.

Formal proposal with working proof of concept.

Coordinate with product, development and support teams to allow said client to switch to SFTP while ensuring other clients carry on with FTP.


