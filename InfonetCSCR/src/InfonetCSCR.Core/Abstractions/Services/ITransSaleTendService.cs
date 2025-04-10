namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITransSaleTendService
{
  Task<bool> CheckTranscationExistsForTendName(string tendName);
}
