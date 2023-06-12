using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderDeliveryServices : IOrderDeliveryServices
{
    private readonly IAppLogger<OrderDeliveryServices> _logger;
    private readonly string _apiUrl;

    public OrderDeliveryServices(
        IOptions<BaseUrlConfiguration> baseUrlConfiguration,
        IAppLogger<OrderDeliveryServices> logger)
    {
        _logger = logger;
        _apiUrl = baseUrlConfiguration.Value.GhHw4Funcapp;
    }

    record class OrderItem(string itemId, int quantity);
    record class Order(Address address, OrderItem[] items, decimal finalPrice);

    public async Task SendOrder(Address address, IDictionary<string, int> orderDetails, decimal finalPrice)
    {
        var http = new HttpClient();
        var orderItems = orderDetails.Select(s => new OrderItem(s.Key,s.Value)).ToArray();
        var order = new Order(address, orderItems, finalPrice);

        var result = await http.PostAsJsonAsync($"{_apiUrl}OrderDelivery", order);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Order is not sent to delivery: {result.ReasonPhrase}");
        }
    }
}
