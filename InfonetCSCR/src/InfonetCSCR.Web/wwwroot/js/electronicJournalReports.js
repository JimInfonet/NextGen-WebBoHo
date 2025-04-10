let companyName;
let searchDescrptions;
let electronicJournalReportDataTable;
let ejSearchDescriptions;
let searchDescriptions;
let ejSearchOptionName;
let startDate;
let endDate;
let searchDate = "";
let storeRadioOption;
let selectedReportRow;
let selecteReportSaleNo;
let selectedStore;
let selectedPageLength = 25;
let ejReportsSearchParams;
let localStore;
let isLocalStoreSelected = false;
let isWareHouse = false;
let _customerData;
let _customersDatatable;

$(async function () {
  ejSearchDescriptions = JSON.parse(localStorage.getItem('ElectronicJournalReportsSearchParams')) ?? [];
  ejSearchOptionName = ejSearchDescriptions.length > 0 ? ejSearchDescriptions[0].optionName : "Date";

  companyName = await getCompanyName();

  const urlParams = new URLSearchParams(window.location.search);
  startDate = urlParams.get('startDate');
  endDate = urlParams.get('endDate');

  if (!_globalIsBackOffice) {
    selectedStore = JSON.parse(localStorage.getItem('SelectedStore'));
    localStore = JSON.parse(localStorage.getItem('LocalStore'));
    isWareHouse = (localStorage.getItem('IsWareHouse') || "").toLowerCase() === "true";
    isLocalStoreSelected = selectedStore.storeId === localStore;
  }

  loadElectronicJournalGridData();
  await getSearchDescriptions(ejSearchOptionName);
  await renderFilterTypes();
  await loadStockItems();
  await loadCustomers();
});

let getSearchDescriptions = async (optionName) => {
  const url = `/ElectronicJournal/GetElectronicJournalFilters?optionName=${optionName}&isWareHouse=${isWareHouse}`;

  await ajaxGetAsync(url)
    .then(_searchDescriptions => searchDescriptions = _searchDescriptions)
    .catch(toastrError);
}

let renderFilterTypes = async () => {
  let control = $("#filter-type");
  let optionData = JSON.stringify({ type: 'DATE', fieldType: 'STRING' });
  let optionHTML =
    `<option class="text-center" data-optiondata='${optionData}' value="Date">Date</option>`;

  control.append(optionHTML);

  searchDescriptions.forEach(option => {
    optionData = JSON.stringify(option);
    optionHTML =
      `<option class="text-center" data-optiondata='${optionData}' value="${option.description}"> ${option.description}</option>`;

    control.append(optionHTML);
  });

  await updateFilterValueOptions();
}

let updateFilterValueOptions = async () => {
  let filterTypeElement = document.getElementById('filter-type');
  let filterValueField = document.querySelector('.filter-value');
  let selectedOption = filterTypeElement.options[filterTypeElement.selectedIndex];
  let optionData = JSON.parse(selectedOption.dataset.optiondata);
  let newFieldHTML = await renderValues(optionData);

  let newDiv = document.createElement('div');
  newDiv.innerHTML = newFieldHTML;

  filterValueField.replaceWith(newDiv.firstChild);

  if (optionData.type === "DATE") {
    filterValueField = document.querySelector('.filter-value');
    filterValueField.value = new Date().toISOString().substring(0, 10);
    filterValueField.addEventListener('input', function () {
      if (!filterValueField.value) {
        filterValueField.value = new Date().toISOString().substring(0, 10);
      }
    });
  }
}

let goBackPage = () => {
  let url = `${window.location.origin}/ElectronicJournal`;
  safelyNavigateTo(url);
}

let searchReport = () => {
  ejSearchDescriptions = JSON.parse(localStorage.getItem('ElectronicJournalReportsSearchParams')) ?? [];
  let filterTypeElement = document.getElementById('filter-type');
  let filterValueField = document.querySelector('.filter-value');
  let selectedOption = filterTypeElement.options[filterTypeElement.selectedIndex];
  let optionData = JSON.parse(selectedOption.dataset.optiondata);

  if (optionData.type === "DATE") {
    searchDate = filterValueField.value;
  }
  else {
    searchDate = "";
    optionData.relation = "AND";
    optionData.operator = document.getElementById('filter-operator').value;
    optionData.value = filterValueField.value;
    ejSearchDescriptions.push(optionData);
  }

  let searchingText = $('#searching').val();
  let searchText = $('#search').val();

  $("#searchEJData")
    .html(`<span class="spinner-border spinner-border-sm" role="status"></span> ${searchingText}...`).prop("disabled", true);

  loadElectronicJournalGridData();

  $("#searchEJData").html(searchText).prop("disabled", false);
}

//Grid
let loadElectronicJournalGridData = () => {
  const isWareHouse = selectedStore ? selectedStore.isWareHouse : "";
  const storeRadioOption = localStorage.getItem('StoreRadioOption');

  let urlParams = new URLSearchParams(window.location.search);

  ejReportsSearchParams = {
    StartDate: urlParams.get('startDate') ?? "",
    EndDate: urlParams.get('endDate') ?? "",
    SearchOperator: $("#filter-operator").val(),
    SearchDate: searchDate.replaceAll('-', ''),
    IsWareHouse: isWareHouse || false,
    StoreRadioOption: storeRadioOption || " ",
    EJSearchDescription: ejSearchDescriptions || []
  }

  createEJReportDataTable();
}

