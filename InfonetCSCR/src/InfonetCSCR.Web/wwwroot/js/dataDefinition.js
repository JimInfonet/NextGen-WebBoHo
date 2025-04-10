let storeOptions = [];
let vendorOptions = [];
let regionOptions = [];
let priceGroupOptions = [];
let taxMasterOptions = [];
let associationOptions = [];

let selectedStoreIds = [];
let selectedRegionIds = [];
let selectedVendorIds = [];
let selectedPriceGroupIds = [];

let renderedSelectedStoreIds = [];
let selectedProducts = [];
let selectedDataDefinitionCodes = [];
let currentDataDefinitionType = "TAXES";

let dataTableProduct = null;
let loading = document.getElementById("loading").value;

let products;
let lastSelectedStoreId;
let lastSelectedVendorId;
let lastSelectedProductId;

$(async function () {
  await loadStores().then(() => renderStoreOptions());
  await loadVendors();
  await loadRegions();
  await loadPriceGroups();
  await loadTaxMasters().then(() => renderTaxMasterOptions());
  await loadGetAssociations();
  if (storeOptions.length > 0) {
    $(`#store-${storeOptions[0].store_ID}`).prop("checked", true);
    await handleSelectedStoresChange(storeOptions[0].store_ID);
  }
  renderProducts();
});

function showByOptionChanged() {
  document.getElementById("select-or-unselect-all-stores").checked = false;
  const showByOption = $('input[name="show-by"]:checked').val();
  let selectedStoresContainers = $("#selected-stores-container");
  selectedStoresContainers.empty();
  selectedProducts = [];

  if (showByOption === "by-store") {
    selectedStoreIds = [];
    renderStoreOptions();
    if (storeOptions && storeOptions.length > 0) {
      $(`#store-${storeOptions[0].store_ID}`).prop("checked", true);
      handleSelectedStoresChange(storeOptions[0].store_ID);
    }
    $("#available-options-label").text($("#byStore").val());
  }
  else if (showByOption === "by-region") {
    renderRegionOptions();
    selectedRegionIds = [];
    if (regionOptions && regionOptions.length > 0) {
      $(`#region-${regionOptions[0].id}`).prop("checked", true);
      handleSelectedRegionsChange(regionOptions[0].id);
    }
    $("#available-options-label").text($("#byRegion").val());
  }
  else if (showByOption === "by-price-group") {
    renderPriceGroupOptions();
    selectedPriceGroupIds = [];
    if (priceGroupOptions && priceGroupOptions.length > 0) {
      $(`#price-group-${priceGroupOptions[0].id}`).prop("checked", true);
      handleSelectedPriceGroupsChange(priceGroupOptions[0].id);
    }
    $("#available-options-label").text($("#byPriceGroup").val());
  }
  else if (showByOption === "by-vendor") {
    renderVendorOptions();
    selectedVendorIds = [];
    if (vendorOptions && vendorOptions.length > 0) {
      $(`#vendor-${vendorOptions[0].code}`).prop("checked", true);
      handleSelectedVendorsChange(vendorOptions[0].code);
    }
    $("#vendor-products-btn").show();
    $("#available-options-label").text($("#byVendor").val());
  }

  renderProducts();
}

async function loadStores() {
  const url = "/DataDefinition/GetStoresData";

  await ajaxGetAsync(url)
    .then(stores => {
      storeOptions = stores
      lastSelectedStoreId = storeOptions.length > 0 ? storeOptions[0].store_ID : null;
    })
    .catch(toastrError);
}

async function loadVendors() {
  let url = "/DataDefinition/GetVendorsData";

  await ajaxGetAsync(url)
    .then((vendors) => vendorOptions = vendors)
    .catch(toastrError);
}

async function loadRegions() {
  const url = "/DataDefinition/GetRegionsData";

  await ajaxGetAsync(url)
    .then((regions) => regionOptions = regions)
    .catch(toastrError);
}

async function loadPriceGroups() {
  const url = "/DataDefinition/GetPriceGroupsData";

  await ajaxGetAsync(url)
    .then((priceGroups) => priceGroupOptions = priceGroups)
    .catch(toastrError);
}

async function loadTaxMasters() {
  const url = "/DataDefinition/GetTaxMastersData";

  await ajaxGetAsync(url)
    .then((taxMasters) => taxMasterOptions = taxMasters)
    .catch(toastrError);
}

