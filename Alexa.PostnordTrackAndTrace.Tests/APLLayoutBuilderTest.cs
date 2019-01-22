using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alexa.PostnordTrackAndTrace.Tests
{
    [TestClass]
    public class APLLayoutBuilderTest
    {
        [TestMethod]
        public void BuildVerticalScrollList()
        {
            var builder = new APLLayoutBuilder();
            var list = builder.VerticalScrollList(new ScrollListData()
            {
                BackgroundImage = "http://image",
                Title = "hello world",
                ListItems = new List<ScrollListItem>()
                {
                    new ScrollListItem()
                    {
                        Description = "desc1 ",
                        Header = "header 1",
                        SubHeader = "sub head 1"
                    }
                }
            });

            Console.Write(JsonConvert.SerializeObject(list));
        }
    }
}
