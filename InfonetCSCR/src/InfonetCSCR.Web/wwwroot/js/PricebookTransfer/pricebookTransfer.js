/*:::::::::::::::: Global ::::::::::::::::*/

let errorMessage = "";
let policies = {};
let selectedDataTransferType = STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE;

let selectedRegionIds = [];
let selectedVendorsOnSelectionTab = [];
let selectedRegionsOnSelectionTab = [];
let selectedPriceGroupsOnSelectionTab = [];
let departments = [];
let subDepartments = [];
let subDetails = [];
let selectedDepartments = [];
let selectedSubDepartments = [];
let selectedSubDetails = [];

let dataTableDepartment = null;
let dataTableSubDepartment = null;
let dataTableSubDetail = null;
let dataTableProduct = null;

let products = [];
let listedProducts = [];
let selectedProductsOnSelectionTab = [];
let selectedProductsOnViewTab = [];
let deselectedProductsOnViewTab = [];
let totalProductsOnViewTabCount = 0;
let selectedProductsOnViewTabCount = 0;

let listedStoresOnViewTab = [];
let listedVendorsOnViewTab = [];
let listedRegionsOnViewTab = [];
let listedPriceGroupsOnViewTab = [];
let selectedStoreIdsOnViewTab = [];
let selectedVendorIdsOnViewTab = [];
let selectedRegionIdsOnViewTab = [];
let selectedPriceGroupIdsOnViewTab = [];

let selectedPriceType = STRING_TABLE.R_REGULAR_PRICE;
let selectedPriceOption = STRING_TABLE.PRICE_OPTION.CURRENT;
let dollarOrPercentage = STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR;
let increaseOrDecrease = STRING_TABLE.PRICE_SETUP.INCREASE;
let marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_PRICE;
let regularPrices = [];
let specialPrices = [];
let isRegularPriceChanged = false;
let isPriceChangesApplied = false;
let selectedTransferOption = "A";

let stockPackCostInfoDict = {};
let packingInfoDict = {};

let PR_Units = null;
let effectiveDateFrom = null;
let effectiveDateTo = null;
let doesNotEnd = false;
let vendorId;
let orderNo = 0;

let validationTimeout;

let showAllProducts = false;
let compTableSectionHeader = "";
let searchValue = "";
let searchColumnName = STRING_TABLE.TABLE.SEARCH_COLUMN_NAME.ID;
let selectedStoreIdOnListTab = null;
let listedStoresOnListTab = [];

let isSelectAllProductsChecked = false;
let isSelectAllProductsModeActivated = false;
let noDeptSubDeptOrSubDetailSelected = false;

let pricebookTransferReportPopUpWindow = null;
let isShowVendorProducts = false;
let regularPriceInputRowNumber = 0;
let specialPriceInputRowNumber = 0;
let _selectedVendor = "";


/*:::::::::::::::: General Section Start ::::::::::::::::*/

$(async function () {
  $("#stockViewTabId").hide();

  let fromDataDefintion = (document.getElementById("fromDataDefinition").value === "True");
  if (fromDataDefintion) {
    vendorId = localStorage.getItem('vendorId');
    if (!vendorId) {
      vendorId = STRING_TABLE.DEFAULT.VENDOR_ID;
    }
    document.getElementById("data-transfer-type").selectedIndex = 1;
    selectedDataTransferType = $("#data-transfer-type").val();
    makeViewTabActive();
    const title = `${$("#stocks").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#stocksSelectionTab").val());
    $("#view-tab-link").text($("#stocksViewTab").val());
    $("#list-tab-link").text($("#stocksListTab").val());
  }

  renderElementsForPriceChange();
  await loadPolicies();

  if (!policies.vendorPrice) {
    $("#show-by-vendor").parent().hide();
  }

  if (!_globalIsBackOffice) {
    await loadHotbuttons();
    await loadPromotions().then(() => renderPromotions());
    await loadDepartments().then(() => renderDepartments());
    await loadSubDepartments().then(() => renderSubDepartments());
    await loadSubDetails().then(() => renderSubDetails());
  }

  renderPricesTable();
  renderComparisonTable();

  if (fromDataDefintion) {
    $("#section-header--promotions").hide();
    $("#table-section--promotion").hide();
    $("#promotion-table-section").hide();
    $("#stock-list-table-section").show();
    $("#section-header--commodity").show();
    $("#data-definition-btn").show();
    $("#recent-changes-btn").show();
    await renderElementsForStockChange();

    let showByOption = localStorage.getItem('showByOption');
    $(`input[name="show-by"][value="${showByOption}"]`).prop('checked', true);
    handleShowByOptionChange();

    if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
      loadSelectedStoresFromLocalStorage();
    }
    else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
      loadSelectedVendorsFromLocalStorage();
    }
    else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
      loadSelectedRegionsFromLocalStorage();
    }
    else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
      loadSelectePriceGroupsFromLocalStorage();
    }

    loadSelectedDepartmentsFromLocalStorage();
    loadSelectedSubDepartmentsFromLocalStorage();
    loadSelectedSubDetailFromLocalStorage();
    loadRecentChangesFromLocalStorage();
    loadShowAllProductsFromLocalStorage();
    $('#view-tab-link-wrapper').trigger('shown.bs.tab');
  } else {
    handleDataTransferTypeChange();
  }

  compTableSectionHeader = $("#r-regularPrice").val();
  let today = new Date().toISOString().split('T')[0];
  $("[type='date']").attr("min", today);

  if (!_globalIsBackOffice)
    renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_STORE);
});

async function loadPolicies() {
  const url = `${STRING_TABLE.URL.GET.POLICIES}`;

  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

async function loadStores() {
  const url = STRING_TABLE.URL.GET.STORES_WITH_REGION_AND_PRICE_GROUP;

  await ajaxGetAsync(url)
    .then((stores) => storeOptions = stores)
    .catch(toastrError);
}

async function loadDepartments() {
  const url = STRING_TABLE.URL.GET.DEPARTMENTS;

  await ajaxGetAsync(url)
    .then((_departments) => departments = _departments)
    .catch(toastrError);
}

async function loadSubDepartments() {
  const url = STRING_TABLE.URL.GET.SUB_DEPARTMENTS;

  await ajaxGetAsync(url)
    .then((_subDepartments) => subDepartments = _subDepartments)
    .catch(toastrError);
}

async function loadSubDetails() {
  const url = STRING_TABLE.URL.GET.SUB_DETAILS;

  await ajaxGetAsync(url)
    .then((_subDetails) => subDetails = _subDetails)
    .catch(toastrError);
}

async function loadProducts() {
  let allProducts;
  const storeId = selectedStoreIdsOnViewTab[selectedStoreIdsOnViewTab.length - 1];
  const url = STRING_TABLE.URL.GET.STOCK_ITEMS_BY_DEPTS_SUB_DEPTS_AND_SUBDETAILS;
  const searchParams = {
    storeId: storeId,
    orderNo: orderNo,
    depts: showAllProducts ? [] : selectedDepartments,
    subDepts: showAllProducts ? [] : selectedSubDepartments,
    subDetails: showAllProducts ? [] : selectedSubDetails,
    transferType: "A",
    includeProductWithoutSubDeptOrSubDetail: $("#includeProducts").prop("checked") || false
  };

  await ajaxPostAsync(url, searchParams)
    .then((_products) => allProducts = _products)
    .catch(toastrError);

  return allProducts;
}

async function loadPendingTransferStores() {
  if (_globalIsBackOffice)
    return;

  const url = `${STRING_TABLE.URL.GET.PENDING_TRANSFER_STORES}?dataTransferType=${selectedDataTransferType}`;

  return await ajaxGetAsync(url)
    .then(stores => listedStoresOnListTab = stores)
    .catch(toastrError);
}

function makeSelectionTabActive() {
  // Get the tab button element by ID
  const tabButton = $("#selection-tab-link");

  // Remove the active class from the previously active tab button
  const activeTabButton = $("#tab .nav-link.active");
  activeTabButton.removeClass("active");

  // Add the active class to the newly selected tab button
  tabButton.addClass("active");

  // Get the tab content element and set its active and show class to show the selected tab content
  const tabContent = $("#selection-tab-pane");
  const activeTabContent = $(".tab-content .tab-pane.active");
  activeTabContent.removeClass("active show");
  tabContent.addClass("active show");
}

function makeViewTabActive() {
  const $viewTabLink = $('#view-tab-link');
  const $viewTabLi = $('#view-tab-link-wrapper');

  const $activeTabLink = $('.nav-link.active');
  const $activeTabLi = $activeTabLink.parent();

  $activeTabLink.removeClass('active');
  $activeTabLi.removeClass('active');
  $viewTabLink.addClass('active');
  $viewTabLi.addClass('active');

  $viewTabLink.trigger('click');

  const $viewTabPane = $('#view-tab-pane');
  const $viewTabContent = $viewTabPane.find('.tab-content');

  $('#view-tab-link').on('show.bs.tab', function (event) {
    const href = $(event.target).attr('href');
    $viewTabContent.load(href);
  });

  $('#selection-tab-pane').removeClass('show active');
  $('#view-tab-pane').addClass('show active');
  renderSelectedStoreOrOtherOptions();
}

async function handleDataTransferTypeChange() {
  let isLoggedIn = false;
  await isAuthenticated().then(async (response) => {
    isLoggedIn = response;
  }).catch(toastrError);

  if (!isLoggedIn) {
    return;
  }

  selectedDataTransferType = $("#data-transfer-type").val();

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    const title = `${$("#price-change").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#priceChangeSelectionTab").val());
    $("#view-tab-link").text($("#priceChangeViewTab").val());
    $("#list-tab-link").text($("#priceChangeListTab").val());
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.STOCKS) {
    const title = `${$("#stocks").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#stocksSelectionTab").val());
    $("#view-tab-link").text($("#stocksViewTab").val());
    $("#list-tab-link").text($("#stocksListTab").val());
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    const title = `${$("#cost-change").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#costChangeSelectionTab").val());
    $("#view-tab-link").text($("#costChangeVeiwTab").val());
    $("#list-tab-link").text($("#costChangeListTab").val());
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY) {
    const title = `${$("#commodity").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#commoditySelectionTab").val());
    $("#list-tab-link").text($("#commodityListTab").val());
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS) {
    const title = `${$("#promotions").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#promotionsSelectionTab").val());
    $("#list-tab-link").text($("#promotionsListTab").val());
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.HOT_BUTTONS) {
    const title = `${$("#hotButtons").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#selection-tab-link").text($("#hotButtonsSelectionTab").val());
    $("#list-tab-link").text($("#hotButtonsListTab").val());
  }

  if (selectedDataTransferType === PRICEBOOK_TRANSFER_STRING_TABLE.DATA_TRANSFER_TYPE.LABEL_PRINTING) {
    $("#selection-tab-link").text($("#labelPrintingSelectionTab").val());
    $("#view-tab-link").text($("#labelPrintingViewTab").val());
    $("#list-tab-link").text($("#labelPrintingListTab").val());
  }

  makeSelectionTabActive();

  // Hides View Tab when data transfer type is selected to Commodity or Promotions or Hot Buttons, shows otherwise
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY
    || selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS
    || selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.HOT_BUTTONS) {
    $("#view-tab-link-wrapper").hide();
    $("#save-as-pending-transfer-btn--selection").show();
  }
  else {
    $("#view-tab-link-wrapper").show();
    $("#save-as-pending-transfer-btn--selection").hide();
  }

  await resetAllSections();
  $("#view__select-or-unselect-all-stores").parent().show();

  // Make elements visible that are specific to a type in `if` block and hide those in `else` block
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    $("#price-change-table-section").show();
    renderElementsForPriceChange();
  }
  else {
    $("#price-change-table-section").hide();
    $("#special-pricing-section").hide();
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    $("#cost-change-table-section").show();
    $("#commodity-table-section").hide();
    $("#current-price-or-cost-label").text($("#currentCost").val());
    $("#future-price-or-cost-label").text($("#futureCost").val());
    $("#regular-cost-0__text").text(`0.${'0'.repeat(policies.costDec)}`);
    $("#regular-cost-0__input").val(`0.${'0'.repeat(policies.costDec)}`);
    renderElementsForCostChange();
  }
  else {
    $("#cost-change-table-section").hide();
    $("#current-price-or-cost-label").text($("#currentPrice").val());
    $("#future-price-or-cost-label").text($("#futurePrice").val());
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.LABEL_PRINTING) {
    const title = `${$("#label-printing").val()} ${$("#updates").val()}`;
    $("#list-tab-table-title").text(title);
    $("#label-print-table-section").show();
    $("#label-printing-format").show();
    $("#label-printing-page-orientation").show();
  }
  else {
    $("#label-print-table-section").hide();
    $("#label-printing-format").hide();
    $("#label-printing-page-orientation").hide();
  }

  if (!_globalIsBackOffice) {
    if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.STOCKS) {

      $("#section-header--promotions").hide();
      $("#table-section--promotion").hide();
      $("#promotion-table-section").hide();
      $("#stock-list-table-section").show();
      $("#section-header--commodity").show();
      $("#data-definition-btn").show();
      $("#recent-changes-btn").show();
      await renderElementsForStockChange();
    }
    else {
      $("#stock-list-table-section").hide();
      $("#data-definition-btn").hide();
      $("#recent-changes-btn").hide();
    }

    if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY) {

      isCommodityTypeNow = true;
      $("#commodity-table-section").show();
      $("#cost-change-table-section").hide();
      $(".commodity-element").show();
      $("#section-header--commodity").show();
      $("#section-header--promotions").hide();
      $("#table-section--dept").show();
      $("#table-section--sub-dept").show();
      $("#table-section--sub-detail").show();
      $("#table-section--promotion").hide();
      $("#promotion-table-section").hide();
      renderDepartments();
      renderSubDepartments();
      renderSubDetails();
      initializeTransTypeValues();
    }
    else {
      $("#commodity-table-section").hide();
      $(".commodity-element").hide();
      if (isCommodityTypeNow) {
        renderDepartments();
        renderSubDepartments();
        renderSubDetails();
      }
      isCommodityTypeNow = false;
    }

    if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS) {

      $("#section-header--commodity").hide();
      $("#section-header--promotions").show();
      $("#table-section--dept").hide();
      $("#table-section--sub-dept").hide();
      $("#table-section--sub-detail").hide();
      $("#table-section--promotion").show();
      $("#promotion-table-section").show();
      $("#cost-change-input-table-wrapper").hide();
      $("#cost-wrapper").hide();
    }
    else {
      $("#section-header--commodity").show();
      $("#section-header--promotions").hide();
      $("#table-section--dept").show();
      $("#table-section--sub-dept").show();
      $("#table-section--sub-detail").show();
      $("#table-section--promotion").hide();
      $("#promotion-table-section").hide();
    }

    if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.HOT_BUTTONS) {

      $("#general-commodity-table-section").fadeOut();
      $("#hot-button-section").show();
      $("#hot-buttons-table-section").show();
      initializeHotButtonsTransType();
      renderHotButtons();
    }
    else {
      $("#general-commodity-table-section").fadeIn();
      $("#hot-button-section").hide();
      $("#hot-buttons-table-section").hide();
    }
  }

  await resetDataAndTables(); // TODO: Find the root cause why the table header breaks, and remove this line after fixing the problem
}

async function resetDataAndTables() {
  selectedDepartments = [];
  selectedSubDepartments = [];
  selectedSubDetails = [];

  if (_globalIsBackOffice) {
    subDepartments = [];
    subDetails = [];
    createDepartmentDatatable();
    createSubDepartmentDatatable();
    createSubDetailDatatable();
  }
  else {
    selectedHotButtons = [];
    renderDepartments();
    renderSubDepartments();
    renderSubDetails();
    reRenderTransferTable();
    renderPromotions();
    renderHotButtons();
    await refreshPendingTransferStores();
  }

  if (dataTableProduct != null) {
    dataTableProduct.state.clear();
    dataTableProduct.clear();
    dataTableProduct.destroy();
    dataTableProduct = null;
  }
}

function resetStoreSelection() {
  $('#show-by-store').prop('checked', true).trigger('change');
}

async function resetAllSections() {
  resetStoreSelection();
  await resetDataAndTables();
  resetPriceSetup();

  selectedProductsOnViewTab = [];
  $("#show-all-products-btn").prop("disabled", false);
  $("#vendor-products-btn").prop("disabled", false);
  $("#recent-changes-btn").prop("disabled", false);
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: Selection Tab Start ::::::::::::::::*/

// Triggers after switching to Selection Tab
$('#selection-link-wrapper').on('shown.bs.tab', async function (e) {
  isAuthenticated().then(async (response) => {
    if (response) {
      await triggerSelectionTab();
    }
    else {
      return;
    }
  }).catch(toastrError);
});

async function triggerSelectionTab() {
  clearLocalStorage();
  await resetDataAndTables();
}

let clearLocalStorage = () => {
  showAllProducts = false;
  localStorage.setItem('showAllProducts', JSON.stringify(showAllProducts));

  recentChanges = false;
  localStorage.setItem('recentChanges', JSON.stringify(recentChanges));

  selectedDepartments = [];
  let selectedDepartmentsJson = JSON.stringify(selectedDepartments);
  localStorage.setItem('selectedDepartments', selectedDepartmentsJson);

  clearSubDepartmentAndSubDetails();
}

let clearSubDepartmentAndSubDetails = () => {
  selectedSubDepartments = [];
  let selectedSubDepartmentsJson = JSON.stringify(selectedSubDepartments);
  localStorage.setItem('selectedSubDepartments', selectedSubDepartmentsJson);

  selectedSubDetails = [];
  let selectedSubDetailJson = JSON.stringify(selectedSubDetails);
  localStorage.setItem('selectedSubDetails', selectedSubDetailJson);
}

function renderDepartments() {
  $("#select-or-unselect-all-department").prop("checked", false);

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DEPT_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DEPT_NAME },
    {
      data: null,
      visible: selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY,
      sortable: false,
      render: function (dept) {
        return `<select class="form-select form-select-sm trans-type" id="${getTransTypeSelector(dept.deptId)}-select-trans-type" onchange="handleTransferTypeChange('${dept.deptId}')">
          <option value="A" selected>${$('#add').val()}</option>
          <option value="U">${$('#update').val()}</option>
          <option value="R">${$('#remove').val()}</option>
        </select>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (dept) {
        return `<input class="form-check-input mt-2 select-by-option-dept" type="checkbox" value="${dept.deptId}" 
                       onChange="handleDepartmentSelectionChange('${dept.deptId}')" />`;
      }
    }
  ];
  props.data = departments;

  if (dataTableDepartment != null) {
    dataTableDepartment.state.clear();
    dataTableDepartment.clear();
    dataTableDepartment.destroy();
  }

  dataTableDepartment = createDatatable("table--dept", props);
}

function renderSubDepartments() {
  $("#select-or-unselect-all-sub-department").prop("checked", false);

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DEPT_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.SUB_DEPT_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.SUB_DEPT_NAME },
    {
      data: null,
      visible: selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY,
      sortable: false,
      render: function (subDept) {
        return `<select class="form-select form-select-sm trans-type" id="${getTransTypeSelector(subDept.deptId, subDept.subDeptId)}-select-trans-type" onchange="handleTransferTypeChange('${subDept.deptId}', '${subDept.subDeptId}')">
          <option value="A" selected>${$('#add').val()}</option>
          <option value="U">${$('#update').val()}</option>
          <option value="R">${$('#remove').val()}</option>
        </select>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (subDept) {
        return `<input class="form-check-input mt-2 select-by-option-subdept" type="checkbox"
                       onChange="handleSubDepartmentSelectionChange('${subDept.deptId} ${subDept.subDeptId}')"
                       value="${subDept.subDeptId}" />`;
      }
    }
  ];
  props.data = subDepartments;

  if (dataTableSubDepartment != null) {
    dataTableSubDepartment.state.clear();
    dataTableSubDepartment.clear();
    dataTableSubDepartment.destroy();
  }

  dataTableSubDepartment = createDatatable("table--sub-dept", props);
}

function renderSubDetails() {
  $("#select-or-unselect-all-sub-detail").prop("checked", false);

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DEPT_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.SUB_DEPT_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.SUB_DETAIL_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.SUB_DETAIL_NAME },
    {
      data: null,
      visible: selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY,
      sortable: false,
      render: function (subDetail) {
        return `<select class="form-select form-select-sm trans-type" id="${getTransTypeSelector(subDetail.deptId, subDetail.subDeptId, subDetail.subDetailId)}-select-trans-type" onchange="handleTransferTypeChange('${subDetail.deptId}', '${subDetail.subDeptId}', '${subDetail.subDetailId}')">
          <option value="A" selected>${$('#add').val()}</option>
          <option value="U">${$('#update').val()}</option>
          <option value="R">${$('#remove').val()}</option>
        </select>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (subDetail) {
        return `<input class="form-check-input mt-2 select-by-option-subdetail" type="checkbox" value="${subDetail.subDetailId}"
                   onChange="handleSubDetailSelectionChange('${subDetail.deptId} ${subDetail.subDeptId} ${subDetail.subDetailId}')" />`;
      }
    }
  ];
  props.data = subDetails;

  if (dataTableSubDetail != null) {
    dataTableSubDetail.state.clear();
    dataTableSubDetail.clear();
    dataTableSubDetail.destroy();
  }

  dataTableSubDetail = createDatatable("table--sub-detail", props);
}

function handleSelectOrUnselectAll() {
  const showByOption = $('input[name="show-by"]:checked').val();
  let isChecked = $("#select-or-unselect-all").is(":checked");
  $(".select-by-option").prop("checked", isChecked);

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE)
    handleSelectedStoresChange();
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR)
    handleSelectedVendorsChange();
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION)
    handleSelectedRegionsChange();
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP)
    handleSelectedPriceGroupsChange();

  renderSelectedStoresOnSelectionTab();
}

