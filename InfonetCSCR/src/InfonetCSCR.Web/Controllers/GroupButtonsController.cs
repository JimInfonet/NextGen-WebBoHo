namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class GroupButtonsController : Controller
{
  private readonly IGroupButtonsService _groupButtonsService;
  private readonly IApplicationConfig _config;

  public GroupButtonsController(IGroupButtonsService groupButtonsService, IApplicationConfig config)
  {
    _groupButtonsService = groupButtonsService;
    _config = config;
  }

  #region Private Properties

  private ImageNameConfigViewModel ImageNameConfig
  {
    get
    {
      ImageNameConfigViewModel imageNameConfig = _config.ImageNameConfig;
      return imageNameConfig;
    }
  }

  #endregion

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetImageNameConfig()
  {
    return Ok(ImageNameConfig);
  }

  public async Task<IActionResult> GetAllGroupButtonsData()
  {
    IList<GroupButtonViewModel>? groupButtons = await _groupButtonsService.GetAllGroupButtonsData();
    return Ok(groupButtons);
  }

  public async Task<IActionResult> SwapGroupButtonData(int sourceButtonId, int targetButtonId)
  {
    bool isUpdated = await _groupButtonsService.SwapGroupButtonData(sourceButtonId, targetButtonId);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> InsertGroupButton([FromBody] GroupButtonViewModel groupButton)
  {
    bool isAdded = await _groupButtonsService.InsertGroupButton(groupButton);
    return Ok(isAdded);
  }

  public async Task<IActionResult> UpdateGroupButton([FromBody] GroupButtonViewModel groupButton)
  {
    bool isUpdated = await _groupButtonsService.UpdateGroupButton(groupButton);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> DeleteGroupButton(int buttonId)
  {
    bool isDeleted = await _groupButtonsService.DeleteGroupButton(buttonId);
    return Ok(isDeleted);
  }
}
