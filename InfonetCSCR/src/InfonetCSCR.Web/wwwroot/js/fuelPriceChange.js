let policies;
let gradeDatatable;
let gradeTierServiceLevelData;
let tempGradeTierServiceLevelData = [];
let filteredGradeTierServiceLevelData;
let cellIndex;
let rowIndex;
let currentSelectedCellData;
let isEnterKeyPressed;
const lowerLimit = FUEL_PRICE_CHANGE.LOWER_LIMIT;
const upperLimit = FUEL_PRICE_CHANGE.UPPER_LIMIT;
let grade = FUEL_PRICE_CHANGE.GRADE;
let tableType = grade;
let companyName;
let employeeId;
let editFieldActive;
let $inputField;

$(async function () {
  $('.text-danger').prop('hidden', true);
  $('#btn-apply').prop('disabled', true);
  $('#btn-transfer').prop('disabled', true);
  $('#btn-report').prop('disabled', true);

  companyName = await getCompanyName();

  await initPolicy();
  await handleSelectedStoreFromStoreListChange();
  handleSelectedStoresChange();
});

let initPolicy = async () => {
  const url = `FuelPriceChange/GetFuelPriceChangePolicies`;

  await ajaxGetAsync(url)
    .then(_ => policies = _)
    .catch(toastrError);
}

let checkStoreDataInHeadOfficeDatabase = async (data) => {
  let storeDataCheckedModel;
  let url = `FuelPriceChange/CheckStoreDataInHeadOfficeDatabase`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        storeDataCheckedModel = response;
      }
    })
    .catch(toastrError);

  return storeDataCheckedModel;
}

let checkStoreData = async (stores, isCheck) => {
  let fuelPriceChangeStoreCheckRecordViewModel = {
    stores: stores,
    isFuelGP: policies.isFuelGP
  }

  let storeDataCheckedModel;
  await checkStoreDataInHeadOfficeDatabase(fuelPriceChangeStoreCheckRecordViewModel)
    .then(result => {
      if (result) {
        storeDataCheckedModel = result;
      }
    });

  let title = $("#fuelPrice").val();
  let message = "";
  let showAlert = false;

  if (storeDataCheckedModel.notExistsInGradeTable?.length > 0) {
    let stores = storeDataCheckedModel.notExistsInGradeTable.join(',');
    message += `${$("#missingRecordsInTableForStores").val().replace("{0}", "Grade").replace("{1}", stores)}\n\n`; // Hard coded in VB code
    showAlert = true;
  }

  if (storeDataCheckedModel.notExistsInTierTable?.length > 0) {
    let stores = storeDataCheckedModel.notExistsInTierTable.join(',');
    message += `${$("#missingRecordsInTableForStores").val().replace("{0}", "Tier").replace("{1}", stores)}\n\n`; // Hard coded in VB code
    showAlert = true;
  }

  if (storeDataCheckedModel.notExistsInServiceLevelTable?.length > 0) {
    let stores = storeDataCheckedModel.notExistsInServiceLevelTable.join(',');
    message += `${$("#missingRecordsInTableForStores").val().replace("{0}", "ServiceLevel").replace("{1}", stores)}\n\n`; // Hard coded in VB code
    showAlert = true;
  }

  if (!storeDataCheckedModel.inSameTierLevel) {
    message += `${$("#allSelectedStoresHaveToHaveSameDefinition").val().replace("{0}", "Tier").replace("{1}", "Tier")}\n\n`; // Hard coded in VB code
    showAlert = true;
  }

  if (!storeDataCheckedModel.inSameServiceLevel) {
    message += `${$("#allSelectedStoresHaveToHaveSameDefinition").val().replace("{0}", "Service Level").replace("{1}", "Service Level")}\n\n`; // Hard coded in VB code
    showAlert = true;
  }

  if (storeDataCheckedModel.notExistsInTierLevelPriceDiffTable?.length > 0) {
    let stores = storeDataCheckedModel.notExistsInTierLevelPriceDiffTable.join(',');
    showAlert = true;
    message += `${$("#missingRecordsInTableForStores").val().replace("{0}", "TierLevelPriceDiff").replace("{1}", stores)}\n\n`; // Hard coded in VB code
  }

  if (showAlert && isCheck) {
    await showAlertModal(title, message).then((result) => { });
  }

  return storeDataCheckedModel.storeIdsThatCannotBeAdded;
}

