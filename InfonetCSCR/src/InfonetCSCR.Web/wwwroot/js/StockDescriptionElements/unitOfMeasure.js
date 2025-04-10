let unitOfMeasureData = [];
let unitOfMeasureDataTable;
let unitOfMeasureEditMode;
let selectedUnitOfMeasure;
let selectedUnitOfMeasureRow;
let unitOfMeasure = STOCK_DESCRIPTION_ELEMENTS.DATA_TABLE.UNIT_OF_MEASURE;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadUnitOfMeasureData();
});

let loadUnitOfMeasureData = async () => {
  let url = "StockDescriptionElements/GetAllUnitOfMeasures";
  await ajaxGetAsync(url)
    .then(_unitOfMeasureData => {
      if (_unitOfMeasureData) {
        unitOfMeasureData = _unitOfMeasureData;
        let isDisableEditDeleteButtons = true;

        if (unitOfMeasureData.length > 0) {
          selectedUnitOfMeasure = unitOfMeasureData[0].unitOfMeasureID;
          selectedUnitOfMeasureRow = unitOfMeasureData[0];
          isDisableEditDeleteButtons = false;
        }
        enableOrDisableGridEditDeleteButtons(unitOfMeasure, isDisableEditDeleteButtons);
        createUnitOfMeasureDataTable();
        adjustSDEPanelHeight([unitOfMeasure, orderUnit]);
      }
    })
    .catch(toastrError);
}

let createUnitOfMeasureDataTable = () => {
  destroyDataTableIfExists(unitOfMeasureDataTable);

  let props = createDataTableProp();
  props.data = unitOfMeasureData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "unitOfMeasureID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  unitOfMeasureDataTable = createDatatableWithSearching('unitOfMeasure-grid', props);

  if (unitOfMeasureData.length > 0) {
    $(`#unitOfMeasure-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#unitOfMeasure-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (unitOfMeasureData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        unitOfMeasureDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedUnitOfMeasure = $(this).find("td:first")[0].innerText;
        selectedUnitOfMeasureRow = unitOfMeasureDataTable.row(this).data();
      }
    });
}

let onUnitOfMeasureAddOrEditClick = (editMode) => {
  unitOfMeasureEditMode = editMode;

  if (editMode) {
    if ($('#unitOfMeasure-grid tbody').find('td.dataTables_empty').length > 0) {
      toastr.success($("#pleaseSelectAUnitOfMeasure").val());
      return;
    }
  }
  unitOfMeasureEditMode ? fillUnitOfMeasureFields(selectedUnitOfMeasureRow) : fillUnitOfMeasureFields();

  $('.not-editable').prop('readOnly', unitOfMeasureEditMode);
  $('#validation-for-unitOfMeasure-id').prop('hidden', true);
  $('#validation-for-unitOfMeasure-description').prop('hidden', true);
  $("#unitOfMeasureFormModal").modal('show');
}

let onUnitOfMeasureDeleteClick = async () => {
  let title = $("#deleteUnitOfMeasureTitle").val();
  let message = `${$("#deleteMessage").val()} ${selectedUnitOfMeasure}?`;

  if ($('#unitOfMeasure-grid tbody').find('td.dataTables_empty').length > 0) {
    toastr.success($("#pleaseSelectAUnitOfMeasure").val());
    return;
  }
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteUnitOfMeasure?unitOfMeasureID=${selectedUnitOfMeasure}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#unitOfMeasureDeletedSuccessfully").val());
            await loadUnitOfMeasureData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onUnitOfMeasureCompleteClick = async () => {
  let unitOfMeasureFormData = prepareUnitOfMeasureFormData();

  if (!isUnitOfMeasureFormValid(unitOfMeasureFormData)) return;

  if (!unitOfMeasureEditMode) {
    let newUnitOfMeasureId = $("#unitOfMeasure-id").val();
    let isExist = unitOfMeasureData.find(obj => obj.unitOfMeasureID.toLowerCase() == newUnitOfMeasureId.toLowerCase());

    if (isExist) {
      toastr.warning($("#idAlreadyExists").val().replace("{0}", newUnitOfMeasureId));
      return;
    }
  }

  let url = `StockDescriptionElements/SaveUnitOfMeasure?isUpdate=${unitOfMeasureEditMode}`;
  await ajaxPostAsync(url, unitOfMeasureFormData)
    .then(async (response) => {
      if (response) {
        toastr.success(unitOfMeasureEditMode ? $("#unitOfMeasureUpdatedSuccessfully").val() : $("#unitOfMeasureAddedSuccessfully").val());
        await loadUnitOfMeasureData();
        $("#unitOfMeasureFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareUnitOfMeasureFormData = () => {
  const formData = {
    unitOfMeasureID: $("#unitOfMeasure-id").val(),
    description: $("#unitOfMeasure-description").val()
  }
  return formData;
}

let isUnitOfMeasureFormValid = (unitOfMeasureFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(unitOfMeasureFormData.unitOfMeasureID)) {
    $('#validation-for-unitOfMeasure-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(unitOfMeasureFormData.description)) {
    $('#validation-for-unitOfMeasure-description').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillUnitOfMeasureFields = (formData = null) => {
  $("#unitOfMeasure-id").val(formData?.unitOfMeasureID);
  $("#unitOfMeasure-description").val(formData?.description);
}
