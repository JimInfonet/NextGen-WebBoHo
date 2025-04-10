let stockCode;
let appModeBackOffice;
let policies;
let allRegions;
let allStores;
let chargesAndDepositsTable;
let taxesTable;
let chargesAndDepositsToDelete;
let chargesAndDepositsToDeleteIndex;
let taxesToDelete;
let taxesToDeleteIndex;
let stockItemDetailsFormData;
let datatable;
const taxType = [];
let availabilityEditMode = false;

let taxExemptPolicy = document.getElementById("tax-exempt-policy").value;
let taxExemptTypePolicy = document.getElementById("tax-exempt-type-policy").value;
let taxExemptByRatePolicy = document.getElementById("tax-exempt-by-rate-policy").value;
let isAddMode = document.getElementById("is-add-mode").value;

let sunday = document.getElementById("sunday").value;
let monday = document.getElementById("monday").value;
let tuesday = document.getElementById("tuesday").value;
let wednesday = document.getElementById("wednesday").value;
let thursday = document.getElementById("thursday").value;
let friday = document.getElementById("friday").value;
let saturday = document.getElementById("saturday").value;
let days = [sunday, monday, tuesday, wednesday, thursday, friday, saturday];

$(async function (event) {
  if (!initStockItemDetailsConfiguration()) return;
  await loadPolicy();

  setTimeout(async () => {
    await getFormData();
  }, 1000);
  createDatatable();

  let isBackOffice = document.getElementById("appModeBackOffice").value;
  if (isBackOffice === "True" && taxExemptPolicy === "Yes" && taxExemptTypePolicy !== "QITE") {
    await loadTaxExemptCategoryOptions();
    await loadTaxExemptVendorOptions();

    if (taxExemptByRatePolicy === "Yes" && taxExemptTypePolicy === "AITE") {
      await loadTaxExemptTaxTypeOptions();
    }

    if (taxExemptByRatePolicy === "Yes" && taxExemptTypePolicy === "AITE" && isAddMode == "False") {
      await loadTaxExemptPrice();
    }

    loadTaxExemptDataFromModel();
    setVisibilityTaxExemptFormInput();
  }
});

let initStockItemDetailsConfiguration = async () => {
  stockCode = $("#stockCode").val().trim();
  if (stockCode === "" || stockCode === null || stockCode === undefined) {
    let message = document.getElementById("stockCodeNotFound").value;
    toastr.warning(message);
    return false;
  }

  await getFormData();
  $("#stockItemDescription").text(`${stockItemDetailsFormData.stockItemName} - (${stockCode})`);

  $("#cancelAvailabilityButtonControlDiv").hide();
  appModeBackOffice = $("#appModeBackOffice").val().trim().toLowerCase() === "true" ? true : false;
  if (!appModeBackOffice) {
    hideOrShowSelectionControls("ALL");
    await loadRegion();
    await loadStore();
  }
}

let loadPolicy = async () => {
  const url = "StockItemDetails/GetStockItemDetailsPolicy";

  await ajaxGetAsync(url)
    .then(_ => policies = _)
    .catch(toastrError);
}

let loadStore = async () => {
  const url = "Store/GetStoresWithRegionAndPriceGroup";

  await ajaxGetAsync(url)
    .then(_ => allStores = _)
    .catch(() => toastr.error($("#failedToLoadStore").val()));
}

let loadRegion = async () => {
  const url = "Region/GetRegion";

  await ajaxGetAsync(url)
    .then(_ => allRegions = _)
    .catch(() => toastr.error($("#failedToLoadRegion").val()));
}

$("#selectionControl").on('change', async (event) => {
  const value = event.target.value;
  hideOrShowSelectionControls(value);
  await getFormData();
});

let hideOrShowSelectionControls = (value) => {
  $("#regionSelectionControlDiv").hide();
  $("#storeSeclectionControlDiv").hide();

  if (value === "R") {
    $("#regionSelectionControlDiv").show();
    $("#storeSeclectionControlDiv").show();
    renderRegionToView();
  }
  else if (value === "S") {
    $("#storeSeclectionControlDiv").show();
    let stores = getStoresByRegionID();
    renderStoreToView(stores);
  }
}

