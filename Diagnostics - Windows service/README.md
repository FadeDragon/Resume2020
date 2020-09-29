# Contents
- Windows service .NET framework project

- Monitoring of critical company event consumers and windows services

- Message sender to Slack channels via web hook

# Background
Many legacy services and workflows rely on windows services to act upon events and updating database records while processing them.

## Currently

Administrators have to monitor these windows services as they can hang or stop unexpectedly, causing delays to customers' workflows.

If attempts to restart fails they need to chat with developers to take remedial action, resulting in even larger delays.

## Overview of requirements
From the business' point of view, the requirement is to ensure service outages and risks to the business is reduced to the minimum. From the development point of view, administrators sometimes do not manage to raise an alert until it is too late. Support teams often have little to no warnings over when a customer's orders will get delayed.

A simple windows service which checks these other services has been created and to automatically raise alerts to Slack channels.

## Jobs
###ServicesCheckJob
Regularly checks all needed services are running.

1. Have a list of services to check
1. Go through the list and determine if a service is running
1. Attempt to restart non-runnning services first
1. By 10 minutes, a non-running service that did not manage to restart must be alerted to a Slack channel
  * double check that alerts are not sent multiple sometimes

###ServicesMetricsJob
Regularly raises performance metrics to cloud watch.

1. Have a list of services to check
1. Go through the list and determine CPU and memory usage
1. Upload to cloud watch, for naming convention of logs use 'MemoryCPUServicesUsage/<process name>' 

# Next steps
Configure cloud watch alarms for the MemoryCPUServicesUsage related logs