const STRING_TABLE = CUSTOMER_STRING_TABLE;
let customersDatatable;
let customers;
let selectedCode;
let policies;
let customerCode = "";
let isAddingOrEditing = false;
let isAddMode = false;

let update = CONSTANT_TEXTS.UPDATE;
let rowDoubleClick = CONSTANT_TEXTS.ROW_DOUBLE_CLICK;
let create = CONSTANT_TEXTS.CREATE;

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  customerCode = urlParams.get('Code');
  let action = urlParams.get('ActionMode');
  let isShowToaster = urlParams.get('ShowToaster');

  await loadGroupIDOptions();
  await loadRestrictionProfileOptions();
  await loadCustomerPolicies();
  toggleRestrictionProfile();
  toggleFuletRebateFields();
  enableTaxExemptFields();
  loadDataFromModel();
  $('.text-danger').prop('hidden', true);

  if (customerCode || $("#isLoyaltyNumberAlreadyExistsUpdate").val() === "true") {
    $('.not-editable').prop('readOnly', true);
    $('#btn-edit').hide();
  }
  else {
    addCustomerModeOn();
  }

  if (action) {
    toggleEditMode();
  }

  if (action === create || action === update) {
    if (isShowToaster) {
      toastr.success(action === "create" ? $("#customerCreatedSuccessfully").val() : $("#customerUpdatedSuccessfully").val());
    }

    let newUrl = removeURLParameter(window.location.search, 'ShowToaster');
    window.history.replaceState(null, null, newUrl);
  }

  if ($("#isCustomerCodeAlreadyExists").val() === "true") {
    toastr.error($("#customerCodeAlreadyExists").val());
  }

  if ($("#isLoyaltyNumberAlreadyExists").val() === "true") {
    toastr.warning($("#loyaltyNumAlreadyExists").val());
  }
});

let enableTaxExemptFields = () => {
  let taxExempt = policies.taxExempt;
  let teType = policies.teType;
  let tePayPump = policies.tePayPump;
  let enableBandAccT = policies.enableBandAccT;

  if (taxExempt && (teType == "QITE" || (teType == "AITE" && tePayPump) || (teType == "SITE" && enableBandAccT))) {
    $('.tax-exempt-field').prop('disabled', false);
  }
}

let addCustomerModeOn = () => {
  isAddMode = true;
  $('#btn-cancel').hide();
  $('#btn-edit').hide();
  $('#btn-customer-card').hide();
}

let toggleEditMode = () => {
  if (!isAddingOrEditing) {
    $('.edit-mode-control').prop('disabled', true);
    $('.tax-exempt-field').prop('disabled', true);
    $('.rstr-profile').prop('disabled', true); // dropdown can not be set to read only
    $('.fuel-rebate').prop('disabled', true);
    $('#btn-cancel').hide();
    $('#btn-save').hide();
    $('#btn-edit').show();
  }
  else {
    $('.edit-mode-control').prop('disabled', false);
    $('#btn-cancel').show();
    $('#btn-save').show();
    $('#btn-edit').hide();
    toggleRestrictionProfile();
    toggleFuletRebateFields();
    enableTaxExemptFields();
  }
  isAddingOrEditing = !isAddingOrEditing;
}

let saveButtonClicked = () => {
  $('.text-danger').prop('hidden', false);
}

let cancelButtonClicked = () => {
  $('.text-danger').prop('hidden', true);
  toggleEditMode();

  const modelDataInput = document.getElementById("modelData");
  let modelData = JSON.parse(modelDataInput.value);
  const form = document.getElementById("customerForm");

  for (let key in modelData) {
    if (modelData.hasOwnProperty(key)) {
      let capitalizedKey = capitalizeFirstLetter(key);
      const inputElement = form.querySelector(`[name="${capitalizedKey}"]`);
      if (inputElement) {
        inputElement.value = modelData[key];
      }
    }
  }

  if ($("#ARCustomerCheckbox")) {
    $("#ARCustomerCheckbox").prop("checked", modelData.arCustomerCheckbox);
  }

  if ($("#TaxExemptCheckbox")) {
    $("#TaxExemptCheckbox").prop("checked", modelData.taxExemptCheckbox);
  }

  if ($("#UsePurchaseOrderCheckbox")) {
    $("#UsePurchaseOrderCheckbox").prop("checked", modelData.usePurchaseOrderCheckbox);
  }

  if ($("#MultiUsePurchaseOrderCheckbox")) {
    $("#MultiUsePurchaseOrderCheckbox").prop("checked", modelData.multiUsePurchaseOrderCheckbox);
  }

  if ($("#useFuelRebateCheckbox")) {
    $("#useFuelRebateCheckbox").prop("checked", modelData.useFuelRebateCheckbox);
  }

  if ($("#useFuelRebateDiscountCheckbox")) {
    $("#useFuelRebateDiscountCheckbox").prop("checked", modelData.useFuelRebateDiscountCheckbox);
  }

  if ($("#dateOpen").length > 0) {
    $("#dateOpen").val(modelData.dateOpen.split('T')[0]);
  }

  if ($("#lastSaleDate").length > 0) {
    $("#lastSaleDate").val(modelData.lastSaleDate.split('T')[0]);
  }

  if ($("#lastPaymentDate").length > 0) {
    $("#lastPaymentDate").val(modelData.lastPaymentDate.split('T')[0]);
  }
}

