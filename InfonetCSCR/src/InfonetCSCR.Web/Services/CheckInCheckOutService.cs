namespace InfonetCSCR.Web.Services;

public class CheckInCheckOutService : ICheckInCheckOutService
{
  private readonly IPolicyService _policy;

  public CheckInCheckOutService(IPolicyService policy)
  {
    _policy = policy;
  }

  public CheckInCheckOutPoliciesDtoViewModel GetCheckInCheckOutPolicies(UserDetailsViewModel? userDetails = null)
  {
    var policies = new CheckInCheckOutPoliciesDtoViewModel
    {
      Timesheets = _policy.GetPolicy(PolicyName.TIMESHEETS),
      HaveUserId = _policy.GetPolicy(PolicyName.HAVE_USERID),
      Version = _policy.GetPolicy(PolicyName.VERSION),
      Hour24Store = _policy.GetPolicy(PolicyName.HOUR24STORE),
      PayPeriod = _policy.GetPolicy(PolicyName.PAYPERIOD),
      UseProvHoliday = _policy.GetPolicy(PolicyName.USE_PROV_HOLIDAY),
      Roundtime = _policy.GetPolicy(PolicyName.ROUNDTIME),
      OvertimeH = _policy.GetPolicy(PolicyName.OVERTIME_H),
      OvertimeS = _policy.GetPolicy(PolicyName.OVERTIME_S),
      PyFile = _policy.GetPolicy(PolicyName.PY_FILE),
      TSSignpad = _policy.GetPolicy(PolicyName.TS_SIGNPAD),
      ViewReports = _policy.GetPolicy(PolicyName.VIEWREPORTS, userDetails),
      EditPostRP = _policy.GetPolicy(PolicyName.EDITPOSTRP, userDetails),
      ChangeCheck = _policy.GetPolicy(PolicyName.CHANGECHECK, userDetails),
    };

    return policies;
  }
}
