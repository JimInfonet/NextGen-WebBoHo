let appModeBackOffice;
let policies;
let stockPriceFormModeCurrent = true;
let stockCode = '';
let priceGroups;
let regions;
let allStores;
let stockPriceFormData;
let costEditMode = false;
let orderNo = 0;
let storeID;
let isPriceChange = false;
let isMarginChange = false;
let specialPriceTable;
let rebatePriceTable;
let priceTable;
let specialPriceToDelete;
let specialPriceToEdit;
let rebateToDelete;
let vendorIDToToggle;
let priceDataTableRow;
let priceDataTableRowIndex;
let isAllStore = false;
let PRICE_GROUP = "PRICEGROUP";
let REGION = "REGION";
let addVendorMode = false;
let priceEditMode = false;
let editOrUpdateCostButtonControlClicked = false;
let updatePrice = true;
let previousStockPriceFormData;

//Start of Stock Price Form Load Section
$(async function () {
  if (!initConfiguration()) return;
  await initPolicy();

  if (!policies.packCost) {
    $("#packCostSection").hide();
  }

  await reInitializeStockPriceData();
  setButtonsHeight('.button-group');
});

let reInitializeStockPriceData = async () => {
  if (!appModeBackOffice) {
    await initHeadOfficeConfiguration();
  }

  await loadStockPriceData();
  clearRebateForm();
}


let addEventListenerNotToCrossSmallDateTimeLimit = (dateTimeInputId) => {
  const dateTimeInput = document.getElementById(dateTimeInputId);
  $(`#${dateTimeInputId}`).val(getCurrentDate());

  dateTimeInput.addEventListener('input', (event) => {
    if (dateTimeInput.value > '2079-06-06') {
      dateTimeInput.value = '2079-06-06';
    }
  });
}

let initConfiguration = () => {
  stockCode = $("#stockCode").val().trim();
  if (stockCode === "" || stockCode === null || stockCode === undefined) {
    toastr.error("Stock code not found.");
    return false;
  }

  stockPriceFormModeCurrent = true;
  orderNo = 0;
  hideControls();

  $("#cboOrderNoSelectControl").hide();
  $('#cancelVendorButtonControl').attr('hidden', true);
  $("#priceSectionEditControls").hide();

  appModeBackOffice = $("#appModeBackOffice").val().trim().toLowerCase() === "true" ? true : false;
  return true;
}

let initPolicy = async () => {
  const url = `StockPrice/GetStockPricePolicies?stockCode=${stockCode}`;

  await ajaxGetAsync(url)
    .then(_ => policies = _)
    .catch(() => toastr.error($("#failedToLoadPolicyForStockPrice").val()));
}

let initHeadOfficeConfiguration = async () => {
  await loadRegions();
  await loadPriceGroups();
  await loadStores();

  $("#regionSelectionControlDiv").hide();
  $("#priceGroupSeclectionControlDiv").hide();
  $("#storeSeclectionControlDiv").hide();

  $("#selectionControl").val("ALL");
  storeID = "ALL";
}

function getConfigData() {
  let vendorID = $("#vendorSelectOptionControl").val();
  return {
    stockCode: stockCode,
    vendorID: (vendorID) ? vendorID : null,
    orderNo: orderNo,
    storeID: appModeBackOffice ? null : storeID
  }
}

let loadStockPriceData = async () => {
  const url = "StockPrice/GetStockPrice";
  const data = {
    ...getConfigData(),
    priceType: "R"
  };

  await ajaxPostAsync(url, data)
    .then(_stockPrice => {
      stockPriceFormData = _stockPrice;
      initStockPriceFrom();
    })
    .catch(() => toastr.error($("#failedToLoadStockPriceData").val()));
}

function initStockPriceFrom() {
  reInitializeVariableOnLoadData();
  if (!stockPriceFormData) {
    toastr.error(`${document.getElementById("failedToLoadStockPriceData").value}.`);
    return;
  }
  $("#stockItemDescription").text(`${stockPriceFormData.stockItemName} - (${stockPriceFormData.stockItemCode})`);

  if (policies.useArinovices && !appModeBackOffice) {
    $("#specialPriceTypeSelectControl").append(`<option value="D">D - Discount Pricing</option>`);
  }

  if (policies.setVendor && (!stockPriceFormData.vendors)) {
    policies.forceCost = true;
    $("#addVendorButtonControl").prop("disabled", !policies.userAccessStockCode);
    $("#editOrUpdateCostButtonControl").prop("disabled", true);
  }
  else {
    $("#editOrUpdateCostButtonControl").prop("disabled", !policies.userAccessStockCode);
  }
  $("#addVendorButtonControl").prop("disabled", !policies.userAccessStockCode);

  // render vendors
  renderVendorsToView();
  setUpateAllPolicy();
}

function reInitializeVariableOnLoadData() {
  costEditMode = false;
  isPriceChange = false;
  isMarginChange = false;
}

function renderVendorFormData(vendor) {
  renderVendorPart(vendor);

  const vendorID = vendor ? vendor.code : null;
  renderStockCost(vendorID);
  renderStockPriceToView(vendorID);
  renderStockPriceRebate(vendorID);
  renderSpecialPrice(vendorID);

  if (stockPriceFormData.promotion) {
    renderPromotionInformation();
  }
  enableDisableControlBasedOnPolicy();
}

function enableDisableControlBasedOnPolicy() {
  let selectedVendor = $("#vendorSelectOptionControl").val();
  let vendorCount = 0;
  if (stockPriceFormData.vendors) {
    vendorCount = stockPriceFormData.vendors.filter(x => x.Code !== "ALL").length;
  }

  if (stockPriceFormData.stockCosts.length === 0) {
    policies.forceCost = true;
  }
  else {
    policies.forceCost = false;
  }

  if (!policies.setVendor && selectedVendor === null) {
    $("#vendorSelectOptionControl").append("<option value='ALL'>ALL</option>");
  }

  if (policies.setVendor && vendorCount === 0) {
    $("#addVendorButtonControl").prop("disabled", false);
    $("#vendorSelectOptionControl").prop("disabled", false);
    $("#editOrUpdateCostButtonControl").prop("disabled", true);
  }
  else {
    $("#addVendorButtonControl").prop("disabled", (policies.forceCost || !policies.userAccessStockCode));
    $("#vendorSelectOptionControl").prop("disabled", policies.forceCost);
  }

  $("#selectionControl").prop("disabled", policies.forceCost);
  $("#specialPriceTypeSelectControl").prop("disabled", (policies.forceCost
    || (policies.setVendor && selectedVendor === null)));
  $("#addRebateButton").prop("disabled", (policies.forceCost
    || (policies.setVendor && selectedVendor === null)));

  if (stockPriceFormModeCurrent) {
    $("#radioButtonFuture").prop("disabled", policies.forceCost);
  }

  $("#stockPriceVendorListSection").prop("disabled", (policies.forceCost || !policies.userAccessStockCode));
  $("#deleteVendorButtonControl").prop("disabled", (policies.forceCost || !policies.userAccessStockCode));
  $("#editPriceButtonControl").prop("disabled", (policies.forceCost || !policies.userAccessStockCode)
    || (policies.setVendor && selectedVendor === null));
  $("#updateMarginButtonControl").prop("disabled", (policies.forceCost || !policies.userAccessStockCode));
  $("#stockPriceMargin").prop("disabled", ((policies.forceCost || !policies.userAccessStockCode)
    || (policies.setVendor && selectedVendor === null)));
  $("#stockPricePrice").prop("disabled", ((policies.forceCost || !policies.userAccessStockCode)
    || (policies.setVendor && selectedVendor === null)));
  $("#cancelPriceButtonControl").prop("disabled", (policies.setVendor && selectedVendor === null));
  $("#cancelPriceButtonControl").prop("disabled", (policies.setVendor && selectedVendor === null));
  $("#editOrUpdateCostButtonControl").prop("disabled", (policies.setVendor && selectedVendor === null)
    || policies.costType === "Average");
  $("#regionSelectionControl").prop("disabled", policies.forceCost);
  $("#priceGroupSeclectionControl").prop("disabled", policies.forceCost);
  $("#storeSeclectionControl").prop("disabled", policies.forceCost);
  $("#storeSeclectionControl").prop("disabled", !policies.userAccessStockCode);

  if (policies.costType === "Average") {
    $("#averageCostSection").show();
    $("#packCostSection").hide();
    $("#standardCostSection").hide();
  }
}
//End of Stock Price Form Load Section



// Start of Region, Store, Price Group Section
let loadRegions = async () => {
  const url = "Region/GetRegion";

  await ajaxGetAsync(url)
    .then(_regions => {
      regions = _regions;
      renderRegionToView();
    })
    .catch(() => toastr.error($("#failedToLoadRegionsStockPrice").val()));
}

function renderRegionToView() {
  let data = regions.map((x, i) => ({
    value: x.id,
    name: x.name,
    isSelected: i === 0 ? true : false
  }));

  let regionSelectControl = $("#regionSelectionControl");
  const controlPlaceHolder = $("#regionControlPlaceHolder").val();
  regionSelectControl.html('');

  renderSelectOptionDefault(regionSelectControl, controlPlaceHolder);
  renderSelectOptions(regionSelectControl, data);
}

let loadPriceGroups = async () => {
  const url = "StockPrice/GetPriceGroup";

  await ajaxGetAsync(url)
    .then(_priceGroups => {
      priceGroups = _priceGroups;
      renderPriceGroupToView();
    })
    .catch(() => toastr.error($("#failedToLoadPriceGroupsForStockPrice").val()));
}

function renderPriceGroupToView() {
  let data = priceGroups.map((x, i) => ({
    value: x.id,
    name: x.name,
    isSelected: i === 0 ? true : false
  }));

  let priceGroupSelectControl = $("#priceGroupSeclectionControl");
  const controlPlaceHolder = $("#priceGroupControlPlaceHolder").val();
  priceGroupSelectControl.html('');

  renderSelectOptionDefault(priceGroupSelectControl, controlPlaceHolder);
  renderSelectOptions(priceGroupSelectControl, data);
}

let loadStores = async () => {
  const url = "Store/GetStoresWithRegionAndPriceGroup";

  await ajaxGetAsync(url)
    .then(_ => allStores = _)
    .catch(() => toastr.error($("#failedToLoadPriceGroupsForStockPrice").val()));
}

