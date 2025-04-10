let familyData = [];
let familyDataTable;
let familyEditMode;
let selectedFamily;
let selectedFamilyRow;
let family = STOCK_DESCRIPTION_ELEMENTS.DATA_TABLE.FAMILY;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadFamilyData();
});

let loadFamilyData = async () => {
  let url = "StockDescriptionElements/GetAllFamily";
  await ajaxGetAsync(url)
    .then(_familyData => {
      if (_familyData) {
        familyData = _familyData;
        let isDisableEditDeleteButtons = true;

        if (familyData.length > 0) {
          selectedFamily = familyData[0].familyID;
          selectedFamilyRow = familyData[0];
          isDisableEditDeleteButtons = false;
        }
        enableOrDisableGridEditDeleteButtons(family, isDisableEditDeleteButtons);
        createFamilyDataTable();
        adjustSDEPanelHeight([characteristic, family]);
      }
    })
    .catch(toastrError);
}

let createFamilyDataTable = () => {
  destroyDataTableIfExists(familyDataTable);

  let props = createDataTableProp();
  props.data = familyData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "familyID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  familyDataTable = createDatatableWithSearching('family-grid', props);

  if (familyData.length > 0) {
    $(`#family-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#family-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (familyData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        familyDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedFamily = $(this).find("td:first")[0].innerText;
        selectedFamilyRow = familyDataTable.row(this).data();
      }
    });
}

let onFamilyAddOrEditClick = (editMode) => {
  familyEditMode = editMode;

  if (editMode) {
    if ($('#family-grid tbody').find('td.dataTables_empty').length > 0) {
      toastr.success($("#pleaseSelectAFamily").val());
      return;
    }
  }
  familyEditMode ? fillFamilyFields(selectedFamilyRow) : fillFamilyFields();

  $('.not-editable').prop('readOnly', familyEditMode);
  $('#validation-for-family-id').prop('hidden', true);
  $('#validation-for-family-description').prop('hidden', true);
  $("#familyFormModal").modal('show');
}

let onFamilyDeleteClick = async () => {
  let title = $("#deleteFamilyTitle").val();
  let message = `${$("#deleteMessage").val()} ${selectedFamily}?`;

  if ($('#family-grid tbody').find('td.dataTables_empty').length > 0) {
    toastr.success($("#pleaseSelectAFamily").val());
    return;
  }
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteFamily?familyID=${selectedFamily}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#familyDeletedSuccessfully").val());
            await loadFamilyData();
            await loadGroupData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onFamilyCompleteClick = async () => {
  let familyFormData = prepareFamilyFormData();

  if (!isFamilyFormValid(familyFormData)) return;

  if (!familyEditMode) {
    let newFamilyId = $("#family-id").val();
    let isExist = familyData.find(obj => obj.familyID.toLowerCase() == newFamilyId.toLowerCase());

    if (isExist) {
      toastr.warning($("#idAlreadyExists").val().replace("{0}", newFamilyId));
      return;
    }
  }

  let url = `StockDescriptionElements/SaveFamily?isUpdate=${familyEditMode}`;
  await ajaxPostAsync(url, familyFormData)
    .then(async (response) => {
      if (response) {
        toastr.success(familyEditMode ? $("#familyUpdatedSuccessfully").val() : $("#familyAddedSuccessfully").val());
        await loadFamilyData();
        await loadGroupData();
        $("#familyFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareFamilyFormData = () => {
  const formData = {
    familyID: $("#family-id").val(),
    description: $("#family-description").val()
  }
  return formData;
}

let isFamilyFormValid = (familyFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(familyFormData.familyID)) {
    $('#validation-for-family-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(familyFormData.description)) {
    $('#validation-for-family-description').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillFamilyFields = (formData = null) => {
  $("#family-id").val(formData?.familyID);
  $("#family-description").val(formData?.description);
}
