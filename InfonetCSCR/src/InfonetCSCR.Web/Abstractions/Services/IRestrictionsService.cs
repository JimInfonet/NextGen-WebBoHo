namespace InfonetCSCR.Web.Abstractions.Services;

public interface IRestrictionsService
{
  Task<IList<RestrictionsViewModel>?> GetRestrictions();
  Task<bool> AddRestriction(RestrictionsViewModel restriction);
  Task<bool> RestrictionAlreadyExists(int code);
  Task<bool> UpdateRestriction(RestrictionsViewModel restriction);
  Task<bool> DeleteRestriction(int code);
}
