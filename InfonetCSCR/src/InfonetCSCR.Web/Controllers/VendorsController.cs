namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class VendorsController : Controller
{
  private readonly IVendorsService _vendorsService;

  public VendorsController(IVendorsService vendorsService, IApplicationConfig applicationConfig)
  {
    _vendorsService = vendorsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> VendorLink()
  {
    var result = await _vendorsService.GetAllStoresAndRegions();
    return View(result);
  }

  public async Task<IActionResult> VendorForm(string vendorId, int actionType = 0, bool showOnly = false)//actionType = 0 is default, 1 for added, 2 for updated
  {
    VendorAddRecordViewModel vendorModel;
    if (string.IsNullOrEmpty(vendorId))
    {
      vendorModel = await _vendorsService.AddVendorForm();
      ViewBag.IsAdd = true;
    }
    else
    {
      vendorModel = await _vendorsService.EditVendorForm(vendorId);
      ViewBag.IsAdd = false;
    }
    ViewBag.Action = actionType;

    ViewBag.ShowOnly = showOnly;
    return View(vendorModel);
  }

  public async Task<IActionResult> AddVendor(VendorAddRecordViewModel vendorModel)
  {
    var codeExists = await _vendorsService.CheckAndMakeCodeExistsMessage(vendorModel.Vendor.Code);
    if (!string.IsNullOrEmpty(codeExists))
    {
      ViewBag.IsAdd = true;
      ViewBag.CodeExists = codeExists;
      vendorModel.SortItems = (await _vendorsService.GetVendorSortSetup()).ToList();

      return View("VendorForm", vendorModel);
    }

    var result = await _vendorsService.SaveVendor(vendorModel, true);
    if(result)
    {
      return RedirectToAction("VendorForm", new { vendorId = vendorModel.Vendor.Code, showOnly = true, actionType = 1 });
    }
    else
    {
      ViewBag.IsAdd = true;
      ViewBag.ErrorOccured = true;

      vendorModel.SortItems = (await _vendorsService.GetVendorSortSetup()).ToList();

      return View("VendorForm", vendorModel);
    }
  }

  public async Task<IActionResult> UpdateVendor(VendorAddRecordViewModel vendorModel)
  {
    var result = await _vendorsService.SaveVendor(vendorModel, false);
    if (result)
    {
      return RedirectToAction("VendorForm", new { vendorId = vendorModel.Vendor.Code, showOnly = true, actionType = 2 });
    }
    else
    {
      ViewBag.IsAdd = false;
      ViewBag.ErrorOccured = true;

      vendorModel.SortItems = (await _vendorsService.GetVendorSortSetup()).ToList();

      return View("VendorForm", vendorModel);
    }
  }

  public async Task<IActionResult> CheckDeleteValidation(string vendorId)
  {
    return Ok(await _vendorsService.CheckDeleteValidation(vendorId));
  }

  public async Task<IActionResult> DeleteVendor(string vendorId)
  {
    return Ok(await _vendorsService.DeleteVendor(vendorId));
  }

  public IActionResult GetVendorsPolicies()
  {
    return Ok(_vendorsService.GetVendorsPolicies());
  }

  public async Task<IActionResult> GetVendors()
  {
    return Ok(await _vendorsService.GetVendors());
  }

  public async Task<IActionResult> GetAllVendorName()
  {
    return Ok(await _vendorsService.GetAllVendorName());
  }

  public async Task<IActionResult> GetVendorLink(string vendorId)
  {
    return Ok(await _vendorsService.GetVendorLink(vendorId));
  }

  public async Task<IActionResult> DeleteVendorLink([FromBody]VendorLinkViewModel vendorLink)
  {
    return Ok(await _vendorsService.DeleteVendorLink(vendorLink));
  }

  public async Task<IActionResult> AddVendorLink([FromBody]VendorLinkViewModel vendorLink)
  {
    return Ok(await _vendorsService.AddVendorLink(vendorLink));
  }
}
