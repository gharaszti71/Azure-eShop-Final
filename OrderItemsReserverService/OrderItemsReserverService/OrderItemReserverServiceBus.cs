using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace OrderItemsReserverService
{
    record class QueueOrderItem(string itemId, int quantity);

    public class OrderItemReserverServiceBus
    {
        [FunctionName("OrderItemReserverServiceBus")]
        public async Task Run([ServiceBusTrigger("orderitemsreserverqueue", Connection = "ServicebusConnection")]
            string myQueueItem, ILogger log)
        {
            QueueOrderItem[] data = JsonConvert.DeserializeObject<QueueOrderItem[]>(myQueueItem);

            if (data is not null && data.Length > 0)
            {
                string content = JsonConvert.SerializeObject(data, Formatting.None);
                
                try
                {
                    ThrowRandomly();
                    string fileName = $"OrderRequest-{Guid.NewGuid()}.json";
                    await SaveBlob(content, fileName, log);
                }
                catch (Exception ex)
                {
                    log.LogWarning(Events.FallbackNeed, ex, "Save blob failed, move to fallback");
                    await SendMessageToFallback(content, log);
                }
            }
        }

        private void ThrowRandomly()
        {
            if (Random.Shared.Next(0, 2) == 0) throw new Exception();
        }

        private static async Task SaveBlob(string content, string fileName, ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("hwfinalcontainer");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content), false);
            await blobClient.UploadAsync(stream);
        }

        private static async Task SendMessageToFallback(string myQueueItem, ILogger log)
        {
            await using var client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServicebusConnection"));
            await using ServiceBusSender sender = client.CreateSender("orderitemsreserverfallbackqueue");

            try
            {
                var message = new ServiceBusMessage(myQueueItem);
                await sender.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                log.LogError(Events.FallbackFailed, ex, "Fallback failed");
                throw;
            }
        }
    }
}
