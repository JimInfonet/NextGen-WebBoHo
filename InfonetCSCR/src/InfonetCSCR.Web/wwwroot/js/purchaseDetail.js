let selectedPurchaseNo = $("#selectedPoNumber").val();
let selectedVendorId;
let purchaseHeaderData;
let vendorData;
let companyData;
let policies;
let purchaseDetails;
let purchaseDetailDatatable;
let selectedPartNo;
let selectedPurchaseDetailData;
let selectedInputToUpdate;
let quantDecPolicy;
let costDecPolicy;
let selectedRowNumber;
let selectedPOStatus = "O";
let orderedQuantity;
let purchaseHeaderModalInputDateTmpData;

const orderQuantityText = CONSTANT_TEXTS.ORDER_QUANTITY;
const packingQuantityText = CONSTANT_TEXTS.PACKING_QUANTITY;
const receivedQuantityText = CONSTANT_TEXTS.RECEIVED_QUANTITY;
const poCostText = CONSTANT_TEXTS.PO_COST;
const applyGSTText = CONSTANT_TEXTS.APPLY_GST;
const buyerText = CONSTANT_TEXTS.BUYER;
const shipViaText = CONSTANT_TEXTS.SHIP_VIA;
const fobText = CONSTANT_TEXTS.FOB;
const freightText = CONSTANT_TEXTS.FREIGHT;
const customerAccountText = CONSTANT_TEXTS.CUSTOMERACCOUNT;
const poDateText = CONSTANT_TEXTS.PO_DATE;
const toBeDeliveredText = CONSTANT_TEXTS.TO_BE_DELIVERED;
const dateReceivedText = CONSTANT_TEXTS.DATE_RECEIVED;
const invoiceNumberText = CONSTANT_TEXTS.INVOICE_NUMBER;
const termsText = CONSTANT_TEXTS.TERMS;
const notesText = CONSTANT_TEXTS.NOTES;
const subTotalText = CONSTANT_TEXTS.SUB_TOTAL;
const surchargeDiscountText = CONSTANT_TEXTS.SURCHARGE_DISCOUNT;
const taxTotalText = CONSTANT_TEXTS.TAX_TOTAL;
const totalText = CONSTANT_TEXTS.TOTAL;

const smallDoubleLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LIMIT;
const smallDoubleLowerLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LOWER_LIMIT;
const smallIntegerLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_INTEGER_LIMIT;

$(async function () {
  if (localStorage.getItem("itemsAdded")) {
    localStorage.removeItem("itemsAdded");
    toastr.success($("#itemsAreAddedSuccessfully").val());
  }

  await loadPolicies();
  await loadPurchaseHeader();
  await loadPurchaseDetailsByPurchaseNumber();
  await loadVendor();
  await loadCompany();
});

let createPurchaseDetailDatatable = async () => {
  if (purchaseDetailDatatable) {
    purchaseDetailDatatable.state.clear();
    purchaseDetailDatatable.clear();
    purchaseDetailDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = purchaseDetails;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "purchaseNo", "visible": false, },
    { "data": "lineNumber", },
    { "data": "partNumber", },
    {
      "data": "orderedQuantity",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === null || data === undefined || data === "") {
            data = 0;
          }
          return parseFloat(data).toFixed(quantDecPolicy);
        }
        return data;
      }
    },
    { "data": "unitOfMeasure", },
    { "data": "unitQuantity", },
    {
      "data": "receivedQuantity",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === null || data === undefined || data === "") {
            data = 0;
          }
          return parseFloat(data).toFixed(quantDecPolicy);
        }
        return data;
      }
    },
    {
      "data": function (row, type, val, meta) {
        if (row.poCost === null || row.poCost === undefined || row.poCost === "") {
          if (row.cost) {
            return parseFloat(row.cost).toFixed(costDecPolicy);
          }
          return parseFloat(0).toFixed(costDecPolicy);
        } else {
          return parseFloat(row.poCost).toFixed(costDecPolicy);
        }
      }
    },
    {
      "data": "calculateDiscount",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === null || data === undefined || data === "") {
            data = 0;
          }
          return parseFloat(data).toFixed(4);
        }
        return data;
      }
    },
    { "data": "description", },
    {
      "data": "extCost",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === null || data === undefined || data === "") {
            data = 0;
          }
          return parseFloat(data).toFixed(costDecPolicy);
        }
        return data;
      }
    },
    {
      "data": "applyGST",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === true) {
            return "True";
          }

          return "False";
        }
        return data;
      }
    },
    {
      "data": "gst",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === null || data === undefined || data === "") {
            data = 0;
          }
          return parseFloat(data).toFixed(costDecPolicy);
        }
        return data;
      }
    },
    { "data": "posted", "visible": false, },
    { "data": "stockCode", "visible": false, },
    { "data": "productVendor", "visible": false, },
    {
      "data": "charges",
      "render": function (data, type, row) {
        if (type === "display") {
          if (data === null || data === undefined || data === "") {
            data = 0;
          }
          return parseFloat(data).toFixed(2);
        }
        return data;
      },
      "visible": policies.poChargesPolicy
    }
  ];

  purchaseDetailDatatable = await createDatatableWithSearching('purchaseDetail-grid', props);

  $(`#purchaseDetail-grid tbody`)
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        purchaseDetailDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
      selectedPurchaseDetailData = purchaseDetailDatatable.row(this).data();
      selectedPartNo = $(this).find("td:eq(1)").text();
      orderedQuantity = selectedPurchaseDetailData.orderedQuantity.toFixed(quantDecPolicy);
    });

  if (purchaseDetails.length > 0) {
    $(`#purchaseDetail-grid tbody tr:eq(0)`).addClass('selected-grid-row');
    selectedPurchaseDetailData = purchaseDetailDatatable.row(0).data();
  }
  else {
    selectedPurchaseDetailData = null;
  }
}