let setSelectedStoresInSelectionTab = async (newlySelectedStores, isCheck) => {
  let storeIDs = [...new Set(newlySelectedStores.map(store => store.storeID))];
  let storeIdsThatCannotBeAdded = await checkStoreData(storeIDs, isCheck);

  newlySelectedStores = newlySelectedStores.filter(store =>
    !storeIdsThatCannotBeAdded.includes(store.storeID)
  );

  selectedStoresOnSelectionTab = selectedStoresOnSelectionTab || new Set();
  let newlySelectedStoresSet = new Set([...newlySelectedStores.map(store => simpleStoreObject(store))]);
  if (isCheck) {
    newlySelectedStoresSet.forEach(store => selectedStoresOnSelectionTab.add(store));
  }
  else {
    let storeIDsToRemove = new Set(newlySelectedStores.map(store => store.storeID));
    if (storeIDsToRemove && storeIDsToRemove.size > 0) {
      for (let store of selectedStoresOnSelectionTab) {
        if (storeIDsToRemove.has(store.storeID)) {
          selectedStoresOnSelectionTab.delete(store);
        }
      }
    }
  }
}

let handleSelectedStoresChange = async (storeId) => {
  const selectedStoreIds = $('.store-option:checked').map((index, element) => $(element).val()).get();
  let isCheck = true;
  let newlySelectedStores;
  if (storeId) {
    newlySelectedStores = storeOptions.filter(store => store.storeID === storeId).map(store => simpleStoreObject(store));
    isCheck = selectedStoreIds.includes(storeId);
  }
  else {
    newlySelectedStores = storeOptions.filter(store => selectedStoreIds.includes(store.storeID));
    isCheck = $("#select-or-unselect-all").prop("checked");
  }

  await setSelectedStoresInSelectionTab(newlySelectedStores, isCheck);
  removeDuplicateStores();
  const isAllOptionSelected = $('.store-option').length === $('.store-option:checked').length;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_STORE);
  await checkTheFirstStoreFromTheList();
}

let handleSelectedRegionsChange = async (regionId) => {
  const selectedRegionIds = $('.region-option:checked').map((index, element) => $(element).val()).get();
  selectedRegionsOnSelectionTab = regionOptions.filter(region => selectedRegionIds.includes(region.id));

  let isCheck = true;
  let newlySelectedStores;
  if (regionId) {
    newlySelectedStores = storeOptions.filter(store => store.regionID === regionId).map(store => simpleStoreObject(store));
    isCheck = selectedRegionIds.includes(regionId);
  }
  else {
    newlySelectedStores = storeOptions.filter(store => selectedRegionIds.includes(store.regionID))
      .map(store => simpleStoreObject(store));
    isCheck = $("#select-or-unselect-all").prop("checked");
  }

  await setSelectedStoresInSelectionTab(newlySelectedStores, isCheck);
  const isAllOptionSelected = regionOptions.length === selectedRegionsOnSelectionTab.length;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_REGION);
  await checkTheFirstStoreFromTheList();
}

let handleSelectedPriceGroupsChange = async (priceGroupId) => {
  const selectedPriceGroupIds = $('.price-group-option:checked').map((index, element) => $(element).val()).get();
  selectedPriceGroupsOnSelectionTab = priceGroupOptions.filter(priceGroup => selectedPriceGroupIds.includes(priceGroup.id));

  let isCheck = true;
  let newlySelectedStores;
  if (priceGroupId) {
    newlySelectedStores = storeOptions.filter(store => store.priceGroupID === priceGroupId).map(store => simpleStoreObject(store));
    isCheck = selectedPriceGroupIds.includes(priceGroupId);
  }
  else {
    newlySelectedStores = storeOptions.filter(store => selectedPriceGroupIds.includes(store.priceGroupID))
      .map(store => simpleStoreObject(store));
    isCheck = $("#select-or-unselect-all").prop("checked");
  }

  await setSelectedStoresInSelectionTab(newlySelectedStores, isCheck);
  const isAllOptionSelected = priceGroupOptions.length === selectedPriceGroupsOnSelectionTab.length;
  $("#select-or-unselect-all").prop("checked", isAllOptionSelected);
  renderSelectedStoresOnSelectionTab();
  renderSelectedEntryData(STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP);
  await checkTheFirstStoreFromTheList();
}

