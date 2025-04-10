let companyName;
let searchDescriptions;
let searchFilterDataTable;
let clickedStockField;
let clickedCustomerField;
let _customerData;
let _customersDatatable;
let isFromTills;
let storeList = [];
let storeRadioOption;
let localStore;
let isLocalStoreSelected;
let selectedStoreId;
let selectedStore;
let isWareHouse;

$(async function () {
  let startDate = document.getElementById('startDate');
  let endDate = document.getElementById('endDate');

  startDate.addEventListener('input', function () {
    if (!startDate.value) {
      startDate.value = new Date().toISOString().substring(0, 10);
    }
  });

  endDate.addEventListener('input', function () {
    if (!endDate.value) {
      endDate.value = new Date().toISOString().substring(0, 10);
    }
  });

  let today = new Date();
  startDate.value = today.toISOString().substring(0, 10);
  endDate.value = today.toISOString().substring(0, 10);

  if (!_globalIsBackOffice) {
    await loadStoreOptions();
    await storeChange();
    storeRadioClicked();
  }
  else
    await loadSearchOptions();

  await loadStockItems();
  await loadCustomers();
});

let loadSearchOptions = async () => {
  const url = "/ElectronicJournal/GetElectronicJournalOptions";
  let searchOptions;

  await ajaxGetAsync(url)
    .then(_options => searchOptions = _options)
    .catch(toastrError);

  let searchOptionsContainer = $('#journal-options-container-id');
  searchOptionsContainer.empty();

  searchOptions.forEach(options => {
    let option =
      $('<option>').attr('value', options)
        .text(`${options}`)
        .click(async () => getSearchDescriptions(options));

    searchOptionsContainer.append(option);
  });

  searchOptionsContainer.prop('selectedIndex', 0).find('option').first().trigger('click');
}

let loadStoreOptions = async () => {
  let url = "/ElectronicJournal/GetSetupCStore"
  localStore = await ajaxGetAsync(url);

  localStorage.setItem('LocalStore', JSON.stringify(localStore));
  await loadStores();

  $('#store-select').find('option').each(function () {
    if ($(this).val() === localStore) {
      $('#store-select').val(localStore);
      return false;
    }
  });
}

let getSearchDescriptions = async (optionName) => {
  const url = `/ElectronicJournal/GetElectronicJournalFilters?optionName=${optionName}&isWareHouse=${isWareHouse}`;

  await ajaxGetAsync(url)
    .then(_searchDescriptions => searchDescriptions = _searchDescriptions)
    .catch(toastrError);

  createSearchFilterDataTable();
}

let createSearchFilterDataTable = () => {
  destroyDataTableIfExists(searchFilterDataTable);

  let props = createDataTableProp();
  props.data = searchDescriptions;
  props.scrollY = '300px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4] }
  ];
  props.paging = false;
  props.bInfo = false;

  props.columns = getColumnsOfSearchFilterDataTable();

  searchFilterDataTable = createDataTableGeneric('search-filter-table', props);
  initSearchTable();
}

function getColumnsOfSearchFilterDataTable() {
  let columns = [
    {
      data: "relation",
      render: renderRelation
    },
    {
      data: "description",
      render: renderSelectHTML
    },
    {
      data: "operators",
      render: renderOperators
    },
    {
      data: "type",
      render: () => `<input type="text" class="col infonet-input w-100 edit-mode-control filter-value">`
    },
    {
      data: "",
      render: renderMinusSignForRowDelete
    }
  ];

  return columns;
}

$('#btn-add-new-row').on('click', function (event) {
  event.preventDefault();
  searchFilterDataTable.row.add({
  }).draw();

  const lastRow = $('#search-filter-table tbody tr').last();
  lastRow.find('.filter-type').each(function () {
    $(this).trigger('change');
  });

  if (searchFilterDataTable.rows().count() >= 10) {
    $(this).prop('disabled', true);
  }
});

$('#search-filter-table').on('click', '.btn-delete-this-row', function () {
  const row = $(this).closest('tr');
  searchFilterDataTable.row(row).remove().draw();

  $("#btn-add-new-row").prop('disabled', false);
});

let initSearchTable = () => {
  const dropdowns = document.querySelectorAll('.filter-type');

  dropdowns.forEach((dropdown, index) => {
    dropdown.selectedIndex = index;
    dropdown.dispatchEvent(new Event('change'));
  });

  document.querySelectorAll('.btn-delete-this-row').forEach((button) => { button.remove(); });
  document.querySelector('.filter-relation').remove();
}

