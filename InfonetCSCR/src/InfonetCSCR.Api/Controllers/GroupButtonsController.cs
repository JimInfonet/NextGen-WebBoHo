namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class GroupButtonsController : ControllerBase
{
  private readonly IGroupButtonsService _groupButtonsService;

  public GroupButtonsController(IGroupButtonsService groupButtonsService)
  {
    _groupButtonsService = groupButtonsService;
  }

  // GET api/GroupButtons/GetAllGroupButtonsData
  [HttpGet]
  [Route("GetAllGroupButtonsData")]
  public async Task<IActionResult> GetAllGroupButtonsData()
  {
    IList<GroupButton> groupButtons = await _groupButtonsService.GetAllGroupButtonsData();
    return Ok(groupButtons);
  }

  // POST api/GroupButtons/SwapGroupButtonData/{sourceButtonId}/{targetButtonId}
  [HttpPost]
  [Route("SwapGroupButtonData/{sourceButtonId}/{targetButtonId}")]
  public async Task<IActionResult> SwapGroupButtonData(int sourceButtonId, int targetButtonId)
  {
    bool isUpdated = await _groupButtonsService.SwapGroupButtonData(sourceButtonId, targetButtonId);
    return Ok(isUpdated);
  }

  // POST api/GroupButtons/InsertGroupButton
  [HttpPost]
  [Route("InsertGroupButton")]
  public async Task<IActionResult> InsertGroupButton(GroupButton groupButton)
  {
    bool isAdded = await _groupButtonsService.InsertGroupButton(groupButton);
    return Ok(isAdded);
  }

  // POST api/GroupButtons/UpdateGroupButton
  [HttpPost]
  [Route("UpdateGroupButton")]
  public async Task<IActionResult> UpdateGroupButton(GroupButton groupButton)
  {
    bool isUpdated = await _groupButtonsService.UpdateGroupButton(groupButton);
    return Ok(isUpdated);
  }

  // POST api/GroupButtons/DeleteGroupButton/{buttonId}
  [HttpPost]
  [Route("DeleteGroupButton/{buttonId}")]
  public async Task<IActionResult> DeleteGroupButton(int buttonId)
  {
    bool isDeleted = await _groupButtonsService.DeleteGroupButton(buttonId);
    return Ok(isDeleted);
  }
}
