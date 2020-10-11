# Playing with - new lambda feature on October 8th 2020
An easy way to integrate monitoring tools into lambdas was announced on the compute blog below.

https://aws.amazon.com/blogs/compute/introducing-aws-lambda-extensions-in-preview/

This portfolio entry shows a tryout of the preview and seeing it in action

# Overview
Using the repo at https://github.com/aws-samples/aws-lambda-extensions/tree/main/awsappconfig-extension-demo

The goal is to get it up and running to make use of AppConfig to roll out configuration changes into the demo lambda

## Steps
1. Set up the code in AWS account, with AppConfig
2. Try changing configurations and see the output

![Output](https://github.com/FadeDragon/Resume2020/blob/master/Email%20Service%20-%20API/Playing%20with%20appconfig)

# Conclusion
Because a function now fetches configuration data faster using a local call rather than over the network by using the AWS AppConfig extension, this translates into cost savings by shortening execution time of lambdas.
