const STRING_TABLE = STOCK_TAKES_AND_RECEIVING_STRING_TABLE;
let stocksDatatable;
let receiveDatatable;
let stocks;
let receiveData = [];
let selectedStock;
let selectedStockInReceiveGrid;
let vendors;
let errorMessage;
let policies;
let selectedVendor;
let selectedDept;
let selectedSubDept;
let selectedSubDetail;
let selectedMonth;
let invoice;
let cellIndex;
let cellData;
let rowIndex;
let rowData;
let selectedPageLength;
let isAllSelected = false;
let isDeleteButtonClicked = false;
let isQuantityUpdate = false;
let isServerSidePagination = true;
const stockCountType = document.getElementById("stockCountType").value;
let isStockTakes = stockCountType === "StockTakes";
let purpose = stockCountType === "StockTakes" ? 2 : 1;
let isInputModalOkButtonClicked;
let modalSource = "";
let selectedTableCellId = "";
let invoiceValue = "";
let companyName;
let deleteImportFile = false;
let isModalSourceInvoice = false;
let selectedReceiveTableRowData;
let searchColumnName = STRING_TABLE.TABLE.SEARCH_COLUMN_NAME.CODE;
let receiveSearchValue = "";
let isStockGridSearch = false;

$(async function () {
  selectedPageLength = 10;
  errorMessage = $("#errorMessage").val();

  await loadPolicies();
  await loadStocks();
  await loadReceiveData();
  await getRecLineSusCount();
  await loadVendor();
  await loadDepartmentDropdown();
  companyName = await getCompanyName();

  const currentMonth = new Date().getMonth();
  $("#select-month").val(currentMonth + 1);
});

let createStocksDatatable = async () => {
  destroyDataTableIfExists(stocksDatatable);

  let props = createDataTableProp();
  props.data = stocks;
  props.scrollY = '440px';

  if (!isStockTakes) {
    props.columnDefs = [
      { className: 'text-center', targets: [0, 1, 2, 3] }
    ];
    props.columns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "stockCode", },
      { "data": "vendorPart", },
      { "data": "description", },
      {
        data: null,
        sortable: false,
        render: function (stock, _, _, meta) {
          return `<input class="form-check-input mt-2 select-by-option-dept" type="checkbox" 
                id="checkbox-${encodeURIComponent(stock.stockCode.replace(/\s/g, ''))}-${encodeURIComponent(meta.row)}" 
                onclick="checkOrUncheckStock('${stock.stockCode}', '${stock.description.replace(/'/g, "\\'")}', '${encodeURIComponent(meta.row)}')"/>`;
        }
      },
    ];
  }
  else {
    props.columnDefs = [
      { className: 'text-center', targets: [0, 1, 2] }
    ];
    props.columns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "stockCode", },
      { "data": "description", },
      {
        data: null,
        sortable: false,
        render: function (stock, _, _, meta) {
          return `<input class="form-check-input mt-2 select-by-option-dept" type="checkbox" 
                id="checkbox-${encodeURIComponent(stock.stockCode.replace(/\s/g, ''))}-${encodeURIComponent(meta.row)}" 
                onclick="checkOrUncheckStock('${stock.stockCode}', '${stock.description.replace(/'/g, "\\'")}', '${encodeURIComponent(meta.row)}')"/>`;
        }
      },
    ];
  }

  props.callOnDrawCallback = function () {
    if (isAllSelected) {
      renderSelectedCheckBoxes();
    }
  };

  stocksDatatable = createDatatableWithSearching('stock-grid', props);

  $('#stock-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'td', async function () {
      let cellIndex = $(this).index();

      if ((isStockTakes && cellIndex === 2) || (!isStockTakes && cellIndex === 3)) {
        disableStockRecievedFields();
      }

      rowData = stocksDatatable.row($(this).closest('tr')).data();
      selectedStock = rowData.stockCode;
      stockDescription = rowData.description;
      const isClickedRowSelected = $(this).closest('tr').hasClass('selected-grid-row');

      if (isClickedRowSelected) {
        $(this).closest('tr').removeClass('selected-grid-row');

        if ((isStockTakes && cellIndex !== 2) || (!isStockTakes && cellIndex !== 3)) {
          stockReceivedText = $("#stockReceived").val();
          $("#stock-received-quantity-update-title").text(`${stockReceivedText}`);
          selectedStock = null;

          disableStockRecievedFields();
        }
      }
      else {
        const anyRowHasClass = $('#stock-grid tbody tr.selected-grid-row').length > 0;

        if (anyRowHasClass) {
          $('#stock-grid tbody tr.selected-grid-row').removeClass('selected-grid-row');
        }

        if ((isStockTakes && cellIndex !== 2) || (!isStockTakes && cellIndex !== 3)) {
          $(this).closest('tr').addClass('selected-grid-row');

          $('#btn-accept').prop('disabled', true);
          $('#stock-received-invoice').val(invoiceValue);

          await populateStockReceivedFields();
        }
      }
    });
}

function disableStockRecievedFields() {
  $('#stock-received-quantity').prop('disabled', true);

  if (!isStockTakes) {
    $('#stock-received-cost').prop('disabled', true);
    $('#stock-received-invoice').prop('disabled', true);
  }
  else {
    $('#stock-takes-extra-count').prop('disabled', true);
  }

  $('#btn-clear').prop('disabled', true);
  $('#btn-accept').prop('disabled', true);
}

async function populateStockReceivedFields() {
  stockReceivedText = $("#stockReceived").val();
  $("#stock-received-quantity-update-title").text(`${stockReceivedText} ${selectedStock} ${stockDescription}`);

  $('#stock-received-quantity').prop('disabled', false);
  $('#stock-received-quantity').val('');

  if (!isStockTakes) {
    $('#stock-received-cost').prop('disabled', false);
    $('#stock-received-invoice').prop('disabled', false);

    cost = await getCost(selectedStock);
    $('#stock-received-cost').val(parseFloat(cost)?.toFixed(policies.costDec));
  }
  else {
    $('#stock-takes-extra-count').prop('disabled', false);
    $('#stock-takes-extra-count').val('');
  }

  $('#btn-clear').prop('disabled', false);
}

