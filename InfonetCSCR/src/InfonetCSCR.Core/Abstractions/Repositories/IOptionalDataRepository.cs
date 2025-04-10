namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IOptionalDataRepository
{
  Task<List<string>> GetOptionalProfileIDsAsync();
}
