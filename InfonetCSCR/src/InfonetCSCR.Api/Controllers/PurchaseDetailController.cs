namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class PurchaseDetailController : ControllerBase
{
  private readonly IPurchaseDetailService _purchaseDetailsService;
  private readonly IStockBranchService _stockBranchService;
  private readonly IVendorPartService _vendorPartService;

  public PurchaseDetailController(
    IPurchaseDetailService purchaseHeaderService,
    IStockBranchService stockBranchService,
    IVendorPartService vendorPartService
    )
  {
    _purchaseDetailsService = purchaseHeaderService;
    _stockBranchService = stockBranchService;
    _vendorPartService = vendorPartService;
  }

  // GET api/PurchaseDetail/GetPOFileData
  [HttpGet]
  [Route("GetPOFileData")]
  public async Task<IActionResult> GetPOFileData(long purchaseNo)
  {
    POFileDto? pofileData = await _purchaseDetailsService.GetPOFileData(purchaseNo);
    return Ok(pofileData);
  }

  // GET api/PurchaseDetail/GetPurchaseDetailsByPurchaseNumber/{purchaseNo}
  [HttpGet]
  [Route("GetPurchaseDetailsByPurchaseNumber/{purchaseNo}")]
  public async Task<IActionResult> GetPurchaseDetailsByPurchaseNumber(string purchaseNo)
  {
    IEnumerable<PurchaseDetail> purchaseDetails = await _purchaseDetailsService.GetPurchaseDetailsByPurchaseNumber(long.Parse(purchaseNo));
    return Ok(purchaseDetails);
  }

  // POST api/PurchaseDetail/UpdatePurchaseDetail
  [HttpPost]
  [Route("UpdatePurchaseDetail")]
  public async Task<IActionResult> UpdatePurchaseDetail(PurchaseDetailUpdateRecord purchaseDetailRecord)
  {
    bool isUpdated = await _purchaseDetailsService.UpdatePurchaseDetail(
      purchaseDetailRecord.PurchaseDetail, purchaseDetailRecord.Policies, purchaseDetailRecord.VendorId);
    return Ok(isUpdated);
  }

  // POST api/PurchaseDetail
  [HttpPost]
  [Route("DeletePurchaseDetail")]
  public async Task<IActionResult> DeletePurchaseDetail(DeletePurchaseDetailParameterRecord deletePurchaseDetailParameter)
  {
    bool isDeleted = await _purchaseDetailsService.DeletePurchaseDetail(
      deletePurchaseDetailParameter.PurchaseNo,
      deletePurchaseDetailParameter.PartNumber,
      deletePurchaseDetailParameter.LineNumber);
    return Ok(isDeleted);
  }

  // GET api/PurchaseDetail/GetVendorUnitCostPercentage
  [HttpPost]
  [Route("GetVendorUnitCostPercentage")]
  public async Task<IActionResult> GetVendorUnitCostPercentage(PurchaseDetailUpdateRecord purchaseDetailRecord)
  {
    VendorUnitCostPercentageRecord vendorUnitCostPercentageRecord = await _purchaseDetailsService.GetVendorUnitCostPercentage(
      purchaseDetailRecord.PurchaseDetail, purchaseDetailRecord.Policies, purchaseDetailRecord.VendorId);
    return Ok(vendorUnitCostPercentageRecord);
  }

  // GET api/PurchaseDetail/GetVendorPartsFromVendorPartStockMstAndStockBr
  [HttpGet]
  [Route("GetVendorPartsFromVendorPartStockMstAndStockBr")]
  public async Task<IActionResult> GetVendorPartsFromVendorPartStockMstAndStockBr()
  {
    IEnumerable<VendorPartDto> vendorParts = await _vendorPartService.GetVendorPartsFromVendorPartStockMstAndStockBr();
    return Ok(vendorParts);
  }

  // GET api/PurchaseDetail/GetVendorPartFromVendorPartStockBrByPartNumber/{partNo)
  [HttpGet]
  [Route("GetVendorPartFromVendorPartStockBrByPartNumber/{partNo}")]
  public async Task<IActionResult> GetVendorPartFromVendorPartStockBrByPartNumber(string partNo)
  {
    VendorPartDto vendorPart = await _vendorPartService.GetVendorPartFromVendorPartStockBrByPartNumber(partNo);
    return Ok(vendorPart);
  }

  // POST api/PurchaseDetail/UpdateStockBranchReorderAndMaximumQuantity
  [HttpPost]
  [Route("UpdateStockBranchReorderAndMaximumQuantity")]
  public async Task<IActionResult> UpdateStockBranchReorderAndMaximumQuantity(StockBranch stockBranch)
  {
    bool isUpdated = await _stockBranchService.UpdateStockBranchReorderAndMaximumQuantity(stockBranch);
    return Ok(isUpdated);
  }

  // POST api/PurchaseDetail/UpdatePurchaseDetailGSTByPurchaseNO
  [HttpPost]
  [Route("UpdatePurchaseDetailGSTByPurchaseNO")]
  public async Task<IActionResult> UpdatePurchaseDetailGSTByPurchaseNO(IEnumerable<PurchaseDetail> purchaseDetails)
  {
    bool isUpdated = await _purchaseDetailsService.UpdatePurchaseDetailGSTByPurchaseNO(purchaseDetails);
    return Ok(isUpdated);
  }

  // POST api/PurchaseDetail/AddTaxToPurchaseDetail
  [HttpPost]
  [Route("AddTaxToPurchaseDetail")]
  public async Task<IActionResult> AddTaxToPurchaseDetail(AddTaxToPurchaseDetailParameterDto addTaxToPurchaseDetail)
  {
    bool isUpdated = await _purchaseDetailsService.AddTaxToPurchaseDetail(addTaxToPurchaseDetail);
    return Ok(isUpdated);
  }
}
