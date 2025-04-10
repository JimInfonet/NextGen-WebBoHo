let customerCardsDatatable;
let rewardCardsDatatable;
let customerCards;
let rewardCards;
let selectedCustomerCardNumber;
let selectedRewardCard;
let isCustomerCardEditMode = true;
let isCustomerCardAddMode = false;
let isRewardCardEditMode = false;
let isRewardCardAddMode = false;
let previousPhoneNumber = "";
let customerCode = "";
let custMem = document.getElementById("CUST_MEM").value === "True";
let clArCust = document.getElementById("CL_ARCUST").value === "True";
let rstrProfile = document.getElementById("RSTR_PROFILE").value === "True";
let useDataCard = document.getElementById("USE_DATACARD").value === "True";
let isBackOffice = document.getElementById("isBackOffice").value === "True";
let isSubmitClicked = false;

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  customerCode = urlParams.get('Code');
  document.getElementById("customer-card-title-id").innerHTML = $('#customerCardTitleID').val() + " " + customerCode;
  $('.text-danger').prop('hidden', true);
  disableEditDeleteButtons();
  await loadRewardCards();
  await loadCustomerCards();
  await loadRestrictionProfileOptions();
});

let disableEditDeleteButtons = () => {
  $('#btn-edit').prop('disabled', true);
  $('#btn-delete').prop('disabled', true);
  $('#rwd-btn-add').prop('disabled', true);
  $('#rwd-btn-edit').prop('disabled', true);
  $('#rwd-btn-delete').prop('disabled', true);
}

// Customer Card Datatable & Form Behaviors

let onCustomerCardAddClick = () => {
  addOrEditMode(true);
}

let onCustomerCardEditClick = () => {
  addOrEditMode(false);
}

let addOrEditMode = (isAddMode) => {
  isCustomerCardAddMode = isAddMode;
  isCustomerCardEditMode = !isAddMode;
  fillCustomerCardForm(isAddMode);
  $('.not-editable').prop('readOnly', !isAddMode);
  $('.text-danger').prop('hidden', true);
  isSubmitClicked = false;
  $("#customerCardModal").modal('show');
}

