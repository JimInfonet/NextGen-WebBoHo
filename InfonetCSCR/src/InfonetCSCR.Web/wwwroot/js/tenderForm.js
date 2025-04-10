const URL_GET_TENDERS = "/Tenders";

$(async function () {
  await loadClassDropdown();

  if ($("#isTenderAdded").val() === "False") {
    toastr.error($("#failedToAddTender").val());
  }
  if ($("#isTenderCodeExist").val() === "True") {
    toastr.warning($("#tenderCodeAlreadyExists").val());
  }
  if ($("#isFullNameExist").val() === "True") {
    toastr.warning($("#fullNameAlreadyExists").val());
  }
});

$("#exchangeRate").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 5) return false;
  return onlyDigitAndDecimalKeyPress(event);
});

$("#sequence").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 1) return false;
  return onlyDigitKeyPress(event);
});

let loadClassDropdown = async () => {
  let url = `${URL_GET_TENDERS}/GetTenderClasses`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response) {
        let payoutSelection = $('#tendClass');
        let optionTemplate = $('<option></option>');

        for (let i = 0; i < response.length; i++) {
          let option = optionTemplate.clone();
          option.append(`${response[i].tendClass} - ${response[i].classDesc}`);
          option.attr('value', response[i].tendClass);
          payoutSelection.append(option);
        }
      }
      else {
        toastr.error($("#failedToLoadTenderClasses").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingTenderClasses").val());
    });
}

// On Cancel button click
$("#btn-cancel").on('click', function () {
  const url = "/Tenders";
  safelyNavigateTo(url);
})