function loadSelectedStoresFromLocalStorage() {
  let selectedStoresOnSelectionTabFromLocalStorage = localStorage.getItem('selectedStoresOnSelectionTab');
  let selectedStoresOnSelectionTabArray = JSON.parse(selectedStoresOnSelectionTabFromLocalStorage);
  selectedStoresOnSelectionTab = new Set(selectedStoresOnSelectionTabArray);
  removeDuplicateStores();
  if (selectedStoresOnSelectionTabArray) {
    selectedStoresOnSelectionTabArray.forEach(function (storeObj) {
      let storeID = storeObj.storeID;
      let checkbox = document.getElementById('store-' + storeID);
      if (checkbox) {
        checkbox.checked = true;
        handleSelectedStoresChange();
      }
    });
  }
}

function loadSelectedVendorsFromLocalStorage() {
  let selectedVendorsOnSelectionTabFromLocalStorage = localStorage.getItem('selectedVendorsOnSelectionTab');

  let selectedVendorsOnSelectionTabArray = JSON.parse(selectedVendorsOnSelectionTabFromLocalStorage);
  selectedVendorsOnSelectionTab = new Set(selectedVendorsOnSelectionTabArray);
  if (selectedVendorsOnSelectionTabArray) {
    selectedVendorsOnSelectionTabArray.forEach(function (storeObj) {
      let vendorId = storeObj.code;
      let checkbox = document.getElementById('vendor-' + vendorId);
      if (checkbox) {
        checkbox.checked = true;
        handleSelectedVendorsChange();
      }
    });
  }
}

function loadSelectedRegionsFromLocalStorage() {
  let selectedRegionsOnSelectionTabFromLocalStorage = localStorage.getItem('selectedRegionsOnSelectionTab');

  let selectedRegionsOnSelectionTabArray = JSON.parse(selectedRegionsOnSelectionTabFromLocalStorage);
  selectedRegionsOnSelectionTab = new Set(selectedRegionsOnSelectionTabArray);
  if (selectedRegionsOnSelectionTabArray) {
    selectedRegionsOnSelectionTabArray.forEach(function (storeObj) {
      let regionId = storeObj.id;
      let checkbox = document.getElementById('region-' + regionId);
      if (checkbox) {
        checkbox.checked = true;
        handleSelectedRegionsChange();
      }
    });
  }
}

function loadSelectePriceGroupsFromLocalStorage() {
  let selectedPriceGroupsOnSelectionTabFromLocalStorage = localStorage.getItem('selectedPriceGroupsOnSelectionTab');

  let selectedPriceGroupsOnSelectionTabArray = JSON.parse(selectedPriceGroupsOnSelectionTabFromLocalStorage);
  selectedPriceGroupsOnSelectionTab = new Set(selectedPriceGroupsOnSelectionTabArray);
  if (selectedPriceGroupsOnSelectionTabArray) {
    selectedPriceGroupsOnSelectionTabArray.forEach(function (storeObj) {
      let priceGroupId = storeObj.id;
      let checkbox = document.getElementById('price-group-' + priceGroupId);
      if (checkbox) {
        checkbox.checked = true;
        handleSelectedPriceGroupsChange()
      }
    });
  }
}

function handleSelectedStoresChange() {
  const selectedStoreIds = $('.store-option:checked').map((index, element) => $(element).val()).get();
  selectedStoresOnSelectionTab = new Set(...[storeOptions.filter(store => selectedStoreIds.includes(store.storeID))]);
  let selectedStoresOnSelectionTabArray = Array.from(selectedStoresOnSelectionTab);
  let selectedStoresOnSelectionTabJsonString = JSON.stringify(selectedStoresOnSelectionTabArray, null, 2);

  localStorage.setItem('selectedStoresOnSelectionTab', selectedStoresOnSelectionTabJsonString);
  removeDuplicateStores();
  const isAllOptionSelected = $('.store-option').length === selectedStoresOnSelectionTab.size;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_STORE);
}

async function handleSelectedVendorsChange() {
  const selectedVendorIds = $('.vendor-option:checked').map((index, element) => $(element).val()).get();
  selectedVendorsOnSelectionTab = vendorOptions.filter(vendor => selectedVendorIds.includes(vendor.code));

  if (selectedVendorsOnSelectionTab.length === 0) {
    selectedStoresOnSelectionTab = new Set();
  }
  else {
    const url = STRING_TABLE.URL.GET.STORE_IDS_BY_VENDOR_IDS;
    const storeIds = await ajaxPostAsync(url, selectedVendorIds).then(stores => stores.map(store => store.store_ID));

    selectedStoresOnSelectionTab = new Set([...storeOptions.filter(store => storeIds.includes(store.storeID))
      .map(store => simpleStoreObject(store))]);
  }
  let selectedVendorsOnSelectionTabArray = Array.from(selectedVendorsOnSelectionTab);
  let selectedVendorsOnSelectionTabJsonString = JSON.stringify(selectedVendorsOnSelectionTabArray, null, 2);

  localStorage.setItem('selectedVendorsOnSelectionTab', selectedVendorsOnSelectionTabJsonString);
  const isAllOptionSelected = vendorOptions.length === selectedVendorsOnSelectionTab.length;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR);
}

function handleSelectedRegionsChange() {
  const selectedRegionIds = $('.region-option:checked').map((index, element) => $(element).val()).get();
  selectedRegionsOnSelectionTab = regionOptions.filter(region => selectedRegionIds.includes(region.id));
  let selectedRegionsOnSelectionTabArray = Array.from(selectedRegionsOnSelectionTab);
  let selectedRegionsOnSelectionTabJsonString = JSON.stringify(selectedRegionsOnSelectionTabArray, null, 2);

  localStorage.setItem('selectedRegionsOnSelectionTab', selectedRegionsOnSelectionTabJsonString);

  selectedStoresOnSelectionTab = new Set([...storeOptions.filter(store => selectedRegionIds.includes(store.regionID))
    .map(store => simpleStoreObject(store))]);

  const isAllOptionSelected = regionOptions.length === selectedRegionsOnSelectionTab.length;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_REGION);
}

function handleSelectedPriceGroupsChange() {
  const selectedPriceGroupIds = $('.price-group-option:checked').map((index, element) => $(element).val()).get();
  selectedPriceGroupsOnSelectionTab = priceGroupOptions.filter(priceGroup => selectedPriceGroupIds.includes(priceGroup.id));
  let selectedPriceGroupsOnSelectionTabArray = Array.from(selectedPriceGroupsOnSelectionTab);
  let selectedPriceGroupsOnSelectionTabJsonString = JSON.stringify(selectedPriceGroupsOnSelectionTabArray, null, 2);

  localStorage.setItem('selectedPriceGroupsOnSelectionTab', selectedPriceGroupsOnSelectionTabJsonString);

  selectedStoresOnSelectionTab = new Set([...storeOptions.filter(store => selectedPriceGroupIds.includes(store.priceGroupID))
    .map(store => simpleStoreObject(store))]);

  const isAllOptionSelected = priceGroupOptions.length === selectedPriceGroupsOnSelectionTab.length;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP);
}

function loadSelectedDepartmentsFromLocalStorage() {
  let selectedDepartmentsFromLocalStorage = localStorage.getItem('selectedDepartments');
  let selectedDepartmentsArray = JSON.parse(selectedDepartmentsFromLocalStorage);
  if (selectedDepartmentsArray) {
    selectedDepartmentsArray.forEach(function (department) {
      let deptId = department.deptId;
      handleDepartmentSelectionChange(deptId)
    });
  }
}

function handleDepartmentSelectionChange(deptId) {
  const alreadyExist = selectedDepartments.find(dept => dept.deptId === deptId);

  if (alreadyExist) {
    selectedDepartments = selectedDepartments.filter(dept => dept.deptId !== deptId);
  }
  else {
    selectedDepartments.push(departments.find(dept => dept.deptId === deptId));
  }

  let selectedDepartmentsJson = JSON.stringify(selectedDepartments);
  localStorage.setItem('selectedDepartments', selectedDepartmentsJson);

  const allSelected = (selectedDepartments.length == departments.length);
  $("#select-or-unselect-all-department").prop("checked", allSelected);
}

