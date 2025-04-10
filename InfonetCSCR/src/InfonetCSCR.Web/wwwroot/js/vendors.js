let vendorsDatatable;
let vendors;
let selectedVendorCode;
let policies;
let vendorDeletionModal;
const URL_GET_VENDORS = "/Vendors";

$(async function () {
  await initVendorPage();
});

let initVendorPage = async () => {
  await loadPolicies();
  await loadVendors();
  initButtonActions();
}

let initButtonActions = () => {
  $("#btn-add").on('click', function () {
    const url = `${URL_GET_VENDORS}/VendorForm`;
    safelyNavigateTo(url);
  });

  $("#btn-edit").on('click', function () {
    if (!selectedVendorCode) {
      toastr.warning($("#pleaseSelectAVendor").val());
      return;
    }
    const url = `${URL_GET_VENDORS}/VendorForm?vendorId=${selectedVendorCode}`;
    safelyNavigateTo(url);
  });

  $("#btn-delete").on('click', async function () {
    if (!selectedVendorCode) {
      toastr.warning($("#pleaseSelectAVendor").val());
      return;
    }
    const url = `${URL_GET_VENDORS}/CheckDeleteValidation?vendorId=${selectedVendorCode}`;
    await ajaxGetAsync(url)
      .then(_response => {
        if (_response.isConfirmationAlert) {
          vendorDeletionModal = $('#deleteVendorConfirmModal');
        }
        else {
          vendorDeletionModal = $('#deleteVendorInfoModal');
        }

        vendorDeletionModal.find('.message').html(_response.message);
        vendorDeletionModal.modal('show');
      })
      .catch(() => toastr.error($("#anErrorHasOccuredWhileDeletingVendor").val()));
  });

  $('#deleteVendorConfirmModalYesButtonControl').on('click', async function () {
    const url = `${URL_GET_VENDORS}/DeleteVendor?vendorId=${selectedVendorCode}`;
    await ajaxPostAsync(url)
      .then(_response => {
        if (_response) {
          vendorsDatatable.row('.selected-grid-row').remove().draw(false);
          selectedVendorCode = null;
          toastr.success($('#vendorDeleted').val());
        }
        else {
          toastr.error($("#anErrorHasOccuredWhileDeletingVendor").val());
        }
        vendorDeletionModal.modal('hide');

        if (vendorsDatatable.data().length == 0) {
          $('#btn-delete, #btn-edit').prop('disabled', true);
        }
      })
      .catch(() => toastr.error($("#anErrorHasOccuredWhileDeletingVendor").val()));
  });

  $("#btn-link").on('click', function () {
    const url = `${URL_GET_VENDORS}/VendorLink`;
    safelyNavigateTo(url);
  });
}

let loadPolicies = async () => {
  let url = URL_GET_VENDORS + "/GetVendorsPolicies";
  await ajaxGetAsync(url)
    .then(_response => {
      policies = _response;
    })
    .catch(toastrError);
}

let createVendorsDatatable = async () => {
  destroyDataTableIfExists(vendorsDatatable);

  let props = createDataTableProp();
  props.data = vendors;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = true;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "code", },
    { "data": "name", },
    { "data": "address", },
    { "data": "city", },
    { "data": "province", },
    { "data": "country", },
    { "data": "postal", },
    { "data": "phone", },
    { "data": "fax", },
    { "data": "email", },
    { "data": "contactName1", },
    { "data": "contactName2", },
    { "data": "taxNumber", },
    { "data": "isTEVendor", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13] },
    { className: 'text-capitalize', visible: policies.isTaxExempted, targets: [13] },
    { className: 'contact-name-col-width', targets: [10, 11] },
    { className: 'name-col-width', targets: [1] },
    { className: 'address-col-width', targets: [2] },
    { className: 'city-col-width', targets: [3] },
    { className: 'province-col-width', targets: [4] },
    { className: 'country-col-width', targets: [5] },
    { className: 'postal-code-col-width', targets: [6] },
    { className: 'phone-col-width', targets: [7] },
    { className: 'fax-col-width', targets: [8] },
    { className: 'email-col-width', targets: [9] },
    { className: 'tax-number', targets: [12] },
  ];

  vendorsDatatable = createDatatableWithSearching('vendors-grid', props);

  $('#vendors-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if (vendorsDatatable.data().length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
      } else {
        vendorsDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
      selectedVendorCode = $(this).find("td:first")[0].innerText;
      
    })
    .on('dblclick', 'tr', function () {
      if (vendorsDatatable.data().length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      let vendorCode = $(this).find("td:first")[0].innerText;
      const url = `/Vendors/VendorForm?vendorId=${vendorCode}&showOnly=true`;
      safelyNavigateTo(url);
    });
}

let loadVendors = async () => {
  let url = URL_GET_VENDORS + "/GetVendors";
  await ajaxGetAsync(url)
    .then(_response => {
      vendors = _response;
      _response.forEach(function (vendor) {
        if (vendor.Code == 0) {
          vendor.Code = null;
        }
      });
      if (vendors.length == 0) {
        $('#btn-delete, #btn-edit, #btn-link').prop('disabled', true);
      }
      createVendorsDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingVendors").val()));
}
