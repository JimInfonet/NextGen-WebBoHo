let _isEdit;
let _addEditPolicySettingSuccessCallback;
let _selectedPolicySetting;
let addEditPolicySettingModalDatatable;

async function openAddEditPolicySettingModalAsync() {
  const url = 'Policy/GetAddEditPolicySettingModal';

  await ajaxGetAsyncAsString(url)
    .then(async (_addEditPolicySettingModalContent) => {
      $('#addEditPolicySettingModalContainer').html(_addEditPolicySettingModalContent);
    })
    .catch(toastrError);

  initDropDownList('ddlLevel', _selectedPolicySetting?.levelsList);

  if (_selectedPolicySetting?.parsedChoices.type === 'Time') {
    $('#txtSetting').prop('hidden', true);
    $('#ddlSetting').prop('hidden', true);
    $('#tmSetting').prop('hidden', false);
  } else {
    let dropDownList = _selectedPolicySetting?.parsedChoices.choicesList;
    if (dropDownList?.length > 0) {
      $('#ddlSetting').prop('hidden', false);
      $('#txtSetting').prop('hidden', true);
    }
    else {
      $('#ddlSetting').prop('hidden', true);
      $('#txtSetting').prop('hidden', false);
    }
    initDropDownList('ddlSetting', _selectedPolicySetting?.parsedChoices.choicesList);
  }

  $('#addEditPolicySettingModal').modal('show');
}

function initDropDownList(element, list) {
  $(`#${element}`).empty();
  let placeHolderText = '';

  if (element === 'ddlLevel') {
    placeHolderText = $('#lclSelectLevel').val();
  } else if (element === 'ddlSetting') {
    placeHolderText = $('#lclSelectSetting').val();
  }

  $(`#${element}`).append($('<option>', {
    value: placeHolderText,
    text: placeHolderText,
    selected: true
  }));

  $.each(list, function (index, item) {
    if (element !== 'ddlLevel' || item !== 'COMPANY') {
      $(`#${element}`).append($('<option>', {
        value: item,
        text: item
      }));
    }
  });

  $(`#${element}`).trigger('change');
}

async function openAddPolicyModalAsync(selectedPolicySetting, callback) {
  _isEdit = false;
  _selectedPolicySetting = selectedPolicySetting;
  _addEditPolicySettingSuccessCallback = callback
  await openAddEditPolicySettingModalAsync();
  $('#addEditPolicySettingModalTitle').text($('#lclAddPolicySetting').val().replace('{0}', _selectedPolicySetting?.name));
}

async function openEditPolicyModalAsync(selectedPolicySetting, callback) {
  _isEdit = true;
  _selectedPolicySetting = selectedPolicySetting;
  _addEditPolicySettingSuccessCallback = callback
  await openAddEditPolicySettingModalAsync();
  $('#addEditPolicySettingModalTitle').text($('#lclEditPolicySetting').val().replace('{0}', _selectedPolicySetting?.name));
  setLevel(_selectedPolicySetting?.levels);
  $('#ddlLevel').prop('disabled', true);
  $('#txtValue').val(_selectedPolicySetting?.value);
  $('#txtValue').prop('disabled', true);

  if (_selectedPolicySetting?.parsedChoices.type === 'Time') {
    $('#tmSetting').val(convertTo24HourFormat(_selectedPolicySetting?.settings));
  } else {
    $('#txtSetting').val(_selectedPolicySetting?.settings);
    $('#ddlSetting').val(_selectedPolicySetting?.settings);
  }

  if (_selectedPolicySetting?.parsedChoices.choicesList) {
    $('#txtSetting').prop('hidden', true);
    $("#ddlSetting").prop('hidden', false);
  }
  else if (_selectedPolicySetting?.parsedChoices.type === "Range") {
    $("#ddlSetting").prop('hidden', true);
    $('#txtSetting').prop('hidden', false);
  }
}