let handleSelectOrUnselectAll = () => {
  const showByOption = $('input[name="show-by"]:checked').val();
  let isChecked = $("#select-or-unselect-all").is(":checked");
  $(".select-by-option").prop("checked", isChecked);

  if (!isChecked) {
    selectedStoresOnSelectionTab.clear();
  }

  if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_STORE)
    handleSelectedStoresChange();
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_REGION)
    handleSelectedRegionsChange();
  else if (showByOption === STRING_TABLE.SHOW_BY_OPTION.BY_PRICE_GROUP)
    handleSelectedPriceGroupsChange();

  renderSelectedStoresOnSelectionTab();
}

let getDataFromGradeTierAndServiceLevelTable = async (storeId) => {
  let data = {
    storeId,
    isFuelCp: policies.isFuelCP
  }

  let url = `FuelPriceChange/GetDataFromGradeTierAndServiceLevelTable`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        gradeTierServiceLevelData = response;
        tempGradeTierServiceLevelData = JSON.parse(JSON.stringify(gradeTierServiceLevelData));
        filteredGradeTierServiceLevelData =
          (tableType === grade) ? gradeTierServiceLevelData.filter(item => item.loadRow !== false) : gradeTierServiceLevelData;
      }
    })
    .catch(toastrError);
}

let createDatatable = () => {
  let data;
  let tableId;
  let dataTableColumns;

  destroyDataTableIfExists(gradeDatatable);
  data = JSON.parse(JSON.stringify(filteredGradeTierServiceLevelData)); // Cloning data without reference
  tableId = "grade-grid";
  dataTableColumns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "stockCode", },
    { "data": "tierDescription", },
    { "data": "serviceLevelDescription", },
    {
      "data": "cashPrice",
      render: function (data) {
        return parseFloat(data).toFixed(3);
      }
    },
    {
      "data": "creditPrice",
      render: function (data) {
        return parseFloat(data).toFixed(3);
      }
    },
  ];

  let props = createDataTableProp();
  props.data = data;
  props.scrollY = '440px';
  props.paging = false;
  props.searching = false;
  props.columns = dataTableColumns;
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];
  props.rowsGroup = [0, 1]; // Reference: https://datatables.net/forums/discussion/29319/new-rowsgroup-plugin-merge-cells-vertically-rowspan

  gradeDatatable = createDataTableGeneric(tableId, props, true);

  if (data.length > 0) {
    $('#btn-apply').prop('disabled', false);
    $('#btn-transfer').prop('disabled', false);
    $('#btn-report').prop('disabled', false);
  }
}

let checkTheFirstStoreFromTheList = async () => {
  let storeId;
  if (selectedStoresOnSelectionTab.size > 0) {
    let [firstStore] = selectedStoresOnSelectionTab;
    storeId = firstStore.storeID;
    $(`#storelistStore-${storeId}`).prop('checked', true);
  }
  await handleSelectedStoreFromStoreListChange(storeId);
}

let handleSelectedStoreFromStoreListChange = async (storeId) => {
  if (!isNullUndefinedOrWhiteSpace(storeId)) {
    await getDataFromGradeTierAndServiceLevelTable(storeId);
    $(`.selected-grid-row`).removeClass('selected-grid-row');
    $(`label[for="storelistStore-${storeId}"]`).addClass('selected-grid-row');
  }
  else {
    gradeTierServiceLevelData = [];
    filteredGradeTierServiceLevelData = [];
    $("#btn-transfer").prop('disabled', true);
    $("#btn-report").prop('disabled', true);
  }

  createDatatable();
}

