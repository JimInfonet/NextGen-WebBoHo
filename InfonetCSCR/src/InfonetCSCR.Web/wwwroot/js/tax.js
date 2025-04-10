const STRING_TABLE = TAX_STRING_TABLE;
let policies = {};
let taxTableData = [];
let taxCodeTableData = [];
let taxDatatable;
let taxCodeDatatable;
let selectedTax;
let selectedTaxRow;
let taxEditMode;
let taxCodeEditMode;
let selectedTaxCodeRow;

$(async function () {
  await loadPolicies();
  await loadTaxTable();

  $('.text-danger').prop('hidden', true);
  $("#tax-rebate-input").prop('hidden', !policies.taxRebate);
});

async function loadPolicies() {
  const url = STRING_TABLE.URL.GET_POLICIES;

  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

let loadTaxTable = async () => {
  const url = STRING_TABLE.URL.GET_TAXES;

  await ajaxGetAsync(url)
    .then(_taxTableData => {
      taxTableData = _taxTableData;
      $("#tax-btn-edit").prop("disabled", (taxTableData.length === 0));
      $("#tax-btn-delete").prop("disabled", (taxTableData.length === 0));

      if (taxTableData.length > 0) {
        selectedTax = taxTableData[0].name;
        selectedTaxRow = taxTableData[0];
        loadTaxCodes();
      }
      createTaxDatatable();
    })
    .catch(toastrError);
}

let createTaxDatatable = () => {
  destroyDataTableIfExists(taxDatatable)

  let props = createDataTableProp();
  props.data = taxTableData;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4] }
  ];

  props.columns = [
    { "data": "name", },
    {
      "data": "active",
      render: function (active) {
        return active ? $("#yes").val() : $("#no").val();
      },
    },
    { "data": "order", },
    { "data": "description", },
    { "data": "default", }
  ];

  taxDatatable = createDataTableGeneric('taxes-grid', props);

  if (taxTableData.length > 0) {
    $(`#taxes-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#taxes-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (taxTableData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        taxDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedTax = $(this).find("td:first")[0].innerText;

        selectedTaxRow = {
          name: $(this).find("td")[0].innerText,
          active: $(this).find("td")[1].innerText == $("#yes").val(),
          order: $(this).find("td")[2].innerText,
          description: $(this).find("td")[3].innerText,
          default: $(this).find("td")[4].innerText
        }
        await loadTaxCodes();
      }
    })
}

let onTaxAddClick = () => {
  taxEditMode = false;
  fillTaxFields();
  prepareTaxModalAndOpen();
}

let onTaxEditClick = () => {
  taxEditMode = true;
  fillTaxFields(selectedTaxRow);
  prepareTaxModalAndOpen();
}

let prepareTaxModalAndOpen = () => {
  $('.not-editable').prop('readOnly', taxEditMode);
  $('.text-danger').prop('hidden', true);
  $("#taxFormModal").modal('show');
}

let onTaxDeleteClick = async () => {
  let taxUsageCount = 0;
  let url = `${STRING_TABLE.URL.GET_TAX_USAGE_COUNT}?taxName=${selectedTax}`;

  await ajaxGetAsync(url)
    .then(_count => taxUsageCount = _count);

  let title = $("#deleteTax").val();
  let message = $("#taxUsageMessage").val().replace("{0}", selectedTax).replace("{1}", taxUsageCount) + "\n" + $("#deleteTaxMessage").val();

  if (message.includes("times") && (taxUsageCount === 0 || taxUsageCount === 1)) {
      message = message.replace("times", "time");
  }
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `${STRING_TABLE.URL.DELETE_TAX}?taxName=${selectedTax}`;
      await ajaxGetAsync(url)
        .then(_ => {
          toastr.success($("#taxDeletedSuccessfully").val());
          loadTaxTable();
        })
        .catch(toastrError);
    }
  });
}

let onTaxCompleteClick = async () => {
  let taxFormData = prepareTaxFormData();
  if (!isTaxFormValid(taxFormData)) return;

  if (!taxEditMode) {
    let newTaxName = $("#tax-name").val();
    let url = `${STRING_TABLE.URL.TAX_NAME_EXISTS}?taxName=${newTaxName}`;

    let response = await ajaxGetAsync(url)
      .catch(toastrError);

    if (response) {
      toastr.warning($("#taxNameAlreadyExists").val())
      return;
    }
  }
  let url = `${STRING_TABLE.URL.SAVE_TAX}?isUpdate=${taxEditMode}`;

  await ajaxPostAsync(url, taxFormData)
    .then(_ => {
      toastr.success(taxEditMode ? $("#taxUpdatedSuccessfully").val() : $("#taxAddedSuccessfully").val())
      loadTaxTable();
      $("#taxFormModal").modal('hide');
    })
    .catch(toastrError)
}

let prepareTaxFormData = () => {
  const formData = {
    Name: $("#tax-name").val(),
    Active: $("#tax-active-select").val() == "Yes",
    Order: parseInt($("#tax-order").val()),
    Description: $("#tax-description").val(),
    Default: $("#tax-default").val(),
  }

  return formData;
}

let isTaxFormValid = (taxFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(taxFormData.Name)) {
    $('#validation-for-tax-name').prop('hidden', false);
    isValid = false;
  }
  if (isNaN(taxFormData.Order)) {
    $('#validation-for-tax-order').prop('hidden', false);
    isValid = false;
  }
  if (isNullOrWhiteSpace(taxFormData.Description)) {
    $('#validation-for-tax-description').prop('hidden', false);
    isValid = false;
  }
  if (isNullOrWhiteSpace(taxFormData.Default)) {
    $('#validation-for-tax-default').prop('hidden', false);
    isValid = false;
  }

  if (taxFormData.Order < 0 || taxFormData.Order > 255) {
    toastr.warning($("#orderRange").val());
    isValid = false;
  }

  return isValid;
}

let fillTaxFields = (formData = null) => {
  $("#tax-name").val(formData?.name);
  $("#tax-description").val(formData?.description);
  $("#tax-active-select").val(formData?.active ? "Yes" : "No");
  $("#tax-default").val(formData?.default);
  $("#tax-order").val(formData?.order);
}

// Tax Code Section

let loadTaxCodes = async () => {
  const url = `${STRING_TABLE.URL.GET_TAX_RATES}?taxName=${selectedTax}`;

  await ajaxGetAsync(url)
    .then(_taxCodeTableData => {
      taxCodeTableData = _taxCodeTableData;
      $("#tax-code-btn-edit").prop("disabled", (taxCodeTableData.length === 0));
      $("#tax-code-btn-delete").prop("disabled", (taxCodeTableData.length === 0));

      if (taxCodeTableData.length > 0) {
        selectedTaxCodeRow = taxCodeTableData[0];
        if (!selectedTaxCodeRow.taxRebate) {
          selectedTaxCodeRow.taxRebate = 0.00;
        }
      }
      createTaxCodeDatatable();
    })
    .catch(toastrError);
}

let createTaxCodeDatatable = () => {

  destroyDataTableIfExists(taxCodeDatatable)

  let props = createDataTableProp();
  props.data = taxCodeTableData;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4] }
  ];

  props.columns = [
    { "data": "code", },
    { "data": "rate", },
    {
      "data": "included",
      render: function (included) {
        return included ? $("#yes").val() : $("#no").val();
      },
    },
    { "data": "description", },
    {
      "data": "taxRebate",
      render: function (taxRebate) {
        return `${(taxRebate ?? 0).toFixed(2)}%`;
      },
    }
  ];

  taxCodeDatatable = createDataTableGeneric('tax-codes-grid', props);
  taxCodeDatatable.column(4).visible(policies.taxRebate);
  
  if (taxCodeTableData.length > 0) {
    $(`#tax-codes-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#tax-codes-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (taxCodeTableData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        taxCodeDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');

        selectedTaxCodeRow = {
          code: $(this).find("td")[0].innerText,
          rate: $(this).find("td")[1].innerText,
          included: $(this).find("td")[2].innerText == $("#yes").val(),
          description: $(this).find("td")[3].innerText,
          taxRebate: $(this).find("td")[4].innerText.slice(0, -1)
        }
      }
    })
}

