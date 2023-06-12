using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IOrderDeliveryServices
{
    Task SendOrder(Address address, IDictionary<string, int> orderDetails, decimal finalPrice);
}
