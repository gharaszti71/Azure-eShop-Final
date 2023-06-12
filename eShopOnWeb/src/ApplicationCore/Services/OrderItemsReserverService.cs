using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public record class OrderItemDTO(string itemId, int quantity);

public class OrderItemsReserverService : IOrderItemsReserverService
{
    private readonly string _serviceBusConnection;
    private readonly IAppLogger<OrderItemsReserverService> _logger;

    const string QueueName = "orderitemsreserverqueue";

    public OrderItemsReserverService(
        IOptions<ServicebusConnectionConfiguration> serviceBusConnection,
        IAppLogger<OrderItemsReserverService> logger)
    {
        _serviceBusConnection = serviceBusConnection.Value.ServicebusConnection 
            ?? throw new Exception("ServicebusConnection must be set");
        _logger = logger;
    }

    public async Task ReserveItems(IDictionary<string, int> orderDetails)
    {
        await using var client = new ServiceBusClient(_serviceBusConnection);
        await using ServiceBusSender sender = client.CreateSender(QueueName);

        var orderItems = orderDetails.Select(s => new OrderItemDTO(s.Key, s.Value)).ToArray();
        string messageBody = JsonSerializer.Serialize(orderItems);
        var message = new ServiceBusMessage(messageBody);

        await sender.SendMessageAsync(message);
    }
}