function createReceiveDataTable(parameters) {
  destroyDataTableIfExists(receiveDatatable);

  let url = "StockTakesAndReceiving/GetStockTakesAndReceiveData" + parameters;
  let props = createDataTableProp();
  props.stateSave = true;
  props.serverSide = true;
  props.pageLength = parseInt(selectedPageLength);

  props.ajax = {
    url: url,
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    data: function (d) {
      d.searchValue = _searchValue === '' || !isStockTakes || isStockGridSearch ? null : _searchValue;
      d.searchColumnName = searchColumnName;
      return JSON.stringify(d);
    },
    dataSrc: function (d) {
      receiveData = d.data;
      return d.data;
    },
  };

  if (!isStockTakes) {
    props.columnDefs = [
      {
        className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6],
        'createdCell': function (td, _, _, row, col) {
          if (col === 4) {
            $(td).attr("onclick", `handleTableCellEdit('${row}','quantity')`);
          }
          else if (col === 5) {
            $(td).attr("onclick", `handleTableCellEdit('${row}','cost')`);
          }
          else if (col === 1) {
            $(td).attr("onclick", `handleTableCellEdit('${row}','invoice')`);
          }
        }
      }
    ];
    props.columns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "recNum", },
      { "data": "invoice", },
      { "data": "stockCode", },
      { "data": "description", },
      {
        "data": "quantity",
        "render": function (quantity, _, _, meta) {
          return `
                    <span class="regular-price__text" id="quantity-${meta.row}-text">${quantity}</span>
                `;
        }
      },
      {
        "data": "cost",
        "render": function (cost, _, _, meta) {
          return `
                    <span class="regular-price__text" id="cost-${meta.row}-text">${cost?.toFixed(4) ?? "0"}</span>
                `;
        }
      },
      {
        "data": "amount",
        "render": function (amount, _, _, meta) {
          return `
                    <span class="regular-price__text" id="amount-${meta.row}-text">${amount?.toFixed(2) ?? ""}</span>
                `;
        }
      },
    ];
  }
  else {
    props.columnDefs = [
      { className: 'text-center', targets: [0, 1, 2, 3, 4, 5] }
    ];
    props.columns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "recNum", },
      { "data": "stockCode", },
      { "data": "description", },
      { "data": "quantity", },
      { "data": "systemQuantity", },
      { "data": "initialCount", },
    ];
  }

  props.callOnDrawCallback = function () {
    enableOrDisableButtonsForReceiveGrid(true);
  };

  receiveDatatable = _createDatatableWithSearching('receive-grid', props);

  $('#receive-grid')
    .off()
    .on('click', 'td', async function () {

      cellIndex = $(this).index();
      cellData = $(this).text();
      rowIndex = receiveDatatable.row($(this).closest('tr')).index();
      rowData = receiveDatatable.row($(this).closest('tr')).data();
      selectedReceiveTableRowData = rowData;

      const isClickedRowSelected = $(this).closest('tr').hasClass('selected-grid-row');

      if (isClickedRowSelected) {
        $(this).closest('tr').removeClass('selected-grid-row');
        selectedStockInReceiveGrid = null;
        enableOrDisableButtonsForReceiveGrid(true);
      }
      else {
        const anyRowHasClass = $('#receive-grid tbody tr.selected-grid-row').length > 0;

        if (anyRowHasClass) {
          $('#receive-grid tbody tr.selected-grid-row').removeClass('selected-grid-row');
        }

        if ((isStockTakes && cellIndex !== 3) || !isStockTakes) {
          $(this).closest('tr').addClass('selected-grid-row');
        }
        selectedStockInReceiveGrid = isStockTakes ? $(this).siblings().eq(1).text() : $(this).siblings().eq(2).text();
        enableOrDisableButtonsForReceiveGrid(false);
      }

      if (cellIndex === 3 && isStockTakes) {
        $("#editCount").modal('show');
        $("#editCountinput").val(cellData);
        selectedStockInReceiveGrid = $(this).siblings().eq(1).text();
        enableOrDisableButtonsForReceiveGrid(true);
      }
    });
}

$("#editCountinput").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 4) return false;
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

async function editQty() {
  const table = $('#receive-grid').DataTable();
  let count = $('#editCountinput').val();
  let quantityDataEntryError = $('#quantityDataEntryError').val();
  let quantityOutOfRangeError = $('#quantityOutOfRangeError').val();
  let countValue = parseFloat(count);
  const currentPage = table.page();

  if (isNaN(countValue)) {
    toastr.warning(quantityDataEntryError);
    return;
  }
  else if (countValue > 9999 || countValue < -9999) {
    toastr.warning(quantityOutOfRangeError);
    return;
  }
  else if (count.length > 5) {
    toastr.warning($('#countCantBeMoreThan').val());
    return;
  }

  let url = `StockTakesAndReceiving/UpdateQuantityForStockTakes?stockCode=${selectedStockInReceiveGrid}&quantity=${countValue}`

  await ajaxGetAsync(url)
    .then(() => {
      $("#editCount").modal('hide');
      table.page(currentPage).draw(false);
    })
    .catch(toastrError);
}

let loadStocks = async () => {
  let stockTypeText = document.getElementById("select-stock-type").value;
  selectedVendor = stockTypeText === "Vendor" ? $("#select-vendor").val() : null;
  selectedDept = stockTypeText === "Category" ? $("#select-department").val() : null;
  selectedSubDept = stockTypeText === "Category" ? $("#select-sub-department").val() : null;
  selectedSubDetail = stockTypeText === "Category" ? $("#select-sub-detail").val() : null;
  selectedMonth = isStockTakes && stockTypeText === "Month" ?
    parseInt($('#select-month').val()) : 0;

  let data = {
    Vendor: selectedVendor,
    Department: selectedDept,
    SubDepartment: selectedSubDept,
    SubDetail: selectedSubDetail,
    SelectedMonth: selectedMonth,
    Purpose: purpose
  };

  let url = "StockTakesAndReceiving/GetStocks";

  await ajaxPostAsync(url, data)
    .then(_stocks => {
      stocks = _stocks;
      createStocksDatatable();
      refreshCheckboxSelection();
    })
    .catch(toastrError);
}

let loadReceiveData = async () => {
  createReceiveDataTable(`?purpose=${purpose}`);
}

const _debouncedSearch = debounce(function (dataTableColumn) {
  // search prefix using regex 
  dataTableColumn.search('^' + receiveSearchValue, true).draw();
});