function getStores(regionID = null, priceGroupID = null) {
  if ((regionID === null || regionID === undefined) && (priceGroupID === null || priceGroupID === undefined)) {
    let newStores = allStores.map(x => (JSON.stringify({
      storeID: x.storeID,
      storeName: x.storeName
    })));

    let groupedStore = [...new Set(newStores)];
    let data = groupedStore.map(x => ({
      value: JSON.parse(x).storeID,
      name: JSON.parse(x).storeName,
      isSelected: false
    }));

    return data;
  }

  if ((regionID === null || regionID === undefined) && (priceGroupID !== null || priceGroupID !== undefined)) {
    let filteredStores = allStores.filter(x => x.priceGroupID === priceGroupID);
    let newStores = filteredStores.map(x => (JSON.stringify({
      storeID: x.storeID,
      storeName: x.storeName
    })));

    let groupedStore = [...new Set(newStores)];
    let data = groupedStore.map(x => ({
      value: JSON.parse(x).storeID,
      name: JSON.parse(x).storeName,
      isSelected: false
    }));

    return data;
  }

  if ((regionID !== null || regionID !== undefined) && (priceGroupID === null || priceGroupID === undefined)) {
    let filteredStores = allStores.filter(x => x.regionID === regionID);
    let newStores = filteredStores.map(x => (JSON.stringify({
      storeID: x.storeID,
      storeName: x.storeName
    })));

    let groupedStore = [...new Set(newStores)];
    let data = groupedStore.map(x => ({
      value: JSON.parse(x).storeID,
      name: JSON.parse(x).storeName,
      isSelected: false
    }));

    return data;
  }
}

let getStoreIDs = () => {
  let storeIDs = [];
  let selectionValue = $("#selectionControl").val();

  switch (selectionValue) {
    case "ALL":
      storeIDs = [...new Set(getStores().map(x => x.value))];
      break;

    case "R":
      const regionID = $("#regionSelectionControl").val();
      storeIDs = [...new Set(getStores(regionID).map(x => x.value))];
      break;

    case "P":
      const priceGroupID = $("#priceGroupSeclectionControl").val();
      storeIDs = [...new Set(getStores(null, priceGroupID).map(x => x.value))];
      break;

    case "S":
      const storeID = $("#storeSeclectionControl").val();
      storeIDs = [storeID];
      break;

    default:
      break;
  }

  return storeIDs;
}

function renderStoresToView(stores) {
  let storeSelectControl = $("#storeSeclectionControl");
  const controlPlaceHolder = $("#storeControlPlaceHolder").val();
  storeSelectControl.html('');

  renderSelectOptionDefault(storeSelectControl, controlPlaceHolder);
  renderSelectOptions(storeSelectControl, stores);
}

let setUpateAllPolicy = () => {
  let selectionValue = $("#selectionControl").val();
  if (selectionValue === "R") {
    const regionID = $("#regionSelectionControl").val();
    const url = `StockPrice/GetNetworkUpdateSettings?level=${REGION}&value=${regionID}`;

    ajaxGetAsync(url)
      .then(_ => policies.updateAll = _.updateAll)
      .catch(() => policies.updateAll = false);
  }
  else if (selectionValue === "P") {
    const priceGroupID = $("#priceGroupSeclectionControl").val();
    const url = `StockPrice/GetNetworkUpdateSettings?level=${PRICE_GROUP}&value=${priceGroupID}`;

    ajaxGetAsync(url)
      .then(_ => policies.updateAll = _.updateAll)
      .catch(() => policies.updateAll = false);
  }
  else if (selectionValue === "S") {
    policies.updateAll = false;
  }
}

$("#selectionControl").on('change', async function (e) {
  const value = e.target.value;

  $("#regionSelectionControlDiv").hide();
  $("#priceGroupSeclectionControlDiv").hide();
  $("#storeSeclectionControlDiv").hide();

  if (value === "ALL") {
    storeID = "ALL";
  }
  else if (value === "R") {
    $("#regionSelectionControlDiv").show();
    $("#storeSeclectionControlDiv").show();
    renderRegionToView();

    const regionID = $("#regionSelectionControl").val();
    const stores = getStores(regionID);
    renderStoresToView(stores);
    storeID = $("#storeSeclectionControl").val();
  }
  else if (value === "P") {
    $("#priceGroupSeclectionControlDiv").show();
    $("#storeSeclectionControlDiv").show();
    renderPriceGroupToView();

    const priceGroupID = $("#priceGroupSeclectionControl").val();
    const stores = getStores(null, priceGroupID);
    renderStoresToView(stores);
    storeID = $("#storeSeclectionControl").val();
  }
  else if (value === "S") {
    $("#storeSeclectionControlDiv").show();
    let stores = getStores();
    renderStoresToView(stores);
    storeID = $("#storeSeclectionControl").val();
  }
  await loadStockPriceData();
});

$("#regionSelectionControl").on('change', async function (e) {
  const regionID = e.target.value;
  const stores = getStores(regionID);
  renderStoresToView(stores);
  storeID = $("#storeSeclectionControl").val();
  setUpateAllPolicy();
  await loadStockPriceData();
});

$("#priceGroupSeclectionControl").on('change', async function (e) {
  const priceGroupID = e.target.value;
  const stores = getStores(null, priceGroupID);
  renderStoresToView(stores);
  storeID = $("#storeSeclectionControl").val();
  await loadStockPriceData();
});

$("#storeSeclectionControl").on('change', async function (e) {
  storeID = e.target.value;
  await loadStockPriceData();
});
// End of Region, Store, Price Group Section



//Start of Current Or Future Section
async function toggleCurrentOrFuture(element) {
  const priceMode = $(element).val();
  if (priceMode === 'current') {
    stockPriceFormModeCurrent = true;
    hideControls();
    orderNo = 0;
    await loadStockPriceData();
  }
  else {
    $("#cboOrderNoSelectControl").val(1);
    stockPriceFormModeCurrent = false;
    showControls();
    orderNo = $("#cboOrderNoSelectControl").val();
    await InsertFutureValues();
  }
}

$("#cboOrderNoSelectControl").on('change', async function () {
  orderNo = $("#cboOrderNoSelectControl").val();
  await InsertFutureValues();
})

function hideControls() {
  $("#costDateControls").hide();
  $("#priceDateControls").hide();
  $("#cboOrderNoSelectControl").hide();
}

function showControls() {
  $("#costDateControls").show();
  addEventListenerNotToCrossSmallDateTimeLimit('costDateStartControl');
  addEventListenerNotToCrossSmallDateTimeLimit('costDateEndControl');

  $("#priceDateControls").show();
  addEventListenerNotToCrossSmallDateTimeLimit('priceDateStartControl');
  addEventListenerNotToCrossSmallDateTimeLimit('priceDateEndControl');

  $("#cboOrderNoSelectControl").show();
}
//End of Current Or Future Section



//Start Of Vendor Section
function getVendorByID(vendorID) {
  return stockPriceFormData.vendors.filter(x => x.code === vendorID)[0];
}

function renderVendorsToView() {
  let vendors = stockPriceFormData.vendors;
  renderVendorToSelectControl(vendors);
  renderVendorToCheckList(vendors);
  let selectedVendorCode = $("#vendorSelectOptionControl").val();
  let vendor = vendors.filter(x => x.code === selectedVendorCode)[0];
  if (vendor === null || vendor === undefined) {
    vendor = stockPriceFormData.vendors[0];
  }
  addVendorMode = false;
  toggleVendorDeleteAndCancelButton();
  renderVendorFormData(vendor);
}

function toggleVendorDeleteAndCancelButton() {
  if (addVendorMode) {
    $('#cancelVendorButtonControl').removeAttr('hidden');
    $("#deleteVendorButtonControl").hide();
  }
  else {
    $('#cancelVendorButtonControl').attr('hidden', true);
    $("#deleteVendorButtonControl").show();
  }
}

function renderVendorToCheckList(vendors) {
  let data = getVendorsDataToRender(vendors);
  dataForCheckList = data.filter(x => x.value !== "ALL");

  let vendorListSection = $("#stockPriceVendorListSection");
  vendorListSection.html('');
  renderCheckBox(vendorListSection, dataForCheckList);
}

function renderVendorToSelectControl(vendors) {
  let data = getVendorsDataToRender(vendors);
  if (policies.setVendor) {
    data = data.filter(obj => obj.value !== 'ALL');
  }

  let vendorSelectControl = $("#vendorSelectOptionControl");
  let selectPlaceholder = $("#vendorSelectOptionPlaceholder").val();

  vendorSelectControl.html('');
  renderSelectOptionDefault(vendorSelectControl, selectPlaceholder);

  if (addVendorMode && policies.setVendor) {
    $("#vendorSelectOptionControl option:first").prop("disabled", false);
  }

  renderSelectOptions(vendorSelectControl, data);
}

function getVendorsDataToRender(vendors) {
  return vendors
    .map(x => ({
      value: x.code,
      name: x.name,
      isSelected: x.activeVendor
    }))
    .sort((a, b) => a.value - b.value);
}

function renderVendorPart(vendor) {
  if (vendor) {
    $("#stockPriceVendorPartControl").val(vendor.partNo);
  }

  $("#stockPriceVendorPartControl").prop("disabled", !policies.userAccessStockCode);
  $("#updateVendorPartNoButtonControl").prop("disabled", !policies.userAccessStockCode);
}

function validateVendorData(vendor) {
  if (vendor.stockCode === undefined || vendor.stockCode === null || vendor.stockCode.trim() === "") {
    toastr.error(`${document.getElementById("stockCodeNotFound").value}.`);
    return false;
  }
  if (vendor.vendorID === undefined || vendor.vendorID === null || vendor.vendorID.trim() === "") {
    toastr.error(`${document.getElementById("vendorIdNotFound").value}.`);
    return false;
  }
  return true;
}

function getVendorsNotInStockCost() {
  const url = "StockPrice/GetVendorsNotInStockCost";
  let data = { ...getConfigData() };

  ajaxPostAsync(url, data)
    .then(_vendors => renderVendorToSelectControl(_vendors))
    .catch(toastrError);
}

async function stockPriceVendorListSection_Click(e) {
  let vendorID = e.value;
  vendorIDToToggle = vendorID;
  let vendor = stockPriceFormData.vendors.filter(x => x.code === vendorID)[0];

  if (vendor.activeVendor) {
    let title = document.getElementById("activeVendor").value;
    let message = document.getElementById("unSelectActiveVendor").value;

    $(e).prop("checked", true);

    let yesButtonClick = false;
    await showConfirmModal(title, message).then((result) => {
      if (result === true) {
        yesButtonClick = true;
      }
    });

    if (yesButtonClick) {
      VendorInActiveConfirmModalYesButtonOnClick();
    }
  }
  else {
    let title = document.getElementById("activeVendor").value;
    let message = document.getElementById("selectAsActiveVendor").value.replace("{0}", `${vendor.code} - ${vendor.name}`);

    $(e).prop("checked", false);

    let yesButtonClick = false;
    await showConfirmModal(title, message).then((result) => {
      if (result === true) {
        yesButtonClick = true;
      }
    });

    if (yesButtonClick) {
      VendorActiveConfirmModalYesButtonOnClick();
    }
  }

  renderVendorToCheckList(stockPriceFormData.vendors);
}

