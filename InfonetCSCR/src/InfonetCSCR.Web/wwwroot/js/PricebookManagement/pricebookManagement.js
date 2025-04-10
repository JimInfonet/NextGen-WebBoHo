let departmentDatatable;
let subDepartmentDatatable;
let subDetailDatatable;
let selectedDeptId = [];
let selectedSubDeptId = [];
let selectedSubDetail = [];

$(async function () {
  await loadDepartmentsPBM();

  clearLocalStorage();
  createDepartmentDatatable();
  createSubDepartmentDatatable();
  createSubDetailDatatable();
  showProductCategories();

  const standardLabel = $("input[name='label-format'][value='STANDARD']");
  const portraitOption = $("input[name='label-orientation'][value='PORTRAIT']");
  const landscapeOption = $("input[name='label-orientation'][value='LANDSCAPE']");
  const labelFormatOptions = $("input[name='label-format']");

  function toggleOrientationOptions() {
    if (standardLabel.is(":checked")) {
      portraitOption.prop("disabled", false);
      landscapeOption.prop("disabled", false);
      portraitOption.prop("checked", true);
    } else {
      portraitOption.prop("disabled", true);
      landscapeOption.prop("checked", true); // Force landscape mode when not standard
    }
  }

  // Attach event listener to all label format radio buttons
  labelFormatOptions.on("change", toggleOrientationOptions);

  // Run on page load to apply the correct state
  toggleOrientationOptions();
});

let showProductCategories = async () => {
  $("#vendorOptionContainer").hide();
  $("#productCategoryContainer").show();
  $("#btn-categories").prop("disabled", true);
  $("#btn-vendors").prop("disabled", false);

  vendorId = STRING_TABLE.DEFAULT.VENDOR_ID;
  localStorage.setItem('vendorId', vendorId);
  await triggerSelectionTab();
  _selectedVendor = "";
}

let showVendors = async () => {
  document.getElementById('code').checked = true;
  await renderVendorOptionsPBM(1);
  $("#vendorOptionContainer").show();
  $("#productCategoryContainer").hide();
  $("#btn-categories").prop("disabled", false);
  $("#btn-vendors").prop("disabled", true);

  if (vendors.length > 0) {
    _selectedVendor = vendors[0].code;
    selectVendor(vendors[0].code);
  }
}

let selectVendor = (selectedVendorId) => {
  vendorId = selectedVendorId;
  _selectedVendor = selectedVendorId;
  localStorage.setItem('vendorId', vendorId);
}

let loadDepartmentsPBM = async () => {
  let url = "PricebookTransfer/GetDepartments";
  await ajaxGetAsync(url, {})
    .then(async (_departments) => {
      departments = _departments;
      createDepartmentDatatable();
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let loadSubDepartmentsPBM = async () => {
  let url = "PricebookManagement/GetSubDepartmentsByDepartments";

  await ajaxPostAsync(url, selectedDeptId)
    .then(async (_subDepartments) => {
      subDepartments = _subDepartments;
      createSubDepartmentDatatable();
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let loadSubDetailsPBM = async () => {
  let url = "PricebookManagement/GetSubDetailsByDepartments";

  await ajaxPostAsync(url, selectedDeptId)
    .then(async (_subDetails) => {
      subDetails = _subDetails;
      createSubDetailDatatable();
    })
    .catch((error) => { toastr.error(error); });
}

let createDepartmentDatatable = () => {
  destroyDataTableIfExists(departmentDatatable);

  let props = createDataTableProp();
  props.data = departments;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    { "data": "deptId", },
    { "data": "deptName", },
    {
      "data": "deptId",
      render: function (deptId) {
        return `<input class="form-check-input mt-2 select-by-option-dept" type="checkbox" value="${deptId}" 
                       onChange="departmentSelectionChange('${deptId}', this.checked )" />`;
      }
    }];

  departmentDatatable = createDatatableWithSearching('department-grid', props);

  $(`#department-grid tbody`)
    .off()
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        departmentDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
    });
}

let createSubDepartmentDatatable = () => {
  destroyDataTableIfExists(subDepartmentDatatable);

  let props = createDataTableProp();
  props.data = subDepartments;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    { "data": "deptId", },
    { "data": "subDeptId", },
    { "data": "subDeptName", },
    {
      "data": "subDeptId",
      render: function (subDeptId, type, row) {
        return `<input class="form-check-input mt-2 select-by-option-sub-dept" type="checkbox" value="${subDeptId}"
                       onChange="subDepartmentSelectionChange('${row.deptId} ${subDeptId}', this.checked)" />`;
      }
    }];

  subDepartmentDatatable = createDatatableWithSearching('sub-department-grid', props);

  $(`#sub-department-grid tbody`)
    .off()
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        subDepartmentDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
    });
}

