let brandData = [];
let brandDataTable;
let selectedBrand;
let selectedBrandRow;
let brandEditMode;
let brand = STOCK_DESCRIPTION_ELEMENTS.DATA_TABLE.BRAND;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadBrandData();
});

let loadBrandData = async () => {
  let url = "StockDescriptionElements/GetAllBrands";
  await ajaxGetAsync(url)
    .then(_brandData => {
      if (_brandData) {
        brandData = _brandData;
        let isDisableEditDeleteButtons = true;

        if (brandData.length > 0) {
          selectedBrand = brandData[0].brandID;
          selectedBrandRow = brandData[0];
          isDisableEditDeleteButtons = false;
        }
        enableOrDisableGridEditDeleteButtons(brand, isDisableEditDeleteButtons);
        createBrandDataTable();
        adjustSDEPanelHeight([brand, generic]);
      }
    })
    .catch(toastrError);
}

let createBrandDataTable = () => {
  destroyDataTableIfExists(brandDataTable);

  let props = createDataTableProp();
  props.data = brandData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "brandID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  brandDataTable = createDatatableWithSearching('brand-grid', props);

  if (brandData.length > 0) {
    $(`#brand-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#brand-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (brandData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        brandDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedBrand = $(this).find("td:first")[0].innerText;
        selectedBrandRow = brandDataTable.row(this).data();
      }
    });
}

let onBrandAddOrEditClick = (editMode) => {
  brandEditMode = editMode;

  if (editMode) {
    if ($('#brand-grid tbody').find('td.dataTables_empty').length > 0) {
      toastr.success($("#pleaseSelectABrand").val());
      return;
    }
  }
  brandEditMode ? fillBrandFields(selectedBrandRow) : fillBrandFields();

  $('.not-editable').prop('readOnly', brandEditMode);
  $('#validation-for-brand-id').prop('hidden', true);
  $('#validation-for-brand-description').prop('hidden', true);
  $("#brandFormModal").modal('show');
}

let onBrandDeleteClick = async () => {
  let title = $("#deleteBrandTitle").val();
  let message = `${$("#deleteMessage").val()} ${selectedBrand}?`;

  if ($('#brand-grid tbody').find('td.dataTables_empty').length > 0) {
    toastr.success($("#pleaseSelectABrand").val());
    return;
  }

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteBrand?brandID=${selectedBrand}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#brandDeletedSuccessfully").val());
            await loadBrandData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onBrandCompleteClick = async () => {
  let brandFormData = prepareBrandFormData();

  if (!isBrandFormValid(brandFormData)) return;

  if (!brandEditMode) {
    let newBrandId = $("#brand-id").val();
    let isExist = brandData.find(obj => obj.brandID.toLowerCase() == newBrandId.toLowerCase());

    if (isExist) {
      toastr.warning($("#idAlreadyExists").val().replace("{0}", newBrandId));
      return;
    }
  }

  let url = `StockDescriptionElements/SaveBrand?isUpdate=${brandEditMode}`;
  await ajaxPostAsync(url, brandFormData)
    .then(async (response) => {
      if (response) {
        toastr.success(brandEditMode ? $("#brandUpdatedSuccessfully").val() : $("#brandAddedSuccessfully").val());
        await loadBrandData();
        $("#brandFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareBrandFormData = () => {
  const formData = {
    brandID: $("#brand-id").val(),
    description: $("#brand-description").val()
  }
  return formData;
}

let isBrandFormValid = (brandFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(brandFormData.brandID)) {
    $('#validation-for-brand-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(brandFormData.description)) {
    $('#validation-for-brand-description').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillBrandFields = (formData = null) => {
  $("#brand-id").val(formData?.brandID);
  $("#brand-description").val(formData?.description);
}
