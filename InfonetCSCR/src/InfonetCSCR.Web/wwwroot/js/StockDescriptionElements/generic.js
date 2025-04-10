let genericData = [];
let genericDataTable;
let selectedGeneric;
let selectedGenericRow;
let genericEditMode;
let generic = STOCK_DESCRIPTION_ELEMENTS.DATA_TABLE.GENERIC;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadGenericData();
});

let loadGenericData = async () => {
  let url = "StockDescriptionElements/GetAllGenerics";
  await ajaxGetAsync(url)
    .then(_genericData => {
      if (_genericData) {
        genericData = _genericData;
        let isDisableEditDeleteButtons = true;

        if (genericData.length > 0) {
          selectedGeneric = genericData[0].genericID;
          selectedGenericRow = genericData[0];
          isDisableEditDeleteButtons = false;
        }
        enableOrDisableGridEditDeleteButtons(generic, isDisableEditDeleteButtons);
        createGenericDataTable();
        adjustSDEPanelHeight([brand, generic]);
      }
    })
    .catch(toastrError);
}

let createGenericDataTable = () => {
  destroyDataTableIfExists(genericDataTable);

  let props = createDataTableProp();
  props.data = genericData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "genericID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  genericDataTable = createDatatableWithSearching('generic-grid', props);

  if (genericData.length > 0) {
    $(`#generic-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#generic-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (genericData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        genericDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedGeneric = $(this).find("td:first")[0].innerText;
        selectedGenericRow = genericDataTable.row(this).data();
      }
    });
}

let onGenericAddOrEditClick = (editMode) => {
  genericEditMode = editMode;

  if (editMode) {
    if ($('#generic-grid tbody').find('td.dataTables_empty').length > 0) {
      toastr.success($("#pleaseSelectAGeneric").val());
      return;
    }
  }
  genericEditMode ? fillGenericFields(selectedGenericRow) : fillGenericFields();

  $('.not-editable').prop('readOnly', genericEditMode);
  $('#validation-for-generic-id').prop('hidden', true);
  $('#validation-for-generic-description').prop('hidden', true);
  $("#genericFormModal").modal('show');
}

let onGenericDeleteClick = async () => {
  let title = $("#deleteGenericTitle").val();
  let message = `${$("#deleteMessage").val()} ${selectedGeneric}?`;

  if ($('#generic-grid tbody').find('td.dataTables_empty').length > 0) {
    toastr.success($("#pleaseSelectAGeneric").val());
    return;
  }
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteGeneric?genericID=${selectedGeneric}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#genericDeletedSuccessfully").val());
            await loadGenericData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onGenericCompleteClick = async () => {
  let genericFormData = prepareGenericFormData();

  if (!isGenericFormValid(genericFormData)) return;

  if (!genericEditMode) {
    let newGenericId = $("#generic-id").val();
    let isExist = genericData.find(obj => obj.genericID.toLowerCase() == newGenericId.toLowerCase());

    if (isExist) {
      toastr.warning($("#idAlreadyExists").val().replace("{0}", newGenericId));
      return;
    }
  }

  let url = `StockDescriptionElements/SaveGeneric?isUpdate=${genericEditMode}`;
  await ajaxPostAsync(url, genericFormData)
    .then(async (response) => {
      if (response) {
        toastr.success(genericEditMode ? $("#genericUpdatedSuccessfully").val() : $("#genericAddedSuccessfully").val());
        await loadGenericData();
        $("#genericFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareGenericFormData = () => {
  const formData = {
    genericID: $("#generic-id").val(),
    description: $("#generic-description").val()
  }
  return formData;
}

let isGenericFormValid = (genericFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(genericFormData.genericID)) {
    $('#validation-for-generic-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(genericFormData.description)) {
    $('#validation-for-generic-description').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillGenericFields = (formData = null) => {
  $("#generic-id").val(formData?.genericID);
  $("#generic-description").val(formData?.description);
}
