let cardCodes;
let cardRangesDatatable;
let cardCode;
let lowerLimit;
let upperLimit;
let cardDefinitionData;
let action = '';

$(async function () {
  let cardType = document.getElementById("cardType-select").value;
  await loadCardCodes();

  changedCardType(cardType);

  const urlParams = new URLSearchParams(window.location.search);
  action = urlParams.get('actionMode');

  const isShowToaster = urlParams.get('ShowToaster');
  if (isShowToaster === "Edit") {
    toastr.success($("#cardDefinitionUpdatedSuccessfully").val());
  }

  let newUrl = removeURLParameter(window.location.search, 'ShowToaster');
  window.history.replaceState(null, null, newUrl);

  if (action === CONSTANT_TEXTS.UPDATE || action === CONSTANT_TEXTS.ROW_DOUBLE_CLICK) {
    editModeOff();
  }
  else {
    editModeOn();
  }

  let productRestrictionCheckBox = document.getElementById("productRestriction");
  if (productRestrictionCheckBox && productRestrictionCheckBox.checked) {
    $("#productRestrictionButton").show();
  }
  else {
    $("#productRestrictionButton").hide();
  }

  let encryptCardNumberModel = document.getElementById("encryptCardNumberModel");
  let encryptCardNumber = document.getElementById("encryptCardNumber");
  encryptCardNumber.checked = encryptCardNumberModel.checked;

  initForm();
  loadDataFromModel();
  const originalData = `${document.getElementById('originalData').value}`;
  cardDefinitionData = JSON.parse(originalData);
});

$("#languageDigitPosition").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#minCardLength").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#maxCardLength").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#floorLimit").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#pourchaseLimit").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#maxCashBack").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#printCopies").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#transactionFee").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#creditFee").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#discountRate").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

let loadDataOnCancel = () => {
  document.getElementById("cardType-select").value = (cardDefinitionData.cardType) ? cardDefinitionData.cardType : 'C';
  changedCardType(cardDefinitionData.cardType);

  document.getElementById("languageDigitPosition").value = cardDefinitionData.languageDigit;
  document.getElementById("minCardLength").value = cardDefinitionData.minLength;
  document.getElementById("maxCardLength").value = cardDefinitionData.maxLength;
  document.getElementById("checkDigitMask").value = cardDefinitionData.checkMask;

  let cardType = document.getElementById("cardType-select").value;
  if (cardType === 'F') {
    document.getElementById("giftType-select-fleet").value = cardDefinitionData.giftType;
  }
  else if (cardType === 'G') {
    document.getElementById("giftType-select-gift").value = cardDefinitionData.giftType;
  }
  else if (cardType === 'L') {
    document.getElementById("giftType-select-ackroo").value = cardDefinitionData.giftType;
  }
  changedCardGiftType(cardDefinitionData.giftType);

  document.getElementById("callTheBank").checked = cardDefinitionData.callCheckBox;
  document.getElementById("allowRefund").checked = cardDefinitionData.refundAllowedCheckBox;
  document.getElementById("printSignatureLine").checked = cardDefinitionData.signatureCheckBox;
  document.getElementById("allowPayPump").checked = cardDefinitionData.allowPayPumpCheckBox;
  document.getElementById("limitToSaleAmount").checked = cardDefinitionData.limitToSaleCheckBox;
  document.getElementById("productRestriction").checked = cardDefinitionData.cardProductRestrictCheckBox;

  document.getElementById("bankCardID").value = cardDefinitionData.bankCardID;
  document.getElementById("floorLimit").value = cardDefinitionData.floorLimit;
  document.getElementById("pourchaseLimit").value = cardDefinitionData.purchaseLimit;

  if (document.getElementById("calcTdrfees").value === "True") {
    document.getElementById("transactionFee").value = cardDefinitionData.transactionFeeAmount;
    document.getElementById("creditFee").value = cardDefinitionData.transactionFeePercentage;
  }
  else {
    document.getElementById("discountRate").value = cardDefinitionData.discountRate;
    document.getElementById("discountType").value = cardDefinitionData.discountType;
  }

  document.getElementById("maxCashBack").value = cardDefinitionData.maxCashBack;
  document.getElementById("printCopies").value = cardDefinitionData.printCopies;
  document.getElementById("receiptText").value = cardDefinitionData.receiptTotalText;

  document.getElementById("optionalDataProfilIDValue").value = cardDefinitionData.optionalDataProfilID;
  changedOptDataPrfileSelect(cardDefinitionData.optionalDataProfilID);

  document.getElementById("verifyTheCheckDigit").checked = cardDefinitionData.verifyCheckDigitCheckBox;
  document.getElementById("verifyTheLast4Digits").checked = cardDefinitionData.encodeCheckBox;
  document.getElementById("verifyUsingNegativeCardFile").checked = cardDefinitionData.negativeCheckBox;
  document.getElementById("verifyUsingPositiveCardFile").checked = cardDefinitionData.positiveCheckBox;
  document.getElementById("checkExpiryDate").checked = cardDefinitionData.expiryCheckBox;
  document.getElementById("allowPayments").checked = cardDefinitionData.allowPaymentCheckBox;
  document.getElementById("encryptCardNumberModel").checked = cardDefinitionData.encryptCardNumberCheckBox;
  document.getElementById("encryptCardNumber").checked = cardDefinitionData.encryptCardNumberCheckBox;
  document.getElementById("verifyCustomerCardNumber").checked = cardDefinitionData.verifyCardNumberCheckBox;
}

