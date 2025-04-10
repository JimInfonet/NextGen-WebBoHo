let storesData = [];
let reformedStoresData = [];
let selectedStore;
let selectedStoreRow;
let storesDataTable;
let supportWarehousePolicy;
let view = CONSTANT_TEXTS.VIEW;
let add = CONSTANT_TEXTS.ADD;
let edit = CONSTANT_TEXTS.EDIT;
let copy = CONSTANT_TEXTS.COPY;
const BASE_URL_STORE = "/Store";

$(async function () {
  $('.text-danger').prop('hidden', true);

  if ($("#isStoreAdded").val() === "true") {
    toastr.success($("#storeAddedSuccessfully").val());
  }
  if ($("#isStoreUpdated").val() === "true") {
    toastr.success($("#storeUpdatedSuccessfully").val());
  }
  if ($("#isStoreCopied").val() === "true") {
    toastr.success($("#storeCopiedSuccessfully").val());
  }

  await loadPolicies();
  await loadStoresData();
});

async function loadPolicies() {
  const url = `${BASE_URL_STORE}/GetPolicies`;
  await ajaxGetAsync(url)
    .then(_policies => supportWarehousePolicy = _policies)
    .catch(toastrError);
}

let loadStoresData = async () => {
  let url = `${BASE_URL_STORE}/GetAllStoresList`;

  await ajaxGetAsync(url)
    .then(_storesData => {
      if (_storesData) {
        storesData = _storesData;
        reformedStoresData = JSON.parse(JSON.stringify(storesData));

        reformedStoresData.forEach(store => {
          store.isWarehouse = isNullUndefinedOrWhiteSpace(store.isWarehouse)
            ? ''
            : store.isWarehouse
              ? "True"
              : "False";
        });

        $("#btn-delete").prop('disabled', !storesData.length);
        $("#btn-edit").prop('disabled', !storesData.length);

        if (storesData.length > 0) {
          selectedStore = storesData[0].store_ID;
          selectedStoreRow = storesData[0];
        }
        
        createStoresDataTable();
      }
    })
    .catch(toastrError);
}

let createStoresDataTable = () => {
  destroyDataTableIfExists(storesDataTable);

  let props = createDataTableProp();
  props.data = reformedStoresData;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = true;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "store_ID", },
    { "data": "store_Desc", },
    { "data": "addr1", },
    { "data": "city", },
    { "data": "province", },
    { "data": "post_Code", },
    { "data": "phone", },
    { "data": "fax", },
    { "data": "contact", },
    { "data": "isWarehouse", "visible": (supportWarehousePolicy) },
    { "data": "data_Path", "visible": (supportWarehousePolicy) },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10] }, 
    { className: 'col-width-100px', targets: [0, 4, 5, 9] },
    { className: 'col-width-225px', targets: [6, 7, 3] },
    { className: 'col-width-325px', targets: [1, 2, 8] },
    { className: 'col-width-550px', targets: [10] },
    {
      "targets": 10,
      "render": function (data, type, row, meta) {
        if (!data) return data;
        const maxCharactersPerLine = 20;
        return (data.length > maxCharactersPerLine) ?
          addSeeMoreBtnInGridData(data, type, row, meta, maxCharactersPerLine) : data;
      }
    }
  ];

  storesDataTable = createDatatableWithSearching('stores-grid', props);

  if (storesData.length > 0) {
    $(`#stores-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#stores-grid tbody')
    .off()
    .on('click', 'tr', function () {
      let rowData = storesDataTable.row(this).data();
      if (!rowData) {
        return;
      }

      if (storesData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        storesDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedStore = $(this).find("td:first")[0].innerText;
        selectedStoreRow = storesDataTable.row(this).data();
      }
    })
    .on('dblclick', 'tr', function () {
      let rowData = storesDataTable.row(this).data();
      if (!rowData) {
        return;
      }

      let SelectedStoreId = $(this).find("td:first")[0].innerText;
      const url = `${BASE_URL_STORE}/StoreForm?storeId=${SelectedStoreId}&action=${view}`;
      safelyNavigateTo(url);
    })
}

// Handle click event for "See more" link of the "Loyalty Footer" column
$('#stores-grid').on('click', '.see-more', function () {
  let $cell = $(this).closest('td');
  let fullContent = $(this).data('full-content');
  // Replace "See more" link with "See less" link
  $cell.html(fullContent + ` <span class="see-less" data-full-content="${fullContent}">
                                <a href="#">${$("#seeLess").val()}</a>
                             </span>`);
  customerGroupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
});

// Handle click event for "See less" link of the "Loyalty Footer" column
$('#stores-grid').on('click', '.see-less', function () {
  let $cell = $(this).closest('td');
  let fullContent = $(this).data('full-content');
  let truncatedData = fullContent.substring(0, 20);
  // Replace "See less" link with "See more" link
  $cell.html(truncatedData + ` <span class="see-more" data-full-content="${fullContent}">
                                  <a href="#">${$("#seeMore").val()}</a>
                               </span>`);
  customerGroupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
});

$("#btn-add").on('click', function () {
  const url = `${BASE_URL_STORE}/StoreForm?action=${add}`;
  safelyNavigateTo(url);
});

$("#btn-edit").on('click', function () {
  const url = `${BASE_URL_STORE}/StoreForm?storeId=${selectedStore}&action=${edit}`;
  safelyNavigateTo(url);
});

$("#btn-copy").on('click', function () {
  if (isNullUndefinedOrWhiteSpace(selectedStore.trim())) {
    toastr.warning($("#storeNotSelected").val());
    return;
  }

  let title = $("#copyStoreTitle").val();
  let message = $("#copyStoreMessage").val().replace("{0}", selectedStore);

  showConfirmModal(title, message).then((result) => {
    if (result === true) {
      const url = `${BASE_URL_STORE}/StoreForm?storeId=${selectedStore}&action=${copy}`;
      safelyNavigateTo(url);
    }
  });

});

$("#btn-delete").on('click', async function () {
  let title = $("#deleteStoreTitle").val();
  let message = $("#deleteStoreMessage").val().replace("{0}", selectedStore);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `${BASE_URL_STORE}/DeleteStore?storeId=${selectedStore}`;
      await ajaxGetAsync(url)
        .then(async () => {
          toastr.success($("#storeDeletedSuccessfully").val());
          await loadStoresData();
        })
        .catch(toastrError);
    }
  });
});
