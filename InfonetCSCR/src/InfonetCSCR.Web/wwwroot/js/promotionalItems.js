let promotionalItemsOptions = [];
let errorMessage = "";
let promotionalItemsDatatable;
let promotionalItemData;
let dynamicPromotionalItemData;
let linkData;
let departmentData;
let subDepartmentData;
let subDetailData;
let cellIndex;
let rowData;
let rowIndex;
let cellData = "";
let descriptionCellIndex = 7;
let descriptionCell;
let stockCodeRowData = {};
let startIndex = 3;
let rowArray = [5, 6, 7];
let isDataTableValidated = true;
let isDiscountTypeByItem = false;
let selectedPromotionalId;
let previousRowIndex = 0;
let nextRowIndex = 0;
let hasToAdd;
let fillGridForEdit = true;
let isAdd = true;
let hasToAddPromotionalItemData = false;
let rowDataArray = [];
let lastEnteredValue = null;
let isAddOrEditMode = false;
let stockCodeDataTableDataLoaded = false;
let isValidate = false;
let deptName;
let subDeptName;
let lastSelectedRowForDelete = null;
let numberOfRowsAtPresent;
let itemDeleted = false;
let stockCodeOldValue = "";
let addForFirstTime = true;
let $inputField;
let doubleClickTimeout = null;
let clickCount = 0;
let isEnterKeyPressed = false;
let stockCodeArray = [];
let modalSource = "";
const urlParams = new URLSearchParams(window.location.search);
const param = urlParams.get('promoId');
let startDate = document.getElementById('start-date');
let endDate = document.getElementById('end-date');

$(async function () {
  const storedSuccessMessage = localStorage.getItem('successMessage');

  if (storedSuccessMessage) {
    toastr.success(decodeURIComponent(storedSuccessMessage));
    localStorage.removeItem('successMessage');
  }

  await loadPromotionalItems().then(() => renderPromotionalItemsOptions());
  await loadStockCode();

  if (param === null || param === '') {
    $('#promotional-items-container-id').val($('#promotional-items-container-id option:first').val()).change();
  }
  else {
    $(`#promotional-items-container-id option[value='${param}']`).attr('selected', 'selected').change();
  }

  addEventListernerToDateField(startDate);
  addEventListernerToDateField(endDate);
});

async function loadPromotionalItems() {
  const url = "/PromotionalItems/GetPromotions";

  try {
    const promotions = await ajaxGetAsync(url);
    promotionalItemsOptions = promotions;
  } catch (error) {
    errorMessage = $('#error-message-for-list-load').val();
    toastr.error(errorMessage);
  }

  $('#primary-button-section-id').hide();
  $('#secondary-button-section-id').show();
}

function renderPromotionalItemsOptions() {
  let promotialItemsContainer = $('#promotional-items-container-id');
  promotialItemsContainer.empty();

  promotionalItemsOptions.forEach(promotion => {
    let option = $('<option>').attr('value', promotion.promoId).text(`${promotion.promoId} -- ${promotion.description}`);
    promotialItemsContainer.append(option);
  });
  updatePromotionalItemCountDisplay();
}

function updatePromotionalItemCountDisplay() {
  let countDisplay = $('#count-display');
  let itemCount = $('#promotional-items-container-id option').length.toString();
  let countShowingMessage = $('#countShowingMessage').val();
  countShowingMessage = countShowingMessage.replace("_COUNT_", itemCount);

  countDisplay.text(countShowingMessage);
}

$('#promotional-items-container-id').on('change', async function () {
  let selectedOption = $(this).val();
  let promoId = selectedOption.split('--')[0].trim();
  selectedPromotionalId = promoId;

  await fillFormData(promoId);
  await fillGrid(promoId);
  makeFormUneditable('promotional-item-form');
  isAddOrEditMode = false;

});

async function fillFormData(promoId) {
  let url = "/promotionalitems/getpromotionbypromoid?promoId=" + promoId;
  let data = await ajaxGetAsync(url);

  $('#promo-id').val(data.promoId);
  $('#description').val(data.description);
  $('#discountType-select').val(data.discType);
  $('#price').val(data.price);

  let startDate = new Date(data.startDate);
  let start = getFormattedDateyyyyMMdd(startDate);
  $('#start-date').val(start);
  $('#startDateTmpData').val(start);

  let endDate = new Date(data.endDate);
  let end = getFormattedDateyyyyMMdd(endDate);
  $('#end-date').val(end);
  $('#endDateTmpData').val(end);
  $('#day-select').val(data.day);

  $("#start-date-span").text(start);
  $("#end-date-span").text(end);

  if (data.discType === "B") {
    $('#radio-section-id').show();
    $('#ammount').hide();

    let Option = data.prType === "%" ? "optPercentage" : "optDollar";
    $('input[name="prType"][value="' + Option + '"]').prop('checked', true);
    isDiscountTypeByItem = true;
  }
  else {
    $('#radio-section-id').hide();
    $('#ammount').show();
    isDiscountTypeByItem = false;
  }
}

$("#price").on('keypress', function (event) {
  return onlyDigitAndDecimalKeyPress(event);
});

$("#start-date").on("keydown", function (e) {
  if (e.keyCode === 8 || e.keyCode === 46) {
    e.preventDefault();
  }
});

$("#end-date").on("keydown", function (e) {
  if (e.keyCode === 8 || e.keyCode === 46) {
    e.preventDefault();
  }
});

