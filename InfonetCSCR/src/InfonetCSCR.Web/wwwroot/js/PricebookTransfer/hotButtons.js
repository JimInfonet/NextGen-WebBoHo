/*:::::::::::::::: Global ::::::::::::::::*/

let hotButtons = [];
let selectedHotButtons = [];
let hotButtonsTransfers = [];
let hotButtonsTransTypes = {};
let dataTableHotButtonsTransfer = null;
let dataTableHotButton = null;

/*:::::::::::::::: General Section Start ::::::::::::::::*/

async function loadHotbuttons() {
  const url = STRING_TABLE.URL.GET.HOT_BUTTONS;
  await ajaxGetAsync(url)
    .then((_hotButtons) => hotButtons = _hotButtons)
    .catch(toastrError);
}

async function loadHotButtonsTransfer(storeId = null) {
  let url = STRING_TABLE.URL.GET.HOT_BUTTONS_TRANSFER;
  if (storeId !== null) url += `?storeId=${storeId}`;

  await ajaxGetAsync(url)
    .then((_hotButtonsTransfers) => hotButtonsTransfers = _hotButtonsTransfers)
    .catch(toastrError);
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: Selection Tab Start ::::::::::::::::*/

function renderHotButtons() {
  $("#select-or-unselect-all-hot-buttons").prop("checked", false);

  let props = createDataTableProp();
  props.columns = [
    { data: 'buttonId' },
    { data: 'product' },
    { data: 'productName' },
    { data: 'caption1' },
    { data: 'caption2' },
    {
      data: null,
      sortable: false,
      render: function (hotButton) {
        return `<select class="form-select form-select-sm trans-type" id="${hotButton.buttonId}-hot-button-trans-type" onchange="handleHotButtonTransferTypeChange(${hotButton.buttonId})">
          <option value="A" selected>${$('#add').val()}</option>
          <option value="U">${$('#update').val()}</option>
          <option value="R">${$('#remove').val()}</option>
        </select>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (hotButton) {
        return `<input class="form-check-input mt-2 select-by-option-hot-buttons" type="checkbox" value="${hotButton.buttonId}" 
                       onChange="handleHotButtonSelectionChange(${hotButton.buttonId})" />`;
      }
    }
  ];
  props.data = hotButtons;

  if (dataTableHotButton != null) {
    dataTableHotButton.state.clear();
    dataTableHotButton.clear();
    dataTableHotButton.destroy();
  }

  dataTableHotButton = createDatatable("table-hot-buttons", props);
}

function handleHotButtonSelectionChange(buttonId) {
  const alreadyExist = selectedHotButtons.find(hotButton => hotButton.buttonId === buttonId);
  if (alreadyExist) {
    selectedHotButtons = selectedHotButtons.filter(hotButton => hotButton.buttonId !== buttonId);
  }
  else {
    selectedHotButtons.push(hotButtons.find(hotButton => hotButton.buttonId === buttonId));
  }

  const allSelected = (selectedHotButtons.length == hotButtons.length);
  $("#select-or-unselect-all-hot-buttons").prop("checked", allSelected);
}

function handleSelectOrUnselectAllHotButtons() {
  let isSelected = $("#select-or-unselect-all-hot-buttons").is(":checked");
  $("#table-hot-buttons").DataTable().rows().every(function () {
    $(this.node()).find(".select-by-option-hot-buttons").prop("checked", isSelected);
  });
  selectedHotButtons = [];
  if (isSelected) {
    hotButtons.forEach(hotButton => selectedHotButtons.push(hotButton));
  }
}

function handleHotButtonTransferTypeChange(buttonId) {
  let selectedTransType = $(`#${buttonId}-hot-button-trans-type`).val();
  hotButtonsTransTypes[buttonId] = selectedTransType;
}

async function handleHotButtonSaveAsPendingTransfer() {
  if (selectedStoresOnSelectionTab.size == 0) {
    const message = $("#selectAtLeastOneStore").val();
    toastr.warning(message);
    return;
  }

  if (selectedHotButtons.length == 0) {
    const message = $("#selectAtLeastOneItem").val();
    toastr.warning(message);
    return;
  }

  const savingText = $("#saving").val();
  $("#hot-buttons-save-transfer")
    .html(`<span class="spinner-border spinner-border-sm" role="status"></span> ${savingText}...`);

  let storeIds = [];
  selectedStoresOnSelectionTab.forEach(store => storeIds.push(store.storeID));

  const pendingTransfers = {
    storeIds: storeIds,
    hotButtons: selectedHotButtons,
    transTypes: hotButtonsTransTypes
  };

  let error = null;
  await ajaxPostAsync(STRING_TABLE.URL.INSERT.PENDING_HOT_BUTTONS_TRANSFERS, pendingTransfers).catch(err => error = err);

  const saveAsPendingTransferText = $("#saveAsPendingTransfer").val();
  $("#hot-buttons-save-transfer").html(saveAsPendingTransferText);
  const message = $("#hotButtonTransferSuccessful").val();
  error ? toastr.error(error) : toastr.success(message);

  resetDataAndTables();
  initializeHotButtonsTransType();
}

/*:::::::::::::::: Selection Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

async function renderHotButtonsListTab(storeId = null) {
  await loadHotButtonsTransfer(storeId).then(() => {
    renderHotButtonsTransfersTable();
    resizeTransferStoreContainer();
  });
}

function renderHotButtonsTransfersTable() {
  if (dataTableHotButtonsTransfer) {
    dataTableHotButtonsTransfer.state.clear();
    dataTableHotButtonsTransfer.clear();
    dataTableHotButtonsTransfer.destroy();
  }

  let props = createDataTableProp();
  let columns = [
    { data: 'batchId' },
    { data: 'button' },
    { data: 'productId' },
    { data: 'description' },
    { data: 'caption1' },
    { data: 'caption2' },
    { data: 'transType' },
    {
      data: null,
      sortable: false,
      render: function (item) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                    onClick="handleDeleteFromTransferList('${item.batchId} ', '${item.button}',null, null, 'ITEM')">
                      <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    }
  ];

  props.columns = columns;
  props.data = hotButtonsTransfers;

  dataTableHotButtonsTransfer = createDatatable("table-hot-buttons-transfer", props);
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/


/*:::::::::::::::: Misc. Functions Start ::::::::::::::::*/

function initializeHotButtonsTransType() {
  hotButtons.forEach(hotButton => {
    hotButtonsTransTypes[hotButton.buttonId] = "A";
  });
}

/*:::::::::::::::: Misc. Functions End ::::::::::::::::*/