let initInput = (inputId, value) => {
  let inputElement = document.getElementById(inputId);
  if (inputElement && !inputElement.value) {
    inputElement.value = value;
  }
}

let initForm = () => {
  initInput("floorLimit", 0);
  initInput("pourchaseLimit", 0);
  initInput("maxCashBack", 0);
  initInput("transactionFee", 0);
  initInput("creditFee", 0);
  initInput("discountRate", 0);
  initInput("discountType", 'P');
  initInput("printCopies", 1);
}

const negativeCheckBox = document.getElementById('verifyUsingNegativeCardFile');
const positiveCheckBox = document.getElementById('verifyUsingPositiveCardFile');

negativeCheckBox.addEventListener('change', function () {
  if (negativeCheckBox.checked) {
    positiveCheckBox.checked = false;
  }
});

positiveCheckBox.addEventListener('change', function () {
  if (positiveCheckBox.checked) {
    negativeCheckBox.checked = false;
  }
});

const transactionFee = document.getElementById('transactionFee');
const creditFee = document.getElementById('creditFee');

if (transactionFee) {
  transactionFee.addEventListener('change', function () {
    if (creditFee.value && creditFee.value !== "0") {
      let title = document.getElementById('error').value;
      let message = document.getElementById('pleaseUseEitherDollarOrPercentage').value;

      creditFee.value = "";
      showAlertModal(title, message).then((result) => { });
    }
  });
}

if (creditFee) {
  creditFee.addEventListener('change', function () {
    if (transactionFee.value && transactionFee.value !== "0") {
      let title = document.getElementById('error').value;
      let message = document.getElementById('pleaseUseEitherDollarOrPercentage').value;

      transactionFee.value = "";
      showAlertModal(title, message).then((result) => { });
    }
  });
}

