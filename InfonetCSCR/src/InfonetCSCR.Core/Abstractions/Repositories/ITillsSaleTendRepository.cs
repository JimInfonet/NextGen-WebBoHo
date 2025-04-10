namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITillsSaleTendRepository
{
  Task<bool> CheckTranscationExistsForTendNameAsync(string tendName);
}
