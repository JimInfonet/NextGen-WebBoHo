namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IRestrictionsRepository
{
  Task<IList<Restriction>> GetRestrictionsAsync();
  Task<bool> AddRestrictionAsync(Restriction restriction);
  Task<bool> RestrictionAlreadyExistsAsync(int code);
  Task<bool> UpdateRestrictionAsync(Restriction restriction);
  Task<bool> DeleteRestrictionAsync(int code);
}