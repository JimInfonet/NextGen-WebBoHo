namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICardRestrictionProfileService
{
  Task<IEnumerable<CardRestrictionProfile>?> GetAllRestrictionProfiles();
  Task<IEnumerable<ProfilePromptDto>?> GetAllProfilePrompts(string? profileId);
  Task<CardRestrictionProfile?> GetCardRestrictionProfile(string? profileId);
  Task<ProfilePromptDto?> GetProfilePromptByPromptSequence(ProfilePromptDto profilePrompt);
  Task<bool> AddProfilePrompt(ProfilePromptDto profilePrompt);
  Task<bool> DeleteProfilePrompt(ProfilePromptDto profilePrompt);
  Task<bool> AddCardRestrictionProfile(CardRestrictionProfile cardRestrictionProfile);
  Task<bool> UpdateCardRestrictionProfile(CardRestrictionProfile cardRestrictionProfile);
  Task<bool> AddTimeOfPurchaseRestriction(CardProfileTimeLimitDto cardProfileTimeLimitDto);
  Task<bool> UpdateTimeOfPurchaseRestriction(CardProfileTimeLimitDto cardProfileTimeLimitDto);
  Task<IEnumerable<CardProfileTimeLimitDto>?> GetTimeOfPurchaseRestriction(string? profileId);
  Task<bool> DeleteTimeOfPurchaseRestriction(string profileId);
  Task<bool> DeleteCardRestrictionProfile(string profileId);
  Task<IEnumerable<ProfilePromptDto>?> GetPromptMessageByProfileID(string profileId);
}
