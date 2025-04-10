namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITransSaleTendRepository
{
  Task<bool> CheckTranscationExistsForTendNameAsync(string tendName);
}
