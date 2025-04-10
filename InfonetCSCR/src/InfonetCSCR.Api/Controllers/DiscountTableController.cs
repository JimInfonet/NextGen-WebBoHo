namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class DiscountTableController : ControllerBase
{
  private readonly IDiscountTableService _discountTableService;

  public DiscountTableController(IDiscountTableService discountTableService)
  {
    _discountTableService = discountTableService;
  }

  // POST api/DiscountTable/GetAllDiscountTableData
  [HttpPost]
  [Route("GetAllDiscountTableData")]
  public async Task<IActionResult> GetAllDiscountTableData(DiscountTablePolicyRecord discountTablePolicyRecord)
  {
    IList<DiscountTable> discountTableData = await _discountTableService.GetAllDiscountTableData(discountTablePolicyRecord);
    return Ok(discountTableData);
  }

  // POST api/DiscountTable/InsertDataOnDiscountTable
  [HttpPost]
  [Route("InsertDataOnDiscountTable")]
  public async Task<IActionResult> InsertDataOnDiscountTable(DiscountTable discountTable)
  {
    bool isAdded = await _discountTableService.InsertDataOnDiscountTable(discountTable);
    return Ok(isAdded);
  }

  // POST api/DiscountTable/UpdateDiscountPercentage
  [HttpPost]
  [Route("UpdateDiscountPercentage")]
  public async Task<IActionResult> UpdateDiscountPercentage(DiscountTable discountTable)
  {
    bool isUpdated = await _discountTableService.UpdateDiscountPercentage(discountTable);
    return Ok(isUpdated);
  }

  // POST api/DiscountTable/DeleteDiscount
  [HttpPost]
  [Route("DeleteDiscount")]
  public async Task<IActionResult> DeleteDiscount(DiscountTable discountTable)
  {
    bool isDeleted = await _discountTableService.DeleteDiscount(discountTable);
    return Ok(isDeleted);
  }
}
