let errorMessage;
$(async function () {
  let productTypeSelectValue = document.getElementById("productType-select").value;

  if (document.getElementById("isAddProduct").value === "True") {
    document.getElementById("btn-cancel").hidden = true;
    setGlobalEditingState(true);
    document.getElementById("availability").checked = true;
    if (document.getElementById("elgLoyPolicy").value === "True") {
      document.getElementById("eligibleForLoyalty").checked = true;
    }
  }
  else {
    setGlobalEditingState(false);
  }

  if (productTypeSelectValue === "" || productTypeSelectValue === null) {
    document.getElementById("productType-select").value = "V";
  }
  await loadCategoryOptions();
  await loadFamilyOptions();
  await loadBrandOptions();
  await loadGenericOptions();
  await loadCharacteristicOptions();
  await loadUnitOfMeasureOptions();
  await loadOrderUnitOptions();
  await loadWarehouse();
  setPOSDescirption();
  setLongDescription();
  loadDataFromModel();
  setVisibilityStockitemFormInput();
  setVisibilityStockCodeInput();
  if (document.getElementById("isBackOffice").value === "False") {
    loadTransferButton();
  }
  errorMessage = document.getElementById("errorMessage").value;

  let packing = document.getElementById("packing").value;
  if (packing === null || packing === "" || packing === 0) {
    document.getElementById("packing").value = "1";
  }

  if (document.getElementById("StockCodeAlreadyExists").value) {
    let message = document.getElementById("StockCodeAlreadyExists").value;
    document.getElementById("StockCodeAlreadyExists").value = "";
    toastr.warning(message);
  }
});

let loadCategoryOptions = async () => {
  $("#category-select").html(optionGeneratorDefault(document.getElementById("category").value));
  let url = "/StockItems/GetDepartments";

  await ajaxGetAsync(url)
    .then(_depts => {
      for (const dept of _depts) {
        $("#category-select").append(optionGenerator(dept.deptId, dept.deptName));
      }
    })
    .catch(toastrError);
}

async function loadSubCategoryOptions(deptId) {
  $("#subcategory-select").html(optionGeneratorDefault(document.getElementById("subCategory").value));
  $("#subdetail-select").html(optionGeneratorDefault(document.getElementById("subDetail").value));
  if (!deptId) return;

  let url = "/StockItems/GetSubDepartments";

  await ajaxGetAsync(url, { department: deptId })
    .then(_subDepts => {
      for (const subDept of _subDepts) {
        $("#subcategory-select").append(optionGenerator(subDept.subDeptId, subDept.subDeptName));
      }
    })
    .catch(toastrError);
}

async function loadSubDetailsOptions(subDeptId) {
  $("#subdetail-select").html(optionGeneratorDefault(document.getElementById("subDetail").value));
  if (!subDeptId) return;

  let category = document.getElementById("category-select");
  let deptId = category.options[category.selectedIndex].value;
  let url = "/StockItems/GetSubDetails";

  await ajaxGetAsync(url, { department: deptId, subDepartment: subDeptId })
    .then(_subDetails => {
      for (const subDetail of _subDetails) {
        $("#subdetail-select").append(optionGenerator(subDetail.subDetailId, subDetail.subDetailName));
      }
    })
    .catch(toastrError);
}

let loadFamilyOptions = async () => {
  $("#family-select").html(optionGeneratorDefault(document.getElementById("family").value));
  let url = "/StockItemForm/GetAllFamilies";

  await ajaxGetAsync(url)
    .then(_families => {
      _families = JSON.parse(_families);
      for (const family of _families) {
        $("#family-select").append(optionGenerator(family.familyID, family.description));
      }
    })
    .catch(toastrError);
}

let loadBrandOptions = async () => {
  $("#brand-select").html(optionGeneratorDefault(document.getElementById("brand").value));
  let url = "/StockItemForm/GetAllBrands";

  await ajaxGetAsync(url)
    .then(_brands => {
      _brands = JSON.parse(_brands);
      for (const brand of _brands) {
        $("#brand-select").append(optionGenerator(brand.brandID, brand.description));
      }
    })
    .catch(toastrError);
}

let loadGenericOptions = async () => {
  $("#generic-select").html(optionGeneratorDefault(document.getElementById("generic").value));
  let url = "/StockItemForm/GetAllGenerics";

  await ajaxGetAsync(url)
    .then(_generics => {
      _generics = JSON.parse(_generics);
      for (const generic of _generics) {
        $("#generic-select").append(optionGenerator(generic.genericID, generic.description));
      }
    })
    .catch(toastrError);
}

