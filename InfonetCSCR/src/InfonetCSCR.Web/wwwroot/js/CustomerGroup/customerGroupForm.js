let cashBonusData;
let discountChartData;
let cashBonusDataTable;
let groupID;
let colIndex;
let initialDiscountType;
let discountChartGradeArray = [];
let isCopyCustomerGroup;
const cashBonusQuantityUpperLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_INTEGER_LIMIT;
const cashBonusPriceUpperLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LIMIT;
const cashBonusQuantityLowerLimit = 1;
const cashBonusPriceLowerLimit = 0.0001;
const discountChartPriceLowerLimit = 0;
const discountChartPriceUpperLimit = 99999999999999.9062;
const custMem = document.getElementById("CUST_MEM").value !== "Customer";
const percentDiscount = DISCOUNT_TYPE.PERCENT_DISCOUNT;
const dollarDiscount = DISCOUNT_TYPE.DOLLAR_DISCOUNT;
const couponDiscount = DISCOUNT_TYPE.COUPON;
const cashBonusDiscount = DISCOUNT_TYPE.CASH_BONUS;
const discountChartDiscount = DISCOUNT_TYPE.DISCOUNT_CHART;
const view = CONSTANT_TEXTS.VIEW;
let cashBonusQuantityDataError = false;
let cashBonusRateDataError = false;
let discountChartDataError = false;
const create = CONSTANT_TEXTS.CREATE;
const update = CONSTANT_TEXTS.UPDATE;
const copy = CONSTANT_TEXTS.COPY;

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  groupID = urlParams.get('groupID');
  isCopyCustomerGroup = urlParams.get('isCopyCustomerGroup');
  const action = urlParams.get('actionMode');
  const operationType = urlParams.get('operationType');
  const isShowToaster = urlParams.get('showToaster');
  let isUpdate = false;
  if (groupID !== null && groupID !== "" && isCopyCustomerGroup !== 'true') {
    $('.not-editable').prop('readOnly', true);
    isUpdate = true;
  } else if (isCopyCustomerGroup === 'true') {
    $("#GroupID").val('');
  }

  initialDiscountType = $("#DiscountType option:selected").val();
  if (initialDiscountType === percentDiscount || initialDiscountType === dollarDiscount || initialDiscountType === couponDiscount) {
    $("#discountRate").show();
    $("#discount-name-container").show();
  }
  else {
    if (initialDiscountType === cashBonusDiscount) {
      $("#cash-bonus-grid").show();
      await loadCashBonusData();
    } else if (initialDiscountType === discountChartDiscount) {
      await loadDiscountChartTable();
      $("#discount-chart-table-container").show();
      $("#discount-name-container").show();
    }
    $("#discountRate").hide();
  }

  if ($("#IsCustomerGroupAlreadyExists").val() === "true") {
    let message = $("#group").val();
    message += ' ';
    message += $("#GroupID").val();
    message += ' ';
    message += $('#alreadyExists').val();

    toastr.error(message);
  }

  if (isUpdate) {
    setEditMode((action !== view));
  } else {
    $('#btn-cancel').hide();
    $('#btn-complete').show();
    $('#btn-edit').hide();
    _globalIsEditing = true;
    setGlobalEditingState(_globalIsEditing);
  }

  if (isShowToaster) {
    if (operationType === create) {
      custMem ? toastr.success($("#memberGroupCreatedSuccessfully").val()) : toastr.success($("#customerGroupCreatedSuccessfully").val());
    } else if (operationType === update) {
      custMem ? toastr.success($("#memberGroupUpdatedSuccessfully").val()) : toastr.success($("#customerGroupUpdatedSuccessfully").val());
    }
    else if (operationType === copy) {
      custMem ? toastr.success($("#memberGroupCopiedSuccessfully").val()) : toastr.success($("#customerGroupCopiedSuccessfully").val());
    }

    let newUrl = removeURLParameter(window.location.search, 'ShowToaster');
    window.history.replaceState(null, null, newUrl);
  }
});

$("#btn-back").on('click', async function () {
  const url = "/CustomerGroup";
  await safelyNavigateTo(url);
});