let getQuantDecPolicyByStockCode = async (stockCode) => {
  let quantDecPolicy = 0;
  let url = `PurchaseDetail/GetQuantDecPolicyByStockCode?stockCode=${stockCode}`;
  await ajaxGetAsync(url)
    .then(_quantDecPolicy => quantDecPolicy = _quantDecPolicy)
    .catch(toastrError);

  return quantDecPolicy;
}

let getCostDecPolicyByStockCode = async (stockCode) => {
  let costDecPolicy = 0;
  let url = `PurchaseDetail/GetCostDecPolicyByStockCode?stockCode=${stockCode}`;
  await ajaxGetAsync(url)
    .then(_costDecPolicy => costDecPolicy = _costDecPolicy)
    .catch(toastrError);

  return costDecPolicy;
}

$('#purchaseDetail-grid')
  .off()
  .on('click', 'td', async function () {
    let cellIndex = $(this).index();
    selectedRowNumber = $(this).closest('tr').index();

    $("#modalInputSelectId").hide();
    $("#modalInputId").show();
    let isEditable = !(purchaseHeaderData.poStatus === 'C' && selectedPurchaseDetailData.posted === true);

    let row = $(this).closest('tr');

    let rowData = purchaseDetailDatatable.row(this).data();

    row.trigger('click');
    $("#modalInputId").val("");

    if (cellIndex === 2 && isEditable) {
      if (rowData && rowData.stockCode) {
        quantDecPolicy = await getQuantDecPolicyByStockCode(rowData.stockCode);
      }

      $("#purchaseDetailInputModal").modal('show');
      $("#modalLabelId").html($("#orderQuantityText").val());
      $("#modalInputId").val(rowData.orderedQuantity);
      selectedInputToUpdate = orderQuantityText;
    }

    if (cellIndex === 4 && isEditable) {
      $("#purchaseDetailInputModal").modal('show');
      $("#modalLabelId").html($("#packingQuantityText").val());
      $("#modalInputId").val(rowData.unitQuantity);
      selectedInputToUpdate = packingQuantityText;
    }

    if (cellIndex === 5 && isEditable) {
      $("#purchaseDetailInputModal").modal('show');
      $("#modalLabelId").html($("#receivedQuantityText").val());
      $("#modalInputId").val(rowData.receivedQuantity);
      selectedInputToUpdate = receivedQuantityText;
    }

    if (cellIndex === 6) {
      $("#purchaseDetailInputModal").modal('show');
      $("#modalLabelId").html($("#costText").val());
      $("#modalInputId").val(rowData.poCost);
      selectedInputToUpdate = poCostText;
    }

    if (cellIndex === 10 && isEditable) {
      $("#purchaseDetailInputModal").modal('show');
      $("#modalInputSelectId").show();
      $("#modalInputId").hide();
      $("#modalLabelId").html($("#taxable").val());
      $("#modalInputSelectId").val(rowData.applyGST ? "true" : "false");
      selectedInputToUpdate = applyGSTText;
    }
  });

let loadPurchaseDetailsByPurchaseNumber = async (loadTable = true) => {
  let url = `PurchaseDetail/GetPurchaseDetailsByPurchaseNumber?purchaseNo=${selectedPurchaseNo}`;
  await ajaxGetAsync(url)
    .then(async _purchaseDetails => {
      purchaseDetails = _purchaseDetails;
      selectedPartNo = purchaseDetails.partNumber
      if (purchaseDetails.length > 0) {
        if (purchaseDetails[0] && purchaseDetails[0].stockCode) {
          quantDecPolicy = await getQuantDecPolicyByStockCode(purchaseDetails[0].stockCode);
          costDecPolicy = await getCostDecPolicyByStockCode(purchaseDetails[0].stockCode);
        }
        else {
          quantDecPolicy = costDecPolicy = 0;
        }
      }

      if (loadTable) {
        createPurchaseDetailDatatable();
      }
    })
    .catch(toastrError);
}