let handleLabelSelected = async (storeId) => {
  let isChecked = $(`#storelistStore-${storeId}`).prop('checked');
  $(`#storelistStore-${storeId}`).prop('checked', !isChecked);
  await handleSelectedStoreFromStoreListChange(storeId);
}

let checkAndShowWarningIfRequired = (value) => {
  let columnTitle = (cellIndex === 3) ? $("#cashPriceText").val() : $("#creditPriceText").val();
  if ((isNaN(value) || isNullUndefinedOrWhiteSpace(value))) {
    let validationMessage = $("#pleaseEnterValid").val().replace("{0}", columnTitle);
    toastr.warning(validationMessage);
    return false;
  } else if (parseFloat(value) < lowerLimit || parseFloat(value) > upperLimit) {
    let validationMessage = $("#pleaseEnterBetween").val().replace("{0}", columnTitle);
    toastr.warning(validationMessage);
    return false;
  }
  return true;
}

let handleEnterKeyPress = async function (event) {
  if (event.key === "Enter") {
    isEnterKeyPressed = true;
    event.preventDefault();
    let inputValue = $inputField.val().trim();
    if (checkAndShowWarningIfRequired(parseFloat(inputValue))) {
      const table = $('#grade-grid').DataTable();
      $inputField.remove();
      table.cell({ row: rowIndex, column: cellIndex }).data(inputValue).draw();

      let tierDescription = table.cell({ row: rowIndex, column: 1 }).data().trim();
      let levelDescription = table.cell({ row: rowIndex, column: 2 }).data().trim();

      if (policies.isFuelCP) {
        if (!(tierDescription === 'Full' && levelDescription === 'Day')) {
          table.cell({ row: rowIndex, column: 4 }).data(inputValue).draw();
        }
      } else {
        table.cell({ row: rowIndex, column: 4 }).data(inputValue).draw();
      }
    }
    else {
      const table = $('#grade-grid').DataTable();
      $inputField.remove();
      table.cell({ row: rowIndex, column: cellIndex }).data(currentSelectedCellData).draw();
    }

    editFieldActive = false;
  }
}

let handleInputFieldBlur = function () {
  if (!isEnterKeyPressed) {
    const table = $('#grade-grid').DataTable();
    $inputField.remove();
    table.cell({ row: rowIndex, column: cellIndex }).data(currentSelectedCellData).draw();
  }
  isEnterKeyPressed = false; // Reset the flag after handling blur event
  editFieldActive = false; // Reset the flag after handling blur event
};

let isCellEditable = () => {
  const table = $('#grade-grid').DataTable();
  let isGradeGrid = $('#gradeRadioButton').is(':checked');
  let tierDescription = table.cell({ row: rowIndex, column: 1 }).data().trim();
  let levelDescription = table.cell({ row: rowIndex, column: 2 }).data().trim();
  let isEditable = false;

  // Cash Price (column no 3) column is always editable
  if (cellIndex === 3) {
    isEditable = true;
  }

  // Credit Price (column no 4) 
  if (cellIndex === 4 && policies.isFuelCP) {
    // will be editable if Fuel_CP policy is true & if it's 'Grade Grid'
    if (isGradeGrid) {
      isEditable = true;
    } else {
      // will be editable if its 'Tier and Level' grid and tierId = 1 (tierDescription = Full) and leveId = 1 (levelDescription = Day)
      isEditable = (tierDescription === 'Full' && levelDescription === 'Day');
    }
  }

  return isEditable;
}

$('#grade-grid')
  .off()
  .on('click', 'td', function () {
    if (editFieldActive) {
      return;
    }

    cellIndex = $(this).index();
    rowIndex = gradeDatatable.row($(this).closest('tr')).index();
    editFieldActive = true;

    if (isCellEditable()) {
      isEnterKeyPressed = false;
      currentSelectedCellData = $(this).text() ? parseFloat($(this).text()) : currentSelectedCellData;

      $(this).empty();
      // Create an input field dynamically
      $inputField = $(`<input type="number" id="edit-field" oninput="restrictDecimalPoints()" onkeypress="return onlyDigitAndDecimalKeyPress(event)">`);
      const columnWidth = $(this).width();
      $inputField.css('max-width', columnWidth + 'px');
      $(this).append($inputField);
      $inputField.focus();

      $inputField.on("keydown", handleEnterKeyPress);
      $inputField.on("blur", handleInputFieldBlur);
    }
  });