$('#discountType-select').on('change', function () {
  let selectedValue = $(this).val();

  if (selectedValue === 'B') {
    $('#radio-section-id').show();
    $('#ammount').hide();
    isDiscountTypeByItem = true;
    if (!isDiscountTypeByItem) {
      $("#radio-section-id input[type='radio'][value='optDollar']").prop("checked", true);
    }

    // Update the visibility of the "Amount" column
    if (promotionalItemsDatatable) {
      promotionalItemsDatatable.column(8).visible(isDiscountTypeByItem);
    }
  }
  else {
    $('#radio-section-id').hide();
    $('#ammount').show();

    isDiscountTypeByItem = false;
    // Update the visibility of the "Amount" column
    if (promotionalItemsDatatable) {
      promotionalItemsDatatable.column(8).visible(isDiscountTypeByItem);
    }
  }
});

function makeFormUneditable(formId) {
  const form = document.getElementById(formId);
  const elements = Array.from(form.elements);

  elements.forEach(element => {
    element.disabled = true;
  });
}

function makeFormEditable(formId) {
  const form = document.getElementById(formId);
  const elements = Array.from(form.elements);

  elements.forEach(element => {
    element.disabled = false;
  });
}


async function fillGrid(promoId) {
  const url = `/PromotionalItems/GetPromotionDetailsByPromoId?promoId=${promoId}`;

  const data = await ajaxGetAsync(url);
  $("#items").val(data.length);
  promotionalItemData = data;

  if (promotionalItemsDatatable) {
    promotionalItemsDatatable.state.clear();
    promotionalItemsDatatable.clear();
    promotionalItemsDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = data;
  props.scrollY = '200px';
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.rowCallback = function (row, data, index) {
    $(row).removeClass('selected'); // Remove selected class to remove highlighting
    $(row).off('click'); // Remove click event
  };
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8] }
  ];
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    {
      "data": null,
      "visible": true,
      "render": function (data, type, row, meta) {
        if (type === 'display') {
          return meta.row + 1;
        }
        return null;
      }
    },
    { "data": "link", },
    { "data": "qty", },
    { "data": "stockCode", },
    { "data": "dept", },
    { "data": "subDept", },
    { "data": "subDetail", },
    {
      "data": null,
      "visible": true,
      "render": function (data) {
        if (data.stockCode !== "0" && data.stockCode !== "<NONE>") {
          return data.descript;
        } else if (data.subDetail !== "0" && data.subDetail !== "<NONE>") {
          return data.subDetailName;
        } else if (data.subDept !== "0" && data.subDept !== "<NONE>") {
          return data.subName;
        } else if (data.dept !== "0" && data.dept !== "<NONE>") {
          return data.deptName;
        } else {
          return null;
        }
      }
    },
    {
      targets: 8, // Index of the "Amount" column
      visible: isDiscountTypeByItem, // Set visibility based on the value of isclicked
      data: 'amount', // Set the data property to 'amount' to use the 'amount' property from your data
      render: function (data) {
        return data;
      }
    }
  ];

  promotionalItemsDatatable = createDataTableGeneric('promotionalItem-grid', props);
}

function handleAddButtonClick() {
  isAdd = true;
  hasToAddPromotionalItemData = false;
  makeFormEditable('promotional-item-form');

  const promotionalListDiv = document.getElementById('promotional-list-id');
  promotionalListDiv.classList.add('blur-div');

  const grid = $('#promotionalItem-grid').DataTable();
  grid.clear().draw();
  grid.column(8).visible(false);

  $('#primary-button-section-id').show();
  $('#secondary-button-section-id').hide();
  $('#radio-section-id').hide();
  $("#btn-delete-item").hide();
  $('#ammount').show();

  setFormValues();
  isAddOrEditMode = true;
}

async function handleEditButtonClick() {
  if (selectedPromotionalId) {
    isAdd = false;
    hasToAddPromotionalItemData = true;
    makeFormEditable('promotional-item-form');

    const promotionalListDiv = document.getElementById('promotional-list-id');
    promotionalListDiv.classList.add('blur-div');

    $('#primary-button-section-id').show();
    $('#secondary-button-section-id').hide();
    $('#radio-section-id').hide();
    $('#ammount').show();
    $('#promo-id').prop('readonly', true);

    await fillFormData(selectedPromotionalId);

    if (!isAdd) {
      $('#items').val(promotionalItemData.length);
      convertToRowData(promotionalItemData);
      previousRowIndex = promotionalItemData.length;
      dataTableForAddOrEdit();
    }

    isAddOrEditMode = true;
  }
}

function setFormValues() {
  $('#promo-id').val("");
  $('#description').val("");
  $('#items').val(0);
  $('#discountType-select').val("P");
  $('#price').val("");
  $('#day-select').val(0);

  let today = new Date();
  let currentDate = getFormattedDateyyyyMMdd(today);

  $("#start-date-span").text(currentDate);
  $("#end-date-span").text(currentDate);

  $('#start-date').val(currentDate);

  $('#end-date').val(currentDate);

  isDiscountTypeByItem = false;
}

let cancelButtonClicked = () => {
  const url = `/PromotionalItems?promoId=${selectedPromotionalId}`;
  safelyNavigateTo(url);
}

