namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class FunctionListController : ControllerBase
{
  private readonly IFunctionListService _functionListService;

  public FunctionListController(IFunctionListService functionListService)
  {
    _functionListService = functionListService;
  }

  // GET api/FunctionList/GetAllFunctionListData
  [HttpGet]
  [Route("GetAllFunctionListData")]
  public async Task<IActionResult> GetAllFunctionListData()
  {
    IList<FunctionList> functionLists = await _functionListService.GetAllFunctionListData();
    return Ok(functionLists);
  }

  // GET api/FunctionList/GetFunctionListByButtonType/{buttonType}
  [HttpGet]
  [Route("GetFunctionListByButtonType/{buttonType}")]
  public async Task<IActionResult> GetFunctionListByButtonType(int buttonType)
  {
    IList<FunctionList> functionLists = await _functionListService.GetFunctionListByButtonType(buttonType);
    return Ok(functionLists);
  }
}
