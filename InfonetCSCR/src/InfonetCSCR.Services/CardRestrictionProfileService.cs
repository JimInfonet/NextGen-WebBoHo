namespace InfonetCSCR.Services;

public class CardRestrictionProfileService : ICardRestrictionProfileService
{
  private readonly IUnitOfWork _unitOfWork;

  public CardRestrictionProfileService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<CardRestrictionProfile>?> GetAllRestrictionProfiles()
  {
    IEnumerable<CardRestrictionProfile> restrictionProfiles = await _unitOfWork.CardRestrictionProfileRepository.GetAllRestrictionProfilesAsync();
    _unitOfWork.Commit();
    return restrictionProfiles;
  }

  public async Task<IEnumerable<ProfilePromptDto>?> GetAllProfilePrompts(string? profileId)
  {
    IEnumerable<ProfilePromptDto> profilePrompts = await _unitOfWork.CardRestrictionProfileRepository.GetAllProfilePromptsAsync(profileId);
    _unitOfWork.Commit();
    return profilePrompts;
  }

  public async Task<CardRestrictionProfile?> GetCardRestrictionProfile(string? profileId)
  {
    CardRestrictionProfile? restrictionProfile = await _unitOfWork.CardRestrictionProfileRepository.GetCardRestrictionProfileAsync(profileId);
    _unitOfWork.Commit();
    return restrictionProfile;
  }

  public async Task<ProfilePromptDto?> GetProfilePromptByPromptSequence(ProfilePromptDto profilePrompt)
  {
    ProfilePromptDto? existingProfilePrompt = await _unitOfWork.CardRestrictionProfileRepository.GetProfilePromptByPromptSequenceAsync(profilePrompt);
    _unitOfWork.Commit();
    return existingProfilePrompt;
  }