function _createDatatableWithSearching(selector, props = {}) {
  let dataTable = createDataTableGeneric(selector, props, true);
  // Add a input element for each footer column having the class "input" 
  let id = 0;
  $($(dataTable.table().footer()).html()).find("th").each(function () {
    let title = $(dataTable.column(id).header()).text();
    $(dataTable.column(id++).footer()).html(`<input class="form-control p-2" type="text" id="${title}" placeholder="${$("#search").val()} ${title}"/>`);
  })

  dataTable.columns().every(function () {
    const dataTableColumn = this;
    const searchInput = $(this.footer()).find('input');

    searchInput.on('keyup change', function () {
      // Clear the search input fields of other columns
      dataTable.columns().every(function () {
        if (this.index() !== dataTableColumn.index()) {
          let searchString = $(this.footer()).find('input').val();
          if (searchString?.length) {
            $(this.footer()).find('input').val('');
            dataTable.column(this.index()).search('').draw();
          }
        }
      });

      isStockGridSearch = selector === 'stock-grid';
      searchColumnName = searchInput.prop("id");
      receiveSearchValue = searchInput.val();
      // Perform the search on the current column with debouncing
      debouncedSearch(dataTableColumn);
    });
  })
  return dataTable;
}

async function loadVendor() {
  let url = "/stockitems/getvendors";

  return ajaxGetAsync(url)
    .then(_vendors => {
      vendors = _vendors;
      renderVendorsToDropdown();
    })
    .catch(toastrError);
}

async function loadPolicies() {
  let url = "/StockTakesAndReceiving/GetPolicies";

  return ajaxGetAsync(url)
    .then(_ => policies = _)
    .catch(toastrError);
}

async function getCost(stockCode) {
  let url = `/StockTakesAndReceiving/GetCost?stockCode=${stockCode}`;

  return ajaxGetAsync(url)
    .catch(toastrError);
}

let renderVendorsToDropdown = () => {
  let vendorSelection = $('#select-vendor');
  let optionTemplate = $('<option></option>');
  let optionsFragment = $(document.createDocumentFragment());

  vendorSelection.empty();

  let allOption = optionTemplate.clone().text($('#all').val()).val("All");
  optionsFragment.append(allOption);
  let undefinedOption = optionTemplate.clone().text($('#undefined').val()).val("Undefined");
  optionsFragment.append(undefinedOption);

  for (let i = 0; i < vendors?.length; i++) {
    let option = optionTemplate.clone().text(vendors[i].code + " - " + vendors[i].name).val(vendors[i].code.split(" ")[0]);
    optionsFragment.append(option);
  }
  vendorSelection.append(optionsFragment);
}

async function loadDepartmentDropdown() {
  let url = "/StockItems/GetDepartments";

  await ajaxGetAsync(url)
    .then(_depts => {
      let initialOptions = [
        { text: $('#all').val(), value: "All" },
        { text: $('#undefined').val(), value: "Undefined" }
      ];
      renderCategoryDropdown("select-department", _depts, initialOptions, "department");
    })
    .catch(toastrError);
}

let loadSubDepartmentDropdown = async () => {
  let department = $("#select-department").val();

  let url = `/StockItems/GetSubDepartments?department=${department}`
  await ajaxGetAsync(url)
    .then(_subDepts => {
      let initialOptions = getInitialOptions(department);

      renderCategoryDropdown("select-sub-department", _subDepts, initialOptions, "sub-department");

      if (department === 'Undefined') {
        $('#select-sub-department').val("Undefined");
      }
      else {
        $('#select-sub-department').val("All");
      }
    })
    .catch(toastrError);
}

let loadSubDetailDropdown = async () => {
  let department = $("#select-department").val();
  let subDepartment = $("#select-sub-department").val();
  let url = `/StockItems/GetSubDetails?department=${department}&subdepartment=${subDepartment}`;

  await ajaxGetAsync(url)
    .then(_subDetails => {
      let initialOptions = getInitialOptions(subDepartment);

      renderCategoryDropdown("select-sub-detail", _subDetails, initialOptions, "sub-detail");

      if (subDepartment === 'Undefined') {
        $('#select-sub-detail').val("Undefined");
      }
      else {
        $('#select-sub-detail').val("All");
      }
    })
    .catch(toastrError);
}

let getInitialOptions = (type) => {
  let initialOptions = [];
  if (type === 'All') {
    initialOptions = [
      { text: $('#all').val(), value: "All" }
    ];
  }
  else if (type === 'Undefined') {
    initialOptions = [
      { text: $('#undefined').val(), value: "Undefined" }
    ];
  }
  else {
    initialOptions = [
      { text: $('#all').val(), value: "All" },
      { text: $('#undefined').val(), value: "Undefined" }
    ];
  }
  return initialOptions;
}

async function handleCategoryChange(type) {
  if (type === 'department') {
    await loadSubDepartmentDropdown();
    await loadSubDetailDropdown();
  }
  else if (type === 'subDepartment') {
    await loadSubDetailDropdown();
  }
  await loadStocks();
}

let handleListTypeChange = async () => {
  let stockTypeText = document.getElementById("select-stock-type").value;
  $('#select-vendor').val("All");
  $('#select-department').val("All");

  if (stockTypeText === "All") {
    $("#select-vendor").parent().attr('hidden', true);
    $("#select-department").parent().attr('hidden', true);
    $("#select-sub-department").parent().attr('hidden', true);
    $("#select-sub-detail").parent().attr('hidden', true);

    if (isStockTakes) {
      $("#select-month").parent().attr('hidden', true);
      $("#btn-load").parent().attr('hidden', true);
    }

    await loadStocks();
  }
  else if (stockTypeText === "Vendor") {
    $("#select-vendor").parent().attr('hidden', false);
    $("#select-department").parent().attr('hidden', true);
    $("#select-sub-department").parent().attr('hidden', true);
    $("#select-sub-detail").parent().attr('hidden', true);

    if (isStockTakes) {
      $("#select-month").parent().attr('hidden', true);
      $("#btn-load").parent().attr('hidden', true);
    }

    await loadStocks();
  }
  else if (stockTypeText === "Category") {
    $("#select-vendor").parent().attr('hidden', true);
    $("#select-department").parent().attr('hidden', false);
    $("#select-sub-department").parent().attr('hidden', false);
    $("#select-sub-detail").parent().attr('hidden', false);

    if (isStockTakes) {
      $("#select-month").parent().attr('hidden', true);
      $("#btn-load").parent().attr('hidden', true);
    }

    await handleCategoryChange('department');
  }
  else {
    $("#select-vendor").parent().attr('hidden', true);
    $("#select-department").parent().attr('hidden', true);
    $("#select-sub-department").parent().attr('hidden', true);
    $("#select-sub-detail").parent().attr('hidden', true);
    $("#select-month").parent().attr('hidden', false);
    $("#btn-load").parent().attr('hidden', false);

    await handleMonthChange();
  }
}