function toggelVendorStatusAjaxCall(data) {
  const url = "StockPrice/ToggleVendorStatus";

  ajaxPostAsync(url, data)
    .then(_vendors => {
      if (_vendors === null) {
        toastr.error($("failedToLoadVendors").val());
        return;
      }

      vendorIDToToggle = undefined;
      stockPriceFormData.vendors = _vendors;
      if (stockPriceFormData.vendors) {
        renderVendorsToView();
      }
    })
    .catch(() => toastr.error($("#failedToUpdateVendorStatus").val()));
}

async function toggleVendorStatus(vendorID, makeActive) {
  let data = { ...getConfigData() };

  if (vendorID !== null || vendorID !== undefined) {
    data.vendorID = vendorID;
  }

  if (!validateVendorData(data)) {
    return;
  }

  if (!appModeBackOffice) {
    data.storeIDs = getStoreIDs();
    const selection = $("#selectionControl").val();
    if (selection === "ALL") {
      if (!makeActive) {
        let title = document.getElementById("activeVendor").value;
        let message = document.getElementById("doYouWantToUnselectThisActiveVendorForAllTheStores").value;
        await showConfirmModal(title, message).then((result) => {
          if (result === false) {
            data.storeIDs = [];
            data.storeIDs.push("ALL");
            toggelVendorStatusAjaxCall(data);
            return;
          }
          else {
            data.storeIDs.push("ALL");
            toggelVendorStatusAjaxCall(data);
          }
        });
      }
      else {
        data.storeIDs.push("ALL");
        toggelVendorStatusAjaxCall(data);
      }
    }
    else {
      toggelVendorStatusAjaxCall(data);
    }
  }
  else {
    toggelVendorStatusAjaxCall(data);
  }
}

async function addVendorToStockCosts() {
  let data = { ...getConfigData() };
  if (!validateVendorData(data)) {
    return;
  }

  if (policies.setVendor && stockPriceFormData.vendors.length === 0) {
    data.ActiveVendor = true;
  }

  if (!appModeBackOffice) {
    data.storeIDs = getStoreIDs();
    const selectionValue = $("#selectionControl").val();
    if (selectionValue === "ALL") {
      data.storeIDs.push("ALL");
    }
  }

  const url = "StockPrice/AddVendorToStockCost";

  await ajaxPostAsync(url, data)
    .then(_vendors => {
      if (_vendors === null) {
        toastr.error($("failedToLoadVendors").val());
        return;
      }

      stockPriceFormData.vendors = _vendors;
      if (stockPriceFormData.vendors) {
        addVendorMode = false;
        toggleVendorDeleteAndCancelButton();
        renderVendorToSelectControl(stockPriceFormData.vendors);
        renderVendorToCheckList(stockPriceFormData.vendors);
      }
    })
    .catch(toastrError);
}

$("#vendorPartNoClearButtonControl").on('click', function () {
  $("#stockPriceVendorPartControl").val("");
})

$("#cancelVendorButtonControl").on('click', function () {
  renderVendorsToView();
})

$("#vendorSelectOptionControl").on('change', async function (e) {
  let vendorID = e.target.value;
  let stockCostsVendorID = getVendorByID(vendorID);

  if (!stockCostsVendorID) {
    if (orderNo > 0) {
      let vendorExistsInPreviousOrder = previousStockPriceFormData.vendors.filter(x => x.value === vendorID).length > 0;
      if (!vendorExistsInPreviousOrder) {
        if (orderNo === 1) {
          toastr.warning(`You cannot define future ${orderNo}, if you don't have Current for this vendor`);
        }
        else {
          toastr.warning(`You cannot define future ${orderNo}, if you don't have Future ${orderNo - 1} for this vendor`);
        }
        renderVendorsToView();
        return;
      }
    }

    vendorID = 'ALL';
    await addVendorToStockCosts();
    CostUpdateConfirmModalYesButtonOnClick();
  }
  let vendor = getVendorByID(vendorID);
  renderVendorFormData(vendor);
});

$("#addVendorButtonControl").on('click', function () {
  if ($("#vendorSelectOptionControl").val() === 'ALL' && addVendorMode) {
    toastr.warning(document.getElementById("pleaseSelectaVendorToAdd").value);
    return;
  }
  addVendorMode = !addVendorMode;
  toggleVendorDeleteAndCancelButton();
  getVendorsNotInStockCost();
});

function VendorActiveConfirmModalYesButtonOnClick() {
  if (vendorIDToToggle !== null || vendorIDToToggle !== undefined) {
    toggleVendorStatus(vendorIDToToggle, true);
  }
};

function VendorInActiveConfirmModalYesButtonOnClick() {
  if (vendorIDToToggle !== null || vendorIDToToggle !== undefined) {
    toggleVendorStatus(vendorIDToToggle, false);
  }
};

$("#updateVendorPartNoButtonControl").on('click', function () {
  if ($("#vendorSelectOptionControl") !== null && $("#vendorSelectOptionControl").val() === 'ALL') {
    let title = document.getElementById("updateVendorPartNo").value;
    let message = document.getElementById("selectVendorBeforeEnteringVendorPart").value;

    showAlertModal(title, message).then((result) => { });
    return;
  }

  let title = document.getElementById("updateVendorPartNo").value;
  let message = document.getElementById("doYouWantToUpdateVendorPartNo").value;

  showConfirmModal(title, message).then((result) => {
    if (result === true) {
      VendorPartNoUpdateConfirmModalYesButtonClicked();
    }
  });
});

function VendorPartNoUpdateConfirmModalYesButtonClicked() {
  let data = {
    ...getConfigData(),
    partNo: $("#stockPriceVendorPartControl").val()
  };
  if (!validateVendorData(data)) return;

  const url = "StockPrice/UpdateVendorPartNo";

  ajaxPostAsync(url, data)
    .then(_vendors => {
      if (_vendors === null) {
        toastr.error($("failedToLoadVendors").val());
        return;
      }

      stockPriceFormData.vendors = _vendors;
      if (stockPriceFormData.vendors) {
        renderVendorsToView();
      }
    })
    .catch(toastrError);
};

$("#deleteVendorButtonControl").on('click', function () {
  const data = { ...getConfigData() };

  if (data.vendorID && data.vendorID !== "ALL") {
    $("#deleteVendorConfirmModal").modal('show');
  }
});

$("#deleteVendorConfirmModalYesButtonControl").on('click', function () {
  const data = { ...getConfigData() };

  if (!appModeBackOffice) {
    data.storeIDs = getStoreIDs();
    const selectionValue = $("#selectionControl").val();
    if (selectionValue === "ALL") {
      data.storeIDs.push("ALL");
    }
  }

  $("#deleteVendorConfirmModal").modal('hide');
  const url = "StockPrice/DeleteVendor";

  ajaxPostAsync(url, data)
    .then(_vendors => {
      if (_vendors === null) {
        toastr.error($("#failedToLoadVendorsAfterDeleteVendors").val());
        return;
      }

      stockPriceFormData.vendors = _vendors;
      if (stockPriceFormData.vendors) {
        renderVendorsToView()
      }
    })
    .catch(toastrError);
})
//End Of Vendor Section



//Start Of Cost Section
function getCostByVendorID(vendorID) {
  let vendor = stockPriceFormData.stockCosts.filter(x => x.vendorID === vendorID)[0];
  if (vendor === undefined) {
    vendor = stockPriceFormData.stockCosts.filter(x => x.vendorID === "ALL")[0];
  }

  return vendor;
}

function setCostEditButtonText() {
  let editText = $("#costEditButtonText").val();
  let updateText = $("#costUpdateButtonText").val();
  if (costEditMode) {
    $("#editOrUpdateCostCancelButtonDiv").show();
    $("#editOrUpdateCostButtonControl").text(updateText);
    $("#packCost").prop('disabled', false);
    $("#costDateStartControl").prop('disabled', false);
    $("#costDateEndControl").prop('disabled', false);
    $("#costDateEndCheckControl").prop('disabled', false);
    if (!policies.packCost) {
      $("#standardCost").prop("disabled", false);
    }
  }
  else {
    $("#editOrUpdateCostCancelButtonDiv").hide();
    $("#editOrUpdateCostButtonControl").text(editText);
    $("#packCost").prop('disabled', true);
    $("#costDateStartControl").prop('disabled', true);
    $("#costDateEndControl").prop('disabled', true);
    $("#costDateEndCheckControl").prop('disabled', true);
    if (!policies.packCost) {
      $("#standardCost").prop("disabled", true);
    }
  }
}

function renderStockCost(vendorID) {
  // for cost if we don't find any cost for vendor then we will take default cost for the stock.
  let cost;
  costEditMode = false;
  $("#packCost").attr('title', '');
  $("#standardCost").attr('title', '');
  if (vendorID) {
    cost = getCostByVendorID(vendorID);
  }
  setCostEditButtonText();

  if (cost) {
    let date = cost.updateDate === null ? null : getFormattedDate(new Date(cost.updateDate));
    let lastUpdateOn = $("#lastUpdateOn").val();
    $("#lastCostUpdateText").text(date === null ? "" : lastUpdateOn + " " + date);

    let standardCost = (cost.cost === null || cost.cost === undefined) ? 0 : cost.cost;
    let packCost = 0;
    if (policies.packCost && policies.costType === "Standard") {
      packCost = (cost.cost === null || cost.cost === undefined) ? 0 : (cost.cost * stockPriceFormData.packing);
    }

    packCost = +packCost.toFixed(policies.costDec);
    $("#packCost").val(makeFixedDecimalDigits(packCost, policies.costDec));


    standardCost = +standardCost.toFixed(policies.costDec);
    $("#standardCost").val(makeFixedDecimalDigits(standardCost, policies.costDec));

    if (!stockPriceFormModeCurrent) {
      if (cost.startDate !== null || cost.startDate !== undefined || cost.startDate !== '') {
        $("#costDateStartControl").val(moment(cost.startDate).format("YYYY-MM-DD"));
      }
      if (cost.endDate === null || cost.endDate === undefined) {
        $("#costDateEndCheckControl").prop("checked", true);
        $("#costDateEndControl").hide();
      }
      else {
        $("#costDateEndCheckControl").prop("checked", false);
        $("#costDateEndControl").show();
        $("#costDateEndControl").val(moment(cost.endDate).format("YYYY-MM-DD"));
      }
    }
  }
  else {
    $("#packCost").val(0);
    $("#standardCost").val(0);
  }
}

