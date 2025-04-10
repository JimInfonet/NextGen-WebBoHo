namespace InfonetCSCR.Web.Validations;

public class RequiredIfPolicyAttribute : ValidationAttribute, IClientModelValidator
{
  public string _policyName { get; set; }

  public RequiredIfPolicyAttribute(string policyName)
  {
    this._policyName = policyName;
  }

  protected override ValidationResult IsValid(object value, ValidationContext validationContext)
  {
    var service = validationContext.GetService(typeof(IPolicyService)) as IPolicyService;
    var policy = service?.GetPolicy(_policyName);


    IValidationMessageTranslationService? errorTranslation = validationContext.GetService(typeof(IValidationMessageTranslationService)) as IValidationMessageTranslationService;
    var errorMessage = errorTranslation?.GetLocalizedError(ErrorMessage);

    if (policy != null && policy == true)
    {
      if (value == null)
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

    context.Attributes.Add("data-val", "true");
    context.Attributes.Add("data-val-requireIfPolicy", errorMessage);
    context.Attributes.Add("data-val-requireIfPolicy-policy", policy.ToString());
  }
}