async function handleLevelSettingChangeAsync(eventElement, targetElement) {
  if (eventElement === 'ddlSetting') {
    $('#txtSetting').val($('#ddlSetting').val());

    if (_selectedPolicySetting?.parsedChoices.type !== 'Table') {
      return;
    }
  }

  switch ($(`#${eventElement}`).val().toUpperCase()) {
    case 'CLCATEG':
      await loadClCategoryListAsync(targetElement);
      break;
    case 'CLIENT':
      await loadClientsListAsync(targetElement);
      break;
    case 'DEPT':
      await loadDepartmentsListAsync(targetElement);
      break;
    case 'SUBDEPT':
      await loadSubDepartmentsListAsync(targetElement);
      break;
    case 'SUBDETAIL':
      await loadSubDetailsListAsync(targetElement);
      break;
    case 'STOCK':
    case 'STOCKMST':
      await loadStockItemsListAsync(targetElement);
      break;
    case 'DISCCODE':
      await loadDiscCodesListAsync(targetElement);
      break;
    case 'STORE':
      await loadBranchesListAsync(targetElement);
      break;
    case 'REGISTER':
      await loadRegistersListAsync(targetElement);
      break;
    case 'TENDMAST':
      await loadTendersListAsync(targetElement);
      break;
    case 'UGROUP':
      await loadUserGroupsListAsync(targetElement);
      break;
    case 'UOM':
      await loadUomListAsync(targetElement);
      break;
    case 'USER':
      await loadUsersListAsync(targetElement);
      break;
    case 'TENDER':
      await loadTendersListByClassNameAsync(targetElement);
      break;
    case 'REGION':
      await loadRegionsListAsync(targetElement);
      break;
    case 'PRICEGROUP':
      await loadPriceGroupsListAsync(targetElement);
      break;
    case 'TAXMAST':
      await loadTaxMastsListAsync(targetElement);
      break;
    case 'TAXEXEMPTREASONS':
      await loadTaxExemptReasonsListAsync(targetElement);
      break;
    case 'SALETYPE':
      await loadSaleTypesListAsync(targetElement);
      break;
  }
}