function validateDateRange(isCost) {
  let startControlId = isCost ? "costDateStartControl" : "priceDateStartControl";
  let endCheckControlId = isCost ? "costDateEndCheckControl" : "priceDateEndCheckControl";
  let endControlId = isCost ? "costDateEndControl" : "priceDateEndControl";
  let startWarningId = "effectiveDateStartCanNotBeNullOrEmpty";
  let endWarningId = "effectiveDateEndCanNotBeNullOrEmptyWhenDoesNotEndIsUnchecked";
  let dateCompareWarningId = "startDateCanNotBeGreaterThanEndDate";

  let startDate = $(`#${startControlId}`).val();
  if (startDate === null || startDate === undefined || startDate === '') {
    toastr.warning(`${document.getElementById(startWarningId).value}.`);
    return false;
  }

  if (!$(`#${endCheckControlId}`)[0].checked) {
    let endDate = $(`#${endControlId}`).val();
    if (endDate === null || endDate === undefined || endDate === '') {
      toastr.warning(`${document.getElementById(endWarningId).value}.`);
      return false;
    }

    if (new Date(startDate) > new Date(endDate)) {
      toastr.warning(`${document.getElementById(dateCompareWarningId).value}.`);
      return false;
    }

    if (isCost) {  // Extra date check for cost validation
      if (new Date(startDate) < new Date('1900-01-01')) {
        $(`#${startControlId}`).val('1900-01-01');
        toastr.warning(`${document.getElementById("dateShouldBeGreaterThan").value} 1900-01-01`);
        return false;
      }

      if (new Date(endDate) < new Date('1900-01-01')) {
        $(`#${endControlId}`).val('1900-01-01');
        toastr.warning(`${document.getElementById("dateShouldBeGreaterThan").value} 1900-01-01`);
        return false;
      }
    }
  }

  return true;
}

function validateStockCost() {
  if (!stockPriceFormModeCurrent) {
    return validateDateRange(true);
  }

  return true;
}

function validateStockPrice() {
  if (!stockPriceFormModeCurrent) {
    return validateDateRange(false);
  }

  return true;
}

$("#editOrUpdateCostButtonControl").on("click", async function () {
  editOrUpdateCostButtonControlClicked = true;
  if (costEditMode) {
    if (orderNo > 1) {
      let vendorId = $("#vendorSelectOptionControl").val();
      let startDate = $("#costDateStartControl").val();
      let previousCosts = previousStockPriceFormData.stockCosts.filter(x => x.vendorID === vendorId);
      let previousEndDate;
      if (previousCosts.length) {
        previousEndDate = previousCosts[0].endDate;
      }

      if (previousCosts.length && !previousEndDate) {
        let warmingMessage = document.getElementById("noEndDateForTheOrderNumber").value.replace("()", `${orderNo - 1}`);
        toastr.warning(warmingMessage);
        editOrUpdateCostButtonControlClicked = false;
        $("#cboOrderNoSelectControl").val(orderNo - 1);
        await InsertFutureValues();
        return;
      }
      else if (previousCosts.length) {
        previousEndDate = previousEndDate.substr(0, 10);
      }

      if (startDate && previousEndDate && new Date(startDate) <= new Date(previousEndDate)) {
        toastr.warning(`${document.getElementById("startDateHasToBeAfter").value} ${previousEndDate}.`);
        editOrUpdateCostButtonControlClicked = false;
        return;
      }
    }

    let title = document.getElementById("costUpdateButtonText").value;
    let message = document.getElementById("doYouWantToUpdateCost").value;

    showConfirmModal(title, message).then((result) => {
      if (result === true) {
        if (policies.packCost && $("#packCost").val() === "0") {
          let error = document.getElementById("error").value;
          let inputaNumericValueGreaterThanZero = document.getElementById("inputaNumericValueGreaterThanZero").value;
          showAlertModal(error, inputaNumericValueGreaterThanZero).then((result) => { });
        }
        else {
          CostUpdateConfirmModalYesButtonOnClick();
        }
      }
    });
    if (policies.defPrice) {
      let firstRowData = priceTable.row().data();
      firstRowData.price = $("#standardCost").val();
    }
  }
  else {
    costEditMode = !costEditMode;
    setCostEditButtonText();
  }
  editOrUpdateCostButtonControlClicked = false;
});

function toggleCostEditMode() {
  costEditMode = !costEditMode;
  const vendorID = $("#vendorSelectOptionControl").val();
  renderStockCost(vendorID);
}

$("#costUpdateConfirmModal").on('hidden.bs.modal', function () {
  toggleCostEditMode();
});

$("#editOrUpdateCostCancelButtonControl").on('click', function () {
  toggleCostEditMode();
});

function getActiveVendorCode() {
  let activeVendorCode = stockPriceFormData.vendors.filter(obj => obj.activeVendor === true);
  if (activeVendorCode.length === 0) {
    return null;
  }
  return activeVendorCode[0].code;
}

async function costUpdateAjaxCall(data) {
  const url = "StockPrice/UpdateStockCost";

  await ajaxPostAsync(url, data)
    .then(async _stockCosts => {
      if (_stockCosts === null || _stockCosts === undefined) {
        toastr.error($("#failedToLoadStockCostAfterUpdatingStockCost").val());
        return;
      }

      stockPriceFormData.stockCosts = _stockCosts;
      costEditMode = !costEditMode;
      setCostEditButtonText();
      let vendorId = data.vendorID;
      if ((vendorId === null || vendorId === undefined) && editOrUpdateCostButtonControlClicked) {
        vendorId = "ALL";
      }
      renderStockCost(vendorId);
      let priceRow = priceTable.row(0).data();

      if (priceRow && updatePrice) {
        if (policies.defMargin && policies.confirmPriceChange && priceRow.price !== 0) {

          isPriceChange = true;
          let priceRow = priceTable.row(0).data();

          if (policies.defMargin && policies.confirmPriceChange && priceRow.margin === 0) {
            isMarginChange = true;
          }

          calculatePriceAndMarginBasedOnPolicy();
          await updateStockPriceAndMargin();
        }
      }
    })
    .catch(toastrError);
}

function updateStoreOrVendorDataForHOAndBO(data) {
  if (!appModeBackOffice) {
    const selectionValue = $("#selectionControl").val();
    data.storeIDs = getStoreIDs();
    if (!policies.updateAllStores) {
      data.storeIDs = [];
    }
    if (selectionValue === "ALL" || policies.updateAll) {
      data.storeIDs.push('ALL');
    }
  }
  if (policies.updVends && $("#vendorSelectOptionControl").val() === "ALL") {
    let vendors = getVendorsDataToRender(stockPriceFormData.vendors);
    data.vendorIDs = [...new Set(vendors.map(x => x.value))];
  }
  if (!data.vendorIDs || data.vendorIDs.length === 0) {
    data.vendorIDs = [data.vendorID];
  }
}

