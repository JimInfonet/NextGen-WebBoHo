let isTaxAvailable = false;
let isChargesAvailable = false;
let isTaxClicked = false;
let isTaxExemptDepartmentClicked = false;
let isTaxExemptSubDepartmentClicked = false;
let isTaxExemptDetailClicked = false;
let isProductDiscountCodeDepartmentClieckd = false;
let isProductDiscountCodeSubDepartmentClieckd = false;
let isProductDiscountCodeDetailClieckd = false;
let appModeBackOffice = ($("#appModeBackOffice").val() === "True");
let spinner = `<div class="d-flex justify-content-center">
                  <div class="spinner-border" role="status">
                    <span class="visually-hidden">Loading...</span>
                  </div>
                </div>`

const smallIntegerLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_INTEGER_LIMIT;
const smallIntegerLowerLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_INTEGER_LOWER_LIMIT;
const departmentText = CONSTANT_TEXTS.DEPARTMENT;
const subDepartmentText = CONSTANT_TEXTS.SUB_DEPARTMENT;
const detailText = CONSTANT_TEXTS.DETAIL;

let clickedTaxOrChargesTableType = departmentText;
let noProductDataForTaxesAndCharges = false;
let products = [];

$(async function () {
  await loadDepartments();
  $("#newEodGroupFormId").hide();
  $("#deleteEodGroupFormId").hide();
  $("#linkEodGroupFormId").hide();
  if (appModeBackOffice) {
    await renderTaxSelectionListForTaxes();
    await renderChargesSelectionListForCharges();
  }
  setButtonsHeight('.button-group button');
});

let createDatatable = async (type) => {
  let dataTable;
  let data;
  let tableId;
  let dataTableColumns;

  if (type === departmentText) {
    dataTable = departmentDatatable;
    data = departments;
    tableId = "categories-grid";
    dataTableColumns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "deptId", },
      { "data": "deptName", },
      { "data": "eodDetail", },
      { "data": "eodGroup", },
      { "data": "cntDetail", },
    ];
  }
  else if (type === subDepartmentText) {
    dataTable = subDepartmentDatatable;
    data = subDepartments;
    tableId = "subCategories-grid";
    dataTableColumns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "subDeptId", },
      { "data": "subDeptName", },
    ];
  }
  else if (type === detailText) {
    dataTable = subDetailDatatable;
    data = subDetails;
    tableId = "subDetail-grid";
    dataTableColumns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "subDetailId", },
      { "data": "subDetailName", },
    ];
  }

  if (dataTable) {
    dataTable.state.clear();
    dataTable.clear();
    dataTable.destroy();
  }

  let props = createDataTableProp();
  props.data = data;
  props.scrollY = '440px';
  props.paging = false;
  props.searching = false;
  props.columns = dataTableColumns;
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];
  dataTable = createDataTableGeneric(tableId, props, true);

  $(`#${tableId} tbody`)
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        dataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        if (type === departmentText) {
          selectedDepartmentTableRow = $(this).index();
        }
      }

      if (type === departmentText) {
        selectedDepartmentRowData = dataTable.row(this).data();
        selectedDepartment = $(this).find("td:first")[0].innerText;
        await loadSubDepartments();
      }
      else if (type === subDepartmentText) {
        selectedSubDepartmentRowData = dataTable.row(this).data();
        selectedSubDepartment = $(this).find("td:first")[0].innerText;
        await loadSubDetails();
      }
      else {
        selectedDetailRowData = dataTable.row(this).data();
        selectedDetail = $(this).find("td:first")[0].innerText;
      }
    });

  if (data.length > 0) {
    if (type === departmentText) {
      $(`#${tableId} tbody tr:eq(${selectedDepartmentTableRow})`).addClass('selected-grid-row');
      selectedDepartmentRowData = dataTable.row(0).data();
    }
    else if (type === subDepartmentText) {
      $(`#${tableId} tbody tr:eq(0)`).addClass('selected-grid-row');
      selectedSubDepartmentRowData = dataTable.row(0).data();
    }
    else {
      $(`#${tableId} tbody tr:eq(0)`).addClass('selected-grid-row');
      selectedDetailRowData = dataTable.row(0).data();
    }
  }

  if (type === departmentText) {
    departmentDatatable = dataTable;
  }
  else if (type === subDepartmentText) {
    subDepartmentDatatable = dataTable;
  }
  else if (type === detailText) {
    subDetailDatatable = dataTable;
  }
}

