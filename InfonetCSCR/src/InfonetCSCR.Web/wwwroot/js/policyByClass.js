let policiesByClassDatatable;
let policySettingsByClassDatatable
let selectedPolicyByClass;
let selectedPolicySettingByClass;

$(async function () {
  createPoliciesByClassDatatable();
  createPolicySettingsByClassDatatable();
  $('#policiesClassList').trigger('change');
  await ajaxPostAsync('PolicyByClass/UpdateTimesheetPayPeriodOnLoad');
});

async function loadPoliciesByClassAsync() {
  let url = `PolicyByClass/GetPoliciesByClass`;
  let data = {
    className: $('#policiesClassList').val()
  };

  await ajaxGetAsync(url, data)
    .then(async (_policiesListByClass) => {
      await refreshPoliciesByClassGridAsync(_policiesListByClass);
    })
    .catch(toastrError);
}

async function loadPolicySettingsByNameAsync() {
  let url = `PolicyByClass/GetPolicySettingsByName`;
  let data = {
    policyName: selectedPolicyByClass?.name
  };

  await ajaxGetAsync(url, data)
    .then(async (_policySettingsListByName) => {
      await refreshPolicySettingsByClassGridAsync(_policySettingsListByName);
    })
    .catch(toastrError);
}


function createPoliciesByClassDatatable() {
  destroyDataTableIfExists(policiesByClassDatatable);

  let props = createDataTableProp();
  props.data = null;
  props.scrollY = '440px';
  props.paging = true;
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    { "data": "name", },
    { "data": "description", },
    { "data": "choices", }
  ];

  policiesByClassDatatable = createDatatableWithSearching('policiesByClassGrid', props);

  $(`#policiesByClassGrid tbody`)
    .off()
    .on('click', 'tr', async function () {
      if (policiesByClassDatatable.page.info().recordsDisplay === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        policiesByClassDatatable.$('tr.selected-grid-row')?.removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedPolicyByClass = policiesByClassDatatable.row(this).data();
        await loadPolicySettingsByNameAsync();
      }
    });
}

async function refreshPoliciesByClassGridAsync(policiesListByClass) {
  policiesByClassDatatable.clear();

  $('#policiesByClassGrid__footer input').each(function () {
    $(this).val('');
  });

  policiesByClassDatatable.rows.add(policiesListByClass);
  policiesByClassDatatable.search('').columns().search('').draw();
  $('#policiesByClassGrid tbody tr').first()?.trigger('click');
}

async function refreshPolicySettingsByClassGridAsync(policySettingsListByName) {
  policySettingsByClassDatatable.clear();

  $('#policySettingsGrid__footer input').each(function () {
    $(this).val('');
  });

  policySettingsByClassDatatable.rows.add(policySettingsListByName);
  policySettingsByClassDatatable.search('').columns().search('').draw();
  $('#lblPolicySettingsGridTitle').text(selectedPolicyByClass?.description);
  $('#txtPolicyValues').text(selectedPolicyByClass?.parsedChoices.description);
  $('#txtPolicyLevels').text(selectedPolicyByClass?.levelsList.join('\n'));
  $('#btnAddPolicyByClass').prop('disabled', !selectedPolicyByClass?.multipleLevelExists);
  $('#btnEditPolicyByClass').prop('disabled', true);
  $('#btnDeletePolicyByClass').prop('disabled', true);
  selectedPolicySettingByClass = null;
}

function createPolicySettingsByClassDatatable() {
  destroyDataTableIfExists(policySettingsByClassDatatable);

  let props = createDataTableProp();
  props.data = null;
  props.scrollY = '440px';
  props.paging = true;
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    { "data": "levels", },
    { "data": "value", },
    { "data": "settings", }
  ];

  policySettingsByClassDatatable = createDatatableWithSearching('policySettingsGrid', props);

  $(`#policySettingsGrid tbody`)
    .off()
    .on('click', 'tr', function () {
      if (policySettingsByClassDatatable.page.info().recordsDisplay === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        policySettingsByClassDatatable.$('tr.selected-grid-row')?.removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedPolicySettingByClass = policySettingsByClassDatatable.row(this).data();
        $('#btnEditPolicyByClass').prop('disabled', false);

        if (selectedPolicySettingByClass?.levels.toUpperCase() === 'COMPANY') {
          $('#btnDeletePolicyByClass').prop('disabled', true);
        }
        else {
          $('#btnDeletePolicyByClass').prop('disabled', false);
        }
      }
    })
    .on('dblclick', 'tr', async function () {
      if (policySettingsByClassDatatable.page.info().recordsDisplay !== 0) {
        $('#btnEditPolicyByClass').trigger('click');
      }
    });
}

function getSelectedPolicyByClassSettingData() {
  let selectedPolicySetting = {
    ...selectedPolicyByClass,
    levels: selectedPolicySettingByClass?.levels,
    value: selectedPolicySettingByClass?.value,
    settings: selectedPolicySettingByClass?.settings,
    source: 'PolicyByClass'
  }

  return selectedPolicySetting;
}

async function btnAddPolicyByClassClickAsync() {
  await openAddPolicyModalAsync(getSelectedPolicyByClassSettingData(), loadPolicySettingsByNameAsync);
}

async function btnEditPolicyByClassClickAsync() {
  await openEditPolicyModalAsync(getSelectedPolicyByClassSettingData(), loadPolicySettingsByNameAsync);
}

async function btnDeletePolicyByClassClickAsync() {
  let successMessage = $('#lclDeletePolicySettingSuccess').val().replace('{0}', selectedPolicyByClass?.name);
  await deletePolicySettingAsync(getSelectedPolicyByClassSettingData(), loadPolicySettingsByNameAsync, successMessage);
}