let createSubDetailDatatable = () => {
  destroyDataTableIfExists(subDetailDatatable);

  let props = createDataTableProp();
  props.data = subDetails;
  props.paging = false;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    { "data": "deptId", },
    { "data": "subDeptId", },
    { "data": "subDetailId", },
    { "data": "subDetailName", },
    {
      "data": "subDetailId",
      render: function (subDetailId, type, row) {
        return `<input class="form-check-input mt-2 select-by-option-sub-detail" type="checkbox" value="${row.deptId} ${row.subDeptId} ${subDetailId}"
                       onChange="subDetailSelectionChange('${row.deptId} ${row.subDeptId} ${subDetailId}')" />`;
      }
    }];

  subDetailDatatable = createDatatableWithSearching('sub-detail-grid', props);

  $(`#sub-detail-grid tbody`)
    .off()
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        subDetailDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
    });
}

let departmentSelectionChange = async (deptId, isChecked) => {
  const index = selectedDeptId.indexOf(deptId);
  (isChecked) ? selectedDeptId.push(deptId) : selectedDeptId.splice(index, 1);

  handleDepartmentSelectionChange(deptId);
  clearSubDepartmentAndSubDetails();

  await loadSubDepartmentsPBM();
  createSubDepartmentDatatable();
  await loadSubDetailsPBM();
}

let subDepartmentSelectionChange = (deptAndSubDeptId, isChecked) => {
  const [deptId, subDeptId] = deptAndSubDeptId.split(" ");
  const index = selectedSubDeptId.indexOf(subDeptId);
  (index === -1) ? selectedSubDeptId.push(subDeptId) : selectedSubDeptId.splice(index, 1);

  handleSubDepartmentSelectionChange(deptAndSubDeptId);

  const subDetailCheckboxes = document.querySelectorAll(`.select-by-option-sub-detail[value^='${deptId} ${subDeptId}']`);
  subDetailCheckboxes.forEach(checkbox => {
    checkbox.checked = isChecked;
    checkbox.dispatchEvent(new Event('change'));
  });
}

let subDetailSelectionChange = (deptAndSubDeptAndSubDetailId) => {
  handleSubDetailSelectionChange(deptAndSubDeptAndSubDetailId);
}

let renderVendorOptionsPBM = async (columnNumber) => {
  await loadVendorsPBM(columnNumber);

  const container = $("#vendorOptions");
  container.empty();

  vendors.forEach(vendor => {
    container.append(`
       <option value="${vendor.code}" onclick="selectVendor(this.value)">
          ${vendor.code} - ${vendor.name}
       </option>
    `)
  });

  container.find("option:first-child").prop("selected", true);
}

let loadVendorsPBM = async (columnNumber) => {
  let url = `PricebookManagement/GetVendorsOrderByColumnNumber?columnNumber=${columnNumber}`;

  await ajaxGetAsync(url)
    .then(_vendors => vendors = _vendors)
    .catch(toastrError);
}

$('#selection-link-wrapper').on('shown.bs.tab', function () {
  selectedDeptId = [];
  selectedSubDeptId = [];
  selectedSubDetail = [];
});

let handleFinalize = async () => {
  if (selectedDataTransferType === PRICEBOOK_TRANSFER_STRING_TABLE.DATA_TRANSFER_TYPE.LABEL_PRINTING) {
    if (labelPrints.length === 0) {
      toastr.warning($("#noDataToTransfer").val());
      return;
    }
    await handleDeleteFromTransferList(null, null, null, 'ALL');
  }
  else {
    await handlePerformTransfer(true);
  }
}

$("#btn-report").on('click', function () {
  if (selectedDataTransferType !== PRICEBOOK_TRANSFER_STRING_TABLE.DATA_TRANSFER_TYPE.LABEL_PRINTING) {
    displayPriceBookTransferReport()
    return;
  }

  let language = document.getElementById('language').value;
  let width = 1000;
  let height = window.screen.height - 50;
  let reportName = "LabelPrintingReport";

  let result = {};
  result.reportName = reportName;
  result.format = $("input[name='label-format']:checked").val();
  result.orientation = $("input[name='label-orientation']:checked").val();
  result.language = language;
  result.reportWidth = "900px";
  result.reportHeight = window.screen.height - 150;
  displayReport(reportName, width, height, result);
});
