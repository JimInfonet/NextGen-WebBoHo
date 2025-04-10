const isUpdate = document.getElementById("isUpdate").value === "True";
const view = CONSTANT_TEXTS.VIEW;
const create = CONSTANT_TEXTS.CREATE;
const update = CONSTANT_TEXTS.UPDATE;
const minimumDateOfBirth = '1753-01-01';

$(function () {
  $('#btn-cancel').hide();
  $('#btn-complete').hide();
  const urlParams = new URLSearchParams(window.location.search);
  const action = urlParams.get('actionMode');
  const isShowToaster = urlParams.get('showToaster');
  const operationType = urlParams.get('operationType');
  
  if (isUpdate) {
    $('.not-editable').prop('readOnly', true);
    $("#Code").prop("disabled", true);
    $("#Password").prop("disabled", true);

    setEditMode(action === view);
  } else {
    $('#btn-cancel').hide();
    $('#btn-complete').show();
    $('#btn-edit').hide();
    _globalIsEditing = true;
    setGlobalEditingState(_globalIsEditing);

    if ($("#IsUserAlreadyExists").val() === "true") {
      const message = document.getElementById("userAlreadyExists").value.replace("{0}", $("#Code").val());
      toastr.error(message);
    }
  }

  if (isShowToaster) {
    if (operationType === create) {
      toastr.success($("#userCreatedSuccessfully").val());
    } else if (operationType === update) {
      toastr.success($("#userUpdatedSuccessfully").val());
    }

    const newUrl = removeURLParameter(window.location.search, 'showToaster');
    window.history.replaceState(null, null, newUrl);
  }

});

$("#btn-back").on('click', async function () {
  const url = "/UserGrid";
  await safelyNavigateTo(url);
});

let setEditMode = (editMode) => {
  _globalIsEditing = editMode;
  setGlobalEditingState(_globalIsEditing);
  $('.edit-mode-control').prop('disabled', !editMode);

  if (!editMode) {
    $('#btn-cancel').hide();
    $('#btn-complete').hide();
    $('#btn-edit').show();
  }
  else {
    $('#btn-cancel').show();
    $('#btn-complete').show();
    $('#btn-edit').hide();
  }
}

$("#btn-complete").on('click', function (event) {
  const dateInput = document.getElementById('DateOfBirth').value;
  const currentDate = getCurrentDate();

  if (!isNullUndefinedOrWhiteSpace(dateInput)) {
    if (dateInput > currentDate) {
      toastr.warning($("#DOBCanNotBeFuture").val());
      event.preventDefault();
      return;
    } else if (dateInput < minimumDateOfBirth) {
      toastr.warning($("#DOBCanNotBeBefore").val());
      event.preventDefault();
      return;
    }
  }

  $("#Code").prop("disabled", false);
});

async function cancelButtonClicked() {
  _globalIsEditing = false;
  setGlobalEditingState(_globalIsEditing);
  const userCode = $("#Code").val();
  const url = `/UserDetailsForm/Index?code=${userCode}`;
  await safelyNavigateTo(url);
}
