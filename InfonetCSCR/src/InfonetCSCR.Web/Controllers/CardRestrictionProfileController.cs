namespace InfonetCSCR.Web.Controllers;

public class CardRestrictionProfileController : Controller
{
  private readonly ICardRestrictionProfileService _cardRestrictionProfileService;
  private static bool _isUpdateCardRestrictionProfile = false;

  public CardRestrictionProfileController(ICardRestrictionProfileService cardRestrictionProfileService, ILogger<CardRestrictionProfileController> logger)
  {
    _cardRestrictionProfileService = cardRestrictionProfileService;
  }

  public async Task<IActionResult> Index(string? profileId, string? actionMode, string? showToaster)
  {
    _isUpdateCardRestrictionProfile = (profileId != null && profileId != "" && actionMode != "copy");

    if (actionMode == "create")
    {
      return View();
    }

    CardRestrictionProfileViewModel? cardRestrictionProfile = await _cardRestrictionProfileService.GetCardRestrictionProfile(profileId);

    if (cardRestrictionProfile == null)
    {
      return View();
    }

    return View(cardRestrictionProfile);
  }

  public async Task<IActionResult> GetAllRestrictionProfiles()
  {
    return Ok(await _cardRestrictionProfileService.GetAllRestrictionProfiles());
  }

  public async Task<IActionResult> SaveCardRestrictionProfile(CardRestrictionProfileViewModel cardRestrictionProfile)
  {
    await _cardRestrictionProfileService.SaveCardRestrictionProfile(cardRestrictionProfile, _isUpdateCardRestrictionProfile);
    return RedirectToAction("Index", new { profileId = cardRestrictionProfile.ProfileID, ActionMode = "view", ShowToaster = _isUpdateCardRestrictionProfile ? "update" : "create" });
  }

  public async Task<CardRestrictionProfileViewModel?> GetCardRestrictionProfile(string? profileId)
  {
    return await _cardRestrictionProfileService.GetCardRestrictionProfile(profileId);
  }

  public async Task<IActionResult> DeleteCardRestrictionProfile(string profileId)
  {
    return Ok(await _cardRestrictionProfileService.DeleteCardRestrictionProfile(profileId));
  }

  public async Task<IActionResult> SaveTimeOfPurchaseRestriction([FromBody] CardProfileTimeLimitViewModel cardProfileTimeLimit)
  {
    return Ok(await _cardRestrictionProfileService.SaveTimeOfPurchaseRestriction(cardProfileTimeLimit, _isUpdateCardRestrictionProfile));
  }

  public async Task<IActionResult> GetTimeOfPurchaseRestriction(string? profileId)
  {
    return Ok(await _cardRestrictionProfileService.GetTimeOfPurchaseRestriction(profileId));
  }

  public async Task<IActionResult> DeleteTimeOfPurchaseRestriction(string profileId)
  {
    return Ok(await _cardRestrictionProfileService.DeleteTimeOfPurchaseRestriction(profileId));
  }

  public async Task<IActionResult> AddProfilePrompt([FromBody] ProfilePromptViewModel profilePrompt)
  {
    return Ok(await _cardRestrictionProfileService.AddProfilePrompt(profilePrompt));
  }

  public async Task<IActionResult> GetAllProfilePrompts(string? profileId)
  {
    return Ok(await _cardRestrictionProfileService.GetAllProfilePrompts(profileId));
  }

  public async Task<IActionResult> GetProfilePromptByPromptSequence([FromBody] ProfilePromptViewModel profilePrompt)
  {
    return Ok(await _cardRestrictionProfileService.GetProfilePromptByPromptSequence(profilePrompt));
  }

  public async Task<IActionResult> DeleteProfilePrompt([FromBody] ProfilePromptViewModel profilePrompt)
  {
    return Ok(await _cardRestrictionProfileService.DeleteProfilePrompt(profilePrompt));
  }

  public async Task<IActionResult> GetPromptMessageByProfileID(string profileID)
  {
    return Ok(await _cardRestrictionProfileService.GetPromptMessageByProfileID(profileID));
  }
}
