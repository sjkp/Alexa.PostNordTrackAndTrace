using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Alexa.PostnordTrackAndTrace
{
    public class TrackTraceNumberLocator
    {
        public static string Parse(string body)
        {
            var matches = new Regex("\\d{18}").Matches(body);

            return matches.Count > 0 ? matches[0].Value : null;
        }
    }
}