$("#regionSelectionControl").on('change', async (event) => {
  const regionID = event.target.value;
  let filteredStores = getStoresByRegionID(regionID);
  renderStoreToView(filteredStores);
  await getFormData();
})

let renderRegionToView = () => {
  let data = allRegions.map((x, i) => ({
    value: x.id,
    name: x.name,
    isSelected: (i === 0)
  }));
  let control = $("#regionSelectionControl");
  const text = $("#regionSelectionControlPlaceHolder").val();

  control.html('');
  renderSelectOptionDefault(control, text);
  renderSelectOptions(control, data);

  let regionID = $("#regionSelectionControl").val();
  let filteredStores = getStoresByRegionID(regionID);
  renderStoreToView(filteredStores);
}

let getStoresByRegionID = (regionID = null) => {
  let newStores;
  if (regionID === null || regionID === undefined) {
    newStores = allStores.map(x => (JSON.stringify({
      storeID: x.storeID,
      storeName: x.storeName
    })));
  }
  else {
    let filteredStores = allStores.filter(x => x.regionID === regionID);
    newStores = filteredStores.map(x => (JSON.stringify({
      storeID: x.storeID,
      storeName: x.storeName
    })));
  }

  let groupedStore = [...new Set(newStores)];
  let data = groupedStore.map((x, i) => ({
    value: JSON.parse(x).storeID,
    name: JSON.parse(x).storeName,
    isSelected: (i === 0)
  }));

  return data;
}

$("#storeSeclectionControl").on('change', async (event) => {
  await getFormData();
})

let renderStoreToView = (data) => {
  let control = $("#storeSeclectionControl");
  const text = $("#storeSeclectionControlPlaceHolder").val();

  control.html('');
  renderSelectOptionDefault(control, text);
  renderSelectOptions(control, data);
}

let getConfigData = () => {
  let data = { stockCode: stockCode };

  if (!appModeBackOffice) {
    const selectedSelection = $("#selectionControl").val();
    if (selectedSelection === "ALL") {
      data.storeID = "ALL";
    }
    else {
      data.storeID = $("#storeSeclectionControl").val();
    }
  }
  return data;
}

let getFormData = async () => {
  const url = "StockItemDetails/GetStockItemDetails";
  const data = { ...getConfigData() };

  await ajaxPostAsync(url, data)
    .then(_stockItemDetails => {
      stockItemDetailsFormData = _stockItemDetails;
      renderStockItemDetaisFormData();
    })
    .catch(() => toastr.error($("#failedToLoadStockItemDetails").val()));
}

let renderStockItemDetaisFormData = () => {
  $("#chargesSelectionControlDiv").hide();
  $("#taxesSelectionControlDiv").hide();
  $("#addChargesButtonControl").prop("disabled", false);
  $("#addTaxesButtonControl").prop("disabled", false);

  renderChargesAndDeposits();
  renderTaxes();
  renderAvailabilityByDay();
}

let renderChargesAndDeposits = () => {
  renderChargesAndDepositesDataTable();

  let len = stockItemDetailsFormData.charges.length;
  let isDisabled = len < stockItemDetailsFormData.totalActiveCharges && policies.userAccessStockCode;
  $("#addChargesButtonControl").prop("disabled", !isDisabled);
}

