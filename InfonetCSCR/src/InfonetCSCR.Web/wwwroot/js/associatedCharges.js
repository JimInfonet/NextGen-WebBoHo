const STRING_TABLE = CHARGE_STRING_TABLE;
let chargeTableData = [];
let chargeDatatable;
let selectedChargeCode;
let selectedChargeRow;
let chargeEditMode;
let taxTableData = [];
let taxDatatable;
let selectedTax;
let selectedTaxRow;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadChargeTable();
});

let loadChargeTable = async () => {
  const url = STRING_TABLE.URL.GET_CHARGES;

  await ajaxGetAsync(url)
    .then(_chargeTableData => {
      chargeTableData = _chargeTableData;
      $("#tax-btn-add").prop("disabled", (chargeTableData.length === 0));
      $("#charge-btn-edit").prop("disabled", (chargeTableData.length === 0));
      $("#charge-btn-delete").prop("disabled", (chargeTableData.length === 0));

      if (chargeTableData.length > 0) {
        selectedChargeCode = chargeTableData[0].code;
        selectedChargeRow = chargeTableData[0];
        loadTaxes();
      }
      createAssociatedChargesDatatable();
    })
    .catch(toastrError);
}

let createAssociatedChargesDatatable = () => {
  destroyDataTableIfExists(chargeDatatable)

  let props = createDataTableProp();
  props.data = chargeTableData;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  props.columns = [
    { "data": "code", },
    { "data": "description", },
    { "data": "price", }
  ];

  chargeDatatable = createDataTableGeneric('charges-grid', props);

  if (chargeTableData.length > 0) {
    $(`#charges-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#charges-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (chargeTableData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        chargeDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedChargeCode = $(this).find("td:first")[0].innerText;

        selectedChargeRow = {
          code: $(this).find("td")[0].innerText,
          description: $(this).find("td")[1].innerText,
          price: $(this).find("td")[2].innerText,
        }
        await loadTaxes();
      }
    })
}

let onChargeAddClick = () => {
  chargeEditMode = false;
  openModal();
}

let onChargeEditClick = () => {
  chargeEditMode = true;
  openModal();
}

let openModal = () => {
  chargeEditMode ? fillChargeFields(selectedChargeRow) : fillChargeFields();

  $('.text-danger').prop('hidden', true);
  $('.not-editable').prop('readOnly', chargeEditMode);
  $("#charge-form-section-title").text(chargeEditMode ? $("#editCharge").val() : $("#addCharge").val());
  $("#chargeFormModal").modal('show');
}

let onChargeDeleteClick = async () => {
  let isChargeInUse = false;
  let url = `${STRING_TABLE.URL.IS_CHARGE_IN_USE}?code=${selectedChargeCode}`;
  await ajaxGetAsync(url)
    .then(_isChargeInUse => isChargeInUse = _isChargeInUse)
    .catch(toastrError);

  if (isChargeInUse) {
    toastr.warning($("#chargeCanNotBeDeleted").val().replace("{0}", `${selectedChargeRow.code} - ${selectedChargeRow.description}`));
    return;
  }

  let title = $("#deleteCharge").val();
  let message = $("#deleteChargeMessage").val();

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `${STRING_TABLE.URL.DELETE_CHARGE}?code=${selectedChargeCode}`;
      await ajaxGetAsync(url)
        .then(_ => {
          toastr.success($("#chargeDeletedSuccessfully").val());
          loadChargeTable();
        })
        .catch(toastrError);
    }
  });
}

let onChargeCompleteClick = async () => {
  let chargeFormData = prepareChargeFormData();
  if (!isChargeFormValid(chargeFormData)) return;

  if (!chargeEditMode) {
    let newChargeCode = $("#charge-code").val();
    let url = `${STRING_TABLE.URL.CHARGE_EXISTS}?code=${newChargeCode}`;

    let response = await ajaxGetAsync(url)
      .catch(toastrError);

    if (response) {
      toastr.warning($("#codeAlreadyExists").val())
      return;
    }
  }

  let url = `${STRING_TABLE.URL.SAVE_CHARGE}?isUpdate=${chargeEditMode}`;
  await ajaxPostAsync(url, chargeFormData)
    .then(_ => {
      toastr.success(chargeEditMode ? $("#chargeUpdatedSuccessfully").val() : $("#chargeAddedSuccessfully").val())
      loadChargeTable();
      $("#chargeFormModal").modal('hide');
    })
    .catch(toastrError)
}

let prepareChargeFormData = () => {
  const formData = {
    Code: $("#charge-code").val(),
    Description: $("#charge-description").val(),
    Price: $("#charge-price").val(),
  }

  return formData;
}

let isChargeFormValid = (formData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(formData.Code)) {
    $('#validation-for-charge-code').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(formData.Description)) {
    $('#validation-for-charge-description').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(formData.Price)) {
    $('#validation-for-charge-price').text($("#thisFieldIsRequired").val());
    $('#validation-for-charge-price').prop('hidden', false);
    isValid = false;
  }

  if (parseFloat(formData.Price) <= 0 || formData.Price.split('.').length > 2) {
    $('#validation-for-charge-price').text($("#priceMustBePositive").val());
    $('#validation-for-charge-price').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillChargeFields = (formData = null) => {
  $("#charge-code").val(formData?.code);
  $("#charge-description").val(formData?.description);
  $("#charge-price").val(formData?.price);
}

let loadTaxes = async () => {
  const url = `${STRING_TABLE.URL.GET_TAXES}?code=${selectedChargeCode}`;

  await ajaxGetAsync(url)
    .then(_taxTableData => {
      taxTableData = _taxTableData;
      $("#tax-btn-delete").prop("disabled", (taxTableData.length === 0));

      if (taxTableData.length > 0) {
        selectedTaxRow = {
          code: selectedChargeCode,
          taxCode: taxTableData[0].taxCode,
          taxName: taxTableData[0].taxName,
        }
        selectedTax = taxTableData[0].name;
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
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];

  props.columns = [
    { "data": "taxName", },
    { "data": "taxCode", },
    { "data": "rate", },
    {
      "data": "included",
      render: function (included) {
        return included ? $("#yes").val() : $("#no").val();
      },
    },
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
          code: selectedChargeCode,
          taxName: $(this).find("td")[0].innerText,
          taxCode: $(this).find("td")[1].innerText,
        }
      }
    })
}

let onTaxAddClick = async () => {
  await loadAddTaxDropDown()
}

let loadAddTaxDropDown = async () => {
  $("#tax-select").empty();
  const url = `${STRING_TABLE.URL.GET_AVAILABLE_TAXES}?code=${selectedChargeCode}`;
  await ajaxGetAsync(url)
    .then((data) => {
      if (data.length === 0) {
        toastr.warning($("#allTaxesAreDefined").val());
      }
      else {
        data.forEach(option => $("#tax-select").append(taxOptionGenerator(option.taxName, option.taxCode)));
        $("#taxFormModal").modal('show');
      }
    })
    .catch(toastrError);
}

let taxOptionGenerator = (name, code) => {
  return `<option data-name=${name} data-code=${code}>${name} - ${code}</option>`;
}

let onTaxCompleteClick = async () => {
  const tax = prepareTax();
  const url = `${STRING_TABLE.URL.ADD_TAX}`;

  await ajaxPostAsync(url, tax)
    .then(_ => {
      toastr.success($("#taxAddedSuccessfully").val())
      $("#taxFormModal").modal('hide');
      loadTaxes();
    })
    .catch(toastrError)
}

let prepareTax = () => {
  let selectElement = document.getElementById('tax-select');
  let selectedOption = selectElement.options[selectElement.selectedIndex]

  const tax = {
    code: selectedChargeCode,
    taxCode: selectedOption.dataset.code,
    taxName: selectedOption.dataset.name
  }

  return tax;
}

let onTaxDeleteClick = async () => {
  let title = $("#deleteTax").val();
  let message = $("#deleteTaxMessage").val().replace("{0}", selectedTaxRow.code).replace("{1}", selectedTaxRow.taxName).replace("{2}", selectedTaxRow.taxCode);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `${STRING_TABLE.URL.DELETE_TAX}`;
      await ajaxPostAsync(url, selectedTaxRow)
        .then(_ => {
          toastr.success($("#taxDeletedSuccessfully").val());
          loadTaxes();
        })
        .catch(toastrError);
    }
  });
}
