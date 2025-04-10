namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CardRestrictionProfileController : ControllerBase
{
  private readonly ICardRestrictionProfileService _restrictionProfileService;

  public CardRestrictionProfileController(ICardRestrictionProfileService restrictionProfileService)
  {
    _restrictionProfileService = restrictionProfileService;
  }

  // GET api/CardRestrictionProfile/GetAllRestrictionProfiles
  [HttpGet]
  [Route("GetAllRestrictionProfiles")]
  public async Task<IActionResult> GetAllRestrictionProfiles()
  {
    IEnumerable<CardRestrictionProfile>? restrictionProfiles = await _restrictionProfileService.GetAllRestrictionProfiles();
    return Ok(restrictionProfiles);
  }

  // POST api/CardRestrictionProfile/AddTimeOfPurchaseRestriction
  [HttpPost]
  [Route("AddTimeOfPurchaseRestriction")]
  public async Task<IActionResult> AddTimeOfPurchaseRestriction([FromBody] CardProfileTimeLimitDto cardProfileTimeLimitDto)
  {
    bool isAdded = await _restrictionProfileService.AddTimeOfPurchaseRestriction(cardProfileTimeLimitDto);
    return Ok(isAdded);
  }

  // POST api/CardRestrictionProfile/UpdateTimeOfPurchaseRestriction
  [HttpPost]
  [Route("UpdateTimeOfPurchaseRestriction")]
  public async Task<IActionResult> UpdateTimeOfPurchaseRestriction([FromBody] CardProfileTimeLimitDto cardProfileTimeLimitDto)
  {
    bool isUpdated = await _restrictionProfileService.UpdateTimeOfPurchaseRestriction(cardProfileTimeLimitDto);
    return Ok(isUpdated);
  }

  // GET api/CardRestrictionProfile/GetTimeOfPurchaseRestriction
  [HttpGet]
  [Route("GetTimeOfPurchaseRestriction")]
  public async Task<IActionResult> GetTimeOfPurchaseRestriction(string? profileId)
  {
    IEnumerable<CardProfileTimeLimitDto>? restrictionProfiles = await _restrictionProfileService.GetTimeOfPurchaseRestriction(profileId);
    return Ok(restrictionProfiles);
  }

  // GET api/CardRestrictionProfile/GetAllProfilePrompts
  [HttpGet]
  [Route("GetAllProfilePrompts")]
  public async Task<IActionResult> GetAllProfilePrompts(string? profileId)
  {
    IEnumerable<ProfilePromptDto>? restrictionProfiles = await _restrictionProfileService.GetAllProfilePrompts(profileId);
    return Ok(restrictionProfiles);
  }

  // GET api/CardRestrictionProfile/GetCardRestrictionProfile
  [HttpGet]
  [Route("GetCardRestrictionProfile")]
  public async Task<IActionResult> GetCardRestrictionProfile(string? profileId)
  {
    CardRestrictionProfile? restrictionProfile = await _restrictionProfileService.GetCardRestrictionProfile(profileId);
    return Ok(restrictionProfile);
  }

  // POST api/CardRestrictionProfile/GetProfilePromptByPromptSequence
  [HttpPost]
  [Route("GetProfilePromptByPromptSequence")]
  public async Task<IActionResult> GetProfilePromptByPromptSequence([FromBody] ProfilePromptDto profilePrompt)
  {
    ProfilePromptDto? existingProfilePrompt = await _restrictionProfileService.GetProfilePromptByPromptSequence(profilePrompt);
    return Ok(existingProfilePrompt);
  }

  // POST api/CardRestrictionProfile/AddProfilePrompt
  [HttpPost]
  [Route("AddProfilePrompt")]
  public async Task<IActionResult> AddProfilePrompt([FromBody] ProfilePromptDto profilePrompt)
  {
    bool isAdded = await _restrictionProfileService.AddProfilePrompt(profilePrompt);
    return Ok(isAdded);
  }

  // POST api/CardRestrictionProfile/DeleteProfilePrompt
  [HttpPost]
  [Route("DeleteProfilePrompt")]
  public async Task<IActionResult> DeleteProfilePrompt([FromBody] ProfilePromptDto profilePrompt)
  {
    bool isDeleted = await _restrictionProfileService.DeleteProfilePrompt(profilePrompt);
    return Ok(isDeleted);
  }

  // POST api/CardRestrictionProfile/AddCardRestrictionProfile
  [HttpPost]
  [Route("AddCardRestrictionProfile")]
  public async Task<IActionResult> AddCardRestrictionProfile([FromBody] CardRestrictionProfile cardRestrictionProfile)
  {
    bool isAdded = await _restrictionProfileService.AddCardRestrictionProfile(cardRestrictionProfile);
    return Ok(isAdded);
  }

  // POST api/CardRestrictionProfile/UpdateCardRestrictionProfile
  [HttpPost]
  [Route("UpdateCardRestrictionProfile")]
  public async Task<IActionResult> UpdateCardRestrictionProfile([FromBody] CardRestrictionProfile cardRestrictionProfile)
  {
    bool isupdated = await _restrictionProfileService.UpdateCardRestrictionProfile(cardRestrictionProfile);
    return Ok(isupdated);
  }

  // GET api/CardRestrictionProfile/DeleteTimeOfPurchaseRestriction
  [HttpGet]
  [Route("DeleteTimeOfPurchaseRestriction")]
  public async Task<IActionResult> DeleteTimeOfPurchaseRestriction(string profileId)
  {
    bool isUpdated = await _restrictionProfileService.DeleteTimeOfPurchaseRestriction(profileId);
    return Ok(isUpdated);
  }

  // GET api/CardRestrictionProfile/DeleteCardRestrictionProfile
  [HttpGet]
  [Route("DeleteCardRestrictionProfile")]
  public async Task<IActionResult> DeleteCardRestrictionProfile(string profileId)
  {
    bool isDeleted = await _restrictionProfileService.DeleteCardRestrictionProfile(profileId);
    return Ok(isDeleted);
  }

  // GET api/CardRestrictionProfile/GetPromptMessageByProfileID
  [HttpGet]
  [Route("GetPromptMessageByProfileID")]
  public async Task<IActionResult> GetPromptMessageByProfileID(string profileId)
  {
    IEnumerable<ProfilePromptDto>? promptMessage = await _restrictionProfileService.GetPromptMessageByProfileID(profileId);
    return Ok(promptMessage);
  }
}
