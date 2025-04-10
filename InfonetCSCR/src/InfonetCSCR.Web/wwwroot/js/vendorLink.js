let vendorsDatatable;
let vendors;
let selectedVendorCode;
let storeRegions;
let selectedStoreId;
let selectedRegionId;
let storeRegionDatatable;
const URL_GET_VENDORS = "/Vendors";

$(async function () {
  await initVendorPage();
});

let initVendorPage = async () => {
  await loadVendors();
  initActions();
}

let initActions = () => {
  $('#btn-add').on('click', function () {
    if (!selectedVendorCode) {
      toastr.warning($("#pleaseSelectAVendor").val());
      return;
    }
    $("#insertStoreRegionBtn").prop('disabled', false);
    $('#storeItemSelect').prop('selectedIndex', 0);
    $('#regionItemSelect').prop('selectedIndex', 0);
    $('#addNewStoreRegionItemModal').modal('show');
  });

  $('#btn-vendor-cancel').on('click', function () {
    safelyNavigateTo('/Vendors');
  });

  $('#storeItemSelect').on('change', function () {
    $('#regionItemSelect').prop('selectedIndex', 0);
  });

  $('#regionItemSelect').on('change', function () {
    $('#storeItemSelect').prop('selectedIndex', 0);
  });

  $('#btn-delete').on('click', async function () {
    if (!selectedVendorCode) {
      toastr.warning($("#pleaseSelectAVendor").val());
      return;
    }

    if (!selectedStoreId && !selectedRegionId) {
      toastr.warning($("#pleaseSelectRegionStore").val());
      return;
    }

    let title = $("#deleteLink").val();
    let message = $("#areYouSureYouWantToDeleteTheVendorLink").val();

    showConfirmModal(title, message).then(async (result) => {
      if (result === true) {

        let url = URL_GET_VENDORS + `/DeleteVendorLink`;
        await ajaxPostAsync(url, {
          VendorId: selectedVendorCode,
          RegionId: selectedRegionId,
          StoreId: selectedStoreId
        })
          .then(_response => {
            if (_response) {
              storeRegions = storeRegions.filter(function (item) {
                return item.vendorId == selectedVendorCode && !(item.storeId == selectedStoreId && item.regionId == selectedRegionId);
              });

          let option;
          if (selectedStoreId != "") {
            toastr.success($('#storeDeleted').val());
            option = $(`#storeItemSelect option[value='${selectedStoreId}']`);
          }
          else {
            toastr.success($('#regionDeleted').val());
            option = $(`#regionItemSelect option[value='${selectedRegionId}']`);
          }

              createStoreRegionsDatatable();

              if (storeRegions.length == 0) {
                $('#btn-delete').addClass('disabled');
              }

              option.show();
              option.next().show();

              selectedRegionId = "";
              selectedStoreId = "";
            }
            else {
              toastr.error($("#anErrorHasOccuredWhileDeletingVendorRegionStores").val());
            }
          })
          .catch(() => toastr.error($("#anErrorHasOccuredWhileDeletingVendorRegionStores").val()));
      }
    });
  });
}

let refreshStoreRegionData = () => {
  storeRegions = storeRegions.sort(function (a, b) {
    if (a.regionId == b.regionId) {
      return a.storeId > b.storeId ? 1 : -1;
    }
    return a.regionId > b.regionId ? 1 : -1;
  });
}

let checkAndHideOption = () => {
  let stores = storeRegions.map(function (item) {
    return item.storeId;
  }); 
  let storesDropDown = $('#storeItemSelect option.value-container').toArray();

  let regions = storeRegions.map(function (item) {
    return item.regionId;
  });
  let regionsDropDown = $('#regionItemSelect option.value-container').toArray();
  storesDropDown.forEach(function (item) {
    if (stores.includes($(item).val())) {
      $(item).hide();
      $(item).next().hide();
    }
    else {
      $(item).show();
      $(item).next().show();
    }
  });

  regionsDropDown.forEach(function (item) {
    if (regions.includes($(item).val())) {
      $(item).hide();
      $(item).next().hide();
    }
    else {
      $(item).show();
      $(item).next().show();
    }
  });
}

