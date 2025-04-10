let entityDataTable;
let selectedEntity;
let policySettingsByEntityDatatable;
let selectedPolicySettingByEntity;

$(async function () {
  $('#ddlEntity').trigger('change');
  createPolicySettingsByEntityDatatable();
});

function createEntityDataTable(columns, data) {
  destroyDataTableIfExists(entityDataTable);

  const headerHtml = columns.map(col => `<th width="${col.width || 'auto'}"></th>`).join('');
  const footerHtml = columns.map(col => `<th class="input substring-search" width="${col.width || 'auto'}" scope="col"></th>`).join('');

  $('#entityGridContainer').html(`
    <table id="entityGrid" class="table table-striped">
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
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = columns;
  entityDataTable = createDatatableWithSearching('entityGrid', props);

  $(`#entityGrid tbody`)
    .off()
    .on('click', 'tr', async function () {
      if (entityDataTable.page.info().recordsDisplay === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        entityDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedEntity = entityDataTable.row(this).data();
        await loadPolicySettingsListByEntityAsync();
      }
    });

  $('#entityGrid tbody tr').first()?.trigger('click');
}

async function handleEntityDDLChangeAsync() {
  $('#lblEntityGridTitle').text($('#lclEntityGridTitle').val().replace('{0}', $('#ddlEntity option:selected').text()));

  switch ($('#ddlEntity').val().toUpperCase()) {
    case 'DEPT':
      await loadEntityDepartmentsAsync();
      break;
    case 'SUBDEPT':
      await loadEntitySubDepartmentsAsync();
      break;
    case 'SUBDETAIL':
      await loadEntitySubDetailsAsync();
      break;
    case 'STOCK':
    case 'STOCKMST':
      await loadEntityStockItemsAsync();
      break;
    case 'USER':
      await loadEntityUsersAsync();
      break;
    case 'UGROUP':
      await loadEntityUserGroupsAsync();
      break;
  }
}

async function loadEntityDepartmentsAsync() {
  const url = 'StockItems/GetDepartments';

  await ajaxGetAsync(url)
    .then(async (_departmentsList) => {
      let columns = [
        { "data": "deptId", "title": $('#lclDepartment').val(), width: "30%" },
        { "data": "deptName", "title": $('#lclDepartmentName').val(), width: "70%" },
      ];

      createEntityDataTable(columns, _departmentsList);
    })
    .catch(toastrError);
}

async function loadEntitySubDepartmentsAsync() {
  const url = 'Policy/GetSubDepartmentsList';

  await ajaxGetAsync(url)
    .then(async (_subDepartmentsList) => {
      let columns = [
        { "data": "deptId", "title": $('#lclDepartment').val(), width: "30%" },
        { "data": "subDeptId", "title": $('#lclSubDepartment').val(), width: "30%" },
        { "data": "subDeptName", "title": $('#lclSubDepartmentName').val(), width: "40%" },
      ];

      createEntityDataTable(columns, _subDepartmentsList);
    })
    .catch(toastrError);
}

async function loadEntitySubDetailsAsync() {
  const url = 'Policy/GetSubDetailsList';

  await ajaxGetAsync(url)
    .then(async (_subDetailsList) => {
      let columns = [
        { "data": "deptId", "title": $('#lclDepartment').val(), width: "20%" },
        { "data": "subDeptId", "title": $('#lclSubDepartment').val(), width: "20%" },
        { "data": "subDetailId", "title": $('#lclSubDetail').val(), width: "20%" },
        { "data": "subDetailName", "title": $('#lclSubDetailName').val(), width: "40%" },
      ];

      createEntityDataTable(columns, _subDetailsList);
    })
    .catch(toastrError);
}

async function loadEntityStockItemsAsync() {
  const url = 'StockItems/GetStockItemsList';

  await ajaxGetAsync(url)
    .then(async (_stockItemsList) => {
      let columns = [
        { "data": "stockCode", "title": $('#lclStockCode').val(), width: "30%" },
        { "data": "description", "title": $('#lclDescription').val(), width: "70%" },
      ];

      createEntityDataTable(columns, _stockItemsList);
    })
    .catch(toastrError);
}

async function loadEntityUsersAsync() {
  const url = 'UserGrid/GetAllUserDetails';

  await ajaxGetAsync(url)
    .then(async (_usersList) => {
      let columns = [
        { "data": "code", "title": $('#lclCode').val(), width: "30%" },
        { "data": "name", "title": $('#lclName').val(), width: "70%" },
      ];

      createEntityDataTable(columns, _usersList);
    })
    .catch(toastrError);
}

async function loadEntityUserGroupsAsync() {
  const url = 'UserGroupGrid/GetAllUserGroups';

  await ajaxGetAsync(url)
    .then(async (_userGroupsList) => {
      let columns = [
        { "data": "code", "title": $('#lclGroup').val(), width: "30%" },
        { "data": "name", "title": $('#lclGroupName').val(), width: "70%" },
      ];

      createEntityDataTable(columns, _userGroupsList);
    })
    .catch(toastrError);
}

function createPolicySettingsByEntityDatatable() {
  destroyDataTableIfExists(policySettingsByEntityDatatable);
  let props = createDataTableProp();
  props.data = null;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-left', targets: '_all' }
  ];

  props.columns = [
    { "data": "description", },
    { "data": "settings", },
    { "data": "levels", },
    { "data": "value", },
  ];

  policySettingsByEntityDatatable = createDatatableWithSearching('policySettingsByEntityGrid', props);

  $(`#policySettingsByEntityGrid tbody`)
    .off()
    .on('click', 'tr', function () {
      if (policySettingsByEntityDatatable.page.info().recordsDisplay === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        policySettingsByEntityDatatable.$('tr.selected-grid-row')?.removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedPolicySettingByEntity = policySettingsByEntityDatatable.row(this).data();
        $('#txtPolicyValues').text(selectedPolicySettingByEntity?.parsedChoices.description);
        $('#btnEditPolicyByEntity').prop('disabled', false);

        if (selectedPolicySettingByEntity?.levels.toUpperCase() === 'COMPANY') {
          $('#btnDeletePolicyByEntity').prop('disabled', true);
        }
        else {
          $('#btnDeletePolicyByEntity').prop('disabled', false);
        }
      }
    })
    .on('dblclick', 'tr', async function () {
      if (policySettingsByEntityDatatable.page.info().recordsDisplay !== 0) {
        $('#btnEditPolicyByEntity').trigger('click');
      }
    });
}

