namespace InfonetCSCR.Core.Abstractions.Services;

public interface IOrderUnitService
{
  Task<IList<OrderUnit>> GetAllOrderUnits();
  Task<bool> AddOrderUnit(OrderUnit orderUnit);
  Task<bool> UpdateOrderUnit(OrderUnit orderUnit);
  Task<bool> DeleteOrderUnit(string orderUnitId);
}