async function onCustomerCardInputChange() {
  if (isSubmitClicked) {
    let formData = await prepareCustomerFormData();
    validateCustomerFormData(formData);
  }
}
$("#credit-limit").on('keypress', function (event) {
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

// Reward Card Datatable & Form Behaviors

let onRewardCardAddClick = () => {
  rewardCardAddOrEditMode(true);
}

let onRewardCardEditClick = () => {
  rewardCardAddOrEditMode(false);
}

let rewardCardAddOrEditMode = (isAddMode) => {
  isRewardCardAddMode = isAddMode;
  isRewardCardEditMode = !isAddMode;
  fillRewardCardForm(isAddMode);
  $('.not-editable').prop('readOnly', !isAddMode);
  $('.text-danger').prop('hidden', true);
  isSubmitClicked = false;
  $("#rewardCardModal").modal('show');
}

let onRewardCardInputChange = () => {
  $('.text-danger').prop('hidden', true);
  if (isSubmitClicked) {
    let rewardFormData = prepareRewardFormData();
    validateRewardFormData(rewardFormData);
  }
}

let onRewardCardCancelClick = () => {
  isRewardCardAddMode = false;
  isRewardCardEditMode = false;
}

let onRewardCardDeleteClick = async () => {
  let title = document.getElementById("delete").value;
  let message = document.getElementById("confirmDeleteRewardCard").value.replace("{0}", selectedRewardCard);

  showConfirmModal(title, message).then((result) => {
    if (result === true) {
      let url = `CustomerCard/DeleteRewardCard?customerCardNumber=${selectedCustomerCardNumber}&rewardCardNumber=${selectedRewardCard}`;
      ajaxPostAsync(url, selectedCustomerCardNumber)
        .then(async () => {
          await loadRewardCards(selectedCustomerCardNumber);
          toastr.success($("#rewardCardDeletedSuccessfully").val());
          selectedRewardCard = null;
          $('#rwd-btn-edit').prop('disabled', true);
          $('#rwd-btn-delete').prop('disabled', true);
        })
        .catch(() => toastr.error($("#anErrorHasOccurredWhileDeletingRewardCard").val()));
    }
    return;
  });
}

async function onRewardCardCompleteClick() {
  isSubmitClicked = true;
  let rewardFormData = prepareRewardFormData();
  let isValid = validateRewardFormData(rewardFormData);
  if (isValid) {
    if (isRewardCardAddMode) {
      let url = `CustomerCard/IsRewardCardExistByCustomerCardAndRewardCardNumber?custCardNumber=${selectedCustomerCardNumber}&rewardCardNumber=${rewardFormData.PointCardNum}`;
      let existingRewardCard = await ajaxGetAsync(url);
      if (existingRewardCard) {
        toastr.warning($("#thisPointCardIsLinked").val());
        return;
      }
    }

    let newPhoneNumber = $('#point-card-phone-number').val();
    if (previousPhoneNumber !== newPhoneNumber) {
      let url = `CustomerCard/IsRewardCardExistByCustomerCardAndPhone?custCardNumber=${selectedCustomerCardNumber}&phoneNumber=${rewardFormData.PhoneNum}`;
      let existingRewardCard = await ajaxGetAsync(url);
      if (existingRewardCard) {
        toastr.warning($("#thisPhoneNumberIsLinked").val());
        return;
      }
    }
    let url = `CustomerCard/SaveRewardCard`;

    await ajaxPostAsync(url, rewardFormData)
      .then(async () => {
        await loadRewardCards(selectedCustomerCardNumber);
        selectedRewardCard = null;
        toastr.success(isRewardCardAddMode ? $("#rewardCardAddedSuccefully").val() : $("#rewardCardUpdatedSuccefully").val());
        onRewardCardCancelClick();
        isSubmitClicked = false;
        $("#rewardCardModal").modal('hide');
      })
      .catch(toastrError);
  }
}

let createCustomerCardsDatatable = async () => {
  destroyDataTableIfExists(customerCardsDatatable);

  let props = createDataTableProp();
  props.data = customerCards;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];

  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "cardNumber", },
    { "data": "code", },
    { "data": "cardName", },
    { "data": "cardStatus", },
  ];

  customerCardsDatatable = createDatatableWithSearching('customer-cards-grid', props);

  $('#customer-cards-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      const tableinfo = customerCardsDatatable.page.info();

      if (tableinfo.recordsDisplay === 0 || customerCards.length === 0) {
        return;
      }

      selectedRewardCard = null;
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        disableEditDeleteButtons();
        selectedCustomerCardNumber = null;
        await loadRewardCards();
      }
      else if (customerCards.length > 0) {
        $('#btn-edit').prop('disabled', false);
        $('#btn-delete').prop('disabled', false);
        $('#rwd-btn-add').prop('disabled', false);
        $('#rwd-btn-edit').prop('disabled', true);
        $('#rwd-btn-delete').prop('disabled', true);

        customerCardsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedCustomerCardNumber = $(this).find("td:first")[0].innerText;

        await loadRewardCards(selectedCustomerCardNumber);
      }
    })
    .on('dblclick', 'tr', function () {
      const tableinfo = customerCardsDatatable.page.info();

      if (tableinfo.recordsDisplay === 0 || customerCards.length === 0) {
        return;
      }

      selectedCustomerCardNumber = $(this).find("td:first")[0].innerText;
      onCustomerCardEditClick();
    });
}

