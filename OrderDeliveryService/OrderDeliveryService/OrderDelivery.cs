using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Web.Http;
using Microsoft.Azure.Cosmos;

namespace OrderDeliveryService
{
    record class OrderItem(string itemId, int quantity);
    record class Address(string street, string city, string state, string country, string zipCode);
    record class Order(string id, Address address, OrderItem[] items, decimal finalPrice) 
    {
        public string id { get; init; } = Guid.NewGuid().ToString("N");
    };

    public static class OrderDelivery
    {
        [FunctionName("OrderDelivery")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("OrderDelivery function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);

            if (order is not null)
            {
                try
                {
                    await SaveToCosmosDb(order);
                    return new OkResult();
                }
                catch (Exception)
                {
                    return new InternalServerErrorResult();
                }
            }

            return new BadRequestObjectResult("Could not process order");
        }

        private static async Task SaveToCosmosDb(Order order)
        {
            string cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionSting");

            CosmosClient cosmosClient = new CosmosClient(cosmosConnectionString);
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("OrderDelivery");
            Container container = await database.CreateContainerIfNotExistsAsync("Orders", "/id");
            var response = await container.UpsertItemAsync(order);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception();
            }
        }
    }
}
