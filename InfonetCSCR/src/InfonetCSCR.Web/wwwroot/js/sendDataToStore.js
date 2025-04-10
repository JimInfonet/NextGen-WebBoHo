let sendDataToStoreItems;
let selectedItemList = [];
let itemsList = [];
let allMasterTableItems = [];
let selectedStoreList;
let selectedItemsDataTable;
let localizedColumnNames;
let lastSelectedRow = null; // To store the last selected row
let _itemsContainerDataTable = null;
let _selectedItemsContainerDataTable;
let lastWeight = 0;
let deleteTimeout;

$(async function () {
  $('#ddlDataTransferType').trigger('change');
  $('input[name="stores"]').on('change', updateViewTabState);
  $('input[name="transferOption"]').on('change', updateFlagAsync);
  $('#viewTabViewer').attr('hidden', true);
  $('#girdAndReportContainer').attr('hidden', true);
});

async function handleSendDataToStoreRegionCheck(regionID, checked) {
  let storesByRegion;
  let url = `SendDataToStore/GetStoresByRegion`;
  let data = [regionID]

  await ajaxPostAsync(url, data)
    .then(async (_storesByRegion) => {
      storesByRegion = _storesByRegion;
    })
    .catch(toastrError);

  storesByRegion.forEach(function (store) {
    $(`#store-${store.store_ID}`).prop('checked', checked);
  });

  updateViewTabState();
}

function handleRegionsSelectAll(checked) {
  $('input[name="regions"]').prop('checked', checked);
  $('#chkAllStores').prop('checked', checked).trigger('change');
  updateViewTabState();
}

function handleStoresSelectAll(checked) {
  $('input[name="stores"]').prop('checked', checked);
  updateViewTabState();
}

async function handleDDLDataTransferTypeChange(tableName) {
  $('#selectionTab').tab('show');
  let url = `SendDataToStore/GetItems`;
  let data = { tableName };
  $("#backdrop").fadeIn();

  await ajaxGetAsync(url, data)
    .then(async (_items) => {
      sendDataToStoreItems = _items;
      allMasterTableItems = sendDataToStoreItems.masterTableItems;
      allMasterTableItems = allMasterTableItems.map((item, index) => {
        const isSelected = sendDataToStoreItems.selectedItems.some(selectedItem =>
          JSON.stringify(selectedItem) === JSON.stringify(item)
        );

        return { ...item, checked: isSelected, id: index, weight: 0 };
      });

      itemsList = allMasterTableItems;
      selectedItemList = [];
      await refreshItemsAsync(true);
    })
    .catch(error => { toastr.error(error.responseText); });

  $("#backdrop").fadeOut();
}

async function refreshItemsAsync(isChangeTrigger) {

  if (isChangeTrigger) {
    $('#chkAllRegions').prop('checked', false).trigger('change');
  }

  let selectedItemCount = selectedItemList?.length;
  itemsList = allMasterTableItems.filter(item => item.checked === false);
  selectedItemList = allMasterTableItems.filter(item => item.checked === true);
  selectedItemList.sort((a, b) => a.weight - b.weight);

  renderItemsDynamicDataTable(itemsList);
  renderSelectedItemsDynamicDataTable(selectedItemList);
  updateViewTabState();

  clearTimeout(deleteTimeout);

  if (selectedItemList?.length == 0 && selectedItemCount > 0) {
    deleteTimeout = setTimeout(async () => {
      await deleteExpTableDataFromTransferDatabaseAsync();
    }, 500);
  }
}

async function deleteExpTableDataFromTransferDatabaseAsync() {
  let url = `SendDataToStore/DeleteExpTableData`;
  let data = { tableName: $('#ddlDataTransferType').val() };

  await ajaxGetAsync(url, data)
    .catch(toastrError);
}

async function removeSelectedStoresFromTransferDatabaseAsync() {
  let url = `SendDataToStore/RemoveSelectedStoresFromTransferDatabase`;
  let data = { tableName: $('#ddlDataTransferType').val() };

  await ajaxGetAsync(url, data)
    .catch(toastrError);
}

async function removeItemFromTransferDatabaseAsync(item) {
  let url = `SendDataToStore/RemoveItemFromTransferDatabase`;
  let data = {
    TableName: $('#ddlDataTransferType').val(),
    Items: [JSON.parse(item)]
  };

  await ajaxPostAsync(url, data)
    .then(async (success) => {
      if (success && $('#selectedItemsContainer').find('table tr').length === 0) {
        await removeSelectedStoresFromTransferDatabaseAsync();
      }
    })
    .catch(toastrError);
}

