namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class PurchasingAndReceivingController : ControllerBase
{
  private readonly IPurchasingAndReceivingService _purchasingAndReceivingService;

  public PurchasingAndReceivingController(IPurchasingAndReceivingService purchasingAndReceivingService)
  {
    _purchasingAndReceivingService = purchasingAndReceivingService;
  }

  // POST api/PurchasingAndReceiving/AddPurchasingAndReceiving
  [HttpPost]
  [Route("AddPurchasingAndReceiving")]
  public async Task<IActionResult> AddPurchasingAndReceiving(PurchasingAndReceivingDto purchasingAndReceivingDto)
  {
    bool isAdded = await _purchasingAndReceivingService.AddPurchasingAndReceiving(
      purchasingAndReceivingDto,
      purchasingAndReceivingDto.Policies
      );
    return Ok(isAdded);
  }

  // POST api/PurchasingAndReceiving/DeletePurchasingAndReceivingByPurchaseNo/{purchaseNo}
  [HttpPost]
  [Route("DeletePurchasingAndReceivingByPurchaseNo/{purchaseNo}")]
  public async Task<IActionResult> DeletePurchasingAndReceivingByPurchaseNo(string purchaseNo)
  {
    bool isDeleted = await _purchasingAndReceivingService.DeletePurchasingAndReceivingByPurchaseNo(purchaseNo);
    return Ok(isDeleted);
  }

  // Get api/PurchasingAndReceiving/GetPurchasingAndReceivingReportData/{purchaseNo}
  [HttpGet]
  [Route("GetPurchasingAndReceivingReportData/{purchaseNo}")]
  public async Task<IActionResult> GetPurchasingAndReceivingReportData(int purchaseNo)
  {
    IEnumerable<PurchasingAndReceivingReportDto> data = await _purchasingAndReceivingService.GetPurchasingAndReceivingReportData(purchaseNo);
    return Ok(data);
  }
}
