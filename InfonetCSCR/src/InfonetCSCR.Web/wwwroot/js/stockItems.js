const URL_GET_STOCK_ITEMS = "/stockitems";
let isBackOffice = true;
let isServerSidePagination = true;
let isVendorPartSelected = false;
let isDetailsClicked = false;
let datatable;
let selectedCriteria = "stockCode";
let dictionary = {};
let errorMessage;
let selectedPageLength;
let isFromStockForm = false;

const LOCAL_STORAGE_KEYS = {
  DATATABLE_STOCK_ITEMS: "DataTables_stock-items-grid_/StockItems",
}

dictionary["stockCode"] = "stockCodeSearchText";
dictionary["vendor"] = "selectVendor";
dictionary["description"] = "descriptionSearchText";
dictionary["alternateCode"] = "alternateCodeSearchText";
dictionary["keyword"] = "keywordSearchText";
dictionary["priceType"] = "selectPriceType";
dictionary["vendorPart"] = "vendorPartSearchText";
dictionary["departments"] = "selectDepartment";

$(async function () {
  selectedPageLength = 10;
  errorMessage = $("#_defaultErrorMessage").val();
  isBackOffice = (document.getElementById("isBackOffice").innerText === "True");
  isServerSidePagination = (document.getElementById("isServerSidePagination").innerText === "True");
  await loadDepartmentDropdown();
  await loadVendor();

  isFromStockForm = $("#isFromStockForm").val() === "True";
  if (isFromStockForm === false) {
    localStorage.setItem("selectedCriteria", null);
    localStorage.setItem("searchText", null);
    localStorage.setItem("stockCodeSearchText", "");
    clearSearchCriteria();
  }
  else {
    let newURL = removeURLParameter(window.location.href, "isFromStockForm");
    window.history.replaceState(null, null, newURL);
  }
  await loadStockItemsFromLocalStorage();
  setButtonsHeight('.button-group a');
});

function clearSearchCriteria() {
  const radioButtons = document.querySelectorAll('input[name="selectCriteria"]');

  for (const radioButton of radioButtons) {
    if (radioButton.checked) {
      selectedCriteria = radioButton.id;
      localStorage.setItem("selectedCriteria", selectedCriteria);

      ele = document.getElementById(dictionary[radioButton.value]);
      if (ele !== null) {
        ele.hidden = false;
        ele.value = "";
      }
      if (radioButton.value === "departments") {
        $('#selectDepartment').val('All')
        document.getElementById("selectDepartment").hidden = false;
        document.getElementById("selectSubDepartment").hidden = false;
        document.getElementById("selectSubDetail").hidden = false;

        //clear SubDepartment and SubDetail dropdown
        let subDepartmentSelection = $('#selectSubDepartment');
        let optionTemplate = $('<option></option>');
        optionTemplate.append("All");
        optionTemplate.attr("value", "All");

        subDepartmentSelection.find('option').remove();
        let option = optionTemplate.clone();
        subDepartmentSelection.append(option);

        let subDetailSelection = $('#selectSubDetail');
        subDetailSelection.find('option').remove();
        option = optionTemplate.clone();
        subDetailSelection.append(option);
      }
      else {
        document.getElementById("selectDepartment").hidden = true;
        document.getElementById("selectSubDepartment").hidden = true;
        document.getElementById("selectSubDetail").hidden = true;
      }
    }
    else {
      ele = document.getElementById(dictionary[radioButton.value]);
      if (ele !== null) ele.hidden = true;
    }
  }

  if (document.getElementById("vendorPart").checked) {
    isVendorPartSelected = true;
  }
  else isVendorPartSelected = false;
  $('#selectVendor').val('All')
  $('#selectPriceType').val('All')
}

let loadVendor = async () => {
  const url = URL_GET_STOCK_ITEMS + "/GetVendors";

  return ajaxGetAsync(url)
    .then(_vendor => {
      let vendorSelection = $('#selectVendor');
      let optionTemplate = $('<option></option>');
      vendorSelection.val('All')

      for (const vendor of _vendor) {
        let option = optionTemplate.clone();
        option.append(vendor.code + " - " + vendor.name);
        option.attr('value', vendor.code.split(" ")[0]);
        vendorSelection.append(option);
      }
    })
    .catch(toastrError);
}