let renderChargesAndDepositesDataTable = () => {
  if (chargesAndDepositsTable) {
    chargesAndDepositsTable.clear();
    chargesAndDepositsTable.destroy();
  }

  let textDelete = $("#textDelete").val();

  let data = stockItemDetailsFormData.charges;
  let props = createDataTableProp();
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.data = data;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];
  props.columns = [
    { data: 'charge' },
    { data: 'description' },
    { data: 'price' },
    {
      data: null,
      defaultContent: `<iconify-icon title="${textDelete}" icon="ep:delete"></iconify-icon>`,
      className: `table-icon ${(policies.userAccessStockCode ? "charges-and-deposits-delete" : "")}`,
      orderable: false
    },
  ];

  chargesAndDepositsTable = createDataTableGeneric('chargesAndDepositsDataTable', props);

  $('#chargesAndDepositsDataTable tbody tr').on('click', 'td.charges-and-deposits-delete', function (e) {
    e.preventDefault();
    let table = $('#chargesAndDepositsDataTable').DataTable();
    let data = table.row(this).data();
    let index = table.row(this).index();
    chargesAndDepositsToDelete = data;
    chargesAndDepositsToDeleteIndex = index;

    let title = document.getElementById("chargesAndDeposits").value;
    let message = document.getElementById("areYouSureYouWantToDeleteThisEntry").value;

    showConfirmModal(title, message).then(async (result) => {
      if (result === true) {
        let data = {
          stockCode: chargesAndDepositsToDelete.stockCode,
          chargeCode: chargesAndDepositsToDelete.charge,
        }
        if (!appModeBackOffice) { data.storeIDs = getRegionStores() }
        const url = "StockItemDetails/DeleteCharge";

        await ajaxPostAsync(url, data)
          .then(async () => getFormData())
          .catch(() => toastr.error($("#failedToDeleteCharges").val()));
      }
    });
  });
}

let renderTaxes = () => {
  renderTaxesDataTable();

  let len = stockItemDetailsFormData.taxes.length;
  let isDisabled = len < stockItemDetailsFormData.totalActiveTaxes && policies.userAccessStockCode;
  $("#addTaxesButtonControl").prop("disabled", !isDisabled);
}

let renderTaxesDataTable = () => {
  if (taxesTable) {
    taxesTable.clear();
    taxesTable.destroy();
  }

  let textDelete = $("#textDelete").val();

  let data = stockItemDetailsFormData.taxes;
  let props = createDataTableProp();
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.data = data;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4] }
  ];
  props.columns = [
    { data: 'taxName' },
    { data: 'taxCode' },
    { data: 'rate' },
    {
      data: 'included',
      render: function (data) {
        if (data) {
          return "Yes";
        }
        else {
          return "No";
        }
      }
    },
    {
      data: null,
      defaultContent: `<iconify-icon title="${textDelete}" icon="ep:delete"></iconify-icon>`,
      className: `table-icon ${(policies.userAccessStockCode ? "taxes-delete" : "")}`,
      orderable: false
    },
  ];

  taxesTable = createDataTableGeneric('taxesDataTable', props);

  $('#taxesDataTable tbody tr').on('click', 'td.taxes-delete', function (e) {
    e.preventDefault();
    let table = $('#taxesDataTable').DataTable();
    let data = table.row(this).data();
    let index = table.row(this).index();
    taxesToDelete = data;
    taxesToDeleteIndex = index;

    let title = document.getElementById("deleteTax").value;
    let message = document.getElementById("areYouSureYouWantToDeleteThisEntry").value;

    showConfirmModal(title, message).then(async (result) => {
      if (result === true) {
        let data = {
          stockCode: taxesToDelete.stockCode,
          taxCode: taxesToDelete.taxCode,
          taxName: taxesToDelete.taxName,
        }

        if (!appModeBackOffice) { data.storeIDs = getRegionStores() }
        const url = "StockItemDetails/DeleteTaxes";

        await ajaxPostAsync(url, data)
          .then(async () => getFormData())
          .catch(() => $("#failedToDeleteTaxes").val());
      }
    });
  });
}

let getRegionStores = () => {
  let storeArray = [];
  const selectedSelection = $("#selectionControl").val();
  if (selectedSelection === "R") {
    const regionID = $("#regionSelectionControl").val();
    let storeIDs = getStoresByRegionID(regionID).map(x => x.value).filter(x => x !== null || x !== undefined);
    storeArray = [...new Set(storeIDs)];
  }
  else if (selectedSelection === "S") {
    const storeID = $("#storeSeclectionControl").val();
    storeArray.push(storeID);
  }
  else if (selectedSelection === "ALL") {
    let ids = allStores.map(x => x.storeID).filter(x => x !== null || x !== undefined);
    storeArray = [...new Set(ids)];
    storeArray.push("ALL");
  }
  return storeArray;
}

