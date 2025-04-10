namespace InfonetCSCR.Web.Abstractions.Services;

public interface IApplicationPolicyService
{
  Task<List<PolicyCompanyViewModel>> GetCompany();
  Task<List<PolicySettingsViewModel>> GetSettings();
  Task<List<PolicyClassViewModel>> GetClass();
  Task<List<PolicyCanbeViewModel>> GetCanbe();
}