function handleItemsSelectUnselectAll(container) {
  $('#uchkAllItems').prop('checked', false);
  $('#chkAllItems').prop('checked', false);

  if (container === "itemsContainer") {
    allMasterTableItems.forEach(item => {
      item.checked = true;
    });
  }
  else {
    allMasterTableItems.forEach(item => {
      item.checked = false;
    });
  }

  itemsList = allMasterTableItems.filter(item => item.checked === false);
  selectedItemList = allMasterTableItems.filter(item => item.checked === true);
  renderItemsDynamicDataTable(itemsList);
  renderSelectedItemsDynamicDataTable(selectedItemList);
  updateViewTabState();
}

async function initViewTab() {
  $("#backdrop").fadeIn();
  closeReport();
  setSelectedStore();
  await createTableAsync();
  $('#transferOptionUpdate').prop('disabled', $('#ddlDataTransferType').val().toUpperCase() !== 'TENDMAST');
  $("#backdrop").fadeOut();
}

function setSelectedStore() {
  $('#selectedStoreList').empty();
  selectedStoreList = [];

  $('input[name="stores"]:checked').each(function () {
    selectedStoreList.push($(this).val());
    let labelText = $(this).next('label').text().trim();
    $('#selectedStoreList').append(labelText + '\n');
  });
}

async function createTableAsync() {
  let url = `SendDataToStore/CreateTable`;
  let data = {
    TableName: $('#ddlDataTransferType').val(),
    SelectedItems: selectedItemList,
    SelectedStoreIDs: selectedStoreList
  };

  await ajaxPostAsync(url, data)
    .then(async (_items) => {
      localizedColumnNames = _items.localizedTitle;
      const specialKeys = ['TrDate', 'UserCode', 'Flag'];

      let columns = Object.keys(_items.masterTableItems[0])
        .filter(key => !specialKeys.includes(key))
        .map(key => {
          return { "data": key, "title": _items.localizedTitle[key] };
        });

      specialKeys.forEach(key => {
        if (key in _items.masterTableItems[0]) {
          columns.push({ "data": key, "title": _items.localizedTitle[key] });
        }
      });

      selectedItemList.forEach(selectedItem => {
        let selectedItemWithoutIdWeightChecked = { ...selectedItem };
        delete selectedItemWithoutIdWeightChecked.id;
        delete selectedItemWithoutIdWeightChecked.weight;
        delete selectedItemWithoutIdWeightChecked.checked;

        let matchedItem = _items.masterTableItems.find(item => {
          return Object.keys(selectedItemWithoutIdWeightChecked).every(key =>
            item[key] === selectedItemWithoutIdWeightChecked[key]
          );
        });

        if (matchedItem) {
          matchedItem.weight = selectedItem.weight;
        }
      });

      await createSelectedItemsDataTableAsync(columns, _items.masterTableItems);
    })
    .catch(toastrError);
}

async function createSelectedItemsDataTableAsync(columns, data) {
  const headerHtml = columns.map(col => `<th width="${col.width || 'auto'}"></th>`).join('');
  const footerHtml = columns.map(col => `<th class="input substring-search" width="${col.width || 'auto'}" scope="col"></th>`).join('');

  destroyDataTableIfExists(selectedItemsDataTable);

  $('#selectedItemsGridContainer').html(`
    <table id="selectedItemsGrid" class="table table-striped">
      <thead class="datatable-allignment">
        <tr>${headerHtml}</tr>
      </thead>
    </table>
  `);

  let props = createDataTableProp();
  props.data = data;
  props.paging = true;
  props.scrollY = '365px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = columns;
  selectedItemsDataTable = createModalDatatableWithSearching('selectedItemsGrid', props);

  setTimeout(() => {
    selectedItemsDataTable.columns.adjust();
  }, 100);

  $(`#selectedItemsGrid tbody`)
    .off()
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        selectedItemsDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        const flag = selectedItemsDataTable.row(this).data().Flag;
        $('input[name="transferOption"][value="' + flag + '"]').prop('checked', true);
      }
    });

  if (selectedItemsDataTable.page.info().recordsDisplay !== 0) {
    $('#selectedItemsGrid tbody tr').first()?.trigger('click');
  }
}

