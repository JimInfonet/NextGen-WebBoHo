/*:::::::::::::::: Global ::::::::::::::::*/

let costChangeTransfers = [];
let datatableCostChangeTransfer = null;
let isCostSelected = false;
let changePriceBasedOnCost = false;

/*:::::::::::::::: General Section Start ::::::::::::::::*/

async function loadCostChangeTransfer(storeId = null) {
  let url = STRING_TABLE.URL.GET.COST_CHANGE_TRANSFERS;
  if (storeId) url = `${url}?storeId=${storeId}`;
  await ajaxGetAsync(url)
    .then((_costChangeTransfers) => costChangeTransfers = _costChangeTransfers)
    .catch(toastrError);
}

function renderElementsForCostChange() {
  renderProducts();
  dataTableProduct?.column(2).visible(true);

  $("#priceSetUp").show();
  $("#priceSetupLabel").text($("#costSetup").val());
  $("#cost-change-table-section").show();
  $("#commodity-table-section").hide();
  $("#price-type-div").hide();
  $("#stockTransferOptions").hide();
  $("#price-type-section").hide();
  $("#change-by-margin").hide();
  $("#change-by-margin-label").hide();

  $("#margin-or-price-wrapper").hide();
  $("#cost-wrapper").show();

  $("#recent-changes-btn").hide();
  $("#price-change-input-table-wrapper").hide();
  $("#cost-change-input-table-wrapper").show();
  $("#save-as-pending-transfer-btn").hide();

  document.getElementById("DTProductsCol1").innerHTML = document.getElementById("productId").value;
  document.getElementById("DTProductsCol2").innerHTML = document.getElementById("productName").value;
  document.getElementById("DTProductsCol3").innerHTML = document.getElementById("cost").value;
  document.getElementById("DTProductsCol4").innerHTML = document.getElementById("nCost").value;
  document.getElementById("DTProductsCol5").innerHTML = document.getElementById("regPrice").value;
  document.getElementById("DTProductsCol6").innerHTML = document.getElementById("margin-p").value;
  document.getElementById("DTProductsCol7").innerHTML = document.getElementById("sel").value;
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: View Tab Start ::::::::::::::::*/

function calculatePriceAndMarginBasedOnCost(stockCode, priceNumber) {
  const newCost = parseDoubleFromUI(`#product-${stockCode}__n-cost`) ?? 0;
  const price = parseDoubleFromUI(`#product-${stockCode}__reg-price`) ?? 0;
  const margin = parseDoubleFromUI(`#product-${stockCode}__margin`) ?? 0;
  let newMargin = margin;
  let newPrice = price;
  if (marginOrPrice === STRING_TABLE.PRICE_SETUP.CHANGE_BY_MARGIN) {
    newMargin = calculateMarginBasedOnPolicy(newCost, price);
  }
  else {
    newPrice = calculatePriceBasedOnPolicy(newCost, margin);
  }

  updateComparisonTableColumn(priceNumber, margin, price, newMargin, newPrice)
}

// Triggers when cost button is clicked
$("#change-by-cost").on('click', async function () {
  $('#currency-dollar, #currency-percentage').prop('checked', false);
  dollarOrPercentage = null;
  $('#increase, #decrease').prop('checked', false);
  $('#increase, #decrease').prop('disabled', true);
  increaseOrDecrease = null;
  $("#currency-dollar").prop("disabled", false);
  $("#currency-percentage").prop("disabled", false);

  if (!isCostSelected) {
    $('#increase, #decrease').prop('disabled', true);
    $("#change-by-cost-label").addClass("selected");
    $("#change-by-cost-label").css("background-color", "#0d6efd");
    $("#change-by-cost-label").css("color", "white");
    isCostSelected = true;
  }
});

async function updateNewCostOnDatatable(stockCode, packCount = 1, oldCost, increase = false, selectedOption, encodedStockCodeAndRowNumber) {
  const inputValue = regularPrices[0].price;
  let newCost;
  if (selectedOption === STRING_TABLE.PRICE_SETUP.CURRENCY_DOLLAR) {
    newCost = (oldCost / packCount) + (increase ? (inputValue / packCount) : -(inputValue / packCount));
  }
  else if (selectedOption === STRING_TABLE.PRICE_SETUP.CURRENCY_PERCENTAGE) {
    newCost = (oldCost / packCount) + (increase ? ((oldCost * inputValue / 100) / packCount) : -((oldCost * inputValue / 100) / packCount ));
  }
  else {
    // cost selected
    newCost = inputValue / packCount;
  }

  if (newCost !== oldCost) {
    $(`#product-${encodedStockCodeAndRowNumber}__n-cost`).text(newCost.toFixed(policies.costDec));
  }
}

/*:::::::::::::::: View Tab End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

$(document).on('draw.dt', '#table--price-change-transfers', function () {
  resizeTransferStoreContainer();
});

async function randerCostChangeListTab(storeId = null) {
  await loadCostChangeTransfer(storeId).then(() => {
    renderCostChangeTransfersTable();
    resizeTransferStoreContainer();
  });
}

function renderCostChangeTransfersTable() {
  if (datatableCostChangeTransfer) {
    datatableCostChangeTransfer.state.clear();
    datatableCostChangeTransfer.clear();
    datatableCostChangeTransfer.destroy();
  }

  let props = createDataTableProp();
  props.columns = [
    { data: 'batchId' },
    { data: 'product' },
    { data: 'description' },
    {
      data: null,
      sortable: false,
      render: function (costChangeTransfers) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                  onClick="handleDeleteFromTransferList('${costChangeTransfers.batchId} ', '${costChangeTransfers.product}', '${costChangeTransfers.description}', null, 'ITEM')">
                    <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    }
  ];

  props.data = costChangeTransfers;

  datatableCostChangeTransfer = createDatatable("table-cost-change-transfer", props);
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/
