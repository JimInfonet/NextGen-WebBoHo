/*:::::::::::::::: Global ::::::::::::::::*/
const STRING_TABLE = ADD_ITEMS_TO_THE_PO_STRING_TABLE;
let policies = {};
let vendorId = "";
let purchaseNo = "";
let vendorParts = [];
let selectedCriteria;
let dataTableVendorParts;
let dataTableVendorPartsDuplicates;
let selectedVendorParts = [];
let vendorPartsToAdd = [];
let existingOrDuplicateVendorPartObjects = [];

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  vendorId = urlParams.get('vendorId');
  purchaseNo = urlParams.get('purchaseNo');

  $("#vendorPartsForTheVendorXLabel").text(`${$("#vendorPartsForTheVendorXLabel").text()} ${vendorId}`);
  await loadPolicies();
  await loadVendorParts().then(() => renderVendorParts());
});

$('input[name="showByOption"]').on('change', async function () {
  selectedVendorParts = [];
  selectedCriteria = $(this).val();
  await loadVendorParts().then(() => renderVendorParts());
});

async function loadPolicies() {
  const url = STRING_TABLE.URL.GET_POLICIES;

  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

async function loadVendorParts() {
  selectedCriteria = $("input[name='showByOption']:checked").val();

  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.VENDOR_ID, vendorId);
  params.append(STRING_TABLE.PARAMS.SELECTED_CRITEIRIA, selectedCriteria);

  const url = `${STRING_TABLE.URL.GET_VENDOR_PARTS}?${params.toString()}`;

  await ajaxGetAsync(url)
    .then(_vendorParts => vendorParts = _vendorParts)
    .catch(toastrError);
}

function renderVendorParts(duplicates = false) {
  const data = duplicates ? existingOrDuplicateVendorPartObjects : vendorParts;
  const columns = [
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.PART_NO,
    },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.STOCK_CODE,
    },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.VENDOR_ID,
      visible: selectedCriteria !== STRING_TABLE.ROW_TEXT.VENDOR_PARTS_FOR_THE_VENDOR_X,
    },
    {
      data: STRING_TABLE.TABLE.COLUMN_NAME.DESCRIPTION,
    },
    {
      data: null,
      sortable: false,
      render: function (vendorPartDetails) {
        if (duplicates) {
          return `<input class="form-check-input vendor-part-checkbox" type="checkbox" checked
                     data-part='${JSON.stringify(vendorPartDetails)}' onchange="handleDuplicatePartNoSelectionChange(this)" />`;
        }
        return `<input class="form-check-input mt-2 vendor-part-checkbox" type="checkbox" 
                       onchange="handleVendorPartSelectionChange(this)" />`;
      }
    }
  ];

  let props = createDataTableProp();
  props = {
    ...props,
    data: data,
    columns: columns,
    pageLength: 10,
    screenY: duplicates ? "500px" : "auto"
  }

  if (duplicates) {
    destroyDataTableIfExists(dataTableVendorPartsDuplicates);
    dataTableVendorPartsDuplicates = createDatatableWithSearching("table--vendor-parts--duplicates", props);
    return;
  }
  destroyDataTableIfExists(dataTableVendorParts);
  dataTableVendorParts = createDatatableWithSearching("table--vendor-parts", props);
}

function handleVendorPartSelectionChange(element) {
  const rowData = dataTableVendorParts.row($(element).closest('tr')).data();
  const isChecked = $(element).prop('checked');

  if (isChecked) selectedVendorParts.push(rowData);
  else selectedVendorParts = selectedVendorParts.filter(part => part != rowData);
}

function handleGoBackToPreviousPage() {
  const params = new URLSearchParams();
  params.append(GENERAL_STRING_TABLE.PARAMS.PURCHASE_ORDER_NO, purchaseNo);

  const url = `${GENERAL_STRING_TABLE.URL.PURCHASE_DETAIL}?${params.toString()}`;
  safelyNavigateTo(url);
}

async function handleDoneButtonClick() {
  /* If we add a line in the PO with a Part number that is already in another line in the PO
     we need to have a pop up requesting to confirm that they want to add this item to the PO 
     even if it's already in another line in the PO. If Yes add line, if No we do not add line. 
  */

  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.PURCHASE_NO, purchaseNo);
  let url = `${STRING_TABLE.URL.GET_EXISTING_PART_NOS}?${params.toString()}`;

  let partNos = [...selectedVendorParts.map(part => part.partNo)];
  let existingPartNos = [];

  await ajaxPostAsync(url, partNos)
    .then(_existingPartNos => existingPartNos = _existingPartNos)
    .catch(toastrError);

  // Filter duplicate and get confirmations if the user still want to add them
  let existingOrDuplicatePartNos = findDuplicatePartNos([...partNos, ...existingPartNos]);

  existingOrDuplicateVendorPartObjects = selectedVendorParts
    .filter(part => existingOrDuplicatePartNos.includes(part.partNo));

  vendorPartsToAdd = [...selectedVendorParts];

  if (existingOrDuplicateVendorPartObjects.length > 0) {
    renderVendorParts(true); // INFO: true means render for duplicates
    let confirmed = await showDuplicateConfirmationModal();
    if (!confirmed) return;
  }

  // Now find the objects confirmed and not existing objects
  params = { policies, vendorParts: vendorPartsToAdd, vendorId, purchaseNo };
  url = STRING_TABLE.URL.ADD_VENDOR_PARTS;

  await ajaxPostAsync(url, params)
    .then(() => {
      if (params.vendorParts.length) {
        localStorage.setItem("itemsAdded", true);
      }
      handleGoBackToPreviousPage();
    })
    .catch(toastrError);
}

function handleDuplicatePartNoSelectionChange(element) {
  const isChecked = $(element).prop('checked');
  const part = JSON.parse(element.getAttribute('data-part'));

  if (isChecked) vendorPartsToAdd.push(part);
  else vendorPartsToAdd = vendorPartsToAdd
    .filter(_part => !(_part.partNo === part.partNo &&
      _part.stockCode === part.stockCode &&
      _part.vendorId === part.vendorId));
}

function findDuplicatePartNos(partNos) {
  const countMap = {};

  partNos.map(partNo => countMap[partNo] = (countMap[partNo] || 0) + 1);
  partNos = partNos.filter(partNo => countMap[partNo] > 1);

  return partNos;
}

function showDuplicateConfirmationModal() {
  $("#confirmationModalCancel").off("click");
  $("#confirmationModalConfirm").off("click");

  $("#duplicateConfirmationModal").fadeOut("slow", function () {
    $("#duplicateConfirmationModal").modal("show");
  });

  return new Promise((resolve) => {
    $("#confirmationModalConfirm").on('click', function () {
      $("#duplicateConfirmationModal").modal("hide");
      resolve(true);
    });
    $("#confirmationModalCancel, #confirmationModalClose").on('click', function () {
      $("#duplicateConfirmationModal").modal("hide");
      resolve(false);
    });
  });
}

$(document).on('shown.bs.modal', '#duplicateConfirmationModal', function () {
  if (dataTableVendorPartsDuplicates) {
    dataTableVendorPartsDuplicates.columns.adjust();
  }
});