function updateViewTabState() {
  const viewTab = $('#viewTab');

  // Check if any store or item is selected
  const storesSelected = $('input[name="stores"]:checked').length > 0;
  const itemsSelected = selectedItemList.length > 0;

  if (storesSelected && itemsSelected) {
    viewTab.removeClass('disabled');
    $('#viewTabViewer').removeAttr('hidden');
    $('#girdAndReportContainer').removeAttr('hidden');
  } else {
    viewTab.addClass('disabled');
    $('#viewTabViewer').attr('hidden', true);
    $('#girdAndReportContainer').attr('hidden', true);
  }
}

async function updateFlagAsync() {
  let selectedFlag = $(this).val();
  let selectedRow = $('#selectedItemsGrid tbody tr.selected-grid-row');
  let rowData = selectedItemsDataTable.row(selectedRow).data();

  let url = `SendDataToStore/UpdateFlag`;
  let data = {
    TableName: $('#ddlDataTransferType').val(),
    SelectedItems: [rowData],
    Flag: selectedFlag
  };

  await ajaxPostAsync(url, data)
    .then(async (success) => {
      if (success) {
        rowData.Flag = selectedFlag;
        selectedItemsDataTable.row(selectedRow).data(rowData).draw(false);
      }
    })
    .catch(toastrError);
}

async function zipAndDownloadAsync(storeOptions, tableData) {
  let zip = new JSZip();

  storeOptions.forEach(store => {
    for (let key in tableData) {
      zip.file(`${store.storeId}/${key}.txt`, tableData[key]);
    }
  });

  // Generate the zip file asynchronously
  const content = await zip.generateAsync({ type: "blob" });

  // Create a link element for the download
  const link = document.createElement("a");
  link.href = URL.createObjectURL(content);
  link.download = 'DataTransfer.zip';
  link.click();
}

async function transferDataAsync() {
  $("#backdrop").fadeIn();
  let url = `SendDataToStore/TransferData`;
  let data = {
    TableName: $('#ddlDataTransferType').val(),
    SelectedStoreIDs: selectedStoreList
  };

  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (!response.dts) {
        await zipAndDownloadAsync(response.storeOptions, response.tableData);
      }
    })
    .catch(toastrError);

  $("#backdrop").fadeOut();

  await handleDDLDataTransferTypeChange($('#ddlDataTransferType').val());
  toastr.success($('#lclTransferCompleted').val());
}

function showReport() {
  $('#reportViewer').text(getReport());
  $('#selectedItemsGridContainer').hide();
  $('#reportViewer').show();
  $('#btnReport').hide();
  $('#btnCloseReport').show();
  $('#btnPrint').prop('disabled', false);
  $('#btnPrint').on('click', generateTextReportPDF);
}

function getReport() {
  let report = [];
  const reportSize = 65;
  const padding = 43;
  let tempSelectedItemsDataTable = selectedItemsDataTable.rows().data().toArray();
  tempSelectedItemsDataTable.sort((a, b) => a.weight - b.weight);

  selectedStoreList.forEach(storeId => {

    tempSelectedItemsDataTable.forEach(item => {
      if (item.Flag === 'A') {
        report.push($('#lclRecordWillBeAppended').val());
      } else if (item.Flag === 'R') {
        report.push($('#lclRecordWillBeRemoved').val());
      } else if (item.Flag === 'U') {
        report.push($('#lclRecordWillBeUpdated').val());
      }

      report.push('-'.repeat(reportSize));

      for (let key in item) {
        if (key === "weight") continue;
          if (item.Flag === 'R' || item.Flag === 'U') {
            const isKeyPresent = [...sendDataToStoreItems.primaryColumns, ...sendDataToStoreItems.descriptionColumns]
                  .some(column => column.toLowerCase() === key.toLowerCase());

          if (isKeyPresent) {
            report.push(`${localizedColumnNames[key].padEnd(padding)}${item[key] ?? ''}`);
          }
        }
        else if (key !== 'Flag' && key !== 'TrDate' && key !== 'UserCode' && item[key] != null && item[key].trim() != "") {
            report.push(`${localizedColumnNames[key].padEnd(padding)}${item[key] ?? ''}`);
        }
        else if (item[key] == null || item[key].trim() == "") {
            if (key == 'TERMS' ||
                key == 'DISCOUNT_DAYS' ||
                key == 'Early_Pay_Percent' ||
                key == 'CUST_DISC' ||
                key == 'PRICE_CODE' ||
                key == 'CL_HISTBAL' ||
                key == 'CL_HPAID' ||
                key == 'CL_LIMIT' ||
                key == 'CL_CURBAL'
            ) {
                report.push(`${localizedColumnNames[key].padEnd(padding)}0`);
            }
            else if (key == 'UseFuelRebate' ||
                key == 'UseFuelRebateDiscount' ||
                key == 'TaxExempt'
            ) {
                report.push(`${localizedColumnNames[key].padEnd(padding)}False`);
            }
        }
      }

      report.push(`${localizedColumnNames['StoreID'].padEnd(padding)}${storeId}`);

      report.push('-'.repeat(reportSize));
      report.push('\n');
    });
  });

  return report.join('\n');
}

