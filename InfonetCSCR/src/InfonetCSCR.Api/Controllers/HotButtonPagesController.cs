namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class HotButtonPagesController : ControllerBase
{
  private readonly IHotButtonPagesService _hotButtonPagesService;

  public HotButtonPagesController(IHotButtonPagesService hotButtonPagesService)
  {
    _hotButtonPagesService = hotButtonPagesService;
  }

  // GET api/HotButtonPages/GetAllHotButtonPages
  [HttpGet]
  [Route("GetAllHotButtonPages")]
  public async Task<IActionResult> GetAllHotButtonPages()
  {
    IList<HotButtonPages> hotButtonPages = await _hotButtonPagesService.GetAllHotButtonPages();
    return Ok(hotButtonPages);
  }

  // POST api/HotButtonPages/UpdateHotButtonPage
  [HttpPost]
  [Route("UpdateHotButtonPage")]
  public async Task<IActionResult> UpdateHotButtonPage(HotButtonPages hotButtonPages)
  {
    bool isDone = await _hotButtonPagesService.UpdateHotButtonPage(hotButtonPages);
    return Ok(isDone);
  }
}