function handleSelectOrUnselectAllDepartment() {
  let isSelected = $("#select-or-unselect-all-department").is(":checked");
  $("#table--dept").DataTable().rows().every(function () {
    $(this.node()).find(".select-by-option-dept").prop("checked", isSelected);
  });
  selectedDepartments = [];
  if (isSelected) {
    departments.forEach(dept => selectedDepartments.push(dept));
  }
  let selectedDepartmentsJson = JSON.stringify(selectedDepartments);
  localStorage.setItem('selectedDepartments', selectedDepartmentsJson);
}

function loadSelectedSubDepartmentsFromLocalStorage() {
  let selectedSubDepartmentsFromLocalStorage = localStorage.getItem('selectedSubDepartments');
  let selectedSubDepartmentsArray = JSON.parse(selectedSubDepartmentsFromLocalStorage);
  if (selectedSubDepartmentsArray) {
    selectedSubDepartmentsArray.forEach(function (subDepartment) {
      handleSubDepartmentSelectionChange(`${subDepartment.deptId} ${subDepartment.subDeptId}`);
    });
  }
}

function handleSubDepartmentSelectionChange(deptAndSubDeptId) {
  const [deptId, subDeptId] = deptAndSubDeptId.split(" ");
  const alreadyExist = selectedSubDepartments
    .find(subDept => subDept.deptId === deptId && subDept.subDeptId === subDeptId);

  if (alreadyExist) {
    selectedSubDepartments = selectedSubDepartments
      .filter(subDept => subDept.deptId !== deptId || subDept.subDeptId !== subDeptId);
  } else {
    selectedSubDepartments.push(subDepartments
      .find(subDept => subDept.deptId === deptId && subDept.subDeptId === subDeptId));
  }

  const allSelected = (selectedSubDepartments.length == subDepartments.length);
  $("#select-or-unselect-all-sub-department").prop("checked", allSelected);

  let selectedSubDepartmentsJson = JSON.stringify(selectedSubDepartments);
  localStorage.setItem('selectedSubDepartments', selectedSubDepartmentsJson);
}

function handleSelectOrUnselectAllSubDepartment() {
  let isSelected = $("#select-or-unselect-all-sub-department").is(":checked");
  $("#table--sub-dept").DataTable().rows().every(function () {
    $(this.node()).find(".select-by-option-subdept").prop("checked", isSelected);
  });
  selectedSubDepartments = [];
  if (isSelected) {
    subDepartments.forEach(subDept => selectedSubDepartments.push(subDept));
  }

  let selectedSubDepartmentsJson = JSON.stringify(selectedSubDepartments);
  localStorage.setItem('selectedSubDepartments', selectedSubDepartmentsJson);
}

function loadSelectedSubDetailFromLocalStorage() {
  let selectedSubDetailFromLocalStorage = localStorage.getItem('selectedSubDetails');
  let selectedSubDetailArray = JSON.parse(selectedSubDetailFromLocalStorage);
  if (selectedSubDetailArray) {
    selectedSubDetailArray.forEach(function (subDetail) {
      handleSubDetailSelectionChange(`${subDetail.deptId} ${subDetail.subDeptId} ${subDetail.subDetailId}`);
    });
  }
}

function handleSubDetailSelectionChange(deptAndSubDeptAndSubDetailId) {
  const [deptId, subDeptId, subDetailId] = deptAndSubDeptAndSubDetailId.split(" ");
  const alreadyExist = selectedSubDetails
    .find(subDetail => subDetail.deptId === deptId
      && subDetail.subDeptId === subDeptId && subDetail.subDetailId === subDetailId);

  if (alreadyExist) {
    selectedSubDetails = selectedSubDetails
      .filter(subDetail => subDetail.deptId !== deptId
        || subDetail.subDeptId !== subDeptId || subDetail.subDetailId !== subDetailId);
  } else {
    selectedSubDetails.push(subDetails
      .find(subDetail => subDetail.deptId === deptId
        && subDetail.subDeptId === subDeptId && subDetail.subDetailId === subDetailId));
  }

  const allSelected = (selectedSubDetails.length == subDetails.length);
  $("#select-or-unselect-all-sub-detail").prop("checked", allSelected);

  let selectedSubDetailJson = JSON.stringify(selectedSubDetails);
  localStorage.setItem('selectedSubDetails', selectedSubDetailJson);
}

function handleSelectOrUnselectAllSubDetail() {
  let isSelected = $("#select-or-unselect-all-sub-detail").is(":checked");
  $("#table--sub-detail").DataTable().rows().every(function () {
    $(this.node()).find(".select-by-option-subdetail").prop("checked", isSelected);
  });
  $(".select-by-option-subdetail").prop("checked", isSelected);
  selectedSubDetails = [];
  if (isSelected) {
    subDetails.forEach(subDetail => selectedSubDetails.push(subDetail));
  }
  let selectedSubDetailJson = JSON.stringify(selectedSubDetails);
  localStorage.setItem('selectedSubDetails', selectedSubDetailJson);
}

let loadSelectedProductsFromLocalStorage = () => {
  let selectedProductsFromLocalStorage = localStorage.getItem('selectedProductsOnViewTab');
  if (selectedProductsFromLocalStorage === null) return;
  selectedProductsOnViewTab = JSON.parse(selectedProductsFromLocalStorage);
}

let hasElements = (collection) => {
  if (Array.isArray(collection)) {
    return collection.length > 0;
  } else if (collection instanceof Set) {
    return collection.size > 0;
  } else {
    return false;
  }
}

function isAnyStoreOrOtherOptionSelected() {
  const showByOption = $('input[name="show-by"]:checked').val();

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE)
    return hasElements(selectedStoresOnSelectionTab);
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR)
    return hasElements(selectedVendorsOnSelectionTab);
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION)
    return hasElements(selectedRegionsOnSelectionTab);
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP)
    return hasElements(selectedPriceGroupsOnSelectionTab);
}

function handleTransferTypeChange(deptId, subDeptId = null, subDetailId = null) {
  let selector = getTransTypeSelector(deptId, subDeptId, subDetailId);
  let selectedTransType = $(`#${selector}-select-trans-type`).val();
  commodityTransTypes[selector] = selectedTransType;
}

function handleSaveAsPendingTransferOnSelectionTab() {
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY) {
    handleCommoditySaveAsPendingTransfer();
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS) {
    handlePromotionSaveAsPendingTransfer();
  }
}

/*:::::::::::::::: Selection Tab End ::::::::::::::::*/


/*:::::::::::::::: View Tab Start ::::::::::::::::*/

// Triggers before switching to View Tab
$('#view-tab-link-wrapper').on('show.bs.tab', async function (e) {
  if (_globalIsBackOffice) return;

  searchValue = "";
  if (isAnyStoreOrOtherOptionSelected()) return;
  e.preventDefault();
  const message = $("#selectStoreRequest").val();
  toastr.warning(message);
});

function removeURLParameter(url, parameter) {
  let urlParts = url.split('?');
  if (urlParts.length >= 2) {
    let prefix = encodeURIComponent(parameter) + '=';
    let params = urlParts[1].split(/[&;]/g);

    for (let i = params.length - 1; i >= 0; i--) {
      if (params[i].lastIndexOf(prefix, 0) !== -1) {
        params.splice(i, 1);
      }
    }

    return urlParts[0] + (params.length > 0 ? '?' + params.join('&') : '');
  } else {
    return url;
  }
}

// Triggers after switching to View Tab
$('#view-tab-link-wrapper').on('shown.bs.tab', async function (e) {
  isAuthenticated().then(response => {
    if (response) {
      triggerViewTab();
    }
    else {
      return;
    }
  }).catch(toastrError);
});

async function triggerViewTab() {
  showAllProducts = false;
  selectedProductsOnViewTabCount = 0;
  let fromDataDefintion = (document.getElementById("fromDataDefinition").value === "True");
  let newURL = removeURLParameter(window.location.href, "fromDataDefinition");
  window.history.replaceState(null, null, newURL);
  if (!_globalIsBackOffice)
    vendorId = STRING_TABLE.DEFAULT.VENDOR_ID;

  if (fromDataDefintion) {
    loadShowAllProductsFromLocalStorage();
    loadSelectedProductsFromLocalStorage();
    localStorage.removeItem('showAllProducts');
    localStorage.removeItem('selectedProductsOnViewTab');
    document.getElementById("fromDataDefinition").value = "False"
    vendorId = localStorage.getItem('vendorId');
    if (!vendorId) {
      vendorId = STRING_TABLE.DEFAULT.VENDOR_ID;
    }
  }

  localStorage.setItem('showAllProducts', JSON.stringify(showAllProducts));
  noDeptSubDeptOrSubDetailSelected = selectedDepartments.length === 0 && selectedSubDepartments.length === 0 && selectedSubDetails.length === 0;
  renderSelectedStoreOrOtherOptions();
  await renderProducts();
  enableOrDisableButtons();
  $("#show-all-products-btn").prop("disabled", false);
  $("#recent-changes-btn").prop("disabled", false);
  $("#vendor-products-btn").prop("disabled", false);
}

$("#regular-cost-0__input").on('keypress', function (event) {
  return onlyDigitAndDecimalKeyPress(event);
});

function validateDateSelection() {
  if (selectedDataTransferType !== STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE &&
    selectedDataTransferType !== STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE)
    return true;

  const isFromElementVisibleInUI = $('.effective-date--from').is(':visible');
  const isToElementVisibleInUI = $('.effective-date--to').is(':visible');

  if (isFromElementVisibleInUI === false) return true;

  const fromDate = $("#effective-date--from__control").val();
  const toDate = $("#effective-date--to__control").val();
  const datePassedMessage = $("#youCantChangeToAPassedDate").val();
  const toDateShouldBeGreaterMessage = $("#toDateShouldBeAlwaysGreaterThanFromDate").val();

  if (isDateInPast(new Date(replaceDashWithSlash(fromDate)))) {
    toastr.warning(datePassedMessage);
    return false;
  }

  // If does not end is checked in price change
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE &&
    selectedPriceOption === STRING_TABLE.PRICE_OPTION.FUTURE &&
    selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE && doesNotEnd)
    return true;

  // If does not end is checked in cost change
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE &&
    selectedPriceOption === STRING_TABLE.PRICE_OPTION.FUTURE && doesNotEnd)
    return true;

  if (isToElementVisibleInUI === false) return true;

  if (toDate < fromDate) {
    toastr.warning(toDateShouldBeGreaterMessage);
    return false;
  }

  return true;
}

$('#effective-date--from__control, #effective-date--to__control').on("blur", function () {
  clearTimeout(validationTimeout);
  validationTimeout = setTimeout(validateDateSelection, 500);
});

async function getStockCostInfo(storeId, stockCode, orderNo) {
  let cost = 0;
  let vendorId = "ALL";
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.STORE_ID, storeId);
  params.append(STRING_TABLE.PARAMS.STOCK_CODE, encodeURIComponent(stockCode));
  params.append(STRING_TABLE.PARAMS.ORDER_NO, orderNo);

  let urlWithParams = `${STRING_TABLE.URL.GET.STOCK_COST_INFO}?${params.toString()}`;
  let stockCostInfo = await ajaxGetAsync(urlWithParams);
  cost = stockCostInfo?.cost ?? 0;
  vendorId = stockCostInfo?.vendorId ?? vendorId;
  return { cost, vendorId };
}

async function getStockCostInfoBatch(stockCostInfoList) {
  let url = `/StockItems/GetStockCostInfoBatch?selectedVendor=${_selectedVendor}`;
  let stockCostInfos = await ajaxPostAsync(url, stockCostInfoList);

  for (let i = 0; i < stockCostInfos.length; i++) {
    stockCostInfos[i].cost = stockCostInfos[i]?.cost ?? 0;
    stockCostInfos[i].vendorId = stockCostInfos[i]?.vendorId ?? "ALL";
  }
  return stockCostInfos;
}

async function getPackingInfo(stockCode) {
  let packing = 1;
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.STOCK_CODE, encodeURIComponent(stockCode));

  let urlWithParams = `${STRING_TABLE.URL.GET.PACKING_INFO}?${params.toString()}`;
  let response = await ajaxGetAsync(urlWithParams);
  packing = response?.packing;
  packingInfoDict[stockCode] = packing;
  return packing;
}

async function getPackingInfoBatch(stockCodes) {
  let url = "/StockItems/GetPackingInfoBatch";
  let packingInfos = await ajaxPostAsync(url, stockCodes);

  for (let i = 0; i < packingInfos.length; i++) {
    packingInfos[i].packing = packingInfos[i]?.packing ?? 1;
    packingInfoDict[stockCodes[i]] = packingInfos[i].packing;
  }
  return packingInfos;
}

async function getStockPriceInfo(storeId, stockCode, vendorId, priceNumber, orderNo) {
  let price = 0;
  let margin = 0;
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.STORE_ID, storeId);
  params.append(STRING_TABLE.PARAMS.STOCK_CODE, encodeURIComponent(stockCode));
  params.append(STRING_TABLE.PARAMS.VENDOR_ID, vendorId);
  params.append(STRING_TABLE.PARAMS.PRICE_NUMBER, priceNumber);
  params.append(STRING_TABLE.PARAMS.ORDER_NO, orderNo);

  let urlWithParams = `${STRING_TABLE.URL.GET.STOCK_PRICE_INFO}?${params.toString()}`;
  let stockPriceInfo = await ajaxGetAsync(urlWithParams);
  price = stockPriceInfo?.price;
  margin = stockPriceInfo?.margin;
  return { price, margin };
}

async function getStockPriceInfoBatch(stockPriceInfoList) {
  let url = "/StockPrice/GetStockPriceInfoBatch";
  let stockPriceInfos = await ajaxPostAsync(url, stockPriceInfoList);

  for (let i = 0; i < stockPriceInfos.length; i++) {
    stockPriceInfos[i].price = stockPriceInfos[i]?.price ?? 0;
    stockPriceInfos[i].margin = stockPriceInfos[i]?.margin ?? 0;
  }
  return stockPriceInfos;
}

function enableOrDisableButtons() {
  $("#show-all-products-btn").prop("disabled", false);
  $("#vendor-products-btn").prop("disabled", false);
  $("#view__select-or-unselect-all-stores").prop("disabled", selectedStoresOnSelectionTab.size === 1);
  $("#view__select-or-unselect-all-products").prop("checked", false);
  isSelectAllProductsChecked = false;
  isSelectAllProductsModeActivated = false;
}

function showOrHideProductInformation(stockCode, show) {
  $(`#product-${stockCode}__cost`).css("display", show ? "flex" : "none");
  $(`#product-${stockCode}__packCost`).css("display", show ? "flex" : "none");
  $(`#product-${stockCode}__n-cost`).css("display", show ? "flex" : "none");
  $(`#product-${stockCode}__reg-price`).css("display", show ? "flex" : "none");
  $(`#product-${stockCode}__margin`).css("display", show ? "flex" : "none");

  if (show === false)
    $(`#product-${stockCode}__n-price`).text("");
}

