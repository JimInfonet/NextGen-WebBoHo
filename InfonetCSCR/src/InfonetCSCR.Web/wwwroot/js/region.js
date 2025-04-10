let regionData = [];
let regionDataTable;
let selectedRegion;
let selectedRegionRow;
// Store global variables
let storeData = [];
let storeDataTable;
let selectedStore;
let selectedStoreRow;
let linkStoreData = []; // Link Store data for dropdown

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadRegionData();
});

let loadRegionData = async () => {
  let url = "Region/GetRegion";
  await ajaxGetAsync(url)
    .then(async (_regionData) => {
      if (_regionData) {
        regionData = _regionData;

        if (regionData.length > 0) {
          selectedRegion = regionData[0].id;
          selectedRegionRow = regionData[0];
        }

        $("#region-btn-delete").prop('disabled', regionData.length === 0);
        $("#delete-store-link-btn").prop('disabled', regionData.length === 0);
        $("#link-store-to-the-region").prop('disabled', regionData.length === 0);

        createRegionDataTable();
        await loadStoreData();
        await loadLinkStoreData();
        adjustGridsHeight(['region', 'store']);
      }
    })
    .catch(toastrError);
}

let createRegionDataTable = () => {
  destroyDataTableIfExists(regionDataTable);

  let props = createDataTableProp();
  props.data = regionData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "id", },
    { "data": "name", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  regionDataTable = createDataTableGeneric('region-grid', props);

  if (regionData.length > 0) {
    $(`#region-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#region-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (regionData.length === 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        regionDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedRegion = $(this).find("td:first")[0].innerText;
        selectedRegionRow = regionDataTable.row(this).data();
      }

      await loadStoreData(); // show the store of the selected region
      await loadLinkStoreData(); // Store link to the region dropdown
      adjustGridsHeight(['region', 'store']);
    });
}

$("#region-btn-add").on('click', async function () {
  $("#region-id").val('');
  $("#region-name").val('');
  $('#validation-for-region-id').prop('hidden', true);
  $('#validation-for-region-name').prop('hidden', true);
  $("#regionFormModal").modal('show');
});

let onRegionCompleteClick = async () => {
  let regionFormData = prepareRegionFormData();

  if (!isRegionFormValid(regionFormData)) return;

  let newRegionId = $("#region-id").val();
  let isExist = regionData.find(obj => obj.id.toLowerCase() == newRegionId.toLowerCase());

  if (isExist) {
    toastr.warning($("#idAlreadyExists").val().replace("{0}", newRegionId));
    return;
  }

  let url = `Region/SaveRegion`;
  await ajaxPostAsync(url, regionFormData)
    .then(async (response) => {
      if (response) {
        toastr.success($("#regionAddedSuccessfully").val());
        await loadRegionData();
        $("#regionFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareRegionFormData = () => {
  const formData = {
    id: $("#region-id").val(),
    name: $("#region-name").val()
  }
  return formData;
}

let isRegionFormValid = (regionFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(regionFormData.id)) {
    $('#validation-for-region-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(regionFormData.name)) {
    $('#validation-for-region-name').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

$("#region-btn-delete").on('click', async function () {
  let title = $("#deleteRegionTitle").val();
  let message = `${$("#deleteRegionMessage").val()} ${selectedRegion}?`;
  
  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Region/DeleteRegion?regionId=${selectedRegion}`;
      await ajaxGetAsync(url)
        .then(async (response) => {
          if (response) {
            toastr.success($("#regionDeletedSuccessfully").val());
            await loadRegionData();
          }
        })
        .catch(toastrError);
    }
  });
});

/* :::::::::::::::::::::::::::::: Store Section ::::::::::::::::::::::::::::::: */

let loadStoreData = async () => {
  let url = `Store/GetStoreBySelectedRegion?regionId=${selectedRegion}`;
  await ajaxGetAsync(url)
    .then(_storeData => {
      if (_storeData) {
        storeData = _storeData;

        storeData.forEach(store => {
          store.regionId = selectedRegion;
        });

        if (storeData.length > 0) {
          selectedStore = storeData[0].store_ID;
          selectedStoreRow = storeData[0];
        }

        $("#delete-store-link-btn").prop('disabled', storeData.length === 0);

        createStoreDataTable();
        adjustGridsHeight(['region', 'store']);
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
    { "data": "regionId", },
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
  let url = `Region/GetStoreToLinkBySelectedRegion?regionId=${selectedRegion}`;
  await ajaxGetAsync(url)
    .then(_linkStoreData => {
      if (_linkStoreData) {
        linkStoreData = _linkStoreData;

        let linkStoreDropDown = $("#link-store-to-the-region");
        linkStoreDropDown.empty();

        let defaultOption = $('<option>').attr('value', 'link-store').attr('selected', true).text(`<${$("#linkStoreToTheRegion").val()}>`);
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

$("#link-store-to-the-region").on('change', async function () {
  let selectedOption = $("#link-store-to-the-region").val();
  let selectedOptionDesc = $("#link-store-to-the-region").text().split('-')[1];

  const store = {
    store_ID: selectedOption,
    store_Desc: selectedOptionDesc.trim(),
    region_Id: selectedRegion
  };

  if (selectedOption === 'link-store') {
    return;
  }

  let url = `Region/AddStoreToTheRegion`;
  await ajaxPostAsync(url, store)
    .then(async (response) => {
      if (response) {
        toastr.success($("#storeAddedSuccessfully").val());
        await loadStoreData();
        await loadLinkStoreData();
      }
    })
    .catch(toastrError);
});

$("#delete-store-link-btn").on('click', async function () {
  let title = $("#deleteStoreTitle").val();
  let message = `${$("#deleteStoreMessage").val()} ${selectedStore}?`;

  const storeData = {
    region_Id: selectedStoreRow.regionId,
    store_ID: selectedStoreRow.store_ID,
    store_Desc: selectedStoreRow.store_Desc
  };

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Region/DeleteStoreForSelectedRegion`;
      await ajaxPostAsync(url, storeData)
        .then(async (response) => {
          if (response) {
            toastr.success($("#storeDeletedSuccessfully").val());
            await loadStoreData();
            await loadLinkStoreData();
          }
        })
        .catch(toastrError);
    }
  });
});
