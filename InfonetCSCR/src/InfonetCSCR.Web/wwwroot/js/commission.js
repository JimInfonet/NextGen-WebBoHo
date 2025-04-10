let commissionsDatatable;
let commissionList;
const baseUrl = "/Commission";
let selectedCommission;
let isEdit;

$(async function () {
  await initCommissionPageAsync();
});

async function initCommissionPageAsync() {
  await loadCommissionListAsync();
  await createCommissionsDatatableAsync();
}

async function createCommissionsDatatableAsync() {
  destroyDataTableIfExists(commissionsDatatable);
  let props = createDataTableProp();
  props.data = commissionList;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = false;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "employeeID", },
    { "data": "employeeName", },
    { "data": "department", },
    { "data": "subDept", },
    { "data": "subDetail", },
    { "data": "stockCode", },
    { "data": "groupName", },
    {
      "data": "commissionPercentage",
      "render": function (data) {
        return data.toFixed(2);
      }
    },
    {
      "data": "commissionAmount",
      "render": function (data) {
        return data.toFixed(2);
      }
    },
    { "data": "formattedApply", }
  ];

  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9] }
  ];

  commissionsDatatable = createDataTableGeneric('commissions-grid', props);

  $('#commissions-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        selectedCommission = null;
      } else {
        commissionsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedCommission = commissionsDatatable.row(this).data();
      }
    })

  $('#btnAddCommission').off().on('click', await openAddCommissionModalAsync);
  $('#btnEditCommission').off().on('click', await openEditCommissionModalAsync);
  $('#btnDeleteCommission').off().on('click', await deleteCommissionAsync);
}

async function loadCommissionListAsync() {
  const getCommissionListUrl = `${baseUrl}/GetCommissionList`;
  await ajaxGetAsync(getCommissionListUrl)
    .then(async (_commissionList) => {
      commissionList = _commissionList;
    })
    .catch(toastrError);
}

async function openCommissionModalAsync() {
  const getCommissionModalUrl = `${baseUrl}/GetCommissionModal`;
  await ajaxGetAsyncAsString(getCommissionModalUrl)
    .then(async (_commissionModal) => {
      $('#commissionModalContainer').html(_commissionModal);
      $('#commissionModal').modal('show');
      await bindCommissionModalEventsAsync();
    })
    .catch(toastrError);
}

async function openAddCommissionModalAsync() {
  isEdit = false;
  await openCommissionModalAsync();
  $('#department').trigger('change');
}

async function openEditCommissionModalAsync() {

  if (!selectedCommission) {
    toastr.warning($('#pleaseSelectACommission').val());
    return;
  }

  isEdit = true;
  await openCommissionModalAsync();

  $("#commissionModalTitle").text($('#editCommissionModalTitle').val());

  let selectedCommissionData = getSelectedCommissionData();

  $('#employeeID').val(selectedCommissionData.EmployeeID);
  $('#employeeID').prop('disabled', true);

  $('#department').val(selectedCommissionData.Department);
  $('#department').prop('disabled', true);

  await refreshSubDeptDropdownAsync();
  $('#subDept').val(selectedCommissionData.SubDept);
  $('#subDept').prop('disabled', true);

  await refreshSubDetailDropdownAsync();
  $('#subDetail').val(selectedCommissionData.SubDetail);
  $('#subDetail').prop('disabled', true);

  if (selectedCommissionData.GroupName) {
    $('#stockCode').val(`${selectedCommissionData.StockCode}-${selectedCommissionData.GroupName}`);
  }
  else {
    $('#stockCode').val(selectedCommissionData.StockCode);
  }

  $('#hdnStockCode').val(selectedCommissionData.StockCode);
  $('#stockCode').prop('disabled', true);

  $('#commissionPercentage').val(selectedCommissionData.CommissionPercentage);
  $('#commissionAmount').val(selectedCommissionData.CommissionAmount);
  $('#apply').val(Number(selectedCommissionData.Apply));
}

async function bindCommissionModalEventsAsync() {
  $('#department').off().on('change', async function () {
    await refreshSubDeptDropdownAsync();
    await refreshSubDetailDropdownAsync();
    resetStockCode();
  });

  $('#subDept').off().on('change', async function () {
    await refreshSubDetailDropdownAsync();
    resetStockCode();
  });

  $('#subDetail').off().on('change', async function () {
    resetStockCode();
  });

  $('#commissionPercentage, #commissionAmount').on('input', function () {
    const newValue = $(this).val();
    const regex = /^-?(0?\d+\.?\d*)$/; // Allow start with digit, optional decimal point with digits

    if (!regex.test(newValue)) {
      $(this).val(newValue.slice(0, -1));
    }
  });
}

async function refreshSubDeptDropdownAsync() {
  const getSubDeptListUrl = `${baseUrl}/GetSubDeptList`;
  let data = {
    department: $('#department').val()
  }

  await ajaxGetAsync(getSubDeptListUrl, data)
    .then(_subDeptList => {
      $('#subDept').empty();
      $.each(_subDeptList, function (index, item) {
        $('#subDept').append($('<option>', {
          value: item.value,
          text: item.text
        }));
      });
    })
    .catch(toastrError);
}