function displayProductInfomation(stockCode, cost, price, margin, packCost = cost, encodedStockCodeAndRowNumber) {
  $(`#product-${encodedStockCodeAndRowNumber}__packCost`).text(packCost.toFixed(policies.costDec));
  $(`#product-${encodedStockCodeAndRowNumber}__cost`).text(cost.toFixed(policies.costDec));
  $(`#product-${encodedStockCodeAndRowNumber}__reg-price`).text(price.toFixed(2));
  $(`#product-${encodedStockCodeAndRowNumber}__margin`).text(margin.toFixed(2));

  stockPackCostInfoDict[stockCode] = packCost;

  const isPriceInputChanged = regularPrices.filter(price => price != 0).length;
  if (isPriceInputChanged) {
    calculatePriceAndMarginAndUpdateRespectiveColumn(encodedStockCodeAndRowNumber);
  }

  showOrHideProductInformation(encodedStockCodeAndRowNumber, true);
}

function showOrHideProductInformationAll(showAll) {
  products.map(({ stockCode }) => {
    let currentProduct = products.find(p => p.stockCode === stockCode);
    let rowId = currentProduct.rowId;
    let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, rowId);
    calculatePriceAndMarginAndUpdateRespectiveColumn(encodedStockCodeAndRowNumber);
    showOrHideProductInformation(encodedStockCodeAndRowNumber, showAll);
  })
}

function showOrHideSpecialPriceTableColumn() {
  selectedPriceType = $("#price-type").val();
  const firstUnitPricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.F_FIRST_UNIT_PRICING;
  const incrementalPricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.I_INCREMENTAL_PRICING;
  const xForPricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.X_X_FOR_PRICING;

  $(".special-price--from").css("display", incrementalPricing || firstUnitPricing || xForPricing ? "table-cell" : "none");
  $(".special-price--to").css("display", incrementalPricing ? "table-cell" : "none");
}

function addBlankSpecialPriceTableRow() {
  const rowNumber = specialPrices.length;
  const specialPricesTableBody = $(".table--special-prices__body");
  specialPrices.push({
    from: 0,
    to: 0,
    price: 0,
    removed: false,
  });

  specialPricesTableBody.append(`
      <tr class="editable" id="special-price-table-row-${rowNumber}">
        <td class="special-price--from" onclick="handleSpecialPriceTableCellEdit(${rowNumber}, 'from')">
          <span id="special-price-${rowNumber}__from__text">0.00</span>
          <input class="form-control"
                 id="special-price-${rowNumber}__from__input" type="number" 
                 value="0.00" min="0.00" style="display: none; width: 100px">
        </td>
        <td class="special-price--to" onclick="handleSpecialPriceTableCellEdit(${rowNumber}, 'to')">
          <span id="special-price-${rowNumber}__to__text">0.00</span>
          <input class="form-control"
                 id="special-price-${rowNumber}__to__input" type="number" 
                 value="0.00" min="0.00" style="display: none; width: 100px">
        </td>
        <td class="special-price--price" onclick="handleSpecialPriceTableCellEdit(${rowNumber}, 'price')">
          <span id="special-price-${rowNumber}__price__text">0.00</span>
          <input class="form-control"
                 id="special-price-${rowNumber}__price__input" type="number" 
                 value="0.00" min="0.00" style="display: none; width: 100px">
        </td>
        <td>
          <button class="btn btn-sm btn-outline-danger remove-special-price-btn comp-hide" onclick="removeSpecialPriceTableRow(${rowNumber})">-</button>
        </td>
      </tr>
  `)

  showOrHideSpecialPriceTableColumn();

  let activeRows = specialPrices.filter(price => price.removed === false);
  $(".remove-special-price-btn").prop("disabled", activeRows.length === 1);
}

function clearPricesTable() {
  isPriceChangesApplied = false;
  renderPricesTable();
}

function getProductsUrl() {
  const url = vendorId === STRING_TABLE.DEFAULT.VENDOR_ID ?
    STRING_TABLE.URL.GET.STOCK_ITEMS_BY_DEPTS_SUB_DEPTS_AND_SUBDETAILS :
    STRING_TABLE.URL.GET.STOCK_ITEMS_BY_VENDOR;

  return url;
}

function removeSpecialPriceTableRow(rowNumber) {
  $(`#special-price-table-row-${rowNumber}`).hide();
  specialPrices[rowNumber].removed = true;

  let activeRows = specialPrices.filter(price => price.removed === false);
  $(".remove-special-price-btn").prop("disabled", activeRows.length === 1);
}

function renderSelectedStoreOrOtherOptions() {
  const showByOption = $('input[name="show-by"]:checked').val();
  $("#view__selected-store-or-other-options").empty();

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
    renderSelectedStores();
    $("#view__selected-store-or-other-options-label").text($("#selectedStores").val());
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    renderSelectedVendors();
    $("#view__selected-store-or-other-options-label").text($("#selectedVendors").val());
    $("#view__select-or-unselect-all-stores").prop("checked", false);
    $("#view__select-or-unselect-all-stores").parent().hide();
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
    renderSelectedRegions();
    $("#view__selected-store-or-other-options-label").text($("#selectedRegions").val());
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
    renderSelectedPriceGroups();
    $("#view__selected-store-or-other-options-label").text($("#selectedPriceGroups").val());
  }
}

function renderSelectedStores() {
  listedStoresOnViewTab = [...selectedStoresOnSelectionTab];
  selectedStoreIdsOnViewTab = [...listedStoresOnViewTab.map(store => store.storeID)];

  const container = $("#view__selected-store-or-other-options");
  container.empty();

  listedStoresOnViewTab.forEach(store => {
    container.append(`
       <option value="${store.storeID}" ${container.children().length === 0 ? ' selected' : ''}>
          ${store.storeID} -- ${store.storeName}
       </option>
    `)
  });
}

function renderSelectedVendors() {
  listedVendorsOnViewTab = [...selectedVendorsOnSelectionTab];
  selectedVendorIdsOnViewTab = [...listedVendorsOnViewTab.map(vendor => vendor.code)];
  selectedStoreIdsOnViewTab = [...selectedStoresOnSelectionTab].map(store => store.storeID);

  const container = $("#view__selected-store-or-other-options");
  container.empty();

  listedVendorsOnViewTab.forEach(vendor => {
    container.append(`
       <option value="${vendor.code}" ${container.children().length === 0 ? ' selected' : ''}>
          ${vendor.code} -- ${vendor.name}
       </option>
    `)
  });
}

function renderSelectedRegions() {
  listedRegionsOnViewTab = [...selectedRegionsOnSelectionTab];
  selectedRegionIdsOnViewTab = [...listedRegionsOnViewTab.map(region => region.id)];
  selectedStoreIdsOnViewTab = [...selectedStoresOnSelectionTab].map(store => store.storeID);

  const container = $("#view__selected-store-or-other-options");
  container.empty();

  listedRegionsOnViewTab.forEach(region => {
    container.append(`
       <option value="${region.id}" ${container.children().length === 0 ? ' selected' : ''}>
          ${region.id} -- ${region.name}
       </option>
    `)
  });
}

function renderSelectedPriceGroups() {
  listedPriceGroupsOnViewTab = [...selectedPriceGroupsOnSelectionTab];
  selectedPriceGroupIdsOnViewTab = [...listedPriceGroupsOnViewTab.map(priceGroup => priceGroup.id)];
  selectedStoreIdsOnViewTab = [...selectedStoresOnSelectionTab].map(store => store.storeID);

  const container = $("#view__selected-store-or-other-options");
  container.empty();

  listedPriceGroupsOnViewTab.forEach(priceGroup => {
    container.append(`
       <option value="${priceGroup.id}" ${container.children().length === 0 ? ' selected' : ''}>
          ${priceGroup.id} -- ${priceGroup.name}
       </option>
    `)
  });
}

function renderPricesTable() {
  $(".table--special-prices__body").empty();
  specialPrices = [];
  addBlankSpecialPriceTableRow();

  const regularPricesTableBody = $("#table--prices__body");
  regularPricesTableBody.empty();
  regularPrices = [];


  for (let i = 0; i < policies.numberOfPrice; ++i) {
    regularPrices.push({ price: 0.00 });
  }
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    for (let rowNumber = 0; rowNumber < policies.numberOfPrice; rowNumber++) {
      regularPricesTableBody.append(`
      <tr class="editable" id="regular-price-table-row-${rowNumber}" onclick="handleRegularPriceTableCellEdit(${rowNumber})">
        <td width="30%">${rowNumber + 1}</td>
        <td width="70%">
          <span class="regular-price__text" id="regular-price-${rowNumber}__text">0.00</span>
          <input class="form-control regular-price__input"
                 id="regular-price-${rowNumber}__input" type="number" 
                 value="0.00" min="0.00" style="display: none; width: 100px;
                 onkeypress="handlePriceInputKeyPressAndFocusout(event, ${rowNumber})" onfocusout="handlePriceInputKeyPressAndFocusout(event, ${rowNumber})">
        </td>
      </tr>
    `)
    }
  }

  $(".date-field").val(getCurrentDate());
  $(".remove-special-price-btn").prop("disabled", true);
}

let changedTrType = (tableRow, trType) => {
  let currentStockCode = tableRow.querySelector('td:nth-child(1)').textContent.trim();
  const filteredProducts = products.filter(product => product.stockCode === currentStockCode);
  filteredProducts.forEach(product => {
    product.transferType = trType;
  });
}

function loadShowAllProductsFromLocalStorage() {
  let showAllProductsFromLocalStorage = localStorage.getItem('showAllProducts');
  showAllProducts = JSON.parse(showAllProductsFromLocalStorage);
  if (!showAllProducts) {
    showAllProducts = false;
  }
}

let encodeStockCodeWithRowNumber = (stockCode, rowNumber) => {
  return `${encodeURIComponent(stockCode.replace(/\s/g, ''))}-${encodeURIComponent(rowNumber)}`;
}

function renderProducts() {
  // if view tab is not the active tab then return
  if ($('a.active').attr('id') != "view-tab-link") return;

  destroyDataTableIfExists(dataTableProduct);

  let columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.STOCK_CODE },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    {
      data: null,
      visible: selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE && policies.packCost,
      render: function (product, _, _, meta) {
        let packCost = (product.packCost ?? 0).toFixed(policies.costDec);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__packCost" style="display: none;">${packCost}</span>`;
      }
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        let cost = (product.cost ?? 0).toFixed(policies.costDec);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__cost" style="display: none;">${cost}</span>`;
      }
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        return `<span class="product__n-${selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE ? 'price' :
          selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE ? 'cost' : ''}" 
                      id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__n-${selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE ? 'price' :
            selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE ? 'cost' : ''}" 
                      data-stock-code="${product.stockCode}"
                      style="display: none;">
                </span>`;
      },
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        let price = (product.price ?? 0).toFixed(2);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__reg-price" style="display: none;">${price}</span>`;
      }
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        let margin = (product.margin ?? 0).toFixed(2);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__margin" style="display: none;">${margin}</span>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (product, _, _, meta) {
        return `<input class="form-check-input mt-2 product" id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__sel"
                  onChange="handleProductSelectionChange('${product.stockCode}',  this.parentElement.parentElement)"
                  type="checkbox" value="${product.stockCode}" />`;
      }
    }
  ];

  if (selectedDataTransferType === "stocks") {
    columns = getColumnsOfProductTableForStockChange();
  }
  else if (selectedDataTransferType === "price-change") {
    columns = getColumnsOfProductTableForPriceChange();
  }

  const lastSelectedStoreId = selectedStoreIdsOnViewTab.length ?
    selectedStoreIdsOnViewTab[selectedStoreIdsOnViewTab.length - 1] : STRING_TABLE.DEFAULT.STORE_ID;

  let props = createDataTableProp();
  props.columns = columns;
  props.serverSide = true;
  props.ajax = {
    url: getProductsUrl(),
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    dataSrc: function (d) {
      //When no commodity is selected and show all products and  recent product is not selected make the product list empty
      if (showAllProducts === false && noDeptSubDeptOrSubDetailSelected && recentChanges === false && !(_globalIsBackOffice && vendorId)) {
        d.iTotalDisplayRecords = 0;
        d.iTotalRecords = 0;
        d.data = [];
      }
      products = d.data;
      if (products) {
        products.forEach(function (product, index) {
          product.rowId = index;
        });
      }
      totalProductsOnViewTabCount = d.iTotalRecords;
      return d.data;
    },
    data: function (d) {
      d.searchValue = searchValue === '' ? null : searchValue;
      d.searchColumnName = searchColumnName;
      d.storeId = lastSelectedStoreId;
      d.vendorId = vendorId;
      d.orderNo = orderNo;
      d.depts = showAllProducts ? [] : selectedDepartments;
      d.subDepts = showAllProducts ? [] : selectedSubDepartments;
      d.subDetails = showAllProducts ? [] : selectedSubDetails;
      d.isDistinct = (selectedDataTransferType === "stocks") ? true : false;
      d.isRecentChanges = recentChanges;
      d.isShowAllProduct = showAllProducts;
      d.dataTransferType = selectedDataTransferType;
      d.includeProductWithoutSubDeptOrSubDetail = $("#includeProducts").prop("checked") || false;

      return JSON.stringify(d);
    }
  };
  props.callOnDrawCallback = function () {
    $('#table--products tbody').on('click', 'td:has(span[data-stock-code])', function () {
      const stockCode = $(this).find('span[data-stock-code]').data('stock-code');
      const isSelected = selectedProductsOnViewTab.find(prod => prod.stockCode == stockCode);
      if (!isSelected) return;
      handleComparisonTable(stockCode);
    });

    if (selectedDataTransferType === "stocks") {
      renderSelectedProductsForStockChangeOnLoad();
    }
    else {
      renderSelectedProductsOnLoad();
    }
  };

  dataTableProduct = createDatatable("table--products", props);

  let isLabelPrinting = selectedDataTransferType === "label-printing";

  $("#store-and-price-setup").prop("hidden", isLabelPrinting);

  if (isLabelPrinting) {
    dataTableProduct.column(2).visible(false);
    dataTableProduct.column(3).visible(false);
    dataTableProduct.column(4).visible(false);
    dataTableProduct.column(6).visible(false);
  }
}

async function renderSelectedProductsOnLoad(selectionChanged = false) {
  let selectedProductsOnCurrentPage;
  if (isSelectAllProductsModeActivated) {
    selectedProductsOnCurrentPage = products.filter(prod => !deselectedProductsOnViewTab.find(p => p.stockCode === prod.stockCode));
  }
  else {
    selectedProductsOnCurrentPage = selectedProductsOnViewTab.filter(prod => products.find(p => p.stockCode === prod.stockCode));
  }

  selectedProductsOnCurrentPage.map((prod) => {
    let currentProduct = products.find(p => p.stockCode === prod.stockCode);
    let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(prod.stockCode, currentProduct.rowId);
    $(`#product-${encodedStockCodeAndRowNumber}__sel`).prop("checked", true);
    if (selectedDataTransferType === "stocks") {
      const filteredProducts = products.filter(product => product.stockCode === stockCode);
      filteredProducts.forEach(product => {
        product.transferType = selectedTransferOption;
      });
    }
    const alreadyExist = selectedProductsOnViewTab.find(p => p.stockCode === prod.stockCode);
    if (!alreadyExist) {
      selectedProductsOnViewTab.push(products.find(p => p.stockCode === prod.stockCode));
    }
    deselectedProductsOnViewTab = deselectedProductsOnViewTab.filter(p => p.stockCode !== prod.stockCode);
  });

  await handleSelectedProducts(selectedProductsOnCurrentPage, selectionChanged);
  localStorage.setItem('selectedProductsOnViewTab', JSON.stringify(selectedProductsOnViewTab))
}

