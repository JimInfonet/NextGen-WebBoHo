/*:::::::::::::::: Global ::::::::::::::::*/

let commodityTransfers = [];
let commodityTransTypes = {};
let datatableCommodityTransfer = null;
let isCommodityTypeNow = false;

/*:::::::::::::::: General Section Start ::::::::::::::::*/

async function loadCommodityTransfer(storeId = null) {
  let url = STRING_TABLE.URL.GET.COMMODITY_TRANSFERS;
  if (storeId !== null) url += `?storeId=${storeId}`;

  await ajaxGetAsync(url)
    .then((_commodityTransfers) => commodityTransfers = _commodityTransfers)
    .catch(toastrError);
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: Selection Tab Start ::::::::::::::::*/

async function handleCommoditySaveAsPendingTransfer() {
  if (selectedStoresOnSelectionTab.size == 0) {
    const message = $("#selectAtLeastOneStore").val();
    toastr.warning(message);
    return;
  }

  const savingText = $("#saving").val();
  $("#save-as-pending-transfer-btn--selection")
    .html(`<span class="spinner-border spinner-border-sm" role="status"></span> ${savingText}...`);

  let storeIds = [];
  selectedStoresOnSelectionTab.forEach(store => storeIds.push(store.storeID));
  const pendingTransfers = {
    storeIds: storeIds,
    departments: selectedDepartments,
    subDepartments: selectedSubDepartments,
    subDetails: selectedSubDetails,
    transTypes: commodityTransTypes
  };

  let error = null;
  await ajaxPostAsync(STRING_TABLE.URL.INSERT.PENDING_COMMODITY_TRANSFERS, pendingTransfers).catch(err => error = err);

  const saveAsPendingTransferText = $("#saveAsPendingTransfer").val();
  $("#save-as-pending-transfer-btn--selection").html(saveAsPendingTransferText);
  const message = $("#commodityTransferSuccessful").val();
  error ? toastr.error(error) : toastr.success(message);
  selectedDepartments = [];
  selectedSubDepartments = [];
  selectedSubDetails = [];
  renderDepartments();
  renderSubDepartments();
  renderSubDetails();
  resetDataAndTables();
  initializeTransTypeValues();
}

/*:::::::::::::::: Selection Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

async function renderCommodityListTab(storeId = null) {
  await loadCommodityTransfer(storeId).then(() => {
    renderCommodityTransfersTable();
    resizeTransferStoreContainer();
  });
}

function renderCommodityTransfersTable() {
  if (datatableCommodityTransfer) {
    datatableCommodityTransfer.state.clear();
    datatableCommodityTransfer.clear();
    datatableCommodityTransfer.destroy();
  }

  let props = createDataTableProp();
  props.columns = [
    { data: 'batchId' },
    { data: 'dept' },
    { data: 'subDept' },
    { data: 'subDetail' },
    { data: 'description' },
    { data: 'transType' },
    {
      data: null,
      sortable: false,
      render: function (commodityTransfers) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                    onClick="handleDeleteFromTransferList('${commodityTransfers.batchId} ', '${commodityTransfers.dept}', '${commodityTransfers.description}', null, 'ITEM')">
                    <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    }
  ];
  props.data = commodityTransfers;

  datatableCommodityTransfer = createDatatable("table-commodity-transfer", props);
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/