let loadPolicies = async () => {
  let url = "PurchaseDetail/GetPolicies";

  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

let renderPurchaseHeader = () => {
  if (purchaseHeaderData.totalGST === 0) {
    $("#addTaxButtonId").show();
    $("#deleteTaxButtonId").hide();
  }
  else {
    $("#addTaxButtonId").hide();
    $("#deleteTaxButtonId").show();
  }

  if (purchaseHeaderData.subTotal === 0) {
    $("#addTaxButtonId").prop("disabled", true);
  }
  else {
    $("#addTaxButtonId").prop("disabled", false);
  }

  $("#poNumberInputId").val(purchaseHeaderData.purchaseNo);
  $("#customerAccountInputId").val(purchaseHeaderData.custAccount);

  if (purchaseHeaderData.purchaseDate) {
    $("#poDateInputId").val(purchaseHeaderData.purchaseDate.split('T')[0]);
  }

  if (purchaseHeaderData.deliveryDate) {
    $("#toBeDeliveredDateInputId").val(purchaseHeaderData.deliveryDate.split('T')[0]);
  }

  if (purchaseHeaderData.receivedDate) {
    $("#dateReceivedInputId").val(purchaseHeaderData.receivedDate.split('T')[0]);
  }

  if ($("#invoiceNumberInputId")) {
    $("#invoiceNumberInputId").val(purchaseHeaderData.invoiceNo);
  }

  if ($("#termsInputId")) {
    $("#termsInputId").val(purchaseHeaderData.terms);
  }

  $("#buyerInputId").val(purchaseHeaderData.buyer);
  $("#shipViaInputId").val(purchaseHeaderData.shipVia);
  $("#fobInputId").val(purchaseHeaderData.fob);
  $("#freightInputId").val(purchaseHeaderData.freight);

  $("#notesInputId").val(purchaseHeaderData.notes);

  if (!purchaseHeaderData.subTotal) {
    purchaseHeaderData.subTotal = 0;
  }
  $("#subTotalInputId").val(purchaseHeaderData.subTotal.toFixed(2));

  if (!purchaseHeaderData.surChargeTotal) {
    purchaseHeaderData.surChargeTotal = 0;
  }
  $("#surchargeDiscountInputId").val(purchaseHeaderData.surChargeTotal.toFixed(2));

  if (!purchaseHeaderData.totalGST) {
    purchaseHeaderData.totalGST = 0;
  }
  $("#taxTotalInputId").val(purchaseHeaderData.totalGST.toFixed(2));

  if (!purchaseHeaderData.charges) {
    purchaseHeaderData.charges = 0;
  }
  $("#chargesInputId").val(purchaseHeaderData.charges.toFixed(2));

  if (!purchaseHeaderData.purchaseTotal) {
    purchaseHeaderData.purchaseTotal = 0;
  }
  $("#totalInputId").val(purchaseHeaderData.purchaseTotal.toFixed(2));
}

let loadPurchaseHeader = async () => {
  let url = `PurchaseDetail/GetPurchaseHeadersByPurchaseNo?purchaseNo=${selectedPurchaseNo}`;

  await ajaxGetAsync(url)
    .then(_purchaseHeader => {
      purchaseHeaderData = _purchaseHeader;
      selectedPOStatus = purchaseHeaderData.poStatus;
      selectedVendorId = purchaseHeaderData.accountNo;
      renderPurchaseHeader();
    })
    .catch(toastrError);
}

let renderToAddress = () => {
  $("#headerToAddressInputToAddress").html(
    `<p class="text-ellipsis" title="${vendorData.name || ''}"><span>${vendorData.name || ''}</span></p>
     <p class="text-ellipsis" title="${vendorData.address || ''}">${vendorData.address || ''}</p>
     <p class="text-ellipsis" title="${vendorData.city || ''} ${vendorData.province || ''}"><span class="adressSpan">${vendorData.city || ''}</span><span>${vendorData.province || ''}</span></p>
     <p class="text-ellipsis" title="${vendorData.postal || ''}"><span>${vendorData.postal || ''}</span></p>`);

  $("#headerToAddressInputPhone").val(vendorData.phone);
  $("#headerToAddressInputFax").val(vendorData.fax);
  $("#headerToAddressInputAccount").val(vendorData.code);
}

let loadVendor = async () => {
  let url = `PurchaseDetail/GetVendorByVendorCode?code=${selectedVendorId}`;

  await ajaxGetAsync(url)
    .then(_vendorData => {
      vendorData = _vendorData;
      renderToAddress();
    })
    .catch(toastrError);
}

let renderFromAddress = () => {
  $("#headerFromAddressInputFromAddress").html(
    `<p>${companyData.companyName || ''}</p>
     <p><span class="adressSpan">${companyData.address1 || ''}</span><span>${companyData.address2 || ''}</span></p>
     <p><span class="adressSpan">${companyData.city || ''}</span><span>${companyData.province || ''}</span></p>
     <p>${companyData.postalCode || ''}</p>`);

  $("#headerFromAddressInputPhone").val(companyData.telephoneNumber);
  $("#headerFromAddressInputFax").val(companyData.faxNumber);
}

let loadCompany = async () => {
  let url = "PurchaseDetail/GetCompany";

  await ajaxGetAsync(url)
    .then(_companyData => {
      companyData = _companyData;
      renderFromAddress();
    })
    .catch(toastrError);
}

let getVendorUnitCostPercentage = async (inputValue) => {
  let vendorUnitCostPercentage;
  let purchaseDetail = {
    purchaseNo: selectedPurchaseDetailData.purchaseNo,
    partNumber: selectedPurchaseDetailData.partNumber,
    stockCode: selectedPurchaseDetailData.stockCode,
    lineNumber: selectedPurchaseDetailData.lineNumber,
  }

  purchaseDetail = {
    ...purchaseDetail,
    poCost: inputValue,
    unitQuantity: selectedPurchaseDetailData.unitQuantity
  }

  let data = {
    purchaseDetail,
    vendorId: selectedPurchaseDetailData.productVendor
  }

  let url = "PurchaseDetail/GetVendorUnitCostPercentage";

  await ajaxPostAsync(url, data)
    .then(_vendorUnitCostPercentage => vendorUnitCostPercentage = _vendorUnitCostPercentage)
    .catch(toastrError);

  return vendorUnitCostPercentage;
}

function handleAddFromListClick() {
  const params = new URLSearchParams();
  params.append(GENERAL_STRING_TABLE.PARAMS.PURCHASE_NO, selectedPurchaseNo);
  params.append(GENERAL_STRING_TABLE.PARAMS.VENDOR_ID, selectedVendorId);

  const url = `${GENERAL_STRING_TABLE.URL.ADD_ITEMS_TO_THE_PO}?${params.toString()}`;
  safelyNavigateTo(url);
}

let deleteButtonClicked = async () => {
  if (!selectedPurchaseDetailData) {
    toastr.warning($("#pleaseSelectAPurchaseDetailBeforeDeleting").val());
    return;
  }

  let isDelete = false;
  let title = $("#confirmDeletion").val();
  let message = $("#areYouSureYouWantToDeleteIt").val();

  await showConfirmModal(title, message).then((result) => {
    isDelete = result;
  });

  if (!isDelete) {
    return;
  }

  if (selectedPurchaseDetailData.posted) {
    let title = $("#postedItem").val();
    let message = $("#youAreNotAllowedToDeleteThisPostedItem").val();

    showAlertModal(title, message).then((result) => { });
    return;
  }

  let data = {
    purchaseNo: selectedPurchaseDetailData.purchaseNo.toString(),
    partNumber: selectedPurchaseDetailData.partNumber.toString(),
    lineNumber: selectedPurchaseDetailData.lineNumber.toString()
  }

  let url = "PurchaseDetail/DeletePurchaseDetail";

  await ajaxPostAsync(url, data)
    .then(async () => {
      await loadPurchaseDetailsByPurchaseNumber();
      selectedInputToUpdate = null;
      await purchaseHeaderModalSaveButtonClicked();
      toastr.success($("#purchaseDetailSuccessfullyDeleted").val());
    })
    .catch(toastrError);
}

let updatePurchaseDetailRowData = (result) => {
  let table = $('#purchaseDetail-grid').DataTable();
  let rowsPerPage = table.page.len();
  let currentPage = table.page();
  let targetRowIndex = (currentPage * rowsPerPage) + selectedRowNumber;
  let selectedRow = table.row(targetRowIndex);

  if (selectedRow.any()) {
    selectedRow.data(result);
    table.draw();
  }

  table.page(currentPage).draw(false);
}

let modalSaveButtonClicked = async () => {
  let purchaseDetail = {
    purchaseNo: selectedPurchaseDetailData.purchaseNo,
    partNumber: selectedPurchaseDetailData.partNumber,
    stockCode: selectedPurchaseDetailData.stockCode,
    lineNumber: selectedPurchaseDetailData.lineNumber,
  }
  let inputValue;

  if (selectedInputToUpdate === orderQuantityText) {
    inputValue = parseFloat($("#modalInputId").val());
    if (inputValue) {
      if (quantDecPolicy === 0 && $("#modalInputId").val().includes('.')) {
        toastr.warning($("#decimalPointsAreNotAllowedForThisProduct").val());
        return;
      }

      if (inputValue < smallDoubleLowerLimit || inputValue > smallDoubleLimit) {
        toastr.warning(quantDecPolicy === 0 ? $("#orderQuantityShouldBeInRangeWithoutDecimal").val()
          : $("#orderQuantityShouldBeIn").val());
        return;
      }
    }
    else {
      toastr.warning($("#youShouldEnterNumericValueForOrderQty").val());
      return;
    }

    inputValue = inputValue.toFixed(quantDecPolicy);
    orderedQuantity = inputValue;

    purchaseDetail = {
      ...purchaseDetail,
      orderedQuantity: inputValue,
    }
  }

  if (selectedInputToUpdate === packingQuantityText) {
    inputValue = parseInt($("#modalInputId").val());
    if (inputValue >= 1) {
      if (inputValue > smallIntegerLimit) {
        toastr.warning($("#packingQuantityShouldBeIn").val());
        return;
      }
    }
    else {
      toastr.warning($("#minimumValueForPackingQty").val());
      return;
    }

    purchaseDetail = {
      ...purchaseDetail,
      unitQuantity: inputValue
    }
  }

  if (selectedInputToUpdate === receivedQuantityText) {
    inputValue = parseFloat($("#modalInputId").val());

    if (quantDecPolicy === 0 && $("#modalInputId").val().includes('.')) {
      toastr.warning($("#decimalPointsAreNotAllowedForThisProductReceivedQuantity").val());
      return;
    }
    else if (inputValue < smallDoubleLowerLimit || inputValue > smallDoubleLimit) {
      toastr.warning(quantDecPolicy === 0 ? $("#receivedQuantityShouldBeInRangeWithoutDecimal").val()
        : $("#receivedQuantityShouldBeIn").val());
      return;
    }
    else if (inputValue > orderedQuantity) {
      toastr.warning($("#receivedQuantityCantBeMoreThanOrderedQuantity").val());
      return;
    }
    else if (isNaN(inputValue)) {
      toastr.warning($("#youShouldEnterNumericValueForReceivedQuantity").val());
      return;
    }

    inputValue = inputValue.toFixed(quantDecPolicy);

    purchaseDetail = {
      ...purchaseDetail,
      receivedQuantity: inputValue
    }
  }

  if (selectedInputToUpdate === poCostText) {
    inputValue = parseFloat($("#modalInputId").val());
    if (inputValue) {
      if (inputValue < smallDoubleLowerLimit || inputValue > smallDoubleLimit) {
        toastr.warning($("#costShouldBeIn").val());
        return;
      }

      let vendorUnitCostPercentage = await getVendorUnitCostPercentage(inputValue);
      if (vendorUnitCostPercentage && vendorUnitCostPercentage.vendorUnitCostPercentage > 50) {
        toastr.warning($("#youCantAllowCostPerUnitTobeMoreThat50Percent").val().replace("{0}", vendorUnitCostPercentage.vendorUnitCost));
        return;
      }
    }
    else if (isNaN(inputValue)) {
      toastr.warning($("#youShouldEnterAValueForCost").val());
      return;
    }
    else {
      toastr.warning($("#costCanNotBeZero").val());
      return;
    }

    purchaseDetail = {
      ...purchaseDetail,
      poCost: inputValue
    }
  }

  updatedCellData = inputValue;

  if (selectedInputToUpdate === applyGSTText) {
    purchaseDetail = {
      ...purchaseDetail,
      applyGST: ($("#modalInputSelectId").val() === "true")
    }

    updatedCellData = $("#modalInputSelectId").val();
  }

  let data = {
    purchaseDetail,
    vendorId: selectedPurchaseDetailData.productVendor
  }

  let url = "PurchaseDetail/UpdatePurchaseDetail";

  await ajaxPostAsync(url, data)
    .then(async () => {
      await loadPurchaseDetailsByPurchaseNumber(false);
      const result = purchaseDetails.find(item => item.purchaseNo === selectedPurchaseDetailData.purchaseNo && item.lineNumber === selectedPurchaseDetailData.lineNumber);
      updatePurchaseDetailRowData(result);

      selectedInputToUpdate = null;
      await purchaseHeaderModalSaveButtonClicked();

      $("#purchaseDetailInputModal").modal('hide');
    })
    .catch(toastrError);
}

let editHeaderDataClick = (selectedInput) => {
  $("#purchaseHeaderModalInputId").val("");
  $("#purchaseHeaderModalInputId").show();
  $("#purchaseHeaderModalInputTextAreaId").hide();
  $("#purchaseHeaderModalInputDateId").hide();
  $("#purchaseHeaderInputModal").modal('show');

  if (selectedInput === buyerText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.buyer);
    $("#purchaseHeaderModalLabelId").html($("#buyerText").val());
  }

  if (selectedInput === shipViaText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.shipVia);
    $("#purchaseHeaderModalLabelId").html($("#shipViaText").val());
  }

  if (selectedInput === fobText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.fob);
    $("#purchaseHeaderModalLabelId").html($("#fobText").val());
  }

  if (selectedInput === freightText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.freight);
    $("#purchaseHeaderModalLabelId").html($("#freightText").val());
  }

  if (selectedInput === customerAccountText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.custAccount);
    $("#purchaseHeaderModalLabelId").html($("#customerAccountText").val());
  }

  if (selectedInput === poDateText) {
    $("#purchaseHeaderModalLabelId").html($("#poDateText").val());
    $("#purchaseHeaderModalInputId").hide();
    $("#purchaseHeaderModalInputTextAreaId").hide();
    $("#purchaseHeaderModalInputDateId").val(purchaseHeaderData.purchaseDate.split('T')[0]);
    purchaseHeaderModalInputDateTmpData = $("#purchaseHeaderModalInputDateId").val();
    $("#purchaseHeaderModalInputDateId").show();
  }

  if (selectedInput === toBeDeliveredText) {
    $("#purchaseHeaderModalLabelId").html($("#toBeDeliveredText").val());
    $("#purchaseHeaderModalInputId").hide();
    $("#purchaseHeaderModalInputTextAreaId").hide();
    $("#purchaseHeaderModalInputDateId").val(purchaseHeaderData.deliveryDate.split('T')[0]);
    purchaseHeaderModalInputDateTmpData = $("#purchaseHeaderModalInputDateId").val();
    $("#purchaseHeaderModalInputDateId").show();
  }

  if (selectedInput === dateReceivedText) {
    $("#purchaseHeaderModalLabelId").html($("#dateReceivedText").val());
    $("#purchaseHeaderModalInputId").hide();
    $("#purchaseHeaderModalInputTextAreaId").hide();
    $("#purchaseHeaderModalInputDateId").val(purchaseHeaderData.receivedDate.split('T')[0]);
    purchaseHeaderModalInputDateTmpData = $("#purchaseHeaderModalInputDateId").val();
    $("#purchaseHeaderModalInputDateId").show();
  }

  if (selectedInput === invoiceNumberText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.invoiceNo);
    $("#purchaseHeaderModalLabelId").html($("#invoiceNumberText").val());
  }

  if (selectedInput === termsText) {
    $("#purchaseHeaderModalInputId").val(purchaseHeaderData.terms);
    $("#purchaseHeaderModalLabelId").html($("#termsText").val());
  }

  if (selectedInput === notesText) {
    $("#purchaseHeaderModalLabelId").html($("#notesText").val());
    $("#purchaseHeaderModalInputId").hide();
    $("#purchaseHeaderModalInputTextAreaId").show();
    $("#purchaseHeaderModalInputTextAreaId").val(purchaseHeaderData.notes);
    $("#purchaseHeaderModalInputDateId").hide();
  }

  selectedInputToUpdate = selectedInput;
}