async function CostUpdateConfirmModalYesButtonOnClick(data = null) {
  if (data === null) {
    if (!validateStockCost()) return;

    data = {
      ...getConfigData(),
      cost: (+$("#standardCost").val()).toFixed(policies.costDec),
      startDate: null,
      endDate: null
    }

    if (!stockPriceFormModeCurrent) {
      data.startDate = $("#costDateStartControl").val();
      $("#priceDateStartControl").val(data.startDate);
      if (!$("#costDateEndCheckControl")[0].checked) {
        data.endDate = $("#costDateEndControl").val();
        $("#priceDateEndControl").val(data.endDate)
      }
      else {
        $("#priceDateEndCheckControl")[0].checked = true;
      }
    }

    let updatingText = document.getElementById("updating").value;
    $("#editOrUpdateCostButtonControl").html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${updatingText}...`);
    document.getElementById("editOrUpdateCostButtonControl").disabled = true;
    document.getElementById("editOrUpdateCostCancelButtonControl").disabled = true;

    updateStoreOrVendorDataForHOAndBO(data);
  }

  const url = `StockPrice/UpdateStockCost`;
  await ajaxPostAsync(url, data)
    .then(async _stockCosts => {
      if (_stockCosts === null || _stockCosts === undefined) {
        toastr.error($("#failedToLoadStockCostAfterUpdatingStockCost").val());
        return;
      }

      stockPriceFormData.stockCosts = _stockCosts;
      costEditMode = !costEditMode;
      let vendorId = data.vendorID;
      if ((vendorId === null || vendorId === undefined) && editOrUpdateCostButtonControlClicked) {
        vendorId = "ALL";
      }

      let priceRow = priceTable.row(0).data();

      if (priceRow && updatePrice) {
        if (policies.defMargin && policies.confirmPriceChange && priceRow.price !== 0) {
          let title = document.getElementById("updatePrice").value;
          let message = document.getElementById("doYouWantToUpdatePrice").value;

          await showConfirmModal(title, message).then(async (result) => {
            if (result === true) {
              isPriceChange = true;
              let priceRow = priceTable.row(0).data();

              if (policies.defMargin && policies.confirmPriceChange && priceRow.margin === 0) {
                let title = document.getElementById("updateMargin").value;
                let message = document.getElementById("doYouWantToUpdateMargin").value;

                await showConfirmModal(title, message).then(async (result) => {
                  if (result === true) {
                    isMarginChange = true;
                  }
                });
              }
            }
          });
        }

        //always update price whenever cost is updated, like desktop application
        //price update doesn't depend on the confirmation modals above.
        calculatePriceAndMarginBasedOnPolicy();
        await updateStockPriceAndMargin();
      }

      let activeVendorCode = getActiveVendorCode();
      if (policies.updateNetWork && activeVendorCode !== null && activeVendorCode === data.vendorID) {
        data.vendorID = 'ALL';
        data.vendorIDs.push('ALL');
        await costUpdateAjaxCall(data);
      }

      renderStockCost(vendorId);
      document.getElementById("editOrUpdateCostButtonControl").disabled = false;
      document.getElementById("editOrUpdateCostCancelButtonControl").disabled = false;
    })
    .catch(toastrError);

  enableDisableControlBasedOnPolicy();
}

$("#packCost").on("input", function () {
  let packCost = $("#packCost").val();

  if (stockPriceFormData.packing === 0) {
    let standardCost = +packCost.toFixed(policies.costDec);
    $("#standardCost").val(makeFixedDecimalDigits(standardCost, policies.costDec));
  }
  else {
    let standardCost = +(packCost / stockPriceFormData.packing).toFixed(policies.costDec);
    $("#standardCost").val(makeFixedDecimalDigits(standardCost, policies.costDec));
  }
});

$("#costDateEndCheckControl").on('click', function (e) {
  if (e.target.checked) {
    $("#costDateEndControl").hide();
  }
  else {
    $("#costDateEndControl").show();
  }
});
//End Of Cost Section



//Start Of Price Section
function getPriceByVendorID(vendorID) {
  if (appModeBackOffice) {
    return stockPriceFormData.stockPrices.filter(x => x.vendorID === vendorID);
  }
  else {
    return stockPriceFormData.stockPrices.filter(x => x.vendorID === vendorID && x.storeID === storeID);
  }
}

function renderStockPriceToView(vendorID) {
  $("#priceSectionEditControls").hide();
  let stockCostPrices;

  if (vendorID) {
    stockCostPrices = JSON.parse(JSON.stringify(getPriceByVendorID(vendorID)));
  }
  else {
    stockCostPrices = [];
  }
  let prices = generatePricesbasedOnPolicy(stockCostPrices);
  renderPriceToDataTable(prices);

  let price = prices[0];
  if (!stockPriceFormModeCurrent) {
    if (price.startDate !== null || price.startDate !== undefined || price.startDate !== '') {
      $("#priceDateStartControl").val(moment(price.startDate).format("YYYY-MM-DD"));
    }
    if (price.endDate === null || price.endDate === undefined) {
      $("#priceDateEndCheckControl").prop("checked", true);
      $("#priceDateEndControl").hide();
    }
    else {
      $("#priceDateEndCheckControl").prop("checked", false);
      $("#priceDateEndControl").show();
      $("#priceDateEndControl").val(moment(price.endDate).format("YYYY-MM-DD"));
    }
  }
}

function generatePricesbasedOnPolicy(stockCostPrices) {
  let prices;
  if (!stockCostPrices) {
    prices = {
      stockCode: $("#stockCode").val(),
      priceNumber: 1,
      price: 0,
      vendorID: $("#vendorSelectOptionControl").val(),
      startDate: null,
      endDate: null,
      orderNo: orderNo,
      margin: 0,
      updateDate: null
    }
  }
  else {
    prices = stockCostPrices;
  }

  let len = prices.length;
  for (let i = len; i < policies.numberOfPrice; i++) {
    let tempPrice = {
      stockCode: $("#stockCode").val(),
      priceNumber: i + 1,
      price: 0,
      vendorID: $("#vendorSelectOptionControl").val(),
      startDate: null,
      endDate: null,
      orderNo: orderNo,
      margin: 0,
      updateDate: null
    }

    prices.push(tempPrice);
  }

  return prices;
}

function renderPriceToDataTable(prices) {
  if (priceTable) {
    priceTable.clear();
    priceTable.destroy();

  }
  let data = prices;
  let textEdit = $("#textEdit").val();

  let props = createDataTableProp();
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.data = data;
  props.pageLength = 10;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];
  props.columns = [
    { data: 'priceNumber' },
    { data: 'margin' },
    {
      data: 'price',
      render: function (data) {
        return Number(data).toFixed(policies.priceDec);
      }
    },
    {
      data: null,
      defaultContent: `<iconify-icon title="${textEdit}" icon="ep:edit"></iconify-icon>`,
      className: 'stock-price-icon price-row-edit',
      orderable: false
    },
  ];

  priceTable = createDataTableGeneric('stockPriceTable', props);

  let date = data[0].updateDate === null ? null : getFormattedDate(new Date(data[0].updateDate));
  let lastUpdateOn = $("#lastUpdateOn").val();
  $("#lastPriceUpdateText").text(date === null ? "" : lastUpdateOn + " " + date);
  $('#stockPriceTable tbody tr').on('click', 'td.price-row-edit', function (e) {
    e.preventDefault();
    let table = $('#stockPriceTable').DataTable();
    let data = table.row(this).data();
    let index = table.row(this).index();
    priceDataTableRow = data;
    priceDataTableRowIndex = index;
    priceEditMode = true;

    $("#priceSectionEditControls").show();

    $("#stockPriceMargin").val(data.margin);
    $("#stockPricePrice").val(data.price);
  });
}

function calculatePriceAndMarginBasedOnPolicy() {
  let priceRows = priceTable.rows().data();

  if (!priceRows) {
    return;
  }
  let prices = priceRows.toArray();
  const len = prices.length;
  const cost = +$("#standardCost").val();

  // looping through each price row and changing price and margin based on policy.
  for (i = 0; i < len; i++) {
    let priceRow = priceTable.row(i).data();
    let price = priceRow.price;
    let margin = priceRow.margin;
    let updatedPrice;
    let updatedMargin;

    if (policies.defMargin && (price + margin) !== 0) {
      //implement the policy "Default price change is based on margin"
      //change the price based on margin if the user answers Yes and the margin is not 0
      if ((policies.marginCalcCostMkup && isPriceChange && margin !== 0) || !policies.confirmPriceChange) {
        if (policies.marginCalcCostMkup) {
          updatedPrice = cost * (100 + margin) / 100;
        }
        else {
          if (margin !== 100) {
            updatedPrice = cost / (1 - margin / 100);
          }
          else {
            updatedPrice = cost;
          }
        }
      }
      else if ((policies.confirmPriceChange && isPriceChange && margin === 0 && isMarginChange) ||
        (policies.confirmPriceChange && isPriceChange)) {
        //if margin is 0 and price is not 0 recalculate the margin based on user's decision
        if (policies.marginCalcCostMkup) {
          if (cost !== 0) {
            updatedMargin = (price - cost) * 100 / cost;
          }
        }
        else {
          if (price !== 0) {
            updatedMargin = (price - cost) * 100 / price;
          }
        }
      }
    }
    //if price change is based on price don't change the price but recalculate the margin
    else if (price + margin !== 0) {
      if (policies.marginCalcCostMkup) {
        if (cost !== 0) {
          updatedMargin = (price - cost) * 100 / cost;
        }
      }
      else {
        if (price !== 0) {
          updatedMargin = (price - cost) * 100 / price;
        }
      }
    }

    if (updatedMargin !== undefined) {
      priceRow.margin = +updatedMargin.toFixed(policies.priceDec);
    }

    if (updatedPrice !== undefined) {
      priceRow.price = +updatedPrice.toFixed(policies.priceDec);
    }

    priceTable.row(i).data(priceRow).draw();
  }

  if (policies.defPrice) {
    let firstRowData = priceTable.row().data();
    firstRowData.price = $("#standardCost").val();
  }
}

function getPricesFromDataTable() {
  let priceRows = priceTable.rows().data();
  if (!priceRows) {
    return;
  }
  let prices = priceRows.toArray();

  prices.forEach(item => {
    let startDate = moment(item.startDate);
    let endDate = moment(item.endDate);
    if (!stockPriceFormModeCurrent) {
      startDate = moment($("#priceDateStartControl").val());
      if (!$("#priceDateEndCheckControl")[0].checked) {
        endDate = moment($("#priceDateEndControl").val());
      }
      else {
        endDate = null;
        item.endDate = null;
      }
    }

    if (startDate.isValid()) {
      item.startDate = moment(startDate).format("YYYY-MM-DD");
    }

    if (endDate && endDate.isValid()) {
      item.endDate = moment(endDate).format("YYYY-MM-DD");
    }

    let updateDate = moment(item.updateDate);
    if (updateDate.isValid()) {
      item.updateDate = moment(item.updateDate).format("YYYY-MM-DD");
    }

    if (editOrUpdateCostButtonControlClicked && item.vendorID === null) {
      item.vendorID = "ALL"
    }
  });

  return prices;
}

function priceAndMarginUpdateAjaxCall(data) {
  const url = "StockPrice/UpdateStockPrice";

  ajaxPostAsync(url, data)
    .then(_stockPrices => {
      stockPriceFormData.stockPrices = _stockPrices;
      renderStockPriceToView(data.vendorID);
      priceEditMode = false;
    })
    .catch(toastrError);
}

async function updateStockPriceAndMargin(data = null, url = null) {
  if (data === null) {
    if (!validateStockPrice()) return;

    let prices = getPricesFromDataTable();
    if (prices === undefined || prices === null) {
      toastr.error(`${document.getElementById("pricesNotFound").value}.`);
      return;
    }

    data = {
      ...getConfigData(),
      pricesInUse: policies.numberOfPrice,
      prices: prices,
    };

    updateStoreOrVendorDataForHOAndBO(data);

    if (editOrUpdateCostButtonControlClicked && data.vendorID === null) {
      data.vendorID = "ALL"
    }
  }

  let updatingText = document.getElementById("updating").value;
  $("#editPriceButtonControl").html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${updatingText}...`);
  document.getElementById("editPriceButtonControl").disabled = true;
  document.getElementById("cancelPriceButtonControl").disabled = true;

  if (policies.defMargin && data.prices.length) {
    if (data.prices[0].margin >= 1000) {
      data.prices[0].margin /= 1000.00;
    }
  }

  url = url ?? "StockPrice/UpdateStockPrice";

  await ajaxPostAsync(url, data)
    .then(_stockPrices => {
      stockPriceFormData.stockPrices = _stockPrices;
      renderStockPriceToView(data.vendorID);
      priceEditMode = false;

      let updatePriceText = document.getElementById("updatePrice").value;
      $("#editPriceButtonControl").html(`${updatePriceText}`);
      document.getElementById("editPriceButtonControl").disabled = false;
      document.getElementById("cancelPriceButtonControl").disabled = false;
    })
    .catch(toastrError);

  let activeVendorCode = getActiveVendorCode();
  if (policies.updateNetWork && activeVendorCode === data.vendorID) {
    data.vendorID = 'ALL';
    data.vendorIDs.push('ALL');
    data.prices.forEach((item) => {
      item.vendorID = 'ALL';
    })
    priceAndMarginUpdateAjaxCall(data);
  }
}

function updatePriceToDataTable() {
  if (priceDataTableRow) {
    priceDataTableRow.margin = (+$("#stockPriceMargin").val()).toFixed(policies.priceDec);
    priceDataTableRow.price = (+$("#stockPricePrice").val()).toFixed(policies.priceDec);

    if (priceDataTableRowIndex !== undefined) {
      priceTable.row(priceDataTableRowIndex).data(priceDataTableRow).draw();
    }
  }
}

function calculateMargin() {
  let price = +($("#stockPricePrice").val() === undefined || $("#stockPricePrice").val() === null || $("#stockPricePrice").val() === "") ? 0 : (+$("#stockPricePrice").val());
  let cost = +$("#standardCost").val();
  let updatedMargin = 0;
  price = Math.abs(price);

  if (policies.marginCalcCostMkup) {
    if (cost !== 0) {
      updatedMargin = (price - cost) * 100 / cost;
    }
  }
  else {
    if (price !== 0) {
      updatedMargin = (price - cost) * 100 / price;
    }
  }

  updatedMargin = Math.abs(updatedMargin);

  if (updatedMargin !== undefined) {
    $("#stockPriceMargin").val(updatedMargin.toFixed(policies.priceDec));
  }
  updatePriceToDataTable();
}

$("#priceDateEndCheckControl").on('click', function (e) {
  if (e.target.checked) {
    $("#priceDateEndControl").hide();
  }
  else {
    $("#priceDateEndControl").show();
  }
});

$("#priceWithCostUpdateConfirmModalYesButton").on('click', function () {
  isPriceChange = true;
  let priceRow = priceTable.row(0).data();

  if (policies.defMargin && policies.confirmPriceChange && priceRow.margin === 0) {
    $("#marginWithCostUpdateConfirmModal").modal('show');
  }
  else {
    calculatePriceAndMarginBasedOnPolicy();
    updateStockPriceAndMargin();
  }
});

