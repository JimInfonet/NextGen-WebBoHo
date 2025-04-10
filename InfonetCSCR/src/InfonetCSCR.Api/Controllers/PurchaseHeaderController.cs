namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class PurchaseHeaderController : ControllerBase
{
  private readonly IPurchaseHeaderService _purchaseHeaderService;

  public PurchaseHeaderController(IPurchaseHeaderService purchaseHeaderService)
  {
    _purchaseHeaderService = purchaseHeaderService;
  }

  // GET api/PurchaseHeader/GetPurchaseHeadersByPOStatus/{poStatus}/{columnIndex}
  [HttpGet]
  [Route("GetPurchaseHeadersByPOStatus/{poStatus}/{columnIndex}")]
  public async Task<IActionResult> GetPurchaseHeadersByPOStatus(string poStatus, int columnIndex)
  {
    IEnumerable<PurchaseHeader>? purchaseHeaders = await _purchaseHeaderService.GetPurchaseHeadersByPOStatus(poStatus, columnIndex);
    return Ok(purchaseHeaders);
  }

  // POST api/PurchaseHeader/UpdatePurchaseHeaderEmailedStatus
  [HttpPost]
  [Route("UpdatePurchaseHeaderEmailedStatus")]
  public async Task<IActionResult> UpdatePurchaseHeaderEmailedStatus([FromBody] PurchaseHeader purchaseheader)
  {
    bool isUpdated = await _purchaseHeaderService.UpdatePurchaseHeaderEmailedStatus(purchaseheader);
    return Ok(isUpdated);
  }

  // GET api/PurchaseHeader/GetPurchaseHeadersByPurchaseNo/{purchaseNo}
  [HttpGet]
  [Route("GetPurchaseHeadersByPurchaseNo/{purchaseNo}")]
  public async Task<IActionResult> GetPurchaseHeadersByPurchaseNo(string purchaseNo)
  {
    PurchaseHeader? purchaseHeader = await _purchaseHeaderService.GetPurchaseHeadersByPurchaseNo(purchaseNo);
    return Ok(purchaseHeader);
  }

  // POST api/PurchaseHeader/UpdatePurchaseHeader
  [HttpPost]
  [Route("UpdatePurchaseHeader")]
  public async Task<IActionResult> UpdatePurchaseHeader([FromBody] PurchaseHeaderUpdateRecord purchaseHeaderUpdateRecord)
  {
    bool isUpdated = await _purchaseHeaderService.UpdatePurchaseHeader(purchaseHeaderUpdateRecord);
    return Ok(isUpdated);
  }

  // POST api/PurchaseHeader/UpdatePurchaseHeaderSurChargeDiscount
  [HttpPost]
  [Route("UpdatePurchaseHeaderSurChargeDiscount")]
  public async Task<IActionResult> UpdatePurchaseHeaderSurChargeDiscount([FromBody] PurchaseHeader purchaseHeader)
  {
    bool isUpdated = await _purchaseHeaderService.UpdatePurchaseHeaderSurChargeDiscount(purchaseHeader);
    return Ok(isUpdated);
  }

  // POST api/PurchaseHeader/POPayment
  [HttpPost]
  [Route("POPayment")]
  public async Task<IActionResult> POPayment([FromBody] PurchaseAndReceivePostDto purchaseAndReceivePost)
  {
    string content = await _purchaseHeaderService.POPayment(purchaseAndReceivePost);
    return Ok(content);
  }

  // POST api/PurchaseHeader/GetPOTransferFilesData/headerFile/detailFile
  [HttpPost]
  [Route("GetPOTransferFilesData/{headerFile}/{detailFile}")]
  public async Task<IActionResult> GetPOTransferFilesData([FromBody] long purchaseNo, string headerFile, string detailFile)
  {
    TransferPOFileDto? purchaseDetail = await _purchaseHeaderService.GetPOTransferFilesData(purchaseNo, headerFile, detailFile);
    return Ok(purchaseDetail);
  }
}