let loadCharacteristicOptions = async () => {
  $("#characteristic-select").html(optionGeneratorDefault(document.getElementById("characteristic").value));
  let url = "/StockItemForm/GetAllCharacteristics";

  await ajaxGetAsync(url)
    .then(_chars => {
      _chars = JSON.parse(_chars);
      for (const char of _chars) {
        $("#characteristic-select").append(optionGenerator(char.characteristicID, char.description));
      }
    })
    .catch(toastrError);
}

let loadUnitOfMeasureOptions = async () => {
  $("#unitOfMeasure-select").html(optionGeneratorDefault(document.getElementById("unitOfMeasure").value));
  let url = "/StockItemForm/GetAllUnitOfMeasures";

  await ajaxGetAsync(url)
    .then(_uoms => {
      _uoms = JSON.parse(_uoms);
      for (let i = 0; i < _uoms.length; i++) {
        $("#unitOfMeasure-select").append(optionGenerator(_uoms[i].unitOfMeasureID, _uoms[i].description));
      }
    })
    .catch(toastrError);
}

let loadOrderUnitOptions = async () => {
  $("#orderUnit-select").html(optionGeneratorDefault(document.getElementById("orderUnit").value));
  let url = "/StockItemForm/GetAllOrderUnits";

  await ajaxGetAsync(url)
    .then(_orderUnits => {
      _orderUnits = JSON.parse(_orderUnits);
      for (const orderUnit of _orderUnits) {
        $("#orderUnit-select").append(optionGenerator(orderUnit.orderID, orderUnit.description));
      }
    })
    .catch(toastrError);
}

let loadWarehouse = async () => {
  if (document.getElementById("isBackOffice").value === "False") {
    $("#warehouse-select").html(`<option value="">${document.getElementById("selectWarehouse").value}</option>`);
    let url = "/StockItemForm/GetWarehouse";

    await ajaxGetAsync(url)
      .then(_warehouses => {
        _warehouses = JSON.parse(_warehouses);
        for (const warehouse of _warehouses) {
          $("#warehouse-select").append(optionGenerator(warehouse.storeID, warehouse.storeDescription));
        }
      })
      .catch(toastrError);
  }
}

let toggleInputs = async (productType) => {
  let taxExemptSetting = document.getElementById("taxExemptSettings");
  const elements = document.querySelectorAll('.visibility-on-product-type');
  elements.forEach(element => {
    if (productType === 'V' || productType === 'O') {
      element.removeAttribute('hidden');
    } else {
      element.setAttribute('hidden', '');
    }
  });

  let targetInputs = document.getElementsByClassName("toggle-visibility-on-product-type");
  if (productType === 'V' || productType === 'O') {
    for (let i = 0; i < targetInputs.length; i++) {
      $(targetInputs[i]).show();
      $(targetInputs[i]).prop('required', true);
    }

    let taxExemptPolicy = document.getElementById("taxExemptPolicy").value;
    let exemptTypePolicy = document.getElementById("exemptTypePolicy").value;
    let trackTEInvPolicy = document.getElementById("trackTEInvPolicy").value;

    if (taxExemptSetting !== null) {
      if (taxExemptPolicy && (exemptTypePolicy === "AITE" || (exemptTypePolicy !== "AITE" && trackTEInvPolicy))) {
        taxExemptSetting.hidden = false;
      }
      else taxExemptSetting.hidden = true;
    }
  }
  else {
    for (let i = 0; i < targetInputs.length; i++) {
      for (let i = 0; i < targetInputs.length; i++) {
        $(targetInputs[i]).hide();
        $(targetInputs[i]).prop('required', false);
      }
    }

    if (taxExemptSetting !== null) taxExemptSetting.hidden = true;
  }
}

let setPOSDescirption = () => {
  let posDescription = document.getElementById("posDescription");
  let description = document.getElementById("description");
  let format = document.getElementById("format");
  let unitOfMeasureSelect = $("#unitOfMeasure-select option:selected").text().split('-');

  posDescription.value = "";
  if (description.value !== "") {
    posDescription.value += description.value + " ";
  }
  if (format.value !== "") {
    posDescription.value += format.value + " ";
  }
  if (unitOfMeasureSelect.length > 1) {
    posDescription.value += unitOfMeasureSelect[1];
  }
}

let setLongDescription = () => {
  let longDescription = document.getElementById("longDescription");
  let brandSelect = $("#brand-select option:selected").text().split('-');
  let genericSelect = $("#generic-select option:selected").text().split('-');
  let characteristicSelect = $("#characteristic-select option:selected").text().split('-');

  longDescription.value = "";
  if (brandSelect.length > 1) {
    longDescription.value += brandSelect[1] + " ";
  }
  if (genericSelect.length > 1) {
    longDescription.value += genericSelect[1] + " ";
  }
  if (characteristicSelect.length > 1) {
    longDescription.value += characteristicSelect[1];
  }
}

