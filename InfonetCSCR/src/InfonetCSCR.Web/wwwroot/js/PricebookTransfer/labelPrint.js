/*:::::::::::::::: Global ::::::::::::::::*/

let dataTableLabelPrint = null;
let labelPrints = [];

/*:::::::::::::::: General Section Start ::::::::::::::::*/

async function loadLabelPrints() {
  let url = STRING_TABLE.URL.GET.LABEL_PRINTS;

  await ajaxGetAsync(url)
    .then((_labelPrints) => labelPrints = _labelPrints)
    .catch(toastrError);
}

/*:::::::::::::::: General Section End ::::::::::::::::*/


/*:::::::::::::::: List Tab Start ::::::::::::::::*/

$(document).on('draw.dt', '#table--label-print', function () {
  resizeTransferStoreContainer();
});

async function renderLabelPrintingListTab() {
  await loadLabelPrints().then(() => {
    renderLabelPrintTable();
  });
}

function renderLabelPrintTable() {
  destroyDataTableIfExists(dataTableLabelPrint)

  let props = createDataTableProp();
  props.columns = [
    { data: STRING_TABLE.TABLE.COLUMN_NAME.STOCK_CODE },
    { data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION },
    {
      data: null,
      sortable: false,
      render: function (item) {
        return `<button class="btn btn-outline-danger d-flex align-items-center p-1" 
                    onClick="handleDeleteFromTransferList(' null', '${item.stockCode}',null, null, 'ITEM')">
                      <iconify-icon height="20" width="20" icon="ic:baseline-delete"></iconify-icon>
                </button>`;
      }
    }
  ];

  props.data = labelPrints;

  dataTableLabelPrint = createDatatable("table--label-print", props);
}

/*:::::::::::::::: List Tab End ::::::::::::::::*/