$("#marginWithCostUpdateConfirmModalYesButton").on('click', function () {
  isMarginChange = true;
  calculatePriceAndMarginBasedOnPolicy();
  updateStockPriceAndMargin();
});

$("#priceDateEndCheck").on('click', function (e) {
  $("#priceDateEndControl").prop("disabled", e.target.checked);
})

$("#editPriceButtonControl").on('click', function () {
  updateStockPriceAndMargin();
});

$("#stockPriceMargin").on("input", function (e) {
  let margin = ($("#stockPriceMargin").val() === undefined || $("#stockPriceMargin").val() === null || $("#stockPriceMargin").val() === "") ? 0 : +$("#stockPriceMargin").val();
  if ((!policies.marginCalcCostMkup && margin < 0) || (!policies.marginCalcCostMkup && margin >= 100)) {
    $("#stockPriceMargin").val(0);
    return;
  }
  margin = Math.abs(margin);

  let vendorID = $("#vendorSelectOptionControl").val();
  vendorID = vendorID === null || vendorID === undefined ? "ALL" : vendorID;
  let cost = stockPriceFormData.stockCosts.find(cost => cost.vendorID === vendorID).cost;
  let updatedPrice;

  if (policies.marginCalcCostMkup) {
    updatedPrice = cost * (100 + margin) / 100;
  }
  else {
    updatedPrice = cost / (1 - margin / 100);
  }

  updatedPrice = Math.abs(updatedPrice);

  if (updatedPrice !== undefined) {
    $("#stockPricePrice").val(updatedPrice.toFixed(policies.priceDec));
  }
  updatePriceToDataTable();
});

$("#stockPricePrice").on('input', function (e) {
  let margin = ($("#stockPriceMargin").val() === undefined || $("#stockPriceMargin").val() === null || $("#stockPriceMargin").val() === "") ? 0 : +$("#stockPriceMargin").val();
  if (margin === 0) {
    return;
  }
  calculateMargin();
});

$("#stockPricePrice").on('keypress', function (event) {
  if (policies.allowNegativePrice) {
    return onlyDigitDecimalPlusMinusKeyPress(event);
  }
  return onlyDigitAndDecimalKeyPress(event);
});

$("#stockPriceMargin").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 6) return false;
  return onlyDigitAndDecimalKeyPress(event);
});

$("#stockPricePrice").focusout(function () {
  let margin = ($("#stockPriceMargin").val() === undefined || $("#stockPriceMargin").val() === null || $("#stockPriceMargin").val() === "") ? 0 : +$("#stockPriceMargin").val();
  if (margin === 0) {
    let title = document.getElementById("calculateMargin").value;
    let message = document.getElementById("recalculateTheMarginBasedOnPriceAndCost").value;

    showConfirmModal(title, message).then((result) => {
      if (result === true) {
        calculateMargin();
        updateStockPriceAndMargin();
      }
      else {
        const vendorID = $("#vendorSelectOptionControl").val();
        renderStockPriceToView(vendorID);
        priceEditMode = false;
      }
    });
  }
});

$("#cancelPriceButtonControl").on('click', function () {
  const vendorID = $("#vendorSelectOptionControl").val();
  renderStockPriceToView(vendorID);
  priceEditMode = false;
});

$('#calculateMarginConfirmModal').on('hidden.bs.modal', function () {
  updatePriceToDataTable();
});

//End Of Price Section



//Start Of Rebate Section
function getRebatesByVendorID(vendorID) {
  if (appModeBackOffice) {
    return stockPriceFormData.stockPriceRebates.filter(x => x.vendorID === vendorID);
  }
  else {
    return stockPriceFormData.stockPriceRebates.filter(x => x.vendorID === vendorID && x.storeID === storeID);
  }
}

function renderStockPriceRebate(vendorID) {
  let rebates = getRebatesByVendorID(vendorID);
  renderRebatesToDataTable(rebates);
}

function renderRebatesToDataTable(rebates) {
  if (rebatePriceTable) {
    rebatePriceTable.clear();
    rebatePriceTable.destroy();
  }

  let data = rebates;
  const textDelete = $("#textDelete").val();

  let props = createDataTableProp();
  props.paging = false;
  props.pageLength = 10;
  props.searching = false;
  props.bInfo = false;
  props.data = data;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4] }
  ];
  props.columns = [
    { data: 'rebateNo' },
    { data: 'rebate' },
    {
      data: 'startDate',
      render: function (data) {
        return moment(data).format("YYYY-MM-DD");
      }
    },
    {
      data: 'endDate',
      render: function (data) {
        return moment(data).format("YYYY-MM-DD");
      }
    },
    {
      data: null,
      defaultContent: `<iconify-icon title="${textDelete}" icon="ep:delete"></iconify-icon>`,
      className: 'stock-price-icon rebate-row-delete',
      orderable: false
    },
  ];

  rebatePriceTable = createDataTableGeneric('stockPriceRebateTable', props);

  $('#stockPriceRebateTable tbody tr').on('click', 'td.rebate-row-delete', function (e) {
    e.preventDefault();
    let table = $('#stockPriceRebateTable').DataTable();
    let data = table.row(this).data();

    rebateToDelete = data;
    let title = document.getElementById("rebateText").value;
    let message = document.getElementById("doYouWantToDeleteRebate").value;

    showConfirmModal(title, message).then((result) => {
      if (result === true) {
        RebateDeleteConfirmModalYesButtonControlOnClick();
      }
    });
  });
}

function validateDateForRebateAndSpecialPrice(data) {
  if (data.startDate === undefined || data.startDate === null || data.startDate === '') {
    toastr.warning(`${document.getElementById("startDateCanNotBeNull").value}.`);
    return false;
  }

  if (data.endDate === undefined || data.endDate === null || data.endDate === '') {
    toastr.warning(`${document.getElementById("endDateCanNotBeNull").value}.`);
    return false;
  }

  if (new Date(data.startDate) > new Date(data.endDate)) {
    toastr.warning(`${document.getElementById("startDataCanNotBeGreaterThanEndDate").value}.`);
    return false;
  }

  return true;
}

function validateRebate() {
  let data = getRebateFormData();
  if (data.rebate === undefined || data.rebate === null || data.rebate === 0) {
    toastr.warning(`${document.getElementById("rebateCantbeNullorZero").value}.`);
    return false;
  }

  return validateDateForRebateAndSpecialPrice(data);
}

function getRebateFormData() {
  return {
    rebate: +$("#rebate").val(),
    startDate: $("#rebateStartDate").val(),
    endDate: $("#rebateEndDate").val()
  };
}

function clearRebateForm() {
  let today = new Date();
  document.getElementById('rebate').value = '0.00';
  let rebateStartDate = document.getElementById('rebateStartDate');
  let rebateEndDate = document.getElementById('rebateEndDate');

  rebateStartDate.addEventListener('input', function () {
    if (!rebateStartDate.value) {
      rebateStartDate.value = new Date().toISOString().substr(0, 10);
    }
  });

  rebateEndDate.addEventListener('input', function () {
    if (!rebateEndDate.value) {
      rebateEndDate.value = new Date().toISOString().substr(0, 10);
    }
  });

  rebateStartDate.value = today.toISOString().substr(0, 10);
  rebateEndDate.value = today.toISOString().substr(0, 10);
}

function RebateDeleteConfirmModalYesButtonControlOnClick() {
  let data = {
    rebateNo: rebateToDelete.rebateNo,
    ...getConfigData()
  };
  if (data === null || data === undefined) {
    toastr.error(`${document.getElementById("someErrorOccuredRefreshAndTryAgain").value}.`);
    return;
  }

  if (!appModeBackOffice) {
    const selectionValue = $("#selectionControl").val();
    if (selectionValue === "ALL" || policies.updateAll) {
      data.storeIDs = ["ALL"]
    }
    else {
      data.storeIDs = getStoreIDs();
      if (policies.updateAll) {
        data.storeIDs.push('ALL');
      }
    }
  }

  const url = "StockPrice/DeleteRebate";

  ajaxPostAsync(url, data)
    .then(_stockPriceRebates => {
      stockPriceFormData.stockPriceRebates = _stockPriceRebates;
      rebateToDelete = undefined;
      renderStockPriceRebate(data.vendorID);
    })
    .catch(toastrError);
};

$("#addRebateButton").on('click', function () {
  let vendorID = $("#vendorSelectOptionControl").val();
  if (vendorID === "ALL" || vendorID === undefined || vendorID === null) {
    toastr.warning(`${document.getElementById("pleaseSelectaVendorToInsertReabte").value}.`);
    return;
  }

  $("#addRebateModal").modal('show');
  addEventListenerNotToCrossSmallDateTimeLimit('rebateStartDate');
  addEventListenerNotToCrossSmallDateTimeLimit('rebateEndDate');
});

$("#stockPriceRebateModalSaveButtonControl").on('click', function () {
  if (!validateRebate()) return;
  let data = {
    ...getConfigData(),
    ...getRebateFormData()
  }

  if (!appModeBackOffice) {
    const selectionValue = $("#selectionControl").val();
    data.storeIDs = getStoreIDs();
    if (!policies.updateAllStores) {
      data.storeIDs = [];
    }
    if (selectionValue === "ALL" || policies.updateAll) {
      data.storeIDs.push("ALL");
    }
  }
  const url = "StockPrice/AddRebate";

  ajaxPostAsync(url, data)
    .then(_stockPriceRebates => {
      stockPriceFormData.stockPriceRebates = _stockPriceRebates;
      clearRebateForm();
      $("#addRebateModal").modal('hide');
      renderStockPriceRebate(data.vendorID);
    })
    .catch(toastrError);
});
//End Of Rebate Section



//Start Of Special Price Section
function getSpecialPriceByVendorID(vendorID) {
  if (appModeBackOffice) {
    return stockPriceFormData.specialPrice.filter(x => x.vendorID === vendorID);
  }
  else {
    if (!appModeBackOffice && policies.useArinovices) {
      return stockPriceFormData.specialPrice;
    }
    return stockPriceFormData.specialPrice.filter(x => x.vendorID === vendorID && x.storeID === storeID);
  }
}

function renderSpecialPrice(vendorID) {
  let specialPrices = getSpecialPriceByVendorID(vendorID);
  if (!appModeBackOffice && policies.useArinovices
    && stockPriceFormData.specialPriceFromPriceL
    && stockPriceFormData.specialPriceFromPriceL.length > 0) {
    $("#specialPriceTypeSelectControl").val(stockPriceFormData.specialPriceFromPriceL[0].priceType);
  }
  else {
    $("#specialPriceTypeSelectControl").val(!policies.useArinovices && specialPrices && specialPrices.length > 0 ? specialPrices[0].priceType : "R");
  }

  let currentPriceType = (specialPrices !== undefined && specialPrices.length > 0) ? specialPrices[0]?.priceType : null;
  toggleAddSpecialPriceButton(currentPriceType);
  hideAndShowControlBasedOnPriceType();
  createSpecialPriceDataTable(specialPrices);
}