async function refreshSubDetailDropdownAsync() {
  const getSubDetailListUrl = `${baseUrl}/GetSubDetailList`;
  let data = {
    department: $('#department').val(),
    subDept: $('#subDept').val()
  }

  await ajaxGetAsync(getSubDetailListUrl, data)
    .then(_subDetailList => {
      $('#subDetail').empty();
      $.each(_subDetailList, function (index, item) {
        $('#subDetail').append($('<option>', {
          value: item.value,
          text: item.text
        }));
      });
    })
    .catch(toastrError);
}

function resetStockCode() {
  let allAption = $('#allOption').val();
  $('#stockCode').val(allAption);
  $('#hdnStockCode').val(allAption);
}

async function refreshCommissionGridAsync() {
  await loadCommissionListAsync();
  commissionsDatatable.clear().draw();
  commissionsDatatable.rows.add(commissionList).draw();
  selectedCommission = null;
}

async function addEditCommissionAsync() {
  let commissionDetails = prepareCommissionData();

  if (!(await validateCommissionFields(commissionDetails))) {
    return false;
  }

  const addEditCommissionUrl = `${baseUrl}/AddEditCommission`;
  await ajaxGetAsyncAsString(addEditCommissionUrl, commissionDetails)
    .then((response) => {
      refreshCommissionGridAsync();
      toastr.success(response);
      $('#commissionModal').modal('hide');
    })
    .catch(toastrError);

  return true;
}

async function validateCommissionFields(commission) {
  if (commission.CommissionAmount + commission.CommissionPercentage === 0) {
    toastr.warning($('#noCommissionValidation').val());
    return false;
  }

  if (commission.CommissionAmount !== 0 && commission.CommissionPercentage !== 0) {
    toastr.warning($('#bothCommissionValidation').val());
    return false;
  }

  if (!isEdit) {
    let isExists = await commissionExistsAsync(commission);

    if (isExists) {
      toastr.warning($('#commissionExistsValidation').val());
      return false;
    }
  }

  return true;
}

function prepareCommissionData() {
  const parsedCommissionAmount = parseFloat($('#commissionAmount').val());
  const parsedCommissionPercentage = parseFloat($('#commissionPercentage').val());

  let data = {
    EmployeeID: $('#employeeID').val(),
    Department: $('#department').val(),
    SubDept: $('#subDept').val(),
    SubDetail: $('#subDetail').val(),
    StockCode: $('#hdnStockCode').val(),
    CommissionPercentage: isNaN(parsedCommissionPercentage) ? 0 : parsedCommissionPercentage,
    CommissionAmount: isNaN(parsedCommissionAmount) ? 0 : parsedCommissionAmount,
    Apply: Boolean(parseInt($('#apply').val())),
    IsEdit: isEdit
  }

  return data;
}

async function deleteCommissionAsync() {
  if (!selectedCommission) {
    toastr.warning($('#pleaseSelectACommission').val());
    return;
  }

  let selectedCommissionData = getSelectedCommissionData();
  let title = $('#deleteCommissionTitle').val();
  let message = $('#deleteCommissionConfirmation').val().replace("{0}", selectedCommissionData.EmployeeID);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      const deleteCommissionUrl = `${baseUrl}/DeleteCommission`;

      await ajaxGetAsyncAsString(deleteCommissionUrl, selectedCommissionData)
        .then((response) => {
          refreshCommissionGridAsync();
          toastr.success(response);
        })
        .catch(toastrError);
    }
  });
}

function decodeHtml(encodedString) {
  return encodedString.replace(/&lt;/g, '<').replace(/&gt;/g, '>');
}

function getSelectedCommissionData() {
  let data = {
    EmployeeID: decodeHtml(selectedCommission.employeeID),
    Department: decodeHtml(selectedCommission.department),
    SubDept: decodeHtml(selectedCommission.subDept),
    SubDetail: decodeHtml(selectedCommission.subDetail),
    StockCode: decodeHtml(selectedCommission.stockCode),
    GroupName: selectedCommission.groupName,
    CommissionPercentage: selectedCommission.commissionPercentage,
    CommissionAmount: selectedCommission.commissionAmount,
    Apply: selectedCommission.apply
  }

  return data;
}

async function commissionExistsAsync(commission) {
  const url = `${baseUrl}/CommissionExists`;
  let isExists = true;

  await ajaxGetAsync(url, commission)
    .then((response) => {
      isExists = response;
    })
    .catch(toastrError);

  return isExists;
}

async function openStockCodeModalForCommission() {
  let url = `${baseUrl}/GetStockItemsByCriteria`;
  let data = {
    Department: $('#department').val(),
    SubDept: $('#subDept').val(),
    SubDetail: $('#subDetail').val()
  }

  await ajaxGetAsync(url, data)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);

  renderStockCodeDataTable(onStockCodeRowClickForCommission);
  $("#stockCodeDescriptionDataTableModal").modal('show');
}

function onStockCodeRowClickForCommission(rowData) {
  let stockCode = decodeHtml(rowData.stockCode);

  if (rowData.description) {
    $("#stockCode").val(`${stockCode}-${rowData.description}`);
  } else {
    $("#stockCode").val(stockCode);
  }

  $("#hdnStockCode").val(stockCode);
  $("#stockCodeDescriptionDataTableModal").modal('hide');
}