function closeReport() {
  $('#reportViewer').hide();
  $('#selectedItemsGridContainer').show();
  $('#btnCloseReport').hide();
  $('#btnReport').show();
  $('#btnPrint').prop('disabled', true);
}

function printToPDF() {
  let pdfDefinition = createPdfDefinition();
  pdfDefinition.styles.preformatted.fontSize = 9;
  pdfDefinition.styles.preformatted.alignment = 'left';

  pdfDefinition.content.push({
    lineHeight: 1.5,
    text: $('#reportViewer').text(),
    preserveLeadingSpaces: false,
    preserveTrailingSpaces: false,
    style: 'preformatted'
  });

  printWithPdfMake(pdfDefinition);
}

async function generateTextReportPDF() {
    console.log("generateTextReportPDF triggered");
    const url = `SendDataToStore/GenerateTextReportPDF`;
    const data = getReport(); // Get the report parameters
    console.log(data);
    // Make an AJAX call to the controller action
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data) // Send the data as a JSON string
    });
    if (response.ok) {
        // If the response is OK, get the blob (PDF file)
        const blob = await response.blob();
        // Create a temporary link to trigger the download
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob); // Create a URL for the blob
        link.download = 'Report.pdf'; // Set the filename for the download
        link.click(); // Trigger the download
        // Clean up by revoking the object URL after download
        URL.revokeObjectURL(link.href);
    } else {
        toastrError('Error generating PDF');
    }
}

function updateCheckedStatusOfMasterTableItems(id, isChecked) {
  allMasterTableItems = allMasterTableItems.map((item) => {
    if (item.id === id) {
      return {
        ...item,
        checked: isChecked,
        weight: isChecked ? ++lastWeight : item.weight
      };
    }
    return item;
  });
}