let loadDepartmentDropdown = async () => {
  let url = `${URL_GET_STOCK_ITEMS}/GetDepartments`;

  await ajaxGetAsync(url)
    .then(_depts => {
      let initialOptions = [
        { text: "All", value: "All" },
      ];

      renderCategoryDropdown("selectDepartment", _depts, initialOptions, "department");
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingDepartment").val()));
}

function selectDropDown(typeId, name) {
  let element = document.getElementById(typeId);
  element.innerHTML = name;
}

let loadSubDepartmentDropdown = async () => {
  let department = $("#selectDepartment").val();
  let url = `${URL_GET_STOCK_ITEMS}/GetSubDepartments?department=${department}`;

  await ajaxGetAsync(url)
    .then(_subDepts => {
      let initialOptions = [
        { text: "All", value: "All" },
      ];

      renderCategoryDropdown("selectSubDepartment", _subDepts, initialOptions, "sub-department");
    })
    .catch(toastrError);
}

let loadSubDetailDropdown = async () => {
  let department = $("#selectDepartment").val();
  let subDepartment = $("#selectSubDepartment").val();
  let url = `${URL_GET_STOCK_ITEMS}/GetSubDetails?department=${department}&subDepartment=${subDepartment}`;

  await ajaxGetAsync(url)
    .then(_subDetails => {
      let initialOptions = [
        { text: "All", value: "All" },
      ];

      renderCategoryDropdown("selectSubDetail", _subDetails, initialOptions, "sub-detail");
    })
    .catch(toastrError);
}

async function departmentChange(id) {
  if (id === 'department') {
    await loadSubDepartmentDropdown();
    document.getElementById("selectSubDepartment").value = "All";
    document.getElementById("selectSubDetail").value = "All";
  }
  else if (id === 'subDepartment') {
    await loadSubDetailDropdown();
    document.getElementById("selectSubDetail").value = "All";
  }
  filterData();
}

let initializeSelectedStockItemData = () => {
  document.getElementById("selectedStockItemsStockCode").value = "";
  document.getElementById("selectedStockItemsDescription").value = "";
}

let makeButtonActive = () => {
  if (isBackOffice) {
    let makeInactiveButton = document.getElementById("makeInactiveButton");
    makeInactiveButton.classList.remove("btn-primary");
    makeInactiveButton.classList.add("btn-danger");
    makeInactiveButton.innerText = document.getElementById("makeActive").value;
  }
}

let makeButtonInactive = () => {
  if (isBackOffice) {
    let makeInactiveButton = document.getElementById("makeInactiveButton");
    makeInactiveButton.classList.remove("btn-danger");
    makeInactiveButton.classList.add("btn-primary");
    makeInactiveButton.innerText = document.getElementById("makeInactive").value;
  }
}

let activeButtonClicked = () => {
  let makeInactiveButton = document.getElementById("makeInactiveButton");
  let makeActive = document.getElementById("makeActive").value;
  let selectedStockCode = document.getElementById("selectedStockItemsStockCode").value;
  let currentRow = datatable.$('tr.selected-grid-row');
  let rowData = datatable.row(currentRow).data();

  if (selectedStockCode === null || selectedStockCode === "") {
    let message = document.getElementById("selectItemBeforeActiveOrInactiveAlert").value;
    toastr.warning(message);
    return;
  }

  let url = "/StockItems/UpdateStockItemActiveStatus";
  let data = { stockCode: selectedStockCode, flag: (makeInactiveButton.innerText === makeActive) };

  ajaxGetAsync(url, data)
    .then(() => {
      if (makeInactiveButton.innerText === makeActive) {
        makeButtonInactive();
        rowData["isActive"] = true;
      }
      else {
        makeButtonActive();
        rowData["isActive"] = false;
      }
    })
    .catch(toastrError);
}

let makeButtonRemoveKit = () => {
  let makeItKitButton = document.getElementById("makeItKitButton");
  makeItKitButton.classList.remove("btn-primary");
  makeItKitButton.classList.add("btn-danger");
  makeItKitButton.innerText = document.getElementById("removeKit").value;
}

let makeButtonMakeItKit = () => {
  let makeItKitButton = document.getElementById("makeItKitButton");
  makeItKitButton.classList.remove("btn-danger");
  makeItKitButton.classList.add("btn-primary");
  makeItKitButton.innerText = document.getElementById("makeItKit").value;
}

let kitButtonClicked = () => {
  let makeItKitButton = document.getElementById("makeItKitButton");
  let removeKitText = document.getElementById("removeKit").value;
  let selectedStockCode = document.getElementById("selectedStockItemsStockCode").value;
  let selectedStockDescription = document.getElementById("selectedStockItemsDescription").value;
  let currentRow = datatable.$('tr.selected-grid-row');
  let rowData = datatable.row(currentRow).data();

  if (selectedStockCode === null || selectedStockCode === "") {
    let message = document.getElementById("selectItemBeforeMakingKitAlert").value;
    toastr.warning(message);
    return;
  }

  if (makeItKitButton.innerText === removeKitText) {
    const url = "/StockItems/RemoveStockItemKit";

    ajaxGetAsync(url, { stockCode: selectedStockCode })
      .then(() => {
        makeButtonMakeItKit();
        rowData["isKit"] = false;
      })
      .catch(() => toastr.error(errorMessage));
  }
  else {
    const url = "/StockItems/MakeStockItemKit";

    ajaxGetAsync(url, { stockCode: selectedStockCode, stockDescription: selectedStockDescription })
      .then(() => {
        makeButtonRemoveKit();
        rowData["isKit"] = true;
      })
      .catch(error => {
        if (error.status !== HttpStatusCode.Conflict) {
          toastr.error(errorMessage);
          return;
        }
        const alreadyAKitErrorMessage = $("#alreadyAKit").val()
          .replace("__STOCK_CODE__", selectedStockCode)
          .replace("__STOCK_DESCRIPTION__", selectedStockDescription);
        toastr.warning(alreadyAKitErrorMessage);
      });
  }
}

function createDatatable(parameters) {

  if (datatable) {
    datatable.state.clear();
    datatable.clear();
    datatable.destroy();
  }

  let url = URL_GET_STOCK_ITEMS + "/GetStockItems" + parameters;
  let props = createDataTableProp();
  props.searching = false;
  props.stateSave = true;
  props.stateLoadCallback = function () {
    let savedState = localStorage.getItem(LOCAL_STORAGE_KEYS.DATATABLE_STOCK_ITEMS);
    return isFromStockForm ? JSON.parse(savedState) : null;
  }
  props.serverSide = isServerSidePagination;
  props.pageLength = parseInt(selectedPageLength);
  props.callOnDrawCallback = function () {
    localStorage.setItem("persistPagination", "true");
  };
  props.ajax = {
    url: url,
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    data: function (d) {
      return JSON.stringify(d);
    }
  };
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "stockCode" },
    { "data": "description" },
    { "data": "vendor", "visible": isDetailsClicked, },
    { "data": "vendorPart", "visible": isDetailsClicked },
    { "data": "department", "visible": !isDetailsClicked, },
    { "data": "subDepartment", "visible": !isDetailsClicked },
    { "data": "cost", className: "dataTable-text-right" },
    { "data": "price", className: "dataTable-text-right" },
    { "data": "available", className: "dataTable-text-center" },
    { "data": "isActive", },
    { "data": "isKit", }
  ];

  datatable = createDataTableGeneric('stock-items-grid', props, true);

  $('#stock-items-grid').on('page.dt', function () {
    makeButtonInactive();
    makeButtonMakeItKit();
  });

  $('#stock-items-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      const tableinfo = datatable.page.info();
      if (tableinfo.recordsDisplay === 0) {
        return;
      }

      let stockCode = $(this).find("td:first")[0].innerText;
      if (stockCode === "No data available in table.") {
        return;
      }

      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        makeButtonInactive();
        makeButtonMakeItKit();
        initializeSelectedStockItemData();
      } else {
        datatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        let stockCode = $(this)[0].childNodes[0].innerText;
        let currentRow = datatable.$('tr.selected-grid-row');
        let data = datatable.row(currentRow).data();
        let isKit = data["isKit"];
        let isActive = data["isActive"];

        if (isActive === true) {
          makeButtonInactive();
        }
        else {
          makeButtonActive();
        }

        if (isKit === true) {
          makeButtonRemoveKit();
        }
        else {
          makeButtonMakeItKit();
        }

        document.getElementById("selectedStockItemsStockCode").value = stockCode;
        document.getElementById("selectedStockItemsDescription").value = $(this)[0].childNodes[1].innerText;
      }
    })
    .on('dblclick', 'tr', function () {
      const tableinfo = datatable.page.info();
      if (tableinfo.recordsDisplay === 0) {
        return;
      }

      let stockCode = $(this).find("td:first")[0].innerText;
      if (stockCode !== "No data available in table.") {
        const url = `/StockItemForm?stockCodeValue=${stockCode}&isUpdateProduct=true`;
        safelyNavigateTo(url);
      }
    });

  $('#button').on('click', function () {
    datatable.row('.selected-grid-row').remove().draw(false);
  });

  loadDetails();

  let isActive = datatable.column('#DTIsActive');
  isActive.visible(false);

  let isKit = datatable.column('#DTIsKit');
  isKit.visible(false);
}