let productCheckboxChanged = () => {
  const checkboxes = document.querySelectorAll('.taxProductCheckBox');
  let isAnyUnchecked = false;

  checkboxes.forEach(checkbox => {
    if (!checkbox.checked) {
      isAnyUnchecked = true;
      return;
    }
  });

  $("#checkAllProductsForTaxOrCharges").prop("checked", !isAnyUnchecked);
}

let loadProductFortaxes = async () => {
  let url = `ProductClassification/GetStockItemsByDepartmentSubDepartmentAndDetail`;

  let data = {
    department: selectedDepartment
  }

  if (clickedTaxOrChargesTableType === subDepartmentText) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment
    }
  }
  else if (clickedTaxOrChargesTableType === detailText) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment,
      detail: selectedDetail
    }
  }

  await ajaxGetAsync(url, data)
    .then((response) => {
      products = response;

      if (products.length === 0) {
        showNoProductsAlertModal(clickedTaxOrChargesTableType);
        noProductDataForTaxesAndCharges = true;
        return;
      }
      else {
        noProductDataForTaxesAndCharges = false;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let renderProductCheckboxesForTaxes = () => {
  let taxCheckboxes = $("#taxOrChargesCheckboxes");

  taxCheckboxes.html("");
  products.forEach(product => {
    taxCheckboxes.append(
      `<div class="form-check mt-2">
        <input class="form-check-input stock-item-form-input taxProductCheckBox" value="${product.stockCode}" onchange="productCheckboxChanged()" type="checkbox"  id="productCheckbox-${product.stockCode}">
        <label class="form-check-label ms-1" for="productCheckbox-${product.stockCode}">
          ${product.stockCode} ${product.description}
        </label>
      </div>`);
  }); 
}

let renderTaxSelectionListForTaxes = async () => {
  let url = `ProductClassification/GetProductTaxes`;
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      let taxes = response;

      isTaxAvailable = (taxes.length > 0 && departments.length > 0);
      $("#taxesButtonId").prop("disabled", !isTaxAvailable);

      const container = $("#taxOrChargesOptions");
      container.empty();

      taxes.forEach(tax => {
        container.append(`
       <option value="${tax.taxName} - ${tax.taxCode} - ${tax.taxDescription}">
          ${tax.taxName} - ${tax.taxCode} - ${tax.taxDescription}
       </option>
    `)
      });
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let renderChargesSelectionListForCharges = async () => {
  let url = `/DataDefinition/GetAssociations`;
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      let charges = response;

      isChargesAvailable = (charges.length > 0 && departments.length > 0);
      $("#chargesButtonId").prop("disabled", !isChargesAvailable);

      const container = $("#taxOrChargesOptions");
      container.empty();

      charges.forEach(charge => {
        container.append(`
       <option value="${charge.code} - ${charge.description} - ${charge.price}">
          ${charge.code} - ${charge.description}
       </option>
    `)
      });
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let getSelectedTaxesOrCharges = (isTax) => {
  let items = $('#taxOrChargesOptions').val();
  let selectedItems = [];

  items.forEach(item => {
    let data;
    if (isTax) {
      data = {
        taxName: item.split("-")[0].trim(),
        taxCode: item.split("-")[1].trim(),
        taxDescription: item.split("-")[2].trim()
      };
    } else {
      data = {
        code: item.split("-")[0].trim(),
        description: item.split("-")[1].trim()
      };
    }

    selectedItems.push(data);
  });

  return selectedItems;
}

let getSelectedProducts = () => {
  let selectedProducts = [];
  $(".taxProductCheckBox").each(function () {
    if ($(this).prop("checked")) {
      selectedProducts.push($(this).val());
    }
  });

  return selectedProducts;
}

let addTaxesToProducts = async () => {
  let selectedTaxes = getSelectedTaxesOrCharges(true);
  let selectedProducts = getSelectedProducts();

  if (selectedProducts.length === 0) {
    toastr.warning($("#pleaseSelectStockCodes").val());
    return;
  }

  if (selectedTaxes.length === 0) {
    toastr.warning($("#pleaseSelectTaxes").val());
    return;
  }

  const duplicateTaxNames = selectedTaxes.filter(
    (item, index, self) => self.findIndex(i => i.taxName === item.taxName) !== index
  );

  if (duplicateTaxNames.length > 0) {
    toastr.warning($("#youHaveSelectedSameTaxMoreThanOneTime").val());
    return;
  }

  let taxesList = "\n";
  selectedTaxes.forEach(tax => {
    taxesList += `${tax.taxName} - ${tax.taxCode} - ${tax.taxDescription}\n`
  })

  let title = $('#add').val();
  let message = $('#youAreGoingToAdd').val()
    .replace("{0}", taxesList)
    .replace("{1}", selectedProducts.length);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {

      let data = {
        taxNameAndCodes: selectedTaxes,
        stockCodes: selectedProducts
      }

      url = `ProductClassification/InsertStockTaxByStockTaxData`;
      await ajaxPostAsync(url, data)
        .then((response) => {
          if (response === true) {
            toastr.success($("#taxesSuccessfullyAdded").val())
          }
        })
        .catch((error) => {
          toastr.error(error);
        });
    }
  });
}

let deleteTaxesFromProducts = async () => {
  let selectedTaxes = getSelectedTaxesOrCharges(true);
  let selectedProducts = getSelectedProducts();

  if (selectedProducts.length === 0) {
    toastr.warning($("#pleaseSelectStockCodes").val());
    return;
  }

  if (selectedTaxes.length === 0) {
    toastr.warning($("#pleaseSelectTaxes").val());
    return;
  }

  const duplicateTaxNames = selectedTaxes.filter(
    (item, index, self) => self.findIndex(i => i.taxName === item.taxName) !== index
  );

  if (duplicateTaxNames.length > 0) {
    toastr.warning($("#youHaveSelectedSameTaxMoreThanOneTime").val());
    return;
  }

  let taxesList = "\n";
  selectedTaxes.forEach(tax => {
    taxesList += `${tax.taxName} - ${tax.taxCode} - ${tax.taxDescription}\n`
  })

  let title = $('#delete').val();
  let message = $('#youAreGoingToDelete').val()
    .replace("{0}", taxesList)
    .replace("{1}", selectedProducts.length);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {

      let data = {
        taxNameAndCodes: selectedTaxes,
        stockCodes: selectedProducts
      }

      url = `ProductClassification/DeleteStockTaxByStockTaxData`;
      await ajaxPostAsync(url, data)
        .then((response) => {
          if (response === true) {
            toastr.success($("#taxesSuccessfullyDeleted").val())
          }
        })
        .catch((error) => {
          toastr.error(error);
        });
    }
  });
}

let addChargesToProducts = async () => {
  let selectedCharges = getSelectedTaxesOrCharges(false);
  let selectedProducts = getSelectedProducts();

  if (selectedProducts.length === 0) {
    toastr.warning($("#pleaseSelectStockCodes").val());
    return;
  }

  if (selectedCharges.length === 0) {
    toastr.warning($("#pleaseSelectCharges").val());
    return;
  }

  let chargeList = "\n";
  selectedCharges.forEach(charge => {
    chargeList += `${charge.code} - ${charge.description}\n`
  })

  let title = $('#add').val();
  let message = $('#youAreGoingToAdd').val()
    .replace("{0}", chargeList)
    .replace("{1}", selectedProducts.length);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let codes = [];

      selectedCharges.forEach(charge => {
        codes.push(charge.code);
      });

      let data = {
        charges: codes,
        stockCodes: selectedProducts
      }

      url = `ProductClassification/InsertStockChargesByStockChargeData`;
      await ajaxPostAsync(url, data)
        .then((response) => {
          if (response === true) {
            toastr.success($("#chargesSuccessfullyAdded").val())
          }
        })
        .catch((error) => {
          toastr.error(error);
        });
    }
  });
}

