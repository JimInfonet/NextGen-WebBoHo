namespace InfonetCSCR.Core.Abstractions.Services;

public interface IRestrictionsService
{
  Task<IList<Restriction>> GetRestrictions();
  Task<bool> AddRestriction(Restriction restriction);
  Task<bool> RestrictionAlreadyExists(int code);
  Task<bool> UpdateRestriction(Restriction restriction);
  Task<bool> DeleteRestriction(int code);
}