async function addPromotionalItem(event) {
  event.preventDefault(); // Prevent form submission
  let successMessage = isAdd ? $('#addedSuccessMessage').val() : $('#updatedSuccessMessage').val();
  const formData = serializeFormData();
  if (formData.Price === null) {
    formData.Price = 0;
  }

  if (parseInt(formData.PromoId) < 1 || parseInt(formData.PromoId) > 999) {
    let promoIdError = $('#promotionIdRangeError').val();
    toastr.warning(promoIdError);
    return;
  }

  if (parseInt(formData.PromoDetailsQtySum) > 255) {
    toastr.warning($("#maximumSumOfQuantityError").val());
    return;
  }

  let url = `/PromotionalItems/IsPromoIdExist?promoId=${formData.PromoId}`;
  let isPromoIdExist = await ajaxGetAsync(url);

  if ((isAdd && isPromoIdExist) || (isPromoIdExist && !isAdd && selectedPromotionalId !== formData.PromoId)) {
    let title = $("#promotion-id-exist-title").val();
    let message = $("#promotion-id-exist-message").val();
    await showConfirmModal(title, message).then(async (result) => {
      if (result) {
        isValidate = true;
        isValidate = await validateConfirmFormData(formData);
        if (!isValidate) {
          return;
        }
        else {
          if (isValidate && isDataTableValidated) {
            const url = `PromotionalItems/SavePromotionalItems`;
            await ajaxPostAsync(url, formData)
              .then(() => {
                localStorage.setItem('successMessage', successMessage);
                const url = `/PromotionalItems?promoId=${formData.PromoId}`;
                safelyNavigateTo(url);
              })
              .catch(toastrError);
          }
          else {
            return;
          }
        }
      }
      else {
        isValidate = false;
        const url = `/PromotionalItems?promoId=${formData.PromoId}`;
        safelyNavigateTo(url);
      }
    });
  }
  else {
    isValidate = await validateConfirmFormData(formData);
    if (isValidate && isDataTableValidated) {
      const url = `PromotionalItems/SavePromotionalItems`;
      await ajaxPostAsync(url, formData)
        .then(() => {
          localStorage.setItem('successMessage', successMessage);
          const url = `/PromotionalItems?promoId=${formData.PromoId}`;
          safelyNavigateTo(url);
        })
        .catch(toastrError);
    }
  }
}

let getStockItemByStockCode = async (stockCode) => {
  let url = `StockItems/GetStockItemByCode?stockCode=${stockCode}`;

  return ajaxGetAsync(url)
    .catch(toastrError);
}

let showWarningForDuplicateDeptSubDeptAndDetails = (id, deptSubDeptAndDetails, promoDetailsData, waringMessage) => {
  let matchingItems = promoDetailsData.filter(function (item) {
    return item.Dept === deptSubDeptAndDetails.Dept &&
      item.SubDept === deptSubDeptAndDetails.SubDept &&
      item.SubDetail === deptSubDeptAndDetails.SubDetail;
  });

  let count = matchingItems.length;
  if (count >= 1 && matchingItems[0].id !== id) {
    if (deptSubDeptAndDetails.Dept) {
      waringMessage += `${$("#dept").val()}:${deptSubDeptAndDetails.Dept}`;
    }
    if (deptSubDeptAndDetails.SubDept) {
      waringMessage += ` /${$("#subDept").val()}:${deptSubDeptAndDetails.SubDept}`;
    }
    if (deptSubDeptAndDetails.SubDetail) {
      waringMessage += ` /${$("#subDetail").val()}:${deptSubDeptAndDetails.SubDetail}`;
    }

    waringMessage += `)`;
    toastr.warning($("#thisItemIsAlreadyInPromo").val().replace("{0}", waringMessage));
    return true;
  }

  return false;
}

let validateDuplicateDeptSubDeptOrDetail = (id, deptSubDeptAndDetails, promoDetailsData, waringMessage) => {
  let isDuplicateDeptSubDeptOrDetail = showWarningForDuplicateDeptSubDeptAndDetails(id, deptSubDeptAndDetails, promoDetailsData, waringMessage);

  if (!isDuplicateDeptSubDeptOrDetail) {
    deptSubDeptAndDetails.SubDetail = null;
    isDuplicateDeptSubDeptOrDetail = showWarningForDuplicateDeptSubDeptAndDetails(id, deptSubDeptAndDetails, promoDetailsData, waringMessage);
  }

  if (!isDuplicateDeptSubDeptOrDetail) {
    deptSubDeptAndDetails.SubDept = null;
    isDuplicateDeptSubDeptOrDetail = showWarningForDuplicateDeptSubDeptAndDetails(id, deptSubDeptAndDetails, promoDetailsData, waringMessage);
  }

  return isDuplicateDeptSubDeptOrDetail;
}

async function validateConfirmFormData(formData) {
  let isValidated = true;
  let isDateValid = checkDateProperties(formData);
  if (!isDateValid) {
    return false;
  }

  if (!isDataTableValidated) {
    let datatableError = $(`#datatable-product-selection-error`).val();
    toastr.warning(datatableError);
    return false;
  }

  const promoDetailsData = JSON.parse(formData.PromoDetails);
  promoDetailsData.forEach((promoDetails, index) => {
    promoDetails.id = index + 1;
  });

  let isDuplicateDeptSubDeptOrDetail = false;

  for (let promoDetails of promoDetailsData) {
    let deptSubDeptAndDetails;
    let waringMessage;

    if (promoDetails.StockCode) {
      waringMessage = `${promoDetails.StockCode}(`;
      let stockItem = await getStockItemByStockCode(promoDetails.StockCode);
      deptSubDeptAndDetails = {
        Dept: (stockItem.department) ? stockItem.department : null,
        SubDept: (stockItem.subDepartment) ? stockItem.subDepartment : null,
        SubDetail: (stockItem.subDetail) ? stockItem.subDetail : null
      }
    }
    else {
      waringMessage = `(`;
      deptSubDeptAndDetails = {
        Dept: promoDetails.Dept,
        SubDept: promoDetails.SubDept,
        SubDetail: promoDetails.SubDetail
      }
    }

    isDuplicateDeptSubDeptOrDetail = validateDuplicateDeptSubDeptOrDetail(promoDetails.id, deptSubDeptAndDetails, promoDetailsData, waringMessage);

    if (isDuplicateDeptSubDeptOrDetail) {
      isValidated = false;
      break;
    }
  }

  if (isValidated === false) {
    return false;
  }

  if (formData.Description === null || formData.Description.length > 30) {
    return false;
  }

  if (Math.round(formData.Items) === 0 || Math.round(formData.Items) > 99 || formData.Items === null) {
    return false;
  }
  else {
    if (!isInteger(formData.Items)) {
      let inputValue = Math.round(formData.Items);
      $('#items').val(inputValue);
      inputValue = inputValue.toString();

      let message = $("#floatItemError").val();
      message = message.replace("_numOf_", inputValue);
      toastr.warning(message);

      redirectToField("items");

      return false;
    }
  }

  if ((formData.DiscType === "P" || formData.DiscType === "$")) {
    isValidated = (parseFloat(formData.Price) >= 0 && parseFloat(formData.Price) <= 9999.99 && formData.Price !== null) ? true : false;

    if (!isValidated) {
      return false;
    }
  }
  else if (formData.DiscType === "%") {
    isValidated = (parseFloat(formData.Price) >= 0 && parseFloat(formData.Price) <= 100) ? true : false;

    if (!isValidated) {
      let invalidPriceError = $('#discount-percentage-error').val();
      toastr.warning(invalidPriceError);
      redirectToField("price");
      return false;
    }
  }
  else if (formData.DiscType === "B") {
    formData.Price = 0;
  }

  return isValidated;
}