$('#DiscountType').on('change', async function () {
  let changedDiscountValue = $(this).val();
  if (changedDiscountValue === percentDiscount || changedDiscountValue === dollarDiscount || changedDiscountValue === couponDiscount) {
    $("#discountRate").show();
    $("#cash-bonus-grid").hide();
    $("#discount-chart-table-container").hide();
    $("#discount-name-container").show();
  }
  else {
    if (changedDiscountValue === cashBonusDiscount) {
      $("#discount-name-container").hide();
      $("#cash-bonus-grid").show();
      $("#discount-chart-table-container").hide();
      await loadCashBonusData();
    }
    else if (changedDiscountValue === discountChartDiscount) {
      $("#cash-bonus-grid").hide();
      await loadDiscountChartTable();
      $("#discount-chart-table-container").show();
      $("#discount-name-container").show();
    }
    $("#discountRate").hide();
  }
});

let loadCashBonusData = async () => {
  if (groupID !== '') {
    let url = `GetCashBonusData?groupID=${groupID}`;
    let errorMessage = $("#anErrorHasOccurredWhileLoadingCashBonusData").val();

    await ajaxGetAsync(url)
      .then((_cashBonusData) => {
        cashBonusData = _cashBonusData;
      })
      .catch(() => toastr.error(errorMessage));
  }
  createCashBonusDataTable();
}

let createCashBonusDataTable = () => {
  destroyDataTableIfExists(cashBonusDataTable)

  let props = createDataTableProp();
  props.data = cashBonusData;
  props.scrollY = '300px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];
  props.paging = false;
  props.bInfo = false;

  props.columns = getColumnsOfCashBonusDataTable();

  cashBonusDataTable = createDataTableGeneric('cash-bonus-table', props);
}

$('#btn-complete').on('click', async function (event) {
  if (groupID === '' || isCopyCustomerGroup !== null && isCopyCustomerGroup === 'true') {
    groupID = $("#GroupID").val();
  }
  let selectedDiscountType = $("#DiscountType option:selected").val();
  let isGlobalIsEditing = _globalIsEditing;
  if (isGlobalIsEditing) {
    _globalIsEditing = false;
    setGlobalEditingState(_globalIsEditing);
  }

  if (initialDiscountType === cashBonusDiscount && selectedDiscountType !== cashBonusDiscount) {
    const url = `/CustomerGroup/DeleteCashBonusData?groupID=${groupID}`;
    await safelyNavigateTo(url);
  } else if (initialDiscountType === discountChartDiscount && selectedDiscountType !== discountChartDiscount) {
    const url = `/CustomerGroup/DeleteDiscountChartData?groupID=${groupID}`;
    await safelyNavigateTo(url);
  }

  if (selectedDiscountType === cashBonusDiscount) {
    const url = `/CustomerGroup/UpdateCashBonusData`;

    let dataArray = convertBonusTableToArray();
    if (cashBonusRateDataError || cashBonusQuantityDataError) {
      event.preventDefault();
      return;
    }
    await ajaxPostAsync(url, dataArray);
  } else if (selectedDiscountType === discountChartDiscount) {
    let discountChartDataArray = convertDiscountChartToArray();
    if (discountChartDataError) {
      event.preventDefault();
      return;
    }
    const url = `/CustomerGroup/SaveDiscountChartData`;
    await ajaxPostAsync(url, discountChartDataArray);
  }

  if (isGlobalIsEditing) {
    _globalIsEditing = true;
    setGlobalEditingState(_globalIsEditing);
  }
});

function getColumnsOfCashBonusDataTable() {
  let columns = [
    {
      data: "incrementalType",
      render: renderSelectHTML
    },
    {
      data: "incrementalQty",
      render: renderInputFieldForCashBonusQuantity
    },
    {
      data: "bonusRate",
      render: renderInputFieldForCashBonusRate
    },
    {
      data: "",
      render: renderMinusSignForRowDelete
    }
  ];

  return columns;
}

$('#btn-add-new-row').on('click', function (event) {
  event.preventDefault();
  cashBonusDataTable.row.add({
    incrementalType: ">=",
    incrementalQty: '',
    bonusRate: '',
  }).draw();
});

$('#cash-bonus-table').on('click', '.btn-delete-this-row', function () {
  const row = $(this).closest('tr');
  cashBonusDataTable.row(row).remove().draw();
});

const renderSelectHTML = function (data) {
  let selectHTML = `<select class="form-select edit-mode-control">`;

  selectHTML += `<option class="text-center" value=">=" ${data === ">=" ? "selected" : ""}>>=</option>`;
  selectHTML += `<option class="text-center" value="*" ${data === "*" ? "selected" : ""}>*</option>`;
  selectHTML += `</select>`;
  return selectHTML;
}