async function loadPolicySettingsListByEntityAsync() {
  let url = `PolicyByEntity/GetPolicySettingsByEntity`;
  let selectedEntityData = getSelectedEntityValue();
  selectedEntityData.value = selectedEntityData.value.join('ÿ');

  await ajaxGetAsync(url, selectedEntityData)
    .then(async (_policySettingsListByEntity) => {
      await refreshPolicySettingsByEntityGridAsync(_policySettingsListByEntity);
    })
    .catch(toastrError);
}

async function refreshPolicySettingsByEntityGridAsync(policySettingsListByEntity) {
  policySettingsByEntityDatatable.clear();

  $('#policySettingsByEntityGrid__footer input').each(function () {
    $(this).val('');
  });

  policySettingsByEntityDatatable.rows.add(policySettingsListByEntity);
  policySettingsByEntityDatatable.search('').columns().search('').draw();
  let selectedEntityData = getSelectedEntityValue();
  let policySettingsByEntityGridTitle = $('#lclPolicySettingByEntityGridTitle').val().replace('{0}', `${selectedEntityData.entityName} - ${selectedEntityData.value.join(' ')}`);
  $('#lblPolicySettingsByEntityGridTitle').text(policySettingsByEntityGridTitle);
  $('#btnEditPolicyByEntity').prop('disabled', true);
  $('#btnDeletePolicyByEntity').prop('disabled', true);
  selectedPolicySettingByEntity = null;
}

function getSelectedEntityValue() {
  let values = Object.values(selectedEntity);
  let entity = {
    entityName: $('#ddlEntity').val().toUpperCase(),
    value: []
  }

  if (entity.entityName === 'SUBDEPT') {
    entity.value = [selectedEntity.deptId, selectedEntity.subDeptId];
  } else if (entity.entityName === 'SUBDETAIL') {
    entity.value = [selectedEntity.deptId, selectedEntity.subDeptId, selectedEntity.subDetailId];
  } else {
    entity.value = [values[0]];
  }

  return entity;
}

function getSelectedPolicyByEntitySettingData() {
  let selectedEntityData = getSelectedEntityValue();
  let selectedPolicySetting = {
    ...selectedPolicySettingByEntity,
    levels: selectedEntityData.entityName,
    value: selectedEntityData.value.join('ÿ'),
    source: 'PolicyByEntity',
    isEdit: selectedPolicySettingByEntity.levels.toUpperCase() !== 'COMPANY'
  }

  return selectedPolicySetting;
}

async function btnEditPolicyByEntityClickAsync() {
  await openEditPolicyModalAsync(getSelectedPolicyByEntitySettingData(), loadPolicySettingsListByEntityAsync);
}

async function btnDeletePolicyByEntityClickAsync() {
  let successMessage = $('#lclDeletePolicySettingSuccess').val().replace('{0}', selectedPolicySettingByEntity?.name);
  await deletePolicySettingAsync(getSelectedPolicyByEntitySettingData(), loadPolicySettingsListByEntityAsync, successMessage);
}