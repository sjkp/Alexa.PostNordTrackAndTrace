using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Alexa.PostnordTrackAndTrace
{
    public class ShipmentRepo : TableRepo<Shipment>
    {
        public ShipmentRepo(string connectionStrin) : base("shipments", connectionStrin)
        {

        }

        public ShipmentRepo() : base("shipments", Environment.GetEnvironmentVariable("AzureTableConnectionString"))
        {

        }
    }

    public class TableRepo<T>
    {
        private Lazy<Task<CloudTable>> table;
        private string cloudTableName;
        private string connectionString;

        public TableRepo(string cloudTableName, string connectionString)
        {
            this.cloudTableName = cloudTableName;
            this.connectionString = connectionString;
            table = new Lazy<Task<CloudTable>>(async () =>
            {
                if (string.IsNullOrEmpty(this.cloudTableName))
                {
                    throw new ArgumentNullException("Table", "Table Name can't be empty");
                }
                try
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                    var table = tableClient.GetTableReference(cloudTableName);
                    await table.CreateIfNotExistsAsync();
                    return table;
                }
                catch (StorageException StorageExceptionObj)
                {
                    throw StorageExceptionObj;
                }
                catch (Exception ExceptionObj)
                {
                    throw ExceptionObj;
                }
            });
        }

        public async Task InsertEntity<T>(T entity, bool insertOrReplace = true) where T : TableEntity, new()
        {
            try
            {
                if (insertOrReplace)
                {
                    var insertOrMergeOperation = TableOperation.InsertOrReplace(entity);
                    await (await table.Value).ExecuteAsync(insertOrMergeOperation);
                }
                else
                {
                    var insertOperation = TableOperation.Insert(entity);
                    await (await table.Value).ExecuteAsync(insertOperation);
                    
                }
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

        public async Task<List<T>> RetrieveEntity<T>(string Query = null) where T : TableEntity, new()
        {
            try
            {
                // Create the Table Query Object for Azure Table Storage  
                TableQuery<T> DataTableQuery = new TableQuery<T>();
                if (!String.IsNullOrEmpty(Query))
                {
                    DataTableQuery = new TableQuery<T>().Where(Query);
                }
                IEnumerable<T> IDataList = await (await table.Value).ExecuteQuerySegmentedAsync(DataTableQuery, default(TableContinuationToken));
                List<T> DataList = new List<T>();
                foreach (var singleData in IDataList)
                    DataList.Add(singleData);
                return DataList;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }
    }
}
