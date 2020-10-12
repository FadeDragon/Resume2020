# Playing with - new lambda feature on October 8th 2020
An easy way to integrate monitoring tools into lambdas was announced on the compute blog below.

https://aws.amazon.com/blogs/compute/introducing-aws-lambda-extensions-in-preview/

This portfolio entry shows a tryout of the preview and seeing it in action

# Overview
Using the repo at https://github.com/aws-samples/aws-lambda-extensions/tree/main/awsappconfig-extension-demo

The goal is to get it up and running to make use of AppConfig to roll out configuration changes into the demo lambda

## Steps
1. Set up the code in AWS account, with AppConfig

![Original](https://github.com/FadeDragon/Resume2020/blob/master/Playing%20with%20-%20Lambda%20Extensions%20Preview/Playing%20with%20extensions.jpg)

2. Try changing configurations and see the output

![Output](https://github.com/FadeDragon/Resume2020/blob/master/Playing%20with%20-%20Lambda%20Extensions%20Preview/Playing%20with%20appconfig.jpg)

# Conclusion
The function ran for about 1.296 seconds with AppConfigs as extension by observing timestamps in cloud watch.

TODO - Make the same function but without using extensions. Check the execution time the function needed to run.

Because a function now fetches configuration data faster using a local call rather than over the network by using the AWS AppConfig extension, this translates into cost savings by shortening execution time of lambdas.