let loadButtonClicked = async () => {
  await loadStocks();
}

let handleMonthChange = async () => {
  let selectedMonth = $('#select-month').val();
  let url = `StockTakesAndReceiving/GetCountOfInventoryDataForMonth?month=${selectedMonth}`;
  let data = await ajaxGetAsync(url);

  if (data) {
    $('#btn-load').prop('disabled', false);
  }
  else {
    $('#btn-load').prop('disabled', true);
    stocks = [];
    createStocksDatatable();
  }
}

let clearButtonClicked = () => {
  $('#stock-received-quantity').val('');
  $('#btn-accept').prop('disabled', true);

  if (!isStockTakes) {
    $('#stock-received-cost').val('');
    $('#stock-received-invoice').val('');
  }
  else {
    $('#stock-takes-extra-count').val('');
  }

}

async function loadAllStockTakesAndReceivingDataVendor() {
  let url = `/StockTakesAndReceiving/CheckStockCodeExistsInStockTakesAndReceiveData?purpose=${purpose}`;
  let params = {
    length: -1,
    purpose: 1,
    searchColumnName: "Stock Code",
    searchValue: null,
    start: 0
  }

  let allData;

  await ajaxPostAsync(url, params)
    .then(async (response) => {
      allData = response.data
    })
    .catch(toastrError);

  return allData;
}

let acceptButtonClicked = async () => {
  let quantity = $('#stock-received-quantity').val() === '' ? 0 : $('#stock-received-quantity').val();
  let description = stocks?.filter(x => x.stockCode == selectedStock)[0]?.description;
  isQuantityUpdate = true;

  let allData = await loadAllStockTakesAndReceivingDataVendor();
  let isStockCodePresent = allData.some(item => item.stockCode === selectedStock);

  if (isStockCodePresent) {
    toastr.warning($('#theSelectedStockCodeIsAlreadyInUse').val());
    return;
  }

  if (quantity.length > 5) {
    toastr.warning($('#maximumCharactersAllowedQuantity').val());
    return;
  }
  if (quantity > 99999 || quantity < -9999) {
    toastr.warning($('#stockTakesQuantityOutOfRangeError').val());
    $("#stock-received-quantity").focus();
    return;
  }

  if (!isStockTakes) {
    let cost = $('#stock-received-cost').val();
    invoiceValue = $('#stock-received-invoice').val();

    if (cost.length > 8) {
      toastr.warning($('#maximumCharactersAllowedCost').val());
      return;
    }
    if (cost > 9999.99) {
      toastr.warning($("#costCantBeGreaterThan").val());
      return;
    }

    if (invoiceValue.length > 10) {
      toastr.warning($('#maximumCharactersAllowedInvoice').val());
      return;
    }
    if (invoiceValue > 9999999999 || invoiceValue < 0) {
      toastr.warning($("#invoiceRangeFromZero").val());
      return;
    }

    await addRecLine(quantity, cost, invoiceValue, description);
  }
  else {
    let extraQuantity = $('#stock-takes-extra-count').val() === '' ? 0 : $('#stock-takes-extra-count').val();
    let totalQuantity = parseFloat(quantity) + parseFloat(extraQuantity);

    if (extraQuantity > 99999 || extraQuantity < -9999) {
      toastr.warning($('#stockTakesExtraCountOutOfRangeError').val());
      $("#stock-takes-extra-count").focus();
      return;
    }

    await addRecLine(totalQuantity, 0, '', description);
  }

  clearButtonClicked()
}

$("#stock-takes-extra-count").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length >= 5) return false;
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

let addRecLine = async (quantity, cost, invoice, description) => {
  const addReclineData = isStockTakes
    ? {
      StockCode: selectedStock,
      Description: description,
      CostType: policies.costType,
      Purpose: purpose,
      Quantity: parseFloat(quantity),
      IsQuantityUpdate: isQuantityUpdate
    }
    : {
      Quantity: parseFloat(quantity),
      Cost: parseFloat(cost),
      Invoice: parseFloat(invoice),
      StockCode: selectedStock,
      Description: description,
      ExceptedTax: policies.exceptedTax,
      CostDec: parseInt(policies.costDec),
      Purpose: parseInt(purpose),
      IsQuantityUpdate: isQuantityUpdate
    };

  let url = "StockTakesAndReceiving/AddRecLine";

  await ajaxPostAsync(url, addReclineData)
    .then(() => {
      toastr.success($("#successfullyAdded").val());
      loadReceiveData();
    })
    .catch(() => toastr.error($("#failedToAdd").val()));
}

let deleteRecLine = async (stockCode, recNum = null, sequence = null) => {
  let url = `StockTakesAndReceiving/DeleteRecLine?stockCode=${stockCode}&purpose=${purpose}`;
  if (recNum !== null) {
    url += `&recNum=${recNum}`;
  }
  if (sequence != null) {
    url += `&sequence=${sequence}`;
  }

  await ajaxGetAsync(url)
    .then(async () => {
      toastr.success($("#successfullyDeleted").val());
      await loadReceiveData();
    })
    .catch(() => toastr.error($("#failedToDelete").val()));
}

let deleteAllRecLine = async () => {
  let url = `StockTakesAndReceiving/DeleteAllRecLine?purpose=${purpose}`;
  let isDeleted = false;

  await ajaxGetAsync(url)
    .then(async () => {
      isDeleted = true;
      if (isDeleteButtonClicked) {
        toastr.success($("#successfullyDeleted").val());
      }
      await loadReceiveData();
    })
    .catch(() => toastr.error($("#failedToDelete").val()));

  return isDeleted;
}

let deleteAllRecLineAndRecHead = async () => {
  let url = `StockTakesAndReceiving/DeleteAllRecLineAndRecHead?purpose=${purpose}`;

  await ajaxGetAsync(url)
    .then(async () => {
      if (isDeleteButtonClicked) {
        toastr.success($("#successfullyDeleted").val());
      }
      refreshCheckboxSelection();
      await loadReceiveData();
    })
    .catch(() => toastr.error($("#failedToDelete").val()));
}

$('#stock-received-quantity').on('change', function () {
  let quantity = $(this).val();
  if (quantity.length > 0) {
    $('#btn-accept').prop('disabled', false);
  }
  else {
    $('#btn-accept').prop('disabled', true);
  }
});

if ($("#stock-received-quantity")) {
  $("#stock-received-quantity").on('keypress', function (event) {
    const value = $(this).val();
    if (value.length >= 5) return false;
    return onlyDigitDecimalPlusMinusKeyPress(event);
  });
}

