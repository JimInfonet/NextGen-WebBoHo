namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class BrandController : ControllerBase
{
  private readonly IBrandService _brandService;

  public BrandController(IBrandService brandService)
  {
    _brandService = brandService;
  }

  // GET api/Brand/GetAllBrands
  [HttpGet]
  [Route("GetAllBrands")]
  public async Task<IActionResult> GetAllBrands()
  {
    IList<Brand> brands = await _brandService.GetAllBrands();
    return Ok(brands);
  }

  // POST api/Brand/AddBrand
  [HttpPost]
  [Route("AddBrand")]
  public async Task<IActionResult> AddBrand([FromBody] Brand brand)
  {
    bool isAdded = await _brandService.AddBrand(brand);
    return Ok(isAdded);
  }

  // POST api/Brand/UpdateBrand
  [HttpPost]
  [Route("UpdateBrand")]
  public async Task<IActionResult> UpdateBrand([FromBody] Brand brand)
  {
    bool isUpdated = await _brandService.UpdateBrand(brand);
    return Ok(isUpdated);
  }

  // POST api/Brand/DeleteBrand
  [HttpPost]
  [Route("DeleteBrand")]
  public async Task<IActionResult> DeleteBrand([FromBody] string brandId)
  {
    bool isDeleted = await _brandService.DeleteBrand(brandId);
    return Ok(isDeleted);
  }
}