$("#addChargesButtonControl").on('click', function () {
  const data = { stockCode: stockCode };

  if (!appModeBackOffice) {
    const selectedSelection = $("#selectionControl").val();
    if (selectedSelection === "ALL") {
      data.storeIDs = ["ALL"];
    }
    else {
      data.storeIDs = getRegionStores();
    }
  }

  const url = "StockItemDetails/GetAssocNotInStockCodeAc";

  ajaxPostAsync(url, data)
    .then(_assocs => {
      $("#chargesSelectionControlDiv").show();
      $("#addChargesButtonControl").prop("disabled", true);
      renderChargesIntoSelectControl(_assocs);
    })
    .catch(() => toastr.error($("#failedToLoadData").val()));
});

let renderChargesIntoSelectControl = (charges) => {
  let data = charges.map((x, i) => ({
    value: x.code,
    name: x.description,
    isSelected: false
  }));
  let control = $("#chargesSelectionControl");
  const text = $("#chargesSelectionControlPlaceHolder").val();

  control.html('');
  renderSelectOptionDefault(control, text, true);
  renderSelectOptions(control, data);
}

$("#chargesSelectionControl").on('change', function (event) {
  let data = {
    chargeCode: event.target.value,
    stockCode: stockCode,
    selectionValue: $("#selectionControl").val(),
  }
  if (!appModeBackOffice) { data.storeIDs = getRegionStores() }
  const url = "StockItemDetails/InsertCharge";

  ajaxPostAsync(url, data)
    .then(() => getFormData())
    .catch(() => toastr.error($("#failedToLoadData").val()));
});

function createDatatable() {
  document.getElementById("pluCode").value = null;
  if (datatable) {
    datatable.clear();
    datatable.destroy();
  }

  let props = createDataTableProp();
  props.searching = false;
  props.paging = false;
  props.bInfo = false;
  props.ajax = {
    url: "/StockItemDetails/GetPLUCodesByStockCode",
    data: { stockCode: document.getElementById("stockCode").value },
    dataSrc: d => d,
  };
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "pluCode" }
  ];

  datatable = createDataTableGeneric('pluCodes', props);

  $('#pluCodes tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      let selectedPLUCode = $(this)[0].childNodes[0].innerText;

      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        document.getElementById("pluCode").value = null;
      } else {
        datatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        document.getElementById("pluCode").value = selectedPLUCode;
      }
    });
}

let showError = (message) => {
  toastr.error(message);
}

let setAlternateCodeDeleteConfirmModalText = () => {
  let selectedPLUCode = document.getElementById("pluCode").value;
  let emptyMessage = document.getElementById("datatableEmptyMessage").value;

  if (selectedPLUCode === null || selectedPLUCode === "" || selectedPLUCode === emptyMessage) {
    let title = document.getElementById("alternateCodeDelete").value;
    let message = document.getElementById("selectAlternateCodeBeforeDelete").value;

    showAlertModal(title, message).then((result) => { });
  }
  else {
    let title = document.getElementById("alternateCodeDelete").value;
    let message = document.getElementById("confirmDeleteAlert").value;

    showConfirmModal(title, message).then((result) => {
      if (result === true) {
        let selectedPLUCode = document.getElementById("pluCode").value;
        let url = "/StockItemDetails/DeletePLUMastByPLUCode";

        ajaxGetAsync(url, { pluCode: selectedPLUCode })
          .then(() => createDatatable())
          .catch(toastrError);
      }
    });
  }
}

