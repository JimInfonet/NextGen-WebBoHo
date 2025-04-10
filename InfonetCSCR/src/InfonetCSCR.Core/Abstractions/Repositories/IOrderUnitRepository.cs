namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IOrderUnitRepository
{
  Task<IList<OrderUnit>> GetAllOrderUnitsAsync();
  Task<bool> AddOrderUnitAsync(OrderUnit orderUnit);
  Task<bool> UpdateOrderUnitAsync(OrderUnit orderUnit);
  Task<bool> DeleteOrderUnitAsync(string orderUnitId);
}