function createSpecialPriceDataTable(specialPrices) {
  let priceType;
  priceType = $("#specialPriceTypeSelectControl").val();
  let showFormQty = ((priceType === 'X' || priceType === 'F' || priceType === 'I' || priceType === 'D') ? true : false);
  let showToQty = ((priceType === 'I') ? true : false);

  if (specialPriceTable) {
    specialPriceTable.clear();
    specialPriceTable.destroy();
  }

  let priceTypeForFilter = priceType;
  if (!appModeBackOffice && policies.useArinovices && priceType !== 'R') {
    priceTypeForFilter = $("#priceTypeForUseARInvoiceOne")[0].checked ? $("#priceTypeForUseARInvoiceOne").val() : $("#priceTypeForUseARInvoiceTwo").val()
  }

  let data = specialPrices.filter(x => x.priceType === priceTypeForFilter);
  const textDelete = $("#textDelete").val();

  let props = createDataTableProp();
  props.paging = false;
  props.pageLength = 10;
  props.searching = false;
  props.bInfo = false;
  props.data = data;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5] }
  ];
  props.columns = [
    { data: 'priceFromQuantity', "visible": showFormQty },
    { data: 'priceToQuantity', "visible": showToQty },
    {
      data: 'price',
      render: function (data) {
        return makeFixedDecimalDigits(Number(data), 4);
      }
    },
    {
      data: 'startDate',
      render: function (data) {
        return moment(data).format("YYYY-MM-DD");
      },
      "visible": !policies.useArinovices
    },
    {
      data: 'endDate',
      render: function (data) {
        return moment(data).format("YYYY-MM-DD");
      },
      "visible": !policies.useArinovices
    },
    {
      data: null,
      defaultContent: `<iconify-icon title="${textDelete}" icon="ep:edit"></iconify-icon>`,
      className: 'stock-price-edit special-price-row-edit',
      orderable: false,
      "visible": (!appModeBackOffice && policies.useArinovices)
    },
    {
      data: null,
      defaultContent: `<iconify-icon title="${textDelete}" icon="ep:delete"></iconify-icon>`,
      className: 'text-center stock-price-icon special-price-row-delete',
      orderable: false
    },
  ];

  specialPriceTable = createDataTableGeneric('specialPriceTable', props);

  specialPriceTable.on('draw', () => {
    priceType = $("#specialPriceTypeSelectControl").val();
    if (!appModeBackOffice && priceType === 'D' && policies.useArinovices) {
      let specialPriceFromTitle = $("#specialPriceFrom");
      if (specialPriceFromTitle && specialPriceFromTitle[0]) {
        specialPriceFromTitle[0].innerText = "Code";
      }
    }
    else {
      let specialPriceTableFromTitle = document.getElementById("specialPriceFrom");
      if (specialPriceTableFromTitle) {
        specialPriceTableFromTitle.innerText = document.getElementById("from").value;
      }
    }
  });

  $("#editSpecialPriceSaveButtonConrol").on('click', function (e) {
    UpdateSpecialPriceData();
  })

  $('#specialPriceTable tbody tr').on('click', 'td.special-price-row-edit', function (e) {
    e.preventDefault();
    let table = $('#specialPriceTable').DataTable();
    let data = table.row(this).data();
    specialPriceToEdit = data;
    $("#specialPriceCode").val(specialPriceToEdit.priceFromQuantity);
    $("#specialPricePrice").val(specialPriceToEdit.price);
    $("#editSpecialPriceModal").modal('show');
  });

  $('#specialPriceTable tbody tr').on('click', 'td.special-price-row-delete', function (e) {
    e.preventDefault();
    let table = $('#specialPriceTable').DataTable();
    let data = table.row(this).data();

    specialPriceToDelete = data;
    let title = document.getElementById("specialPrices").value;
    let message = document.getElementById("doYouWantToDeleteSpecialPrice").value;

    showConfirmModal(title, message).then((result) => {
      if (result === true) {
        SpecialPriceDeleteConfirmModalYesButtonControlOnClick();
      }
    });
  });
}

function toggleAddSpecialPriceButton(currentPriceType = null) {
  let priceType = $("#specialPriceTypeSelectControl").val();
  if (priceType === undefined || priceType === null || priceType === '' || priceType === 'R' || (priceType === currentPriceType && priceType === 'S') || (policies.setVendor && $("#vendorSelectOptionControl").val() === null)) {
    $("#addSpecialPriceButton").prop("disabled", true);
  }
  else {
    $("#addSpecialPriceButton").prop("disabled", false);
  }
}

function showSpecialPriceDeleteConfirmModal() {
  $("#specialPriceDeleteConfirmModal").modal('show');
};