let deleteChargesFromProducts = async () => {
  let selectedCharges = getSelectedTaxesOrCharges(false);
  let selectedProducts = getSelectedProducts();

  if (selectedProducts.length === 0) {
    toastr.warning($("#pleaseSelectStockCodes").val());
    return;
  }

  if (selectedCharges.length === 0) {
    toastr.warning($("#pleaseSelectCharges").val());
    return;
  }

  let chargeList = "\n";
  selectedCharges.forEach(charge => {
    chargeList += `${charge.code} - ${charge.description}\n`
  })

  let title = $('#delete').val();
  let message = $('#youAreGoingToDelete').val()
    .replace("{0}", chargeList)
    .replace("{1}", selectedProducts.length);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let codes = [];

      selectedCharges.forEach(charge => {
        codes.push(charge.code);
      });

      let data = {
        charges: codes,
        stockCodes: selectedProducts
      }

      url = `ProductClassification/DeleteStockChargesByStockChargeData`;
      await ajaxPostAsync(url, data)
        .then((response) => {
          if (response === true) {
            toastr.success($("#chargesSuccessfullyDeleted").val())
          }
        })
        .catch((error) => {
          toastr.error(error);
        });
    }
  });
}

$("#taxesOrChargesModal").on("shown.bs.modal", async function () {
  renderProductCheckboxesForTaxes();
  if (isTaxClicked) {
    await renderTaxSelectionListForTaxes();
  }
  else {
    await renderChargesSelectionListForCharges();
  }
});