let purchaseHeaderModalSaveButtonClicked = async () => {
  let purchaseHeader = {
    purchaseNo: selectedPurchaseNo,
  };

  let inputValue = $("#purchaseHeaderModalInputId").val();

  if (selectedInputToUpdate === buyerText) {
    if (inputValue.length > 15) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#buyerText").val()).replace("{1}", "15"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      buyer: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === shipViaText) {
    if (inputValue.length >= 15) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#shipViaText").val()).replace("{1}", "15"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      shipVia: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === fobText) {
    if (inputValue.length > 15) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#fobText").val()).replace("{1}", "15"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      fob: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === freightText) {
    if (inputValue.length > 15) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#freightText").val()).replace("{1}", "15"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      freight: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === customerAccountText) {
    if (inputValue.length > 10) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#customerAccountText").val()).replace("{1}", "10"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      custAccount: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === poDateText) {
    purchaseHeader = {
      ...purchaseHeader,
      purchaseDate: $("#purchaseHeaderModalInputDateId").val(),
    }
  }

  if (selectedInputToUpdate === toBeDeliveredText) {
    purchaseHeader = {
      ...purchaseHeader,
      deliveryDate: $("#purchaseHeaderModalInputDateId").val(),
    }
  }

  if (selectedInputToUpdate === dateReceivedText) {
    purchaseHeader = {
      ...purchaseHeader,
      receivedDate: $("#purchaseHeaderModalInputDateId").val(),
    }
  }

  if (selectedInputToUpdate === invoiceNumberText) {
    if (inputValue.length > 20) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#invoiceNumberText").val()).replace("{1}", "20"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      invoiceNo: $("#purchaseHeaderModalInputId").val(),
      terms: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === termsText) {
    if (inputValue.length > 20) {
      toastr.warning($("#cannotBeMoreThanChar").val().replace("{0}", $("#termsText").val()).replace("{1}", "20"));
      return;
    }

    purchaseHeader = {
      ...purchaseHeader,
      invoiceNo: $("#purchaseHeaderModalInputId").val(),
      terms: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === notesText) {
    purchaseHeader = {
      ...purchaseHeader,
      Notes: $("#purchaseHeaderModalInputTextAreaId").val(),
    }
  }

  if (selectedInputToUpdate === subTotalText) {
    purchaseHeader = {
      ...purchaseHeader,
      subTotal: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === surchargeDiscountText) {
    purchaseHeader = {
      ...purchaseHeader,
      surChargeTotal: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === taxTotalText) {
    purchaseHeader = {
      ...purchaseHeader,
      totalGST: $("#purchaseHeaderModalInputId").val(),
    }
  }

  if (selectedInputToUpdate === totalText) {
    purchaseHeader = {
      ...purchaseHeader,
      purchaseTotal: $("#purchaseHeaderModalInputId").val(),
    }
  }

  let data = {
    purchaseHeader,
    vendorId: selectedVendorId,
  }

  let url = "PurchaseDetail/UpdatePurchaseHeader";

  await ajaxPostAsync(url, data)
    .then(async () => {
      await loadPurchaseHeader();
      $("#purchaseHeaderInputModal").modal('hide');
    })
    .catch(toastrError);
}

let goToPurchasingAndReceivingPage = () => {
  const url = `/PurchasingAndReceiving?poStatus=${selectedPOStatus}&selectedPurchaseNo=${selectedPurchaseNo}`;

  if (policies.popdFilePolicy && !$("#invoiceNumberInputId").val()) {
    let title = $('#invoiceText').val();
    let message = $('#noInvoiceEnteredDoYouWantToUseTheDefault').val();
    showConfirmModal(title, message).then(async (result) => {
      if (result === true) {
        selectedInputToUpdate = invoiceNumberText;
        $("#purchaseHeaderModalInputId").val(`72${purchaseHeaderData.storeID.slice(-2)}-${selectedPurchaseNo}`); // 72 is hard coded in VB code
        await purchaseHeaderModalSaveButtonClicked();
        safelyNavigateTo(url);
      }
    });
  }
  else {
    safelyNavigateTo(url);
  }
}

let inputSurchargeModalApplyButtonClicked = async () => {
  let discountBy = $('input[name="by"]:checked').val();
  let surchargeDiscount = $('#surchargeDiscountModalInputId').val();
  if (!surchargeDiscount) {
    toastr.warning($("#youShouldEnterNumericValueForSurchargeDiscount").val());
    return;
  }
  else if (isNaN(surchargeDiscount)) {
    toastr.warning($("#invalidNumberWarningMessage").val());
    return;
  }

  let data = {
    purchaseNo: selectedPurchaseNo,
    surChargeTotalDiscount: surchargeDiscount,
    surChargeTotalDiscountType: discountBy,
    subTotal: purchaseHeaderData.subTotal,
    totalGST: purchaseHeaderData.totalGST
  }

  let url = "PurchaseDetail/UpdatePurchaseHeaderSurChargeDiscount";

  await ajaxPostAsync(url, data)
    .then(() => {
      loadPurchaseHeader();
      $("#inputSurchargeModal").modal('hide');
    })
    .catch(toastrError);
}

let inputSurchargeButtonClicked = () => {
  $('#surchargeDiscountModalInputId').val("");
  $('#byDollar').prop("checked", true);
  $("#inputSurchargeModal").modal("show");
}

$("#surchargeDiscountModalInputId").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 12) return false;
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

let partNumberOptionChnaged = async (partNumber) => {
  $("#editMinMaxModalChangeButtonId").prop("disabled", partNumber ? false : true);

  if (partNumber) {
    await loadVendorPartFromVendorPartStockBrByPartNumber(partNumber);
  }
  else {
    makeEmptyEditMinMaxModal();
  }
}

let renderVendorPartModalData = vendorPartData => {
  $("#reorderPointModalInputId").val(vendorPartData.reOrderQuantity);
  $("#reorderMaximumModalInputId").val(vendorPartData.maximumQuantity);
  $("#quantityInStockModalInputId").val(vendorPartData.inStock);
  let selectedVendorPartId = $("#partNumberModalSelectInputId").val();
  let selectedVondorPartDescription = $(`#partNumberModalSelectInputId option[value=${selectedVendorPartId}]`).text().split(' - ')[1];
  $("#partNumberModalData").html(`${vendorPartData.stockCode} - ${selectedVondorPartDescription}`);
}

let loadVendorPartsFromVendorPartStockMstAndStockBr = async () => {
  let url = "PurchaseDetail/GetVendorPartsFromVendorPartStockMstAndStockBr";

  await ajaxGetAsync(url)
    .then(_vendorParts => {
      if (selectedPurchaseDetailData && selectedPurchaseDetailData.partNumber) {
        $("#partNumberModalSelectInputId").val(selectedPurchaseDetailData.partNumber);
        $("#editMinMaxModalChangeButtonId").prop("disabled", false);
      }
    })
    .catch(toastrError);
}

let loadVendorPartFromVendorPartStockBrByPartNumber = async (partNumber) => {
  let url = `PurchaseDetail/GetVendorPartFromVendorPartStockBrByPartNumber?partNo=${partNumber}`;

  await ajaxGetAsync(url)
    .then(vendorPart => renderVendorPartModalData(vendorPart))
    .catch(toastrError);
}

let makeEmptyEditMinMaxModal = () => {
  $("#partNumberModalSelectInputId").val("");
  $("#reorderPointModalInputId").val("");
  $("#reorderMaximumModalInputId").val("");
  $("#quantityInStockModalInputId").val("");
  $("#partNumberModalData").html("");
  $("#editMinMaxModalChangeButtonId").prop("disabled", true);
}

let editMinMaxButtonClicked = async () => {
  makeEmptyEditMinMaxModal();
  $('#editMinMaxModal').modal("show");
  await loadVendorPartsFromVendorPartStockMstAndStockBr();
  if (selectedPurchaseDetailData && selectedPurchaseDetailData.partNumber) {
    await loadVendorPartFromVendorPartStockBrByPartNumber(selectedPurchaseDetailData.partNumber);
  }
}

let editMinMaxModalChangeButtonClicked = async () => {
  let url = "PurchaseDetail/UpdateStockBranchReorderAndMaximumQuantity";
  let partNumber = $("#partNumberModalSelectInputId").val();
  let maxQuantity = $("#reorderMaximumModalInputId").val();
  let reOrderQuantity = $("#reorderPointModalInputId").val();

  if (!partNumber) {
    toastr.warning($("#pleaseSelectPartNumber").val());
    return;
  }

  if (!maxQuantity) {
    toastr.warning($("#youShouldEnterNumericValueForReorderMaximum").val());
    return;
  }

  if (!reOrderQuantity) {
    toastr.warning($("#youShouldEnterNumericValueForReorderPoint").val());
    return;
  }

  let data = {
    stockCode: $("#partNumberModalData").text().split(" - ")[0],
    branch: purchaseHeaderData.storeID,
    maxQty: maxQuantity,
    reOrderQty: reOrderQuantity
  }

  await ajaxPostAsync(url, data)
    .then(() => $("#editMinMaxModal").modal('hide'))
    .catch(toastrError);
}

$("#reorderMaximumModalInputId").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 12) return false;
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

$("#reorderPointModalInputId").on('keypress', function (event) {
  const value = $(this).val();
  if (value.length > 12) return false;
  return onlyDigitDecimalPlusMinusKeyPress(event);
});

let addTaxButtonClicked = async () => {
  let data = {
    purchaseDetails,
    costDec: costDecPolicy,
    purchaseDate: purchaseHeaderData.purchaseDate
  }

  let url = "PurchaseDetail/AddTaxToPurchaseDetail";

  await ajaxPostAsync(url, data)
    .then(async () => {
      await loadPurchaseDetailsByPurchaseNumber();
      await loadPurchaseHeader();

      if (purchaseHeaderData.totalGST !== 0) {
        toastr.success($("#taxesSuccessfullyAdded").val());
      }
    })
    .catch(toastrError);
}

let deleteTaxButtonClicked = async () => {
  let purchaseDetailsForDeleteTax = purchaseDetails;
  purchaseDetailsForDeleteTax.forEach(purchaseDetail => {
    purchaseDetail.gst = 0;
    purchaseDetail.applyGST = false;
  });

  let data = { purchaseDetails: purchaseDetailsForDeleteTax };
  let url = `PurchaseDetail/UpdatePurchaseDetailGSTByPurchaseNO`;

  await ajaxPostAsync(url, data)
    .then(async () => {
      await loadPurchaseDetailsByPurchaseNumber();
      await loadPurchaseHeader();
      toastr.success($("#taxesSuccessfullyDeleted").val());
    })
    .catch(toastrError);
}

$('#purchaseHeaderModalInputDateId').on('change', function () {
  const selectedDate = $(this).val();
  if (!selectedDate) {
    $("#purchaseHeaderModalInputDateId").val(purchaseHeaderModalInputDateTmpData);
  }
  purchaseHeaderModalInputDateTmpData = $("#purchaseHeaderModalInputDateId").val();
});