function checkOnlyDigitKeyPress(event) {
  if (!onlyDigitKeyPress(event)) {
    return false;
  }
}

function checkOnlyDigitAndDecimalKeyPress(event) {
  if (!onlyDigitAndDecimalKeyPress(event)) {
    return false;
  }
}

let renderInputFieldForCashBonusQuantity = function (data) {
  const inputFieldHTML = `<input type="number" class="col infonet-input w-100 cash-bonus-quantity edit-mode-control" value="${data}" 
                  onkeypress="return checkOnlyDigitKeyPress(event)" onBlur="handleBlurEventForInputField(event)">`;
  return inputFieldHTML;
}

let renderInputFieldForCashBonusRate = function (data) {
  const inputFieldHTML = `<input type="number" class="col infonet-input w-100 cash-bonus-rate edit-mode-control" value="${data}"
                  onkeypress="return checkOnlyDigitAndDecimalKeyPress(event)" onBlur="handleBlurEventForInputField(event)">`;
  return inputFieldHTML;
}

let renderMinusSignForRowDelete = function () {
  const inputFieldHTML = `<div class="d-flex justify-content-center">
                            <button class="btn btn-sm btn-outline-danger edit-mode-control btn-delete-this-row"> - </button>
                          </div>`;
  return inputFieldHTML;
}

function handleBlurEventForInputField(event) {
  if (event.target.classList.contains('cash-bonus-quantity')) {
    const inputValue = parseInt(event.target.value);
    checkAndShowWarningIfRequired(inputValue, cashBonusQuantityLowerLimit, cashBonusQuantityUpperLimit,
      $("#cashBonusQuantityLimitExceed").val(), $("#cashBonusQuantityIsRequired").val());
  } else if (event.target.classList.contains('cash-bonus-rate')) {
    const inputValue = parseFloat(event.target.value);
    checkAndShowWarningIfRequired(inputValue, cashBonusPriceLowerLimit, cashBonusPriceUpperLimit,
      $("#cashBonusPriceLimitExceed").val(), $("#cashBonusRateIsRequired").val());
  } else if (event.target.classList.contains('discount-chart')) {
    const inputValue = parseFloat(event.target.value);
    checkAndShowWarningIfRequired(inputValue, discountChartPriceLowerLimit, discountChartPriceUpperLimit,
      $("#discountChartPriceLimitExceed").val());
  }
}

function checkAndShowWarningIfRequired(value, lowerLimit, upperLimit, validationMessage, emptyMessage = null) {
  if (emptyMessage !== null && (isNaN(value) || isNullUndefinedOrWhiteSpace(value))) {
    toastr.warning(emptyMessage);
    return false;
  } else if (value < lowerLimit || value > upperLimit) {
    toastr.warning(validationMessage);
    return false;
  }
  return true;
}

let loadDiscountChartData = async () => {
  let url = `GetDiscountChartData?groupID=${groupID}`;
  let errorMessage = $("#anErrorHasOccurredWhileLoadingDiscountChartData").val();

  await ajaxGetAsync(url)
    .then((_discountChartData) => {
      discountChartData = _discountChartData;
    })
    .catch(() => toastr.error(errorMessage));

  discountChartGradeArray = discountChartData.map(obj => obj.grade);
}

let prepareDiscountChartTable = () => {
  $('#discount-chart-table').find('thead').remove();
  $('#discount-chart-table').find('tbody').remove();

  let thead = $('<thead>');
  let tbody = $('<tbody>');
  let headerRow = '<tr>';
  let dataRow = '<tr class="text-center">';
  discountChartData.forEach((discountData) => {
    headerRow += '<th class="p-2 min-width-270px">' + discountData.fullName + '</th>';
    const value = (discountData.discountRate === '' ? 0 : parseFloat(discountData.discountRate));
    dataRow += '<td class="p-2 min-width-270px">' + renderInputFieldForDiscountChart(value) + '</td>';
  });
  headerRow += '</tr>';
  dataRow += '</tr>';

  thead.append(headerRow);
  tbody.append(dataRow);

  $('#discount-chart-table').append(thead);
  $('#discount-chart-table').append(tbody);
}

