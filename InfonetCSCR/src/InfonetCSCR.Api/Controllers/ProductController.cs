namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class ProductController : ControllerBase
{
  private readonly IProductService _productService;
  private readonly IStockItemService _stockItemService;
  private readonly IStockBranchService _stockBranchService;
  private readonly IPLUMastService _plumastService;
  private readonly ICompanySetupService _companySetupService;
  private readonly ISaleService _saleService;
  private readonly IProductTaxExemptService _productTaxExemptService;
  private readonly IStockTransferService _stockTransferService;
  private readonly IApplicationConfig _applicationConfig;

  public ProductController(
      IProductService productService,
      IStockItemService stockItemService,
      IStockBranchService stockBranchService,
      IPLUMastService plumastService,
      IStockPriceService stockPriceService,
      ICompanySetupService companySetupService,
      IKitItemService kitItemService,
      IKitMasterService kitMasterService,
      ISaleService saleService,
      IProductTaxExemptService productTaxExemptService,
      IStockTransferService stockTransferService,
      IApplicationConfig applicationConfig)
  {
    _productService = productService;
    _stockItemService = stockItemService;
    _stockBranchService = stockBranchService;
    _plumastService = plumastService;
    _companySetupService = companySetupService;
    _saleService = saleService;
    _productTaxExemptService = productTaxExemptService;
    _stockTransferService = stockTransferService;
    _applicationConfig = applicationConfig;
  }

  // GET api/Product/GetProduct/{stockCode}
  [HttpGet]
  [Route("GetProduct/{stockCode}")]
  public async Task<IActionResult> GetProduct(string stockCode)
  {
    StockMaster stockMaster = await _stockItemService.GetStockMasterByStockCode(stockCode);
    StockBranch stockBranch = await _stockBranchService.GetStockBranch(stockCode);
    ProductTaxExempt productTaxExempt = _applicationConfig.IsBackOffice ?
                                        await _productTaxExemptService.GetProductTaxExempt(stockCode) : new ProductTaxExempt();
    Product product = ProductHelper.GetProductFromStockMasterAndStockBranch(stockMaster, stockBranch, productTaxExempt);
    return Ok(product);
  }

  // GET api/Product/SaleExistsForStockCode/{stockCode}
  [HttpGet]
  [Route("SaleExistsForStockCode/{stockCode}")]
  public async Task<IActionResult> SaleExistsForStockCode(string stockCode)
  {
    bool isSaleExistsForStockCode = await _saleService.SaleExistsForStockCode(stockCode);
    return Ok(isSaleExistsForStockCode);
  }

  // POST api/Product/SaveProduct
  [HttpPost]
  [Route("SaveProduct")]
  public async Task<IActionResult> SaveProduct([FromBody] Product product)
  {
    PLUMast? pluMast = await _plumastService.GetPLUMastByPLUCode(product.StockCode);
    StockMaster stockMaster = await _stockItemService.GetStockMasterByStockCode(product.StockCode);

    if (pluMast != null || stockMaster != null)
    {
      return Conflict(new { message = $"Stock code already exists" });
    }

    StockMaster stockMasterFromProduct = ProductHelper.GetStockMasterFromProduct(product);

    if (_applicationConfig.IsBackOffice == false)
    {
      stockMasterFromProduct.Transfer = true;
    }

    StockBranch stockBranchFromProduct = ProductHelper.GetStockBranchFromProduct(product);
    PLUMast pluMastFromProduct = ProductHelper.GetPLUMastFromProduct(product);
    Company? company = await _companySetupService.GetCompany();

    if (company != null && company.StoreCode != null)
    {
      stockBranchFromProduct.Branch = company.StoreCode;
    }
    else
    {
      stockBranchFromProduct.Branch = "01"; // Hard coded in old codebase
    }

    bool response = await _productService.AddProduct(stockMasterFromProduct, stockBranchFromProduct, pluMastFromProduct);

    return Ok(response);
  }

  // POST api/Product/CopyProduct/baseStockCode
  [HttpPost]
  [Route("CopyProduct/{baseStockCode}")]
  public async Task<IActionResult> CopyProduct(Product product, string baseStockCode)
  {
    PLUMast? pluMast = await _plumastService.GetPLUMastByPLUCode(product.StockCode);
    StockMaster stockMaster = await _stockItemService.GetStockMasterByStockCode(product.StockCode);

    if (pluMast != null || stockMaster != null)
    {
      return Conflict(new { message = $"Stock code already exists" });
    }

    StockMaster stockMasterFromProduct = ProductHelper.GetStockMasterFromProduct(product);

    if (_applicationConfig.IsBackOffice == false)
    {
      stockMasterFromProduct.Transfer = true;
    }

    StockBranch stockBranchFromProduct = ProductHelper.GetStockBranchFromProduct(product);
    PLUMast pluMastFromProduct = ProductHelper.GetPLUMastFromProduct(product);
    Company? company = await _companySetupService.GetCompany();

    if (company != null && company.StoreCode != null)
    {
      stockBranchFromProduct.Branch = company.StoreCode; // Branch is the store code from the company table
    }
    else
    {
      stockBranchFromProduct.Branch = "01"; // Hard coded in old codebase
    }

    bool response = await _productService.CopyProduct(
      stockMasterFromProduct,
      stockBranchFromProduct,
      pluMastFromProduct,
      baseStockCode);
    return Ok(response);
  }

  // POST api/Product/UpdateProduct
  [HttpPost]
  [Route("UpdateProduct")]
  public async Task<IActionResult> UpdateProduct([FromBody] Product product)
  {
    StockMaster stockMasterFromProduct = ProductHelper.GetStockMasterFromProduct((Product)product);
    StockBranch stockBranchFromProduct = ProductHelper.GetStockBranchFromProduct((Product)product);
    ProductTaxExempt taxExemptfromProduct = ProductHelper.GetTaxExemptfromProduct((Product)product);
    bool isProductUpdated = await _productService.UpdateProduct(
      stockMasterFromProduct,
      stockBranchFromProduct,
      taxExemptfromProduct);
    return Ok(isProductUpdated);
  }

  // POST api/Product/DeleteProduct
  [HttpPost]
  [Route("DeleteProduct")]
  public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductDto model)
  {
    if (model.BatchDelete && _applicationConfig.IsBackOffice == false)
    {
      bool isAddedStockTransferForBatchDelete = await _stockTransferService.AddStockTransferForBatchDelete(model.StockCode, model.UserCode);
    }

    if (model.UseTaxExempt)
    {
      bool isProductTaxExemptDeletedByStockCode = await _productTaxExemptService.DeleteProductTaxExemptByStockCode(model.StockCode);
    }

    bool response = await _productService.DeleteProduct(model.StockCode);
    return Ok(true);
  }

  // GET api/Product/GetPLUCodesByPLUPrime/{pluPrime}
  [HttpGet]
  [Route("GetPLUCodesByPLUPrime/{pluPrime}")]
  public async Task<IActionResult> GetPLUCodesByPLUPrime(string pluPrime)
  {
    IEnumerable<PLUMast>? pluCodes = await _plumastService.GetPLUCodesByPLUPrime(pluPrime);
    return Ok(pluCodes);
  }

  // POST api/Product/AddPLUCodeToPLUMast
  [HttpPost]
  [Route("AddPLUCodeToPLUMast")]
  public async Task<IActionResult> AddPLUCodeToPLUMast(PLUMast pluMast)
  {
    string result = await _plumastService.AddPLUMast(pluMast);

    if (result == "conflict")
    {
      return Conflict(new { message = $"PLUCode already exists" });
    }

    if (result == "success")
    {
      return Ok(true);
    }

    return Ok(false);
  }

  // POST api/Product/DeletePLUMastByPLUCode
  [HttpPost]
  [Route("DeletePLUMastByPLUCode")]
  public async Task<IActionResult> DeletePLUMastByPLUCode([FromBody] string pluCode)
  {
    bool isDeletedPLUMastByPLUCode = await _plumastService.DeletePLUMastByPLUCode(pluCode);
    return Ok(isDeletedPLUMastByPLUCode);
  }

  // GET api/Product/GetPLUMastByPLUCode/{pluCode}
  [HttpGet]
  [Route("GetPLUMastByPLUCode/{pluCode}")]
  public async Task<IActionResult> GetPLUMastByPLUCode(string pluCode)
  {
    PLUMast? pluMast = await _plumastService.GetPLUMastByPLUCode(pluCode);
    return Ok(pluMast);
  }
}
