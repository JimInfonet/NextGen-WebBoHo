namespace InfonetCSCR.Web.Abstractions.Services;

public interface IReasonsService
{
  Task<IList<ReasonViewModel>> GetReasons(string reasonType);
  public ReasonsPoliciesViewModel GetPolicies();
  public Task<bool> ReasonAlreadyExists(string code, char type);
  Task<bool> AddReason(ReasonViewModel reason);
  Task<bool> UpdateReason(ReasonViewModel reason);
  Task<bool> DeleteReason(string code, char type);
}
