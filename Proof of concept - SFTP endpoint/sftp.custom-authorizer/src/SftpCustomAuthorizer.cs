using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace sftp.custom_authorizer
{
    public class SftpCustomAuthorizer
    {
        private static AmazonCognitoIdentityProviderClient cognitoClient;
        private static readonly string userPoolId = Environment.GetEnvironmentVariable("UserPoolId");
        private static readonly string clientId = Environment.GetEnvironmentVariable("ClientId");

        // <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public SftpCustomAuthorizer()
        {
            cognitoClient = new AmazonCognitoIdentityProviderClient(RegionEndpoint.USWest2);
        }

        /// <summary>
        /// Constructor for unit test
        /// </summary>
        public SftpCustomAuthorizer(AmazonCognitoIdentityProviderClient client)
        {
            cognitoClient = client;
        }

        /// <summary>
        /// Respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns>If the user is found, returns user attributes</returns>
        public async Task<APIGatewayProxyResponse> GetCognitoUserHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get GetCognitoUserHandler\n");

            // Username will be in the query path, password will be passed via Header
            var userName = request.PathParameters.ContainsKey("username") ? request.PathParameters["username"] : null;
            var password = request.Headers.ContainsKey("Password") ? request.Headers["Password"] : null;

            try
            {
                foreach (var header in request.Headers)
                {
                    context.Logger.LogLine($"Header '{header.Key}' : '{header.Value}'");
                }

                foreach (var path in request.PathParameters)
                {
                    context.Logger.LogLine($"Path Parameter '{path.Key}' : '{path.Value}'");
                }
            }
            catch
            {
                context.Logger.LogLine("Ignoring malformed headers and PathParameters\n");
            }

            // TODO : Remove from production
            context.Logger.LogLine($"User Name-{userName} & password-{password}\n");
            if (string.IsNullOrEmpty(userName))
            {
                context.Logger.LogLine("No UserName found!");

                return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            CognitoUserCustomAttributes userCustomAttributes;

            // SSH Support for users that prefer using SSH key
            if (string.IsNullOrEmpty(password))
            {
                context.Logger.LogLine("No password, using SSH Key flow");

                userCustomAttributes = await CognitoSshSupport(userName);
            }
            else
            {
                context.Logger.LogLine("Password flow");

                userCustomAttributes = await AuthenticateAndGetCognitoUserCustomAttributes(userName, password);
            }

            // API gateway expects json response
            var jsonUserAttributes = JsonConvert.SerializeObject(userCustomAttributes);

            if (!string.IsNullOrEmpty(userCustomAttributes.ErrorMessage))
            {
                context.Logger.LogLine($"custom attributes return from cognito - {jsonUserAttributes}");

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = jsonUserAttributes,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // User home directories stored in user attributes, preventing different users from seeing each other's content
            context.Logger.LogLine($"user with attirbutes - {jsonUserAttributes}");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = jsonUserAttributes,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        /// <summary>
        /// SSH Key support, using admin API to fetch user details
        /// </summary>
        /// <param name="userName"></param>
        /// <returns> CognitoUserCustomAttributes </returns>
        private async Task<CognitoUserCustomAttributes> CognitoSshSupport(string userName)
        {
            var adminRequest = new AdminGetUserRequest
            {
                Username = userName,
                UserPoolId = userPoolId
            };

            try
            {
                var adminGetUserResponse = await cognitoClient.AdminGetUserAsync(adminRequest);

                var identityResponse = new CognitoUserCustomAttributes
                {
                    Role = GetAdminUserAttributeValue(adminGetUserResponse, CustomAttributeField.Role),
                    HomeDirectoryDetails = GetAdminUserAttributeValue(adminGetUserResponse, CustomAttributeField.HomeDirectoryDetails),
                    HomeDirectory = GetAdminUserAttributeValue(adminGetUserResponse, CustomAttributeField.HomeDirectory),
                    HomeDirectoryType = CustomAttributeField.HomeDirectoryType,
                    PublicKeys = new List<string> { GetAdminUserAttributeValue(adminGetUserResponse, CustomAttributeField.PublicKey) },
                    Policy = GetAdminUserAttributeValue(adminGetUserResponse, CustomAttributeField.Policy)
                };

                // HomeDirectoryDetails to hide actual S3 path
                identityResponse.HomeDirectoryType = identityResponse.HomeDirectoryDetails != null ? CustomAttributeField.HomeDirectoryType : null;

                return identityResponse;
            }
            catch (Exception ex)
            {
                return new CognitoUserCustomAttributes
                {
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Password flow, using admin API to fetch user details
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns> CognitoUserCustomAttributes </returns>
        private async Task<CognitoUserCustomAttributes> AuthenticateAndGetCognitoUserCustomAttributes(string userName, string password)
        {

            var request = new AdminInitiateAuthRequest
            {
                UserPoolId = userPoolId,
                ClientId = clientId,
                AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH
            };

            request.AuthParameters.Add("USERNAME", userName);
            request.AuthParameters.Add("PASSWORD", password);

            AdminInitiateAuthResponse response;

            try
            {
                response = await cognitoClient.AdminInitiateAuthAsync(request);
            }
            catch (Exception ex)
            {
                return new CognitoUserCustomAttributes
                {
                    ErrorMessage = ex.Message
                };
            }

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                return new CognitoUserCustomAttributes
                {
                    ErrorMessage = "Login error"
                };
            }

            var hand = new JwtSecurityTokenHandler();
            var customAttributes = hand.ReadJwtToken(response.AuthenticationResult.IdToken);

            var identityResponse = new CognitoUserCustomAttributes
            {
                Role = GetCustomAttributeValue(customAttributes, CustomAttributeField.Role),
                HomeDirectoryDetails = GetCustomAttributeValue(customAttributes, CustomAttributeField.HomeDirectoryDetails),
                HomeDirectory = GetCustomAttributeValue(customAttributes, CustomAttributeField.HomeDirectory),
                Policy = GetCustomAttributeValue(customAttributes, CustomAttributeField.Policy)
            };

            // HomeDirectoryDetails to hide actual S3 path
            identityResponse.HomeDirectoryType = identityResponse.HomeDirectoryDetails != null ? CustomAttributeField.HomeDirectoryType : null;

            return identityResponse;
        }

        private static string GetCustomAttributeValue(JwtSecurityToken token, string attributeType)
        {
            return token.Claims.FirstOrDefault(claim => claim.Type == attributeType)?.Value;
        }

        private static string GetAdminUserAttributeValue(AdminGetUserResponse response, string attributeType)
        {
            return response.UserAttributes.FirstOrDefault(att => att.Name == attributeType)?.Value;
        }


    }

    /// <summary>
    /// Contains information to be used for user authentication and authorization
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CognitoUserCustomAttributes
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Role { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HomeDirectoryDetails { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HomeDirectoryType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HomeDirectory { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Policy { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PublicKeys { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }
    }



    /// <summary>
    /// Use custom attributes in Cognito User Pool for user authentication and authorization
    /// </summary>
    public static class CustomAttributeField
    {
        public static readonly string Role = "custom:Role";
        public static readonly string HomeDirectoryDetails = "custom:HomeDirectoryDetails";
        public static readonly string HomeDirectoryType = "LOGICAL";
        public static readonly string HomeDirectory = "custom:HomeDirectory";
        public static readonly string PublicKey = "custom:PublicKey";
        public static readonly string Policy = "custom:Policy";
    }
}