async function loadGetAssociations() {
  const url = "/DataDefinition/GetAssociations";

  await ajaxGetAsync(url)
    .then((associations) => associationOptions = associations)
    .catch(toastrError);
}

function renderSelectedStoresOnSelectionTab(selectedStoresOnSelectionTab) {
  let selectedStoresContainers = $("#selected-stores-container");
  selectedStoresContainers.empty();

  renderedSelectedStoreIds = [];
  selectedStoresOnSelectionTab.forEach(store => {
    selectedStoresContainers.append(`
        <div class="my-1">${store.store_ID} -- ${store.store_Desc}</div>
    `);
    renderedSelectedStoreIds.push(store.store_ID);
  })
}

let handleSelectOrUnselectAll = async () => {
  let selectedStoresContainers = $("#selected-stores-container");
  selectedStoresContainers.empty();
  selectedStoreIds = [];

  const showByOption = $('input[name="show-by"]:checked').val();
  let isChecked = document.getElementById("select-or-unselect-all-stores").checked;
  if (showByOption === "by-store") {
    if (isChecked) {
      storeOptions.forEach(function (store) {
        $(`#store-${store.store_ID}`).prop("checked", true);
        handleSelectedStoresChange(store.store_ID);
      });
    }
    else {
      storeOptions.forEach(function (store) {
        $(`#store-${store.store_ID}`).prop("checked", false);
        handleSelectedStoresChange(store.store_ID);
      });

      if (storeOptions && storeOptions.length > 0) {
        $(`#store-${storeOptions[0].store_ID}`).prop("checked", true);
        handleSelectedStoresChange(storeOptions[0].store_ID);
      }
    }
  }
  else if (showByOption === "by-region") {
    if (isChecked) {
      regionOptions.forEach(function (region) {
        $(`#region-${region.id}`).prop("checked", true);
        handleSelectedRegionsChange(region.id);
      });
    }
    else {
      regionOptions.forEach(function (region) {
        $(`#region-${region.id}`).prop("checked", false);
        handleSelectedRegionsChange(region.id);
      });

      if (regionOptions && regionOptions.length > 0) {
        $(`#region-${regionOptions[0].id}`).prop("checked", true);
        handleSelectedRegionsChange(regionOptions[0].id);
      }
    }
  }
  else if (showByOption === "by-price-group") {
    if (isChecked) {
      priceGroupOptions.forEach(function (priceGroup) {
        $(`#price-group-${priceGroup.id}`).prop("checked", true);
        handleSelectedPriceGroupsChange(priceGroup.id);
      });
    }
    else {
      priceGroupOptions.forEach(function (priceGroup) {
        $(`#price-group-${priceGroup.id}`).prop("checked", false);
        handleSelectedPriceGroupsChange(priceGroup.id);
      });

      if (priceGroupOptions && priceGroupOptions.length > 0) {
        $(`#price-group-${priceGroupOptions[0].id}`).prop("checked", true);
        handleSelectedPriceGroupsChange(priceGroupOptions[0].id);
      }
    }
  }
  else if (showByOption === "by-vendor") {
    if (isChecked) {
      vendorOptions.forEach(function (vendor) {
        $(`#vendor-${vendor.code}`).prop("checked", true);
        handleSelectedVendorsChange(vendor.code);
      });
    }
    else {
      vendorOptions.forEach(function (vendor) {
        $(`#vendor-${vendor.code}`).prop("checked", false);
        handleSelectedVendorsChange(vendor.code);
      });

      if (vendorOptions && vendorOptions.length > 0) {
        $(`#vendor-${vendorOptions[0].code}`).prop("checked", true);
        handleSelectedVendorsChange(vendorOptions[0].code);
      }
    }
  }
}

let changeCurrentRowSelect = (isChecked, currentRow) => {
  let checkBox = $(currentRow).find('input.form-check-input');
  if (checkBox && checkBox[0]) {
    checkBox[0].checked = isChecked;
  }
}

let handleSelectOrUnselectAllProducts = () => {
  let isChecked = $("#view__select-or-unselect-all-products").is(":checked");
  dataTableProduct.on('draw.dt', async function () {
    let tbody = await $("#table--products").find('tbody');
    tbody.find('tr').each(function () {
      changeCurrentRowSelect(isChecked, $(this)[0]);
    });
  });

  let tbody = $("#table--products").find('tbody');
  tbody.find('tr').each(function () {
    changeCurrentRowSelect(isChecked, $(this)[0]);
  });
}