function renderItemsDynamicDataTable(dynamicData) {
  let dataTableId = "itemsContainerDataTable";

  // Extract dynamic column names
  let columns = Object.keys(dynamicData[0] || {});
  if (columns.includes('checked')) {
    columns = columns.filter(col => col !== 'checked');
    columns.unshift('checked');
  }

  if (columns.length === 0) {
    columns = ["checked"];
  }

  $("#itemsContainerDataTableContainer").empty();
  $("#itemsContainerDataTableContainer").append(`<table id="itemsContainerDataTable" class="table table-striped">
                <thead>
                  <tr id="dynamicTableItemsHeader"></tr>
                </thead>
                <tfoot>
                  <tr id="dynamicTableItemsFooter"></tr>
                </tfoot>
              </table>`);

  const headerRow = $("#dynamicTableItemsHeader");
  const footerRow = $("#dynamicTableItemsFooter");

  headerRow.empty();
  footerRow.empty();

  // Add dynamic columns to header/footer
  columns.forEach((col, index) => {
    headerRow.append(`<th>${col === "checked" ? "" : `${$("#columnText").val()} ${index}`}</th>`);
    footerRow.append(`<th class="input searchTd substring-search ${col === "checked" ? "d-none" : ""}" scope="col"></th>`);
  });

  // Set DataTable properties
  let props = createDataTableProp();
  props.data = dynamicData;
  props.scrollY = '300px';
  props.autoWidth = true;
  props.paging = true;
  props.searching = true;
  props.info = false;

  // Dynamically create column definitions based on the data
  props.columns = columns.map((col) => {
    if (col === 'checked') {
      return {
        "data": "checked",
        "className": 'text-center',
        "orderable": false,
        "searchable": false,
        "width": '50px',
        "render": function (data, type, row, meta) {
          return `<input type="checkbox" class="form-check-input" ${data ? 'checked' : ''} data-index="${meta.row}" id="item-${meta.row}">`;
        }
      };
    } else if (col === 'id') {
      return {
        "data": "id",
        "visible": false,
      };
    } else if (col === 'weight') {
      return {
        "data": "weight",
        "visible": false,
      };
    } else {
      return {
        "data": col,
        "className": 'text-center dataTableData',
      };
    }
  });

  props.columnDefs = [
    { className: 'text-center dataTableData', targets: '_all' },
  ];

  _itemsContainerDataTable = createModalDatatableWithSearching(dataTableId, props);

  const childId = `${dataTableId}_info`;
  const childDiv = document.getElementById(childId);

  if (childDiv) {
    const parentDiv = childDiv.parentElement;
    parentDiv.removeAttribute('class');
    parentDiv.classList.add('row');
  }

  // Handle row click
  $(`#${dataTableId} tbody`)
    .off()
    .on('click', 'tr', async function (e) {
      const tableinfo = _itemsContainerDataTable.page.info();

      if (tableinfo.recordsDisplay === 0)
        return;

      let rowData = _itemsContainerDataTable.row(this).data();
      if ($(this).hasClass('selected-grid-row') || $(e.target).is(':checkbox')) {
        updateCheckedStatusOfMasterTableItems(rowData.id, true);
        await refreshItemsAsync(false);
      }
      else {
        _itemsContainerDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
    });
}

function renderSelectedItemsDynamicDataTable(dynamicData) {
  let dataTableId = "selectedItemsContainerDataTable";

  let columns = Object.keys(dynamicData[0] || {});
  if (columns.includes('checked')) {
    columns = columns.filter(col => col !== 'checked');
    columns.unshift('checked');
  }

  if (columns.length === 0) {
    columns = ["checked"];
  }

  $("#selectedItemsContainerDataTableContainer").empty();
  $("#selectedItemsContainerDataTableContainer").append(`<table id="selectedItemsContainerDataTable" class="table table-striped" width="100%">
                <thead>
                  <tr id="dynamicTableSelectedItemsHeader"></tr>
                </thead>
                <tfoot>
                  <tr id="dynamicTableSelectedItemsFooter"></tr>
                </tfoot>
              </table>`);

  const headerRow = $("#dynamicTableSelectedItemsHeader");
  const footerRow = $("#dynamicTableSelectedItemsFooter");

  headerRow.empty();
  footerRow.empty();

  // Add dynamic columns to header/footer
  columns.forEach((col, index) => {
    headerRow.append(`<th>${col === "checked" ? "" : `${$("#columnText").val()} ${index}`}</th>`);
    footerRow.append(`<th class="input searchTd substring-search ${col === "checked" ? "d-none" : ""}" scope="col"></th>`);
  });

  // Set DataTable properties
  let props = createDataTableProp();
  props.data = dynamicData;
  props.scrollY = '300px';
  props.autoWidth = true;
  props.paging = true;
  props.searching = true;
  props.info = false;

  // Dynamically create column definitions based on the data
  props.columns = columns.map((col) => {
    if (col === 'checked') {
      return {
        "data": "checked",
        "className": 'text-center',
        "orderable": false,
        "searchable": false,
        "render": function (data, type, row, meta) {
          return `<input type="checkbox" class="form-check-input" ${data ? 'checked' : ''} data-index="${meta.row}" id="item-${meta.row}">`;
        }
      };
    } else if (col === 'id') {
      return {
        "data": "id",
        "visible": false,
      };
    } else if (col === 'weight') {
      return {
        "data": "weight",
        "visible": false,
      };
    } else {
      return {
        "data": col,
        "className": 'text-center dataTableData',
      };
    }
  });

  props.columnDefs = [
    { className: 'text-center dataTableData', targets: '_all' },
  ];

  _selectedItemsContainerDataTable = createModalDatatableWithSearching(dataTableId, props);

  const childId = `${dataTableId}_info`;
  const childDiv = document.getElementById(childId);

  if (childDiv) {
    const parentDiv = childDiv.parentElement;
    parentDiv.removeAttribute('class');
    parentDiv.classList.add('row');
  }

  // Handle row click
  $(`#${dataTableId} tbody`)
    .off()
    .on('click', 'tr', async function (e) {
      const tableinfo = _selectedItemsContainerDataTable.page.info();

      if (tableinfo.recordsDisplay === 0)
        return;

      let rowData = _selectedItemsContainerDataTable.row(this).data();
      if ($(this).hasClass('selected-grid-row') || $(e.target).is(':checkbox')) {
        updateCheckedStatusOfMasterTableItems(rowData.id, false);
        await refreshItemsAsync(false);
      }
      else {
        _selectedItemsContainerDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
    })
}