$('#check-box-select-all').on('click', async function () {
  if ($('#check-box-select-all').prop('checked')) {
    if (!isStockTakes) {
      let title = $("#invoice").val();
      let message = $("#pleaseEnterTheInvoiceNumber").val();

      document.getElementById("inputModalTitleId").innerText = title;
      document.getElementById("inputModalMessageId").innerText = message;
      modalSource = "check-box-select-all";
      $("#inputModalId").modal("show");
    }

    if (isStockTakes) {
      await addAllStocksToRecline();
    }
  }
  else {
    refreshCheckboxSelection();
  }
});

$("#inputModalValueId").on('keypress', function (event) {
  if (!isStockTakes && (modalSource === "invoice" || modalSource === "cost")) {
    return onlyDigitAndDecimalKeyPress(event);
  }
  else if (!isStockTakes && (modalSource === "quantity" || modalSource === "check-box-select-all")) {
    return onlyDigitDecimalPlusMinusKeyPress(event);
  }
});

let handleInputModalOkButtonClick = async () => {
  isModalSourceInvoice = modalSource === "invoice";
  if (modalSource === "check-box-select-all" || isModalSourceInvoice) {
    isInputModalOkButtonClicked = true;
    invoice = parseFloat($("#inputModalValueId").val());

    if (isInvoiceValid(invoice)) {
      isAllSelected = !isModalSourceInvoice ? true : isAllSelected;
      $("#inputModalId").modal("hide");
    }
    else {
      return;
    }

    if (isModalSourceInvoice) {
      let url = `StockTakesAndReceiving/UpdateInvoiceNumber?invNum=${invoice}&stockCode=${receiveData[selectedTableCellId].stockCode}&batchId=${receiveData[selectedTableCellId].recNum}&purpose=1`;

      await ajaxGetAsync(url)
        .then(async () => loadReceiveData())
        .catch(toastrError);
    }
    else {
      await addAllStocksToRecline();
      $("#inputModalValueId").val("");
    }
  }
  else if (modalSource === "cost") {
    let cost = parseFloat($("#inputModalValueId").val());
    let quantity = receiveData[selectedTableCellId].quantity;

    let tempCost = cost || 1;
    let tempQuantity = quantity || 1;
    let maxCost = Math.min(Math.floor(2147483647 / Math.abs(tempQuantity)) / 10000, 9999.99);      // Floor to 4 decimal places

    let warningMessage = $("#costCantBeMoreThanXWhenQuantityIsY").val();
    warningMessage = warningMessage.replace("_COST_", maxCost);
    warningMessage = warningMessage.replace("_QUANTITY_", quantity);

    if (tempCost * tempQuantity > 214748.3647 || tempCost * tempQuantity < -214748.3648) {
      $("#inputModalValueId").val(maxCost);
      $("#inputModalValueId").focus();
      toastr.warning(warningMessage);
      return;
    }

    if (isNaN(cost)) {
      $("#inputModalValueId").focus();
      toastr.warning($("#fieldMustBeNumeric").val());
      return;
    }
    if (cost > 9999.99) {
      $("#inputModalValueId").val(Math.min(9999.99, maxCost));
      $("#inputModalValueId").focus();
      toastr.warning(warningMessage);
      return;
    }

    let url = `StockTakesAndReceiving/UpdateTotal?stockCode=${receiveData[selectedTableCellId].stockCode}&cost=${cost}&quantity=${receiveData[selectedTableCellId].quantity}&sequence=${receiveData[selectedTableCellId].sequence}&fromText=false&batchId=${receiveData[selectedTableCellId].recNum}&purpose=1`;

    await ajaxGetAsync(url)
      .then(async () => {
        $(`#${modalSource}-${selectedTableCellId}-text`).text(cost.toFixed(4));
        $(`#amount-${selectedTableCellId}-text`).text((cost.toFixed(4) * $(`#quantity-${selectedTableCellId}-text`).text()).toFixed(2));
        await loadReceiveData();
      })
      .catch(toastrError);
    $("#inputModalId").modal("hide");
  }
  else if (modalSource === "quantity") {
    let quantity = parseFloat($("#inputModalValueId").val());
    let cost = receiveData[selectedTableCellId].cost;

    let tempCost = cost || 1;
    let tempQuantity = quantity || 1;

    if (tempCost * tempQuantity > 214748.3647) {
      let maxQuantity = Math.min(Math.floor(2147483647 / tempCost) / 10000, 9999);               // Floor to 4 decimal places
      $("#inputModalValueId").val(maxQuantity);
      $("#inputModalValueId").focus();
      let warningMessage = $("#quantityCantBeMoreThanXWhenCostIsY").val();
      warningMessage = warningMessage.replace("_COST_", cost);
      warningMessage = warningMessage.replace("_QUANTITY_", maxQuantity);
      toastr.warning(warningMessage);
      return;
    }

    if (tempCost * tempQuantity < -214748.3648) {
      let minQuantity = -Math.floor(2147483648 / tempCost) / 10000;               // Floor to 4 decimal places
      $("#inputModalValueId").val(minQuantity);
      $("#inputModalValueId").focus();
      let warningMessage = $("#quantityCantBeLessThanXWhenCostIsY").val();
      warningMessage = warningMessage.replace("_COST_", cost);
      warningMessage = warningMessage.replace("_QUANTITY_", minQuantity);
      toastr.warning(warningMessage);
      return;
    }

    if (isNaN(quantity)) {
      $("#inputModalValueId").focus();
      toastr.warning($("#fieldMustBeNumeric").val());
      return;
    }
    if (quantity > 9999) {
      $("#inputModalValueId").val(9999);
      toastr.warning($("#quantityRange").val());
      $("#inputModalValueId").focus();
      return;
    }

    let url = `StockTakesAndReceiving/UpdateTotal?stockCode=${receiveData[selectedTableCellId].stockCode}&cost=${receiveData[selectedTableCellId].cost}&quantity=${quantity}&sequence=${receiveData[selectedTableCellId].sequence}&fromText=false&batchId=${receiveData[selectedTableCellId].recNum}&purpose=1`;

    await ajaxGetAsync(url)
      .then(async () => {
        $(`#${modalSource}-${selectedTableCellId}-text`).text(quantity);
        $(`#amount-${selectedTableCellId}-text`).text((quantity * $(`#cost-${selectedTableCellId}-text`).text()).toFixed(2));
        await loadReceiveData();
      })
      .catch(toastrError);

    $("#inputModalId").modal("hide");
  }
  else $("#inputModalId").modal("hide");
}

