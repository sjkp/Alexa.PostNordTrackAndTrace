using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Alexa.PostnordTrackAndTrace
{
    public class Shipment : TableEntity
    {
        public Shipment()
        {

        }
        public Shipment(string user, string shipmentId) : base(user, (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"))
        {
            ShipmentId = shipmentId;
        }

        public string User
        {
            get
            {
                return this.PartitionKey;
            }
        }

        public string ShipmentId
        {
            get; set;
        }


    }
}