async function handleSelectedStoresChange(storeId) {
  lastSelectedStoreId = storeId;

  const isChecked = $(`#store-${storeId}`).is(":checked");
  if (isChecked) {
    selectedStoreIds.push(storeId);
  }
  else {
    selectedStoreIds = selectedStoreIds.filter(function (value) {
      return value !== storeId;
    });
  }

  const url = "/DataDefinition/GetStoresByStoreIds";

  await ajaxPostAsync(url, selectedStoreIds)
    .then(stores => {
      if (stores.length > 0) lastSelectedStoreId = stores[0].store_ID;
      renderSelectedStoresOnSelectionTab(stores);
    })
    .catch((error) => {
      toastr.error(error);
    });
  renderProducts();
}

async function handleSelectedVendorsChange(vendorId) {
  lastSelectedVendorId = vendorId;

  const isChecked = $(`#vendor-${vendorId}`).is(":checked");

  if (isChecked) {
    selectedVendorIds.push(regionId);
  }
  else {
    selectedVendorIds = selectedVendorIds.filter(function (value) {
      return value !== vendorId;
    });
  }
  const url = "/DataDefinition/GetStoresByVendorIds";

  await ajaxPostAsync(url, selectedVendorIds)
    .then(stores => {
      if (stores.length > 0) lastSelectedStoreId = stores[0].store_ID;
      renderSelectedStoresOnSelectionTab(stores);
    })
    .catch((error) => {
      toastr.error(error);
    });
  renderProducts();
}

async function handleSelectedPriceGroupsChange(priceGroupId) {
  const isChecked = $(`#price-group-${priceGroupId}`).is(":checked");

  if (isChecked) {
    selectedPriceGroupIds.push(priceGroupId);
  }
  else {
    selectedPriceGroupIds = selectedPriceGroupIds.filter(function (value) {
      return value !== priceGroupId;
    });
  }
  const url = "/DataDefinition/GetStoresByPriceGroupIds";

  await ajaxPostAsync(url, selectedPriceGroupIds)
    .then(stores => {
      if (stores.length > 0) lastSelectedStoreId = stores[0].store_ID;
      renderSelectedStoresOnSelectionTab(stores);
    })
    .catch((error) => {
      toastr.error(error);
    });
  renderProducts();
}

async function handleSelectedRegionsChange(regionId) {
  const isChecked = $(`#region-${regionId}`).is(":checked");

  if (isChecked) {
    selectedRegionIds.push(regionId);
  }
  else {
    selectedRegionIds = selectedRegionIds.filter(function (value) {
      return value !== regionId;
    });
  }
  const url = "/DataDefinition/GetStoresByRegionId";

  await ajaxPostAsync(url, selectedRegionIds)
    .then(stores => {
      if (stores.length > 0) lastSelectedStoreId = stores[0].store_ID;
      renderSelectedStoresOnSelectionTab(stores);
    })
    .catch((error) => {
      toastr.error(error);
    });
  renderProducts();
}

function renderStoreOptions() {
  let availableOptionsContainer = $('#available-options-container');
  availableOptionsContainer.empty();
  storeOptions.forEach(store => {
    availableOptionsContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option" id="store-${store.store_ID}" 
                value="${store.store_ID}" 
                onChange="handleSelectedStoresChange('${store.store_ID}')">
        <label class="form-check-label" for="store-${store.store_ID}">${store.store_ID} -- ${store.store_Desc}</label>
      </div>`
    )
  })
}

function renderVendorOptions() {
  let availableOptionsContainer = $('#available-options-container');
  availableOptionsContainer.empty();
  vendorOptions.forEach(vendor => {
    availableOptionsContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option" id="vendor-${vendor.code}" 
                value="${vendor.code}" 
                onChange="handleSelectedVendorsChange('${vendor.code}')">
        <label class="form-check-label" for="vendor-${vendor.code}">${vendor.code} -- ${vendor.name}</label>
      </div>`
    )
  })
}

