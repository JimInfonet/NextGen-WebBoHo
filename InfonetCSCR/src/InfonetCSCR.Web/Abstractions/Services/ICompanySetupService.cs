using InfonetCSCR.Core.Models;

namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICompanySetupService
{
    Task<Company?> GetCompanyAsync();
    Task<string> GetCompanyNameAsync();
}