function checkDateProperties(formData) {
  if (formData.StartDate < '1900-01-01') {
    formData.StartDate = '1900-01-01';
    $('#start-date').val(formData.StartDate);
  }

  if (formData.EndDate < '1900-01-01') {
    formData.EndDate = '1900-01-01';
    $('#end-date').val(formData.EndDate);
  }

  let startDate = new Date(formData.StartDate);
  let isStartDateInvalid = isNaN(startDate.getTime());
  let endDate = new Date(formData.EndDate);
  let isEndDateInvalid = isNaN(endDate.getTime());

  if (isStartDateInvalid && !isEndDateInvalid) {
    formData.StartDate = formData.EndDate
  }
  else if (!isStartDateInvalid && isEndDateInvalid) {
    formData.EndDate = formData.StartDate
  }
  else if (isStartDateInvalid && isEndDateInvalid) {
    formData.StartDate = new Date().toISOString().slice(0, 10);
    formData.EndDate = new Date().toISOString().slice(0, 10);
  }
  else {
    if (formData.StartDate > formData.EndDate) {
      let invalidDaterangeErrorMessage = $('#invalid-date-range').val();
      toastr.warning(invalidDaterangeErrorMessage);
      return false;
    }
  }

  return true;
}

function serializeFormData() {
  isDataTableValidated = true;
  const form = document.getElementById('promotional-item-form');
  const formData = new FormData(form);

  for (let [key, value] of formData.entries()) {
    if (value === "") {
      formData.set(key, null);
    }
  }

  const table = $('#promotionalItem-grid').DataTable();
  const dataTableData = table.rows().data().toArray();

  serializedDataTableData = dataTableData.map(row => {
    return row.map(cell => cell === "" ? null : cell);
  });

  const promoDetails = serializedDataTableData.map(row => mapToPromoDetailViewModel(row));
  let promoDetailsQtySum = 0;

  promoDetails.forEach(function (promoDetail) {
    promoDetailsQtySum += promoDetail.Qty;
  });

  formData.append('PromoDetails', JSON.stringify(promoDetails));
  formData.append('PromoDetailsQtySum', promoDetailsQtySum);

  const hasEmptyDescription = serializedDataTableData.some(row => row[7] === null || row[7] === "");

  if (hasEmptyDescription) {
    isDataTableValidated = false;
  }

  const serializedData = Object.fromEntries(formData);

  return serializedData;
}

function mapToPromoDetailViewModel(row) {
  return {
    Link: row[1],
    Qty: row[2],
    StockCode: row[3],
    Dept: row[4],
    SubDept: row[5],
    SubDetail: row[6],
    SubDetailName: row[7],
    Amount: row[8]
  };
}

function convertToRowData(data) {
  for (let i = 0; i < data.length; i++) {
    const item = promotionalItemData[i]; // Arrays are zero-indexed
    let description = null;

    if (item.stockCode !== "0" && item.stockCode !== "<NONE>") {
      description = item.descript;
      stockCodeArray.push(item.stockCode);
    } else if (item.subDetail !== "0" && item.subDetail !== "<NONE>") {
      description = item.subDetailName;
    } else if (item.subDept !== "0" && item.subDept !== "<NONE>") {
      description = item.subName;
    } else if (item.dept !== "0" && item.dept !== "<NONE>") {
      description = item.deptName;
    }

    const prop1Value = item.link === null || item.link === "<NONE>" ? '' : item.link;
    const prop2Value = item.qty === null || item.qty === "<NONE>" ? '' : item.qty;
    const prop3Value = item.stockCode === null || item.stockCode === "<NONE>" ? '' : item.stockCode;
    const prop4Value = item.dept === null || item.dept === "<NONE>" ? '' : item.dept;

    const prop5Value = item.subDept === null || item.subDept === "<NONE>" ? '' : item.subDept;
    const prop6Value = item.subDetail === null || item.subDetail === "<NONE>" ? '' : item.subDetail;
    const prop7Value = description === null || description === "<NONE>" ? '' : description;
    const prop8Value = item.amount === null || item.amount === "<NONE>" ? '' : item.amount;

    const rowData = [i + 1, prop1Value, prop2Value, prop3Value, prop4Value, prop5Value, prop6Value, prop7Value, prop8Value];

    rowDataArray.push(rowData);
  }
}

