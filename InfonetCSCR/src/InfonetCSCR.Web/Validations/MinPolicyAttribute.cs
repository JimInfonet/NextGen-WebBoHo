namespace InfonetCSCR.Web.Validations;

public class MinPolicyAttribute : ValidationAttribute, IClientModelValidator
{
  public string _policyName { get; set; }
  public int _minValue { get; set; }

  public MinPolicyAttribute(string policyName, int minValue)
  {
    this._policyName = policyName;
    this._minValue = minValue;
  }

  protected override ValidationResult IsValid(object value, ValidationContext validationContext)
  {
    var service = validationContext.GetService(typeof(IPolicyService)) as IPolicyService;
    var policy = service?.GetPolicy(_policyName);


    IValidationMessageTranslationService? errorTranslation = validationContext.GetService(typeof(IValidationMessageTranslationService)) as IValidationMessageTranslationService;
    var errorMessage = errorTranslation?.GetLocalizedError(ErrorMessage);

    if (policy != null && policy == true && value != null)
    {
      if (Convert.ToInt32(value) < _minValue)
      {
        return new ValidationResult(errorMessage);
      }
    }
    return ValidationResult.Success;
  }

  public void AddValidation(ClientModelValidationContext context)
  {
    var service = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IPolicyService)) as IPolicyService;
    var policy = service?.GetPolicy(_policyName) ?? "";

    IValidationMessageTranslationService? errorTranslation = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IValidationMessageTranslationService)) as IValidationMessageTranslationService;
    var errorMessage = errorTranslation?.GetLocalizedError(ErrorMessage) ?? "";

    context.Attributes.Add("data-val-min", "true");
    context.Attributes.Add("data-val-minValuePolicy", errorMessage);
    context.Attributes.Add("data-val-minValuePolicy-policy", policy.ToString());
    context.Attributes.Add("data-val-minValuePolicy-minValue", _minValue.ToString());
  }
}