let loadSelectFromModel = (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId} option[value="${document.getElementById(inputDataId).value}"]`).attr('selected', 'selected');
  }
}

let loadProductTypeSelectFromModel = (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId} option[value="${document.getElementById(inputDataId).value}"]`).attr('selected', 'selected');
    toggleInputs(document.getElementById(inputDataId).value)
  }
  else {
    $(`#${selectId} option[value=V`).attr('selected', 'selected');
    toggleInputs('V')
  }
}

let loadCategorySelectFromModel = async (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId} option[value="${document.getElementById(inputDataId).value}"]`).attr('selected', 'selected');
    await loadSubCategoryOptions(document.getElementById(inputDataId).value);
  }
}

let loadSubcategorySelectFromModel = async (inputDataId, selectId) => {
  if (document.getElementById(inputDataId) && document.getElementById(inputDataId).value) {
    $(`#${selectId} option[value="${document.getElementById(inputDataId).value}"]`).attr('selected', 'selected');
    await loadSubDetailsOptions(document.getElementById(inputDataId).value);
  }
}

let loadDataFromModel = async () => {
  loadProductTypeSelectFromModel("productType-select-from-model", "family-select");
  await loadCategorySelectFromModel("category-select-from-model", "category-select");
  await loadSubcategorySelectFromModel("subcategory-select-from-model", "subcategory-select");
  loadSelectFromModel("family-select-from-model", "family-select");
  loadSelectFromModel("subdetail-select-from-model", "subdetail-select");
  loadSelectFromModel("brand-select-from-model", "brand-select");
  setLongDescription();
  loadSelectFromModel("generic-select-from-model", "generic-select");
  setLongDescription();
  loadSelectFromModel("characteristic-select-from-model", "characteristic-select");
  setLongDescription();
  loadSelectFromModel("orderUnit-select-from-model", "orderUnit-select");
  loadSelectFromModel("productType-select-from-model", "productType-select");
  loadSelectFromModel("orderUnit-select-from-model", "orderUnit-select");
  loadSelectFromModel("unitOfMeasure-select-from-model", "unitOfMeasure-select");
  setPOSDescirption();
}

let setVisibilityStockCodeInput = () => {
  if (document.getElementById("isUpdateProduct") && document.getElementById("isUpdateProduct").value === "True") {
    document.getElementById("stock-code").readOnly = true;
  }
}

let toggleEditMode = () => {
  _globalIsEditing = !_globalIsEditing;
  setGlobalEditingState(_globalIsEditing);
  if (document.getElementById("isUpdateProduct").value === "True") {
    document.getElementById("isUpdateProduct").value = "False"
  }
  else {
    document.getElementById("isUpdateProduct").value = "True"
  }
  setVisibilityStockitemFormInput();
}

let setVisibilityStockitemFormInput = () => {
  let StockitemFormInputs = document.getElementsByClassName("stock-item-form-input");
  if (StockitemFormInputs) {
    for (let i = 0; i < StockitemFormInputs.length; i++) {
      if (document.getElementById("isUpdateProduct") && document.getElementById("isUpdateProduct").value === "True") {
        StockitemFormInputs[i].disabled = true;
      }
      else {
        StockitemFormInputs[i].disabled = false;
      }
    }
  }

  if (document.getElementById("isUpdateProduct") && document.getElementById("isUpdateProduct").value === "True") {
    document.getElementById("btn-cancel").hidden = true;
    document.getElementById("btn-save").hidden = true;
    document.getElementById("btn-edit").hidden = false;
    document.getElementById("btn-price").hidden = false;
    document.getElementById("btn-details").hidden = false;

    if (document.getElementById("btn-transfer")) {
      document.getElementById("btn-transfer").hidden = false;
    }

    if (document.getElementById("btn-transfer-disabled")) {
      document.getElementById("btn-transfer-disabled").hidden = false;
    }

    if (document.getElementById("btn-pricebook")) {
      document.getElementById("btn-pricebook").hidden = false;
    }
  }
  else {
    document.getElementById("btn-cancel").hidden = (document.getElementById("isAddProduct").value === "True")
    document.getElementById("btn-save").hidden = false;
    document.getElementById("btn-edit").hidden = true;
    document.getElementById("btn-price").hidden = true;
    document.getElementById("btn-details").hidden = true;

    if (document.getElementById("btn-transfer")) {
      document.getElementById("btn-transfer").hidden = true;
    }

    if (document.getElementById("btn-transfer-disabled")) {
      document.getElementById("btn-transfer-disabled").hidden = true;
    }

    if (document.getElementById("btn-pricebook")) {
      document.getElementById("btn-pricebook").hidden = true;
    }
  }
}

