namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICompanySetupService
{
  Task<string> GetCompanyNameAsync();
}
