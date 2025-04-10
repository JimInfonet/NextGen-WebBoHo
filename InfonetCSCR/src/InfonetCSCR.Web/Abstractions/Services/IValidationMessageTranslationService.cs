namespace InfonetCSCR.Web.Abstractions.Services;

public interface IValidationMessageTranslationService
{
  public string GetLocalizedError(string errorKey);
}