let goToCustomersGridPage = () => {
  _globalIsEditing = isAddMode || !isAddingOrEditing;
  setGlobalEditingState(_globalIsEditing);
  const url = `/CustomersGrid`;
  safelyNavigateTo(url);
}

let toggleRestrictionProfile = () => {
  let isArCustomerChecked = false;
  if (document.getElementById("ARCustomerCheckbox"))
    isArCustomerChecked = document.getElementById("ARCustomerCheckbox").checked;

  $('.rstr-profile').prop('disabled', !(isArCustomerChecked && policies.rstrProfile));
}

let toggleFuletRebateFields = () => {
  $('.fuel-rebate').prop('disabled', !policies.fuelRebate);
  if (policies.fuelRebate) {
    useFuelRebateCheckboxChanged();
  }
}

let addCustomer = () => {
  $('.edit-mode-control').prop('disabled', false);
  $('.customer-code').prop('disabled', false);
  $('#btn-customer-card').hide();
  $('#btn-cancel').hide();
  $('#btn-save').show();
  $('#btn-edit').hide();
  isAddingOrEditing = true;
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

let loadGroupIDOptions = async () => {
  $("#group-number-select").html(optionGeneratorDefault(document.getElementById("groupID").value));
  await loadDataFromCustomerController('GetAllCustomerGroups', {}).then((data) => {
    data.forEach(option => $("#group-number-select").append(optionGenerator(option.groupID, option.groupName?? '')));
  });
}

let loadRestrictionProfileOptions = async () => {
  await loadDataFromCustomerController('GetAllRestrictionProfiles', {}).then((data) => {
    data.forEach(option => $("#restriction-profile-select").append(optionGenerator(option.profileID, option.profileName)));
  });
}

async function loadCustomerPolicies() {
  const url = STRING_TABLE.URL.GET.POLICIES;

  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

let loadSelectFromModel = (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId} option[value=${document.getElementById(inputDataId).value}]`).attr('selected', 'selected');
  }
}

let loadDataFromModel = () => {
  loadSelectFromModel("restriction-profile-select-from-model", "restriction-profile-select");
  loadSelectFromModel("group-number-select-from-model", "group-number-select");
  loadSelectFromModel("status-select-from-model", "status-select");
}

let goToCustomerCard = () => {
  _globalIsEditing = !isAddingOrEditing;
  setGlobalEditingState(_globalIsEditing);
  const url = `/CustomerCard?Code=${customerCode}`;
  safelyNavigateTo(url);
}

let enableFields = () => {
  $('.tax-exempt-field').prop('disabled', false);
  $('.fuel-rebate').prop('disabled', false);
  $('.rstr-profile').prop('disabled', false);
}

let validateCustomerForm = async () => {
  let customerCodeFromForm = document.getElementById("customer-code").value;
  let customerName = document.getElementById("name").value;
  let existingCustomer = null;
  if (!customerCodeFromForm || !customerName)
    return false;

  if (!customerCode) {
    let url = `Customer/GetCustomer?code=${customerCodeFromForm}`;
    existingCustomer = await ajaxGetAsync(url);
    if (existingCustomer) {
      toastr.warning($("#customerCodeAlreadyExists").val());
      return false;
    }
  }

  let loyaltyNumFromForm = document.getElementById("loyalty-number").value;

  if (loyaltyNumFromForm) {
    let url = `Customer/GetCustomerByLoyaltyNum?loyaltyNum=${loyaltyNumFromForm}`;
    let existingCustomerByloyaltyNum = await ajaxGetAsync(url);
    if (!isNullOrUndefined(existingCustomerByloyaltyNum) && customerCode !== existingCustomerByloyaltyNum.Code) {
      toastr.warning($("#loyaltyNumAlreadyExists").val());
      return false;
    }
  }

  return true;
}

$("#customer-discount").on('keypress', function (event) {
  return onlyDigitKeyPress(event);
});

let changedGroupNumberSelect = (groupNumber) => {
  document.getElementById("group-number").value = groupNumber;
  document.getElementById("group-number-select-from-model").value = groupNumber;
}

let useFuelRebateCheckboxChanged = () => {
  let isChecked = $('#useFuelRebateCheckbox').is(':checked');
  if (isChecked) {
    $('#useFuelRebateDiscountCheckbox').prop('disabled', false);
  }
  else {
    $('#useFuelRebateDiscountCheckbox').prop('checked', false);
    $('#useFuelRebateDiscountCheckbox').prop('disabled', true);
  }
}