let fillCustomerCardForm = async (isClear = false) => {
  if (isClear) {
    let customerSeqNumber = '';
    let nextSeqNumber = 0;

    if (useDataCard) {
      if (clArCust) {
        nextSeqNumber = await getNextSequenceNumber();
      }

      customerSeqNumber = `999${customerCode}=${nextSeqNumber}`;
    }

    $('#card-name').val("");
    $('#card-number').val(customerSeqNumber);
    $('#pin').val("");
    $('#card-status-select').val("");
    $('#restriction-profile-select').val("");
    $('#exp-date').val(new Date().toISOString().split('T')[0]);
    $('#credit-limit').val("");
    $('#tax-exempt-card').val("");
    $('#allow-redemption').prop('checked', false);
    document.getElementById("customer-form-section-title").innerHTML = $('#cardNumberSectionTitle').val();
  }
  else if (selectedCustomerCardNumber) {
    const selectedCard = customerCards.find(({ cardNumber }) => cardNumber === selectedCustomerCardNumber);
    selectedCard.expDate = new Date(selectedCard.expDate).toISOString().split('T')[0];
    $('#card-name').val(selectedCard.cardName);
    $('#card-number').val(selectedCard.cardNumber);
    $('#pin').val(selectedCard.pin);
    $('#card-status-select').val(selectedCard.cardStatus);
    $('#restriction-profile-select').val(selectedCard.profileID);
    $('#exp-date').val(selectedCard.expDate);
    $('#credit-limit').val(selectedCard.creditLimit);
    $('#tax-exempt-card').val(selectedCard.teCardNumber);
    $('#allow-redemption').prop('checked', selectedCard.allowRedemption);
    document.getElementById("customer-form-section-title").innerHTML = $('#cardNumberSectionTitle').val() + " - " + selectedCustomerCardNumber;
  }
}

let fillRewardCardForm = (isClear = false) => {
  if (isClear) {
    $('#point-card-number').val("");
    $('#point-card-phone-number').val("");
  }
  else if (selectedRewardCard) {
    const selectedCard = rewardCards.find(({ pointCardNum }) => pointCardNum === selectedRewardCard);
    $('#point-card-number').val(selectedCard.pointCardNum);
    $('#point-card-phone-number').val(selectedCard.phoneNum);
    previousPhoneNumber = selectedCard.phoneNum;
  }
}

let loadDataFromCustomerController = async (url, data) => {
  return await $.ajax({
    type: 'GET',
    url: `/Customer/${url}`,
    dataType: 'JSON',
    data: data,
    success: function (result) {
      if (result === null) return;
      return result;
    }
  });
}

let loadRestrictionProfileOptions = async () => {
  await loadDataFromCustomerController('GetAllRestrictionProfiles', {}).then((data) => {
    data.forEach(({ profileID, profileName }) => {
      let option;
      if (rstrProfile && useDataCard)
        option = optionGeneratorForSingleElement(profileID);
      else
        option = optionGenerator(profileID, profileName);

      $("#restriction-profile-select").append(option);
    });
  });
}