let createEJReportDataTable = () => {
  destroyDataTableIfExists(electronicJournalReportDataTable);

  let props = createDataTableProp();
  props.scrollY = '440px';
  props.serverSide = true;
  props.stateSave = true;
  props.searching = false;
  props.pageLength = parseInt(selectedPageLength);
  props.lengthMenu = [[10, 25, 50, 100], [10, 25, 50, 100]];
  props.columns = [
    { "data": "storeID", },
    { "data": "saleNo", },
    { "data": "shiftDate", },
    { "data": "till", },
    { "data": "amount", },
    { "data": "customer", },
    { "data": "type", },
    {
      "data": "sdt",
      render: sdt => sdt.replace("T", " "),
    },
    { "data": "source", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];
  props.ajax = {
    url: `/ElectronicJournal/GetElectronicJournalGridData`,
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    data: function (d) {
      d.StartDate = ejReportsSearchParams.StartDate;
      d.EndDate = ejReportsSearchParams.EndDate;
      d.SearchOperator = ejReportsSearchParams.SearchOperator;
      d.SearchDate = ejReportsSearchParams.SearchDate;
      d.IsWareHouse = ejReportsSearchParams.IsWareHouse;
      d.StoreRadioOption = ejReportsSearchParams.StoreRadioOption;
      d.EJSearchDescription = ejReportsSearchParams.EJSearchDescription;
      d.IsLocalStoreSelected = isLocalStoreSelected;
      d.SelectedStoreId = (!_globalIsBackOffice ? selectedStore.storeId : null);

      return JSON.stringify(d);
    },
  };
  props.rowsGroup = [0];

  electronicJournalReportDataTable = createDataTableGeneric('electronic-journal-grid', props);
  electronicJournalReportDataTable.column('#Store').visible(isLocalStoreSelected);

  if (_globalIsBackOffice) {
    electronicJournalReportDataTable.on('draw', function () {
      $("#summary-button").prop("disabled", electronicJournalReportDataTable.data().length === 0);
      $("#details-button").prop("disabled", electronicJournalReportDataTable.data().length === 0);
      $("#btnViewDetails").prop("disabled", electronicJournalReportDataTable.data().length === 0);
    });
  }

  $('#electronic-journal-grid tbody')
    .off()
    .on('click', 'tr', function () {

      if (electronicJournalReportDataTable.data().length === 0)
        return;

      electronicJournalReportDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
      $(this).addClass('selected-grid-row');

      selectedReportRow = electronicJournalReportDataTable.row(this).data();
      let isFromTills = selectedReportRow.source === 1 ? true : false;

      let isDetailsReport = false;

      if (!_globalIsBackOffice)
        isDetailsReport = $('input[name="report-radio"]:checked').val() === "TD";

      displayEJReport(selectedReportRow.saleNo, isFromTills, isDetailsReport, selectedReportRow.storeID)
    });
}

// Triggers when page length is changed
$(document).on('change', 'select[name="electronic-journal-grid_length"]', function () {
  selectedPageLength = parseInt($(this).val());
});

function displayEJReport(saleNo, isFromTills, isDetailsReport = false, storeID = "") {
  let iFrame = document.getElementById('IframeReportViewer');
  let language = document.getElementById('language').value;
  let reportName = isDetailsReport ? "ElectronicJournalTenderDetailsReport" : "ElectronicJournalReport";

  let data = {};
  data.reportName = reportName;
  data.language = language;
  data.saleNo = saleNo;
  data.storeID = storeID;
  data.isFromTills = isFromTills;
  data.companyName = companyName;
  iFrame.height = "1250px";
  iFrame.removeAttribute("hidden");
  iFrame.contentWindow.postMessage(data, $("#_reportBaseUrl").val());
}

let showSummaryOrDetailsReport = (summaryOrDetails) => {
  const start = new Date(startDate.slice(0, 4), startDate.slice(4, 6) - 1, startDate.slice(6, 8));
  const end = new Date(endDate.slice(0, 4), endDate.slice(4, 6) - 1, endDate.slice(6, 8));

  const timeDifference = end - start;
  const dayDifference = timeDifference / (1000 * 60 * 60 * 24) + 1;

  const acceptedDateRange = document.getElementById("acceptedDateRange").innerText;
  const warningMessage = `${$("#pleaseSelectLessThan").val()} ${acceptedDateRange} ${$("#days").val()}`;

  if (dayDifference > acceptedDateRange) {
    toastr.warning(warningMessage);
    return;
  }

  let language = document.getElementById('language').value;
  let reportName = summaryOrDetails === "summary" ? "ElectronicJournalSummaryReport" : "ElectronicJournalDetailsReport";

  let result = {};
  result.reportName = reportName;
  result.language = language;
  result.companyName = companyName;
  result.reportWidth = "950px";
  result.reportHeight = window.screen.height - 150;

  let width = 1000;
  let height = window.screen.height - 50;
  displayReport(reportName, width, height, result);
}

let reportRadioClicked = () => {
  if (!isNullOrUndefined(selectedReportRow)) {
    let isFromTills = selectedReportRow.source === 1 ? true : false;
    let isDetailsReport = $('input[name="report-radio"]:checked').val() === "TD";

    displayEJReport(selectedReportRow.saleNo, isFromTills, isDetailsReport)
  }
}

function viewElectronicJournalDetailReport() {
  const url = '/ElectronicJournal/ElectronicJournalDetailReport';
  let width = 1100;
  let height = 1100;
  let left = (screen.width / 2) - (width / 2);
  let top = (screen.height / 2) - (height / 2);
  let options = "toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=" + width + ", height=" + height + ", top=" + top + ", left=" + left;
  let ejDetailReportPopUpWindow = window.open(url, 'Electronic Journal Detail Report', options);
  ejDetailReportPopUpWindow.focus();
}
