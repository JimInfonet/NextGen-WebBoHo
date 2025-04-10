namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class AddItemsToThePOController : Controller
{
  private readonly IHttpService _http;

  public AddItemsToThePOController(IHttpService http)
  {
    _http = http;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetVendorParts([FromQuery] string vendorId, [FromQuery] AddItemsToThePOSelectedCriteria selectedCriteria)
  {
    string url = $"AddItemsToThePO/GetVendorParts?vendorId={vendorId}&selectedCriteria={selectedCriteria}";
    var vendorParts = await _http.GetAndParseAsync<List<VendorPartDetailsDtoViewModel>>(url);
    return Ok(vendorParts);
  }

  public async Task<IActionResult> GetExistingPartNos([FromBody] List<string> partNos, [FromQuery] string purchaseNo)
  {
    string url = $"AddItemsToThePO/GetExistingPartNos?purchaseNo={purchaseNo}";
    var existingPartNos = await _http.PostAndParseAsync<List<string>>(url, partNos);
    return Ok(existingPartNos);
  }

  public async Task<IActionResult> AddVendorParts([FromBody] AddVendorPartsDtoViewModel model)
  {
    string url = "AddItemsToThePO/AddVendorParts";
    bool added = await _http.PostAndParseAsync<bool>(url, model);
    return Ok(added);
  }
}