let restrictDecimalPoints = () => { 
  const newValue = $(`#edit-field`).val();
  const regex = /^(\d*\.?\d{0,3})$/; // restrict user to put more than 3 decimal points

  if (!regex.test(newValue)) {
    $(`#edit-field`).val(newValue.slice(0, -1));
  }
}

$('input[name="selection"]').on('change', function () {
  tempGradeTierServiceLevelData = JSON.parse(JSON.stringify(gradeTierServiceLevelData));
  tableType = $('input[name="selection"]:checked').val();
  filteredGradeTierServiceLevelData =
    (tableType === grade) ? gradeTierServiceLevelData.filter(item => item.loadRow !== false) : gradeTierServiceLevelData;
  createDatatable();
});

$("#btn-apply").on('click', function () {
  let priceChangeVal = parseFloat($('#price-change-val').val().trim());
  let isIncrease = $('#increase').is(':checked');
  let isDollar = $('#currency-dollar').is(':checked');

  if (isNaN(priceChangeVal)) {
    $('#required-validation-for-price-change').prop('hidden', false);
    return;
  }

  let increase = $('#strIncrease').val();
  let decrease = $('#strDecrease').val();
  let title = $('#confirmPriceChange').val();
  let message = $('#confirmPriceChangeMessage').val().replace("{0}", isIncrease ? increase : decrease).replace("{1}", isDollar ? priceChangeVal.toFixed(3) : priceChangeVal).replace("{2}", isDollar ? "$" : "%");

  showConfirmModal(title, message).then((result) => {
    if (result === true) {
      $('#grade-grid tr').each(function () {
        let currentCashPrice = parseFloat($(this).find('td:eq(3)').text().trim());
        let currentCreditPrice = parseFloat($(this).find('td:eq(4)').text().trim());

        $(this).find('td:eq(3)').text(changePriceApplyCalculation(currentCashPrice, isIncrease, isDollar, priceChangeVal));
        $(this).find('td:eq(4)').text(changePriceApplyCalculation(currentCreditPrice, isIncrease, isDollar, priceChangeVal));
      });
    }

    $('#required-validation-for-price-change').prop('hidden', true);
  });

  for (let item of tempGradeTierServiceLevelData) {
    item.cashPrice = changePriceApplyCalculation(parseFloat(item.cashPrice), isIncrease, isDollar, priceChangeVal);
    item.creditPrice = changePriceApplyCalculation(parseFloat(item.creditPrice), isIncrease, isDollar, priceChangeVal);
  }
});

let changePriceApplyCalculation = (currentPrice, isIncrease, isDollar, priceChangeVal) => {
  let changedPrice = currentPrice;

  if (isIncrease && isDollar) {
    changedPrice += priceChangeVal;
  } else if (isIncrease) {
    let applyPrice = (changedPrice * priceChangeVal) / 100.0;
    changedPrice += applyPrice;
  } else if (isDollar) {
    changedPrice -= priceChangeVal;
  } else {
    let applyPrice = (changedPrice * priceChangeVal) / 100.0;
    changedPrice -= applyPrice;
  }

  return parseFloat(changedPrice).toFixed(3);
}