let updateFilterValueOptions = async (filter) => {
  let filterValueField = filter.closest('tr').querySelector('.filter-value');
  let selectedOption = filter.options[filter.selectedIndex];
  let optionData = JSON.parse(selectedOption.dataset.optiondata);

  let newFieldHTML = await renderValues(optionData);

  let newDiv = document.createElement('div');
  newDiv.innerHTML = newFieldHTML;

  filterValueField.replaceWith(newDiv.firstChild);
}

let renderRelation = () => {
  let selectHTML =
    `<select class="form-select edit-mode-control filter-relation">
      <option class="text-center" value="AND">${$("#and").val()}</option>
      <option class="text-center" value="OR">${$("#or").val()}</option>
     </select>`;
  return selectHTML;
}

let renderOperators = () => {
  let selectHTML =
    `<select class="form-select edit-mode-control">
      <option class="text-center" value="=">=</option>
      <option class="text-center" value="!=">!=</option>
      <option class="text-center" value=">">&gt;</option>
      <option class="text-center" value="<">&lt;</option>
      <option class="text-center" value="<=">&lt;=</option>
      <option class="text-center" value=">=">&gt;=</option>
     </select>`;
  return selectHTML;
}

let renderSelectHTML = (data) => {
  let selectHTML = `<select class="form-select edit-mode-control filter-type" onchange="updateFilterValueOptions(this)">`;

  searchDescriptions.forEach(option => {
    const optionData = JSON.stringify(option);
    selectHTML +=
      `<option class="text-center" data-optiondata='${optionData}' value="${option.description}"> ${option.description}</option>`;
  })

  selectHTML += `</select>`;
  return selectHTML;
}

let renderMinusSignForRowDelete = () => {
  return `<div class="d-flex justify-content-center">
            <button class="btn btn-sm btn-outline-danger edit-mode-control btn-delete-this-row"> - </button>
          </div>`;
}

let extractTableData = () => {
  const table = document.getElementById('search-filter-table');
  const rows = table.querySelector('tbody').querySelectorAll('tr');
  let result = [];

  rows.forEach(row => {
    const cells = row.querySelectorAll('td');

    let fieldNameDropdown = cells[1].querySelector('select')
    let selectedFieldName = fieldNameDropdown.options[fieldNameDropdown.selectedIndex];
    let searchData = JSON.parse(selectedFieldName.dataset.optiondata)

    const relation = cells[0].querySelector('select') ? cells[0].querySelector('select').value : "AND";
    const operator = cells[2].querySelector('select').value;
    const value = cells[3].querySelector('input') ? cells[3].querySelector('input').value : cells[3].querySelector('select').value;

    searchData.relation = relation;
    searchData.operator = operator;
    searchData.value = value;
    result.push(searchData);
  });

  return result;
}

let ejSearch = () => {
  let startDateString = startDate.value.replaceAll('-', '');
  let endDateString = endDate.value.replaceAll('-', '');

  if (startDateString > endDateString) {
    toastr.warning($("#startDateCannotBeLaterThanEndDate").val());
    return;
  }

  let electronicJournalReportsSearchParams = extractTableData();
  localStorage.setItem('ElectronicJournalReportsSearchParams', JSON.stringify(electronicJournalReportsSearchParams));

  if (!_globalIsBackOffice)
    setStoreInfo();

  const url = `ElectronicJournal/ElectronicJournalReports?startDate=${startDateString}&endDate=${endDateString}`;
  safelyNavigateTo(url);
}

// HO logics
let storeChange = async () => {
  selectedStoreId = $("#store-select").val();
  selectedStore = storeList.find(store => store.storeId === selectedStoreId) || null;
  isWareHouse = selectedStore ? selectedStore.isWareHouse : "";
  isLocalStoreSelected = selectedStore.storeId === localStore;
  storeRadioClicked();
  localStorage.setItem('IsWareHouse', isWareHouse);

  await loadSearchOptions();
}

let setStoreInfo = () => {
  localStorage.setItem('StoreRadioOption', storeRadioOption);
  localStorage.setItem('SelectedStore', JSON.stringify(selectedStore));
}

let storeRadioClicked = () => {
  storeRadioOption = $('input[name="storeRadioOption"]:checked').val();
  validateLocalStoreAndCurrentSales();

  $("#startDate").prop("disabled", storeRadioOption === 'C');
  $("#endDate").prop("disabled", storeRadioOption === 'C');

  if (storeRadioOption === 'C') {
    startDate.value = new Date().toISOString().substring(0, 10);
    endDate.value = new Date().toISOString().substring(0, 10);
  }
}

let validateLocalStoreAndCurrentSales = () => {
  if (isLocalStoreSelected && storeRadioOption === "C") {
    toastr.warning($("#hoContainsHistoricalSalesOnly").val());
    $("#saleHistory").prop("checked", true);
    storeRadioOption = $('input[name="storeRadioOption"]:checked').val();
  }
}