  public async Task<bool> AddProfilePrompt(ProfilePromptDto profilePrompt)
  {
    bool isAdded = await _unitOfWork.CardRestrictionProfileRepository.AddProfilePromptAsync(profilePrompt);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteProfilePrompt(ProfilePromptDto profilePrompt)
  {
    bool isDeleted = await _unitOfWork.CardRestrictionProfileRepository.DeleteProfilePromptAsync(profilePrompt);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> AddCardRestrictionProfile(CardRestrictionProfile cardRestrictionProfile)
  {
    cardRestrictionProfile.SngleTransaction ??= (decimal?)0.00;
    cardRestrictionProfile.DailyTransaction ??= (decimal?)0.00;
    cardRestrictionProfile.MonthlyTransaction ??= (decimal?)0.00;
    cardRestrictionProfile.TransactionsPerDay ??= 0;

    bool isAdded = await _unitOfWork.CardRestrictionProfileRepository.AddCardRestrictionProfileAsync(cardRestrictionProfile);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateCardRestrictionProfile(CardRestrictionProfile cardRestrictionProfile)
  {
    cardRestrictionProfile.SngleTransaction ??= (decimal?)0.00;
    cardRestrictionProfile.DailyTransaction ??= (decimal?)0.00;
    cardRestrictionProfile.MonthlyTransaction ??= (decimal?)0.00;
    cardRestrictionProfile.TransactionsPerDay ??= 0;

    bool isUpdated = await _unitOfWork.CardRestrictionProfileRepository.UpdateCardRestrictionProfileAsync(cardRestrictionProfile);

    if (cardRestrictionProfile.PromptForFuel == false)
    {
      ProfilePromptDto profilePrompt = new()
      {
        ProfileID = cardRestrictionProfile.ProfileID,
        Type = 'F'
      };

      _ = await _unitOfWork.CardRestrictionProfileRepository.DeleteProfilePromptAsync(profilePrompt);
    }
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> AddTimeOfPurchaseRestriction(CardProfileTimeLimitDto cardProfileTimeLimitDto)
  {
    CardProfileTimeLimit cardProfileTimeLimit = DataReformation(cardProfileTimeLimitDto);

    bool isAdded = await _unitOfWork.CardRestrictionProfileRepository.AddTimeOfPurchaseRestrictionAsync(cardProfileTimeLimit);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateTimeOfPurchaseRestriction(CardProfileTimeLimitDto cardProfileTimeLimitDto)
  {
    CardProfileTimeLimit cardProfileTimeLimit = DataReformation(cardProfileTimeLimitDto);

    bool isUpdated = await _unitOfWork.CardRestrictionProfileRepository.UpdateTimeOfPurchaseRestrictionAsync(cardProfileTimeLimit);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<IEnumerable<CardProfileTimeLimitDto>?> GetTimeOfPurchaseRestriction(string? profileId)
  {
    IEnumerable<CardProfileTimeLimit> cardProfileTimeLimits = await _unitOfWork.CardRestrictionProfileRepository.GetTimeOfPurchaseRestrictionAsync(profileId);
    _unitOfWork.Commit();

    string format = "hh:mm tt";

    IEnumerable<CardProfileTimeLimitDto> cardProfileTimeLimitsDto = cardProfileTimeLimits.Select(data => new CardProfileTimeLimitDto()
    {
      ProfileID = data.ProfileID,
      DayOfWeek = data.DayOfWeek,
      AllowPurchase = data.AllowPurchase,
      TimeRestriction = data.TimeRestriction,
      StartTime = data.StartTime.HasValue ? data.StartTime?.ToString(format) : null,
      EndTime = data.EndTime.HasValue ? data.EndTime?.ToString(format) : null
    });

    return cardProfileTimeLimitsDto;
  }

  public async Task<bool> DeleteTimeOfPurchaseRestriction(string profileId)
  {
    bool isDeleted = await _unitOfWork.CardRestrictionProfileRepository.DeleteTimeOfPurchaseRestrictionAsync(profileId);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<IEnumerable<ProfilePromptDto>?> GetPromptMessageByProfileID(string profileId)
  {
    IEnumerable<ProfilePromptDto>? promptMessage = await _unitOfWork.CardRestrictionProfileRepository.GetPromptMessageByProfileIDAsync(profileId);
    _unitOfWork.Commit();
    return promptMessage;
  }

  public async Task<bool> DeleteCardRestrictionProfile(string profileId)
  {
    ProfilePromptDto profilePrompt = new()
    {
      ProfileID = profileId,
      Type = 'F'
    };

    bool success = true;
    success &= await _unitOfWork.CardRestrictionProfileRepository.DeleteProfilePromptAsync(profilePrompt);
    success &= await _unitOfWork.CardRestrictionProfileRepository.DeleteTimeOfPurchaseRestrictionAsync(profileId);
    success &= await _unitOfWork.CardRestrictionProfileRepository.DeleteCardRestrictionProfileAsync(profileId);
    _unitOfWork.Commit();
    return success;
  }

  private CardProfileTimeLimit DataReformation(CardProfileTimeLimitDto cardProfileTimeLimitDto)
  {
    string format = "yyyy-MM-dd hh:mm tt";

    CardProfileTimeLimit cardProfileTimeLimit = new()
    {
      ProfileID = cardProfileTimeLimitDto.ProfileID,
      DayOfWeek = cardProfileTimeLimitDto.DayOfWeek,
      AllowPurchase = cardProfileTimeLimitDto.AllowPurchase,
      TimeRestriction = cardProfileTimeLimitDto.TimeRestriction,
      StartTime = string.IsNullOrEmpty(cardProfileTimeLimitDto?.StartTime) ? null : DateTime.ParseExact(cardProfileTimeLimitDto?.StartTime!, format, CultureInfo.InvariantCulture),
      EndTime = string.IsNullOrEmpty(cardProfileTimeLimitDto?.EndTime) ? null : DateTime.ParseExact(cardProfileTimeLimitDto?.EndTime!, format, CultureInfo.InvariantCulture)
    };

    return cardProfileTimeLimit;
  }
}
