namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class OrderUnitController : ControllerBase
{
  private readonly IOrderUnitService _orderUnitService;

  public OrderUnitController(IOrderUnitService orderUnitService)
  {
    _orderUnitService = orderUnitService;
  }

  // GET api/OrderUnit/GetAllOrderUnits
  [HttpGet]
  [Route("GetAllOrderUnits")]
  public async Task<IActionResult> GetAllOrderUnits()
  {
    IList<OrderUnit> orderUnits = await _orderUnitService.GetAllOrderUnits();
    return Ok(orderUnits);
  }

  // POST api/OrderUnit/AddOrderUnit
  [HttpPost]
  [Route("AddOrderUnit")]
  public async Task<IActionResult> AddOrderUnit([FromBody] OrderUnit orderUnit)
  {
    bool isAdded = await _orderUnitService.AddOrderUnit(orderUnit);
    return Ok(isAdded);
  }

  // POST api/OrderUnit/UpdateOrderUnit
  [HttpPost]
  [Route("UpdateOrderUnit")]
  public async Task<IActionResult> UpdateOrderUnit([FromBody] OrderUnit orderUnit)
  {
    bool isUpdated = await _orderUnitService.UpdateOrderUnit(orderUnit);
    return Ok(isUpdated);
  }

  // POST api/OrderUnit/DeleteOrderUnit
  [HttpPost]
  [Route("DeleteOrderUnit")]
  public async Task<IActionResult> DeleteOrderUnit([FromBody] string orderUnitId)
  {
    bool isDeleted = await _orderUnitService.DeleteOrderUnit(orderUnitId);
    return Ok(isDeleted);
  }
}
