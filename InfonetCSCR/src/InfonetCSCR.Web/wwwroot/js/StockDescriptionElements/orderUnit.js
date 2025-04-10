let orderUnitData = [];
let orderUnitDataTable;
let selectedOrderUnit;
let selectedOrderUnitRow;
let orderUnitEditMode;
let orderUnit = STOCK_DESCRIPTION_ELEMENTS.DATA_TABLE.ORDER_UNIT;

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadOrderUnitData();
});

let loadOrderUnitData = async () => {
  let url = "StockDescriptionElements/GetAllOrderUnits";
  await ajaxGetAsync(url)
    .then(_orderUnitData => {
      if (_orderUnitData) {
        orderUnitData = _orderUnitData;
        let isDisableEditDeleteButtons = true;

        if (orderUnitData.length > 0) {
          selectedOrderUnit = orderUnitData[0].orderID;
          selectedOrderUnitRow = orderUnitData[0];
          isDisableEditDeleteButtons = false;
        }
        enableOrDisableGridEditDeleteButtons(orderUnit, isDisableEditDeleteButtons);
        createOrderUnitDataTable();
        adjustSDEPanelHeight([unitOfMeasure, orderUnit]);
      }
    })
    .catch(toastrError);
}

let createOrderUnitDataTable = () => {
  destroyDataTableIfExists(orderUnitDataTable);

  let props = createDataTableProp();
  props.data = orderUnitData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "orderID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  orderUnitDataTable = createDatatableWithSearching('orderUnit-grid', props);

  if (orderUnitData.length > 0) {
    $(`#orderUnit-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#orderUnit-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (orderUnitData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        orderUnitDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedOrderUnit = $(this).find("td:first")[0].innerText;
        selectedOrderUnitRow = orderUnitDataTable.row(this).data();
      }
    });
}

let onOrderUnitAddOrEditClick = (editMode) => {
  orderUnitEditMode = editMode;

  if (editMode) {
    if ($('#orderUnit-grid tbody').find('td.dataTables_empty').length > 0) {
      toastr.success($("#pleaseSelectAOrderUnit").val());
      return;
    }
  }
  orderUnitEditMode ? fillOrderUnitFields(selectedOrderUnitRow) : fillOrderUnitFields();

  $('.not-editable').prop('readOnly', orderUnitEditMode);
  $('#validation-for-orderUnit-id').prop('hidden', true);
  $('#validation-for-orderUnit-description').prop('hidden', true);
  $("#orderUnitFormModal").modal('show');
}

let onOrderUnitDeleteClick = async () => {
  let title = $("#deleteOrderUnitTitle").val();
  let message = `${$("#deleteMessage").val()} ${selectedOrderUnit}?`;

  if ($('#orderUnit-grid tbody').find('td.dataTables_empty').length > 0) {
    toastr.success($("#pleaseSelectAOrderUnit").val());
    return;
  }
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteOrderUnit?orderUnitID=${selectedOrderUnit}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#orderUnitDeletedSuccessfully").val());
            await loadOrderUnitData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onOrderUnitCompleteClick = async () => {
  let orderUnitFormData = prepareOrderUnitFormData();

  if (!isOrderUnitFormValid(orderUnitFormData)) return;

  if (!orderUnitEditMode) {
    let newOrderUnitId = $("#orderUnit-id").val();
    let isExist = orderUnitData.find(obj => obj.orderID.toLowerCase() == newOrderUnitId.toLowerCase());

    if (isExist) {
      toastr.warning($("#idAlreadyExists").val().replace("{0}", newOrderUnitId));
      return;
    }
  }

  let url = `StockDescriptionElements/SaveOrderUnit?isUpdate=${orderUnitEditMode}`;
  await ajaxPostAsync(url, orderUnitFormData)
    .then(async (response) => {
      if (response) {
        toastr.success(orderUnitEditMode ? $("#orderUnitUpdatedSuccessfully").val() : $("#orderUnitAddedSuccessfully").val());
        await loadOrderUnitData();
        $("#orderUnitFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareOrderUnitFormData = () => {
  const formData = {
    orderID: $("#orderUnit-id").val(),
    description: $("#orderUnit-description").val()
  }
  return formData;
}

let isOrderUnitFormValid = (orderUnitFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(orderUnitFormData.orderID)) {
    $('#validation-for-orderUnit-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(orderUnitFormData.description)) {
    $('#validation-for-orderUnit-description').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillOrderUnitFields = (formData = null) => {
  $("#orderUnit-id").val(formData?.orderID);
  $("#orderUnit-description").val(formData?.description);
}