function renderRegionOptions() {
  let availableOptionsContainer = $('#available-options-container');
  availableOptionsContainer.empty();
  regionOptions.forEach(region => {
    availableOptionsContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option" id="region-${region.id}" 
                value="${region.id}" 
                onChange="handleSelectedRegionsChange('${region.id}')">
        <label class="form-check-label" for="region-${region.id}">${region.id} -- ${region.name}</label>
      </div>`
    )
  })
}

function renderPriceGroupOptions() {
  let availableOptionsContainer = $('#available-options-container');
  availableOptionsContainer.empty();

  priceGroupOptions.forEach(priceGroup => {
    availableOptionsContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option" id="price-group-${priceGroup.id}" 
                value="${priceGroup.id}" 
                onChange="handleSelectedPriceGroupsChange('${priceGroup.id}')">
        <label class="form-check-label" for="price-group-${priceGroup.id}">${priceGroup.id} -- ${priceGroup.name}</label>
      </div>`
    )
  })
}

let selectedDataTaxDefinitionCodeChange = (defCode) => {
  let selectedDef = $(`#data-tax-defintion-code-${defCode}`);
  const isChecked = selectedDef.is(":checked");
  if (isChecked) {
    selectedDataDefinitionCodes.push(selectedDef.val());
  }
  else {
    selectedDataDefinitionCodes = selectedDataDefinitionCodes.filter(function (value) {
      return value !== selectedDef.val();
    });
  }
}

let selectedDataAssociationDefinitionCodeChange = (defCode) => {
  let selectedDef = $(`#data-association-defintion-code-${defCode}`);
  const isChecked = selectedDef.is(":checked");
  if (isChecked) {
    selectedDataDefinitionCodes.push(selectedDef.val());
  }
  else {
    selectedDataDefinitionCodes = selectedDataDefinitionCodes.filter(function (value) {
      return value !== selectedDef.val();
    });
  }
}

let goToStockDefinition = () => {
  const url = `/PricebookTransfer?fromDataDefinition=true`;
  safelyNavigateTo(url);
}

function renderTaxMasterOptions() {
  let dataDefintionTypeContainer = $('#data-defintion-code-container');
  dataDefintionTypeContainer.empty();

  taxMasterOptions.forEach(taxMaster => {
    dataDefintionTypeContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option tax-option" id="data-tax-defintion-code-${taxMaster.name}--${taxMaster.code}"
                value="${taxMaster.name}--${taxMaster.code}" 
                onChange="selectedDataTaxDefinitionCodeChange('${taxMaster.name}--${taxMaster.code}')">
        <label class="form-check-label" for="data-defintion-code">${taxMaster.name} -- ${taxMaster.code} - ${parseFloat(taxMaster.rate.toFixed(3))}%</label>
      </div>`
    )
  })
}

function renderAssociationOptions() {
  let dataDefintionTypeContainer = $('#data-defintion-code-container');
  dataDefintionTypeContainer.empty();

  associationOptions.forEach(association => {
    dataDefintionTypeContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option charge-option" id="data-association-defintion-code-${association.code}"
                value="${association.code}" 
                onChange="selectedDataAssociationDefinitionCodeChange('${association.code}')">
        <label class="form-check-label" for="data-defintion-code">${association.code} -- ${association.description}</label>
      </div>`
    )
  })
}

async function renderDataDefinition() {
  clearTaxOrChargeSelection();
  const showByOption = $('input[name="show-by"]:checked').val();

  let params = new URLSearchParams();
  params.append('productId', lastSelectedProductId);
  params.append('storeId', lastSelectedStoreId);
  params.append('defType', currentDataDefinitionType);

  if (showByOption === "by-vendor")
    params.append('vendorId', lastSelectedVendorId);

  let urlWithParams = `DataDefinition/GetDataDefinitionsByCriteria?${params.toString()}`;

  await ajaxGetAsync(urlWithParams).then(dataDefinitions => {
    selectedDataDefinitionCodes = [];
    if (dataDefinitions.length > 0) {
      if (currentDataDefinitionType === "TAXES") {
        dataDefinitions.forEach(dataDefinition => {
          taxMasterOptions.forEach(taxMasterOption => {
            const taxMasterOptionNameAndCode = taxMasterOption.name + "--" + taxMasterOption.code;

            if (dataDefinition.defCode === taxMasterOptionNameAndCode) {
              $(`#data-tax-defintion-code-${taxMasterOption.name}--${taxMasterOption.code}`).prop('checked', true);
              selectedDataDefinitionCodes.push(`${taxMasterOption.name}--${taxMasterOption.code}`);
            }
          });
        });
      }
      else if (currentDataDefinitionType === "CHARGES") {
        dataDefinitions.forEach(dataDefinition => {
          associationOptions.forEach(associationOption => {
            if (dataDefinition.defCode === associationOption.code) {
              $(`#data-association-defintion-code-${associationOption.code}`).prop('checked', true);
              selectedDataDefinitionCodes.push(associationOption.code);
            }
          });
        });
      }
    }
    else {
      renderDefaultDefinitionSelection();
    }
  });
}