function renderComparisonTable() {
  const currentPriceTableBody = $("#current-price-table__body");
  const regularPriceTableBody = $("#regular-price-table__body");

  for (let priceNumber = 1; priceNumber <= policies.numberOfPrice; priceNumber++) {
    currentPriceTableBody.append(`
      <tr>
        <td>${priceNumber}</td>
        <td id="current-margin-${priceNumber}">0.00</td>
        <td id="current-price-${priceNumber}">0.00</td>
      </tr>
    `);
  }

  for (let priceNumber = 1; priceNumber <= policies.numberOfPrice; priceNumber++) {
    regularPriceTableBody.append(`
      <tr>
        <td>${priceNumber}</td>
        <td id="regular-margin-${priceNumber}">0.00</td>
        <td id="regular-price-${priceNumber}">0.00</td>
      </tr>
    `);
  }
}

function verifyPriceChange() {
  let source = selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE ? regularPrices : specialPrices;
  let isChanged = source.filter(({ price }) => price !== 0).length > 0;
  return isChanged;
}

// Triggers when available store [or other] selection changes
$("#view__selected-store-or-other-options").on("change", function () {
  const showByOption = $('input[name="show-by"]:checked').val();
  const selectedOptions = $(this).val();

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE)
    handleSelectedStoresChangeOnViewTab(selectedOptions);
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR)
    handleSelectedVendorsChangeOnViewTab(selectedOptions);
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION)
    handleSelectedRegionsChangeOnViewTab(selectedOptions);
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP)
    handleSelectedPriceGroupsChangeOnViewTab(selectedOptions);

  renderSelectedProductsOnLoad(true);

  const isAllListedStoresSelected = $("#view__selected-store-or-other-options option").length === selectedOptions.length;
  $("#view__select-or-unselect-all-stores").prop("checked", isAllListedStoresSelected);
});

function handleSelectedStoresChangeOnViewTab(selectedOptions) {
  selectedStoreIdsOnViewTab = selectedOptions;
}

async function handleSelectedVendorsChangeOnViewTab(selectedOptions) {
  selectedVendorIdsOnViewTab = selectedOptions;
  vendorId = selectedVendorIdsOnViewTab[0];
  const url = STRING_TABLE.URL.GET.STORE_IDS_BY_VENDOR_IDS;
  selectedStoreIdsOnViewTab = await ajaxPostAsync(url, selectedOptions)
    .then(stores => stores.map(store => store.store_ID));
}

function handleSelectedRegionsChangeOnViewTab(selectedOptions) {
  selectedRegionIdsOnViewTab = selectedOptions;
  selectedStoreIdsOnViewTab = storeOptions
    .filter(store => selectedRegionIdsOnViewTab.includes(store.regionID))
    .map(store => store.storeID);
}

function handleSelectedPriceGroupsChangeOnViewTab(selectedOptions) {
  selectedPriceGroupIdsOnViewTab = selectedOptions;
  selectedStoreIdsOnViewTab = storeOptions
    .filter(store => selectedPriceGroupIdsOnViewTab.includes(store.priceGroupID))
    .map(store => store.storeID);
}

// Triggers when select or unselect all store [or other] button is clicked
$("#view__select-or-unselect-all-stores").on("click", function () {
  const isChecked = $(this).is(":checked");
  if (isChecked) {
    $("#view__selected-store-or-other-options option").prop("selected", isChecked);
  }
  else {
    $("#view__selected-store-or-other-options option:not(:first)").prop("selected", isChecked);
  }
  $("#view__selected-store-or-other-options").trigger("change");
});

// Triggers when any editable field of regular price is double clicked
function handleRegularPriceTableCellEdit(rowId) {
  let text;
  let input;
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    text = $(`#regular-price-${rowId}__text`);
    input = $(`#regular-price-${rowId}__input`);
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    text = $(`#regular-cost-${rowId}__text`);
    input = $(`#regular-cost-${rowId}__input`);
  }

  input.val(isNullOrWhiteSpace(input.val()) ? 0 : input.val());
  text.hide();
  input.show();
  input.focus();
  input.select();
}

function handlePriceInputKeyPressAndFocusout(event, rowId, column = "") {
  regularPriceInputRowNumber = rowId;
  let text;
  let input;

  if (selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE) {
    if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
      text = $(`#regular-price-${rowId}__text`);
      input = $(`#regular-price-${rowId}__input`);
    }
    else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
      text = $(`#regular-cost-${rowId}__text`);
      input = $(`#regular-cost-${rowId}__input`);
    }
  }
  else {
    text = $(`#special-price-${rowId}__${column}__text`);
    input = $(`#special-price-${rowId}__${column}__input`);
  }

  if (event.key === "e" || event.key === "E") {
    event.preventDefault();
    return;
  }

  if (event.type === "keypress" && event.key !== "Enter") {
    return;
  }

  let inputValue = (parseFloat(isNullOrWhiteSpace(input.val()) ? 0 : input.val())).toFixed(2);
  text.text(inputValue);
  text.show();
  input.hide();


  if (selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE) {
    regularPrices[rowId].price = parseFloat(inputValue);
    isRegularPriceChanged = regularPrices.find(value => value !== 0.00);
  }
  else {
    specialPrices[rowId][column] = parseFloat(inputValue);
  }
}

$("#prices-apply").on('mousedown', function (event) {

  if (selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE) {
    handlePriceInputKeyPressAndFocusout(event, regularPriceInputRowNumber)
  }
  else {
    handlePriceInputKeyPressAndFocusout(event, specialPriceInputRowNumber, "price");
  }

  handleApplyButtonClicked();
});

// Triggers when any editable field of special price is double clicked
function handleSpecialPriceTableCellEdit(rowId, column) {
  specialPriceInputRowNumber = rowId;
  const text = $(`#special-price-${rowId}__${column}__text`);
  const input = $(`#special-price-${rowId}__${column}__input`);

  input.val(isNullOrWhiteSpace(input.val()) ? 0 : input.val());
  text.hide();
  input.show();
  input.focus();
  input.select();

  input.on("keypress focusout", function (event) {
    if (event.key === "e" || event.key === "E") {
      event.preventDefault();
      return;
    }

    if (event.type === "keypress" && event.key !== "Enter") {
      return;
    }
    let inputValue = (parseFloat(isNullOrWhiteSpace(input.val()) ? 0 : input.val())).toFixed(2);
    text.text(inputValue);
    text.show();
    input.hide();

    specialPrices[rowId][column] = parseFloat(inputValue);
  });
};

function handlePriceOptionChange() {
  selectedPriceOption = $("input[name='price-option']:checked").val();
  let beforeHeaderText = "";
  let afterHeaderText = "";

  if (selectedPriceOption === STRING_TABLE.PRICE_OPTION.CURRENT) {
    orderNo = 0;
    $("#select-order-no").hide();
    $(".effective-dates-section").hide();
    $("#does-not-end").hide();
    renderProducts();
    beforeHeaderText = $("#currentPrice").val();
    afterHeaderText = $("#newCurrentPrice").val();
  }
  else {
    orderNo = 1;
    $("#select-order-no").val(1);
    $("#select-order-no").show();
    $(".effective-dates-section").show();
    if (selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE) {
      $("#does-not-end").show();
    }
    handleOrderNoChange();
    beforeHeaderText = $("#futurePrice").val();
    afterHeaderText = $("#newFuturePrice").val();
  }

  $("#comp--before__header").html(beforeHeaderText);
  $("#comp--after__header").html(afterHeaderText);
}

function handleOrderNoChange() {
  orderNo = $("#select-order-no").val();
  renderProducts();
}

function handleEffectiveDatesDoesNotEnd() {
  const isChecked = $("#does-not-end__input").prop('checked');
  $(".effective-date--to").css("display", isChecked ? "none" : "flex");
  doesNotEnd = isChecked;

  if (!isChecked) {
    clearTimeout(validationTimeout);
    validateDateSelection();
  }
}

function handlePriceTypeChange() {
  clearPricesTable();
  if (isBackOffice) {
    $("#currency-percentage").removeAttr("hidden");
    $("#currency-percentage-label").removeAttr("hidden");
  }

  selectedPriceType = $("#price-type").val();
  const regularPrice = selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE;
  const salePricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.S_SALE_PRICING;
  const firstUnitPricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.F_FIRST_UNIT_PRICING;
  const incrementalPricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.I_INCREMENTAL_PRICING;
  const xForPricing = selectedPriceType === STRING_TABLE.PRICE_TYPE.X_X_FOR_PRICING;
  const futurePrice = orderNo > 0;

  $("#increase-or-decrease-wrapper").css("display", regularPrice ? "flex" : "none");
  $("#margin-or-price-wrapper").css("display", ((selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) && regularPrice) ? "flex" : "none");
  if ((selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) && regularPrice) {
    $("#price-change-input-table-wrapper").show();
  }
  else {
    $("#price-change-input-table-wrapper").hide();
  }
  $(".special-price-change-input-table-wrapper").css("display", regularPrice ? "none" : "table");
  $(".special-pricing-section").css("display", regularPrice ? "none" : "flex");
  $("#price-units-label").css("display", regularPrice ? "none" : "flex");
  $("#currency-percentage").prop("disabled", false);
  $("#currency-dollar").prop("disabled", false);
  $(".effective-dates-section").css("display", !regularPrice || futurePrice ? "flex" : "none");
  $("#add-blank-special-price-table-row-btn").css("display", salePricing ? "none" : "flex");
  $(".remove-special-price-btn").css("display", salePricing ? "none" : "flex");

  showOrHideSpecialPriceTableColumn();

  compTableSectionHeader = "";

  if (regularPrice) {
    compTableSectionHeader = $("#r-regularPrice").val();
    if (futurePrice) {
      $("#does-not-end").show();
    }
  }
  else {
    doesNotEnd = false;
    $("#does-not-end").hide();
    $(".effective-date--to").show();
  }

  if (salePricing) {
    compTableSectionHeader = $("#s-salePricing").val();
  }
  if (firstUnitPricing) {
    const quantityText = $("#quantity").val();
    $(".special-price--from__header").text(quantityText);

    compTableSectionHeader = $("#f-firstUnitPricing").val();
  }
  if (incrementalPricing) {
    const quantityText = $("#from").val();
    $(".special-price--from__header").text(quantityText);

    compTableSectionHeader = $("#i-incrementalPricing").val();
  }
  if (xForPricing) {
    if (isBackOffice) {
      $("#currency-percentage").attr("hidden", true);
      $("#currency-percentage-label").attr("hidden", true);
    }
    else {
      $("#currency-percentage").prop("disabled", false)
    }

    $("#currency-dollar").prop("disabled", false).prop("checked", true);
    dollarOrPercentage = STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR;
    const priceText = $("#price").val();
    $(".special-price--price__header").text(priceText);
    const quantityText = $("#quantity").val();
    $(".special-price--from__header").text(quantityText);

    compTableSectionHeader = $("#x-xForPricing").val();
  }
}

// Triggers when dollar or percentage radio button toggles
$("input[name='dollar-or-percentage']").on('change', function () {
  const selectedOption = $("input[name='dollar-or-percentage']:checked").attr("id");
  const priceText = $("#price").val();
  const percentText = $("#percent").val();

  if (isCostSelected) {
    $('#increase, #decrease').prop('disabled', false);
    $("#change-by-cost-label").removeClass("selected");
    $("#change-by-cost-label").css("background-color", "white");
    $("#change-by-cost-label").css("color", "#0d6efd");
    isCostSelected = false;
  }

  if (increaseOrDecrease === null) {
    const increasedByText = $("#increasedBy").val();
    $("#increase").prop("checked", true);
    increaseOrDecrease = STRING_TABLE.PRICE_SETUP.INCREASE;
    $("#regular-price__input-column__header").html(increasedByText);
  }

  if (selectedOption === STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR) {
    dollarOrPercentage = STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR;
    PR_Units = '$';
    $(".special-price--price__header").text(priceText);
  }
  else if (selectedOption === STRING_TABLE.PRICE_SETUP.CURRENCY_PERCENTAGE) {
    dollarOrPercentage = STRING_TABLE.PRICE_SETUP.CURRENCY_PERCENTAGE;
    PR_Units = '%';
    $(".special-price--price__header").text(percentText);
  }
  else dollarOrPercentage = null;
  $("input[name='increase-or-decrease']").trigger("change");
})

// Triggers when increase or decrease radio button toggles
$("input[name='increase-or-decrease']").on('change', function () {
  const selectedOption = $("input[name='increase-or-decrease']:checked").attr("id");
  const increasedByText = $("#increasedBy").val();
  const decreasedByText = $("#decreasedBy").val();

  if (selectedOption === STRING_TABLE.PRICE_SETUP.INCREASE) {
    increaseOrDecrease = STRING_TABLE.PRICE_SETUP.INCREASE;
    $("#regular-price__input-column__header").html(increasedByText);
    $("#regular-cost__input-column__header").html(increasedByText);
  }
  else if (selectedOption === STRING_TABLE.PRICE_SETUP.DECREASE) {
    increaseOrDecrease = STRING_TABLE.PRICE_SETUP.DECREASE;
    $("#regular-price__input-column__header").html(decreasedByText);
    $("#regular-cost__input-column__header").html(decreasedByText);
  }
  else increaseOrDecrease = null;
})

$('#change-by-cost-label').on('click', function () {
  $("#regular-cost__input-column__header").html($("#packageCost").val());
});

// Triggers when margin or price radio button toggles
$("input[name='margin-or-price']").on('change', function () {
  const selectedOptionId = $("input[name='margin-or-price']:checked").attr("id");
  $('#currency-dollar, #currency-percentage').prop('checked', false);
  dollarOrPercentage = null;
  $('#increase, #decrease').prop('checked', false);
  increaseOrDecrease = null;
  $("#currency-dollar").prop("disabled", false);
  $("#currency-percentage").prop("disabled", false);

  if (selectedOptionId === STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN) {
    marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN;
    dollarOrPercentage = STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR;
    $("#currency-dollar").prop("disabled", true);
    $("#currency-percentage").prop("disabled", true);

    const marginText = $("#margin-p").val();
    $("#regular-price__input-column__header").text(marginText);
  }
  else if (selectedOptionId === STRING_TABLE.PRICE_SETUP.CHANGE_BY_PRICE) {
    marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_PRICE;

    const pricesText = $("#prices").val();
    $("#regular-price__input-column__header").text(pricesText);
  }
  else marginOrPrice = null;
})

function handleSpecialPricingChange() {
  PR_Units = dollarOrPercentage ? '$' : '%';
  effectiveDateFrom = $("#effective-date--from__control").val();
  effectiveDateTo = $("#effective-date--to__control").val();
  PR_FROM_QTY = $("#special-price--from__control").val();
  PR_TO_QTY = $("#special-price--to__control").val();
  PR_TO_QTY = $("#special-price--quantity__control").val();
  PR_TO_QTY = $("#special-price--price__control").val();
}

