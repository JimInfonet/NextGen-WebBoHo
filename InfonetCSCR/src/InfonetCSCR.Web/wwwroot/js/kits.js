let kits;
let kitItemsData;
let kitsDatatable;
let kitItemsDatatable;
let selectedStockCode;
let selectedKitCode;
let selectedKitItem;

$(async function () {
  await loadKits();
});

let loadKits = async () => {
  let url = "Kits/GetKits";

  await ajaxGetAsync(url)
    .then(_kits => {
      kits = _kits;
      $("#kit-item-btn-add").prop("disabled", (kits.length === 0));
      if (kits.length > 0 && !selectedKitCode) {
        selectedStockCode = kits[0].stockCode;
      }
      createKitsDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingKits").val()));

  await loadKitItems();
}

let loadKitItems = async () => {
  let url = `Kits/GetKitItemsByKitCodeWithDesc?stockCode=${selectedStockCode}`;

  await ajaxGetAsync(url)
    .then(_kitItems => {
      kitItemsData = _kitItems;
      $("#kit-item-btn-delete").prop("disabled", (kitItemsData.length === 0));
      if (kitItemsData.length > 0 && !selectedKitItem) {
        selectedKitCode = kitItemsData[0].stockCode;
      }
      createKitItemsDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingKitItems").val()));
}

let createKitsDatatable = () => {
  destroyDataTableIfExists(kitsDatatable)

  let props = createDataTableProp();
  props.data = kits;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];

  props.columns = [
    { "data": "stockCode", },
    { "data": "description", },
    { "data": "dept", },
    { "data": "priceType", },
  ];

  kitsDatatable = createDataTableGeneric('kits-grid', props);

  if (kits.length > 0) {
    $(`#kits-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#kits-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (kits.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        kitsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedStockCode = $(this).find("td:first")[0].innerText;
        await loadKitItems();
      }
    })
}

let createKitItemsDatatable = () => {
  destroyDataTableIfExists(kitItemsDatatable)

  let props = createDataTableProp();
  props.data = kitItemsData;
  props.scrollY = '440px';
  props.pageLength = 10;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  props.columns = [
    { "data": "stockCode", },
    { "data": "description", },
    {
      "data": "quantity",
      render: function (quantity) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${quantity}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleQuantityEdit(td, cellData, rowData);
      }
    },
  ];

  kitItemsDatatable = createDataTableGeneric('kit-items-grid', props);

  if (kitItemsData.length > 0) {
    $(`#kit-items-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#kit-items-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (kitItemsData.length === 0) {
        return;
      }

      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
      }

      if (!$(this).hasClass('selected-grid-row')) {
        kitItemsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');

        selectedKitCode = $(this).find("td:first")[0].innerText;
      }
    })
}

let addKitItem = async () => {
  await loadStockCode();
  $("#stockCodeDescriptionDataTableModal").modal('show');
}

let deleteKitItem = async () => {
  let kitItem = {
    StockCode: selectedKitCode,
    KitCode: selectedStockCode
  }

  let title = $("#deleteKitItemTitle").val();
  let message = $("#deleteKitItemMessage").val();

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Kits/DeleteKitItem`;
      await ajaxPostAsync(url, kitItem)
        .then(async _ => {
          toastr.success($("#kitItemDeletedSuccessfully").val());
          await loadKitItems();
        })
        .catch(toastrError);
    }
  });
}

async function handleQuantityEdit(td, cellData, rowData, cellType) {
  const cellContainer = $(td).find('.cell-container');
  const textSpan = $(td).find('.text-span');

  // Event listener to switch to edit mode when the span is clicked
  textSpan.on('click', function () {
    textSpan.hide();
    let editInput = prepareInputField(cellType, cellData, cellContainer);
    cellContainer.append(editInput);
    editInput.focus().select();
    let oldValue = editInput.val();

    editInput.on('keypress', function (e) {
      return onlyDigitAndDecimalKeyPress(e);
    })

    editInput.on('focusout keypress', async function (e) {
      if (e.type === "keypress" && e.key !== "Enter") return;

      let newValue = isNullOrWhiteSpace(editInput.val()) ? "0" : editInput.val();
      newValue = +parseFloat(newValue).toFixed(6);

      if (newValue < 0) {
        newValue *= -1;
      }

      if (newValue === 0 || isNaN(newValue)) {
        toastr.warning($("#quantityHasToBePositiveNumericValue").val())
        cellData = oldValue;
        editInput.remove();
        textSpan.html(oldValue).show();
        return;
      }

      if (newValue > 99999) {
        toastr.warning($("#quantityHasToBeLowerThanOrEqualTo").val())
        cellData = oldValue;
        editInput.remove();
        textSpan.html(oldValue).show();
        return;
      }

      cellData = newValue;
      editInput.remove();
      textSpan.html(newValue).show();

      if (newValue === parseFloat(oldValue)) {
        return;
      }

      let kitItem = {
        StockCode: selectedKitCode,
        KitCode: selectedStockCode,
        Quantity: newValue
      };

      let url = `Kits/UpdateKitItem`;
      await ajaxPostAsync(url, kitItem)
        .then(toastr.success($("#kitItemUpdatedSuccessfully").val()))
        .catch(toastrError);

      await loadKitItems();
    });
  })
}

function prepareInputField(cellType, cellData, cellContainer) {
  let editInput;
  let elementType = "text";
  editInput = $('<input>', { type: elementType, value: cellData });
  const fullSizeOfParentConatiner = cellContainer.width();
  editInput.width(fullSizeOfParentConatiner);

  return editInput;
}

async function loadStockCode() {
  let url = "kits/GetKitItems";
  await ajaxGetAsync(url)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);

  renderStockCodeDataTable(onStockCodeRowClick);
}


let onStockCodeRowClick = async (rowData) => {
  isAuthenticated().then(response => {
    if (!response) {
      return;
    }
  }).catch(toastrError);

  let url = `Kits/IsKitItem?stockCode=${rowData.stockCode}`;
  let isKitItem;

  await ajaxGetAsync(url)
    .then(_isKitItem => isKitItem = _isKitItem)
    .catch(toastrError);

  if (isKitItem) {
    toastr.warning($("#kitCanNotContainOtherKits").val().replace("{0}", rowData.stockCode).replace("{1}", rowData.description));
    return false;
  }

  let kitItem = {
    StockCode: rowData.stockCode,
    KitCode: selectedStockCode
  }

  url = `Kits/IsItemAlreadyInKit`;
  let isItemAlreadyInKit;
  await ajaxPostAsync(url, kitItem)
    .then(_isItemAlreadyInKit => isItemAlreadyInKit = _isItemAlreadyInKit)
    .catch(toastrError);

  if (isItemAlreadyInKit) {
    toastr.warning($("#kitItemAlreadyAdded").val().replace("{0}", rowData.stockCode));
    return false;
  }

  url = `Kits/AddKitItem`;
  await ajaxPostAsync(url, kitItem)
    .then(async _ => {
      toastr.success($("#kitItemAddedSuccessfully").val());
      $("#stockCodeDescriptionDataTableModal").modal('hide');
      await loadKitItems();
    })
    .catch(toastrError);
}
