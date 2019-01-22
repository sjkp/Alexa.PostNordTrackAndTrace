using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;
using System.Collections.Generic;
using Alexa.NET.Response.APL;
using Alexa.NET.APL.Components;
using Alexa.NET.APL;
using System.Linq;

namespace Alexa.PostnordTrackAndTrace
{
    public static class Alexa
    {
        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var repo = new ShipmentRepo();
            string json = await req.ReadAsStringAsync();
            log.LogInformation(json);
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();
            var session = skillRequest.Session;
            var aplSupport = skillRequest.Context.System.Device.IsInterfaceSupported("Alexa.Presentation.APL");
            SkillResponse response = null;
            var shipment = (await repo.RetrieveEntity<Shipment>($"PartitionKey eq '{session.User.UserId}'")).FirstOrDefault();

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to PostNord My Shipment. " + (shipment == null || (DateTimeOffset.UtcNow - shipment.Timestamp) > new TimeSpan(24,0, 0) ? 
                    "Say find shipment followed by your reference number to get the latest status" : "You can say get latest to get an updated status on your last shipment"));
                if (aplSupport)
                {
                    AddWelcomeAPL(response);
                }
                response.Response.ShouldEndSession = false;
            }

            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                if (intentRequest.Intent.Name == "AMAZON.HelpIntent")
                {
                    response = ResponseBuilder.Tell("Say find shipment followed by your reference number to get the latest status");
                    response.Response.ShouldEndSession = false;
                }
                if (intentRequest.Intent.Name == "lookupstatus")
                {
                    
                    if (intentRequest.Intent.Slots.ContainsKey("trackingnumber"))
                    {

                        string trackingnumber = intentRequest.Intent.Slots["trackingnumber"].Value;                        
                        var newshipment = new Shipment(session.User.UserId, trackingnumber);
                        SetSessionAttribute(session, "rowkey", newshipment.RowKey);
                        await repo.InsertEntity(newshipment, true);
                        response = await CreateResponse(log, session, response, trackingnumber, aplSupport);
                    }
                    else
                    {
                        string output = "Ooops didn't quite get that, please say find shipment followed by your reference number";
                        response = ResponseBuilder.Tell(output);
                        response.Response.ShouldEndSession = false;
                    }
                    
                    
                }
                if (intentRequest.Intent.Name == "lookupagain")
                {                   
                    if (shipment!=null)
                    {
                        response = await CreateResponse(log, session, response, shipment.ShipmentId, aplSupport);
                    }
                    else
                    {
                        string output = "You haven't asked for any shipment information before. Please say find shipment followed by your reference number";
                        response = ResponseBuilder.Tell(output);
                        response.Response.ShouldEndSession = false;
                    }                    
                }
                if (intentRequest.Intent.Name == "AMAZON.YesIntent")
                {
                    if (session.Attributes.ContainsKey("trackingnumber"))
                    {
                        response = ResponseBuilder.Tell("Great, we will inform you with a notification once the shipment status changes.");
                    }
                }
                if (intentRequest.Intent.Name == "AMAZON.NoIntent")
                {
                    if (session.Attributes.ContainsKey("trackingnumber"))
                    {
                        response = ResponseBuilder.Tell("Okay, if you change your mind just lookup the shipment again.");
                    }
                }
                if (intentRequest.Intent.Name == "AMAZON.FallbackIntent")
                {
                    string output = "Please say find shipment followed by your reference number";
                    response = ResponseBuilder.Tell(output);
                    response.Response.ShouldEndSession = false;
                }
                if (intentRequest.Intent.Name == "AMAZON.StopIntent")
                {
                    response = ResponseBuilder.Empty();
                    response.Response.ShouldEndSession = true;
                }
            }

            return new OkObjectResult(response);
        }

        private static void AddWelcomeAPL(SkillResponse response)
        {
            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument
                {
                    Imports = new Import[]
                                    {
                            new Import("alexa-layouts", "1.0.0")
                                    },
                    MainTemplate = new Layout(new[]
                               {
                        new Container(new APLComponent[]{
                            new Image("https://alexatt.z16.web.core.windows.net/transite.jpg")
                            { Width = "100vw", Height = "100vh", Position = "absolute", Scale = new NET.APL.APLValue<ImageScale>(ImageScale.BestFill), OverlayColor = "#000000bb" },
                            new Container(new APLComponent[]{
                                new Text("Welcome to PostNord My Shipment")
                                {
                                    TextAlign = "${viewport.shape == 'round' ? 'center' : 'left'}",
                                    PaddingRight = "40",
                                    PaddingLeft = "40",
                                    Color = "#ffffff"
                                }
                            })
                            {
                                Grow = 0,
                                JustifyContent = "${viewport.shape == 'round' ? 'center' : 'end'}"
                            },
                            new AlexaFooter()
                            {
                               When = When.RectangleViewport,
                               FooterHint = "Say find shipment followed by your reference number to get the latest status"
                            }

                        }){Height = "100vh"}
                    })
                }
            };
            response.Response.Directives.Add(directive);
        }

        private static async Task<SkillResponse> CreateResponse(ILogger log, Session session, SkillResponse response, string value, bool isAplSupported)
        {
            var service = new TrackAndTraceService();
            log.LogInformation($"Looking up track and trace for {value}");
            var data = await service.GetTrackAndTraceInfo(value);

            log.LogInformation($"Reponse was {data.Title}");
            var output = new SsmlOutputSpeech()
            {
                Ssml = $"<speak>The status of shipment  <say-as interpret-as=\"digits\">{value}</say-as> is: {data.LatestStatus}</speak>" //Do you want to recieve notifications?" ;
            };
            SetSessionAttribute(session, "trackingnumber", value);            
            //var reprompt = new Reprompt("Do you want to recieve notifications?");
            response = ResponseBuilder.Tell(output, session);

            if (isAplSupported)
            {
                response.Response.Directives.Add(new APLLayoutBuilder().VerticalScrollList(data));
            }
            return response;
        }

        private static void SetSessionAttribute(Session session, string key, object value)
        {
            if (session.Attributes == null)
                session.Attributes = new Dictionary<string, object>();
            session.Attributes[key] = value;
        }
    }
}