let loadSelectFromModel = (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId} option[value=${document.getElementById(inputDataId).value}]`).attr('selected', 'selected');
  }
}

let loadDataFromModel = () => {
  let giftType = document.getElementById("giftTypeSelectFromModel").value
  let cardType = document.getElementById("cardType-select").value;

  if (cardType === 'F') {
    document.getElementById("giftType-select-fleet").value = giftType;
  }
  else if (cardType === 'G') {
    document.getElementById("giftType-select-gift").value = giftType;
  }
  else if (cardType === 'L') {
    document.getElementById("giftType-select-ackroo").value = giftType;
  }
}

let editModeOff = () => {
  let cardType = document.getElementById("cardType-select").value;
  _globalIsEditing = false;
  setGlobalEditingState(_globalIsEditing);
  $('.edit-mode-control').prop('disabled', true);
  if ($('#productRestrictionButton')) {
    $('#productRestrictionButton').prop('disabled', false);
  }
  $('#btn-cancel').hide();
  $('#btn-save').hide();
  $('#btn-edit').show();

  $('#cardCode').prop('readonly', true);
  $('#description').prop('readonly', true);

  if (cardType === 'C' || cardType === 'D') {
    document.getElementById("encryptCardNumber").checked = true;
    document.getElementById("encryptCardNumberModel").checked = true;
  }

  $("#cardRangeDeleteButton").prop("disabled", true);
  $('#cardCodes-grid tbody tr').prop("disabled", true);
  $("#encryptCardNumber").prop("disabled", true);
}

let editModeOn = () => {
  let cardType = document.getElementById("cardType-select").value;
  _globalIsEditing = true;
  setGlobalEditingState(_globalIsEditing);
  $('.edit-mode-control').prop('disabled', false);
  if ($('#productRestrictionButton')) {
    $('#productRestrictionButton').prop('disabled', true);
  }
  $('#btn-cancel').show();
  $('#btn-save').show();
  $('#btn-edit').hide();

  $('#cardCode').prop('readonly', true);
  $('#description').prop('readonly', true);

  if (cardType === 'C' || cardType === 'D') {
    let isCardTypeNull = document.getElementById("isCardTypeNull").value === "True";
    if (isCardTypeNull === true) {
      $("#encryptCardNumber").prop("disabled", false);
      document.getElementById("encryptCardNumber").checked = false;
      document.getElementById("encryptCardNumberModel").checked = false;
    }
    else {
      $("#encryptCardNumber").prop("disabled", true);
      document.getElementById("encryptCardNumber").checked = true;
      document.getElementById("encryptCardNumberModel").checked = true;
    }
  }

  if (cardCodes.length === 0) {
    $("#cardRangeDeleteButton").prop("disabled", true);
  }
  else {
    $("#cardRangeDeleteButton").prop("disabled", false);
  }
  $('#cardCodes-grid tbody tr').prop("disabled", false);
}

let goBack = (isGoToCardDefinitionGridButton) => {
  let url = "/Tenders";
  if (document.getElementById("fromGrid").value === "True" || isGoToCardDefinitionGridButton) {
    url = "/CardDefinitionGrid";
  }
  safelyNavigateTo(url);
}

let cancelButtonClicked = () => {
  loadDataOnCancel();
  editModeOff();
}

let changedCardType = (cardType) => {
  if (cardType === 'C') {
    $('#fleetCardTypeSection').hide();
    $('#ackrooCardTypeSection').hide();
    $('#giftCardTypeSection').hide();
    document.getElementById("giftTypeSelectFromModel").value = "";
    $('#cardProductRestictionSection').hide();
    $('#optionalDataProfileIDSection').show();

    let isCardTypeNull = document.getElementById("isCardTypeNull").value === "True";
    if (isCardTypeNull === true) {
      $("#encryptCardNumber").prop("disabled", false);
      document.getElementById("encryptCardNumber").checked = false;
      document.getElementById("encryptCardNumberModel").checked = false;
    }
    else {
      $("#encryptCardNumber").prop("disabled", true);
      document.getElementById("encryptCardNumber").checked = true;
      document.getElementById("encryptCardNumberModel").checked = true;
    }
  }
  else if (cardType === 'D') {
    $('#fleetCardTypeSection').hide();
    $('#ackrooCardTypeSection').hide();
    $('#giftCardTypeSection').hide();
    $('#cardProductRestictionSection').hide();
    $('#optionalDataProfileIDSection').hide();
    $("#encryptCardNumber").prop("disabled", true);
    document.getElementById("encryptCardNumber").checked = true;
    document.getElementById("encryptCardNumberModel").checked = true;
  }
  else if (cardType === 'F') {
    $('#fleetCardTypeSection').show();
    $('#ackrooCardTypeSection').hide();
    $('#giftCardTypeSection').hide();
    $('#cardProductRestictionSection').show();
    $('#optionalDataProfileIDSection').show();
    $("#encryptCardNumber").prop("disabled", false);
  }
  else if (cardType === 'L') {
    $('#fleetCardTypeSection').hide();
    $('#ackrooCardTypeSection').show();
    $('#giftCardTypeSection').hide();
    $('#cardProductRestictionSection').show();
    $('#optionalDataProfileIDSection').show();
    $("#encryptCardNumber").prop("disabled", false);
  }
  else if (cardType === 'G') {
    $('#fleetCardTypeSection').hide();
    $('#ackrooCardTypeSection').hide();
    $('#giftCardTypeSection').show();
    $('#optionalDataProfileIDSection').hide();
    let giftTypeSelectData = document.getElementById("giftTypeSelectFromModel");
    if (giftTypeSelectData && !giftTypeSelectData.value) {
      giftTypeSelectData.value = "G"
    }
    $('#cardProductRestictionSection').hide();
    $("#encryptCardNumber").prop("disabled", false);
  }
  else if (cardType === 'T') {
    $('#fleetCardTypeSection').hide();
    $('#ackrooCardTypeSection').hide();
    $('#giftCardTypeSection').hide();
    document.getElementById("giftTypeSelectFromModel").value = "";
    $('#cardProductRestictionSection').show();
    $("#encryptCardNumber").prop("disabled", false);
    $('#optionalDataProfileIDSection').hide();
  }
  else if (cardType === 'K') {
    $('#fleetCardTypeSection').hide();
    $('#ackrooCardTypeSection').hide();
    $('#giftCardTypeSection').hide();
    document.getElementById("giftTypeSelectFromModel").value = "";
    $('#cardProductRestictionSection').hide();
    $("#encryptCardNumber").prop("disabled", false);
    $('#optionalDataProfileIDSection').hide();
  }
}

let encryptCardNumberChanged = () => {
  let encryptCardNumberModel = document.getElementById("encryptCardNumberModel");
  let encryptCardNumber = document.getElementById("encryptCardNumber");
  encryptCardNumberModel.checked = encryptCardNumber.checked;
}

let changedCardGiftType = (giftType) => {
  document.getElementById("giftTypeSelectFromModel").value = giftType;
}

let changedOptDataPrfileSelect = (optDataProfile) => {
  document.getElementById("optionalDataProfilIDValue").value = optDataProfile;
}

$('#productRestrictionButton').on('click', function () {
  let bankCardId = $('#bankCardID').val();
  let cardDescription = $('#description').val();
  let urlParams = new URLSearchParams(window.location.search);

  let params = new URLSearchParams();
  params.append("bankCardId", bankCardId);
  params.append("cardDescription", cardDescription);
  params.append("cardCode", urlParams.get('cardCode') ?? "");
  params.append("tendCodeData", urlParams.get('tendCodeData') ?? "");
  params.append("fromGrid", urlParams.get('fromGrid') ?? "");
  params.append("actionMode", action);

  const url = `/CardProductRestriction?${params.toString()}`;
  safelyNavigateTo(url);
})

//Card Number Ranges
let createCardRangesDatatable = async () => {
  if (cardRangesDatatable) {
    cardRangesDatatable.state.clear();
    cardRangesDatatable.clear();
    cardRangesDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = cardCodes;
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "lowerLimit", },
    { "data": "upperLimit", },
  ];

  cardRangesDatatable = createDataTableGeneric('cardCodes-grid', props);

  $('#cardCodes-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        cardCode = null;
        lowerLimit = null;
        upperLimit = null;
      } else {
        cardRangesDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        let currentRow = cardRangesDatatable.$('tr.selected-grid-row');
        let currentRowData = cardRangesDatatable.row(currentRow).data();

        cardCode = $("#cardCode").val();
        lowerLimit = currentRowData.lowerLimit;
        upperLimit = currentRowData.upperLimit;
      }
    })
    .on('dblclick', 'tr', function () {
      const url = `/CardDefinition?cardCode=${selectedCardCode}&fromGrid=${true}`;
      safelyNavigateTo(url);
    });
}

let loadCardCodes = async () => {
  let cardCode = document.getElementById("cardCode").value;
  let url = `CardDefinition/GetCardRangesByCardCode?cardCode=${cardCode}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response) {
        cardCodes = response;
        createCardRangesDatatable();
      }
      else {
        toastr.error($("#failedToLoadTender").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingTenders").val());
    });

  if (cardCodes.length === 0) {
    $("#cardRangeDeleteButton").prop("disabled", true);
  }
  else {
    $("#cardRangeDeleteButton").prop("disabled", false);
  }
}

