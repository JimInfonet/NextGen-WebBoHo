/*:::::::::::::::: Global ::::::::::::::::*/

let recentChanges = false;
let dataTableStockTransfer = null;
let localizedTextOfAdd;
let localizedTextOfRemove;
let localizedTextOfUpdate;

/*:::::::::::::::: General Section Start ::::::::::::::::*/

$(async function () {
  localizedTextOfAdd = document.getElementById("add").value;
  localizedTextOfRemove = document.getElementById("remove").value;
  localizedTextOfUpdate = document.getElementById("update").value;
});

function goToDataDefinition() {
  localStorage.setItem('vendorId', vendorId);
  const url = "/DataDefinition";
  safelyNavigateTo(url);
}

async function renderElementsForStockChange() {
  await renderProducts();
  dataTableProduct?.column(2).visible(false);

  $("#priceSetUp").hide();
  $("#stockTransferOptions").show();
  $("#recent-changes-btn").show();
  $("#cost-change-input-table-wrapper").hide();
  $("#cost-wrapper").hide();
  $("#save-as-pending-transfer-btn").show();

  document.getElementById("DTProductsCol1").innerHTML = document.getElementById("stockCode").value;
  document.getElementById("DTProductsCol2").innerHTML = document.getElementById("name").value;
  document.getElementById("DTProductsCol3").innerHTML = document.getElementById("dept").value;
  document.getElementById("DTProductsCol4").innerHTML = document.getElementById("subDept").value;
  document.getElementById("DTProductsCol5").innerHTML = document.getElementById("subDetail").value;
  document.getElementById("DTProductsCol6").innerHTML = document.getElementById("trType").value;
  document.getElementById("DTProductsCol7").innerHTML = document.getElementById("sel").value;
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: View Tab Start ::::::::::::::::*/

function getColumnsOfProductTableForStockChange() {
  let columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.STOCK_CODE },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    {
      data: null,
      visible: false,
    },
    {
      data: "dept"
    },
    {
      data: "subDept"
    },
    {
      data: "subDetail"
    },
    {
      data: null,
      render: function () {
        return `<select class="form-select form-select-sm trans-type col infonet-input edit-mode-control" onchange="changedTrType(this.parentElement.parentElement, this.options[this.selectedIndex].value)">
              <option value="A">${localizedTextOfAdd}</option>
              <option value="U">${localizedTextOfUpdate}</option>
              <option value="R">${localizedTextOfRemove}</option>
            </select>`;
      }
    },
    {
      data: null,
      sortable: false,
      render: function (product, _, _, meta) {
        return `<input class="form-check-input mt-2 product" id="product-${encodeStockCodeWithRowNumber(product.stockCode, meta.row)}__sel"
                  onChange="handleProductSelectionChange('${product.stockCode}', this.parentElement.parentElement)" 
                  type="checkbox" value="${product.stockCode}" />`;
      }
    }
  ];

  return columns;
}

function loadRecentChangesFromLocalStorage() {
  let recentChangesFromLocalStorage = localStorage.getItem('recentChanges');
  recentChanges = JSON.parse(recentChangesFromLocalStorage)
  if (!recentChanges) {
    recentChanges = false;
  }
}

function handleShowRecentChanges() {
  $("#show-all-products-btn").prop("disabled", false);
  $("#recent-changes-btn").prop("disabled", true);
  selectedProductsOnViewTab = [];
  recentChanges = true;
  localStorage.setItem('recentChanges', JSON.stringify(recentChanges));
  dataTableProduct.draw();
}

/*:::::::::::::::: View Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

$(document).on('draw.dt', '#table--stock-transfer', function () {
  resizeTransferStoreContainer();
});

function renderStockTransfersTable(storeId = null) {
  destroyDataTableIfExists(dataTableStockTransfer);

  let props = createDataTableProp();
  props.columns = [
    { data: "batchID" },
    { data: "productID" },
    { data: "description" },
    { data: "transType" },
    {
      data: null,
      sortable: false,
      render: function (item) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                    onClick="handleDeleteFromTransferList('${item.batchID} ', '${item.productID}',null, null, 'ITEM')">
                      <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    },
  ];
  props.serverSide = true;
  props.ajax = {
    url: "/PricebookTransfer/GetStockTransfersByStoreId",
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    dataSrc: function (d) {
      return d.data;
    },
    data: function (d) {
      d.storeIds = (storeId === null) ? null : [storeId];
      d.regionIds = null;
      d.priceGroupIds = null;
      d.vendorIds = null;
      d.isAllProductSelected = false;

      return JSON.stringify(d);
    }
  };

  dataTableStockTransfer = createDatatable("table--stock-transfer", props);
}

function renderSelectedProductsForStockChangeOnLoad() {
  let selectedProductsOnCurrentPage;
  if (isSelectAllProductsModeActivated) {
    selectedProductsOnCurrentPage = products;
  }
  else {
    selectedProductsOnCurrentPage = selectedProductsOnViewTab.filter(prod => products.find(p => p.stockCode === prod.stockCode));
  }

  selectedProductsOnCurrentPage.map((prod) => {
    let currentProduct = products.find(p => p.stockCode === prod.stockCode);
    let rowId = currentProduct.rowId;
    let encodedStockCodeAndRowNumber = encodeStockCodeWithRowNumber(prod.stockCode, rowId);
    $(`#product-${encodedStockCodeAndRowNumber}__sel`).prop("checked", true);

    prod.transferType = selectedTransferOption;

    const alreadyExist = selectedProductsOnViewTab.find(p => p.stockCode === prod.stockCode);
    if (!alreadyExist) {
      selectedProductsOnViewTab.push(products.find(p => p.stockCode === prod.stockCode));
    }
    deselectedProductsOnViewTab = deselectedProductsOnViewTab.filter(p => p.stockCode !== prod.stockCode);
  });

  localStorage.setItem('selectedProductsOnViewTab', JSON.stringify(selectedProductsOnViewTab))
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/