let taxesOrChargesButtonClicked = async (isTaxButtonClicked, tableType) => {
  if (isTaxButtonClicked) {
    $("#taxesButtons").show();
    $("#chargesButtons").hide();

    $("#taxesTitle").show();
    $("#chargesTitle").hide();
  } else {
    $("#taxesButtons").hide();
    $("#chargesButtons").show();

    $("#taxesTitle").hide();
    $("#chargesTitle").show();
  }

  isTaxClicked = isTaxButtonClicked;
  clickedTaxOrChargesTableType = tableType;

  await loadProductFortaxes();

  if (noProductDataForTaxesAndCharges) {
    return;
  }

  let taxesOrChargesModalTitleText = `${$("#dept").val()} ${selectedDepartmentRowData.deptId} ${selectedDepartmentRowData.deptName}`;

  if (tableType === subDepartmentText) {
    taxesOrChargesModalTitleText += `<br>${$("#subDept").val()} ${selectedSubDepartmentRowData.subDeptId} ${selectedSubDepartmentRowData.subDeptName}`;
  }
  else if (tableType === detailText) {
    taxesOrChargesModalTitleText += `<br>${$("#subDept").val()} ${selectedSubDepartmentRowData.subDeptId} ${selectedSubDepartmentRowData.subDeptName}`;
    taxesOrChargesModalTitleText += `<br>${$("#subDetail").val()} ${selectedDetailRowData.subDetailId} ${selectedDetailRowData.subDetailName}`;
  }

  $("#taxesOrChargesModalTitle").html(taxesOrChargesModalTitleText);

  $("#taxOrChargesCheckboxes").html(spinner);
  $("#taxOrChargesOptions").html(spinner);

  $("#checkAllProductsForTaxOrCharges").prop("checked", false);
  $("#taxesOrChargesModal").modal("show");
}

$('#taxOrChargesOptions').mousedown(function (e) {
  e.preventDefault();

  let originalScrollTop = $(this).scrollTop();
  let isOptionSelected = $(e.target).prop('selected');
  $(e.target).prop('selected', !isOptionSelected);

  $(this).focus().scrollTop(originalScrollTop);

  let selectedOptionTop = $(e.target).position().top;
  $(this).animate({
    scrollTop: originalScrollTop + selectedOptionTop - 100
  }, 200);

  return false;
});

$('#checkAllProductsForTaxOrCharges').on('change', function () {
  if ($(this).prop('checked')) {
    $('.taxProductCheckBox').prop('checked', true);
  } else {
    $('.taxProductCheckBox').prop('checked', false);
  }
});

