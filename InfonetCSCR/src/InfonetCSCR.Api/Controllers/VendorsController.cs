namespace InfonetCSCR.Api.Controllers;

[Route("api/vendors")]
[ApiController]
[Authorize]

public class VendorsController : ControllerBase
{
  private readonly IVendorService _vendorService;

  public VendorsController(IVendorService vendorService)
  {
    _vendorService = vendorService;
  }

  [HttpGet]
  public async Task<IActionResult> GetVendorByCode(string code)
  {
    IEnumerable<Vendor> vendors = await _vendorService.GetVendorByCodeAsync(code);
    return Ok(vendors);
  }

  // GET api/vendors/all
  [HttpGet]
  [Route("all")]
  public async Task<IActionResult> GetAllVendors()
  {
    IEnumerable<Vendor> vendors = await _vendorService.GetAllVendorsAsync();
    return Ok(vendors);
  }

  // GET api/vendors/GetActiveVendorsOrderByColumnNumber/{columnNumber}
  [HttpGet]
  [Route("GetActiveVendorsOrderByColumnNumber/{columnNumber}")]
  public async Task<IActionResult> GetActiveVendorsOrderByColumnNumber(int columnNumber)
  {
    IEnumerable<Vendor> vendors = await _vendorService.GetActiveVendorsOrderByColumnNumber(columnNumber);
    return Ok(vendors);
  }

  // GET api/vendors/GetVendorByPurchaseNo
  [HttpGet]
  [Route("GetVendorByPurchaseNo")]
  public async Task<IActionResult> GetVendorByPurchaseNo(long purchaseNo)
  {
    Vendor? vendor = await _vendorService.GetVendorByPurchaseNo(purchaseNo);
    return Ok(vendor);
  }


  // GET api/vendors/GetVendorByVendorCode/{code}
  [HttpGet]
  [Route("GetVendorByVendorCode/{code}")]
  public async Task<IActionResult> GetVendorByVendorCode(string code)
  {
    Vendor vendor = await _vendorService.GetVendorByVendorCode(code);
    return Ok(vendor);
  }

  // GET api/vendors/GetVendorSortSetup
  [HttpGet]
  [Route("GetVendorSortSetup")]
  public async Task<IActionResult> GetVendorSortSetup()
  {
    return Ok(await _vendorService.GetVendorsSortSetup());
  }

  // GET api/vendors/GetVendorSort/vendorId
  [HttpGet]
  [Route("GetVendorSort")]
  public async Task<IActionResult> GetVendorSort(string vendorId)
  {
    return Ok(await _vendorService.GetVendorsSort(vendorId));
  }

  // Post api/vendors/AddVendor
  [HttpPost]
  [Route("AddVendor")]
  public async Task<IActionResult> AddVendor(VendorAddRecord vendorAdd)
  {
    return Ok(await _vendorService.SaveVendor(vendorAdd, true));
  }

  // Post api/vendors/UpdateVendor
  [HttpPost]
  [Route("UpdateVendor")]
  public async Task<IActionResult> UpdateVendor(VendorAddRecord vendorAdd)
  {
    return Ok(await _vendorService.SaveVendor(vendorAdd, false));
  }

  // Post api/vendors/DeleteVendor
  [HttpPost]
  [Route("DeleteVendor")]
  public async Task<IActionResult> DeleteVendor([FromBody] string vendorId)
  {
    return Ok(await _vendorService.DeleteVendor(vendorId));
  }

  // Get api/vendors/CheckDeleteValidation/vendorId
  [HttpGet]
  [Route("CheckDeleteValidation/{vendorId}")]
  public async Task<IActionResult> CheckDeleteValidation(string vendorId)
  {
    return Ok(await _vendorService.CheckDeleteValidation(vendorId));
  }

  // Get api/vendors/GetAllVendorsName
  [HttpGet]
  [Route("GetAllVendorsName")]
  public async Task<IActionResult> GetAllVendorsName()
  {
    return Ok(await _vendorService.GetAllVendorsNameAsync());
  }

  // Get api/vendors/GetVendorLink/vendorId
  [HttpGet]
  [Route("GetVendorLink/{vendorId}")]
  public async Task<IActionResult> GetVendorLink(string vendorId)
  {
    return Ok(await _vendorService.GetVendorLinksAsync(vendorId));
  }

  // Get api/vendors/GetAllStoresAndRegions
  [HttpGet]
  [Route("GetAllStoresAndRegions")]
  public async Task<IActionResult> GetAllStoresAndRegions()
  {
    return Ok(await _vendorService.GetAllStoresAndRegions());
  }

  // Post api/vendors/DeleteVendorLink
  [HttpPost]
  [Route("DeleteVendorLink")]
  public async Task<IActionResult> DeleteVendorLink([FromBody] VendorLink vendorLink)
  {
    return Ok(await _vendorService.DeleteVendorLink(vendorLink));
  }

  // Post api/vendors/AddVendorLink
  [HttpPost]
  [Route("AddVendorLink")]
  public async Task<IActionResult> AddVendorLink([FromBody] VendorLink vendorLink)
  {
    return Ok(await _vendorService.AddVendorLink(vendorLink));
  }

  // GET api/vendors/GetVendorsOrderByColumnNumber/{columnNumber}
  [HttpGet]
  [Route("GetVendorsOrderByColumnNumber/{columnNumber}")]
  public async Task<IActionResult> GetVendorsOrderByColumnNumber(int columnNumber)
  {
    IEnumerable<Vendor> vendors = await _vendorService.GetVendorsOrderByColumnNumber(columnNumber);
    return Ok(vendors);
  }
}