let addPLUCode = () => {
  let stockCode = document.getElementById("stockCode").value;
  let alternateCode = document.getElementById("alternate-code").value;

  if (alternateCode === null || alternateCode === "") {
    let message = document.getElementById("enterAnAlternateCode").value;
    toastr.warning(message);
    return;
  }

  let url = "/StockItemDetails/AddPLUCodeToPLUMast";

  ajaxGetAsync(url, { pluPrim: stockCode, pluCode: alternateCode })
    .then(() => {
      $('#addPLUCodeModal').modal('hide');
      createDatatable();
      document.getElementById("alternate-code").value = null;
    })
    .catch(error => {
      if (error.status === HttpStatusCode.Conflict) {
        let _message = $("#alternateCodeExist").val().replace("__PLU_CODE__", alternateCode);
        toastr.warning(_message);
        return;
      }
      toastr.error($("#_defaultErrorMessage").val());
    });
}

let cancelButtonClicked = () => {
  document.getElementById("alternate-code").value = null;
}

$("#addTaxesButtonControl").on('click', function () {
  let data = {
    stockCode: stockCode
  }

  if (!appModeBackOffice) {
    const selectedSelection = $("#selectionControl").val();
    if (selectedSelection === "ALL") {
      data.storeIDs = ["ALL"];
    }
    else {
      data.storeIDs = getRegionStores();
    }
  }

  const url = "StockItemDetails/GetTaxesAndCode";

  ajaxPostAsync(url, data)
    .then(_taxes => {
      $("#taxesSelectionControlDiv").show();
      $("#addTaxesButtonControl").prop("disabled", true);
      renderTaxesIntoSelectControl(_taxes);
    })
    .catch(toastrError);
});

let renderTaxesIntoSelectControl = (taxes) => {
  let data = taxes.map((x, i) => ({
    value: `${x.taxName} - ${x.taxCode}`,
    name: "",
    isSelected: false
  }));
  let control = $("#taxesSelectionControl");
  const text = $("#taxesSelectionControlPlaceHolder").val();

  control.html('');
  renderSelectOptionDefault(control, text, true);
  renderSelectOptions(control, data);
}

$("#taxesSelectionControl").on('change', function (event) {
  const value = event.target.value;
  const codeAndName = value.split("-");
  let data = {
    taxCode: codeAndName[1].trim(),
    taxName: codeAndName[0].trim(),
    stockCode: stockCode
  }
  if (!appModeBackOffice) { data.storeIDs = getRegionStores() }
  const url = "StockItemDetails/InsertTaxes";

  ajaxPostAsync(url, data)
    .then(() => getFormData())
    .catch(toastrError);
});

let renderAvailabilityByDay = () => {
  const availability = stockItemDetailsFormData.availabilityByDays;
  const data = availability.map(x => ({
    name: days[x.dayOfWeek - 1],
    value: x.dayOfWeek,
    isSelected: x.isAvailable
  }));

  let control = $("#availabilityByDayCheckBoxDiv");
  control.html('');
  renderCheckBox(control, data, true);
  renderAvailabilityButtonText();
}

let renderAvailabilityButtonText = () => {
  const editText = $("#editButtonControlText").val();
  const updateText = $("#updateButtonControlText").val();
  $("#updateAvailabilityButtonControl").text('');
  let allCheckedControl = $(`#availabilityByDayCheckBoxDiv input[type="checkbox"]`);

  if (availabilityEditMode) {
    $("#updateAvailabilityButtonControl").text(updateText);
    $("#cancelAvailabilityButtonControlDiv").show();

    for (let control of allCheckedControl) {
      control.disabled = false;
    };
  }
  else {
    $("#updateAvailabilityButtonControl").text(editText);
    $("#cancelAvailabilityButtonControlDiv").hide();

    for (let control of allCheckedControl) {
      control.disabled = true;
    };
  }
}

let updateAvailability = () => {
  let allCheckedValue = $(`#availabilityByDayCheckBoxDiv input[type="checkbox"]:checked`);
  let dayArray = [];
  for (let item of allCheckedValue) {
    dayArray.push({
      stockCode: stockCode,
      dayOfWeek: +item.value
    });
  }
  const url = "StockItemDetails/InsertStockAvailabilityByDay";

  ajaxPostAsync(url, { availabilityByDays: dayArray, stockCode: stockCode })
    .then(_availabilityByDays => {
      stockItemDetailsFormData.availabilityByDays = _availabilityByDays;
      availabilityEditMode = false;
      renderAvailabilityByDay();
    })
    .catch(() => toastr.error($("#failedToLoadAvailabilityByDay").val()));
}

