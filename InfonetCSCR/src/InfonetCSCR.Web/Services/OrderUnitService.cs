namespace InfonetCSCR.Web.Services;

public class OrderUnitService : IOrderUnitService
{
  private readonly IHttpService _http;

  public OrderUnitService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IList<OrderUnitViewModel>?> GetAllOrderUnits()
  {
    string url = "OrderUnit/GetAllOrderUnits";
    IList<OrderUnitViewModel>? orderUnits = await _http.GetAndParseAsync<IList<OrderUnitViewModel>>(url);
    return orderUnits;
  }

  public async Task<bool> AddOrderUnit(OrderUnitViewModel orderUnit)
  {
    string url = $"OrderUnit/AddOrderUnit";
    var isOrderUnitAddSuccess = await _http.PostAndParseAsync<bool>(url, orderUnit);
    return isOrderUnitAddSuccess;
  }

  public async Task<bool> UpdateOrderUnit(OrderUnitViewModel orderUnit)
  {
    string url = $"OrderUnit/UpdateOrderUnit";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, orderUnit);
    return isUpdated;
  }

  public async Task<bool> DeleteOrderUnit(string orderUnitId)
  {
    string url = $"OrderUnit/DeleteOrderUnit";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, orderUnitId);
    return isDeleted;
  }
}