let loadTaxExemptCategories = async () => {
  let url = "StockItemDetails/GetAllTaxExemptCategory";
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      taxExemptCategories = JSON.parse(response);
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let renderTaxExemptCategories = async () => {
  let url = `StockItemDetails/GetAllTaxExemptCategory`;
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      taxExemptCategories = JSON.parse(response);

      const container = $("#newTECategorySelect");
      container.empty();

      container.html(optionGeneratorDefault($("#newTECategoryText").val()));
      taxExemptCategories.forEach(taxExemptCategory => {
        container.append(`
       <option value="${taxExemptCategory.id} - ${taxExemptCategory.name}">
          ${taxExemptCategory.id} - ${taxExemptCategory.name}
       </option>
    `)
      });
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let renderTaxExemptRates = async () => {
  let url = `StockItemDetails/GetAllTaxExemptRate`;
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      taxExemptRates = JSON.parse(response);

      const container = $("#taxTypeSelect");
      container.empty();

      container.html(optionGeneratorDefault($("#taxType").val()));
      taxExemptRates.forEach(taxExemptRate => {
        container.append(`
       <option value="${taxExemptRate.taxCode} - ${taxExemptRate.productCode}">
          ${taxExemptRate.taxCode} - ${taxExemptRate.taxDescirption}
       </option>
    `)
      });
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let taxTypeSelectChanged = (taxType) => {
  $("#productCode").val(taxType.split(" - ")[1])
}

let calculateEquivalentUnits = () => {
  let quantityPerPkg = $("#quantityPkg").val();
  let baseUnitQuantity = $("#baseUnitQuantity").val();

  if (!baseUnitQuantity || baseUnitQuantity === "0") {
    $("#baseUnitQuantity").val(1);
    baseUnitQuantity = 1;
  }

  if (baseUnitQuantity <= 0) {
    $("#baseUnitQuantity").val(1);
    baseUnitQuantity = 1;
  }

  let equivalentUnits = quantityPerPkg * baseUnitQuantity;

  $("#equivalentUnits").val(equivalentUnits);
}

let clearAddNewTaxExemptModal = () => {
  $("#setTaxExemptCategoryForm").hide();
  $("#newTECategoryInput").val("");
  $("#newTECategorySelect").val("");
  $("#taxTypeSelect").val("");
  $("#equivalentUnits").val("");
  $("#quantityPkg").val("");
  $("#baseUnitQuantity").val("1");
  $("#productCode").val("");
  if ($("#newTEPrice")) {
    $("#newTEPrice").val("");
  }
}

let setTaxExemptButtonClicked = async () => {
  let isDepartmentExistsInFuelDept = await CheckDepartmentExistsInFuelDept(selectedDepartment);
  $("#taxExemptButtons").hide();
  $("#setTaxExemptCategoryForm").show();

  if ($("#taxExemptByRatePolicy").val() === "Yes" && $("#taxExemptTypePolicy").val() === "SITE") {
    if (!isDepartmentExistsInFuelDept) {
      $("#newTEPriceLabel").hide();
      $("#newTEPrice").hide();
    }
    else {
      $("#newTEPriceLabel").show();
      $("#newTEPrice").show();
    }
  }
  else if ($("#taxExemptByRatePolicy").val() === "No") {
    $("#newTEPriceLabel").show();
    $("#newTEPrice").show();
  }

  if (isDepartmentExistsInFuelDept) {
    if ($("#newTEPrice")) {
      $("#newTEPrice").prop("disabled", true);
    }
  }
  else {
    if ($("#newTEPrice")) {
      $("#newTEPrice").prop("disabled", false);
    }
  }

  $("#setTaxExemptCancelButtonId").prop("disabled", true);
}

let setTaxExemptFormCancelButtonClicked = () => {
  $("#taxExemptButtons").show();
  $("#setTaxExemptCategoryForm").hide();
  $("#setTaxExemptCancelButtonId").prop("disabled", false);
  clearAddNewTaxExemptModal();
}

let addNewTaxExempt = async () => {
  let selectedCategory = $("#newTECategoryInput").val().split(" - ")[0];
  let selectedTaxType = "";
  if ($("#taxTypeSelect") && $("#taxTypeSelect").val()) {
    selectedTaxType = $("#taxTypeSelect").val().split(" - ")[0];
  }
  let equivalentUnits = $("#equivalentUnits").val();
  let quantityPkg = $("#quantityPkg").val();
  let baseUnitQuantity = $("#baseUnitQuantity").val();
  let productCode = $("#productCode").val();
  let userCode = $("#userCode").val();
  let taxFreePrice = $("#newTEPrice").val();

  if (taxFreePrice > 214748 || taxFreePrice < -99999) {
    toastr.warning($("#taxFreePriceRange").val());
    return;
  }

  if (quantityPkg > smallIntegerLimit || quantityPkg < smallIntegerLowerLimit) {
    toastr.warning($("#quantityPerPkgRange").val());
    return;
  }

  if (baseUnitQuantity > smallIntegerLimit) {
    toastr.warning($("#baseUnitQuantityRange").val());
    return;
  }

  let department = selectedDepartment;
  let subDepartment = null;
  let detail = null;

  if (isTaxExemptSubDepartmentClicked) {
    subDepartment = selectedSubDepartment;
  }
  else if (isTaxExemptDetailClicked) {
    subDepartment = selectedSubDepartment;
    detail = selectedDetail
  }

  let data = {
    department: department,
    SubDepartment: subDepartment,
    detail: detail,
    taxType: !isNullUndefinedOrWhiteSpace(selectedTaxType) ? selectedTaxType : 0,
    unitsPerPkg: !isNullUndefinedOrWhiteSpace(equivalentUnits) ? equivalentUnits : 0,
    category: !isNullUndefinedOrWhiteSpace(selectedCategory) ? selectedCategory : '0',
    quantityPerPkg: !isNullUndefinedOrWhiteSpace(quantityPkg) ? quantityPkg : 0,
    baseUnitQty: !isNullUndefinedOrWhiteSpace(baseUnitQuantity) ? baseUnitQuantity : 0,
    taxFreePrice: !isNullUndefinedOrWhiteSpace(taxFreePrice) ? taxFreePrice : 0,
    productCode: !isNullUndefinedOrWhiteSpace(productCode) ? productCode : '0',
    employeeId: userCode
  }

  url = `ProductClassification/AddProductTaxExemptForDepartmentSubDepartmentAndDetail`;
  await ajaxPostAsync(url, data)
    .then((response) => {
      if (response === true) {
        toastr.success($("#taxExemptSuccessfullyAdded").val())
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  $("#taxExemptCategoryModal").modal("hide");
  clearAddNewTaxExemptModal();
}

let deleteProductTaxExempt = async () => {
  let data = {
    department: selectedDepartment,
  }

  let categoryData = selectedDepartment;
  if (isTaxExemptSubDepartmentClicked) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment
    }

    categoryData += ('-' + selectedSubDepartment);
  }
  else if (isTaxExemptDetailClicked) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment,
      detail: selectedDetail,
    }

    categoryData += ('-' + selectedSubDepartment);
    categoryData += ('-' + selectedDetail);
  }

  let title = "";
  let message = $('#areYouSureToDeleteTaxExemptCategory').val().replace("{0}", categoryData);


  await showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      url = `ProductClassification/DeleteProductTaxExempt`;
      await ajaxPostAsync(url, data)
        .then((response) => {
          if (response === true) {
            toastr.success($("#taxExemptSuccessfullyDeleted").val())
            $("#taxExemptCategoryModal").modal("hide");
          }
        })
        .catch((error) => {
          toastr.error(error);
        });
    }
  });

  clearAddNewTaxExemptModal();
}