$("#cancelAvailabilityButtonControl").on('click', function () {
  availabilityEditMode = !availabilityEditMode;
  renderAvailabilityByDay();
});

$("#updateAvailabilityButtonControl").on('click', function () {
  if (availabilityEditMode) {
    updateAvailability();
  }
  else {
    availabilityEditMode = !availabilityEditMode;
    renderAvailabilityButtonText();
  }
})

$("#stockDetailsCancelButtonControl").on('click', function () {
  const url = `StockItemForm?stockCodeValue=${stockCode}&isUpdateProduct=true`;
  safelyNavigateTo(url);
})

let loadDataFromStockItemDetailsController = async (url, data) => {
  url = "/StockItemDetails/" + url;

  return ajaxGetAsync(url, data)
    .catch(toastrError);
}

let loadTaxExemptCategoryOptions = async () => {
  $("#tax-exempt-category-select").html(optionGeneratorDefault(document.getElementById("taxExemptCategory").value));
  await loadDataFromStockItemDetailsController('GetAllTaxExemptCategory', {}).then((data) => {
    data = JSON.parse(data);
    for (let i = 0; i < data.length; i++) {
      $("#tax-exempt-category-select").append(optionGenerator(data[i].id, data[i].name));
    }
  });
}

let loadTaxExemptVendorOptions = async () => {
  $("#vendor-select").html(optionGeneratorDefault(document.getElementById("vendor").value));
  await loadDataFromStockItemDetailsController('GetAllTaxExemptVendorsByStockCode', { stockCode: document.getElementById("stockCode").value }).then((data) => {
    data = JSON.parse(data);
    for (let i = 0; i < data.length; i++) {
      $("#vendor-select").append(optionGenerator(data[i].vendorId, data[i].vendorName));
    }
  });
}

let loadTaxExemptTaxTypeOptions = async () => {
  $("#tax-type-select").html(optionGeneratorDefault(document.getElementById("taxType").value));
  await loadDataFromStockItemDetailsController('GetAllTaxExemptRate', {}).then((data) => {
    data = JSON.parse(data);
    for (let i = 0; i < data.length; i++) {
      $("#tax-type-select").append(optionGenerator(data[i].taxCode, data[i].taxDescirption));
      taxType[data[i].taxCode] = data[i].productCode;
    }
  });
}

let loadTaxExemptPrice = async () => {
  if (document.getElementById("is-add-mode").value === "True") {
    $("#tax-exemept-price-field").addClass("hidden");
    return;
  }

  let StockCode = document.getElementById("stockCode").value;
  let TaxCode = document.getElementById("tax-type-select-model").value;
  let TaxExemptCategory = document.getElementById("tax-exempt-category-select-model").value;
  let QuantityPerPkg = document.getElementById("quantity-per-pkg").value;

  let url = "/StockItemDetails/CalculateTaxExemptPrice";
  let data = {
    stockCode: StockCode,
    taxCode: TaxCode,
    taxExemptCategory: TaxExemptCategory,
    quantityPerPkg: QuantityPerPkg
  };

  return ajaxGetAsync(url, data)
    .then(_taxExemptPrice => $("#tax-exempt-price-data").innerText = _taxExemptPrice)
    .catch(toastrError);
}

let loadSelectFromModel = (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) !== null && document.getElementById(inputDataId).value === "0") {
    document.getElementById(inputDataId).value = "";
  }

  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId}`).val(document.getElementById(inputDataId).value);
  }
  else {
    $(`#${selectId}`).val("");
  }
}

let loadInputDataFromModel = (inputId, modelInputDataId) => {
  if (document.getElementById(inputId) !== null && document.getElementById(modelInputDataId) !== null) {
    document.getElementById(inputId).value = document.getElementById(modelInputDataId).value
  }
}