let hasPlusUpdateSpecialPrice = 0;
let lastCharCodeUpdateSpecialPrice = 0;
$("#specialPricePrice").on('keypress', function (event) {
  const value = $(this).val();
  hasPlusUpdateSpecialPrice = (value.length === 0 && lastCharCodeUpdateSpecialPrice !== 43) ? (event.charCode === 43 ? 1 : 0) : hasPlusUpdateSpecialPrice;
  lastCharCodeUpdateSpecialPrice = event.charCode;
  if (hasPlusUpdateSpecialPrice + value.length >= 6) return false;
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

function hideAndShowControlBasedOnPriceType() {
  let priceType = $("#specialPriceTypeSelectControl").val();
  if (priceType === undefined || priceType === null || priceType === '' || priceType === 'R') return;

  $("#specialPriceForm").show();
  $("#specialPriceTo").show();
  $("#specialPrice").show();

  if (priceType === 'S') {
    $("#specialPriceForm").hide();
    $("#specialPriceTo").hide();
  }
  else if (priceType === "F" || priceType === "X") {
    $("#specialPriceTo").hide();
    let quantity = document.getElementById("quantity").value;
    document.getElementById("specialPriceFormLable").innerText = quantity;
    $('#specialPriceFormControl').attr('placeholder', quantity);
  }
  else {
    let from = document.getElementById("from").value;
    document.getElementById("specialPriceFormLable").innerText = from;
    $('#specialPriceFormControl').attr('placeholder', from);
  }

  $('#specialPriceForm').unbind("keypress");
  $('#specialPriceTo').unbind("keypress");
  $("#specialPriceFormControl").on('keypress', function (event) {
    const value = $(this).val();
    if (value.length >= 6 || (!appModeBackOffice && policies.useArinovices && value.length >= 1)) return false;
    return onlyDigitAndDecimalKeyPress(event);
  });

  $("#specialPriceToControl").on('keypress', function (event) {
    const value = $(this).val();
    if (value.length >= 6) return false;
    return onlyDigitAndDecimalKeyPress(event);
  });

  $('#specialPriceControl').unbind("keypress");
  if (priceType === 'S' || priceType === 'I') {
    $("#specialPriceControl").on('keypress', function (event) {
      const value = $(this).val();
      if (value.length >= 6 || (!appModeBackOffice && policies.useArinovices && value.length >= 1 && priceType === 'S')) return false;
      return onlyDigitAndDecimalKeyPress(event);
    });
  }
  else {
    let hasPlusAddSpecialPrice = 0;
    let lastCharCodeAddSpecialPrice = 0;
    $("#specialPriceControl").on('keypress', function (event) {
      const value = $(this).val();
      hasPlusAddSpecialPrice = (value.length === 0 && lastCharCodeAddSpecialPrice !== 43) ? (event.charCode === 43 ? 1 : 0) : hasPlusAddSpecialPrice;
      lastCharCodeAddSpecialPrice = event.charCode;
      if (hasPlusAddSpecialPrice + value.length >= 6) return false;
      return onlyDigitDecimalPlusMinusKeyPress(event);
    });
  }
}

function validateSpecialPrice() {
  let priceType = $("#specialPriceTypeSelectControl").val();
  let data = getSpecialPriceFormData();
  if (data.price === undefined || isNaN(parseFloat(data.price)) || parseFloat(data.price) === 0) {
    toastr.warning(`${document.getElementById("specialPriceNotFoundOrCanNotBeZero").value}.`);
    return false;
  }

  if (priceType === "F" || priceType === "X") {
    if (data.priceFromQuantity === undefined || isNaN(parseFloat(data.priceFromQuantity)) || parseFloat(data.priceFromQuantity) === 0) {
      toastr.warning(`${document.getElementById("specialFromQuantityNotFoundOrCanNotBeZero").value}.`);
      return false;
    }
  }

  if (priceType === "I") {
    if (data.priceToQuantity === undefined || isNaN(parseFloat(data.priceToQuantity)) || parseFloat(data.priceToQuantity) === 0) {
      toastr.warning(`${document.getElementById("specialToQuantityNotFoundOrCanNotBeZero").value}.`);
      return false;
    }
  }

  return validateDateForRebateAndSpecialPrice(data);
}

function getSpecialPriceFormData() {
  let priceUnits = $("#priceUnitDollar")[0].checked ? $("#priceUnitDollar").val() : $("#priceUnitPercent").val();
  let price = ($("#specialPriceControl").val() === undefined || $("#specialPriceControl").val() === null ||
    isNaN($("#specialPriceControl").val()) || $("#specialPriceControl").val() === 'NaN')
    ? 0 : +$("#specialPriceControl").val();

  let priceFromQuantity = ($("#specialPriceFormControl").val() === undefined || $("#specialPriceFormControl").val() === null ||
    isNaN($("#specialPriceFormControl").val()) || $("#specialPriceFormControl").val() === 'NaN')
    ? 0 : +$("#specialPriceFormControl").val();

  let priceToQuantity = ($("#specialPriceToControl").val() === undefined || $("#specialPriceToControl").val() === null ||
    isNaN($("#specialPriceToControl").val()) || $("#specialPriceToControl").val() === 'NaN')
    ? 0 : +$("#specialPriceToControl").val();

  return {
    priceUnits: priceUnits,
    price: price,
    priceFromQuantity: priceFromQuantity,
    priceToQuantity: priceToQuantity,
    startDate: $("#specialPriceStartDate").val(),
    endDate: $("#specialPriceEndDate").val()
  };
}

function clearSpecialPriceForm() {
  $("#priceUnitDollar")[0].checked = true;
  $("#specialPriceControl").val('');
  $("#specialPriceFormControl").val('');
  $("#specialPriceToControl").val('');
  $("#specialPriceStartDate").val('');
  $("#specialPriceEndDate").val('');
}

function renderPromotionInformation() {
  let id = stockPriceFormData.promotion.promotionId;
  let date = stockPriceFormData.promotion.endDate;

  if (id && date) {
    date = moment(date).format("MM/DD/YYYY");
    const promoText = `Part of promo ID ${id} <br> Ending on ${date}`;

    $("#promoText").html(promoText);
  }
}

function UpdateSpecialPriceData() {
  specialPriceToEdit.price = document.getElementById("specialPricePrice").value;

  let data = {
    ...getConfigData(),
    ...specialPriceToEdit
  };

  if (data === undefined || data === null) {
    toastr.error(`${document.getElementById("someErrorOccuredRefreshAndTryAgain").value}.`);
    return;
  }

  if (data.price >= 214748) {
    toastr.warning(`${document.getElementById("specialPriceCannotBeGreaterThan214748").value}.`);
    return;
  }

  $("#editSpecialPriceModal").modal('hide');
  const url = "StockPrice/UpdateSpecialPriceForUseARInvoicePolicy";

  ajaxPostAsync(url, data)
    .then(_specialPrice => {
      stockPriceFormData.specialPrice = _specialPrice;
      clearSpecialPriceForm();
      specialPriceToEdit = undefined;
      renderSpecialPrice(data.vendorID);
    })
    .catch(() => toastr.error($("#failedToUpdateStockSpecialPrice").val()));
}

function SpecialPriceDeleteConfirmModalYesButtonControlOnClick() {
  let priceType = $("#specialPriceTypeSelectControl").val();

  let data = {
    ...getConfigData(),
    ...specialPriceToDelete
  };

  if (!appModeBackOffice && policies.useArinovices && priceType !== 'D') {
    $("#specialPriceTypeSelectControl").val('D');
    clearSpecialPriceForm();
    specialPriceToDelete = undefined;
    renderSpecialPrice(data.vendorID);
    return;
  }

  if (!appModeBackOffice) {
    const selectionValue = $("#selectionControl").val();
    data.storeIDs = getStoreIDs();
    if (!policies.updateAllStores) {
      data.storeIDs = [];
    }
    if (selectionValue === "ALL" || policies.updateAll) {
      data.storeIDs.push('ALL');
    }
  }

  if (data === undefined || data === null) {
    toastr.error($("#someErrorOccuredRefreshAndTryAgain").val());
    return;
  }

  const url = "StockPrice/DeleteSpecialPrice";

  ajaxPostAsync(url, data)
    .then(_specialPrice => {
      stockPriceFormData.specialPrice = _specialPrice;
      clearSpecialPriceForm();
      specialPriceToDelete = undefined;
      renderSpecialPrice(data.vendorID);
    })
    .catch(() => toastr.error($("#failedToDeleteStockSpecialPrice").val()));
}

function handlePriceTypeChange() {
  let vendorID = $("#vendorSelectOptionControl").val();
  let specialPrices = getSpecialPriceByVendorID(vendorID);
  let currentPriceType = (specialPrices !== undefined && specialPrices.length > 0) ? specialPrices[0]?.priceType : null;

  toggleAddSpecialPriceButton(currentPriceType);
  hideAndShowControlBasedOnPriceType();
  createSpecialPriceDataTable(specialPrices);
}

$("#priceTypeForUseARInvoiceOne").on('change', handlePriceTypeChange);
$("#priceTypeForUseARInvoiceTwo").on('change', handlePriceTypeChange);
$("#specialPriceTypeSelectControl").on('change', handlePriceTypeChange);

$("#addSpecialPriceButton").on('click', function () {
  clearSpecialPriceForm();

  if ($("#specialPriceTypeSelectControl").val() === 'D') {
    document.getElementById("specialPriceFormLable").innerText = 'Code';
    document.getElementById("specialPriceFormControl").placeholder = "Code";
    $("#specialPriceTo").hide();
    $("#effectiveDate").hide();
  }

  $("#addSpecialPriceModal").modal('show');

  if (!appModeBackOffice && policies.useArinovices) {
    if ($("#priceTypeForUseARInvoiceOne")[0].checked) {
      $("#priceUnitDollar")[0].checked = true;
    }
    else {
      $("#priceUnitPercent")[0].checked = true;
    }
  }

  addEventListenerNotToCrossSmallDateTimeLimit('specialPriceStartDate');
  addEventListenerNotToCrossSmallDateTimeLimit('specialPriceEndDate');
});

$("#addSpecialPriceSaveButtonConrol").on('click', function () {
  if (!validateSpecialPrice()) return;

  let data = {
    ...getConfigData(),
    priceType: $("#specialPriceTypeSelectControl").val(),
    ...getSpecialPriceFormData()
  };
  data.priceFromQuantity = Math.round(data.priceFromQuantity);

  if (data.price >= 214748) {
    toastr.warning(`${document.getElementById("specialPriceCannotBeGreaterThan214748").value}.`);
    return;
  }

  if (!appModeBackOffice) {
    const selectionValue = $("#selectionControl").val();
    data.storeIDs = getStoreIDs();
    if (!policies.updateAllStores) {
      data.storeIDs = [];
    }
    if (selectionValue === "ALL" || policies.updateAll) {
      data.storeIDs.push('ALL');
    }
  }

  if (!appModeBackOffice && policies.useArinovices && data.priceType === 'S') {
    data.priceFromQuantity = data.price;
    data.price = 0;
  }

  let filteredSpecialPrices = stockPriceFormData.specialPrice.filter(
    price => price.priceFromQuantity === data.priceFromQuantity && price.priceType === data.priceUnits);

  if (!appModeBackOffice && policies.useArinovices && filteredSpecialPrices.length) {
    $("#addSpecialPriceModal").modal('hide');
    let title = "C-Store Commander Head Office";
    let message = "Duplicated Code, please type a different code.";

    showAlertModal(title, message).then((result) => { });
    return;
  }
  const url = "StockPrice/AddSpecialPrice";

  ajaxPostAsync(url, data)
    .then(_specialPrice => {
      stockPriceFormData.specialPrice = _specialPrice;
      $("#addSpecialPriceModal").modal('hide');
      renderSpecialPrice(data.vendorID)
    })
    .catch(toastrError);
});

let goBackToTheStockItemForm = () => {
  _globalIsEditing = addVendorMode || costEditMode || priceEditMode;

  let price = $('#stockPriceTable tbody tr:first').find('td:eq(2)').text();
  let standardCost = $('#standardCost').val();
  if (appModeBackOffice && policies.warn0Price && Number(price) === 0 && Number(standardCost) !== 0) {
    let title = document.getElementById("zeroPrice").value;
    let message = `${document.getElementById("theSellingPriceIs0").value}!`;

    showConfirmModal(title, message).then((result) => {
      if (result === true) {
        const url = `StockItemForm?stockCodeValue=${stockCode}&isUpdateProduct=true`;
        safelyNavigateTo(url);
      }
    });
    return;
  }

  const url = `StockItemForm?stockCodeValue=${stockCode}&isUpdateProduct=true`;
  safelyNavigateTo(url);
}
//End Of Special Price Section

//format date to mm/dd/year
function getFormattedDate(date) {
  return (((date.getMonth() > 8) ? (date.getMonth() + 1) : ('0' + (date.getMonth() + 1))) + '/' + ((date.getDate() > 9) ? date.getDate() : ('0' + date.getDate())) + '/' + date.getFullYear());
}

let InsertFutureValues = async () => {
  if (stockPriceFormModeCurrent) return;

  orderNo = $("#cboOrderNoSelectControl").val() - 1;
  await loadStockPriceData();

  previousStockPriceFormData = stockPriceFormData;
  let today = new Date();
  orderNo = orderNo + 1;
  await loadStockPriceData();

  // Insert future cost
  if (previousStockPriceFormData.stockCosts.length === 0) {
    toastr.warning(`There are no values for the cost defined for order number ${orderNo - 1}. Please define those values and then define the future values.`);
    return;
  }

  if (stockPriceFormData.stockCosts.length === 0) {
    let stockCosts = previousStockPriceFormData.stockCosts;

    for (let i = 0; i < stockCosts.length; i++) {
      if (stockCosts[i].startDate === null) stockCosts[i].startDate = today.toISOString().substr(0, 10);
      if (stockCosts[i].endDate === null) stockCosts[i].endDate = today.toISOString().substr(0, 10);
      stockCosts[i].storeID = "ALL";
      stockCosts[i].orderNo = orderNo;
    }
    let stockCostsList = {
      Costs: stockCosts,
      storeIDs: null
    };

    if (!appModeBackOffice) {
      $("#selectionControl").val("ALL");
      const selectionValue = $("#selectionControl").val();
      stockCostsList.storeIDs = getStoreIDs();
      if (!policies.updateAllStores) {
        stockCostsList.storeIDs = [];
      }
      if (selectionValue === "ALL" || policies.updateAll) {
        stockCostsList.storeIDs.push('ALL');
      }
    }

    const url = "StockPrice/InsertFutureStockCosts";

    await ajaxPostAsync(url, stockCostsList)
      .then(async () => {
        enableDisableControlBasedOnPolicy();
        await reInitializeStockPriceData();
      })
      .catch(toastrError);
  }

  // Insert future price
  if (previousStockPriceFormData.stockPrices.length === 0) {
    toastr.warning(`There are no values for the price defined for order number ${orderNo - 1}. Please define those values and then define the future values.`);
    return;
  }

  if (stockPriceFormData.stockPrices.length === 0) {
    let prices = previousStockPriceFormData.stockPrices;

    for (let i = 0; i < prices.length; i++) {
      let price = prices[i];
      if (price.startDate === null) price.startDate = today.toISOString().substr(0, 10);
      if (price.endDate === null && !$("#priceDateEndCheckControl")[0].checked) price.endDate = today.toISOString().substr(0, 10);
      price.updateDate = today.toISOString().substr(0, 10);
      price.orderNo = orderNo;

      if (price.storeIDs === null) price.storeIDs = [];
      price.storeIDs.push(price.storeID);

      if (price.vendorIDs === null) price.vendorIDs = [];
      price.vendorIDs.push("ALL");
      prices[i] = price;
    }
    let data = {
      ...getConfigData(),
      pricesInUse: policies.numberOfPrice,
      prices: prices,
    };

    if (!appModeBackOffice) {
      $("#selectionControl").val("ALL");
      const selectionValue = $("#selectionControl").val();
      data.storeIDs = getStoreIDs();
      if (!policies.updateAllStores) {
        data.storeIDs = [];
      }
      if (selectionValue === "ALL" || policies.updateAll) {
        data.storeIDs.push('ALL');
      }
    }

    data.vendorID = "ALL";
    let vendors = getVendorsDataToRender(previousStockPriceFormData.vendors);
    if (vendors !== null) {
      data.vendorIDs = [...new Set(vendors.map(x => x.value))];
    }
    else {
      data.vendorIDs = [data.vendorID];
    }

    let url = "StockPrice/InsertFuturePrice";
    await updateStockPriceAndMargin(data, url);
    renderStockPriceToView(data.vendorID);
  }
}
