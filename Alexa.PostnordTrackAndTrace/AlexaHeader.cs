using Alexa.NET.APL;
using Alexa.NET.Response.APL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alexa.PostnordTrackAndTrace
{
    public class AlexaHeader : APLComponent
    {
        public override string Type => "AlexaHeader";

        [JsonProperty("headerTitle", NullValueHandling = NullValueHandling.Ignore)]
        public APLValue<string> HeaderTitle { get; set; }


        [JsonProperty("headerAttributionImage", NullValueHandling = NullValueHandling.Ignore)]
        public APLValue<string> HeaderAttributionImage { get; set; }
    }
}