function filterData() {
  const radioButtons = document.querySelectorAll('input[name="selectCriteria"]');

  for (const radioButton of radioButtons) {
    if (radioButton.checked) {
      let id = dictionary[radioButton.value];
      let searchText;
      if (id) {
        searchText = document.getElementById(id).value;
        localStorage.setItem(id, searchText);
        localStorage.setItem("selectedCriteria", selectedCriteria);
        if (searchText === "All") searchText = "";
      }

      switch (radioButton.value) {
        case "vendorPartSearchText":
          searchText = "%";
          break;
        case "saleItemsOnly":
          searchText = "saleItemsOnly=S";
          break;
        case "scalableItemsOnly":
          searchText = "scalableItemsOnly=1"
          break;
        case "taxExemptItemsOnly":
          searchText = "taxExemptItemsOnly=%"
          break;
        case "inactiveItemsOnly":
          searchText = "inactiveItemsOnly=0"
          break;
        case "departments":
          let departmentText = "";
          let subDepartmentText = "";
          let subDetailText = "";

          departmentText = document.getElementById("selectDepartment").value
          departmentText = departmentText.split(" ")[0];

          subDepartmentText = document.getElementById("selectSubDepartment").value
          subDepartmentText = subDepartmentText.split(" ")[0];

          subDetailText = document.getElementById("selectSubDetail").value
          subDetailText = subDetailText.split(" ")[0];
          localStorage.setItem("selectSubDepartment", null);
          localStorage.setItem("selectSubDetail", null);

          if (departmentText === "All") departmentText = "";
          if (subDepartmentText === "All") subDepartmentText = "";
          if (subDetailText === "All") subDetailText = "";

          if (departmentText !== "") {
            searchText = "selectDepartment=" + departmentText;
            localStorage.setItem("selectDepartment", departmentText);
          }
          if (subDepartmentText !== "") {
            searchText += "&selectSubDepartment=" + subDepartmentText;
            localStorage.setItem("selectSubDepartment", subDepartmentText);
          }
          if (subDetailText !== "") {
            searchText += "&selectSubDetail=" + subDetailText;
            localStorage.setItem("selectSubDetail", subDetailText);
          }
          break;
        default:
          searchText = id + "=" + encodeURIComponent(searchText);
      }
      searchText += "&showDetails=" + isDetailsClicked;
      searchText += "&vendorPartSelected=" + isVendorPartSelected;
      localStorage.setItem("searchText", searchText);
      createDatatable("?" + searchText);
    }
  }

  makeButtonInactive();
  makeButtonMakeItKit();
}

