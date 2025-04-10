namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class FunctionButtonsController : ControllerBase
{
  private readonly IFunctionButtonsService _functionButtonsService;

  public FunctionButtonsController(IFunctionButtonsService functionButtonsService)
  {
    _functionButtonsService = functionButtonsService;
  }

  // GET api/FunctionButtons/GetAllFunctionButtonsData
  [HttpGet]
  [Route("GetAllFunctionButtonsData")]
  public async Task<IActionResult> GetAllFunctionButtonsData()
  {
    IList<FunctionButton> functionButtons = await _functionButtonsService.GetAllFunctionButtonsData();
    return Ok(functionButtons);
  }

  // POST api/FunctionButtons/SwapFunctionButtonData/{sourceButtonId}/{targetButtonId}
  [HttpPost]
  [Route("SwapFunctionButtonData/{sourceButtonId}/{targetButtonId}")]
  public async Task<IActionResult> SwapFunctionButtonData(int sourceButtonId, int targetButtonId)
  {
    bool isUpdated = await _functionButtonsService.SwapFunctionButtonData(sourceButtonId, targetButtonId);
    return Ok(isUpdated);
  }

  // POST api/FunctionButtons/InsertFunctionButton
  [HttpPost]
  [Route("InsertFunctionButton")]
  public async Task<IActionResult> InsertFunctionButton(FunctionButton functionButton)
  {
    bool isAdded = await _functionButtonsService.InsertFunctionButton(functionButton);
    return Ok(isAdded);
  }

  // POST api/FunctionButtons/UpdateFunctionButton
  [HttpPost]
  [Route("UpdateFunctionButton")]
  public async Task<IActionResult> UpdateFunctionButton(FunctionButton functionButton)
  {
    bool isUpdated = await _functionButtonsService.UpdateFunctionButton(functionButton);
    return Ok(isUpdated);
  }

  // POST api/FunctionButtons/DeleteFunctionButton/{buttonId}
  [HttpPost]
  [Route("DeleteFunctionButton/{buttonId}")]
  public async Task<IActionResult> DeleteFunctionButton(int buttonId)
  {
    bool isDeleted = await _functionButtonsService.DeleteFunctionButton(buttonId);
    return Ok(isDeleted);
  }
}
