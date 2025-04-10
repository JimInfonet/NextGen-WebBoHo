let priceGroupData = [];
let priceGroupDataTable;
let selectedPriceGroup;
let selectedPriceGroupRow;
// Store global variables
let storeData = [];
let storeDataTable;
let selectedStore;
let selectedStoreRow;
let linkStoreData = []; // Link Store data for dropdown

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadPriceGroupData();
  await loadStoreData();
  await loadLinkStoreData();
  adjustGridsHeight(['priceGroup', 'store']);
});

let loadPriceGroupData = async () => {
  let url = "PriceGroup/GetAllPriceGroups";
  await ajaxGetAsync(url)
    .then(async (_priceGroupData) => {
      if (_priceGroupData) {
        priceGroupData = _priceGroupData;

        if (priceGroupData.length > 0) {
          selectedPriceGroup = priceGroupData[0].id;
          selectedPriceGroupRow = priceGroupData[0];
        }

        $("#priceGroup-btn-delete").prop('disabled', priceGroupData.length === 0);
        $("#link-store-to-the-priceGroup").prop('disabled', priceGroupData.length === 0);

        createPriceGroupDataTable();
        await loadStoreData();
        await loadLinkStoreData();
        adjustGridsHeight(['priceGroup', 'store']);
      }
    })
    .catch(toastrError);
}

let createPriceGroupDataTable = () => {
  destroyDataTableIfExists(priceGroupDataTable);

  let props = createDataTableProp();
  props.data = priceGroupData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "id", },
    { "data": "name", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  priceGroupDataTable = createDataTableGeneric('priceGroup-grid', props);

  if (priceGroupData.length > 0) {
    $(`#priceGroup-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#priceGroup-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (priceGroupData.length === 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        priceGroupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedPriceGroup = $(this).find("td:first")[0].innerText;
        selectedPriceGroupRow = priceGroupDataTable.row(this).data();
      }

      await loadStoreData(); // show the store of the selected priceGroup
      await loadLinkStoreData(); // Store link to the priceGroup dropdown
      adjustGridsHeight(['priceGroup', 'store']);
    });
}

$("#priceGroup-btn-add").on('click', async function () {
  $("#priceGroup-id").val('');
  $("#priceGroup-name").val('');
  $('#validation-for-priceGroup-id').prop('hidden', true);
  $('#validation-for-priceGroup-name').prop('hidden', true);
  $("#priceGroupFormModal").modal('show');
});

let onPriceGroupCompleteClick = async () => {
  let priceGroupFormData = preparePriceGroupFormData();

  if (!isPriceGroupFormValid(priceGroupFormData)) return;

  let newPriceGroupId = $("#priceGroup-id").val();
  let isExist = priceGroupData.find(obj => obj.id.toLowerCase() == newPriceGroupId.toLowerCase());

  if (isExist) {
    toastr.warning($("#idAlreadyExists").val().replace("{0}", newPriceGroupId));
    return;
  }

  let url = `PriceGroup/SavePriceGroup`;
  await ajaxPostAsync(url, priceGroupFormData)
    .then(async (response) => {
      if (response) {
        toastr.success($("#priceGroupAddedSuccessfully").val());
        await loadPriceGroupData();
        $("#priceGroupFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let preparePriceGroupFormData = () => {
  const formData = {
    id: $("#priceGroup-id").val(),
    name: $("#priceGroup-name").val()
  }
  return formData;
}

let isPriceGroupFormValid = (priceGroupFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(priceGroupFormData.id)) {
    $('#validation-for-priceGroup-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(priceGroupFormData.name)) {
    $('#validation-for-priceGroup-name').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

$("#priceGroup-btn-delete").on('click', async function () {
  let title = $("#deletePriceGroupTitle").val();
  let message = `${$("#deletePriceGroupMessage").val()} ${selectedPriceGroup}?`;

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `PriceGroup/DeletePriceGroup?priceGroupId=${selectedPriceGroup}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#priceGroupDeletedSuccessfully").val());
            await loadPriceGroupData();
          }
        })
        .catch(toastrError);
    }
  });
});

/* :::::::::::::::::::::::::::::: Store Section ::::::::::::::::::::::::::::::: */