let renderInputFieldForDiscountChart = function (data) {
  const inputFieldHTML = `<input type="number" class="col infonet-input w-100 discount-chart edit-mode-control" value="${data}"
                  onkeypress="return checkOnlyDigitAndDecimalKeyPress(event)" onBlur="handleBlurEventForInputField(event)">`;
  return inputFieldHTML;
}

async function loadDiscountChartTable() {
  await loadDiscountChartData();
  prepareDiscountChartTable();
}

function convertDiscountChartToArray() {
  let discountChartArray = [];

  let discountChartHeader = [];
  let discountChartBody = [];
  discountChartDataError = false;

  $('#discount-chart-table thead th').each(function () {
    discountChartHeader.push($(this).text());
  });

  $('#discount-chart-table tbody td').each(function () {
    discountChartBody.push($(this).find('input').val());
  });

  discountChartHeader.forEach((element, index) => {
    if (discountChartDataError) {
      return;
    }

    let discountChartDataTemplate = {
      grade: discountChartGradeArray[index].trim(),
      fullName: element,
      discountRate: discountChartBody[index] === '' ? '0' : discountChartBody[index],
      groupID: groupID
    };

    const inputValue = parseFloat(discountChartDataTemplate.discountRate);
    const success = checkAndShowWarningIfRequired(inputValue, discountChartPriceLowerLimit, discountChartPriceUpperLimit,
      $("#discountChartPriceLimitExceed").val());

    if (!success) {
      discountChartDataError = true;
      return;
    }

    discountChartArray.push(discountChartDataTemplate);
  });

  return discountChartArray;
}

function convertBonusTableToArray() {
  let cashBonusTableArray = [];
  cashBonusQuantityDataError = false;
  cashBonusRateDataError = false;

  $('#cash-bonus-table tbody tr').each(function () {
    let rowData = {};

    if (cashBonusRateDataError || cashBonusQuantityDataError) {
      return;
    }
    $(this).find('td').each(function (index) {

      if (cashBonusRateDataError || cashBonusQuantityDataError) {
        return;
      }
      // index 0 means Type (>= or *)
      if (index === 0) {
        rowData.incrementalType = $(this).find('select').val();
        // index 1 means Liters
      } else if (index === 1) {
        let inputValue = $(this).find('input').val();
        inputValue = parseInt(inputValue);
        const success = checkAndShowWarningIfRequired(inputValue, cashBonusQuantityLowerLimit, cashBonusQuantityUpperLimit,
          $("#cashBonusQuantityLimitExceed").val(), $("#cashBonusQuantityIsRequired").val());
        if (!success) {
          cashBonusQuantityDataError = true;
          return;
        }
        rowData.incrementalQty = Math.round(inputValue);
        // index 2 means Cash Bonus ($)
      } else if (index === 2) {
        let inputValue = $(this).find('input').val();
        inputValue = parseFloat(inputValue);
        const success = checkAndShowWarningIfRequired(inputValue, cashBonusPriceLowerLimit, cashBonusPriceUpperLimit,
          $("#cashBonusPriceLimitExceed").val(), $("#cashBonusRateIsRequired").val());
        if (!success) {
          cashBonusRateDataError = true;
          return;
        }
        rowData.bonusRate = inputValue;
      }
    });

    rowData.groupID = groupID;

    if (cashBonusRateDataError || cashBonusQuantityDataError) {
      return;
    }
    cashBonusTableArray.push(rowData);
  });

  return cashBonusTableArray;
}

let setEditMode = (editMode) => {
  _globalIsEditing = editMode;
  setGlobalEditingState(_globalIsEditing);
  $('.edit-mode-control').prop('disabled', !editMode);

  if (!editMode) {
    $('#btn-cancel').hide();
    $('#btn-complete').hide();
    $('#btn-edit').show();
  }
  else {
    $('#btn-cancel').show();
    $('#btn-complete').show();
    $('#btn-edit').hide();
  }
}

$('.not-editable').on('keydown', function (e) {
    if ($(this).prop('readonly')) {
        e.preventDefault();
    }
});

async function cancelButtonClicked() {
  const customerGroupId = $("#GroupID").val();
  const url = `/CustomerGroup/CustomerGroupForm?groupID=${customerGroupId}&actionMode=view`;
  _globalIsEditing = false;
  setGlobalEditingState(_globalIsEditing);
  await safelyNavigateTo(url);
}
