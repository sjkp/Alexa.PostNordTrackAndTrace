using Alexa.NET.APL;
using Alexa.NET.APL.Components;
using Alexa.NET.APL.DataSources;
using Alexa.NET.Response;
using Alexa.NET.Response.APL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexa.PostnordTrackAndTrace
{
    public class APLLayoutBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public RenderDocumentDirective VerticalScrollList(ScrollListData data)
        {
            var layout = new Layout();
            layout.Parameters = new[] { new Parameter("payload") };
            layout.Items = new APLComponent[]{new VerticalScrollListContainer()
                {
                    Title = "${payload.data.title}",
                    BackgroundImage = "${payload.data.backgroundImage}",
                    ListData = "${payload.data.listItems}"
                }
            };
            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument
                {
                    Imports = new Import[]
                        {
                            new Import("alexa-layouts", "1.0.0")
                        },
                    MainTemplate = layout,
                    Layouts = new Dictionary<string, Layout>()
                    {
                        {nameof(VerticalScrollListContainer), MakeVerticalScrollListContainer()},
                        {nameof(VerticalListItem), MakeVerticalScrollListItem() }
                    }                    
                },
                DataSources = new Dictionary<string, APLDataSource>()
                {
                    { "data", new KeyValueDataSource() {
                        Properties = data.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).ToDictionary(s => s.GetCustomAttributes(typeof(JsonPropertyAttribute), false).Cast<JsonPropertyAttribute>().Select(p => p.PropertyName).First(), k => k.GetValue(data))
                    } }
                }
            };

            return directive;
        }

        private Layout MakeVerticalScrollListItem()
        {
            var layout = new Layout();
            layout.Parameters = new[] { new Parameter("header"), new Parameter("subHeader"), new Parameter("description") };
            layout.Items = new APLComponent[] {new Container(new APLComponent[]
                {
                    new Container(new APLComponent[]
                    {
                        new Text("${header}")
                        {
                            FontSize = "15",
                            Color = "#ffffff"
                        },
                        new Text("${subHeader}"){
                            FontSize = "30",
                            Color = "#ffffff"
                        },
                        new Text("${description}")
                        {
                            FontSize = "20",
                            Color = "#ffffff"
                        }
                    })
                    {
                        Direction= "column",
                        Spacing = "40dp"
                    }
                })
            {
                Direction = "row",
                Height = 125,
                PaddingLeft = 40,
                Spacing = "5vh",                
                Width = "100%",
                AlignItems = "center",
            } };

            return layout;
        }

        private Layout MakeVerticalScrollListContainer()
        {
            var layout = new Layout();
            layout.Parameters = new[] { new Parameter("backgroundImage"), new Parameter("listData"), new Parameter("title") };
            layout.Items = new APLComponent[]{
                new Container(
                    new APLComponent[]{
                        new Image()
                        {
                            Position = "absolute",
                            Width = "100vw",
                            Height = "100vh",
                            Scale = ImageScale.BestFill,
                            OverlayColor = "#000000bb",
                            Source = "${backgroundImage}"
                        },
                        new AlexaHeader()
                        {
                            HeaderTitle = "${title}"
                        },
                        new DataBoundSequence()
                        {
                            Grow = 1,
                            ScrollDirection = "vertical",
                            Height = "80vh",
                            Data = "${listData}",
                            Items = new APLComponent[]
                            {
                                new VerticalListItem() {
                                    Header = "${data.header}",
                                    SubHeader = "${data.subHeader}",
                                    Description = "${data.description}"
                                }
                            }.ToList()
                        }
                    }
             )
                {
                    Direction = "column",
                    Height = "100vh",
                    Width= "100vw"
                }
            };
            return layout;
        }

        public class VerticalListItem : APLComponent
        {
            public override string Type => nameof(VerticalListItem);
            [JsonProperty("header", NullValueHandling = NullValueHandling.Ignore)]
            public APLValue<string> Header { get; set; }
            [JsonProperty("subHeader", NullValueHandling = NullValueHandling.Ignore)]
            public APLValue<string> SubHeader { get; set; }
            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public APLValue<string> Description { get; set; }
        }

        public class VerticalScrollListContainer : APLComponent
        {
            public VerticalScrollListContainer()
            {
                
            }
            public override string Type => nameof(VerticalScrollListContainer);
            [JsonProperty("backgroundImage", NullValueHandling = NullValueHandling.Ignore)]
            public APLValue<string> BackgroundImage { get; set; }
            [JsonProperty("listData", NullValueHandling = NullValueHandling.Ignore)]
            public APLValue<string> ListData { get; set; }
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public APLValue<string> Title { get; set; }

        }
    }

    public class ScrollListData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("latestStatus")]
        public string LatestStatus { get; set; }

        [JsonProperty("listItems")]
        public List<ScrollListItem> ListItems { get; set; }

        [JsonProperty("backgroundImage")]
        public string BackgroundImage { get; set; }
    }

    public class ScrollListItem
    {
        [JsonProperty("header", NullValueHandling = NullValueHandling.Ignore)]
        public string Header { get; set; }
        [JsonProperty("subHeader", NullValueHandling = NullValueHandling.Ignore)]
        public string SubHeader { get; set; }
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