function handleApplyButtonClicked() {
  if (verifyPriceChange()) isPriceChangesApplied = true;
  else handleCancelButtonClick();

  clearTimeout(validationTimeout);
  if (!validateDateSelection()) return;

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    const isPriceInputChange = (selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE) ?
      (regularPrices[0].price !== 0) : (specialPrices[0].price !== 0);

    if (!isPriceInputChange) {
      $(".product__n-price").hide();
      return;
    }
    selectedProductsOnViewTab.forEach(({ stockCode }) => {
      let currentProduct = products.find(p => p.stockCode === stockCode);
      let rowId = currentProduct.rowId;
      let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, rowId);
      calculatePriceAndMarginAndUpdateRespectiveColumn(encodedStockCodeAndRowNumber);
    });

    $(".product__n-price").show();
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    const isCostInputChange = regularPrices[0].price !== 0;

    if (!isCostInputChange) {
      $(".product__n-cost").hide();
      return;
    }

    if (policies.defMargin) {
      if (policies.confirmPc) {
        let title = $("#changePriceBasedOnMarginSetup").val();
        let message = $("#doYouWantToChangeThePrice").val();
        showConfirmModal(title, message).then((result) => {
          if (result) {
            // Change the price based on new cost using the current margin
            marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_PRICE
            changePriceBasedOnCost = true;
          }
          else {
            // Do not change the price, but reclculate the margin based on Cost and price
            marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN
            changePriceBasedOnCost = false;
          }
        });
      }
      else {
        // No need for comnfirmation-Change the price based on new cost using the current margin
        marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_PRICE
        changePriceBasedOnCost = true;
      }
    }
    else {
      // Do not change the price, but recalculate the margin
      marginOrPrice = STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN
      changePriceBasedOnCost = false;
    }
    let increase = false;
    if ($('#increase').is(':checked')) {
      increase = true;
    }
    let selectedOption = $("input[name='dollar-or-percentage']:checked").attr("id");

    selectedProductsOnViewTab.forEach(async ({ stockCode }) => {
      let currentProduct = products.find(p => p.stockCode === stockCode);
      let rowId = currentProduct.rowId;
      let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, rowId);
      await updateNewCostOnDatatable(encodedStockCodeAndRowNumber, packingInfoDict[stockCode], stockPackCostInfoDict[stockCode], increase, selectedOption, encodedStockCodeAndRowNumber);
      showOrHideProductInformation(encodedStockCodeAndRowNumber, true);
    });
  }
}

function handleCancelButtonClick() {
  isPriceChangesApplied = false;
  $("input[name='dollar-or-percentage']").trigger("change");
  $("#regular-cost-0__text").text(`0.${'0'.repeat(policies.costDec)}`);
  $("#regular-cost-0__input").val(`0.${'0'.repeat(policies.costDec)}`);
  $(".product__n-price").hide();
  selectedProductsOnViewTab.forEach(({ stockCode }) => {
    let currentProduct = products.find(p => p.stockCode === stockCode);
    let rowId = currentProduct.rowId;
    let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, rowId);
    $(`#product-${encodedStockCodeAndRowNumber}__n-cost`).text("");
  });
  clearPricesTable();
}

let getStoresByRegionID = async (regionID = null) => {
  const url = "/DataDefinition/GetStoresByRegionId";
  let data = null;

  await ajaxPostAsync(url, [regionID])
    .then((response) => {
      data = response;
    })
    .catch((error) => {
      toastr.error(error);
    });

  return data;
}

let getStoresByGroupID = async (groupID = null) => {
  const url = "/DataDefinition/GetStoresByPriceGroupIds";
  let data = null;

  await ajaxPostAsync(url, [groupID])
    .then((response) => {
      data = response;
    })
    .catch((error) => {
      toastr.error(error);
    });

  return data;
}

let getStoresByVendorID = async (vendorID = null) => {
  const url = "/DataDefinition/GetStoresByVendorIds";
  let data = null;

  await ajaxPostAsync(url, [vendorID])
    .then((response) => {
      data = response;
    })
    .catch((error) => {
      toastr.error(error);
    });

  return data;
}

async function handleProductSelectionChange(stockCode, currentRow, onLoad = false) {
  let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, currentRow.rowIndex - 1);
  const isChecked = $(`#product-${encodedStockCodeAndRowNumber}__sel`).is(":checked");
  if (isChecked) {
    if (selectedDataTransferType === "stocks") {
      const filteredProducts = products.filter(product => product.stockCode === stockCode);
      filteredProducts.forEach(product => {
        product.transferType = selectedTransferOption;
      });

      let dropdown = $(currentRow).find('select.form-select');
      dropdown.val(selectedTransferOption);
      dropdown.trigger('change');
    }

    let storeId = STRING_TABLE.DEFAULT.STORE_ID;

    const showByOption = $('input[name="show-by"]:checked').val();
    if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
      storeId = selectedStoreIdsOnViewTab.length ? selectedStoreIdsOnViewTab[0] : STRING_TABLE.DEFAULT.STORE_ID;
    }
    else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
      if (selectedRegionsOnSelectionTab.length) {
        let storeIds = await getStoresByRegionID(selectedRegionsOnSelectionTab[0].id);
        if (storeIds && storeIds.length) {
          storeId = storeIds[0].store_ID;
        }
      }
    }
    else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
      if (selectedPriceGroupsOnSelectionTab.length) {
        let storeIds = await getStoresByGroupID(selectedPriceGroupsOnSelectionTab[0].id);
        if (storeIds && storeIds.length) {
          storeId = storeIds[0].store_ID;
        }
      }
    }
    else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
      if (selectedVendorsOnSelectionTab.length) {
        let storeIds = await getStoresByVendorID(selectedVendorsOnSelectionTab[0].code);
        if (storeIds && storeIds.length) {
          storeId = storeIds[0].store_ID;
        }
      }
    }

    let cost = 0;
    let packing = 1;
    let vendorId = STRING_TABLE.DEFAULT.VENDOR_ID;
    let price = 0;
    let margin = 0;

    ({ cost, vendorId } = await getStockCostInfo(storeId, stockCode, orderNo));
    if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
      if (selectedVendorsOnSelectionTab.length) {
        vendorId = selectedVendorsOnSelectionTab[0].code;
      }
    }

    if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
      ({ price, margin } = await getStockPriceInfo(storeId, stockCode, vendorId, 1, orderNo));
    }
    else {
      ({ price, margin } = await getStockPriceInfo(storeId, stockCode, vendorId, 1, 0));
    }

    if (policies.packCost) {
      packing = await getPackingInfo(stockCode);
      packing = packing ?? 1;
    }

    displayProductInfomation(stockCode, cost, price, margin, packing * cost, encodedStockCodeAndRowNumber);
    selectedProductsOnViewTab.push(products.find(prod => prod.stockCode === stockCode));
    if (isPriceChangesApplied) {
      let increase = false;
      if ($('#increase').is(':checked')) {
        increase = true;
      }
      let selectedOption = $("input[name='dollar-or-percentage']:checked").attr("id");

      await updateNewCostOnDatatable(stockCode, packingInfoDict[stockCode], stockPackCostInfoDict[stockCode], increase, selectedOption, encodedStockCodeAndRowNumber);
    }
    deselectedProductsOnViewTab = deselectedProductsOnViewTab.filter(prod => prod.stockCode !== stockCode);
    if (onLoad === false) selectedProductsOnViewTabCount++;
    if (selectedProductsOnViewTabCount === totalProductsOnViewTabCount) {
      isSelectAllProductsChecked = true;
      $("#view__select-or-unselect-all-products").prop("checked", true);
    }
  }
  else {
    if (selectedDataTransferType === "stocks") {
      const filteredProducts = products.filter(product => product.stockCode === stockCode);
      filteredProducts.forEach(product => {
        product.transferType = 'A';
      });

      let dropdown = $(currentRow).find('select.form-select');
      dropdown.val('A');
      dropdown.trigger('change');
    }

    let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, currentRow.rowIndex - 1);
    showOrHideProductInformation(encodedStockCodeAndRowNumber, false);

    selectedProductsOnViewTab = selectedProductsOnViewTab.filter(prod => prod.stockCode !== stockCode);
    deselectedProductsOnViewTab.push(products.find(prod => prod.stockCode === stockCode));
    if (onLoad === false) selectedProductsOnViewTabCount--;
    isSelectAllProductsChecked = false;
    $("#view__select-or-unselect-all-products").prop("checked", false);
  };
  localStorage.setItem('selectedProductsOnViewTab', JSON.stringify(selectedProductsOnViewTab))
}

function handleShowVendorProducts() {
  selectedProductsOnViewTab = [];
  selectedProductsOnViewTabCount = 0;
  showAllProducts = false;
  isShowVendorProducts = true;
  localStorage.setItem('showAllProducts', JSON.stringify(showAllProducts))
  $("#view__select-or-unselect-all-products").prop("checked", false);
  isSelectAllProductsModeActivated = false;
  noDeptSubDeptOrSubDetailSelected = false;
  vendorId = selectedVendorIdsOnViewTab[0];
  $("#show-all-products-btn").prop("disabled", false);
  $("#vendor-products-btn").prop("disabled", true);
  renderProducts();
}

async function handleShowAllProducts() {
  selectedProductsOnViewTab = [];
  selectedProductsOnViewTabCount = 0;
  showAllProducts = true;
  isShowVendorProducts = false;
  localStorage.setItem('showAllProducts', JSON.stringify(showAllProducts));
  vendorId = STRING_TABLE.DEFAULT.VENDOR_ID;
  recentChanges = false;
  localStorage.setItem('recentChanges', JSON.stringify(recentChanges));
  $("#show-all-products-btn").prop("disabled", true);
  $("#recent-changes-btn").prop("disabled", false);
  $("#vendor-products-btn").prop("disabled", false);
  dataTableProduct.draw();
  $("#show-all-products-btn").prop("disabled", true);
  $("#view__select-or-unselect-all-products").prop("checked", false);
}

let changeCurrentRowSelect = (isChecked, currentRow) => {
  let currentStockCode = currentRow.querySelector('td:nth-child(1)').textContent.trim();
  if (isChecked) {
    const filteredProducts = products.filter(product => product.stockCode === currentStockCode);
    filteredProducts.forEach(product => {
      product.transferType = selectedTransferOption;
    });

    let dropdown = $(currentRow).find('select.form-select');
    dropdown.val(selectedTransferOption);
    dropdown.trigger('change');

    let checkBox = $(currentRow).find('input.form-check-input');
    checkBox[0].checked = true;
  }
  else {
    const filteredProducts = products.filter(product => product.stockCode === currentStockCode);
    filteredProducts.forEach(product => {
      product.transferType = 'A';
    });

    let dropdown = $(currentRow).find('select.form-select');
    dropdown.val('A');
    dropdown.trigger('change');

    let checkBox = $(currentRow).find('input.form-check-input');
    checkBox[0].checked = false;
  }
}

async function handleSelectOrUnselectAllProducts() {
  let isChecked = $("#view__select-or-unselect-all-products").is(":checked");
  selectedProductsOnViewTab = [];
  selectedProductsOnViewTabCount = isChecked ? totalProductsOnViewTabCount : 0;
  isSelectAllProductsChecked = isChecked;
  isSelectAllProductsModeActivated = isChecked;
  $(".product").prop("checked", isChecked);
  showOrHideProductInformationAll(isChecked);

  dataTableProduct.on('page.dt', function () {
    let tbody = $("#table--products").find('tbody');
    tbody.find('tr').each(function () {
      changeCurrentRowSelect(isChecked, $(this)[0]);
    });
  });

  let tbody = $("#table--products").find('tbody');
  tbody.find('tr').each(function () {
    changeCurrentRowSelect(isChecked, $(this)[0]);
  });

  if (selectedDataTransferType === "stocks") {
    if (isChecked) {
      selectedProductsOnViewTab = products;
    }
    else {
      selectedProductsOnViewTab = [];
    }
  }

  if (selectedDataTransferType === "price-change" || selectedDataTransferType === "cost-change" || selectedDataTransferType === "label-printing") {
    deselectedProductsOnViewTab = [];

    if (isChecked) {
      selectedProductsOnViewTab = products;
      productInformationList = [];
      await handleSelectedProducts(selectedProductsOnViewTab);

    } else {
      selectedProductsOnViewTab.map(({ stockCode }) => {
        let currentProduct = products.find(p => p.stockCode === stockCode);
        let rowId = currentProduct.rowId;
        let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, rowId);
        showOrHideProductInformation(encodedStockCodeAndRowNumber)
      });
      selectedProductsOnViewTab = [];
    }
  }
}

async function handleSelectedProducts(selectedProducts, selectionChanged = false) {
  let storeId = STRING_TABLE.DEFAULT.STORE_ID;
  if (selectionChanged) {
    storeId = getSelectedStoreIdOnChangeSelectedOption();
  }
  else {
    storeId = await getSelectedStoreId();
  }
  let stockCostInfoList = [];
  let stockPriceInfoList = [];
  let stockPriceInfoListExtra = [];
  let stockCodes = [];
  let packingInfos = [];
  let _vendorId = vendorId;

  selectedProducts.map(async ({ stockCode, vendorId }) => {
    stockCodes.push(stockCode);

    if (!_globalIsBackOffice || isNullOrWhiteSpace(vendorId))
      vendorId = _vendorId;

    stockCostInfoList.push({ storeId, stockCode, vendorId, orderNo });

    if (orderNo > 1) {
      if (!_globalIsBackOffice || isNullOrWhiteSpace(vendorId))
        vendorId = _vendorId;

      stockPriceInfoList.push({ storeId, stockCode, vendorId, priceNumber: 1, orderNo });
      stockPriceInfoListExtra.push({ storeId, stockCode, vendorId, priceNumber: 1, orderNo: 0 });

    }
    else {
      if (!_globalIsBackOffice || isNullOrWhiteSpace(vendorId))
        vendorId = _vendorId;

      stockPriceInfoList.push({ storeId, stockCode, vendorId, priceNumber: 1, orderNo });
    }
    if (!policies.packCost) {
      packingInfos.push({ packing: 1 });
    }
  })

  let stockCostInfos = await getStockCostInfoBatch(stockCostInfoList);

  const showByOption = $('input[name="show-by"]:checked').val();

  for (let i = 0; i < stockCostInfoList.length; i++) {
    if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
      if (selectedVendorsOnSelectionTab.length) {
        stockCostInfos[i].vendorId = selectedVendorsOnSelectionTab[0].code;
      }
    }

    if (!_globalIsBackOffice)
      stockPriceInfoList[i].vendorId = stockCostInfos[i].vendorId;

    if (stockPriceInfoListExtra.length > 0) {
      stockPriceInfoListExtra[i].vendorId = stockCostInfos[i].vendorId;
    }
  }

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    for (let i = 0; i < stockPriceInfoList.length; i++) {
      stockPriceInfoList[i].orderNo = 0;
    }
  }

  let stockPriceInfos = await getStockPriceInfoBatch(stockPriceInfoList);
  let stockPriceInfosExtra = null;
  if (orderNo > 1) {
    stockPriceInfosExtra = await getStockPriceInfoBatch(stockPriceInfoListExtra);
  }
  if (policies.packCost) {
    packingInfos = await getPackingInfoBatch(stockCodes);
  }
  let i = 0;
  let increase = false;
  if ($('#increase').is(':checked')) {
    increase = true;
  }
  let selectedOption = $("input[name='dollar-or-percentage']:checked").attr("id");

  selectedProducts.map(async ({ stockCode, rowId }) => {
    let cost = stockCostInfos[i].cost;
    let price = stockPriceInfos[i].price;
    let margin = orderNo > 1 ? stockPriceInfosExtra[i].margin : stockPriceInfos[i].margin;
    let packing = packingInfos[i]?.packing ?? 1;
    i++;
    let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(stockCode, rowId);
    displayProductInfomation(stockCode, cost, price, margin, packing * cost, encodedStockCodeAndRowNumber);
    if (isPriceChangesApplied) {
      await updateNewCostOnDatatable(stockCode, packingInfoDict[stockCode], stockPackCostInfoDict[stockCode], increase, selectedOption, encodedStockCodeAndRowNumber);
    }
  });
}