let isInvoiceValid = (invoice) => {
  if (isNaN(invoice)) {
    isAllSelected = !isModalSourceInvoice ? false : isAllSelected;
    toastr.warning($("#pleaseEnterANumericValueAsInvoiceNumber").val());
    $("#inputModalValueId").focus();
    return false;
  }
  else if (!isModalSourceInvoice && (invoice > 9999999999 || invoice < -999999999)) {
    isAllSelected = false;
    $("#inputModalValueId").focus();
    toastr.warning($("#invoiceRange").val());
    return false;
  }
  else if (isModalSourceInvoice && (invoice > 9999999999 || invoice < 0)) {
    $("#inputModalValueId").val("");
    toastr.warning($("#invoiceRangeFromZero").val());
    $("#inputModalValueId").focus();
    return false;
  }
  return true;
}

let handleInputModalCancelButtonClick = () => {
  if (modalSource === "check-box-select-all") {
    refreshCheckboxSelection();
    $("#inputModalValueId").val("");
  }
}

$("#inputModalCloseButtonId").on('click', function () {
  if (modalSource === "check-box-select-all") {
    handleInputModalCancelButtonClick();
  }
});

$("#inputModalId").on("shown.bs.modal", function () {     // Detect if modal is shown
  $("#inputModalValueId").focus();
  isInputModalOkButtonClicked = false;
}).on("hidden.bs.modal", function () {                    // Detect if modal is hidden
  if (!isInputModalOkButtonClicked) {
    if (modalSource === "check-box-select-all") {
      handleInputModalCancelButtonClick();
    }
  }
  $("#inputModalValueId").val("");
});

async function addAllStocksToRecline() {
  let data = {
    Vendor: selectedVendor,
    Department: selectedDept,
    SubDepartment: selectedSubDept,
    SubDetail: selectedSubDetail,
    SelectedMonth: selectedMonth,
    Invoice: invoice,
    Purpose: purpose
  };

  let url = `StockTakesAndReceiving/AddAllStocksToRecLine`;

  $("#loading-screen").show();
  await ajaxPostAsync(url, data)
    .then(async _added => {
      $('#loading-screen').hide();
      if (_added) {
        isAllSelected = true;
        toastr.success($("#successfullyAdded").val());
        await loadReceiveData();
      }
      else {
        // reclines already added
        isAllSelected = true;
      }
    })
    .catch(() => {
      $('#loading-screen').hide();
      toastr.error(errorMessage);
    });

  renderSelectedCheckBoxes();
}

let renderSelectedCheckBoxes = () => {
  stocks.forEach(function (stock, i) {
    $(`#checkbox-${stock.stockCode.replace(/\s/g, '')}-${i}`).prop('checked', function () {
      return isAllSelected;
    });
  });
}

let checkOrUncheckStock = async (stockCode, description, row) => {
  stockReceivedText = $("#stockReceived").val();
  let costValue = 0;

  $("#stock-received-quantity-update-title").text(`${stockReceivedText}`);
  selectedStock = stockCode;
  if ($(`#checkbox-${stockCode.replace(/\s/g, '')}-${row}`).prop('checked')) {
    isQuantityUpdate = false;
    if (receiveData.filter(x => x.stockCode == stockCode).length > 0) return;
    if (!isStockTakes) {
      costValue = await getCost(stockCode);
    }

    await addRecLine(0, costValue, '', description);
  }
  else {
    if (receiveData.filter(x => x.stockCode === stockCode && (x.quantity === 0 || x.quantity === null)).length > 0) {
      await deleteRecLine(stockCode);
    }
  }
}

$('#btn-delete').on('click', async function () {
  let title = isStockTakes ? $("#stockTakes").val() : $("#stockReceive").val();
  let message = $("#areYouSureYouWantToDeleteThisEntry").val();

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      isDeleteButtonClicked = true;
      if (receiveData.length == 1) {
        await deleteAllRecLineAndRecHead();
        return;
      }
      let stock = receiveData?.filter(x => x.stockCode == selectedReceiveTableRowData.stockCode)[0];
      await deleteRecLine(stock.stockCode, stock.recNum, stock.sequence);
      isDeleteButtonClicked = false;
    }
  });
});

$('#btn-suspend').on('click', async function () {
  await Suspend();
});

$('#btn-unsuspend').on('click', async function () {
  await unsuspend();
});

async function Suspend() {
  $("#loading-screen").show();
  let url = `StockTakesAndReceiving/Suspend?purpose=${purpose}`;

  await ajaxGetAsync(url)
    .then(async () => {
      $("#loading-screen").hide();
      await loadReceiveData();
      toastr.success($("#successfullySuspended").val());
      $('#btn-unsuspend').prop('disabled', false);
    })
    .catch(() => toastr.error($("#failedToSuspend").val()));
}

async function unsuspend() {
  let url = `StockTakesAndReceiving/Unsuspend?purpose=${purpose}`;

  await ajaxGetAsync(url)
    .then(async () => {
      await loadReceiveData();
      toastr.success($("#successfullyUnsuspended").val());
      $('#btn-unsuspend').prop('disabled', true);
    })
    .catch(() => toastr.error($("#failedToUnsuspend").val()));
}

async function getRecLineSusCount() {
  let url = `StockTakesAndReceiving/GetRecLineSusCount?purpose=${purpose}`;

  await ajaxGetAsync(url)
    .then(_count => $('#btn-unsuspend').prop('disabled', !_count))
    .catch(toastrError);
}

$('#btn-delete-all').on('click', async function () {
  let title = $("#stockReceive").val();
  let message = $("#areYouSureYouWantToDeleteAll").val();

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      isDeleteButtonClicked = true;
      await deleteAllRecLineAndRecHead();
      isDeleteButtonClicked = false;
    }
  });

});

let enableOrDisableButtonsForReceiveGrid = (value) => {
  let hasReceivedData = !receiveData.length > 0;

  $('#btn-delete').prop('disabled', value);
  $('#btn-process').prop('disabled', hasReceivedData);
  $('#btn-suspend').prop('disabled', hasReceivedData);
  $('#btn-delete-all').prop('disabled', hasReceivedData);
  $('#btn-export').prop('disabled', hasReceivedData);

  if (isStockTakes) {
    $('#btn-variance-report').prop('disabled', hasReceivedData);
  }
  else {
    $('#btn-batch-report').prop('disabled', hasReceivedData);
  }
}