$("#btn-transfer").on('click', async function () {
  let selectedGradeStoresForTransfer = [];
  let selectedStoresForTransfer = [];
  let isInRange = true;

  $('#selected-stores-container input[type="checkbox"]').each(function () {
    if ($(this).is(':checked')) {
      let selectedStoreIdForTransfer = $(this).val();

      selectedStoresOnSelectionTab.forEach(store => {
        if (store.storeID === selectedStoreIdForTransfer) {
          selectedStoresForTransfer.push(store);
          let prevIndex = 0;

          for (let gradeItem of tempGradeTierServiceLevelData) {
            let creditPriceValColumn = 4;
            let gridIndex = filteredGradeTierServiceLevelData.findIndex(data =>
              data.stockCode === gradeItem.stockCode &&
              data.tierId === gradeItem.tierId &&
              data.serviceLevelId === gradeItem.serviceLevelId);

            // if the grade grid is 'Grade'
            if (gridIndex === -1) {
              gridIndex = prevIndex;

              if (policies.isFuelCP) {
                creditPriceValColumn = 3;
              }
            } else {
              prevIndex = gridIndex;
            }

            let transferStore = {
              storeId: store.storeID,
              gradeId: gradeItem.gradeId,
              tierId: gradeItem.tierId,
              serviceLevelId: gradeItem.serviceLevelId,
              cashPrice: parseFloat($(`#grade-grid tr`).find('td:eq(3)')[gridIndex].innerText.trim()),
              creditPrice: parseFloat($(`#grade-grid tr`).find(`td:eq(${creditPriceValColumn})`)[gridIndex].innerText.trim()),
              taxFreePrice: 0,
              taxFreeCreditPrice: 0
            };

            selectedGradeStoresForTransfer.push(transferStore);
            isInRange = ((transferStore.cashPrice >= lowerLimit && transferStore.cashPrice <= upperLimit) && (transferStore.creditPrice >= lowerLimit && transferStore.creditPrice <= upperLimit))

            if (!isInRange) {
              return;
            }
          }
        }
      });
    }
  });

  if (selectedStoresForTransfer.length === 0) {
    toastr.warning($("#noStoreSelected").val());
    return;
  }

  if (!isInRange) {
    toastr.warning($("#cashOrCreditPriceNotInRange").val());
    return;
  }

  const stores = selectedStoresForTransfer.map(store => `${store.storeID} -- ${store.storeName}`).join(', ');
  let title = $("#transferConfirmTitle").val();
  let message = $("#transferConfirmMessage").val().replace("{0}", stores);
  
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `FuelPriceChange/TransferFuelPriceChange`;
      await ajaxPostAsync(url, selectedGradeStoresForTransfer)
        .then(async (response) => {
            if (response) {
                selectedStoresOnSelectionTab.forEach(store => {
                    $(`#store-${store.storeID}`).prop('checked', false);
                });

                selectedStoresOnSelectionTab.clear();
                $(`#select-or-unselect-all`).prop('checked', false);

                await handleSelectedStoreFromStoreListChange();
                handleSelectedStoresChange();
                handleSelectOrUnselectAll();

                $("#price-change-val").val('');
                toastr.success($("#fuelPricesSaved").val());
            }
        })
        .catch(toastrError);
    }
  });
});

$('#btn-report').on('click', async function () {
  let selectedStoreIdsForReport = [];
  
  $('#selected-stores-container input[type="checkbox"]').each(function () {
    if ($(this).is(':checked')) {
      selectedStoreIdsForReport.push($(this).val());
    }
  });

  if (isNullUndefinedOrWhiteSpace(selectedStoreIdsForReport)) {
    toastr.warning($("#noStoreSelected").val());
    return;
  }

  await getEmployeeId();
  let language = document.getElementById('language').value;
  let reportName = "FuelPriceChangeReport";
  let windowName = "FuelPriceChangeReport";
  let width = 1000;
  let height = window.screen.height - 50;

  let result = {};
  
  result.reportName = reportName;
  result.language = language;
  result.reportWidth = "900px";
  result.reportHeight = window.screen.height - 150;
  result.storeIds = selectedStoreIdsForReport;
  result.isFuelCp = policies.isFuelCP;
  result.emplId = employeeId;
  result.companyName = companyName;

  displayReport(windowName, width, height, result);
});

let getEmployeeId = async () => {
  const url = `FuelPriceChange/GetEmployeeId`;

  await ajaxGetAsync(url)
    .then(id => employeeId = id)
    .catch(toastrError);
}
