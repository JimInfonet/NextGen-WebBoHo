/*:::::::::::::::: Global ::::::::::::::::*/

let promotions = [];
let selectedPromotionIds = [];
let dataTablePromotion = null;
let dataTablePromotionTransfer = null;
let promotionTransfers = [];

/*:::::::::::::::: General Section Start ::::::::::::::::*/

async function loadPromotions() {
  const url = STRING_TABLE.URL.GET.PROMOTIONS;

  await ajaxGetAsync(url)
    .then((_promotions) => promotions = _promotions)
    .catch(toastrError);
}

async function loadPromotionTransfer(storeId = null) {
  let url = STRING_TABLE.URL.GET.PROMOTION_TRANSFERS;
  if (storeId !== null) url += `?storeId=${storeId}`;

  await ajaxGetAsync(url)
    .then((_promotionTransfers) => promotionTransfers = _promotionTransfers)
    .catch(toastrError);
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: Selection Tab Start ::::::::::::::::*/

function renderPromotions() {

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.PROMO_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.START_DATE,
      render: function (startDate) {
        const dateString = getFormattedDate(new Date(startDate));
        return dateString;
      }
    },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.END_DATE,
      render: function (endDate) {
        const dateString = getFormattedDate(new Date(endDate));
        return dateString;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (promotion) {
        return `<select class="form-select form-select-sm trans-type" id="promotion--${promotion.promoId}__trans-type">
                  <option value="A" selected>${$('#add').val()}</option>
                  <option value="U">${$('#update').val()}</option>
                  <option value="R">${$('#remove').val()}</option>
                </select>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (promotion) {
        return `<input class="form-check-input mt-2 promotion__sel" 
                       type="checkbox" value="${promotion.promoId}" id="promotion--${promotion.promoId}__sel"
                       onChange="handlePromotionSelectionChange('${promotion.promoId}')" />`;
      }
    }
  ];
  props.data = promotions;

  if (dataTablePromotion != null) {
    dataTablePromotion.state.clear();
    dataTablePromotion.clear();
    dataTablePromotion.destroy();
  }

  dataTablePromotion = createDatatable("table--promotion", props);
}

$(document).on('draw.dt', '#table--promotion', function () {
  const isChecked = $("#select-or-unselect-all--promotion").prop("checked");

  if (isChecked) {
    $(".promotion__sel").prop("checked", isChecked);
  }
  else {
    $(".promotion__sel").prop("checked", false);
    selectedPromotionIds.forEach(promoId => $(`#promotion--${promoId}__sel`).prop("checked", true));
  }
});

function handlePromotionSelectionChange(promoId) {
  const isChecked = $(`#promotion--${promoId}__sel`).prop("checked");

  if (isChecked) {
    selectedPromotionIds.push(promoId);
    const isAllOptionSelected = selectedPromotionIds.length === promotions.length;
    if (isAllOptionSelected) {
      $("#select-or-unselect-all--promotion").prop("checked", true);
    }
  }
  else {
    $("#select-or-unselect-all--promotion").prop("checked", false);
    selectedPromotionIds = selectedPromotionIds.filter(_promoId => _promoId != promoId);
  }
}

function handleSelectOrUnselectAllPromotion() {
  const isChecked = $("#select-or-unselect-all--promotion").prop("checked");

  if (isChecked) {
    $(".promotion__sel").prop("checked", true);
    selectedPromotionIds = promotions.map(promotion => promotion.promoId);
  }
  else {
    $(".promotion__sel").prop("checked", false);
    selectedPromotionIds = [];
  }
}

async function handlePromotionSaveAsPendingTransfer() {
  if (selectedStoresOnSelectionTab.size == 0) {
    const message = $("#selectAtLeastOneStore").val();
    toastr.warning(message);
    return;
  }

  if (selectedPromotionIds.length === 0) {
    const message = $("#selectAtLeastOneItem").val();
    toastr.warning(message);
    return;
  }

  const savingText = $("#saving").val();
  $("#save-as-pending-transfer-btn--selection")
    .html(`<span class="spinner-border spinner-border-sm" role="status"></span> ${savingText}...`);

  const promotionTransferItems = selectedPromotionIds.map(promoId => {
    const promotion = promotions.find(promotion => promotion.promoId === promoId);

    $('#table--promotion').DataTable().rows().every(function () {
      let selectElement = $(this.node()).find(`#promotion--${promoId}__trans-type`);
      if (selectElement.length > 0) {
        promotion.transType = selectElement.val();
      }
    });

    return promotion;
  });

  let storeIds = [];
  selectedStoresOnSelectionTab.forEach(store => storeIds.push(store.storeID));

  const pendingPromotionTransfers = {
    promotionTransferItems: promotionTransferItems,
    storeIds: storeIds,
  };

  let error = null;
  await ajaxPostAsync(STRING_TABLE.URL.INSERT.PENDING_PROMOTION_TRANSFERS, pendingPromotionTransfers).catch(err => error = err);

  const saveAsPendingTransferText = $("#saveAsPendingTransfer").val();
  $("#save-as-pending-transfer-btn--selection").html(saveAsPendingTransferText);
  const message = $("#promotionPendingTransferSuccessful").val();
  error ? toastr.error(error) : toastr.success(message);

  selectedPromotionIds = [];
  $(".promotion__sel").prop("checked", false);
  $("#select-or-unselect-all--promotion").prop("checked", false);
}

/*:::::::::::::::: Selection Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

async function renderPromotionListTab(storeId = null) {
  await loadPromotionTransfer(storeId).then(() => {
    renderPromotionTransfersTable();
    resizeTransferStoreContainer();
  });
}

function renderPromotionTransfersTable() {
  if (dataTablePromotionTransfer) {
    dataTablePromotionTransfer.state.clear();
    dataTablePromotionTransfer.clear();
    dataTablePromotionTransfer.destroy();
  }

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.BATCH_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.PROMO_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.START_DATE,
      render: function (startDate) {
        const dateString = getFormattedDate(new Date(startDate));
        return dateString;
      }
    },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.END_DATE,
      render: function (endDate) {
        const dateString = getFormattedDate(new Date(endDate));
        return dateString;
      }
    },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.TRANS_TYPE },
    {
      data: null,
      sortable: false,
      render: function (item) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                    onClick="handleDeleteFromTransferList('${item.batchId} ', '${item.promoId}',null, null, 'ITEM')">
                      <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    }
  ];

  props.data = promotionTransfers;

  dataTablePromotionTransfer = createDatatable("table--promotion-transfer", props);
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/