async function renderDefaultDefinitionSelection() {
  clearTaxOrChargeSelection();
  if (storeOptions.length === 0 || products.length === 0) return;

  let params = new URLSearchParams();
  params.append('stockCode', lastSelectedProductId);
  params.append('storeId', lastSelectedStoreId);

  if (currentDataDefinitionType === "TAXES") {
    let urlWithParams = `DataDefinition/GetStockTaxByStockCodeAndStore?${params.toString()}`;

    await ajaxGetAsync(urlWithParams).then(stockTaxs => {
      stockTaxs.forEach(stockTax => {
        taxMasterOptions.forEach(taxOption => {
          if (stockTax.name === taxOption.name && stockTax.code === taxOption.code) {
            $(`#data-tax-defintion-code-${stockTax.name}--${stockTax.code}`).prop('checked', true);
          }
        });
      });
    });
  }
  else if (currentDataDefinitionType === "CHARGES") {
    let urlWithParams = `DataDefinition/GetStockAssociationsByStockCodeAndStore?${params.toString()}`;

    await ajaxGetAsync(urlWithParams).then(associations => {
      associations.forEach(association => {
        associationOptions.forEach(associationOption => {
          if (association.code === associationOption.code) {
            $(`#data-association-defintion-code-${association.code}`).prop('checked', true);
          }
        });
      });
    });
  }
}

async function dataDefinitionTypeChanged(dataDefinitionType) {
  selectedDataDefinitionCodes = [];
  currentDataDefinitionType = dataDefinitionType;
  if (dataDefinitionType === "TAXES") {
    renderTaxMasterOptions();
  }
  else {
    renderAssociationOptions();
  }
  await renderDataDefinition();
}

let getUrlForProducts = () => {
  let url;
  const showByOption = $('input[name="show-by"]:checked').val();

  if (showByOption === "by-store") {
    url = "/DataDefinition/GetStockTransfersByStoreIds";
  }
  else if (showByOption === "by-region") {
    url = "/DataDefinition/GetStockTransfersByRegionIds";
  }
  else if (showByOption === "by-price-group") {
    url = "/DataDefinition/GetStockTransfersByPriceGroupIds";
  }
  else if (showByOption === "by-vendor") {
    url = "/DataDefinition/GetStockTransfersByVendorIds";
  }

  return url;
}

let handleProductSelectionChange = (productId, currentRow) => {
  lastSelectedProductId = productId;

  let tdElement = currentRow.querySelector("td:nth-child(2)");
  let tdCheckBox = currentRow.querySelector("td:nth-child(3)");
  let checkboxValue = tdCheckBox.querySelector("input").checked;

  let description = tdElement.textContent;
  let data = {
    productID: productId,
    description: description,
    vendorId: ''
  }

  if (checkboxValue) {
    selectedProducts.push(data);
  }
  else {
    selectedProducts = selectedProducts.filter(function (product) {
      return product.productID !== productId;
    });
  }
}

let checkGSTAndPST = () => {
  let count = 0;
  for (let i = 0; i < selectedDataDefinitionCodes.length; i++) {
    if (selectedDataDefinitionCodes[i].startsWith('GST-')) {
      count++;
    }
  }

  return (count > 1);
}

let clearAllCheckBoxes = () => {
  dataTableProduct.on('draw.dt', async function () {
    let tbody = await $("#table--products").find('tbody');
    tbody.find('tr').each(function () {
      changeCurrentRowSelect(false, $(this)[0]);
    });
  });

  let tbody = $("#table--products").find('tbody');
  tbody.find('tr').each(function () {
    changeCurrentRowSelect(false, $(this)[0]);
  });

  selectedProducts = [];
  selectedDataDefinitionCodes.forEach(function (defCode) {
    $(`#data-tax-defintion-code-${defCode}`).checked = false;
  });

  selectedDataDefinitionCodes = [];
  document.getElementById('view__select-or-unselect-all-products').checked = false;
}

function clearTaxOrChargeSelection() {
  $(".tax-option, .charge-option").prop("checked", false);
}

