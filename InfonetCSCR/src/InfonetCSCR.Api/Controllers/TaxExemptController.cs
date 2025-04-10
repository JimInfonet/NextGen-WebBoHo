namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class TaxExemptController : ControllerBase
{
  private readonly ITaxExemptCategoryService _taxExemptCategoryService;
  private readonly ITaxExemptVendorsService _taxExemptVendorsService;
  private readonly ITaxExemptRateService _taxExemptRateService;
  private readonly IProductTaxExemptService _productTaxExemptService;

  public TaxExemptController(
      ITaxExemptCategoryService taxExemptCategoryService,
      ITaxExemptVendorsService taxExemptVendorsService,
      ITaxExemptRateService taxExemptRateService,
      IProductTaxExemptService productTaxExemptService)
  {
    _taxExemptCategoryService = taxExemptCategoryService;
    _taxExemptVendorsService = taxExemptVendorsService;
    _taxExemptRateService = taxExemptRateService;
    _productTaxExemptService = productTaxExemptService;
  }

  // GET api/TaxExempt/GetAllTaxExemptCategory
  [HttpGet]
  [Route("GetAllTaxExemptCategory")]
  public async Task<IActionResult> GetAllTaxExemptCategory()
  {
    IEnumerable<TaxExemptCategory> taxExemptCategories = await _taxExemptCategoryService.GetAllTaxExemptCategory();
    return Ok(taxExemptCategories);
  }

  // GET api/TaxExempt/GetAllTaxExemptVendorsByStockCode/{stockCode}
  [HttpGet]
  [Route("GetAllTaxExemptVendorsByStockCode/{stockCode}")]
  public async Task<IActionResult> GetAllTaxExemptVendorsByStockCode(string stockCode)
  {
    IEnumerable<TaxExemptVendor> taxExemptCategories = await _taxExemptVendorsService.GetAllTaxExemptVendors(stockCode);
    return Ok(taxExemptCategories);
  }

  // GET api/TaxExempt/GetAllTaxExemptRate
  [HttpGet]
  [Route("GetAllTaxExemptRate")]
  public async Task<IActionResult> GetAllTaxExemptRate()
  {
    IEnumerable<TaxExemptRate> taxExemptRates = await _taxExemptRateService.GetAllTaxExemptRate();
    return Ok(taxExemptRates);
  }

  // GET api/TaxExempt/GetProductTaxExemptByProductKey/{productKey}
  [HttpGet]
  [Route("GetProductTaxExemptByProductKey/{productKey}")]
  public async Task<IActionResult> GetProductTaxExemptByProductKey(string productKey)
  {
    ProductTaxExemptDto? productTaxExempt = await _productTaxExemptService.GetProductTaxExemptByProductKey(productKey);
    return Ok(productTaxExempt);
  }

  // POST api/TaxExempt/AddProductTaxExempt
  [HttpPost]
  [Route("AddProductTaxExempt")]
  public async Task<IActionResult> AddProductTaxExempt(ProductTaxExempt productTaxExempt)
  {
    bool isProductTaxExemptAdded = await _productTaxExemptService.AddProductTaxExempt(productTaxExempt);
    return Ok(isProductTaxExemptAdded);
  }

  // POST api/TaxExempt/UpdateProductTaxExempt
  [HttpPost]
  [Route("UpdateProductTaxExempt")]
  public async Task<IActionResult> UpdateProductTaxExempt(ProductTaxExempt productTaxExempt)
  {
    bool isProductTaxExemptUpdated = await _productTaxExemptService.UpdateProductTaxExempt(productTaxExempt);
    return Ok(isProductTaxExemptUpdated);
  }

  // POST api/TaxExempt/DeleteProductTaxExemptByProductKey
  [HttpPost]
  [Route("DeleteProductTaxExemptByProductKey")]
  public async Task<IActionResult> DeleteProductTaxExemptByProductKey([FromBody] string productKey)
  {
    bool isProductTaxExemptDeleted = await _productTaxExemptService.DeleteProductTaxExemptByStockCode(productKey);
    return Ok(isProductTaxExemptDeleted);
  }

  // GET api/TaxExempt/CalculateTaxExemptPrice?stockCode=stockCode&taxCode=taxCode&taxExemptCategory=taxExemptCategory&quantityPerPkg=quantityPerPkg&isUseFuelPolicy=isUseFuelPolicy
  [HttpGet]
  [Route("CalculateTaxExemptPrice")]
  public async Task<IActionResult> CalculateTaxExemptPrice(
    string stockCode,
    int taxCode,
    int taxExemptCategory,
    int quantityPerPkg,
    bool isUseFuelPolicy)
  {
    double taxExemptPrice = await _productTaxExemptService.CalculateTaxExemptPrice(
      stockCode,
      taxCode,
      taxExemptCategory,
      quantityPerPkg,
      isUseFuelPolicy);
    return Ok(taxExemptPrice);
  }

  // POST api/TaxExempt/AddProductTaxExemptForDepartmentSubDepartmentAndDetail
  [HttpPost]
  [Route("AddProductTaxExemptForDepartmentSubDepartmentAndDetail")]
  public async Task<IActionResult> AddProductTaxExemptForDepartmentSubDepartmentAndDetail(TaxExemptDto taxExemptCategory)
  {
    bool isAdded = await _productTaxExemptService.AddProductTaxExemptForDepartmentSubDepartmentAndDetail(taxExemptCategory);
    return Ok(isAdded);
  }

  // POST api/TaxExempt/DeleteProductTaxExempt
  [HttpPost]
  [Route("DeleteProductTaxExempt")]
  public async Task<IActionResult> DeleteProductTaxExempt([FromBody] DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    bool isDeleted = await _productTaxExemptService.DeleteProductTaxExempt(departmentSubDepartmentAndDetail);
    return Ok(isDeleted);
  }

  // GET api/TaxExempt/GetTaxExemptReasonsListByType
  [HttpGet]
  [Route("GetTaxExemptReasonsListByType")]
  public async Task<IActionResult> GetTaxExemptReasonsListByType(string reasonType)
  {
    return Ok(await _taxExemptCategoryService.GetTaxExemptReasonsListByTypeAsync(reasonType));
  }
}
