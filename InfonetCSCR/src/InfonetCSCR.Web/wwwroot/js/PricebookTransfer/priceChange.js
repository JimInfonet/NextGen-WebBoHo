/*:::::::::::::::: Global ::::::::::::::::*/

let dataTablePriceChangeTransfer = null;
let priceChangeTransfers = [];

/*:::::::::::::::: General Section Start ::::::::::::::::*/

async function loadPriceChangeTransfers(storeId = null) {
  let url = STRING_TABLE.URL.GET.PRICE_CHANGE_TRANSFERS;
  if (storeId !== null) url = `${url}?storeId=${storeId}`

  await ajaxGetAsync(url)
    .then((_priceChangeTransfers) => priceChangeTransfers = _priceChangeTransfers)
    .catch(toastrError);
}

function renderElementsForPriceChange() {
  renderPricesTable();
  renderProducts();
  dataTableProduct?.column(2).visible(false);

  $("#priceSetupLabel").text($("#priceSetup").val());
  $("#price-change-input-table-wrapper").show();
  $("#price-type-section").show();
  $("#margin-or-price-wrapper").show();
  $("#price-type-div").show();
  $("#priceSetUp").show();
  $("#stockTransferOptions").hide();
  $("#cost-change-input-table-wrapper").hide();
  $("#cost-wrapper").hide();
  $("#recent-changes-btn").hide();

  document.getElementById("DTProductsCol1").innerHTML = document.getElementById("productId").value;
  document.getElementById("DTProductsCol2").innerHTML = document.getElementById("productName").value;
  document.getElementById("DTProductsCol3").innerHTML = document.getElementById("cost").value;
  document.getElementById("DTProductsCol4").innerHTML = document.getElementById("nPrice").value;
  document.getElementById("DTProductsCol5").innerHTML = document.getElementById("regPrice").value;
  document.getElementById("DTProductsCol6").innerHTML = document.getElementById("margin-p").value;
  document.getElementById("DTProductsCol7").innerHTML = document.getElementById("sel").value;
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: View Tab Start ::::::::::::::::*/

function getColumnsOfProductTableForPriceChange() {
  let columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.STOCK_CODE },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    {
      data: null,
      visible: false,
      render: function (product, _, _, meta) {
        let packCost = (product.packCost ?? 0).toFixed(policies.costDec);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__packCost" style="display: none;">${packCost}</span>`;
      }
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        let cost = (product.cost ?? 0).toFixed(policies.costDec);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__cost" style="display: none;">${cost}</span>`;
      }
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        return `<span class="product__n-price" 
                      id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__n-price" 
                      data-stock-code="${product.stockCode}"
                      style="display: none;">
                </span>`;
      },
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        let price = (product.price ?? 0).toFixed(2);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__reg-price" style="display: none;">${price}</span>`;
      }
    },
    {
      data: null,
      render: function (product, _, _, meta) {
        let margin = (product.margin ?? 0).toFixed(2);
        return `<span id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__margin" style="display: none;">${margin}</span>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (product, _, _, meta) {
        return `<input class="form-check-input mt-2 product" id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__sel"
                  onChange="handleProductSelectionChange('${product.stockCode}',  this.parentElement.parentElement)"
                  type="checkbox" value="${product.stockCode}" />`;
      }
    }
  ];

  return columns;
}

/*:::::::::::::::: View Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

async function renderPriceChangeListTab(storeId = null) {
  await loadPriceChangeTransfers(storeId).then(() => {
    renderPriceChangeTransfersTable();
    resizeTransferStoreContainer();
  });
}

function renderPriceChangeTransfersTable() {
  if (dataTablePriceChangeTransfer) {
    dataTablePriceChangeTransfer.state.clear();
    dataTablePriceChangeTransfer.clear();
    dataTablePriceChangeTransfer.destroy();
  }

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.BATCH_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.PRODUCT_ID },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.PRICE_TYPE },
    {
      data: null,
      sortable: false,
      render: function (item) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                    onClick="handleDeleteFromTransferList('${item.batchId} ', '${item.productId}',null, null, 'ITEM')">
                      <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    }
  ];
  props.data = priceChangeTransfers;

  dataTablePriceChangeTransfer = createDatatable("table--price-change-transfer", props);
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/

