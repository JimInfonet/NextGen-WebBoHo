namespace InfonetCSCR.Web.Services;

public class ValidationMessageTranslationService : IValidationMessageTranslationService
{
  private readonly IStringLocalizer<ValidationMessageTranslationService> _localizer;
  public ValidationMessageTranslationService(IStringLocalizer<ValidationMessageTranslationService> localizer)
  {
    _localizer = localizer;
  }

  public string GetLocalizedError(string errorKey)
  {
    return _localizer[errorKey];
  }
}
