async function loadStockItems() {
  const url = `${window.location.origin}/StockItems/GetStockItemsList`;
  await ajaxGetAsync(url)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);
}

async function loadCustomers() {
  const url = `${window.location.origin}/CustomersGrid/GetAllCustomers?orderByCode=${false}`;

  await ajaxGetAsync(url)
    .then(_customer => _customerData = _customer)
    .catch(toastrError);
}

let renderValues = async (optionData) => {
  if (optionData.type !== "TABLE")
    if (optionData.fieldType == "NUMERIC") {
      return `<input type="${optionData.type.toLowerCase()}" onkeypress="return onlyDigitKeyPress(event)" class="col infonet-input w-100 text-center edit-mode-control filter-value">`;
    }
    else {
      return `<input type="${optionData.type.toLowerCase()}" class="col infonet-input w-100 text-center edit-mode-control filter-value">`;
    }

  if (optionData.fieldName === "Stock_Code")
    return `<input type="text" id="stockCode" class="col infonet-input stock-form-input w-100 text-center filter-value" readonly placeholder="${$("#select").val()}"
              onclick="selectStockClicked(this)"/>`;

  if (optionData.fieldName === "Client")
    return `<input type="text" id="customer" class="col infonet-input customer-form-input w-100 text-center filter-value" readonly placeholder="${$("#select").val()}"
              onclick="selectCustomerClicked(this)"/>` ;

  let html = `<select class="form-select edit-mode-control filter-value">`;

  const url = `${window.location.origin}/ElectronicJournal/GetSearchDropdownValues`;

  await ajaxPostAsync(url, optionData)
    .then(_valueData => {
      html += `<option class="text-center" value="">${$("#select").val()}</option>`;

      _valueData.forEach(data => {
        html += `<option class="text-center" value="${data}"> ${data}</option>`;
      })
    })
    .catch(toastrError);

  html += `</select>`;
  return html;
}

let selectCustomerClicked = (clickedElement) => {
  clickedCustomerField = clickedElement;
  $("#customerDataTableModal").modal('show');
  renderCustomerDataTable();
}

let onCustomerRowClick = (rowData) => {
  $(clickedCustomerField).closest('.customer-form-input').val(rowData.name.trim());
  $("#customerDataTableModal").modal('hide');
}

let selectStockClicked = (clickedElement) => {
  clickedStockField = clickedElement;
  renderStockCodeDataTable(onStockCodeRowClick);
  $("#stockCodeDescriptionDataTableModal").modal('show');
}

let onStockCodeRowClick = (rowData) => {
  $(clickedStockField).closest('.stock-form-input').val(rowData.stockCode);
  $("#stockCodeDescriptionDataTableModal").modal('hide');
}

let renderCustomerDataTable = () => {
  destroyDataTableIfExists(_customersDatatable);
  let props = createDataTableProp();
  props.data = _customerData;
  props.paging = false;
  props.autoWidth = false;
  props.fixedColumns = true;
  props.scrollY = '300px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' },
    { width: "220px", targets: '_all' },
  ];

  props.columns = [
    { "data": "name" },
    { "data": "code" }
  ];

  _customersDatatable = createModalDatatableWithSearching('customers-grid', props);

  $('#customers-grid tbody')
    .off()
    .on('click', 'tr', function () {
      const tableinfo = _customersDatatable.page.info();

      if (tableinfo.recordsDisplay === 0)
        return;

      _customersDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
      $(this).addClass('selected-grid-row');

      let rowData = _customersDatatable.row(this).data();
      onCustomerRowClick(rowData);
    });
}

let loadStores = async () => {
  let url = `${window.location.origin}/ElectronicJournal/GetStoreDropdownValues`;
  await ajaxGetAsync(url)
    .then(_stores => {
      storeList = _stores;
      _stores.forEach(option => { $("#store-select").append(optionGeneratorForDifferentIdAndData(option.storeId, option.storeDesc)); });
    })
    .catch(toastrError);
}