function loadDetails() {
  let costColumn = datatable.column('#DTCost');
  costColumn.visible(isDetailsClicked);
  let priceColumn = datatable.column('#DTPrice');
  priceColumn.visible(isDetailsClicked);
  let availableColumn = datatable.column('#DTAvailable');
  availableColumn.visible(isDetailsClicked);

  let vendorColumn = datatable.column('#DTVendor');
  vendorColumn.visible(false);
  let vendorPartColumn = datatable.column('#DTVendorPart');
  vendorPartColumn.visible(false);
  let departmentColumn = datatable.column('#DTDepartment');
  departmentColumn.visible(false);
  let subDepartmentColumn = datatable.column('#DTSubDepartment');
  subDepartmentColumn.visible(false);

  if (isDetailsClicked) {
    //do nothing
  }
  else if (isVendorPartSelected) {
    let vendorColumn = datatable.column('#DTVendor');
    vendorColumn.visible(true);
    let vendorPartColumn = datatable.column('#DTVendorPart');
    vendorPartColumn.visible(true);
  }
  else {
    let familyColumn = datatable.column('#DTDepartment');
    familyColumn.visible(true);
    let brandColumn = datatable.column('#DTSubDepartment');
    brandColumn.visible(true);
  }
}

function toggleDetails() {
  let showDetails = document.getElementById("showDetails").value;
  let hideDetails = document.getElementById("hideDetails").value;
  if (!isDetailsClicked) {
    document.getElementById("detailsButton").innerText = hideDetails;
  }
  else {
    document.getElementById("detailsButton").innerText = showDetails;
  }

  isDetailsClicked = !isDetailsClicked;

  filterData();
  loadDetails();
}

let checkSaleExistsForStockCode = async (data) => {
  const url = "/StockItems/SaleExistsForStockCode";

  return ajaxGetAsync(url, data)
    .catch(toastrError);
}

