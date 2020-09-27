# Contents
- Windows service .NET framework projecct

- Monitoring of critical company event consumers and windows services

- Message sender to Slack channels

# Proof of concept - Background
Many legacy services and workflows rely on windows services to act upon events and update database records when processing is complete.

## Currently

Administrators have to monitor these windows services as they can become hung or stopped unexpectedly. If attempts to restart fails they need to chat up developers to take remedial action, resulting in occassional, large delays to the workflows.

## Overview of proposal
From the business' point of view, the requirement is to ensure large delays to the workflows is reduced to the minimum. From the development point of view, administrators sometimes do not manage to raise an alert until too late. Support teams often have little to no warnings over customer's orders becoming delayed.

A simple windows service to watch these other services has been created and to automatically raise alerts to different parties. 




