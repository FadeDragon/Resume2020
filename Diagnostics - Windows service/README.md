# Contents
- Windows service .NET framework projecct

- Monitoring of critical company event consumers and windows services

- Message sender to Slack channels

# Background
Many legacy services and workflows rely on windows services to act upon events and updating database records while processing them.

## Currently

Administrators have to monitor these windows services as they can hang or stop unexpectedly. If attempts to restart fails they need to chat with developers to take remedial action, resulting in occassional, large delays to the workflows.

## Overview of proposal
From the business' point of view, the requirement is to ensure service outages and risks to the business is reduced to the minimum. From the development point of view, administrators sometimes do not manage to raise an alert until it is too late. Support teams often have little to no warnings over when a customer's orders will get delayed.

A simple windows service to watch these other services has been created and to automatically raise alerts to different channels. Logic to use Slack web hook is present in this example.