let newTECategorySelectChanged = (newTECategory) => {
  $("#newTECategoryInput").val(newTECategory);
}

let taxExemptButtonClicked = async (tableType) => {
  isTaxExemptDepartmentClicked = (tableType === departmentText);
  isTaxExemptSubDepartmentClicked = (tableType === subDepartmentText);
  isTaxExemptDetailClicked = (tableType === detailText);

  let isExists = await isProductsExists(tableType);
  if (!isExists) {
    showNoProductsAlertModal(tableType);
    return;
  }

  $("#taxExemptButtons").show();
  clearAddNewTaxExemptModal();
  $("#taxExemptCategoryModal").modal("show");
  $("#setTaxExemptCancelButtonId").prop("disabled", false);
  await renderTaxExemptCategories();
  await renderTaxExemptRates();
}

if ($("#quantityPkg")) {
  $("#quantityPkg").on('keypress', function (event) {
    const value = $(this).val();
    if (value.length >= 6) return false;
    return onlyDigitPlusMinusKeyPress(event);
  });
}

if ($("#equivalentUnits")) {
  $("#equivalentUnits").on('keypress', function (event) {
    const value = $(this).val();
    if (value.length >= 6) return false;
    return onlyDigitPlusMinusKeyPress(event);
  });
}

if ($("#newTEPrice")) {
  $("#newTEPrice").on('keypress', function (event) {
    const value = $(this).val();
    if (value.length >= 6) return false;
    return onlyDigitDecimalPlusMinusKeyPress(event);
  });
}

