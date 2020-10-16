# Playing with - new lambda feature on October 8th 2020
An easy way to integrate monitoring tools into lambdas was announced on the compute blog below.

https://aws.amazon.com/blogs/compute/introducing-aws-lambda-extensions-in-preview/

This portfolio entry shows a tryout of the preview and seeing it in action

# Overview
Using the repo at https://github.com/aws-samples/aws-lambda-extensions/tree/main/awsappconfig-extension-demo

The goal is to get it up and running making use of AppConfig in two ways
- Retrieve configuration with the current, SDK way
- Retrieve configuration with the new extensions way

Doing each of the above, twice. Also, call the lambda twice.

Will there be any cost savings? Let's find out!

## Steps
1. Set up the code in AWS account, with AppConfig. See the output

![Output](https://github.com/FadeDragon/Resume2020/blob/master/Playing%20with%20-%20Lambda%20Extensions%20Preview/Playing%20with%20appconfig.jpg)

2. Get configurations with the current way, and the extensions way. Run the function twice and then check cloudwatch.

![Results](https://github.com/FadeDragon/Resume2020/blob/master/Playing%20with%20-%20Lambda%20Extensions%20Preview/Playing%20with%20extensions-results.jpg)

# Conclusion
The first function ran for about:

1. AppConfig without extensions, called twice, total of 1.280 seconds 
2. AppConfig with extensions, called twice, total of 1.180 seconds 

There is an improvement of 0.1 seconds, not much of time saved by switching to usage of extensions. How above the second function?

1. AppConfig without extensions, called twice, total of 0.206 seconds 
2. AppConfig with extensions, called twice, total of 0.059 seconds 

There is an improvement of 0.15 seconds this time. 

The time and cost savings can be significant if there are many more calls to multiple appconfig.getConfiguration() in real world lambdas.

## Next Steps

The importance of extensions is the ability to write custom tools using the extensions API.

https://aws.amazon.com/blogs/compute/building-extensions-for-aws-lambda-in-preview/

Lambda extensions with third party tools will make work easier as we integrate with some of them.