let showAddCardCodeModal = () => {
  $("#addCardCodes").modal("show");
}

let addCardRange = async () => {
  let cardCode = $("#cardCode").val();
  let lowerLimit = $("#fromRange").val();
  let upperLimit = $("#toRange").val();

  if (!lowerLimit) {
    toastr.warning($("#pleaseEnterLowerLimit").val());
    return;
  }

  if (!upperLimit) {
    toastr.warning($("#pleaseEnterUpperLimit").val());
    return;
  }

  let cardRangeData = {
    cardCode: cardCode,
    lowerLimit: lowerLimit,
    upperLimit: upperLimit
  }

  let url = `/CardDefinition/AddCardRange`;
  await ajaxPostAsync(url, cardRangeData)
    .then(async () => {
      await loadCardCodes();
      await createCardRangesDatatable();
      $("#addCardCodes").modal("hide");
    })
    .catch(error => {
      if (error.status === HttpStatusCode.Conflict) {
        toastr.error($("#suchLowerLimitAlreayExistsForThisCardCode").val());
        return;
      }
      toastr.error($("#anErrorOccurredWhileAddingCardNumberRanges").val());
      $("#addCardCodes").modal("hide");
    });

  $("#fromRange").val("");
  $("#toRange").val("");
}

let deleteCardCode = async () => {
  if (!cardCode) {
    cardCode = cardCodes[0].cardCode;
    lowerLimit = cardCodes[0].lowerLimit;
    upperLimit = cardCodes[0].upperLimit;
  }

  let cardRangeData = {
    cardCode: cardCode,
    lowerLimit: lowerLimit,
    upperLimit: upperLimit
  }

  let title = document.getElementById("deleteRange").value;
  let message = document.getElementById("areYouSureYouWantToDeleteThisRange").value;

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `/CardDefinition/DeleteCardRangesByCardCodeData`;
      await ajaxPostAsync(url, cardRangeData)
        .then(async (response) => {
          if (!response) {
            toastr.error($("#anErrorOccurredWhileDeletingFromCardNumberRanges").val());
          }
          else {
            await loadCardCodes();
            await createCardRangesDatatable();

            cardCode = null;
            lowerLimit = null;
            upperLimit = null;
          }
        })
        .catch((error) => {
          toastr.error($("#anErrorOccurredWhileDeletingFromCardNumberRanges").val());
        });
    }
  });
}

let cardRangeCancelButtonClicked = () => {
  $("#fromRange").val("");
  $("#toRange").val("");
}

$('#addCardCodes').on('hidden.bs.modal', function (e) {
  $("#fromRange").val("");
  $("#toRange").val("");
});
