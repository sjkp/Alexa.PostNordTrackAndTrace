using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Postnord.API;

namespace Alexa.PostnordTrackAndTrace
{
    public class TrackAndTraceService
    {
        const string backgroundImage = "https://alexatt.z16.web.core.windows.net/transite.jpg";
        private readonly PostnordTrackAndTraceClient client;
        private readonly string apiKey;

        public TrackAndTraceService()
        {
            this.client = new Postnord.API.PostnordTrackAndTraceClient(new System.Net.Http.HttpClient());
            this.apiKey = Environment.GetEnvironmentVariable("PostnordAPIKey");
        }
        public TrackAndTraceService(PostnordTrackAndTraceClient client, string apiKey)
        {
            this.client = client;
            this.apiKey = apiKey;
        }

        public async Task<ScrollListData> GetTrackAndTraceInfo(string id)
        {
            var response = await this.client.RestShipmentV2TrackandtraceFindByIdentifierByReturntypeGetAsync(Returntype2.Json, this.apiKey, id);
            var shipment = response.TrackingInformationResponse.Shipments.FirstOrDefault();
            if (shipment == null)
            {
                var status = $"No track and trace info found";
                return new ScrollListData()
                {
                    Title = status,
                    LatestStatus = status, 
                    BackgroundImage = backgroundImage
                };
            }
            var listItems = shipment.Items.FirstOrDefault()?.Events.OrderByDescending(s => s.EventTime).Select(s => new ScrollListItem()
            {
                Description = MakeDescription(s),
                Header = s.EventTime.ToString("f"),
                SubHeader = s.EventDescription
            }).ToList();
            return new ScrollListData()
            {
                Title = $"Track and trace for {id}",
                LatestStatus = shipment.StatusText?.Header,
                ListItems = listItems,
                BackgroundImage = shipment.Status == Status18.DELIVERED ? "https://alexatt.z16.web.core.windows.net/delivered2.jpg" : backgroundImage
            };
        }

        private static string MakeDescription(TrackingEventDto s)
        {
            return $"{s.Location?.DisplayName}: " + String.Join(", ", new[] { s.Location?.City, s.Location?.Postcode, s.Location?.Country }.Where(w => !string.IsNullOrEmpty(w)));
        }
    }
}
