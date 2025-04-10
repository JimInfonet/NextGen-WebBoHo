namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICompanySetupRepository
{
  Task<Company?> GetCompanyAsync();
}
