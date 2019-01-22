using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alexa.PostnordTrackAndTrace.Tests
{
    [TestClass]
    public class TrackAndTraceLocatorTest
    {
        [TestMethod]
        public void ParseTest()
        {
            var testStr = @"Nyt om leveringen
Produkt	Antal	Leveringsinformation
Apple MacBook Pro 15"" med Touch Bar MR972DK/ A(2018 - model) - Silver
1   Status: Vi har modtaget Track &Trace info
Leveres af: Post Nord -Track & Trace nummer:
            373303810003399402";
            Assert.AreEqual("373303810003399402", TrackTraceNumberLocator.Parse(testStr));
        }
    }
}
