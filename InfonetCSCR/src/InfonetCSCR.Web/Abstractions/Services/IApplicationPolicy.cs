namespace InfonetCSCR.Web.Abstractions.Services;

public interface IApplicationPolicy
{
  List<PolicyCanbeViewModel> PolicyCanbe { get; }
  List<PolicyClassViewModel> PolicyClass { get; }
  List<PolicyCompanyViewModel> PolicyCompany { get; }
  List<PolicySettingsViewModel> PolicySettings { get; }
}