namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class FunctionButtonsController : Controller
{
  private readonly IApplicationConfig _config;
  private readonly IFunctionButtonsService _functionButtonsService;
  private readonly IFunctionListService _functionListService;

  public FunctionButtonsController(
    IApplicationConfig config,
    IFunctionButtonsService functionButtonsService,
    IFunctionListService functionListService)
  {
    _config = config;
    _functionButtonsService = functionButtonsService;
    _functionListService = functionListService;
  }

  #region Private Properties

  private IList<FunctionButtonImagesPathViewModel> FunctionButtonImagesPathData
  {
    get
    {
      string functionButtonImagesFolderPath = _config.FileConfig.ImagesFolderPath;

      DirectoryInfo functionButtonImagesDirectory = new(functionButtonImagesFolderPath);

      if (functionButtonImagesFolderPath != null && !Directory.Exists(functionButtonImagesFolderPath))
      {
        Directory.CreateDirectory(functionButtonImagesFolderPath);
      }


      FunctionButtonImagesPathViewModel[] functionButtonImages = functionButtonImagesDirectory.GetFiles()
        .Select(file => new FunctionButtonImagesPathViewModel
        {
          Name = file.Name,
          FullPath = file.FullName,
          Base64String = Convert.ToBase64String(System.IO.File.ReadAllBytes(file.FullName))
        })
        .ToArray();

      return functionButtonImages;
    }
  }

  #endregion

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetFunctionButtonImagesPathData()
  {
    return Ok(FunctionButtonImagesPathData);
  }

  public async Task<IActionResult> GetAllFunctionButtonsData()
  {
    IList<FunctionButtonViewModel>? functionButtons = await _functionButtonsService.GetAllFunctionButtonsData();
    return Ok(functionButtons);
  }

  public async Task<IActionResult> GetAllFunctionListData()
  {
    IList<FunctionListViewModel>? functionLists = await _functionListService.GetAllFunctionListData();
    return Ok(functionLists);
  }

  public async Task<IActionResult> SwapFunctionButtonData(int sourceButtonId, int targetButtonId)
  {
    bool isUpdated = await _functionButtonsService.SwapFunctionButtonData(sourceButtonId, targetButtonId);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> GetFunctionListByButtonType(int buttonType)
  {
    IList<FunctionListViewModel>? functionLists = await _functionListService.GetFunctionListByButtonType(buttonType);
    return Ok(functionLists);
  }

  public async Task<IActionResult> InsertFunctionButton([FromBody] FunctionButtonViewModel functionButton)
  {
    bool isAdded = await _functionButtonsService.InsertFunctionButton(functionButton);
    return Ok(isAdded);
  }

  public async Task<IActionResult> UpdateFunctionButton([FromBody] FunctionButtonViewModel functionButton)
  {
    bool isUpdated = await _functionButtonsService.UpdateFunctionButton(functionButton);
    return Ok(isUpdated);
  }

  public async Task<IActionResult> DeleteFunctionButton(int buttonId)
  {
    bool isDeleted = await _functionButtonsService.DeleteFunctionButton(buttonId);
    return Ok(isDeleted);
  }
}