let loadStoreData = async () => {
  let url = `PriceGroup/GetStoreBySelectedPriceGroup?priceGroupId=${selectedPriceGroup}`;
  await ajaxGetAsync(url)
    .then(_storeData => {
      if (_storeData) {
        storeData = _storeData;

        storeData.forEach(store => {
          store.priceGroupId = selectedPriceGroup;
        });

        if (storeData.length > 0) {
          selectedStore = storeData[0].store_ID;
          selectedStoreRow = storeData[0];
        }

        $("#delete-store-link-btn").prop('disabled', storeData.length === 0);

        createStoreDataTable();
        adjustGridsHeight(['priceGroup', 'store']);
      }
    })
    .catch(toastrError);
}

let createStoreDataTable = () => {
  destroyDataTableIfExists(storeDataTable);

  let props = createDataTableProp();
  props.data = storeData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "priceGroupId", },
    { "data": "store_ID", },
    { "data": "store_Desc", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  storeDataTable = createDataTableGeneric('store-grid', props);

  if (storeData.length > 0) {
    $(`#store-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#store-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (storeData.length === 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        storeDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedStore = $(this).find("td:eq(1)").text();
        selectedStoreRow = storeDataTable.row(this).data();
      }
    });
}

let loadLinkStoreData = async () => {
  let url = `PriceGroup/GetStoreToLinkBySelectedPriceGroup?priceGroupId=${selectedPriceGroup}`;
  await ajaxGetAsync(url)
    .then(_linkStoreData => {
      if (_linkStoreData) {
        linkStoreData = _linkStoreData;

        let linkStoreDropDown = $("#link-store-to-the-priceGroup");
        linkStoreDropDown.empty();

        let defaultOption = $('<option>').attr('value', 'link-store').attr('selected', true).text(`<${$("#linkStoreToThePriceGroup").val()}>`);
        linkStoreDropDown.append(defaultOption);

        linkStoreData.forEach(store => {
          let option = $('<option>').attr('value', store.store_ID).text(`${store.store_ID} - ${store.store_Desc}`);
          linkStoreDropDown.append(option);
        });
      }
    })
    .catch(toastrError);
}

let adjustGridsHeight = (row) => {
  let mediaQuery = window.matchMedia('(min-width: 1366px)');

  if (!mediaQuery.matches) return;

  let maxHeight = 0;

  row.forEach(col => {
    let grid = $(`#${col}-table`);
    grid.height('auto');
    maxHeight = Math.max(grid.height(), maxHeight);
  });

  row.forEach(col => {
    let grid = $(`#${col}-table`);
    grid.height(maxHeight);
  });
}

$("#link-store-to-the-priceGroup").on('change', async function () {
  let selectedOption = $("#link-store-to-the-priceGroup").val();
  let selectedOptionDesc = $("#link-store-to-the-priceGroup").text().split('-')[1];

  const store = {
    store_ID: selectedOption,
    store_Desc: selectedOptionDesc,
    priceGroup_Id: selectedPriceGroup
  };

  if (selectedOption === 'link-store') {
    return;
  }

  let url = `PriceGroup/AddStoreToThePriceGroup`;
  await ajaxPostAsync(url, store)
    .then(async (response) => {
      if (response) {
        toastr.success($("#storeAddedSuccessfully").val());
        await loadStoreData();
        await loadLinkStoreData();
        adjustGridsHeight(['priceGroup', 'store']);
      }
    })
    .catch(toastrError);
});

$("#delete-store-link-btn").on('click', async function () {
  let title = $("#deleteStoreTitle").val();
  let message = `${$("#deleteStoreMessage").val()} ${selectedStore}?`;

  const storeData = {
    priceGroup_Id: selectedStoreRow.priceGroupId,
    store_ID: selectedStoreRow.store_ID,
    store_Desc: selectedStoreRow.store_Desc
  };

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `PriceGroup/DeleteStoreForSelectedPriceGroup`;
      await ajaxPostAsync(url, storeData)
        .then(async (response) => {
          if (response) {
            toastr.success($("#storeDeletedSuccessfully").val());
            await loadStoreData();
            await loadLinkStoreData();
            adjustGridsHeight(['priceGroup', 'store']);
          }
        })
        .catch(toastrError);
    }
  });
});