using Alexa.NET.APL;
using Alexa.NET.Response.APL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alexa.PostnordTrackAndTrace
{
    public class AlexaFooter : APLComponent
    {
        public override string Type => "AlexaFooter";

        [JsonProperty("footerHint", NullValueHandling = NullValueHandling.Ignore)]
        public APLValue<string> FooterHint { get; set; }
    }
}