function handleOnFocusOutOfItems(items) {
  let needToChangeRows = 0;

  if (!hasToAddPromotionalItemData) {
    promotionalItemData = [];
  }

  if (!itemDeleted && !addForFirstTime) {
    previousRowIndex = rowDataArray.length;
  }
  else {
    itemDeleted = false;
    addForFirstTime = false;
  }

  nextRowIndex = Math.round(items);

  if (nextRowIndex > previousRowIndex) {
    needToChangeRows = nextRowIndex - previousRowIndex;
    hasToAdd = true;
  }
  else {
    needToChangeRows = previousRowIndex - nextRowIndex;
    hasToAdd = false;
  }

  if (needToChangeRows !== 0) {
    if (hasToAdd) {
      for (let i = previousRowIndex; i < Math.round(items); i++) {
        const rowData = [i + 1, i + 1, 1, '', '', '', '', '', 0];
        rowDataArray.push(rowData);
      }
    }
    else {
      for (let i = previousRowIndex; i > items; i--) {
        rowDataArray.pop();
      }
    }

    dataTableForAddOrEdit();
  }
}

const handleEnterKeyPress = async function (event) {
  checkDescriptionIsEmpty();
  const table = $('#promotionalItem-grid').DataTable();
  let title = $("#item-exist").val();
  let message = $("#item-exist-message").val();
  let enterValueOfStockCode = $("#enterValueOfStockCode").val();
  let stockCodeExist = $("#stockCodeExists").val();

  if (event.key === "Enter") {
    isEnterKeyPressed = true;
    event.preventDefault();
    let inputValue = $inputField.val().trim();
    if (inputValue === "") {
      toastr.warning(enterValueOfStockCode);
      $inputField.focus(); // Set focus back to the input field
    }
    else {
      const url = `/PromotionalItems/IsStockCodeValid?stockCode=${inputValue}`;
      let stockCode = await ajaxGetAsync(url);

      if (stockCode.stockCode !== null && stockCode.description !== null) {
        let formData = serializeFormData();
        let promoDetailsData = JSON.parse(formData.PromoDetails);

        promoDetailsData.forEach((promoDetails, index) => {
          promoDetails.id = index + 1;
        });

        let matchingItems = promoDetailsData.filter(function (item) {
          return item.StockCode === stockCode.stockCode
        });

        let count = matchingItems.length;
        const stockCodeFound = (count > 0);

        let waringMessage = `${stockCode.stockCode}(`;
        let stockItem = await getStockItemByStockCode(stockCode.stockCode);
        let deptSubDeptAndDetails = {
          Dept: (stockItem.department) ? stockItem.department : null,
          SubDept: (stockItem.subDepartment) ? stockItem.subDepartment : null,
          SubDetail: (stockItem.subDetail) ? stockItem.subDetail : null
        }

        let isDuplicateDeptSubDeptOrDetail = validateDuplicateDeptSubDeptOrDetail(rowIndex + 1, deptSubDeptAndDetails, promoDetailsData, waringMessage);
        if (isDuplicateDeptSubDeptOrDetail) {
          isEnterKeyPressed = false;
          $inputField.blur();
          return;
        }

        if (!stockCodeFound) {

          if (stockCodeOldValue !== "" || rowData[descriptionCellIndex] !== "") {
            showConfirmModal(title, message).then((result) => {
              if (result) {
                for (let col = startIndex; col < descriptionCellIndex + 1; col++) {
                  table.cell({ row: rowIndex, column: col }).data("").invalidate();
                }

                table.cell({ row: rowIndex, column: cellIndex }).data(stockCode.stockCode).draw();
                table.cell({ row: rowIndex, column: descriptionCellIndex }).data(stockCode.description).draw();
                stockCodeArray.push(stockCode.stockCode);
              }
              else {
                table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeOldValue).draw();
                return;
              }
            });
          }
          else {
            table.cell({ row: rowIndex, column: cellIndex }).data(stockCode.stockCode).draw();
            table.cell({ row: rowIndex, column: descriptionCellIndex }).data(stockCode.description).draw();
            stockCodeArray.push(stockCode.stockCode);
          }
        }
        else {
          stockCodeExist = stockCodeExist.replace("_code_", inputValue);
          toastr.warning(stockCodeExist);
          isEnterKeyPressed = false;
          $inputField.blur();
        }
      }
      else {
        let message = $("#invalidProductCode").val();
        message = message.replace("_code_", inputValue);
        toastr.warning(message);
        $inputField.focus();
      }
    }
  }
  isEnterKeyPressed = false;
};

//Function to handle the input field blur event
const handleInputFieldBlur = function () {
  if (!isEnterKeyPressed) {
    checkDescriptionIsEmpty();
    const table = $('#promotionalItem-grid').DataTable();
    $inputField.remove();
    table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeOldValue).draw();
    $("#OnSinleClickMessageDiv").text("");
  }
  isEnterKeyPressed = false; // Reset the flag after handling blur event
};

const handleInputFieldMouseDown = function () {
  clickCount++;
  if (clickCount === 1) {
    doubleClickTimeout = setTimeout(() => {
      clickCount = 0;
    }, 300); // Adjust the time (in milliseconds) to determine the double-click threshold
  } else if (clickCount === 2) {
    clearTimeout(doubleClickTimeout);
    clickCount = 0;
    handleDoubleClick();
  }
};

const handleDoubleClick = async function () {
  checkDescriptionIsEmpty();
  renderStockCodeDataTable(onStockCodeRowClick);
  $("#stockCodeDescriptionDataTableModal").modal('show');
  $("#OnSinleClickMessageDiv").text("");
};

let onStockCodeRowClick = (rowData) => {
  stockCodeRowData = rowData;
  renderStockCodeData();
  $("#stockCodeDescriptionDataTableModal").modal('hide');
}

function checkDescriptionIsEmpty() {
  const description = rowData[descriptionCellIndex];
  const deptValue = rowData[4];

  if (deptValue !== '' || (description === '' && deptValue === '')) {
    stockCodeOldValue = "";
  }
}

