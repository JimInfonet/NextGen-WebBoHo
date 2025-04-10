namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IReasonsRepository
{
  Task<IList<Reason>> GetReasonsAsync(string reasonType);
  Task<bool> ReasonAlreadyExistsAsync(Reason reason);
  Task<bool> AddReasonAsync(Reason reason);
  Task<bool> UpdateReasonAsync(Reason reason);
  Task<bool> DeleteReasonAsync(Reason reason);
}
