
$(function () {
  setUpRequiredIfPolicyValidator();
  setUpMinValuePolicyValidator();
});

let setUpRequiredIfPolicyValidator = () => {
  jQuery.validator.addMethod("requireIfPolicy",
    function (value, element, param) {
      let policy = element.dataset.valRequireifpolicyPolicy;
      if (policy === "True" && (value === null || value === "")) {
        return false;
      }

      return true;
    });

  jQuery.validator.unobtrusive.adapters.addBool("requireIfPolicy");
}

let setUpMinValuePolicyValidator = () => {
  jQuery.validator.addMethod("minValuePolicy",
    function (value, element, param) {
      let policy = element.dataset.valRequireifpolicyPolicy;
      let minValue = parseInt(element.dataset.valMinvaluepolicyMinvalue);
      value = parseInt(value);

      if (policy === "True" && value < minValue) {
        return false;
      }

      return true;
    });

  jQuery.validator.unobtrusive.adapters.addBool("minValuePolicy");
}