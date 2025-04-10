namespace InfonetCSCR.Web.Abstractions.Services;

public interface IOrderUnitService
{
  Task<IList<OrderUnitViewModel>?> GetAllOrderUnits();
  Task<bool> AddOrderUnit(OrderUnitViewModel orderUnit);
  Task<bool> UpdateOrderUnit(OrderUnitViewModel orderUnit);
  Task<bool> DeleteOrderUnit(string orderUnitId);
}
