using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.Text;
using System.Web.Http;

namespace OrderItemsReserverService
{
    record class OrderItem(string itemId, int quantity);
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("OrderItemsReserver function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            OrderItem[] data = JsonConvert.DeserializeObject<OrderItem[]>(requestBody);

            if (data is not null && data.Length > 0)
            {
                try
                {
                    string content = JsonConvert.SerializeObject(data, Formatting.None);
                    string fileName = $"OrderRequest-{Guid.NewGuid()}.json";
                    await SaveBlob(content, fileName);
                    return new OkResult();
                }
                catch (Exception)
                {
                    return new InternalServerErrorResult();
                }
            }
            return new BadRequestObjectResult("Could not process item, itemId and/or quantity not found!");
        }
        private static async Task SaveBlob(string content, string fileName)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("hw3container");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content), false);
            await blobClient.UploadAsync(stream);
        }
    }
}
