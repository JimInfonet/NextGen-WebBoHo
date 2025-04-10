namespace InfonetCSCR.Services;

public class OrderUnitService : IOrderUnitService
{
  private readonly IUnitOfWork _unitOfWork;

  public OrderUnitService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<OrderUnit>> GetAllOrderUnits()
  {
    IList<OrderUnit> orderUnits = await _unitOfWork.OrderUnit.GetAllOrderUnitsAsync();
    _unitOfWork.Commit();
    return orderUnits;
  }

  public async Task<bool> AddOrderUnit(OrderUnit orderUnit)
  {
    bool isAdded = await _unitOfWork.OrderUnit.AddOrderUnitAsync(orderUnit);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateOrderUnit(OrderUnit orderUnit)
  {
    var isUpdated = await _unitOfWork.OrderUnit.UpdateOrderUnitAsync(orderUnit);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteOrderUnit(string orderUnitId)
  {
    var isDeleted = await _unitOfWork.OrderUnit.DeleteOrderUnitAsync(orderUnitId);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