let onTaxCodeAddClick = () => {
  taxCodeEditMode = false;
  fillTaxCodeFields();
  prepareTaxCodeModalAndOpen();
}

let onTaxCodeEditClick = () => {
  taxCodeEditMode = true;
  fillTaxCodeFields(selectedTaxCodeRow);
  prepareTaxCodeModalAndOpen();
}

let prepareTaxCodeModalAndOpen = () => {
  $('.not-editable').prop('readOnly', taxCodeEditMode);
  $('.text-danger').prop('hidden', true);
  $("#taxCodeFormModal").modal('show');
}

let fillTaxCodeFields = (formData = null) => {
  $("#tax-code").val(formData?.code);
  $("#tax-code-included-select").val(formData?.included ? "Yes" : "No");
  $("#tax-rebate").val(formData ? parseFloat(formData.taxRebate).toFixed(2) : "");
  $("#tax-code-rate").val(formData?.rate);
  $("#tax-code-description").val(formData?.description);

  if (taxCodeEditMode && isNaN(formData?.rate))
    $("#tax-rebate").val("0.00");
}

let onTaxCodeCompleteClick = async () => {
  let taxCodeFormData = prepareTaxCodeFormData();
  if (!isTaxCodeFormValid(taxCodeFormData)) return;

  if (!taxCodeEditMode) {
    let newTaxCode = $("#tax-code").val();
    let url = `${STRING_TABLE.URL.TAX_CODE_EXISTS}?taxName=${selectedTax}&taxCode=${newTaxCode}`;

    let response = await ajaxGetAsync(url)
      .catch(toastrError);

    if (response) {
      toastr.warning($("#taxCodeAlreadyExists").val())
      return;
    }
  }

  let url = `${STRING_TABLE.URL.SAVE_TAX_RATE}?isUpdate=${taxCodeEditMode}`;

  await ajaxPostAsync(url, taxCodeFormData)
    .then(_ => {
      toastr.success(taxCodeEditMode ? $("#taxCodeUpdatedSuccessfully").val() : $("#taxCodeAddedSuccessfully").val())
      loadTaxCodes();
      $("#taxCodeFormModal").modal('hide');
    })
    .catch(toastrError)
}

