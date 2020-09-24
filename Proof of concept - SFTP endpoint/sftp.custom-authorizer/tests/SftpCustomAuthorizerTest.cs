using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace sftp.custom_authorizer.Tests
{
    public class SftpCustomAuthorizerTest
    {
        private Mock<AmazonCognitoIdentityProviderClient> cognitoIdProvider;
        private SftpCustomAuthorizer handler;
        private TestLambdaContext context;
        private APIGatewayProxyRequest gatewayRequest;

        [SetUp]
        public void Setup()
        {
            cognitoIdProvider = new Mock<AmazonCognitoIdentityProviderClient>();
            handler = new SftpCustomAuthorizer(cognitoIdProvider.Object);
            context = new TestLambdaContext();
        }

        [Test, Category("Unit")]
        public async Task Given_Username_Without_Password_When_Lambda_Executes_Then_SSH_Auth_Is_Used()
        {
            // Arrange
            gatewayRequest = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "username", "test" }
                },
                Headers = new Dictionary<string, string>
                {
                    { "Password", null }
                }
            };

            cognitoIdProvider.Setup(p =>
                    p.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(),
                                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AdminGetUserResponse
                {
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = CustomAttributeField.Role, Value = "test-role" },
                        new AttributeType { Name = CustomAttributeField.HomeDirectoryDetails, Value = "for role" },
                        new AttributeType { Name = CustomAttributeField.HomeDirectory, Value = "uploads/" },
                        new AttributeType { Name = CustomAttributeField.HomeDirectoryType, Value = "" },
                        new AttributeType { Name = CustomAttributeField.PublicKey, Value = "rsa-public" },
                        new AttributeType { Name = CustomAttributeField.Policy, Value = "policy" },
                    }
                });

            // Act
            var result = await handler.GetCognitoUserHandler(gatewayRequest, context);

            // Assert
            Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.OK);
            Assert.That(result.Body.Contains("\"Role\":\"test-role\""));
            Assert.That(result.Body.Contains("\"HomeDirectoryType\":\"LOGICAL\""));
            cognitoIdProvider.Verify(p =>
                    p.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test, Category("Unit")]
        public async Task Given_UserName_And_Password_When_Lambda_Executes_Then_Password_Auth_And_Extract_Atrributes_From_Token()
        {
            // Arrange
            gatewayRequest = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "username", "test" }
                },
                Headers = new Dictionary<string, string>
                {
                    { "Password", "pwd1" }
                }
            };

            cognitoIdProvider.Setup(p => p.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AdminInitiateAuthResponse
            {
                AuthenticationResult = new AuthenticationResultType { IdToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJjdXN0b206Um9sZSI6IlJvbGUxIn0.1Df8zKJ-XYNW70QvLcBwzIpUX7hWGxr9Oa-irI2T0Gs" },
                HttpStatusCode = HttpStatusCode.OK
            });

            //Act
            var result = await handler.GetCognitoUserHandler(gatewayRequest, context);

            // Assert
            Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.OK);
            Assert.That(result.Body.Contains("\"Role\":\"Role1\""));
            cognitoIdProvider.Verify(p => p.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, Category("Unit")]
        public async Task Given_No_UserName_When_Lambda_Executes_Then_StatusCode_Is_BadRequest()
        {
            // Arrange
            gatewayRequest = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string>
                {
                    { "username", null }
                },
                Headers = new Dictionary<string, string>()
            };

            // Act
            var result = await handler.GetCognitoUserHandler(gatewayRequest, context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }
    }
}
