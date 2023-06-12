using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IOrderItemsReserverService
{
    Task ReserveItems(IDictionary<string, int> orderDetails);
}