let prepareTaxCodeFormData = () => {
  let rate = parseFloat($("#tax-code-rate").val());
  let taxRebate = parseFloat($("#tax-rebate").val()) || 0;
  if (rate < taxRebate) taxRebate = 0;

  const formData = {
    Name: selectedTax,
    Code: $("#tax-code").val(),
    Included: $("#tax-code-included-select").val() == "Yes",
    TaxRebate: taxRebate,
    Description: $("#tax-code-description").val(),
    Rate: rate,
  }

  return formData;
}

let isTaxCodeFormValid = (taxCodeForm) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(taxCodeForm.Code)) {
    $('#validation-for-tax-code').prop('hidden', false);
    isValid = false;
  }
  if (isNullOrWhiteSpace(taxCodeForm.Description)) {
    $('#validation-for-tax-code-description').prop('hidden', false);
    isValid = false;
  }
  if (isNullUndefinedOrWhiteSpace($("#tax-code-rate").val())) {
    $('#validation-for-tax-code-rate').text($("#thisFieldIsRequired").val())
    $('#validation-for-tax-code-rate').prop('hidden', false);
    isValid = false;
  }
  else if (isNaN(taxCodeForm.Rate)) {
    $('#validation-for-tax-code-rate').text($("#rateHasToBeNumeric").val());
    $('#validation-for-tax-code-rate').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let onTaxCodeDeleteClick = async () => {
  let taxCodeUsageCount = 0;

  let url = `${STRING_TABLE.URL.GET_TAX_USAGE_COUNT}?taxName=${selectedTax}&taxCode=${selectedTaxCodeRow.code}`;
  await ajaxGetAsync(url)
    .then(_count => taxCodeUsageCount = _count);

  let title = $("#deleteCode").val();
  let message = (taxCodeUsageCount ? `${$("#taxCodeUsageMessage").val().replace("{0}", `${selectedTax} - ${selectedTaxCodeRow.code}`).replace("{1}", taxCodeUsageCount)}\n` : "") + $("#deleteTaxCodeMessage").val();

  if (message.includes("times") && (taxCodeUsageCount === 0 || taxCodeUsageCount === 1)) {
      message = message.replace("times", "time");
  }

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `${STRING_TABLE.URL.DELETE_TAX_RATE}?taxName=${selectedTax}&taxCode=${selectedTaxCodeRow.code}`;
      await ajaxGetAsync(url)
        .then(_ => {
          toastr.success($("#taxCodeDeletedSuccessfully").val());
          loadTaxCodes();
        })
        .catch(toastrError);
    }
  });
}