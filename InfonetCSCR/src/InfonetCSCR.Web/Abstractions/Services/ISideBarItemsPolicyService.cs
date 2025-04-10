namespace InfonetCSCR.Web.Abstractions.Services;

public interface ISideBarItemsPolicyService
{
  Task<SideBarItemsPoliciesViewModel> GetSideBarItemsPoliciesAsync();
}