let loadCustomerCards = async () => {
  let url = "CustomerCard/GetCustomerCards";
  await ajaxGetAsync(url)
    .then(_customerCards => {
      customerCards = _customerCards;
      createCustomerCardsDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingCustomersCards").val()));
}

async function addCustomerCard(event) {
  event.preventDefault(); // Prevent form submission
  isSubmitClicked = true;
  const formData = await prepareCustomerFormData();
  let isValid = validateCustomerFormData(formData);

  if (!isValid) {
    $('.text-danger').prop('hidden', false);
    return;
  }

  formData.AllowRedemption = formData.AllowRedemption === undefined ? false : formData.AllowRedemption;
  formData.PIN = formData.PIN === undefined ? "" : formData.PIN;
  formData.ProfileID = formData.ProfileID === undefined ? null : formData.ProfileID;
  formData.TECardNumber = formData.TECardNumber === undefined ? null : formData.TECardNumber;

  if (isCustomerCardAddMode) {
    let url = `CustomerCard/GetCustomerCardByNumber?cardNumber=${formData.CardNumber}`;
    let existingCustomerCard = await ajaxGetAsync(url);
    if (existingCustomerCard) {
      toastr.error($("#customerCardAlreadyExistsInCustomer").val().replace("{0}", formData.CardNumber).replace("{1}", existingCustomerCard.code));
      return;
    }
  }

  let url = `CustomerCard/SaveCustomerCard`;
  await ajaxPostAsync(url, formData)
    .then(async () => {
      await loadCustomerCards();
      await loadRewardCards();
      disableEditDeleteButtons();
      selectedCustomerCardNumber = null;
      fillCustomerCardForm(true);
      toastr.success(isCustomerCardAddMode ? $("#customerCardAddedSuccefully").val() : $("#customerCardUpdatedSuccefully").val());
      isSubmitClicked = false;
      $("#customerCardModal").modal('hide');
    })
    .catch(toastrError);
}

let deleteCustomerCard = async () => {
  let title = document.getElementById("delete").value;
  let message = document.getElementById("confirmDeleteCustomerCard").value.replace("{0}", selectedCustomerCardNumber);

  showConfirmModal(title, message).then((result) => {
    if (result === true) {
      let url = `CustomerCard/DeleteCustomerCard?customerCardNumber=${selectedCustomerCardNumber}`;
      ajaxPostAsync(url, selectedCustomerCardNumber)
        .then(async () => {
          await loadCustomerCards();
          await loadRewardCards();
          fillCustomerCardForm(true);
          selectedCustomerCardNumber = null;
          disableEditDeleteButtons();
          toastr.success($("#customerCardDeletedSuccessfully").val());
        })
        .catch(() => toastr.error($("#anErrorHasOccurredWhileDeletingCustomerCard").val()));
    }
  });
}

async function prepareCustomerFormData() {
  let AllowRedemption = document.getElementById("allow-redemption");
  let CardName = document.getElementById("card-name");
  let CardNumber = document.getElementById("card-number");
  let CardStatus = document.getElementById("card-status-select");
  let CreditLimit = document.getElementById("credit-limit");
  let ExpDate = document.getElementById("exp-date");
  let PIN = document.getElementById("pin");
  let ProfileID = document.getElementById("restriction-profile-select");
  let TECardNumber = document.getElementById("tax-exempt-card");

  let formData = {
    AllowRedemption: AllowRedemption?.checked,
    CardName: CardName?.value,
    CardNumber: CardNumber?.value,
    CardStatus: CardStatus?.value,
    Code: customerCode,
    CreditLimit: clArCust ? CreditLimit?.value : (CreditLimit === null || CreditLimit.value === "") ? 0 : CreditLimit.value,
    ExpDate: ExpDate?.value,
    IsEditMode: !isCustomerCardAddMode,
    PIN: PIN?.value,
    ProfileID: ProfileID?.value,
    TECardNumber: TECardNumber?.value
  }

  if (useDataCard && !isBackOffice) {
    let newNextSequenceNumber = await getNextSequenceNumber();

    if (formData.isAddMode) formData.sequence = newNextSequenceNumber;
    else formData.sequence = $('#card-number').val().split("=")[1];
  }

  return formData;
}

let prepareRewardFormData = () => {
  let rewardCardNumber = document.getElementById("point-card-number").value;
  let rewardCardPhone = document.getElementById("point-card-phone-number").value;

  const formData = {
    CustomerCardNum: selectedCustomerCardNumber,
    PointCardNum: rewardCardNumber,
    PhoneNum: rewardCardPhone,
    IsEditMode: isRewardCardEditMode
  }

  return formData;
}

let validateCustomerFormData = (formData) => {
  let isValid = true;
  const balancePattern = /^([-+]?\d+(\.\d*)?|[-+]?\.\d+)?$/;
  const pinPattern = /^(?:\d{4,10})?$/;

  if (formData.CreditLimit && !balancePattern.test(formData.CreditLimit))
    isValid = false;

  if (formData.TECardNumber && !balancePattern.test(formData.TECardNumber))
    isValid = false;

  if (clArCust && formData.CreditLimit === '') {
    $('#validation-for-credit-limit').html($("#thisFieldIsRequired").val());
    $('#validation-for-credit-limit').prop('hidden', false);
    isValid = false;
  }
  else if (isNaN(formData.CreditLimit)) {
    $('#validation-for-credit-limit').html($("#theValueMustBeANumber").val());
    $('#validation-for-credit-limit').prop('hidden', false);
    isValid = false;
  }
  else if (formData.CreditLimit !== '' && (formData.CreditLimit > 32767 || formData.CreditLimit < -32767)) {
    $('#validation-for-credit-limit').html($("#creditLimitRange").val());
    $('#validation-for-credit-limit').prop('hidden', false);
    isValid = false;
  }
  else {
    $('#validation-for-credit-limit').html('');
  }

  if (formData.PIN && !pinPattern.test(formData.PIN))
    isValid = false;

  if (!formData.CardStatus || !formData.CardName || !formData.ExpDate)
    isValid = false;

  if (!useDataCard && !isBackOffice && !balancePattern.test(formData.CardNumber)) {
    $('#validation-for-card-number').html($("#numericValueOnlyForCardNumber").val());
    $('#validation-for-card-number').prop('hidden', false);
    isValid = false;
  }
  else if (!formData.CardNumber) {
    $('#validation-for-card-number').html($("#thisFieldIsRequired").val());
    $('#validation-for-card-number').prop('hidden', false);
    isValid = false;
  }
  else {
    $('#validation-for-card-number').html('');
  }

  return isValid;
}

// Reward Card Section

let validateRewardFormData = (formData) => {
  let isValid = true;
  const phoneNumberPattern = /^[+\-]?\d+$/;

  if (!phoneNumberPattern.test(formData.PointCardNum)) {
    let message = formData.PointCardNum != '' ? $("#numericValueOnlyForCardNumber").val() : $("#thisFieldIsRequired").val();
    $('#validation-for-reward-card-number').html(message);
    $('#validation-for-reward-card-number').prop('hidden', false);
    isValid = false;
  }

  if (formData.PhoneNum != "" && !phoneNumberPattern.test(formData.PhoneNum)) {
    $('#validation-for-reward-phone').prop('hidden', false);
    isValid = false;
  }
  return isValid;
}

let createRewardCardsDatatable = async () => {
  if (rewardCardsDatatable) {
    rewardCardsDatatable.state.clear();
    rewardCardsDatatable.clear();
    rewardCardsDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = rewardCards;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "pointCardNum", },
    { "data": "phoneNum", },
  ];

  rewardCardsDatatable = createDataTableGeneric('reward-cards-grid', props);

  $('#reward-cards-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if (rewardCards.length === 0) {
        return;
      }

      if (selectedCustomerCardNumber) {
        if ($(this).hasClass('selected-grid-row')) {
          $(this).removeClass('selected-grid-row');
          $('#rwd-btn-edit').prop('disabled', true);
          $('#rwd-btn-delete').prop('disabled', true);
          selectedRewardCard = null;
        } else if (rewardCards.length > 0) {
          rewardCardsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
          $(this).addClass('selected-grid-row');
          selectedRewardCard = $(this).find("td:first")[0].innerText;
          $('#rwd-btn-edit').prop('disabled', false);
          $('#rwd-btn-delete').prop('disabled', false);
        }
      }
    })
    .on('dblclick', 'tr', function () {
      if (rewardCards.length === 0) {
        return;
      }

      selectedRewardCard = $(this).find("td:first")[0].innerText;
      onRewardCardEditClick();
    });
}

let loadRewardCards = async (cardNumber) => {
  let url = `CustomerCard/GetRewardCardsByCardNumber?cardNumber=${cardNumber}`;
  await ajaxGetAsync(url)
    .then(_rewardCards => {
      rewardCards = _rewardCards;
      createRewardCardsDatatable();
    })
    .catch(() => toastr.error($("#failedToLoadRewardCards").val()));
}

let goToCustomerPage = () => {
  const url = "/CustomersGrid";
  safelyNavigateTo(url);
}

async function getNextSequenceNumber() {
  if (!useDataCard) return 1;

  let nextSequenceNumber = 0;
  let url = `CustomerCard/GetNextSequenceNumber?customerCode=${customerCode}`;

  await ajaxGetAsync(url)
    .then(_ => nextSequenceNumber = _)
    .catch(error => toastr.error(error));

  return nextSequenceNumber;
}