let setProductDiscountCodeModal = async () => {
  let productDiscountCode = $("#productDiscount").val();
  if (!productDiscountCode || !(productDiscountCode >= 0 && productDiscountCode <= 99)) {
    toastr.warning($("#discountCodeShouldBeBetween0And99Inclusive").val());
    return;
  }

  let data = {
    department: selectedDepartment
  }

  if (isProductDiscountCodeSubDepartmentClieckd) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment
    }
  }
  else if (isProductDiscountCodeDetailClieckd) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment,
      detail: selectedDetail
    }
  }

  url = `ProductClassification/AddProductDiscountCode?productDiscountCode=${productDiscountCode}`;
  await ajaxPostAsync(url, data)
    .then((response) => {
      if (response === true) {
        toastr.success($("#productDiscountCodeSuccessfullyAdded").val())
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  $("#productDiscountCodeModal").modal("hide");
}

let deleteProductDiscountCode = async () => {
  let data = {
    department: selectedDepartment
  }

  let categoryData = selectedDepartment;
  if (isProductDiscountCodeSubDepartmentClieckd) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment
    }

    categoryData += ('-' + selectedSubDepartment);
  }
  else if (isProductDiscountCodeDetailClieckd) {
    data = {
      ...data,
      subDepartment: selectedSubDepartment,
      detail: selectedDetail
    }

    categoryData += ('-' + selectedSubDepartment);
    categoryData += ('-' + selectedDetail);
  }

  let title = document.getElementById("confirmDelete").value;
  let message = document.getElementById("areYouSureToDeleteProductDiscountCodeForCategory").value.replace("{0}", categoryData);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {

      url = `ProductClassification/DeleteProductDiscountCode`;
      await ajaxPostAsync(url, data)
        .then((response) => {
          if (response === true) {
            toastr.success($("#productDiscountCodeSuccessfullyDeleted").val());
            $("#productDiscountCodeModal").modal("hide");
          }
        })
        .catch((error) => {
          toastr.error(error);
        });
    }
  });
}

let enableControlInProductDiscountCodeModal = (enable) => {
  $("#productDiscountCodeDeleteButtonId").prop("disabled", !enable);
  $("#productDiscountCodeSetButtonId").prop("disabled", !enable);
  $("#setProductDiscountCodeCancelButtonId").prop("disabled", !enable);
}

let productDiscountCodeButtonClicked = async (tableType) => {
  isProductDiscountCodeDepartmentClieckd = (tableType === departmentText);
  isProductDiscountCodeSubDepartmentClieckd = (tableType === subDepartmentText);
  isProductDiscountCodeDetailClieckd = (tableType === detailText);

  let isExists = await isProductsExists(tableType);
  if (!isExists) {
    showNoProductsAlertModal(tableType);
    return;
  }

  $("#productDiscountCodeForm").hide();
  $("#productDiscountCodeModal").modal("show");
  enableControlInProductDiscountCodeModal(true);
}

let setProductDiscountCodeModalButtonClicked = () => {
  $("#productDiscount").val("");
  $("#productDiscountCodeForm").show();
  enableControlInProductDiscountCodeModal(false);
}

let productDiscountCodeFormCancelButtonClicked = () => {
  $("#productDiscount").val("");
  $("#productDiscountCodeForm").hide();
  enableControlInProductDiscountCodeModal(true);
}

let isProductsExists = async (tableType) => {
  let isExist = false;
  let url = `ProductClassification/IsProductsExist?departmentId=${selectedDepartment}`;
  if (tableType === subDepartmentText || tableType === detailText) url += `&subDepartmentId=${selectedSubDepartment}`;
  if (tableType === detailText) url += `&detailId=${selectedDetail}`;

  await ajaxGetAsync(url, {})
    .then(async (response) => {
      isExist = JSON.parse(response);
    })
    .catch((error) => {
      toastr.error(error);
    });

  return isExist
}

let showNoProductsAlertModal = (tableType) => {
  let title = $('#noProducts').val();
  let noProductAlertMessage = $('#noProductsFor').val();

  if (tableType === departmentText) {
    noProductAlertMessage = noProductAlertMessage.replace("{0}", `${$("#dept").val()} ${selectedDepartmentRowData.deptName}`);
  }
  else if (tableType === subDepartmentText) {
    noProductAlertMessage = noProductAlertMessage.replace("{0}", `${$("#subDept").val()} ${selectedSubDepartmentRowData.subDeptName}`);
  }
  else {
    noProductAlertMessage = noProductAlertMessage.replace("{0}", `${$("#subDetail").val()} ${selectedDetailRowData.subDetailName}`);
  }

  showAlertModal(title, noProductAlertMessage).then((result) => { });
}
