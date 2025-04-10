namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICompanySetupService
{
  Task<Company?> GetCompany();
  Task<string?> GetCompanyName();
  Task<string?> GetCompanyStoreCode();
}
