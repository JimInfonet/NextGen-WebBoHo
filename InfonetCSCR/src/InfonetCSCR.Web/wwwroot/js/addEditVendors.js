let policies;
let sortingDatatable;
let addedSortItems = [];
let removedSortItems = [];
const URL_GET_VENDORS = "/Vendors";

$(async function () {
  await initVendorPage();
});

let initVendorPage = async () => {
  await loadPolicies();
  initButtonActions();
  initToastMessages();
  initJsonData();
  createSortTables();
  disableFieldsIfNeeded();
  policyBasedFieldSetup();

  if (!$('#showOnly').val()) {
    _globalIsEditing = true;
  }
}

let toogleDeleteSortButtonDisablity = () => {
  if (addedSortItems && addedSortItems.length > 0) {
    $('#btn-sort-delete').removeClass('disabled');
  }
  else {
    $('#btn-sort-delete').addClass('disabled');
  }
}

let policyBasedFieldSetup = () => {
  if (!policies.isTaxExempted) {
    $('#TEVendor').prop('checked', false);
    $('.te-vendor-container').hide();
  }

  if (!policies.isPurchase) {
    $('#SortItemContainer').hide();
  }
}

let disableFieldsIfNeeded = () => {
  if ($('#showOnly').val()) {
    let inputFields = $('input');
    inputFields.prop('disabled', true);
    $('#SortItemContainer button').hide();
    $('#btn-complete').hide();
    $('#btn-enable-edit').show();
  }
  else {
    $('#btn-enable-edit').hide();
  }
}

let initToastMessages = () => {
  if ($('#actionType').val() == '1') {
    toastr.success($('#vendorAdded').val());
    return;
  }
  else if ($('#actionType').val() == '2') {
    toastr.success($('#vendorUpdated').val());
    return;
  }

  if ($('#isCodeAlreadyExist').val()) {
    toastr.warning($('#isCodeAlreadyExist').val());
    return;
  }

  if ($('#isErrorOccured').val()) {
    if ($('#isAdd').val()) {
      toastr.error($('#anErrorOccuredWhileAddingVendor').val());
    }
    else {
      toastr.error($('#anErrorOccuredWhileUpdatingVendor').val());
    }
  }
}

let createSortTables = () => {
  destroyDataTableIfExists(sortingDatatable);

  let props = createDataTableProp();
  props.data = addedSortItems;
  props.scrollY = '440px';
  props.paging = false;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "SortItem", },
    { "data": "OrderNo", },
    { "data": "Ascending", }
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] },
    { className: 'order-no', targets: [1] },
    { className: 'sort-order', targets: [2] },
  ];

  props.rowCallback = function (row, data, index) {
    $(row).find('.order-no').text(index + 1);
    $(row).find('.sort-order').text(data.Ascending? $('#ascending').val() : $('#descending').val());
  }

  sortingDatatable = createDataTableGeneric('vendor-sort-grid', props);

  $('#vendor-sort-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if (sortingDatatable.data().length === 0) {
        return;
      }
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
      } else {
        sortingDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
    });
}

let toggleSortItemOptionVisiblity = (sortItem) => {
  let option = $('#sortItemSelect option[value=\"' + sortItem + '\"]');
  option.next().toggleClass('d-none');
  option.toggleClass('d-none');
}

let deleteSortItem = () => {
  let sortItem = sortingDatatable.row('.selected-grid-row').data()?.SortItem;
  if (!sortItem) {
    toastr.warning($("#pleaseSelectASortItem").val());
    return;
  }
  let index = addedSortItems.findIndex(function (item) {
    return item.SortItem === sortItem;
  });
  if (index !== -1) {
    let orderNo = addedSortItems[index].OrderNo;
    if (orderNo && orderNo > 0) {
      removedSortItems.push(addedSortItems[index]);
      $('#RemovedSortItems').val(JSON.stringify(removedSortItems));
    }
    addedSortItems.splice(index, 1);
    toggleSortItemOptionVisiblity(sortItem);
    sortingDatatable.row('.selected-grid-row').remove().draw(false);
  }
  $('#AddedSortItems').val(JSON.stringify(addedSortItems));
  toogleDeleteSortButtonDisablity();
}

let insertSortItem = () => {
  let sortItem = $('#sortItemSelect').val();
  let sortOrder = $('#sortOrderSelect').val();

  if (sortItem == '-1') {
    toastr.warning($("#pleaseSelectASortItem").val());
    return;
  }

  if (sortOrder == '-1') {
    toastr.warning($("#pleaseSelectASortOrder").val());
    return;
  }

  addedSortItems.push({
    VendorId: "",
    OrderNo: -1,
    SortItem: sortItem,
    Ascending: sortOrder == "0"
  });

  sortingDatatable.row.add({
    SortItem: sortItem,
    OrderNo: -1,
    VendorId: "",
    Ascending: sortOrder == "0"
  }).draw(false);

  toggleSortItemOptionVisiblity(sortItem);
  $('#AddedSortItems').val(JSON.stringify(addedSortItems));
  $("#addNewSortItemModal").modal("hide");
  toogleDeleteSortButtonDisablity();
}

let initButtonActions = () => {
  // On Add button click
  $("#btn-sort-add").on('click', function () {
    $('#sortItemSelect').prop('selectedIndex', 0);
    $('#sortOrderSelect').prop('selectedIndex', 0);
    $("#addNewSortItemModal").modal("show");
  });

  $('#btn-cancel').on('click', function () {
    safelyNavigateTo('/Vendors');
  });

  $('#btn-enable-edit').on('click', function () {
    let inputFields = $('input:not(#Code)');
    inputFields.removeAttr('disabled');

    $('#SortItemContainer button').show();
    $('#btn-complete').show();
    $('#btn-enable-edit').hide();
    _globalIsEditing = true;
  });
}

let initJsonData = () => {
  addedSortItems = JSON.parse($('#AddedSortItems').val());
  removedSortItems = JSON.parse($('#RemovedSortItems').val());

  addedSortItems.forEach(function (aitem) {
    toggleSortItemOptionVisiblity(aitem.SortItem);
  });
  toogleDeleteSortButtonDisablity();
}

let loadPolicies = async () => {
  let url = URL_GET_VENDORS + "/GetVendorsPolicies";
  await ajaxGetAsync(url)
    .then(_response => {
      policies = _response;
    })
    .catch(toastrError);
}
