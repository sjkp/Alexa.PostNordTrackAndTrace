using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alexa.PostnordTrackAndTrace.Tests
{
    [TestClass]
    public class ShipmentRepoTest
    {

        [TestMethod]
        public async Task TestEmpty()
        {
            var builder = new ConfigurationBuilder()
         .AddUserSecrets<ShipmentRepoTest>();

            var Configuration = builder.Build();
            var repo = new ShipmentRepo(Configuration["AzureTableConnectionString"]);

            var entity = await repo.RetrieveEntity<Shipment>("PartitionKey eq 'empty'");

            Assert.AreEqual(null, entity.FirstOrDefault());
        }

        [TestMethod]
        public async Task TestInsertOrMerge()
        {
            var builder = new ConfigurationBuilder()
         .AddUserSecrets<ShipmentRepoTest>();

            var Configuration = builder.Build(); 
            var repo = new ShipmentRepo(Configuration["AzureTableConnectionString"]);

            await repo.InsertEntity(new Shipment("testuser", "5505"), true);

            var entity = await repo.RetrieveEntity<Shipment>("PartitionKey eq 'testuser'");

            Assert.AreEqual("5505", entity.FirstOrDefault().ShipmentId);
        }


        [TestMethod]
        public async Task GetLatest()
        {
            var builder = new ConfigurationBuilder()
         .AddUserSecrets<ShipmentRepoTest>();

            var Configuration = builder.Build();
            var repo = new ShipmentRepo(Configuration["AzureTableConnectionString"]);

            await repo.InsertEntity(new Shipment("testuser", "first"), true);
            await Task.Delay(200);
            await repo.InsertEntity(new Shipment("testuser", "last"), true);

            var entity = await repo.RetrieveEntity<Shipment>("PartitionKey eq 'testuser'");

            Assert.AreEqual("last", entity.FirstOrDefault().ShipmentId);
        }
    }
}
