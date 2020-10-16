 // Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

const AWS = require('aws-sdk');
const appconfig = new AWS.AppConfig({apiVersion: '2019-10-09'});
const http = require('http');
const params = process.env.APPCONFIG_PROFILE.split('/')
const AppConfigApplication = params [0]
const AppConfigEnvironment = params [1]
const AppConfigConfiguration = params [2]
let coldstart = true;

function getConfiguration(application, environment, configuration) {
    return new Promise((resolve, reject) => {
        const req = http.get(`http://localhost:2772/applications/${application}/environments/${environment}/configurations/${configuration}`, (res) => {
            if (res.statusCode < 200 || res.statusCode >= 300) {
                return reject(new Error('statusCode=' + res.statusCode));
            }
            var body = [];
            res.on('data', function(chunk) {
                body.push(chunk);
            });
            res.on('end', function() {
                resolve(Buffer.concat(body).toString());
            });
        });
        req.on('error', (e) => {
            reject(e.message);
        });
        req.end();
    });
}

exports.handler = async (event) => {
  try {
    const params1 = {
        Application: AppConfigApplication, /* required */
        ClientId: 'AppConfigLabAPIGatewayLambda', /* required */
        Configuration: AppConfigConfiguration, /* required */
        Environment: AppConfigEnvironment, /* required */
    };
	console.log("current way1");
    let appConfigResponse = await appconfig.getConfiguration(params1).promise();
	console.log("complete");
	console.log("current way2");
	appConfigResponse = await appconfig.getConfiguration(params1).promise();
	console.log("complete");
    const configDataOri = Buffer.from(appConfigResponse.Content, 'base64').toString('ascii');
    const parsedConfigDataOri = JSON.parse(configDataOri);
    let LogLevelOri = parsedConfigDataOri.loglevel;
  
    console.log("extensions way1");
    let configData = await getConfiguration(AppConfigApplication, AppConfigEnvironment, AppConfigConfiguration);
	console.log("complete");
	console.log("extensions way2");
	configData = await getConfiguration(AppConfigApplication, AppConfigEnvironment, AppConfigConfiguration);
	console.log("complete");
    const parsedConfigData = JSON.parse(configData);
    let LogLevel = parsedConfigData.loglevel
    
    return {
      'event' : event,
      'ColdStart' : coldstart,
      'LogLevel': LogLevel,
      'LogLevelOri': LogLevelOri
      }
  } catch (err) {
      console.error(err)
      return err
  } finally {
    coldstart = false;
  }
}; 