$('#promotionalItem-grid')
  .off()
  .on('click', 'td', async function () {
    if (!isAddOrEditMode) {
      return;
    }
    cellIndex = $(this).index();

    if (cellIndex !== 0) {
      const isClickedRowSelected = $(this).closest('tr').hasClass('selected-grid-row');

      if (isClickedRowSelected) {
        $(this).closest('tr').removeClass('selected-grid-row');
        lastSelectedRowForDelete = null;
      }
      else {
        const anyRowHasClass = $('#promotionalItem-grid tbody tr.selected-grid-row').length > 0;

        if (anyRowHasClass) {
          $('#promotionalItem-grid tbody tr.selected-grid-row').removeClass('selected-grid-row');
        }

        lastSelectedRowForDelete = null;
      }
    }

    cellData = $(this).text();
    numberOfRowsAtPresent = promotionalItemsDatatable.rows().count();
    rowData = promotionalItemsDatatable.row($(this).closest('tr')).data();
    rowIndex = promotionalItemsDatatable.row($(this).closest('tr')).index();
    let msg = '';

    switch (cellIndex) {
      case 0:
        $("#OnSinleClickMessageDiv").text("");
        const isClickedRowSelected = $(this).closest('tr').hasClass('selected-grid-row');

        if (isClickedRowSelected) {
          $(this).closest('tr').removeClass('selected-grid-row');
          lastSelectedRowForDelete = null;
        }
        else {
          const anyRowHasClass = $('#promotionalItem-grid tbody tr.selected-grid-row').length > 0;

          if (anyRowHasClass) {
            $('#promotionalItem-grid tbody tr.selected-grid-row').removeClass('selected-grid-row');
          }

          $(this).closest('tr').addClass('selected-grid-row');
          lastSelectedRowForDelete = rowIndex;
        }
        break;

      case 1:
        $("#OnSinleClickMessageDiv").text("");
        loadLinkSelect();
        break;

      case 2:
        $("#OnSinleClickMessageDiv").text("");
        handleTableCell("qty-field");
        break;

      case 3:
        msg = $("#stockCodeMsg").val();
        $("#OnSinleClickMessageDiv").text(msg);
        stockCodeOldValue = $(this).text() ? $(this).text() : stockCodeOldValue;

        $(this).empty();
        // Create an input field dynamically
        $inputField = $('<input type="text">');
        const columnWidth = $(this).width();
        $inputField.css('width', columnWidth + 'px');
        $(this).append($inputField);
        $inputField.focus();

        $inputField.on("keydown", handleEnterKeyPress);
        $inputField.on("blur", handleInputFieldBlur);
        $inputField.on("mousedown", handleInputFieldMouseDown);

        break;

      case 4:
        msg = $("#departmentMsg").val();
        $("#OnSinleClickMessageDiv").text(msg);
        await loadDepartmentSelect();
        break;

      case 5:
        msg = $("#subdepartmentMSg").val();
        $("#OnSinleClickMessageDiv").text(msg);
        deptName = rowData[cellIndex - 1];
        await loadSubDepartmentSelect(deptName);
        break;

      case 6:
        msg = $("#detailMsg").val();
        $("#OnSinleClickMessageDiv").text(msg);
        deptName = rowData[cellIndex - 2];
        subDeptName = rowData[cellIndex - 1];
        await loadSubDetailtSelect(deptName, subDeptName);
        break;

      case 8:
        $("#OnSinleClickMessageDiv").text("");
        handleTableCell("amount-field");
        break;
    }
  });

function handleTableCell(columnName) {
  let localizedColumnName = $(`#${columnName}`).val();
  let title = `${$("#edit").val()} ${localizedColumnName}`;
  let message = localizedColumnName;

  modalSource = columnName;

  let inputElement = $('#inputModalValueId');

  if (modalSource === "qty-field") {
    inputElement.addClass('only-digit');
    inputElement.removeClass('only-digit-decimal');
    inputElement.attr('max', '254');
    inputElement.attr('min', '0');
  }
  else {
    inputElement.addClass('only-digit-decimal');
    inputElement.removeClass('only-digit');
    inputElement.attr('max', '214748.36');
    inputElement.attr('min', '0');
  }

  $("#inputModalTitleId").text(title);
  $("#inputModalMessageId").text(message);
  $("#inputModalValueId").val(cellData);
  $("#inputModalId").modal("show");
}

function handleInputModalApplyButtonClick() {
  const table = $('#promotionalItem-grid').DataTable();
  let input = $('#inputModalValueId').val();
  let inputValue = modalSource === "qty-field" ? parseInt(input) : parseFloat(input);

  if (modalSource === "qty-field") {
    if ((isNaN(inputValue) || inputValue > 254 || inputValue < 0)) {
      $("#inputModalValueId").focus();
      toastr.warning($("#maximumValueForQuantityError").val());
      return;
    }
  }
  else {
    if ((isNaN(inputValue) || inputValue > 214748.37 || inputValue < 0)) {
      $("#inputModalValueId").focus();
      toastr.warning($("#maximumValueForAmountError").val());
      return;
    }
  }

  table.cell({ row: rowIndex, column: cellIndex }).data(inputValue).draw();
  $("#inputModalId").modal("hide");
}

function deleteItem() {
  if (lastSelectedRowForDelete === null) {
    let itemClickError = $("#clickOnAItem").val();
    toastr.warning(itemClickError);
    return;
  }

  let totalRows = promotionalItemsDatatable.rows().count();
  let title = $("#deleteItem").val();
  let message = $("#deleteMessageOfItem").val();
  let item = (lastSelectedRowForDelete + 1).toString();
  message = message.replace("_Item_", item);
  let itemDeleteError = $("#promoItemDeleteErrorMessage").val();

  showConfirmModal(title, message).then((result) => {
    if (result) {
      if (totalRows === 1) {
        toastr.warning(itemDeleteError);
        return;
      }
      else {
        promotionalItemsDatatable.row(lastSelectedRowForDelete).remove().draw();
        let rows = promotionalItemsDatatable.rows().nodes();
        $(rows).each(function (index, row) {
          let newRowNumber = index + 1;
          $(row).find('td:first-child').text(newRowNumber);
        });
        totalRows = promotionalItemsDatatable.rows().count();
        rowDataArray = promotionalItemsDatatable.rows().data().toArray();
        itemDeleted = true;
        previousRowIndex = totalRows;
        $('#items').val(totalRows);
        lastSelectedRowForDelete = null;
      }
    }
    else {
      return;
    }
  });
}