let loadTaxExemptDataFromModel = async () => {
  loadSelectFromModel("tax-exempt-category-select-model", "tax-exempt-category-select");
  loadSelectFromModel("vendor-select-model", "vendor-select");
  loadSelectFromModel("tax-type-select-model", "tax-type-select");
}

function showDeleteTaxExemptModal() {
  if (document.getElementById("is-add-mode").value === "True") {
    let title = document.getElementById("deleteTaxExempt").value;
    let message = document.getElementById("noTaxExemptToDelete").value;

    showAlertModal(title, message).then((result) => { });
  }
  else {
    let title = document.getElementById("deleteTaxExempt").value;
    let message = document.getElementById("deleteTaxExemptWarning").value;

    showConfirmModal(title, message).then(async (result) => {
      if (result) {
        let stockCode = $("#stockCode").val();
        let url = "/StockItemDetails/DeleteProductTaxExemptByProductKey";

        await ajaxGetAsync(url, { productKey: stockCode })
          .then(() => {
            const url = `/StockItemDetails?stockCode=${stockCode}`;
            safelyNavigateTo(url);
          })
          .catch(toastrError);
      }
    });
  }
}

let resetTaxExemptDataFromModel = async () => {
  loadSelectFromModel("tax-exempt-category-select-model", "tax-exempt-category-select");
  loadSelectFromModel("vendor-select-model", "vendor-select");
  loadSelectFromModel("tax-type-select-model", "tax-type-select");

  loadInputDataFromModel("tax-exempt-price", "tax-free-price-model");
  loadInputDataFromModel("equivalent-units", "units-per-pkg-model");
  loadInputDataFromModel("quantity-per-pkg", "quantity-per-pkg-model");
  loadInputDataFromModel("base-unit-quantity", "base-unit-qty-model");
  loadInputDataFromModel("product-code", "product-code-model");
}

function loadTaxExemptProductCode() {
  let taxTypeValue = document.getElementById("tax-type-select").value;
  if (taxTypeValue != null && taxType[taxTypeValue] != null) {
    document.getElementById("product-code").value = taxType[taxTypeValue];
  }
  else {
    document.getElementById("product-code").value = null;
  }
}

function calculateEquivalentUnits() {
  let quantityPerPkg = document.getElementById("quantity-per-pkg").value;
  let baseUnitQuantity = document.getElementById("base-unit-quantity").value;
  let equivalentUnits = quantityPerPkg * baseUnitQuantity;
  if (equivalentUnits !== 0) {
    document.getElementById("equivalent-units").value = equivalentUnits;
  }
  else {
    document.getElementById("equivalent-units").value = "";
  }
}

let setVisibilityTaxExemptFormInput = () => {
  let taxExemptFormInputs = document.getElementsByClassName("tax-exempt-form-input");
  let editModeEnabled = document.getElementById("edit-mode").value === "true";

  if (editModeEnabled) {
    $("#btn-cancel").show();
  }
  else {
    $("#btn-cancel").hide();
    resetTaxExemptDataFromModel();
  }

  if (taxExemptFormInputs) {
    for (let i = 0; i < taxExemptFormInputs.length; i++) {
      taxExemptFormInputs[i].disabled = !editModeEnabled;
    }
  }

  document.getElementById("btn-apply").hidden = !editModeEnabled;
  document.getElementById("btn-edit").hidden = editModeEnabled;
  document.getElementById("edit-mode").value = (document.getElementById("edit-mode").value === "true") ? "false" : "true";
}

function goBackToStockItemForm() {
  let editModeEnabled = document.getElementById("edit-mode").value === "true";
  _globalIsEditing = availabilityEditMode || (!editModeEnabled && document.getElementById("tax-exempt-setting"));
  setGlobalEditingState(_globalIsEditing);

  const url = `StockItemForm?stockCodeValue=${stockCode}&isUpdateProduct=true`;
  safelyNavigateTo(url);
}
