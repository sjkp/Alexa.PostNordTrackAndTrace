using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Postnord.API
{
    [TestClass]
    public class PostnordTest
    {
        IConfiguration Configuration { get; set; }

        [DeploymentItem("200.json")]
        [TestCategory("SkipWhenLiveUnitTesting")] 
        [TestMethod]
        public async Task TrackAndTraceTest()
        {
            Mock<HttpMessageHandler> handlerMock = Setup(File.ReadAllText("200.json"));

            var builder = new ConfigurationBuilder()
            .AddUserSecrets<PostnordTest>();

            Configuration = builder.Build();

            var client = new PostnordTrackAndTraceClient(new System.Net.Http.HttpClient(handlerMock.Object));

            var response = await client.RestShipmentV2TrackandtraceFindByIdentifierByReturntypeGetAsync(Returntype2.Json, Configuration["PostnordAPIKey"], "373303810003399402");

            Assert.AreEqual(1, response.TrackingInformationResponse.Shipments.Count);
            Verify(handlerMock);
        }

        [TestMethod]
        [TestCategory("SkipWhenLiveUnitTesting")]
        public async Task TrackAndTraceTestIntegration()
        {
            var builder = new ConfigurationBuilder()
           .AddUserSecrets<PostnordTest>();

            Configuration = builder.Build();

            var client = new PostnordTrackAndTraceClient(new System.Net.Http.HttpClient());

            var response = await client.RestShipmentV2TrackandtraceFindByIdentifierByReturntypeGetAsync(Returntype2.Json, Configuration["PostnordAPIKey"], "373303810003399402");

            Assert.AreEqual(1, response.TrackingInformationResponse.Shipments.Count);
        }
        
        
        [TestMethod]
        public async Task TrackAndTraceTest2()
        {
            Mock<HttpMessageHandler> handlerMock = Setup(@"{
  ""TrackingInformationResponse"": {
    ""shipments"": []
    }
}");

            var builder = new ConfigurationBuilder()
            .AddUserSecrets<PostnordTest>();

            Configuration = builder.Build();

            var client = new PostnordTrackAndTraceClient(new System.Net.Http.HttpClient(handlerMock.Object));

            var response = await client.RestShipmentV2TrackandtraceFindByIdentifierByReturntypeGetAsync(Returntype2.Json, Configuration["PostnordAPIKey"], "373303810003399402");

            Assert.AreEqual(0, response.TrackingInformationResponse.Shipments.Count);
            Verify(handlerMock);
        }

        public static void Verify(Mock<HttpMessageHandler> handlerMock)
        {
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        public static Mock<HttpMessageHandler> Setup(string content)
        {
            // ARRANGE
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(content),
               })
               .Verifiable();
            return handlerMock;
        }
    }
}
