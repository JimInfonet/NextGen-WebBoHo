let reasonsData;
let reasonsDataTable;
let selectedType;
let selectedReason;
let selectedReasonRow;
let policies = {};
let reasonEditMode = false;

$(async function () {
  await loadPolicies();

  $("#payouts-input").prop('disabled', !policies.poReason);
  $("#void-sales-input").prop('disabled', !policies.voidReason);
  $("#refunds-input").prop('disabled', !policies.retReason);
  $("#discounts-input").prop('disabled', !policies.discReason);
  $("#price-changes-input").prop('disabled', !policies.prReason);
  $("#open-cash-drawer-input").prop('disabled', !policies.ocdReason);
  $("#cash-draw-input").prop('disabled', !policies.drawReason);

  if (policies.poReason) { selectedType = 'P'; $("#payouts-input").prop('checked', policies.poReason); }
  else if (policies.voidReason) { selectedType = 'V'; $("#void-sales-input").prop('checked', policies.voidReason); }
  else if (policies.retReason) { selectedType = 'R'; $("#refunds-input").prop('checked', policies.retReason); }
  else if (policies.discReason) { selectedType = 'D'; $("#discounts-input").prop('checked', policies.discReason); }
  else if (policies.prReason) { selectedType = 'C'; $("#price-changes-input").prop('checked', policies.prReason); }
  else if (policies.ocdReason) { selectedType = 'O'; $("#open-cash-drawer-input").prop('checked', policies.ocdReason); }
  else { selectedType = 'M'; $("#markDown-sales-input").prop('checked', true); }

  await loadReasonsData();
});

async function loadPolicies() {
  const url = "Reasons/GetPolicies";
  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

let createReasonsDatatable = () => {
  destroyDataTableIfExists(reasonsDataTable);
  let props = createDataTableProp();
  props.data = reasonsData;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = false;
  props.columns = [
    { "data": "code", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  reasonsDataTable = createDataTableGeneric('reasons-grid', props);

  if (reasonsData.length > 0) {
    $(`#reasons-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#reasons-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (reasonsData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        reasonsDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedReason = $(this).find("td:first")[0].innerText;
        selectedReasonRow = {
          code: $(this).find("td")[0].innerText,
          description: $(this).find("td")[1].innerText,
        }
      }
    })
}

let loadReasonsData = async () => {
  let url = `Reasons/GetReasons?reasonType=${selectedType}`;
  await ajaxGetAsync(url)
    .then((_reasonsData) => {
      if (_reasonsData) {
        reasonsData = _reasonsData;
        $('#btn-edit').prop('disabled', (reasonsData.length === 0));
        $('#btn-delete').prop('disabled', (reasonsData.length === 0));

        if (reasonsData.length > 0) {
          selectedReason = reasonsData[0].code;
          selectedReasonRow = reasonsData[0];
        }

        createReasonsDatatable();
      }
    })
    .catch(toastrError);
}

let onReasonAddOrEditClick = (isEdit) => {
  reasonEditMode = isEdit;

  if (!isEdit) {
    fillReasonFields();
  }
  else {
    fillReasonFields(selectedReasonRow);
  }
  $('.not-editable').prop('readOnly', isEdit);
  $('#validation-for-code').prop('hidden', true);
  $('#validation-for-reason').prop('hidden', true);
  $("#reasonFormModal").modal('show');
}

$('.not-editable').on('keydown', function (e) {
    if ($(this).prop('readonly')) {
        e.preventDefault();
    }
});

let onReasonDeleteClick = async () => {
  let title = $("#deleteReasonTitle").val();
  let message = $("#deleteReasonMessage").val();
  message += selectedReason;
  message += " - ";
  message += selectedReasonRow.description;
  message += "?";

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Reasons/DeleteReason?code=${selectedReason}&type=${selectedType}`;
      await ajaxGetAsync(url)
        .then(async () => {
          toastr.success($("#reasonDeletedSuccessfully").val());
          await loadReasonsData();
        })
        .catch(toastrError);
    }
  });
}

let onReasonCompleteClick = async () => {
  let reasonFormData = prepareReasonFormData();
  if (!isReasonFormValid(reasonFormData)) return;

  if (!reasonEditMode) {
    let newReasonCode = $("#reasonCode").val();
    let url = `Reasons/ReasonAlreadyExists?code=${newReasonCode}&type=${selectedType}`;

    let response = await ajaxGetAsync(url)
      .catch(toastrError);

    if (response) {
      let message = $("#code").val();
      message += ' ';
      message += $("#reasonCode").val();
      message += ' ';
      message += $('#alreadyExists').val();
      toastr.warning(message);
      return;
    }
  }

  let url = `Reasons/SaveReason?isUpdate=${reasonEditMode}`;

  await ajaxPostAsync(url, reasonFormData)
    .then(async () => {
      toastr.success(reasonEditMode ? $("#reasonUpdatedSuccessfully").val() : $("#reasonAddedSuccessfully").val());
      await loadReasonsData();
      $("#reasonFormModal").modal('hide');
    })
    .catch(toastrError)
}

let isReasonFormValid = (reasonFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);
  let parsedCode = reasonFormData.code;

  if (!parsedCode) {
    $('#validation-for-code').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(reasonFormData.description)) {
    $('#validation-for-reason').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let prepareReasonFormData = () => {
  const formData = {
    code: $("#reasonCode").val().toUpperCase(),
    description: $("#reason").val(),
    type: selectedType
  }

  return formData;
}

$('input[type="radio"]').change(async function () {
  if ($(this).is(':checked')) {
    selectedType = $(this).val();
    await loadReasonsData();
  }
});

let fillReasonFields = (formData = null) => {
  $("#reasonCode").val(formData?.code);
  $("#reason").val(formData?.description);
}
