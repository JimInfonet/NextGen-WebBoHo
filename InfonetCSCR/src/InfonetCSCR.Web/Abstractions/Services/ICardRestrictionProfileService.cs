namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICardRestrictionProfileService
{
  Task<IEnumerable<CardRestrictionProfileViewModel>?> GetAllRestrictionProfiles();
  Task<bool> SaveCardRestrictionProfile(CardRestrictionProfileViewModel cardRestrictionProfile, bool isUpdate);
  Task<CardRestrictionProfileViewModel?> GetCardRestrictionProfile(string? profileId);
  Task<bool> DeleteCardRestrictionProfile(string profileId);
  Task<bool> SaveTimeOfPurchaseRestriction(CardProfileTimeLimitViewModel cardProfileTimeLimit, bool isUpdate);
  Task<IEnumerable<CardProfileTimeLimitViewModel>?> GetTimeOfPurchaseRestriction(string? profileId);
  Task<bool> DeleteTimeOfPurchaseRestriction(string profileId);
  Task<bool> AddProfilePrompt(ProfilePromptViewModel profilePrompt);
  Task<IEnumerable<ProfilePromptViewModel>?> GetAllProfilePrompts(string? profileId);
  Task<ProfilePromptViewModel?> GetProfilePromptByPromptSequence(ProfilePromptViewModel profilePrompt);
  Task<bool> DeleteProfilePrompt(ProfilePromptViewModel profilePrompt);
  Task<IEnumerable<ProfilePromptViewModel>?> GetPromptMessageByProfileID(string profileId);
}
