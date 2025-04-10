let restrictionsData = [];
let restrictionsDataTable;
let selectedRestriction;
let selectedRestrictionRow;
let restrictionEditMode;
let previousAgeLimit;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadRestrictionsData();
});

let loadRestrictionsData = async () => {
  let url = "Restrictions/GetRestrictions";
  await ajaxGetAsync(url)
    .then(_restrictionsData => {
      if (_restrictionsData) {
        restrictionsData = _restrictionsData;

        if (restrictionsData.length > 0) {
          selectedRestriction = restrictionsData[0].code;
          selectedRestrictionRow = restrictionsData[0];
          enableEditDeleteButtons();
        }
        else {
          disableEditDeleteButtons();
        }
        createRestrictionsDataTable();
      }
    })
    .catch(toastrError);
}

let createRestrictionsDataTable = () => {
  destroyDataTableIfExists(restrictionsDataTable);

  let props = createDataTableProp();
  props.data = restrictionsData;
  props.scrollY = '440px';
  props.paging = false;
  props.searching = false;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "code", },
    { "data": "description", },
    { "data": "ageLimit", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  restrictionsDataTable = createDataTableGeneric('restrictions-grid', props);

  if (restrictionsData.length > 0) {
    $(`#restrictions-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#restrictions-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (restrictionsData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        restrictionsDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedRestriction = $(this).find("td:first")[0].innerText;
        selectedRestrictionRow = restrictionsDataTable.row(this).data();
      }
    });
}

let disableEditDeleteButtons = () => {
  $('#btn-edit').prop('disabled', true);
  $('#btn-delete').prop('disabled', true);
}

let enableEditDeleteButtons = () => {
  $('#btn-edit').prop('disabled', false);
  $('#btn-delete').prop('disabled', false);
}

let onRestrictionAddClick = () => {
  restrictionEditMode = false;
  fillRestrictionFields();
  $('.not-editable').prop('readOnly', false);
  $('#validation-for-code').prop('hidden', true);
  $('#validation-for-code-range').prop('hidden', true);
  $('#validation-for-age-limit').prop('hidden', true);
  $('#validation-for-restriction').prop('hidden', true);
  $("#restrictionFormModal").modal('show');
}

let onRestrictionEditClick = () => {
  restrictionEditMode = true;
  previousAgeLimit = selectedRestrictionRow.ageLimit;
  fillRestrictionFields(selectedRestrictionRow);
  $('.not-editable').prop('readOnly', true);
  $('#validation-for-code').prop('hidden', true);
  $('#validation-for-code-range').prop('hidden', true);
  $('#validation-for-age-limit').prop('hidden', true);
  $('#validation-for-restriction').prop('hidden', true);
  $("#restrictionFormModal").modal('show');
}

$('.not-editable').on('keydown', function (e) {
    if ($(this).prop('readonly')) {
        e.preventDefault();
    }
});

let onRestrictionDeleteClick = async () => {
  let title = $("#deleteRestrictionTitle").val();
  let message = $("#deleteRestrictionMessage").val();
  message += selectedRestriction;
  message += " - ";
  message += selectedRestrictionRow.description;
  message += "?";

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Restrictions/DeleteRestriction?code=${selectedRestriction}`;
      await ajaxGetAsync(url)
        .then(async () => {
          toastr.success($("#restrictionDeletedSuccessfully").val());
          await loadRestrictionsData();
        })
        .catch(toastrError);
    }
  });
}

let onRestrictionCompleteClick = async () => {
  let restrictionFormData = prepareRestrictionFormData();
  if (!isRestrictionFormValid(restrictionFormData)) return;

  if (!restrictionEditMode) {
    let newRestrictionCode = $("#code").val();
    let url = `Restrictions/RestrictionAlreadyExists?code=${newRestrictionCode}`;

    let response = await ajaxGetAsync(url)
      .catch(toastrError);

    if (response) {
      toastr.warning($("#codeAlreadyExists").val().replace("{0}", newRestrictionCode));
      return;
    }
  }

  let url = `Restrictions/SaveRestriction?isUpdate=${restrictionEditMode}`;

  await ajaxPostAsync(url, restrictionFormData)
    .then(async () => {
      toastr.success(restrictionEditMode ? $("#restrictionUpdatedSuccessfully").val() : $("#restrictionAddedSuccessfully").val());
      await loadRestrictionsData();
      $("#restrictionFormModal").modal('hide');
    })
    .catch(toastrError)
}

let prepareRestrictionFormData = () => {
  const formData = {
    code: $("#code").val(),
    description: $("#restriction").val(),
    ageLimit: ageLimitExtraction($("#age-limit").val())
  }
  return formData;
}

let ageLimitExtraction = (ageLimitCheck) => {
  if (restrictionEditMode) {
    if (isNullOrWhiteSpace(ageLimitCheck)) {
      return parseInt(previousAgeLimit);
    }
    else {
      return Math.round(parseFloat(ageLimitCheck));
    }
  }
  else {
    return Math.round(parseFloat(ageLimitCheck));
  }
}

let isRestrictionFormValid = (restrictionFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);
  let parsedCode = parseInt(restrictionFormData.code);
  let parsedAgeLimit = restrictionFormData.ageLimit;

  if (!restrictionFormData.code) {
    $('#validation-for-code').prop('hidden', false);
    isValid = false;
  }

  if (parsedCode < 0 || parsedCode > 10) {
    $('#validation-for-code-range').prop('hidden', false);
    isValid = false;
  }

  if (parsedAgeLimit < 0 || parsedAgeLimit > 32767) {
    $('#validation-for-age-limit').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(restrictionFormData.description)) {
    $('#validation-for-restriction').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillRestrictionFields = (formData = null) => {
  $("#code").val(formData?.code);
  $("#restriction").val(formData?.description);
  $("#age-limit").val(formData?.ageLimit);
}