function dataTableForAddOrEdit() {
  if (promotionalItemsDatatable) {
    promotionalItemsDatatable.state.clear();
    promotionalItemsDatatable.clear();
    promotionalItemsDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = rowDataArray;
  props.scrollY = '200px';
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8] },
    {
      targets: 8, // Index of the "Amount" column
      visible: isDiscountTypeByItem, // Set visibility based on the value of isclicked
      data: 8, // Set the data property to 'amount' to use the 'amount' property from your data
      render: function (data) {
        return data;
      }
    }
  ];

  promotionalItemsDatatable = createDataTableGeneric('promotionalItem-grid', props);
}

function loadLinkSelect() {
  $("#dropdownSelectModal").modal('show');
  $("#dropdownSelectId").show();
  $("#dropdwonSelectApplyButtonId").prop("disabled", true);
  loadLink();
  renderLinkSelect();
}

async function loadDepartmentSelect(dept) {
  $("#dropdownSelectModal").modal('show');
  $("#dropdownSelectId").show();
  $("#dropdwonSelectApplyButtonId").prop("disabled", true);
  await loadDepartment();
  renderDepartmentSelect()
}

async function loadSubDepartmentSelect(dept) {
  $("#dropdownSelectModal").modal('show');
  $("#dropdownSelectId").show();
  $("#dropdwonSelectApplyButtonId").prop("disabled", true);
  await loadSubDepartment(dept);
  renderSubDepartmentSelect()
}

async function loadSubDetailtSelect(dept, subDept) {
  $("#dropdownSelectModal").modal('show');
  $("#dropdownSelectId").show();
  $("#dropdwonSelectApplyButtonId").prop("disabled", true);
  await loadSubDetail(dept, subDept);
  renderSubDetailSelect()
}

async function loadStockCode() {
  let url = "/PromotionalItems/GetStockCodeMstForPromotion";
  await ajaxGetAsync(url)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);
}

function loadLink() {
  linkData = [];
  let len = rowDataArray.length;
  for (let i = 1; i <= len; i++) {
    linkData.push(i);
  }
}

async function loadDepartment() {
  let url = "/StockItems/GetDepartments";
  await ajaxGetAsync(url)
    .then(_departmentData => departmentData = _departmentData)
    .catch(toastrError);
}

async function loadSubDepartment(dept) {
  if (dept === "") {
    subDepartmentData = null;
    return;
  }

  let url = `/StockItems/GetSubDepartments?department=${dept}`;
  await ajaxGetAsync(url)
    .then(_subDepartmentData => subDepartmentData = _subDepartmentData)
    .catch(toastrError);
}

async function loadSubDetail(dept, subDept) {
  if (dept === "" || subDept === "") {
    subDetailData = null;
    return;
  }

  let url = `/StockItems/GetSubDetails?department=${dept}&subdepartment=${subDept}`;
  await ajaxGetAsync(url)
    .then(_subDetailData => subDetailData = _subDetailData)
    .catch(toastrError);
}


function renderStockCodeData() {
  const table = $('#promotionalItem-grid').DataTable();
  let title = $("#item-exist").val();
  let message = $("#item-exist-message").val();

  subDepartmentData = null;
  subDetailData = null;

  const stockCodeFound = stockCodeArray.some((stockCode) =>
    caseInsensitiveComparison(stockCode, stockCodeRowData.stockCode)
  );

  if (!stockCodeFound) {
    if (rowData[cellIndex] !== '' || rowData[descriptionCellIndex] !== '') {
      showConfirmModal(title, message).then((result) => {
        if (result) {
          for (let col = startIndex; col < descriptionCellIndex + 1; col++) {
            table.cell({ row: rowIndex, column: col }).data("").invalidate();
          }

          // Update the DataTable cell value with the selected selectInputValue
          table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeRowData.stockCode).draw();
          table.cell({ row: rowIndex, column: descriptionCellIndex }).data(stockCodeRowData.description).draw();
          stockCodeArray.push(stockCodeRowData.stockCode);
        }
        else {
          $("#stockCodeDescriptionDataTableModal").modal('hide');
          table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeOldValue).draw();
          return;
        }
      });
    }
    else {
      // Update the DataTable cell value with the selected selectInputValue
      table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeRowData.stockCode).draw();
      table.cell({ row: rowIndex, column: descriptionCellIndex }).data(stockCodeRowData.description).draw();
      stockCodeArray.push(stockCodeRowData.stockCode);
    }
  }
  else {
    let stockCodeExist = $("#stockCodeExists").val();
    stockCodeExist = stockCodeExist.replace("_code_", stockCodeRowData.stockCode);
    toastr.warning(stockCodeExist);
    table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeOldValue).draw();
  }
}

let renderLinkSelect = () => {
  let linkTitle = $("#link").val();
  $("#dropdownSelectInputId").html(optionGeneratorDefault(linkTitle));
  linkData.forEach(link => {
    $("#dropdownSelectInputId").append(optionGeneratorForSingleElement(link));
  });
}

let renderDepartmentSelect = () => {
  let department = $("#department").val();
  $("#dropdownSelectInputId").html(optionGeneratorDefault(department));
  departmentData.map(dept => {
    $("#dropdownSelectInputId").append(optionGenerator(dept.deptId, dept.deptName));
  });
}

