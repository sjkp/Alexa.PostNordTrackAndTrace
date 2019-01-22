using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Postnord.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Alexa.PostnordTrackAndTrace.Tests
{
    [TestClass]
    public class TrackAndTraceServiceTest
    {
        [DeploymentItem("200.json")]
        [TestCategory("SkipWhenLiveUnitTesting")]
        [TestMethod]
        public async Task TrackAndTraceTest()
        {
            Mock<HttpMessageHandler> handlerMock = PostnordTest.Setup(File.ReadAllText("200.json"));
            var service = new TrackAndTraceService(new Postnord.API.PostnordTrackAndTraceClient(new HttpClient(handlerMock.Object)), "mykey");
            var data = await service.GetTrackAndTraceInfo("373303810003399402");

            PostnordTest.Verify(handlerMock);
        }
    }
}
