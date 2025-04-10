let characteristicData = [];
let characteristicDataTable;
let characteristicEditMode;
let selectedCharacteristic;
let selectedCharacteristicRow;
let characteristic = STOCK_DESCRIPTION_ELEMENTS.DATA_TABLE.CHARACTERISTIC;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadCharacteristicData();
});

let loadCharacteristicData = async () => {
  let url = "StockDescriptionElements/GetAllCharacteristics";
  await ajaxGetAsync(url)
    .then(_characteristicData => {
      if (_characteristicData) {
        characteristicData = _characteristicData;
        let isDisableEditDeleteButtons = true;

        if (characteristicData.length > 0) {
          selectedCharacteristic = characteristicData[0].characteristicID;
          selectedCharacteristicRow = characteristicData[0];
          isDisableEditDeleteButtons = false;
        }
        enableOrDisableGridEditDeleteButtons(characteristic, isDisableEditDeleteButtons);
        createCharacteristicDataTable();
        adjustSDEPanelHeight([characteristic, family]);
      }
    })
    .catch(toastrError);
}

let createCharacteristicDataTable = () => {
  destroyDataTableIfExists(characteristicDataTable);

  let props = createDataTableProp();
  props.data = characteristicData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "characteristicID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  characteristicDataTable = createDatatableWithSearching('characteristic-grid', props);

  if (characteristicData.length > 0) {
    $(`#characteristic-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#characteristic-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (characteristicData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        characteristicDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedCharacteristic = $(this).find("td:first")[0].innerText;
        selectedCharacteristicRow = characteristicDataTable.row(this).data();
      }
    });
}

let onCharacteristicAddOrEditClick = (editMode) => {
  characteristicEditMode = editMode;

  if (editMode) {
    if ($('#characteristic-grid tbody').find('td.dataTables_empty').length > 0) {
      toastr.success($("#pleaseSelectACharacteristic").val());
      return;
    }
  }
  characteristicEditMode ? fillCharacteristicFields(selectedCharacteristicRow) : fillCharacteristicFields();

  $('.not-editable').prop('readOnly', characteristicEditMode);
  $('#validation-for-characteristic-id').prop('hidden', true);
  $('#validation-for-characteristic-description').prop('hidden', true);
  $("#characteristicFormModal").modal('show');
}

let onCharacteristicDeleteClick = async () => {
  let title = $("#deleteCharacteristicTitle").val();
  let message = `${$("#deleteMessage").val()} ${selectedCharacteristic}?`;

  if ($('#characteristic-grid tbody').find('td.dataTables_empty').length > 0) {
    toastr.success($("#pleaseSelectACharacteristic").val());
    return;
  }
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteCharacteristic?characteristicID=${selectedCharacteristic}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#characteristicDeletedSuccessfully").val());
            await loadCharacteristicData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onCharacteristicCompleteClick = async () => {
  let characteristicFormData = prepareCharacteristicFormData();

  if (!isCharacteristicFormValid(characteristicFormData)) return;

  if (!characteristicEditMode) {
    let newCharacteristicId = $("#characteristic-id").val();
    let isExist = characteristicData.find(obj => obj.characteristicID.toLowerCase() == newCharacteristicId.toLowerCase());

    if (isExist) {
      toastr.warning($("#idAlreadyExists").val().replace("{0}", newCharacteristicId));
      return;
    }
  }

  let url = `StockDescriptionElements/SaveCharacteristic?isUpdate=${characteristicEditMode}`;
  await ajaxPostAsync(url, characteristicFormData)
    .then(async (response) => {
      if (response) {
        toastr.success(characteristicEditMode ? $("#characteristicUpdatedSuccessfully").val() : $("#characteristicAddedSuccessfully").val());
        await loadCharacteristicData();
        $("#characteristicFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareCharacteristicFormData = () => {
  const formData = {
    characteristicID: $("#characteristic-id").val(),
    description: $("#characteristic-description").val()
  }
  return formData;
}

let isCharacteristicFormValid = (characteristicFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(characteristicFormData.characteristicID)) {
    $('#validation-for-characteristic-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(characteristicFormData.description)) {
    $('#validation-for-characteristic-description').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillCharacteristicFields = (formData = null) => {
  $("#characteristic-id").val(formData?.characteristicID);
  $("#characteristic-description").val(formData?.description);
}
