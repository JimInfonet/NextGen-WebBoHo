let action;
let selectedStoreId;
let supportWarehousePolicy;
const view = CONSTANT_TEXTS.VIEW;
const add = CONSTANT_TEXTS.ADD;
const edit = CONSTANT_TEXTS.EDIT;
const copy = CONSTANT_TEXTS.COPY;
const BASE_URL_STORE = "/Store";

$(async function () {
  await loadPolicies();

  const params = new URLSearchParams(window.location.search);
  action = params.get('action');
  selectedStoreId = params.get('storeId');

  const modelDataInput = document.getElementById("modelData");
  let modelData = modelDataInput.value;
  const form = document.getElementById("storeForm");

  let isExist = false;

  for (let key in modelData) {
    if (modelData.hasOwnProperty(key)) {
      let capitalizedKey = capitalizeFirstLetter(key);
      const inputElement = form.querySelector(`[name="${capitalizedKey}"]`);

      if (inputElement) {
        inputElement.value = modelData[key];
      }
    }
  }

  if ($("#isAlreadyExist").val() === "True") {
    toastr.warning($("#storeIdAlreadyExist").val().replace("{0}", $("#storeId").val()));

    $('#btn-edit').hide();
    $('#btn-cancel').hide();

    isExist = true;
    action = $("#addEditOrCopy").val();
    selectedStoreId = $("#copyStoreId").val();
    _globalIsEditing = true;
    setGlobalEditingState(_globalIsEditing);
  }

  if (action === view) {
    $('.infonet-input').prop('disabled', true);
    $('#isWarehouse').prop('disabled', true);
    $('#btn-cancel').hide();
    $('#btn-complete').hide();
    setGlobalEditingState(false);
  } else if (action === edit) {
    $('#storeId').prop('disabled', true);
    $('#btn-edit').hide();

    setGlobalEditingState(true);
  } else if (action === add || action === copy) {
    $('#btn-edit').hide();
    $('#btn-cancel').hide();
    if (!isExist) {
      $('#storeId').val('');
    }

    setGlobalEditingState(true);
  }
});

async function loadPolicies() {
  const url = `${BASE_URL_STORE}/GetPolicies`;
  await ajaxGetAsync(url)
    .then(_policies => supportWarehousePolicy = _policies)
    .catch(toastrError);
}

$("#btn-back").on('click', async function () {
  const url = `${BASE_URL_STORE}`;
  await safelyNavigateTo(url);
});

$("#btn-complete").on('click', async function () {
  setGlobalEditingState(false);
});

$("#btn-cancel").on('click', async function () {
  $('.infonet-input').prop('disabled', true);
  $('#isWarehouse').prop('disabled', true);
  $('#btn-cancel').hide();
  $('#btn-complete').hide();
  $('#btn-edit').show();
  setGlobalEditingState(false);
  action = view;
});

$("#btn-edit").on('click', async function () {
  $('.infonet-input').prop('disabled', false);
  $('#isWarehouse').prop('disabled', false);
  $('#storeId').prop('disabled', true);
  $('#btn-cancel').show();
  $('#btn-complete').show();
  $('#btn-edit').hide();
  action = edit;
  setGlobalEditingState(true);
});

function handleUpdateStore(e) {
  e.preventDefault();
  const form = $(e.target);
  
  if (!form.valid()) {
    return; // Do not submit the form if there are validation errors
  }

  const isWarehouseVal = $("#isWarehouse").prop('checked');
  const uploadPathVal = $("#uploadPath").val().trim();

  if (supportWarehousePolicy && isWarehouseVal && isNullUndefinedOrWhiteSpace(uploadPathVal)) {
    toastr.warning($("#storeDataPathBlank").val());
    return;
  }

  $("#addEditOrCopy").val(action);
  $("#copyStoreId").val(action === copy ? selectedStoreId : '');

  if (action === edit) {
    $("#storeId").prop('disabled', false);
  }

  e.target.submit();
}