function calculatePriceAndMarginAndUpdateRespectiveColumn(stockCode) {
  // If it's regular price, it would calculate and assign the value in respective column,
  // in case of other price type, it would set the column value as 0.00

  let isRegularPrice = selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE;
  const cost = parseDoubleFromUI(`#product-${stockCode}__cost`) ?? 0;
  const price = parseDoubleFromUI(`#product-${stockCode}__reg-price`) ?? 0;
  const margin = parseDoubleFromUI(`#product-${stockCode}__margin`) ?? 0;
  const inputValue = regularPrices[0].price;
  let { newPrice } = calculatePriceAndMargin(cost, price, margin, inputValue);
  newPrice = isRegularPrice ? newPrice : 0;

  if (newPrice !== price || !isRegularPrice) {
    $(`#product-${stockCode}__n-price`).show();
    $(`#product-${stockCode}__n-price`).text(newPrice.toFixed(2));
  }
}

function calculatePriceBasedOnPolicy(cost, margin) {
  let price = 0;

  if (policies.marginCalcCostMkup) {
    price = cost * (1 + margin / 100);
  }
  else {
    if (margin >= 0 && margin < 100) {
      price = cost / (1 - margin / 100);
    }
  }
  return price;
}

function calculateMarginBasedOnPolicy(cost, price) {
  price = price.toFixed(2)
  let margin = 0;

  if (policies.marginCalcCostMkup) {
    if (cost !== 0) {
      margin = (price - cost) / cost * 100;
    }
  }
  else {
    if (price !== 0 && cost !== 0) {
      margin = (price - cost) / price * 100;
    }
  }
  return margin;
}

function calculatePriceAndMargin(cost, oldPrice, oldMargin, inputValue) {
  let newPrice;
  let newMargin;
  let increase = increaseOrDecrease === STRING_TABLE.PRICE_SETUP.INCREASE;

  if (marginOrPrice === STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN) {
    if (increaseOrDecrease !== null) {
      newMargin = oldMargin + (increase ? inputValue : -inputValue);
    }
    else newMargin = inputValue;
    newPrice = calculatePriceBasedOnPolicy(cost, newMargin);
  }
  else {
    if (dollarOrPercentage === STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR) {
      newPrice = oldPrice + (increase ? inputValue : -inputValue);
    }
    else if (dollarOrPercentage === STRING_TABLE.PRICE_SETUP.CURRENCY_PERCENTAGE) {
      newPrice = oldPrice + (increase ? (oldPrice * inputValue / 100) : -(oldPrice * inputValue / 100));
    }
    else newPrice = inputValue;

    newMargin = calculateMarginBasedOnPolicy(cost, newPrice);
  }

  return { newPrice, newMargin };
}

const radioInputsForTransfer = document.querySelectorAll('input[name="flexRadioDefault"]');
if (radioInputsForTransfer) {
  radioInputsForTransfer.forEach(input => {
    input.addEventListener('change', () => {
      const checkedRadioId = document.querySelector('input[name="flexRadioDefault"]:checked').id;
      if (checkedRadioId === "addTransferOption") {
        selectedTransferOption = "A";
      }
      else if (checkedRadioId === "updateTransferOption") {
        selectedTransferOption = "U";
      }
      else {
        selectedTransferOption = "R";
      }
    });
  });
}

async function handleSaveAsPendingTransfer() {
  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    if (regularPrices[0].price === 0) {
      toastr.warning($("#costChangeRequest").val());
      return;
    }
  }
  else if (!verifyPriceChange() && selectedDataTransferType !== "stocks" && selectedDataTransferType !== "label-printing") {
    const message = $("#priceChangeRequest").val();
    toastr.warning(message);
    return;
  }

  let selectAllChecked = $("#view__select-or-unselect-all-products").is(":checked");
  if (selectedProductsOnViewTab.length === 0) {
    const message = $("#selectProductRequest").val();
    toastr.warning(message);
    return;
  }

  if (!_globalIsBackOffice && selectedStoreIdsOnViewTab.length === 0) {
    const message = $("#cantIdentifyDestinationStores").val();
    toastr.warning(message);
    return;
  }

  clearTimeout(validationTimeout);
  if (!validateDateSelection()) {
    return;
  }

  if ($('input[name="show-by"]:checked').val() === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    vendorId = selectedVendorIdsOnViewTab[0];
  }

  selectedProductsOnViewTab = removeDuplicateProducts(selectedProductsOnViewTab);
  deselectedProductsOnViewTab = removeDuplicateProducts(deselectedProductsOnViewTab);

  const isRegularPrice = selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE;
  const transUser = $("#transUser").val();

  const savingText = $("#saving").val();
  $("#save-as-pending-transfer-btn-view")
    .html(`<span class="spinner-border spinner-border-sm" role="status"></span> ${savingText}...`).prop("disabled", true);

  const showByOption = $('input[name="show-by"]:checked').val();
  let selectedOptionIds;

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
    selectedOptionIds = selectedStoreIdsOnViewTab;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
    selectedOptionIds = selectedRegionIdsOnViewTab;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
    selectedOptionIds = selectedPriceGroupIdsOnViewTab;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    selectedOptionIds = selectedVendorIdsOnViewTab;
  }

  const pendingTransfers = {
    products: selectedProductsOnViewTab,
    deselectedProducts: deselectedProductsOnViewTab,
    storeIds: selectedStoreIdsOnViewTab,
    selectedOptionIds: selectedOptionIds,
    priceDetails: isRegularPrice ? regularPrices : specialPrices,
    departments: selectedDepartments,
    subDepartments: selectedSubDepartments,
    subDetails: selectedSubDetails,
    showAllProducts: showAllProducts,
    isSelectAllStoreOrOtherOptionChecked: $("#select-or-unselect-all").is(":checked"),
    isSelectAllProductsChecked: $("#view__select-or-unselect-all-products").is(":checked"),
    isSelectAllProductsModeActivated: isSelectAllProductsModeActivated,
    orderNo: orderNo,
    vendorId: (selectedDataTransferType === "stocks" || _globalIsBackOffice) ? "" : vendorId,
    PR_Units: PR_Units,
    increaseOrDecrease: increaseOrDecrease,
    dollarOrPercentage: dollarOrPercentage,
    marginOrPrice: marginOrPrice,
    selectedPriceOption: selectedPriceOption,
    selectedPriceType: selectedPriceType,
    effectiveDateFrom: $("#effective-date--from__control").val(),
    effectiveDateTo: $("#effective-date--to__control").val(),
    effectiveDateDoesNotEnd: doesNotEnd,
    policies: policies,
    selectedDataTransferType: selectedDataTransferType,
    changePriceBasedOnCost: changePriceBasedOnCost,
    transUser: transUser,
    isStockChangeSelected: selectedDataTransferType === "stocks",
    userCode: document.getElementById("userCode").value,
    transType: selectedTransferOption,
    showByOption: $('input[name="show-by"]:checked').val(),
    isShowVendorProducts: isShowVendorProducts
  };

  let error = null;
  await ajaxPostAsync(STRING_TABLE.URL.INSERT.PENDING_TRANSFERS, pendingTransfers).catch(err => error = err);

  const saveAsPendingTransferText = $("#saveAsPendingTransfer").val();
  $("#save-as-pending-transfer-btn-view").html(saveAsPendingTransferText).prop("disabled", false);
  let message = $("#confirmSaveAsPendingTransfer").val();
  let language = document.getElementById('language').value;

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) message = message + $("#price-change").val();
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) message = message + $("#cost-change").val();
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.STOCKS) message = message + $("#stocks").val();

  if (language === "en") message = message + " Transfer.";
  else message = message + ".";

  error ? toastr.error(error) : toastr.success(message);
  clearPricesTable();
  $("#regular-cost-0__text").text(`0.${'0'.repeat(policies.costDec)}`);
  $("#regular-cost-0__input").val(`0.${'0'.repeat(policies.costDec)}`);
}

function handleTransferTypeChange(deptId, subDeptId = null, subDetailId = null) {
  let selector = getTransTypeSelector(deptId, subDeptId, subDetailId);
  let selectedTransType = $(`#${selector}-select-trans-type`).val();
  commodityTransTypes[selector] = selectedTransType;
}

function updateComparisonTableColumn(priceNumber, oldMargin, oldPrice, newMargin, newPrice) {
  newMargin = isFinite(newMargin) ? newMargin : 0;

  $(`#current-margin-${priceNumber}`).text(oldMargin.toFixed(2));
  $(`#current-price-${priceNumber}`).text(oldPrice.toFixed(2));
  $(`#regular-margin-${priceNumber}`).text(newMargin.toFixed(2));
  $(`#regular-price-${priceNumber}`).text(newPrice.toFixed(2));
}

function getSelectedStoreIdOnChangeSelectedOption() {
  let storeId = STRING_TABLE.DEFAULT.STORE_ID;
  const showByOption = $('input[name="show-by"]:checked').val();

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
    storeId = selectedStoreIdsOnViewTab.length ? selectedStoreIdsOnViewTab[0] : STRING_TABLE.DEFAULT.STORE_ID;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
    storeId = selectedRegionIdsOnViewTab.length ? selectedRegionIdsOnViewTab[0] : STRING_TABLE.DEFAULT.STORE_ID;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
    storeId = selectedPriceGroupIdsOnViewTab.length ? selectedPriceGroupIdsOnViewTab[0] : STRING_TABLE.DEFAULT.STORE_ID;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    storeId = selectedVendorIdsOnViewTab.length ? selectedVendorIdsOnViewTab[0] : STRING_TABLE.DEFAULT.STORE_ID;
  }
  return storeId;
}

async function getSelectedStoreId() {
  let storeId = STRING_TABLE.DEFAULT.STORE_ID;

  const showByOption = $('input[name="show-by"]:checked').val();
  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
    storeId = selectedStoreIdsOnViewTab.length ? selectedStoreIdsOnViewTab[0] : STRING_TABLE.DEFAULT.STORE_ID;
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
    if (selectedRegionsOnSelectionTab.length) {
      let storeIds = await getStoresByRegionID(selectedRegionsOnSelectionTab[0].id);
      if (storeIds && storeIds.length) {
        storeId = storeIds[0].store_ID;
      }
    }
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
    if (selectedPriceGroupsOnSelectionTab.length) {
      let storeIds = await getStoresByGroupID(selectedPriceGroupsOnSelectionTab[0].id);
      if (storeIds && storeIds.length) {
        storeId = storeIds[0].store_ID;
      }
    }
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    if (selectedVendorsOnSelectionTab.length) {
      let storeIds = await getStoresByVendorID(selectedVendorsOnSelectionTab[0].code);
      if (storeIds && storeIds.length) {
        storeId = storeIds[0].store_ID;
      }
    }
  }

  return storeId;
}

async function handleComparisonTable(stockCode) {
  $("#comparison-modal").modal("toggle");
  let storeId = await getSelectedStoreId();

  const product = selectedProductsOnViewTab.find(prod => prod.stockCode == stockCode);

  if (!product) return;

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    if (selectedPriceType === STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE) {
      regularPrices.forEach(async (priceDetails, index) => {
        const priceNumber = index + 1;
        let { cost, vendorId } = await getStockCostInfo(storeId, product.stockCode, orderNo); const showByOption = $('input[name="show-by"]:checked').val();
        if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
          if (selectedVendorsOnSelectionTab.length) {
            vendorId = selectedVendorsOnSelectionTab[0].code;
          }
        }
        let { price, margin } = await getStockPriceInfo(storeId, product.stockCode, vendorId, priceNumber, orderNo);
        let newPrice = price;
        let newMargin = margin;

        if (priceDetails.price !== 0) {
          ({ newPrice, newMargin } = calculatePriceAndMargin(cost, price, margin, priceDetails.price));
        }
        updateComparisonTableColumn(priceNumber, margin, price, newMargin, newPrice);
      });

      $(".regular-price-comparison-table").show();

      if (selectedPriceOption === STRING_TABLE.PRICE_OPTION.FUTURE) {
        $("#comp--before__effective-dates-section").show();
        $("#comp--after__effective-dates-section").html($("#effective-dates-section").clone()).show();
        if (!doesNotEnd) {
          $(".modal-body #does-not-end").hide();
        }
      }
    }
    else {
      $("#comp__special-pricing-section").html($("#special-pricing-section").clone());
      $("#comp--after__effective-dates-section").html($("#effective-dates-section").clone());
      $(".regular-price-comparison-table").hide();
      $(".comp-hide").hide();
      $("#comp--before__effective-dates-section").hide();
    }
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    let packCount = await getPackingInfo(stockCode);
    if (policies.packCost) {
      packCount = await getPackingInfo(stockCode);
      packCount = packCount ?? 1;
    }

    let increase = false;
    if ($('#increase').is(':checked')) {
      increase = true;
    }
    regularPrices.forEach(async (_, index) => {
      const priceNumber = index + 1;
      const inputValue = regularPrices[0].price;
      let oldCostInfo = await getStockCostInfo(storeId, product.stockCode, orderNo);
      let oldCost = oldCostInfo.cost * packCount;

      let vendorId = oldCostInfo.vendorId ?? STRING_TABLE.DEFAULT.VENDOR_ID;
      //For vendor options, we will use first selected vendor
      const showByOption = $('input[name="show-by"]:checked').val();
      if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
        if (selectedVendorsOnSelectionTab.length) {
          vendorId = selectedVendorsOnSelectionTab[0].code;
        }
      }

      let { price, margin } = await getStockPriceInfo(storeId, product.stockCode, vendorId, priceNumber, orderNo);
      let newPrice = price;
      let newMargin = margin;
      let newCost;

      const selectedOption = $("input[name='dollar-or-percentage']:checked").attr("id");
      if (selectedOption === STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR) {
        newCost = (oldCost / packCount) + (increase ? (inputValue / packCount) : -(inputValue / packCount));
      }
      else if (selectedOption === STRING_TABLE.PRICE_SETUP.CURRENCY_PERCENTAGE) {
        newCost = (oldCost / packCount) + (increase ? ((oldCost * inputValue / 100) / packCount) : -((oldCost * inputValue / 100) / packCount));
      }
      else {
        newCost = inputValue;
      }

      if (marginOrPrice === STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN) {
        newMargin = calculateMarginBasedOnPolicy(newCost, price);
      }
      else {
        newPrice = calculatePriceBasedOnPolicy(newCost, margin);
      }

      updateComparisonTableColumn(priceNumber, margin, price, newMargin, newPrice);
    });

    $(".regular-price-comparison-table").show();

    if (selectedPriceOption === STRING_TABLE.PRICE_OPTION.FUTURE) {
      $("#comp--before__effective-dates-section").show();
      $("#comp--after__effective-dates-section").html($("#effective-dates-section").clone()).show();
      if (!doesNotEnd) {
        $(".modal-body #does-not-end").hide();
      }
    }
  }
  else {
    $("#comp__special-pricing-section").html($("#special-pricing-section").clone());
    $("#comp--after__effective-dates-section").html($("#effective-dates-section").clone());
    $(".regular-price-comparison-table").hide();
    $(".comp-hide").hide();
    $("#comp--before__effective-dates-section").hide();
  }

  $(".modal-body #special-pricing-section__header").addClass("comp__section-header");
  $(".modal-body .comp__section-header").text(compTableSectionHeader);
  $(".modal-body td").prop("disabled", true);
  $(".modal-body button").hide();
  $(".modal-body input").prop("disabled", true);
}