let deleteProduct = () => {
  let selectedStockItemsStockCode = document.getElementById("selectedStockItemsStockCode").value;

  if (selectedStockItemsStockCode === null || selectedStockItemsStockCode === "") {
    let message = document.getElementById("selectBeforeDeletingMessage").value;
    toastr.warning(message);
    return;
  }

  checkSaleExistsForStockCode({ stockCode: selectedStockItemsStockCode }).then((data) => {
    if (data === true) {
      let title = document.getElementById("warning").value;
      let message = document.getElementById("deleteSaleItemWarning").value;

      showConfirmModal(title, message).then((result) => {
        if (result === true) {
          StockItemDeleteConfirmModalYesButtonClicked();
        }
      });
    }
    else {
      let title = document.getElementById("warning").value;
      let message = document.getElementById("deleteStockItemWarning").value;

      showConfirmModal(title, message).then((result) => {
        if (result === true) {
          StockItemDeleteConfirmModalYesButtonClicked();
        }
      });
    }
  });
}

async function StockItemDeleteConfirmModalYesButtonClicked() {
  let selectedStockItemsStockCode = $("#selectedStockItemsStockCode").val();
  const url = "/StockItems/DeleteProduct";

  await ajaxGetAsync(url, { stockCode: selectedStockItemsStockCode })
    .then(() => {
      initializeSelectedStockItemData();
      filterData();
    }).catch(toastrError);
};

let copyProductByStockCode = () => {
  let copyStockCode = document.getElementById("selectedStockItemsStockCode").value;
  if (copyStockCode === null || copyStockCode === "") {
    let message = document.getElementById("selectItemBeforeCopyAlert").value;
    toastr.warning(message);
    return;
  }

  const url = `/StockItemForm?stockCodeValue=${copyStockCode}&isCopyProduct=true`;
  safelyNavigateTo(url);
}

function reportsClick() {
  if (document.getElementsByClassName("selected-grid-row").length != 0) {
    let selectedRow = $('#stock-items-grid').DataTable().row(".selected-grid-row").data();

    let stockCode = selectedRow["stockCode"] === null ? "" : selectedRow["stockCode"];
    let description = selectedRow["description"] === null ? "" : selectedRow["description"];
    let vendor = selectedRow["vendor"] === null ? "" : selectedRow["vendor"];
    let vendorPart = selectedRow["vendorPart"] === null ? "" : selectedRow["vendorPart"];

    const url = "/stockItemReports?stockCode=" + stockCode + "&description=" + description + "&vendor=" + vendor + "&vendorPart=" + vendorPart;
    safelyNavigateTo(url);
  }
  else {
    let message = document.getElementById("selectItemAlert").value;
    toastr.warning(message);
  }
}

function reFilterData() {
  clearSearchCriteria();
  filterData();
}

async function loadStockItemsFromLocalStorage() {
  let selectedCriteria = localStorage.getItem("selectedCriteria");
  if (selectedCriteria === 'null') {
    selectedCriteria = "stockCode";
    localStorage.setItem("selectedCriteria", selectedCriteria);
    localStorage.setItem("searchText", "stockCodeSearchText=&showDetails=false&vendorPartSelected=false");
  }

  document.getElementById(selectedCriteria).checked = true;
  clearSearchCriteria();

  if (dictionary[selectedCriteria] !== undefined) {
    document.getElementById(dictionary[selectedCriteria]).value = localStorage.getItem(dictionary[selectedCriteria]);

    if (selectedCriteria === 'departments') {
      document.getElementById('selectDepartment').value = localStorage.getItem("selectDepartment");
      await loadSubDepartmentDropdown();
      document.getElementById("selectSubDepartment").value = "All";
      document.getElementById("selectSubDetail").value = "All";

      if (localStorage.getItem("selectSubDepartment") !== 'null') {
        document.getElementById('selectSubDepartment').value = localStorage.getItem("selectSubDepartment");
        await loadSubDetailDropdown();
        document.getElementById("selectSubDetail").value = "All";

        if (localStorage.getItem("selectSubDetail") !== 'null') {
          document.getElementById('selectSubDetail').value = localStorage.getItem("selectSubDetail");
        }
      }
    }
  }

  let searchText = localStorage.getItem("searchText");
  createDatatable("?" + searchText);
}

function debounce(func, timeout = 1000) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => {
      func.apply(this, args);
    }, timeout);
  }
}

const debounceFilterData = debounce(() => filterData());

// Triggers when page length is changed
$(document).on('change', 'select[name="stock-items-grid_length"]', function () {
  selectedPageLength = parseInt($(this).val());
});