let cancelButtonClicked = () => {
  const modelDataInput = document.getElementById("modelData");
  let modelData = JSON.parse(modelDataInput.value);
  const form = document.getElementById("stockItemsForm");

  for (let key in modelData) {
    if (modelData.hasOwnProperty(key)) {
      let capitalizedKey = capitalizeFirstLetter(key);
      const inputElement = form.querySelector(`[name="${capitalizedKey}"]`);
      if (inputElement) {
        inputElement.value = modelData[key];
      }
    }
  }

  if ($("#eligibleForTaxExempt")) {
    $("#eligibleForTaxExempt").prop("checked", modelData.eligibleTaxExemptionCheckBox);
  }

  if ($("#eligibleForFuelRebate")) {
    $("#eligibleForFuelRebate").prop("checked", modelData.eligibleFuelRebateCheckBox);
  }

  if ($("#qualifiedForTaxRebate")) {
    $("#qualifiedForTaxRebate").prop("checked", modelData.qualifiedTaxRebateCheckBox);
  }

  if ($("#eligibleForTaxRebate")) {
    $("#eligibleForTaxRebate").prop("checked", modelData.eligibleTaxRebateCheckBox);
  }

  if ($("#availability")) {
    $("#availability").prop("checked", modelData.availabilityCheckBox);
  }

  if ($("#eligibleForLoyalty")) {
    $("#eligibleForLoyalty").prop("checked", modelData.eligibleLoyaltyCheckBox);
  }

  if ($("#label")) {
    $("#label").prop("checked", modelData.labelCheckBox);
  }

  if ($("#scalable")) {
    $("#scalable").prop("checked", modelData.sbywghtCheckBox);
  }

  $("span.text-danger").text("");
  loadDataFromModel();
  toggleEditMode();
}

let goToStockItemsPage = () => {
  let url = `/StockItems?isFromStockForm=${true}`;
  if (document.getElementById("isUpdateProduct").value === "False") {
    url = "/StockItems";
  }

  safelyNavigateTo(url);
}

let goToDetailsPage = () => {
  let stockCode = document.getElementById("stock-code").value;
  const url = `/StockItemDetails?stockCode=${stockCode}`;
  safelyNavigateTo(url);
}

let loadTransferButton = () => {
  if (document.getElementById("isTransferred") !== null) {
    if (document.getElementById("isTransferred").value === "True") {
      if (document.getElementById("btn-transfer-disabled") !== null) {
        $("#btn-transfer-disabled").show();
      }
      if (document.getElementById("btn-transfer") !== null) {
        $("#btn-transfer").hide();
      }
    }
    else {
      if (document.getElementById("btn-transfer-disabled") !== null) {
        $("#btn-transfer-disabled").hide();
      }
      if (document.getElementById("btn-transfer") !== null) {
        $("#btn-transfer").show();
      }
    }
  }
}

let transferButtonClicked = () => {
  let stockCode = $("#stock-code").val();
  let url = `stockItemForm/UpdateStockItemTransfer?stockCode=${stockCode}`;

  ajaxGetAsync(url)
    .then(() => {
      if ($("#isTransferred") !== null) {
        $("#isTransferred").val("True");
        loadTransferButton();
      }
      toastr.success($("#addedToTransferList").val());
    })
    .catch(() => toastr.error($("#failedToAddToTransferList").val()));
}

let onDetailsButtonClick = () => {
  let stockCode = $("#stock-code").val();
  const url = `StockItemDetails?stockCode=${stockCode}`;
  safelyNavigateTo(url);
}

let priceButtonClicked = () => {
  let stockCode = $("#stock-code").val();
  const url = `StockPrice?stockCode=${stockCode}`;
  safelyNavigateTo(url);
}

$("#stock-code").focusout(function () {
  let expandTo13DigitsPolicy = (document.getElementById("expandTo13DigitsPolicy").value === "True");
  let stockCode = $(this).val();
  if (!isNaN(stockCode) && expandTo13DigitsPolicy) {
    let codeLength = stockCode.length;
    if (codeLength > 13) {
      let warningMessage = `${document.getElementById("stockCodeAlreadyMoreThan13DigitsWarning").value}`;
      warningMessage = warningMessage.replace("()", `${codeLength}`);
      toastr.warning(warningMessage);
    }
    let zeroes = "";
    while (codeLength < 13) {
      zeroes += "0";
      codeLength++;
    }
    $(this).val(zeroes + stockCode);
  }
});

function goToPriceBookTransfer() {
  selectNavItem("routineMaintenance");
  selectNavLink("pricebook-transfer");
  const url = `/PricebookTransfer`;
  safelyNavigateTo(url);
}
