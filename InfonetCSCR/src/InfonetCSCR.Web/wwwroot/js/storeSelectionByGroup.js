const STRING_TABLE = PRICEBOOK_TRANSFER_STRING_TABLE;

let selectedStoresOnSelectionTab = new Set();
let storeOptions = [];
let vendorOptions = [];
let regionOptions = [];
let priceGroupOptions = [];
let isStoreTabStoresCheckBox = false;

$(async function () {
  let showByStore = $('#showByStore').val() === 'True';
  let showByVendor = $('#showByVendor').val() === 'True';
  let showByRegion = $('#showByRegion').val() === 'True';
  let showByPrice = $('#showByPrice').val() === 'True';
  isStoreTabStoresCheckBox = $('#storeTabStoresCheckBox').val() === 'True';

  if (showByStore) {
    await loadStores().then(() => renderStoreOptions());
  }

  if (showByVendor) {
    await loadVendors();
  }

  if (showByRegion) {
    await loadRegions();
  }

  if (showByPrice) {
    await loadPriceGroups();
  }
});

async function loadStores() {
  const url = STRING_TABLE.URL.GET.STORES_WITH_REGION_AND_PRICE_GROUP;

  await ajaxGetAsync(url)
    .then((stores) => storeOptions = stores)
    .catch(toastrError);
}

async function loadVendors() {
  let url = STRING_TABLE.URL.GET.VENDORS;

  await ajaxGetAsync(url)
    .then((vendors) => vendorOptions = vendors)
    .catch(toastrError);
}

async function loadRegions() {
  const url = STRING_TABLE.URL.GET.REGION;

  await ajaxGetAsync(url)
    .then((regions) => regionOptions = regions)
    .catch(toastrError);
}

async function loadPriceGroups() {
  const url = STRING_TABLE.URL.GET.PRICE_GROUP;

  await ajaxGetAsync(url)
    .then((priceGroups) => priceGroupOptions = priceGroups)
    .catch(toastrError);
}

function renderStoreOptions() {
  let availableOptionsContainer = $('#available-options-container');
  availableOptionsContainer.empty();

  // finds the unique stores based on id and name
  let storeSet = new Set(storeOptions.reduce((acc, curr) => {
    if (!acc.find(store => store.storeID === curr.storeID)) {
      acc.push(curr);
    }
    return acc;
  }, []));

  storeSet.forEach(store => {
    availableOptionsContainer.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option store-option" id="store-${store.storeID}" 
                value="${store.storeID}" 
                onChange="handleSelectedStoresChange(this.value)">
        <label class="form-check-label" for="store-${store.storeID}">${store.storeID} -- ${store.storeName}</label>
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
        <input type="checkbox" class="form-check-input select-by-option vendor-option" id="vendor-${vendor.code}" 
                value="${vendor.code}" 
                onChange="handleSelectedVendorsChange(this.value)">
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
        <input type="checkbox" class="form-check-input select-by-option region-option" id="region-${region.id}" 
                value="${region.id}" 
                onChange="handleSelectedRegionsChange(this.value)">
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
        <input type="checkbox" class="form-check-input select-by-option price-group-option" id="price-group-${priceGroup.id}" 
                value="${priceGroup.id}" 
                onChange="handleSelectedPriceGroupsChange(this.value)">
        <label class="form-check-label" for="price-group-${priceGroup.id}">${priceGroup.id} -- ${priceGroup.name}</label>
      </div>`
    )
  })
}

let renderSelectedEntryData = (selectedOption) => {
  let entriesSelected = $('#entriesSelected').val();
  if (selectedOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
    const numberOfStores = $('.store-option').map((index, element) => $(element).val()).get();
    const selectedStoreIds = $('.store-option:checked').map((index, element) => $(element).val()).get();
    entriesSelected = entriesSelected.replace("{0}", selectedStoreIds.length).replace("{1}", numberOfStores.length);
    $("#total-entries-options-container").text(entriesSelected);
    $("#total-selected-stores").text(selectedStoresOnSelectionTab.size);
  }
  else if (selectedOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
    entriesSelected = entriesSelected.replace("{0}", selectedRegionsOnSelectionTab.length).replace("{1}", regionOptions.length);
    $("#total-entries-options-container").text(entriesSelected);
    $("#total-selected-stores").text(selectedStoresOnSelectionTab.size);
  }
  else if (selectedOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
    entriesSelected = entriesSelected.replace("{0}", selectedPriceGroupsOnSelectionTab.length).replace("{1}", priceGroupOptions.length);
    $("#total-entries-options-container").text(entriesSelected);
    $("#total-selected-stores").text(selectedStoresOnSelectionTab.size);
  }
  else if (selectedOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    entriesSelected = entriesSelected.replace("{0}", selectedVendorsOnSelectionTab.length).replace("{1}", vendorOptions.length);
    $("#total-entries-options-container").text(entriesSelected);
    $("#total-selected-stores").text(selectedStoresOnSelectionTab.size);
  }
}

function handleShowByOptionChange() {
  $("#select-or-unselect-all").prop("checked", false);

  const showByOption = $('input[name="show-by"]:checked').val();
  localStorage.setItem('showByOption', showByOption);
  $("#selected-stores-container").empty();
  selectedStoresOnSelectionTab = new Set();
  $("#vendor-products-btn").hide();

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE) {
    renderStoreOptions();
    $("#available-options-label").text($("#byStore").val());
    renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_STORE)
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION) {
    renderRegionOptions();
    selectedRegionsOnSelectionTab = [];
    $("#available-options-label").text($("#byRegion").val());
    renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_REGION)
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP) {
    renderPriceGroupOptions();
    selectedPriceGroupsOnSelectionTab = [];
    $("#available-options-label").text($("#byPriceGroup").val());
    renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP)
  }
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR) {
    renderVendorOptions();
    selectedVendorsOnSelectionTab = [];
    $("#vendor-products-btn").show();
    $("#available-options-label").text($("#byVendor").val());
    renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_VENDOR)
  }
}

function removeDuplicateStores() {
  if (selectedStoresOnSelectionTab === null) return;
  selectedStoresOnSelectionTab = new Set([...selectedStoresOnSelectionTab]
    .reduce((acc, curr) => {
      if (!acc.find(store => store.storeID === curr.storeID)) {
        acc.push(curr);
      }
      return acc;
    }, []));
}

function renderSelectedStoresOnSelectionTab() {
  let selectedStoresContainers = $("#selected-stores-container");
  selectedStoresContainers.empty();
  removeDuplicateStores();

  selectedStoresOnSelectionTab.forEach(store => {
    selectedStoresContainers.append(!isStoreTabStoresCheckBox ? `
        <div class="my-1">${store.storeID} -- ${store.storeName}</div>
    ` : `<div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option storeListStoreCheckBox" id="storelistStore-${store.storeID}" 
                value="${store.storeID}"
                onclick="handleSelectedStoreFromStoreListChange(this.value)">
        <label class="form-check-label" for="storelistStore-${store.storeID}"
                onclick="handleLabelSelected('${store.storeID}')">
          ${store.storeID} -- ${store.storeName}
        </label>
      </div>`);
  });

  if (isStoreTabStoresCheckBox) {
    selectedStoresContainers.on('change', 'input[type="checkbox"][name="storeCheckbox"]', function () {
      let checkboxes = $('input[type="checkbox"][name="storeCheckbox"]');
      checkboxes.not(this).prop('checked', false);
    });
  }
}

function simpleStoreObject(store) {
  return {
    storeID: store.storeID,
    storeName: store.storeName
  }
}