async function loadUsersListAsync(targetElement) {
  const url = 'UserGrid/GetAllUserDetails';

  await ajaxGetAsync(url)
    .then(async (_usersList) => {
      let columns = [
        { "data": "code", "title": $('#lclCode').val(), width: "30%" },
        { "data": "name", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _usersList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadUserGroupsListAsync(targetElement) {
  const url = 'UserGroupGrid/GetAllUserGroups';

  await ajaxGetAsync(url)
    .then(async (_userGroupsList) => {
      let columns = [
        { "data": "code", "title": $('#lclGroup').val(), width: "30%" },
        { "data": "name", "title": $('#lclGroupName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _userGroupsList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadTendersListByClassNameAsync(targetElement) {
  const url = 'Tenders/GetTendersListByClassName';
  let data = { tendClass: 'GIFTCERT' }

  await ajaxGetAsync(url, data)
    .then(async (_tendersList) => {
      let columns = [
        { "data": "code", "title": $('#lclCode').val(), width: "30%" },
        { "data": "fullName", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _tendersList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadSaleTypesListAsync(targetElement) {
  const saleTypes = ["REFUND", "ARPAY", "PAYOUT", "PAYMENT", "BOTTLERETURN", "CASHDROP", "CASHDRAW"];
  let data = saleTypes.map(item => ({ saleType: item }));

  let columns = [
    { "data": "saleType", "title": $('#lclSaleType').val(), width: "100%" },
  ];

  createAddEditPolicySettingModalDatatable(columns, data, targetElement, 'saleType');
}

async function loadStockItemsListAsync(targetElement) {
  const url = 'StockItems/GetStockItemsList';

  await ajaxGetAsync(url)
    .then(async (_stockItemsList) => {
      let columns = [
        { "data": "stockCode", "title": $('#lclCode').val(), width: "30%" },
        { "data": "description", "title": $('#lclDescription').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _stockItemsList, targetElement, 'stockCode');
    })
    .catch(toastrError);
}

async function loadDepartmentsListAsync(targetElement) {
  const url = 'StockItems/GetDepartments';

  await ajaxGetAsync(url)
    .then(async (_departmentsList) => {
      let columns = [
        { "data": "deptId", "title": $('#lclDepartment').val(), width: "30%" },
        { "data": "deptName", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _departmentsList, targetElement, 'deptId');
    })
    .catch(toastrError);
}

async function loadSubDepartmentsListAsync(targetElement) {
  const url = 'Policy/GetSubDepartmentsList';

  await ajaxGetAsync(url)
    .then(async (_subDepartmentsList) => {
      let columns = [
        { "data": "combinedId", "title": $('#lclSubDepartment').val(), width: "30%" },
        { "data": "subDeptName", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _subDepartmentsList, targetElement, 'combinedId');
    })
    .catch(toastrError);
}

async function loadSubDetailsListAsync(targetElement) {
  const url = 'Policy/GetSubDetailsList';

  await ajaxGetAsync(url)
    .then(async (_subDetailsList) => {
      let columns = [
        { "data": "combinedId", "title": $('#lclSubDetail').val(), width: "30%" },
        { "data": "subDetailName", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _subDetailsList, targetElement, 'combinedId');
    })
    .catch(toastrError);
}

async function loadClientsListAsync(targetElement) {
  const url = 'CustomersGrid/GetAllCustomers';

  await ajaxGetAsync(url)
    .then(async (_clientsList) => {
      let columns = [
        { "data": "code", "title": $('#lclCode').val(), width: "30%" },
        { "data": "name", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _clientsList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadTaxExemptReasonsListAsync(targetElement) {
  const url = 'Tax/GetTaxExemptReasonsListByType';
  let data = { reasonType: 'FUEL' };

  await ajaxGetAsync(url, data)
    .then(async (_taxExemptReasonsList) => {
      let columns = [
        { "data": "code", "title": $('#lclReasonCode').val(), width: "30%" },
        { "data": "description", "title": $('#lclReasonDescription').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _taxExemptReasonsList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadTaxMastsListAsync(targetElement) {
  const url = 'Tax/GetTaxesOrderByTaxName';

  await ajaxGetAsync(url)
    .then(async (_taxesList) => {
      let columns = [
        { "data": "name", "title": $('#lclTaxName').val(), width: "30%" },
        { "data": "description", "title": $('#lclDescription').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _taxesList, targetElement, 'name');
    })
    .catch(toastrError);
}

async function loadTendersListAsync(targetElement) {
  const url = 'Tenders/GetTendersList';

  await ajaxGetAsync(url)
    .then(async (_tendersList) => {
      let columns = [
        { "data": "code", "title": $('#lclName').val(), width: "30%" },
        { "data": "fullName", "title": $('#lclDescription').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _tendersList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadRegistersListAsync(targetElement) {
  const url = 'Registers/GetRegisters';

  await ajaxGetAsync(url)
    .then(async (_registersList) => {
      let transformedData = _registersList.map(item => ({ registerNo: item }));

      let columns = [
        { "data": "registerNo", "title": $('#lclRegister').val(), width: "100%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, transformedData, targetElement, 'registerNo');
    })
    .catch(toastrError);
}

async function loadRegionsListAsync(targetElement) {
  const url = 'Region/GetRegion';

  await ajaxGetAsync(url)
    .then(async (_regionsList) => {
      let columns = [
        { "data": "id", "title": $('#lclRegionID').val(), width: "30%" },
        { "data": "name", "title": $('#lclRegionName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _regionsList, targetElement, 'id');
    })
    .catch(toastrError);
}

async function loadPriceGroupsListAsync(targetElement) {
  const url = 'Policy/GetPriceGroupsList';

  await ajaxGetAsync(url)
    .then(async (_priceGroupsList) => {
      let columns = [
        { "data": "id", "title": $('#lclPriceGroupID').val(), width: "30%" },
        { "data": "name", "title": $('#lclPriceGroupName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _priceGroupsList, targetElement, 'id');
    })
    .catch(toastrError);
}

async function loadClCategoryListAsync(targetElement) {
  const url = 'Policy/GetClCategoryList';

  await ajaxGetAsync(url)
    .then(async (_clCategoryList) => {
      let columns = [
        { "data": "category", "title": $('#lclCategory').val(), width: "30%" },
        { "data": "clCategoryName", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _clCategoryList, targetElement, 'category');
    })
    .catch(toastrError);
}

async function loadDiscCodesListAsync(targetElement) {
  const url = 'Policy/GetDiscCodesList';

  await ajaxGetAsync(url)
    .then(async (_discCodesList) => {
      let columns = [
        { "data": "code", "title": $('#lclCode').val(), width: "30%" },
        { "data": "discCodeDescription", "title": $('#lclDescription').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _discCodesList, targetElement, 'code');
    })
    .catch(toastrError);
}

async function loadBranchesListAsync(targetElement) {
  const url = 'Policy/GetBranchesList';

  await ajaxGetAsync(url)
    .then(async (_branchesList) => {
      let columns = [
        { "data": "branchCode", "title": $('#lclCode').val(), width: "30%" },
        { "data": "branchName", "title": $('#lclName').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _branchesList, targetElement, 'branchCode');
    })
    .catch(toastrError);
}

async function loadUomListAsync(targetElement) {
  const url = 'Policy/GetUomList';

  await ajaxGetAsync(url)
    .then(async (_uomList) => {
      let columns = [
        { "data": "unitOfMeasureID", "title": $('#lclUom').val(), width: "30%" },
        { "data": "description", "title": $('#lclDescription').val(), width: "70%" },
      ];

      createAddEditPolicySettingModalDatatable(columns, _uomList, targetElement, 'unitOfMeasureID');
    })
    .catch(toastrError);
}

function createAddEditPolicySettingModalDatatable(columns, data, targetElement, targetField) {
  destroyDataTableIfExists(addEditPolicySettingModalDatatable);

  const headerHtml = columns.map(col => `<th width="${col.width || 'auto'}"></th>`).join('');
  const footerHtml = columns.map(col => `<th class="input substring-search" width="${col.width || 'auto'}" scope="col"></th>`).join('');

  $('#addEditPolicySettingModalGridContainer').html(`
    <table id="addEditPolicySettingModalGrid" class="table table-striped">
      <thead class="datatable-allignment">
        <tr>${headerHtml}</tr>
      </thead>
      <tfoot>
        <tr>${footerHtml}</tr>
      </tfoot>
    </table>
  `);

  let props = createDataTableProp();
  props.data = data;
  props.scrollY = '240px';
  props.paging = true;
  props.searching = true;

  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = columns;
  addEditPolicySettingModalDatatable = createModalDatatableWithSearching('addEditPolicySettingModalGrid', props);

  $('#addEditPolicySettingModalFieldsContainer').removeClass('col-lg-12').addClass('col-lg-5');
  $('.policy-by-class-modal').removeClass('reduced').addClass('expanded');
  $('#addEditPolicySettingModalGridContainer').prop('hidden', false);
  addEditPolicySettingModalDatatable.columns.adjust();

  $(`#addEditPolicySettingModalGrid tbody`)
    .off()
    .on('click', 'tr', function () {
      if (addEditPolicySettingModalDatatable.page.info().recordsDisplay === 0) {
        return;
      }

      let selectedItem = addEditPolicySettingModalDatatable.row(this).data();
      let selectedElement = selectedItem[targetField];

      if (targetElement == 'ddlSetting' || targetElement == 'txtSetting') {
        $(`#ddlSetting`).prepend(`<option value="" disabled selected hidden>${selectedElement}</option>`);
      }
      $(`#${targetElement}`).val(selectedElement);
      $(`#${targetElement}`).trigger('change');
      $('#addEditPolicySettingModalFieldsContainer').removeClass('col-lg-5').addClass('col-lg-12');
      $('#addEditPolicySettingModalGridContainer').prop('hidden', true);
      $('.policy-by-class-modal').removeClass('expanded').addClass('reduced');
    });
}

function setLevel(value) {
  if (!$(`#ddlLevel option[value='${value}']`).length) {
    $('#ddlLevel').append($('<option>', {
      value: value,
      text: value
    }));
  }

  $('#ddlLevel').val(value);
}

async function validateAndGetDataAsync() {
  if ($('#ddlLevel').val().toUpperCase() !== 'COMPANY' && $('#txtValue').val().trim() === '') {
    toastr.warning($('#lclNoValueSupplied').val());
    return null;
  }

  if ($('#ddlLevel').val().trim() === '' || $('#ddlLevel').val() === $('#lclSelectLevel').val()) {
    toastr.warning($('#lclSelectALevel').val());
    return null;
  }

  let settingElement = _selectedPolicySetting?.parsedChoices.type === 'Time' ? 'tmSetting' : 'txtSetting';

  if ($(`#${settingElement}`).val().trim() === '' || $(`#${settingElement}`).val() === $('#lclSelectSetting').val()) {
    toastr.warning($('#lclSelectASetting').val());
    return null;
  }

  if (_selectedPolicySetting?.parsedChoices.type === 'Range') {
    const setting = Number($('#txtSetting').val().trim());

    if (isNaN(setting) || setting < _selectedPolicySetting?.parsedChoices.startRange || _selectedPolicySetting?.parsedChoices.endRange < setting) {
      let rangeErrorMessage = $('#lclRangeError').val();
      rangeErrorMessage = rangeErrorMessage.replace('{0}', _selectedPolicySetting?.parsedChoices.startRange);
      rangeErrorMessage = rangeErrorMessage.replace('{1}', _selectedPolicySetting?.parsedChoices.endRange);
      toastr.warning(rangeErrorMessage);
      return null;
    }

    if (_selectedPolicySetting?.name === 'INITUSERID') {
      let maxUserID = 0;
      const url = 'UserGrid/GetMaxUserID';

      await ajaxGetAsync(url)
        .then(async (_maxUserID) => {
          maxUserID = Number(_maxUserID);
        })
        .catch(toastrError);

      if (setting <= maxUserID) {
        toastr.warning($('#lclInvalidInitUserID').val());
        return null;
      }
    }
  }

  let data = {
    Name: _selectedPolicySetting?.name,
    Levels: $('#ddlLevel').val().trim(),
    Value: $('#txtValue').val().trim(),
    Settings: $(`#${settingElement}`).val().trim()
  }

  if (_selectedPolicySetting?.name.toUpperCase() === 'INITUSERID') {
    let logText = `InitialEmpID in Timesheetpayperiod updated from Value_Selected procedure in frmPolicy with ${data.Settings}`;
    await saveLogToFileBasedOnConfigAsync($('#_logFolderPath').val(), $('#_logFileName').val(), logText);
  }

  if ($('#_isBackOffice').val() !== 'True' && _selectedPolicySetting?.name.toUpperCase() === 'SET_VEND') {
    let setVendPolicyLog;
    const url = 'Policy/GetSetVendPolicyLog';

    await ajaxGetAsync(url, data)
      .then(async (_setVendPolicyLog) => {
        setVendPolicyLog = _setVendPolicyLog;
      })
      .catch(toastrError);

    await saveLogToFileBasedOnConfigAsync($('#_logFolderPath').val(), 'StockItems', setVendPolicyLog.logText);

    if (!setVendPolicyLog.success) {
      toastr.warning($('#lclSetVendError').val());
      return null;
    }
  }

  if (_selectedPolicySetting?.parsedChoices.type === 'Time') {
    data.Settings = convertTo12HourFormat(data.Settings);
  }

  if (_selectedPolicySetting.source === 'PolicyByClass' && !_isEdit) {
    let isSettingExists = false;
    const url = 'Policy/CheckIfPolicySettingExists';

    await ajaxGetAsync(url, data)
      .then(async (_isSettingExists) => {
        isSettingExists = _isSettingExists;
      })
      .catch(toastrError);

    if (isSettingExists) {
      toastr.warning($('#lclDuplicatePolicySettingError').val());
      return null;
    }
  }

  return data;
}

async function addEditPolicySetting() {
  $('#addEditPolicySettingModal button').prop('disabled', true);
  let data = await validateAndGetDataAsync();

  if (!data) {
    $('#addEditPolicySettingModal button').prop('disabled', false);
    return;
  }

  let url = _isEdit ? 'Policy/UpdatePolicySetting' : 'Policy/AddPolicySetting';

  if (_selectedPolicySetting.source === 'PolicyByEntity' && !_selectedPolicySetting.isEdit) {
    url = 'Policy/AddPolicySetting';
  }

  await ajaxGetAsync(url, data)
    .then(async (response) => {
      await _addEditPolicySettingSuccessCallback();
      let successMessage = _isEdit ? $('#lclEditPolicySettingSuccess').val() : $('#lclAddPolicySettingSuccess').val();
      successMessage = successMessage.replace('{0}', _selectedPolicySetting?.name);
      toastr.success(successMessage);
      $('#addEditPolicySettingModal').modal('hide');
      _selectedPolicySetting = null;
    })
    .catch(toastrError);
}

async function deletePolicySettingAsync(selectedPolicySetting, callback, successMessage) {
  let data = {
    Name: selectedPolicySetting?.name,
    Levels: selectedPolicySetting?.levels,
    Value: selectedPolicySetting?.value,
    Settings: selectedPolicySetting?.settings
  }

  const url = 'Policy/DeletePolicySetting';

  await ajaxGetAsync(url, data)
    .then(async (response) => {
      await callback();
      toastr.success(successMessage);
    })
    .catch(toastrError);
}

function handletxtSettingChange(value) {
  const maxLength = 45;
  $('#ddlSetting').val('');

  if (value.length > maxLength) {
    $("#txtSettingError").prop("hidden", false);
    $('#addEditPolicySettingModal button').prop('disabled', true);
  } else {
    $("#txtSettingError").prop("hidden", true);
    $('#addEditPolicySettingModal button').prop('disabled', false);
  }
}