let insertStoreRegionItem = async () => {
  if ($('#storeItemSelect').prop('selectedIndex') == 0 && $('#regionItemSelect').prop('selectedIndex') == 0) {
    toastr.warning($("#pleaseSelectRegionStore").val());
    return;
  }

  $("#insertStoreRegionBtn").prop('disabled', true);

  let url = URL_GET_VENDORS + `/AddVendorLink`;
  await ajaxPostAsync(url, {
    VendorId: selectedVendorCode,
    RegionId: $('#regionItemSelect').val(),
    StoreId: $('#storeItemSelect').val()
  })
  .then(_response => {
    if (_response) {
      let option;
      if ($('#storeItemSelect').val() != "") {
        toastr.success($('#storeAdded').val());
        option = $(`#storeItemSelect option[value='${$('#storeItemSelect').val()}']`);
      }
      else {
        toastr.success($('#regionAdded').val());
        option = $(`#regionItemSelect option[value='${$('#regionItemSelect').val()}']`);
      }
      option.hide();
      option.next().hide();

      $('#addNewStoreRegionItemModal').modal('hide');
      storeRegions.push({
        vendorId: selectedVendorCode,
        regionId: $('#regionItemSelect').val(),
        storeId: $('#storeItemSelect').val()
      });

      $('#btn-delete').removeClass('disabled');
      refreshStoreRegionData();
      createStoreRegionsDatatable();
    }
    else {
      toastr.error($("#anErrorHasOccuredWhileAddingVendorRegionStores").val());
    }
  })
  .catch(() => toastr.error($("#anErrorHasOccuredWhileAddingVendorRegionStores").val()));
}

let createVendorsDatatable = async () => {
  destroyDataTableIfExists(vendorsDatatable);

  let props = createDataTableProp();
  props.data = vendors;
  props.scrollY = '440px';
  props.paging = true;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "code", },
    { "data": "name", }
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  vendorsDatatable = createDataTableGeneric('vendors-grid', props);

  $('#vendors-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (vendorsDatatable.data().length === 0) {
        return;
      }
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
      } else {
        vendorsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
      selectedVendorCode = $(this).find("td:first")[0].innerText;
      await loadStoreRegion(selectedVendorCode);
    });
}

let createStoreRegionsDatatable = () => {
  destroyDataTableIfExists(storeRegionDatatable);

  let props = createDataTableProp();
  props.data = storeRegions;
  props.scrollY = '440px';
  props.paging = true;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "vendorId", },
    { "data": "regionId", },
    { "data": "storeId", }
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  storeRegionDatatable = createDataTableGeneric('vendors-region-store-grid', props);

  $('#vendors-region-store-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if (storeRegionDatatable.data().length === 0) {
        return;
      }
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
      } else {
        storeRegionDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
      selectedRegionId = $(this).find("td")[1].innerText;
      selectedStoreId = $(this).find("td")[2].innerText;
    });
  selectedRegionId = "";
  selectedStoreId = "";
}

let loadStoreRegion = async (code) => {
  let url = URL_GET_VENDORS + `/GetVendorLink?vendorId=${code}`;
  await ajaxGetAsync(url)
    .then(_response => {
      storeRegions = _response;
      createStoreRegionsDatatable();
      checkAndHideOption();
      if (storeRegions && storeRegions.length > 0) {
        $('#btn-delete').removeClass('disabled');
      }
      else {
        $('#btn-delete').addClass('disabled');
      }
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingVendorRegionStores").val()));
}

let loadVendors = async () => {
  let url = URL_GET_VENDORS + "/GetAllVendorName";
  await ajaxGetAsync(url)
    .then(_response => {
      vendors = _response;
      _response.forEach(function (vendor) {
        if (vendor.Code == 0) {
          vendor.Code = null;
        }
      });
      createVendorsDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingVendors").val()));
}
