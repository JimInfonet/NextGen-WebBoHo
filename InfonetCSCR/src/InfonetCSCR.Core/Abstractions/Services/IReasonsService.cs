namespace InfonetCSCR.Core.Abstractions.Services;

public interface IReasonsService
{
  Task<IList<Reason>> GetReasons(string reasonType);
  Task<bool> ReasonAlreadyExists(Reason reason);
  Task<bool> AddReason(Reason reason);
  Task<bool> UpdateReason(Reason reason);
  Task<bool> DeleteReason(Reason reason);
}
