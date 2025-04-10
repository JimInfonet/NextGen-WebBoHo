namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITillsSaleTendService
{
  Task<bool> CheckTranscationExistsForTendName(string tendName);
}