// Triggers switching to other tab from View Tab 
$('#view-tab-link-wrapper').on('hidden.bs.tab', async function (e) {
  selectedStoreIdsOnViewTab = [];
  selectedProductsOnViewTab = [];
  searchValue = "";

  $("#view__select-or-unselect-all-stores").parent().show();
});

function resetPriceSetup() {
  clearPricesTable();
  $('input[name="margin-or-price"]:checked').prop('checked', false);
  $('input[name="margin-or-price"]').trigger('change');   // INFO: These two should be at the top, because the events need to be executed sequentially
  $("#current-price").prop('checked', true).trigger('change');
  $('#price-type').val(STRING_TABLE.PRICE_TYPE.R_REGULAR_PRICE).trigger('change');
  $('#currency-dollar').prop('checked', true).trigger('change');
  $('#increase').prop('checked', true).trigger('change');
  $("#does-not-end__input").prop('checked', false).trigger('change');
}

/*:::::::::::::::: View Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

// Triggers after switching to List Tab
$('#list-tab-link-wrapper').on('shown.bs.tab', async function (e) {
  isAuthenticated().then(response => {
    if (response) {
      triggerListTab();
    }
    else {
      return;
    }
  }).catch(toastrError);
});

async function triggerListTab() {
  reRenderTransferTable();
  refreshPendingTransferStores();
}

// Resizes the store selection container to match height with transfer table
function resizeTransferStoreContainer() {
  const selectedStoresContainer = $("#list__selected-stores-container");
  const transfersTableContainer = $("#table-container--transfers");
  const outerHeight = transfersTableContainer.outerHeight();
  selectedStoresContainer.css("height", outerHeight);
}

async function reRenderTransferTable(storeId = null) {
  let checkDataExistsInList = false;

  if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    await renderPriceChangeListTab(storeId);
    checkDataExistsInList = $('#table--price-change-transfer').DataTable().data().any();
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY) {
    renderCommodityListTab(storeId);
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS) {
    renderPromotionListTab(storeId);
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    await randerCostChangeListTab(storeId);
    checkDataExistsInList = $('#table-cost-change-transfer').DataTable().data().any();
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.HOT_BUTTONS) {
    renderHotButtonsListTab(storeId);
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.STOCKS) {
    renderStockTransfersTable(storeId);
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.LABEL_PRINTING) {
    await renderLabelPrintingListTab();
    checkDataExistsInList = $('#table--label-print').DataTable().data().any();
  }

  $("#buttonDeleteFromTransferList").prop("disabled", !checkDataExistsInList);
}

async function refreshPendingTransferStores() {
  await loadPendingTransferStores().then(() => renderPendingTransferStores());
}

function renderPendingTransferStores() {
  const container = $("#list__selected-stores-container");
  container.empty();

  if (listedStoresOnListTab == null || listedStoresOnListTab.length === 0) return;
  selectedStoreIdOnListTab = listedStoresOnListTab[0].store_ID;

  listedStoresOnListTab.forEach((store, index) => {
    container.append(`
        <option class="transfer-store-option"
                id="transfer-store-option-${store.store_ID}" 
                value="${store.store_ID}" 
                ${index === 0 ? "selected" : ""}
                onclick="handleSelectedStoreChangeOnListTab('${store.store_ID}')">
                    ${store.store_ID} -- ${store.store_Desc}
        </option>`)
  })
}

function handleSelectedStoreChangeOnListTab(storeId) {
  selectedStoreIdOnListTab = storeId;
  $(".transfer-store-option").prop("selected", false);
  $(`#transfer-store-option-${storeId}`).prop("selected", true);
  reRenderTransferTable(storeId);
}

async function handleDeleteFromTransferList(batchId, itemId, description, storeId, deleteType) {
  let deleteSelectedItem = false;
  let deleteStoreData = false;
  let deleteAll = false;

  deleteType === STRING_TABLE.DELETE.ITEM ? deleteSelectedItem = true
    : deleteType === STRING_TABLE.DELETE.STORE ? deleteStoreData = true
      : deleteAll = true;

  let title = $("#warning").val();
  let message = deleteSelectedItem ? $("#deleteSelectedItemWarning").val()
    : deleteStoreData ? $("#deleteStoreDataWarning").val()
      : $("#deleteAllWarning").val();

  showConfirmModal(title, message).then((result) => {
    if (result === true) {
      let params = new URLSearchParams();
      params.append(STRING_TABLE.PARAMS.DATA_TRANSFER_TYPE, selectedDataTransferType);

      if (deleteSelectedItem) {

        if (selectedDataTransferType !== STRING_TABLE.DATA_TRANSFER_TYPE.LABEL_PRINTING)
          params.append(STRING_TABLE.PARAMS.BATCH_ID, batchId);

        params.append(STRING_TABLE.PARAMS.ITEM_ID, itemId);
        if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY) {
          params.append(STRING_TABLE.PARAMS.DESCRIPTION, description);
        }
      }
      else if (deleteStoreData)
        params.append(STRING_TABLE.PARAMS.STORE_ID, storeId);
      else
        params.append(STRING_TABLE.PARAMS.DELETE_ALL, true);

      let urlWithParams = `${STRING_TABLE.URL.DELETE.FROM_TRANSFER_LIST}?${params.toString()}`;

      $.ajax({
        url: urlWithParams,
        type: "GET",
        success: function (response) {
          toastr.success($("#deleteSuccessful").val());
          reRenderTransferTable();
          refreshPendingTransferStores();
        },
        error: function (xhr, status, error) {
          toastr.error("Could not be deleted! Some error occured!"); // TODO: Localize this
        }
      });
    }
  });
}

async function handlePerformTransfer(isFinalizeText = false) {

  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE && priceChangeTransfers.length === 0) {
    toastr.warning($("#noDataToTransfer").val());
    return;
  }
  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE && costChangeTransfers.length === 0) {
    toastr.warning($("#noDataToTransfer").val());
    return;
  }
  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.STOCKS && dataTableStockTransfer.rows().count() == 0) {
    toastr.warning($("#noDataToTransfer").val());
    return;
  }
  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY && commodityTransfers.length === 0) {
    toastr.warning($("#noDataToTransfer").val());
    return;
  }
  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS && promotionTransfers.length === 0) {
    toastr.warning($("#noDataToTransfer").val());
    return;
  }
  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.HOT_BUTTONS && hotButtonsTransfers.length === 0) {
    toastr.warning($("#noDataToTransfer").val());
    return;
  }

  let setVend = policies.setVend;
  let updateAllVendor = policies.updateAllVendor;
  let marginCalcCostMkup = policies.marginCalcCostMkup;
  let userCode = $("#userCode").val();
  $("#btn-transfer").html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${$("#transferingText").val()}...`).prop("disabled", true);

  const url = `${STRING_TABLE.URL.POST.PERFORM_TRANSFER}?dataTransferType=${selectedDataTransferType}&setVend=${setVend}&updateAllVendor=${updateAllVendor}&marginCalcCostMkup=${marginCalcCostMkup}&NumberOfPrice=${policies.numberOfPrice}&userCode=${userCode}`;

  await ajaxPostAsync(url)
    .then(async () => {
      toastr.success($("#tranferSuccessful").val());
      reRenderTransferTable();
      refreshPendingTransferStores();
      await resetAllSections();
    })
    .catch(() => toastr.error($("#anUnexpectedErrorHasOccured").val()))
    .finally(() => {
        const buttonText = isFinalizeText ? $("#finalizeText").val() : $("#transferText").val();
        $("#btn-transfer").html(buttonText).prop("disabled", false);
    })
}

function displayPriceBookTransferReport() {
  let language = document.getElementById('language').value;
  let url = "";
  let reportName = "";
  let width = 800;
  let height = 1000;
  let reportWidth = "790px";
  let reportHeight = "1200px";

  if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.HOT_BUTTONS) {
    url = "/PricebookTransfer/GetHotButtonsReportData";
    reportName = "HotButtonsReport";
    width = 1000;
    height = 1000;
    reportWidth = "810px";
    reportHeight = "1400px";
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PRICE_CHANGE) {
    url = "/PricebookTransfer/GetPriceChangeReportData";
    reportName = "PriceChangeReport";
    width = 1150;
    height = 1100;
    reportWidth = "990px";
    reportHeight = "1400px";
  }
  else if (selectedDataTransferType === "stocks") {
    url = "/PricebookTransfer/GetStockTransferReportData";
    reportName = "StockTransferReport";
    width = 1100;
    height = 1100;
    reportWidth = "870px";
    reportHeight = "1400px";
  }
  else if (selectedDataTransferType == STRING_TABLE.DATA_TRANSFER_TYPE.COST_CHANGE) {
    url = "/PricebookTransfer/GetCostChangeReportData";
    reportName = "CostChangeReport";
    width = 1100;
    height = 1100;
    reportWidth = "900px";
    reportHeight = "1400px";
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.COMMODITY) {
    url = "/PricebookTransfer/GetCommodityTransferReportData";
    reportName = "CommodityTransferReport";
    width = 1100;
    height = 1100;
    reportWidth = "870px";
    reportHeight = "1400px";
  }
  else if (selectedDataTransferType === STRING_TABLE.DATA_TRANSFER_TYPE.PROMOTIONS) {
    url = "/PricebookTransfer/GetPromotionalDiscountTransferReportData";
    reportName = "PromotionalDiscountTransferReport";
    width = 1100;
    height = 1100;
    reportWidth = "870px";
    reportHeight = "1400px";
  }
  else {
    return;
  }
  $.ajax({
    type: "GET",
    dataType: 'json',
    url: url,
    success: function (result) {
      if (result === null) return;
      result.reportName = reportName;
      result.language = language;
      result.reportWidth = reportWidth;
      result.reportHeight = reportHeight;
      result.isBackOffice = _globalIsBackOffice;

      if (pricebookTransferReportPopUpWindow == null || pricebookTransferReportPopUpWindow.closed) {
        let left = (screen.width / 2) - (width / 2);
        let top = (screen.height / 2) - (height / 2);
        let options = "toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=" + width + ", height=" + height + ", top=" + top + ", left=" + left;
        pricebookTransferReportPopUpWindow = window.open('PricebookTransfer/ShowReport', 'Pricebook Transfer Report', options);
        pricebookTransferReportPopUpWindow.focus();

        pricebookTransferReportPopUpWindow.onload = function () {
          pricebookTransferReportPopUpWindow.postMessage(result, $("#_webBaseUrl").val());
        };
      }
      else {
        let left = (screen.width / 2) - (width / 2);
        let top = (screen.height / 2) - (height / 2);
        pricebookTransferReportPopUpWindow.resizeTo(width, height);
        pricebookTransferReportPopUpWindow.moveTo(left, top);
        pricebookTransferReportPopUpWindow.focus();

        if (pricebookTransferReportPopUpWindow.document.readyState === "complete") {
          pricebookTransferReportPopUpWindow.postMessage(result, $("#_webBaseUrl").val());
        }
        else {
          pricebookTransferReportPopUpWindow.onload = function () {
            pricebookTransferReportPopUpWindow.postMessage(result, $("#_webBaseUrl").val());
          };
        }
      }
    },
    error: function () {
      let message = document.getElementById("errorMessage").value;
      toastr.error(message);
    }
  })
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/


/*:::::::::::::::: Misc. Functions Start ::::::::::::::::*/

function createDatatable(selector, props = {}) {
  let dataTable;
  if (selector !== 'table--products') {
    props.searching = true;
    dataTable = createDatatableWithSearching(selector, props);
  }
  else {
    dataTable = createDataTableGeneric(selector, props, true);
    // Add a input element for each footer column having the class "input" 
    let id = 0;
    $($(dataTable.table().footer()).html()).find("th").each(function () {
      let title = $($(dataTable.table().header()).html()).find("th").eq($(this).index()).text();
      $(dataTable.column(id++).footer()).html(`<input class="form-control p-2" type="text" id="${title}" placeholder="${$("#search").val()} ${title}"/>`);
    })

    dataTable.columns().every(function () {
      const dataTableColumn = this;
      const searchInput = $(this.footer()).find('input');

      searchInput.on('keyup change', function () {
        // Clear the search input fields of other columns
        dataTable.columns().every(function () {
          if (this.index() !== dataTableColumn.index()) {
            let searchValue = $(this.footer()).find('input').val();
            if (searchValue?.length) {
              $(this.footer()).find('input').val('');
              dataTable.column(this.index()).search('').draw();
            }
          }
        });

        searchColumnName = searchInput.prop("id");
        searchValue = searchInput.val();
        // Perform the search on the current column with debouncing
        debouncedSearch(dataTableColumn, true);
      });
    })
  }
  return dataTable;
}

function removeDuplicateProducts(prods) {
  return prods
    .reduce((acc, curr) => {
      if (!acc.find(prod => prod.stockCode === curr.stockCode)) {
        acc.push(curr);
      }
      return acc;
    }, []);
}

function initializeTransTypeValues() {
  departments.forEach(dept => {
    commodityTransTypes[dept.deptId] = "A";
  });
  subDepartments.forEach(subDept => {
    commodityTransTypes[getTransTypeSelector(subDept.deptId, subDept.subDeptId)] = "A";
  });
  subDetails.forEach(subDetail => {
    commodityTransTypes[getTransTypeSelector(subDetail.deptId, subDetail.subDeptId, subDetail.subDetailId)] = "A";
  });
}

function getTransTypeSelector(deptId, subDeptId, subDetailId) {
  let selector = deptId.trim();
  if (subDeptId != null) selector += `-${subDeptId.trim()}`;
  if (subDetailId != null) selector += `-${subDetailId.trim()}`;
  return selector;
}

/*:::::::::::::::: Misc. Functions End ::::::::::::::::*/

