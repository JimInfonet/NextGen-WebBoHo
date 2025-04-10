namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICardRestrictionProfileRepository
{
  Task<IEnumerable<CardRestrictionProfile>> GetAllRestrictionProfilesAsync();
  Task<IEnumerable<ProfilePromptDto>> GetAllProfilePromptsAsync(string? profileId);
  Task<CardRestrictionProfile?> GetCardRestrictionProfileAsync(string? profileId);
  Task<ProfilePromptDto?> GetProfilePromptByPromptSequenceAsync(ProfilePromptDto profilePrompt);
  Task<bool> AddProfilePromptAsync(ProfilePromptDto profilePrompt);
  Task<bool> DeleteProfilePromptAsync(ProfilePromptDto profilePrompt);
  Task<bool> AddCardRestrictionProfileAsync(CardRestrictionProfile cardRestrictionProfile);
  Task<bool> UpdateCardRestrictionProfileAsync(CardRestrictionProfile cardRestrictionProfile);
  Task<IEnumerable<CardProfileTimeLimit>> GetTimeOfPurchaseRestrictionAsync(string? profileId);
  Task<bool> AddTimeOfPurchaseRestrictionAsync(CardProfileTimeLimit cardProfileTimeLimit);
  Task<bool> UpdateTimeOfPurchaseRestrictionAsync(CardProfileTimeLimit cardProfileTimeLimit);
  Task<bool> DeleteTimeOfPurchaseRestrictionAsync(string profileId);
  Task<bool> DeleteCardRestrictionProfileAsync(string profileId);
  Task<IEnumerable<ProfilePromptDto>?> GetPromptMessageByProfileIDAsync(string profileId);
}