let handleSaveButtonClicked = async () => {

  const showByOption = $('input[name="show-by"]:checked').val();

  let data = {
    DataDefinitions: selectedProducts,
    storeIds: renderedSelectedStoreIds,
    priceGroupIds: (showByOption === "by-price-group") ? selectedPriceGroupIds : null,
    regionIds: (showByOption === "by-region") ? selectedRegionIds : null,
    vendorIds: (showByOption === "by-vendor") ? selectedVendorIds : null,
    isAllStoreSelected: document.getElementById("select-or-unselect-all-stores").checked,
    isAllProductSelected: document.getElementById("view__select-or-unselect-all-products").checked,
    transferUser: document.getElementById("userCode").value,
    definitionType: currentDataDefinitionType,
    defCodes: selectedDataDefinitionCodes,
    showByOption: showByOption
  }

  if ((data.DataDefinitions.length === 0 && data.isAllProductSelected === false) || data.storeIds.length === 0) {
    let title = "";
    let message = document.getElementById('youShouldSelectAtLeastOneItemToTransfer').value;

    showAlertModal(title, message).then((result) => { });
    return;
  }

  if (checkGSTAndPST()) {
    let title = "";
    let message = document.getElementById('youCannotSelectTwoGSTorPST').value;

    showAlertModal(title, message).then((result) => { });
    return;
  }

  const url = "/DataDefinition/SaveDataDefinition";
  await ajaxPostAsync(url, data)
    .then(() => {
      toastr.success($("#dataDefinitionSuccessfullySaved").val());

      clearAllCheckBoxes();
      dataDefinitionTypeChanged(currentDataDefinitionType);
    })
    .catch(() => toastr.error($("#dataDefinitionFailed").val()));
}

function renderProducts() {
  selectedProducts = [];
  if (dataTableProduct != null) {
    dataTableProduct.state.clear();
    dataTableProduct.clear();
    dataTableProduct.destroy();
  }

  let columns = [
    {
      data: "productID"
    },
    {
      data: "description"
    },
    {
      data: null,
      sortable: false,
      render: function (product) {
        let description = product.description
        return `<input class="form-check-input mt-2 product" id="product-${product.productID}__sel"
                  onChange="handleProductSelectionChange('${product.productID}', this.parentElement.parentElement)"
                  type="checkbox" value="false" />`;
      }
    }
  ];

  let props = createDataTableProp();
  props.columns = columns;
  props.serverSide = true;
  props.pageLength = 10;
  props.lengthMenu = [[10, 25, 50, 100], [10, 25, 50, 100]];
  props.rowCallback = function (row, data) {
    $(row).on('click', 'td', function () {
      lastSelectedProductId = data.productID;
      const columnIndex = $(this).index();
      if (columnIndex !== 2) {
        renderDataDefinition();
      }
    });
  }
  props.ajax = {
    url: getUrlForProducts(),
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    dataSrc: function (d) {
      products = d.data;
      if (products.length > 0) {
        lastSelectedProductId = products[0].productID;
      }
      renderDataDefinition();
      return d.data;
    },
    data: function (d) {
      const showByOption = $('input[name="show-by"]:checked').val();

      if (showByOption === "by-store") {
        let productStores = [];
        for (const x of storeOptions) {
          if (selectedStoreIds.includes(x.store_ID)) {
            productStores.push(x.store_ID);
            break;
          }
        }
        d.storeIds = productStores;
      }
      else if (showByOption === "by-region") {
        let productRegions = [];
        for (const x of regionOptions) {
          if (selectedRegionIds.includes(x.id)) {
            productRegions.push(x.id);
            break;
          }
        }
        d.regionIds = productRegions;
      }
      else if (showByOption === "by-price-group") {
        let productPriceGroup = [];
        for (const x of priceGroupOptions) {
          if (selectedPriceGroupIds.includes(x.id)) {
            productPriceGroup.push(x.id);
            break;
          }
        }
        d.priceGroupIds = productPriceGroup;
      }
      else if (showByOption === "by-vendor") {
        let productVendors = [];
        for (const x of vendorOptions) {
          if (selectedVendorIds.includes(x.code)) {
            productVendors.push(x.code);
            break;
          }
        }
        d.vendorIds = productVendors;
      }
      return JSON.stringify(d);
    }
  };

  dataTableProduct = createDatatableWithSearching("table--products", props);
}