let displayStockTakesAndReceivingReport = () => {
  let language = $('#language').val();
  let reportName = !isStockTakes ? "StockReceivingReport" : "StockTakingReport";
  let windowName = !isStockTakes ? 'Stock Receive Report' : 'Stock Takes Report';
  let width = 900;
  let height = 1050;
  let reportWidth = !isStockTakes ? "842px" : "900px";
  let reportHeight = window.screen.height - 50;
  let data = {};

  data.reportName = reportName;
  data.language = language;
  data.reportWidth = reportWidth;
  data.reportHeight = reportHeight;
  data.purpose = purpose;
  data.companyName = companyName;

  displayReport(windowName, width, height, data);
}

$("#btn-process").on("click", async function () {
  let isChangePrice = false;
  if (purpose === 1) {
    if (policies.defMargin) {
      if (policies.confirmPc) {
        let title = $("#changePriceBasedOnMarginSetup").val();
        let message = $("#doYouWantToChangeThePrice").val();
        await showConfirmModal(title, message).then((result) => {
          if (result) {
            // Change the price based on new cost using the current margin
            isChangePrice = true;
          }
          else {
            // Do not change the price, but reclculate the margin based on Cost and price
            isChangePrice = false;
          }
        });
      }
      else {
        // No need for comnfirmation-Change the price based on new cost using the current margin
        isChangePrice = true;
      }
    }
    else {
      // Do not change the price, but recalculate the margin
      isChangePrice = false;
    }
  }
  else if (purpose === 2) {
    let title = $("#stockUpdate").val();
    let message = $("#calculateAllSalesConfirmationMessage").val();
    let isCalculatedAllSales = false;

    await showConfirmModal(title, message).then(async (result) => {
      if (result === true) {
        let processingText = $("#processing").val();
        $("#btn-process").html(`<div class="spinner-border spinner-border-sm"> </div> ${processingText}...`);
        $('#btn-process').prop('disabled', true);

        let url = `StockTakesAndReceiving/CalculateAllSales?purpose=${purpose}&isUpdateInitial=${false}`;

        await ajaxGetAsync(url)
          .then(async () => {
            isCalculatedAllSales = true;
            let notPrintedRecLinseCount = await getNotPrintedRecLinesCount();
            if (notPrintedRecLinseCount) {
              // TODO: show report
            }
          })
          .catch((error) => toastr.error($("#failedToCalculateAllSales").val()));
      }
      else {
        toastr.warning($("#processAborted").val());
        return;
      }
    });
    if (!isCalculatedAllSales) return;
  }

  let processingText = $("#processing").val();
  $("#btn-process").html(`<div class="spinner-border spinner-border-sm"> </div> ${processingText}...`);
  $('#btn-process').prop('disabled', true);

  let userCode = $("#userCode").val();
  let url = `StockTakesAndReceiving/Process`;

  let parameters = {
    setVendPolicy: policies.setVendor,
    updateNetworkPolicy: policies.updateNetwork,
    isChangePrice: isChangePrice,
    isCostMarkupPolicy: policies.isMarginCalcEqualsCostMkup,
    numPricePolicy: policies.numPrice,
    userCode: userCode,
    purpose: purpose
  };

  await ajaxPostAsync(url, parameters)
    .then(async () => {
      toastr.success($("#successfullyProcessed").val());
      await loadReceiveData();
    })
    .catch(() => {
      toastr.error($("#failedToProcess").val());
      $('#btn-process').prop('disabled', false);
    });

  let processText = $("#process").val();
  $("#btn-process").html(`${processText}`);
})

$("#btn-export").on("click", async function () {
  if (policies.hndheldFiles === "Access Database") {
    let warningMessage = $("#accessDatabaseOptionIsNotSupported").val();
    toastr.warning(warningMessage);
    return;
  }

  let exportingText = $("#exporting").val();
  $("#btn-export").html(`<div class="spinner-border spinner-border-sm"> </div> ${exportingText}...`);
  $('#btn-export').prop('disabled', true);

  if (purpose === 1) {                  // Stock Receiving
    await exportStockReceivingData();
  }
  else {                                // Stock Takes
    await exportStockTakesData();
  }

  let exportText = $("#exportSelectedStocks").val();
  $("#btn-export").html(`${exportText}`);
  enableOrDisableButtonsForReceiveGrid(selectedStock ? false : true);
})

let handleFileInput = async () => {
  if (policies.autoInv && $("#isAutoImportExport").val().toLowerCase() === "true") {
    let url = `File/GetImportFileContent?purpose=${purpose}`;

    await ajaxGetAsync(url)
      .then(async (response) => {
        if (response === "FileNotFound") {
          toastr.error($("#fileDoesNotExist").val());
        }
        else if (response !== null) {
          await importFile(response);
        }
      })
      .catch(() => toastr.error($("#anErrorHasOccurredWhileImportingFile").val()));
  }
  else {
    const fileInput = document.getElementById('file-input');
    fileInput.value = null;
    fileInput.click();
  }
}

let onFileSelected = async (event) => {
  const selectedFile = event.target.files[0];
  if (selectedFile.type !== 'text/plain') {
    toastr.error($("#invalidFileType").val());
    return;
  }

  try {
    const fileContent = await readFileAsText(selectedFile);
    importFile(fileContent);
  }
  catch (error) {
    toastr.error($("#anErrorHasOccurredWhileImportingFile").val());
  }
};