let renderSubDepartmentSelect = () => {
  let subDepartment = $("#subDepartment").val();
  $("#dropdownSelectInputId").html(optionGeneratorDefault(subDepartment));

  if (subDepartmentData !== null) {
    subDepartmentData.map(subDept => {
      $("#dropdownSelectInputId").append(optionGenerator(subDept.subDeptId, subDept.subDeptName));
    });
  }
}

let renderSubDetailSelect = () => {
  let subDetailTitle = $("#subDetail").val();
  $("#dropdownSelectInputId").html(optionGeneratorDefault(subDetailTitle));

  if (subDetailData !== null) {
    subDetailData.map(subDetail => {
      $("#dropdownSelectInputId").append(optionGenerator(subDetail.subDetailId, subDetail.subDetailName));
    });
  }
}

function promotionalItemSelect(id) {
  if (id) {
    $("#dropdwonSelectApplyButtonId").prop("disabled", false);
  }
  else {
    $("#dropdwonSelectApplyButtonId").prop("disabled", true);
  }
}

function promotionalItemFormApplyButtonClicked() {
  $("#OnSinleClickMessageDiv").text("");
  let selectInputValue = $("#dropdownSelectInputId").val();

  switch (cellIndex) {
    case 1:
      const table = $('#promotionalItem-grid').DataTable();
      table.cell({ row: rowIndex, column: cellIndex }).data(parseInt(selectInputValue)).draw();
      break;

    case 4:
      descriptionCell = departmentData.find((dept) => dept.deptId === selectInputValue).deptName;
      checkTheRemainingCols(cellIndex, descriptionCell, selectInputValue);
      break;

    case 5:
      descriptionCell = subDepartmentData.find((subDept) => subDept.subDeptId === selectInputValue).subDeptName;
      checkTheRemainingCols(cellIndex, descriptionCell, selectInputValue);
      break;

    case 6:
      descriptionCell = subDetailData.find((subDetail) => subDetail.subDetailId === selectInputValue).subDetailName;
      checkTheRemainingCols(cellIndex, descriptionCell, selectInputValue);
      break;
  }

  $("#dropdownSelectModal").modal('hide');
}


function promotionalItemFormCancelButtonClicked() {
  $("#OnSinleClickMessageDiv").text("");
  $("#dropdownSelectModal").modal('hide');
}

function promotionalItemTableCancelButtonClicked() {
  const table = $('#promotionalItem-grid').DataTable();
  table.cell({ row: rowIndex, column: cellIndex }).data(stockCodeOldValue).draw();
  $("#stockCodeDescriptionDataTableModal").modal('hide');

}

function checkTheRemainingCols(cellIndex, descriptionCell, selectInputValue) {
  const table = $('#promotionalItem-grid').DataTable();
  let title = $("#item-exist").val();
  let message = $("#item-exist-message").val();

  const remainingCols = rowData.slice(cellIndex + 1);
  const nonEmptyCols = remainingCols.filter(cellValue => cellValue !== '');

  if ((rowData[cellIndex] !== '' || rowData[descriptionCellIndex] !== '') && !rowArray.includes(cellIndex)) {
    showConfirmModal(title, message).then((result) => {
      if (result) {
        for (let col = startIndex; col < descriptionCellIndex + 1; col++) {
          table.cell({ row: rowIndex, column: col }).data("").invalidate();
        }

        // Update the DataTable cell value with the selected selectInputValue
        table.cell({ row: rowIndex, column: cellIndex }).data(selectInputValue).draw();
        table.cell({ row: rowIndex, column: descriptionCellIndex }).data(descriptionCell).draw();
      }
      else {
        $("#dropdownSelectModal").modal('hide');
        return;
      }
    });
  }
  else if (rowData[cellIndex] !== '' && rowArray.includes(cellIndex)) {
    showConfirmModal(title, message).then((result) => {
      if (result) {
        for (let col = cellIndex + 1; col < cellIndex + 1 + nonEmptyCols.length; col++) {
          table.cell({ row: rowIndex, column: col }).data("").invalidate();
        }

        // Update the DataTable cell value with the selected selectInputValue
        table.cell({ row: rowIndex, column: cellIndex }).data(selectInputValue).draw();
        table.cell({ row: rowIndex, column: descriptionCellIndex }).data(descriptionCell).draw();
      }
      else {
        $("#dropdownSelectModal").modal('hide');
        return;
      }
    });
  }
  else {
    // Update the DataTable cell value with the selected selectInputValue
    table.cell({ row: rowIndex, column: cellIndex }).data(selectInputValue).draw();
    table.cell({ row: rowIndex, column: descriptionCellIndex }).data(descriptionCell).draw();
  }
}

async function handleDeleteButtonClick() {
  let title = $("#delete").val();
  let message = $("#deleteMessage").val();
  let successMessage = $("#deleteSuccessMessage").val();

  if (selectedPromotionalId) {
    showConfirmModal(title, message).then(async result => {
      if (result) {
        let url = `/PromotionalItems/DeletePromo`;
        await ajaxPostAsync(url, selectedPromotionalId)
          .then(() => {
            localStorage.setItem('successMessage', successMessage);
            url = "/PromotionalItems";
            safelyNavigateTo(url);
          });
      }
      else {
        $("#dropdownSelectModal").modal('hide');
        return;
      }
    });
  }
}

$('#start-date').on('change', function () {
  const selectedDate = $(this).val();
  if (!selectedDate) {
    $("#start-date").val($("#startDateTmpData").val());
  }

  $("#startDateTmpData").val($("#start-date").val());
});

$('#end-date').on('change', function () {
  const selectedDate = $(this).val();
  if (!selectedDate) {
    $("#end-date").val($("#endDateTmpData").val());
  }

  $("#endDateTmpData").val($("#end-date").val());
});
