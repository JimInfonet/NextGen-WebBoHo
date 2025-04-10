const URL_GET_TENDERS = "/Tenders";
const view = CONSTANT_TEXTS.VIEW;
let cardCode;
let selectedTendCode;
let action;
let tenderDetailsData;
let isCardCodeChanged = false;

$(async function () {
  $('#btn-cancel').hide();
  $('#btn-save').hide();

  const params = new URLSearchParams(window.location.search);
  action = params.get('action');
  selectedTendCode = params.get('tendCode');

  setEditMode(action !== view);

  await initTenderPage();
});

let initTenderPage = async () => {
  await loadPayoutDropdown();

  let tendClass = $("#class").val().toUpperCase();
  if (tendClass === "CASH") {
    $("#payoutCode").val($("#payoutCodeHidden").val());
  }
  if ($("#baseCurrCantBeInactive").val() === "True") {
    toastr.warning($("#baseCurrCantBeInactiveErrorMessage").val());
  }
  if ($("#isTenderDetailsUpdated").val() === "False") {
    toastr.error($("#failedToUpdateTender").val());
  }

  if (tendClass !== tenderClass.CRCARD && tendClass !== tenderClass.DBCARD &&
    tendClass !== tenderClass.FLEET && tendClass !== tenderClass.GIFTCARD &&
    tendClass !== tenderClass.LOYALTY && tendClass !== tenderClass.THIRDPARTY) {
    $("#cardCode").prop('disabled', true);
  }
  else {
    if (action === view) {
      $("#cardCode").prop('disabled', true);
    } else {
      $("#cardCode").prop('disabled', false);
    }
  }
  cardCode = $("#cardCode").val();
}

let loadPayoutDropdown = async () => {
  let url = `${URL_GET_TENDERS}/GetPayout`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response !== null && response !== undefined) {
        let payoutSelection = $('#payoutCode');
        let optionTemplate = $('<option></option>');

        for (let i = 0; i < response.length; i++) {
          let option = optionTemplate.clone();
          option.append(response[i].code + " - " + response[i].reason);
          option.attr('value', response[i].code.split(" ")[0]);
          payoutSelection.append(option);
        }
      }
      else {
        toastr.error($("#failedToLoadPayout").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingPayout").val());
    });
}

$("#btn-back").on('click', function () {
  safelyNavigateTo(URL_GET_TENDERS);
})

$("#btn-save").on('click', function () {
  _globalIsEditing = false;
  setGlobalEditingState(_globalIsEditing);
})

$("#btn-cancel").on('click', function () {
  const modelDataInput = document.getElementById("modelData");
  let modelData = JSON.parse(modelDataInput.value);
  const form = document.getElementById("tendersForm");

  for (let key in modelData) {
    if (modelData.hasOwnProperty(key)) {
      let capitalizedKey = capitalizeFirstLetter(key);
      const inputElement = form.querySelector(`[name="${capitalizedKey}"]`);

      if (inputElement) {
        inputElement.value = modelData[key];
      }
    }
  }
  
  if ($("#exactChange")) {
    $("#exactChange").prop("checked", modelData.exactChange);
  }

  if ($("#inactive")) {
    $("#inactive").prop("checked", modelData.inactive);
  }

  if ($("#giveChange")) {
    $("#giveChange").prop("checked", modelData.giveChange);
  }

  if ($("#acceptAsPayment")) {
    $("#acceptAsPayment").prop("checked", modelData.acceptAsPayment);
  }

  if ($("#giveAsRefund")) {
    $("#giveAsRefund").prop("checked", modelData.giveAsRefund);
  }

  if ($("#useItOrLoseIt")) {
    $("#useItOrLoseIt").prop("checked", modelData.useItOrLoseIt);
  }

  if ($("#bankable")) {
    $("#bankable").prop("checked", modelData.bankable);
  }

  if ($("#includeInClose")) {
    $("#includeInClose").prop("checked", modelData.includeInClose);
  }

  if ($("#putInDrawer")) {
    $("#putInDrawer").prop("checked", modelData.putInDrawer);
  }

  if ($("#signature")) {
    $("#signature").prop("checked", modelData.signature);
  }

  setEditMode(false);
})

$("#cardCode").on('change', function () {
  if (cardCode === $("#cardCode").val()) return;

  if (cardCode && $("#cardCode").val() === "") {
    toastr.warning($("#_pleaseEnterANumericValue").val());
    $("#cardCode").focus();
    return;
  }

  isCardCodeChanged = true;
})

function handleUpdateTender(e) {
  e.preventDefault();
  const form = $(e.target);

  if (!form.valid()) {
    return; // Do not submit the form if there are validation errors
  }

  if (cardCode && $("#cardCode").val() === "") {
    toastr.warning($("#_pleaseEnterANumericValue").val());
    $("#cardCode").focus();
    return;
  }

  if (isCardCodeChanged) {
    $("#changeCardCodeConfirmModal").modal('show');

    $("#changeCardCodeConfirmModalNoButtonControl, #changeCardCodeConfirmModalCloseButtonControl").on('click', async function () {
      $("#cardCode").val(cardCode);
    })

    $("#changeCardCodeConfirmModalYesButtonControl").on('click', async function () {
      let tempCardCode = $("#cardCode").val();
      if (!tempCardCode) {
        $("#changeCardCodeConfirmModal").modal('hide');
        return;
      }

      let url = `/CardDefinition/DeleteCardDefinitionByCardCode?cardCode=${tempCardCode}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (!response) {
            toastr.error($("#failedToDeleteTender").val());
          }
        })
        .catch((error) => {
          toastr.error($("#anErrorHasOccuredWhileDeletingTender").val());
        });

      $("#previousCardCode").val(tempCardCode);
      $("#changeCardCodeConfirmModal").modal('hide');

      e.target.submit();
    });
  } else {
    e.target.submit();
  }
}

let setEditMode = (editMode) => {
  _globalIsEditing = editMode;
  setGlobalEditingState(_globalIsEditing);
  $('.edit-mode-control').prop('disabled', !editMode);
  $('.text-danger').prop('hidden', !editMode);

  let tendClass = $("#class").val().toUpperCase();

  if (tendClass !== tenderClass.CRCARD && tendClass !== tenderClass.DBCARD &&
    tendClass !== tenderClass.FLEET && tendClass !== tenderClass.GIFTCARD &&
    tendClass !== tenderClass.LOYALTY && tendClass !== tenderClass.THIRDPARTY) {
    $("#cardCode").prop('disabled', true);
  }

  if (!editMode) {
    $('#btn-cancel').hide();
    $('#btn-save').hide();
    $('#btn-edit').show();
  }
  else {
    $('#btn-cancel').show();
    $('#btn-save').show();
    $('#btn-edit').hide();
  }
}