let importFile = async (fileContent) => {
  const lengthOfFile = fileContent.split(/\r\n|\n|\r/).filter(line => line.trim() !== '').length;
  let stockCodeMissingInfo = $("#stockCodeMissingInfo").val();
  let stockCodeNotExist = $("#stockCodeDoesNotExist").val();
  let stockCodeIsKit = $("#stockCodeIsAKit").val();
  let qtyMissingInfo = $("#qtyMissingInfo").val();
  let costMissingInfo = $("#costMissingInfo").val();
  let extraQuantityMissingInfo = $("#extraQuantityMissingInfo").val();
  let invoiceMissingInfo = $("#invoiceMissingInfo").val();
  let dateMissingInfo = $("#dateMissingInfo").val();

  const missinginformationMap = {
    "StockCodeMissing": stockCodeMissingInfo,
    "StockCodeDoesNotExist": stockCodeNotExist,
    "StockCodeIsAKit": stockCodeIsKit,
    "QuantityMissing": qtyMissingInfo,
    "ExtraQuantityMissing": extraQuantityMissingInfo,
    "InvoiceMissing": invoiceMissingInfo,
    "CostMissing": costMissingInfo,
    "DateMissing": dateMissingInfo
  };

  let url = `StockTakesAndReceiving/ImportFromFile?exceptedtax=${policies.exceptedTax}&costDec=${policies.costDec}&purpose=${purpose}`;
  let stockCountType = isStockTakes ? $("#stockTakes").val() : $("#stockReceiving").val();
  let theFollowingRecordsAreNotLoaded = $("#theFollowingRecordsAreNotLoaded").val();
  theFollowingRecordsAreNotLoaded = theFollowingRecordsAreNotLoaded.replace("_StockCountType_", stockCountType.toString());

  await ajaxPostAsync(url, fileContent)
    .then(async _badLines => {
      if (!_badLines.length) {
        deleteImportFile = true;
        toastr.success($("#successfullyImported").val());
        return;
      }

      deleteImportFile = lengthOfFile !== _badLines.length;

      let stockImportErrorReport = $("#stockImportErrorReport").val();
      let text = `                            ${stockImportErrorReport}\n`;
      text += `_____________________________________________________________________________________________\n`;
      text += `** ${theFollowingRecordsAreNotLoaded} **\n`;
      text += `_____________________________________________________________________________________________\n`;

      for (let i = 0; i < _badLines.length; i++) {
        const line = _badLines[i];
        const missinginfo = line.match(/StockCodeMissing|StockCodeDoesNotExist|StockCodeIsAKit|QuantityMissing|ExtraQuantityMissing|InvoiceMissing|CostMissing|DateMissing/);
        if (missinginfo && missinginformationMap.hasOwnProperty(missinginfo[0])) {
          const value = missinginformationMap[missinginfo[0]];
          _badLines[i] = line.replace(missinginfo[0], value);
        }
        text += _badLines[i] + "\n";
      }
      let message = $("#doYouWantToPrintTheLogFile").val();
      let title = $("#import").val();

      showConfirmModal(title, message).then((result) => {
        if (result === true) {
          let docDefinition = {
            content: [
              { text: text }
            ],
            defaultStyle: {
            }
          };
          pdfMake.createPdf(docDefinition).print();
        }
      });
    })
    .catch(() => toastr.error($("#anErrorHasOccurredWhileImportingFile").val()));

  if (deleteImportFile) {
    await ajaxPostAsync(`File/DeleteImportedFile?filePath=${$("#importFilePath").val()}`, {});
  }
  await loadReceiveData();
}

async function readFileAsText(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsText(file);

    reader.onload = function (event) {
      resolve(event.target.result);
    };
    reader.onerror = function (event) {
      reject(new Error(errorMessage));
    };
  });
}

function handleTableCellEdit(id, columnName) {
  let localizedColumnName = $(`#${columnName}`).val();
  let title = `${$("#edit").val()} ${localizedColumnName}`;
  let message = localizedColumnName;

  selectedTableCellId = id;
  modalSource = columnName;
  $("#inputModalTitleId").text(title);
  $("#inputModalMessageId").text(message);
  $("#inputModalId").modal("show");
}

$(document).on('change', 'select[name="receive-grid_length"]', function () {
  selectedPageLength = parseInt($(this).val());
});

let exportStockReceivingData = async () => {
  let url = `StockTakesAndReceiving/GetStockReceivingExportData`;

  await ajaxGetAsync(url)
    .then(async _data => {
      if (_data?.length > 0) {
        let text = "";

        for (let i = 0; i < _data?.length; i++) {
          text += `${i + 1}|` +
            `${_data[i].stockCode}|` +
            `${_data[i].description}|` +
            `${_data[i].cost?.toFixed(2)}|` +
            `${_data[i].price?.toFixed(2)}|` +
            `${_data[i].assoc === null ? "0.00" : _data[i].assoc?.toFixed(2)}\n`;
        }

        if ($("#isAutoImportExport").val().toLowerCase() === "true") {
          let url = "File/SaveFile?fileName=Recv.txt";

          await ajaxPostAsync(url, text)
            .then(async () => {
              await deleteAllRecLine();
              toastr.success(`${$("#successfullyExportedTo").val()} ${$("#exportFolderPath").val()}\\Recv.txt`);
            })
            .catch(() => toastr.error($("#anErrorHasOccurredWhileExportingFile").val()));
        }
        else {
          download("Recv.txt", text);
          await deleteAllRecLine();
        }
      }
      else {
        toastr.error($("#failedToExport").val());
      }
    })
    .catch(() => toastr.error($("#anErrorHasOccurredWhileExportingFile").val()));
}

let exportStockTakesData = async () => {
  let url = "StockTakesAndReceiving/GetStockTakesExportData";

  await ajaxGetAsync(url)
    .then(async _data => {
      let stockFileContent = "";
      let pluMastFileContent = "";

      for (let i = 0; i < _data?.stocks?.length; i++) {
        stockFileContent += `${i + 1}|` +
          `${_data.stocks[i].stockCode.trim()}|` +
          `${_data.stocks[i].description.trim()}|` +
          `${_data.stocks[i].sysQuantity}|` +
          `${_data.stocks[i].cost?.toFixed(2) ?? ""}|` +
          `${_data.stocks[i].price?.toFixed(2)}\n`;
      }

      for (let i = 0; i < _data?.pluMasts?.length; i++) {
        pluMastFileContent += `${_data.pluMasts[i].pluCode.trim()}|` +
          `${_data.pluMasts[i].pluPrim.trim()}|S\n`;
      }

      let folderPath = $("#exportFolderPath").val();
      let isStockFileExported = await exportFileBasedOnConfig(folderPath, "Stock.txt", stockFileContent);
      let isPluMastFileExported = await exportFileBasedOnConfig(folderPath, "PLUMast.txt", pluMastFileContent);
      if (isStockFileExported || isPluMastFileExported) {
        await deleteAllRecLine();
      }
    })
    .catch(() => toastr.error($("#anErrorHasOccurredWhileExportingFile").val()));
}

let getNotPrintedRecLinesCount = async () => {
  let notPrintedRecLinesCount = 0;
  let url = `StockTakesAndReceiving/GetNotPrintedRecLinesCount?purpose=${purpose}`;

  await ajaxGetAsync(url)
    .then(_ => notPrintedRecLinesCount = _)
    .catch(toastrError);

  return notPrintedRecLinesCount;
}

let refreshCheckboxSelection = () => {
  isAllSelected = false;
  $('#check-box-select-all').prop('checked', isAllSelected);
  renderSelectedCheckBoxes();
}
