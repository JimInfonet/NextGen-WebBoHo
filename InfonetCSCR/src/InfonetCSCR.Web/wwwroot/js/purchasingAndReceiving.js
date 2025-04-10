let purchaseHeaderDatatable;
let purchaseHeaders = [];
let selectedPO = null;
let selectedPOStatus = "O";
let previouslySelectedPOStatus = $("#selectedPoStatus").val();
let vendors = [];
let selectedPurchaseNo;
let previouslySelectedPurchaseNo = $("#selectedPurchaseNo").val();
let shiftDate = null;
let policies;
let selectedShiftDate = "";
let selectedShiftID = "";
let selectedRowIndex = 0;

$(async function () {
  $("#post-spinner").hide();

  if (previouslySelectedPOStatus) {
    selectedPOStatus = previouslySelectedPOStatus;
    $("#poStatus").val(selectedPOStatus);
    await poStatusChanged(selectedPOStatus);
    selectedRowIndex = purchaseHeaders.findIndex(row => row.purchaseNo === parseInt(previouslySelectedPurchaseNo));
    selectPurchaseHeaderTableRowByIndex(selectedRowIndex);
  }
  else {
    await loadPurchaseHeaders(selectedPOStatus, 1);
  }

  await renderVendorOptions(2);
  await loadPurchaseAndReceivingPolicies();

  $('#generatePurchaseOrderModal').on('shown.bs.modal', function () {
    //Without animation, changing the scrollbar to the top seems weird.
    $('#vendorOptions').animate({ scrollTop: 0 }, 'fast');
  });

  $("#spinner").hide();
  window.history.replaceState(null, null, "/PurchasingAndReceiving");
});

let loadPurchaseHeaders = async (poStatus, columnIndex) => {
  let url = `PurchasingAndReceiving/GetPurchaseHeadersByPOStatus?poStatus=${poStatus}&columnIndex=${columnIndex}`;

  await ajaxGetAsync(url)
    .then(async _purchaseHeaders => {
      purchaseHeaders = _purchaseHeaders;

      $("#createButtonId").prop("disabled", (poStatus !== 'O'));
      $("#removeButtonId").prop("disabled", (poStatus !== 'O' || purchaseHeaders.length === 0));
      $("#detailButtonId").prop("disabled", (purchaseHeaders.length === 0));
      $("#btn-post").prop("disabled", (purchaseHeaders.length === 0));

      if (purchaseHeaders.length > 0 && !selectedPurchaseNo) {
        selectedPurchaseNo = purchaseHeaders[0].purchaseNo;
      }

      await createPurchaseHeaderDatatable();
    })
    .catch(toastrError);
}

let createPurchaseHeaderDatatable = async () => {
  if (purchaseHeaderDatatable) {
    purchaseHeaderDatatable.state.clear();
    purchaseHeaderDatatable.clear();
    purchaseHeaderDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = purchaseHeaders;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];

  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "purchaseNo", },
    { "data": "accountNo", },
    { "data": "vendorName", },
    {
      "data": "purchaseDate",
      "render": function (data, type, row) {
        if (type === "display") {
          let dateObject = new Date(data);
          const formattedDate = `${(dateObject.getMonth() + 1).toString().padStart(2, '0')}/${dateObject.getDate().toString().padStart(2, '0')}/${dateObject.getFullYear()}`;
          return formattedDate;
        }
        return data;
      },
    },
    {
      "data": "paid",
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
    { "data": "lines", },
    { "data": "shiftID", },
    { "data": "shiftNum", },
    {
      "data": "emailed",
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
  ];

  purchaseHeaderDatatable = await createDatatableWithSearching('purchasingAndReceiving-grid', props);

  $(`#purchasingAndReceiving-grid tbody`)
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        purchaseHeaderDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');

        selectedPO = $(this).find("td:eq(0)").text();
        if (selectedPOStatus === "O" && parseInt(selectedPO) > 0) {
          $('#btn-send').prop('disabled', false);
        }
        else {
          $('#btn-send').prop('disabled', true);
        }

        selectedPurchaseNo = $(this).find("td:first")[0].innerText;
      }
    });

  if (purchaseHeaders.length > 0) {
    selectedPO = $(`#purchasingAndReceiving-grid tbody td:eq(0)`).text();
    $('#btn-print').prop('disabled', false);
    if (selectedPOStatus === "O" && parseInt(selectedPO) > 0) {
      $('#btn-send').prop('disabled', false);
    }
    else {
      $('#btn-send').prop('disabled', true);
    }
    $(`#purchasingAndReceiving-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }
  else {
    $('#btn-send').prop('disabled', true);
    $('#btn-print').prop('disabled', true);
  }
}

let selectPurchaseHeaderTableRowByIndex = (rowIndex) => {
  const selectedRow = $(`#purchasingAndReceiving-grid tbody tr:eq(${rowIndex})`);

  if (!selectedRow.hasClass('selected-grid-row')) {
    purchaseHeaderDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
    selectedRow.addClass('selected-grid-row');

    selectedPO = selectedRow.find("td:eq(0)").text();
    if (selectedPOStatus === "O" && parseInt(selectedPO) > 0) {
      $('#btn-send').prop('disabled', false);
    }
    else {
      $('#btn-send').prop('disabled', true);
    }

    selectedPurchaseNo = selectedRow.find("td:first")[0].innerText;
  }

}

$(`#purchasingAndReceiving-grid thead th`).each(function (index) {
  let clickedColumnIndex = index + 1;

  $(this).on('click', async function () {
    await loadPurchaseHeaders($("#poStatus").val(), clickedColumnIndex);
  });
});

let poStatusChanged = async (poStatus) => {
  selectedPOStatus = $("#poStatus").val();
  if (selectedPOStatus === "O" && parseInt(selectedPO) > 0) {
    $('#btn-send').prop('disabled', false);
  }
  else {
    $('#btn-send').prop('disabled', true);
  }

  selectedPurchaseNo = null;
  await loadPurchaseHeaders(poStatus, 1);
}

$("#btn-send").on("click", () => {
  document.getElementsByName('select-send-type')[2].checked = true;
  $("#destination-for-po-title").html($("#destinationForPO").val() + " " + selectedPO);
  $("#destination-for-po-modal").modal("show");
})

$("#btn-done").on("click", async () => {
  if (selectedPO == null) {
    toastr.warning($("#pleaseSelectAnItem").val());
    return;
  }

  let sendType = document.getElementsByName('select-send-type');

  if (sendType[0].checked) {
    const isEmailed = purchaseHeaders.filter(x => x.purchaseNo == selectedPO)[0].emailed;
    if (isEmailed === false) {
      let url = `PurchasingAndReceiving/GetVendorByPurchaseNo?purchaseNo=${selectedPO}`;
      await ajaxGetAsync(url)
        .then(async (vendors) => {
          if (vendors) {
            const title = $("#readyToEmailThePOToVendor").val();
            let message = $("#areYouSureYouWantToEmailThisPO").val();
            const selectedPurchaseHeader = purchaseHeaders.filter(x => x.purchaseNo === parseInt(selectedPO))[0];
            message = message.replace("_PO_", `${selectedPurchaseHeader.accountNo}-${selectedPurchaseHeader.vendorName}`);

            showConfirmModal(title, message).then(async (result) => {
              if (result === true) {
                url = `PurchasingAndReceiving/GetPOFileData?purchaseNo=${selectedPO}`;
                await ajaxGetAsync(url)
                  .then(async poFileData => {
                    if (poFileData?.purchaseDetails?.length === 0) {
                      toastr.warning($("#fileIsNotCreated").val());
                      return;
                    }

                    const fileContent = getFormattedPOFileContent(poFileData);
                    const fileName = `PO${poFileData.purchaseHeader.custAccount === null ? poFileData.purchaseHeader.accountNo : poFileData.purchaseHeader.custAccount}_${selectedPO}`;

                    url = `File/SaveFile?FileName=${fileName}&forceAutoImportExport=true`;

                    await ajaxPostAsync(url, fileContent)
                      .catch(() => toastr.error($("#anErrorHasOccurredWhileSavingFile").val()));

                    if (validateEmail(vendors.email)) {
                      const attachmentFilePath = `${$("#exportFolderPath").val()}\\${fileName}`;
                      const subject = `PO File ${fileName}`;
                      const body = `Attached is the latest PO File ${fileName}`;
                      const isMailSent = await sendMail(vendors.email, subject, body, attachmentFilePath);

                      if (isMailSent) {
                        const purchaseHaeder = {
                          emailed: true,
                          pOStatus: "S",
                          sendBy: $("#userCode").val(),
                          purchaseNo: parseInt(selectedPO)
                        }

                        await updatePurchaseHeader(purchaseHaeder);
                      }
                    }
                    else {
                      toastr.warning($("#invalidEmailAddress").val());
                    }
                  })
                  .catch(() => toastr.error($("#anErrorHasOccurred").val()));
              }
            });
          }
        })
        .catch(() => toastr.error($("#vendorAccountDoesNotExist").val()));
    }
    else {
      toastr.warning($("#emailAlreadySent").val());
    }
  }
  else if (sendType[1].checked) {
    let url = `PurchasingAndReceiving/GetPOFileData?purchaseNo=${selectedPO}`;

    await ajaxGetAsync(url)
      .then(async poFileData => {
        const fileContent = getFormattedPOFileContent(poFileData);
        const fileName = `PO${poFileData.purchaseHeader.custAccount === null ? poFileData.purchaseHeader.accountNo : poFileData.purchaseHeader.custAccount}_${selectedPO}`;

        if ($("#isAutoImportExport").val().toLowerCase() === "true") {
          url = `File/SaveFile?FileName=${fileName}`;

          await ajaxPostAsync(url, fileContent)
            .then(async () => {
              const purchaseHaeder = {
                emailed: false,
                pOStatus: "S",
                sendBy: $("#userCode").val(),
                purchaseNo: parseInt(selectedPO)
              }

              await updatePurchaseHeader(purchaseHaeder);
              toastr.success(`${$("#successfullySavedTo").val()} ${$("#exportFolderPath").val()}\\${fileName}`);
            })
            .catch(() => toastr.error($("#anErrorHasOccurredWhileSavingFile").val()));
        }
        else {
          const purchaseHaeder = {
            emailed: false,
            pOStatus: "S",
            sendBy: $("#userCode").val(),
            purchaseNo: parseInt(selectedPO)
          }

          await updatePurchaseHeader(purchaseHaeder);
          download(fileName, fileContent);
        }
      })
      .catch(() => toastr.error($("#emptyDataFile").val()));
  }
  else if (sendType[2].checked) {
    const filePath = $("#batchFilePath").val();
    let url = "File/ExeceuteBatchFile";

    await ajaxPostAsync(url, filePath)
      .then(async () => {
        toastr.success($("#successfullySentToFtp").val());

        const purchaseHaeder = {
          emailed: true,
          pOStatus: "S",
          sendBy: $("#userCode").val(),
          purchaseNo: parseInt(selectedPO)
        }

        await updatePurchaseHeader(purchaseHaeder);
      })
      .catch(() => toastr.error($("#anErrorHasOccurredWhileExecutingBatchFile").val()));
  }

  $("#destination-for-po-modal").modal("hide");
})

$("#btn-cancel").on("click", () => $("#destination-for-po-modal").modal("hide"));

let updatePurchaseHeader = async (purchaseHaeder) => {
  const url = "PurchasingAndReceiving/UpdatePurchaseHeader";

  await ajaxPostAsync(url, purchaseHaeder)
    .catch(() => toastr.error($("#anErrorHasOccurred").val()));

  await loadPurchaseHeaders($("#poStatus").val(), 1);
}

function validateEmail(email) {
  // Regular expression for email validation
  const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
  return emailPattern.test(email);
}

async function sendMail(email, subject, body, attachmentFilePath) {
  let url = `Mail/SendMail?recipientEmail=${email}&subject=${subject}&body=${body}&attachmentFilePath=${attachmentFilePath}`;
  $("#btn-send").prop('disabled', true)
    .html($("#_send").val() + '  <span class="spinner-border spinner-border-sm" role="status"></span>');

  return ajaxGetAsync(url)
    .then(() => {
      toastr.success(`${$("#successfullySentMailTo").val()} ${email}`);
      return true;
    })
    .catch(() => {
      toastr.error($("#anErrorHasOccurredWhileSendingMail").val());
      return false;
    })
    .finally(() => $("#btn-send").prop('disabled', false).html($("#_send").val()));
}

let getFormattedPOFileContent = (poFileData) => {
  const dateString = poFileData.purchaseHeader.purchaseDate;
  const date = new Date(dateString);
  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  const formattedDate = year + month + day;

  let formattedData = "";
  formattedData += `H,${formattedDate},${poFileData.purchaseHeader.purchaseNo},${poFileData.purchaseHeader.custAccount}\n`;
  for (let i = 0; i < poFileData?.purchaseDetails.length; i++) {
    formattedData += `D,` +
      `${poFileData?.purchaseDetails[i].lineNumber},` +
      `${poFileData?.purchaseDetails[i].orderedQuantity},` +
      `${poFileData?.purchaseDetails[i].partNumber},` +
      `${poFileData?.purchaseDetails[i].stockCode},` +
      `${(poFileData?.purchaseDetails[i].unitQuantity === 1 ? "EA" : "CS")}\n`;
  }
  return formattedData;
}

let createPurchasingAndReceivingButtonClicked = async () => {
  $("#blank").prop("checked", true);
  $("#name").prop("checked", true);
  // Column number = 2, to get active vendor order by name
  await renderVendorOptions(2);
  $("#generatePurchaseOrderModal").modal("show");
}

let renderVendorOptions = async (columnNumber) => {
  await loadVendors(columnNumber);

  const container = $("#vendorOptions");
  container.empty();

  vendors.forEach(vendor => {
    container.append(`
       <option value="${vendor.code} - ${vendor.name}">
          ${vendor.code} - ${vendor.name}
       </option>
    `)
  });

  container.find("option:first-child").prop("selected", true);
}

let loadVendors = async (columnNumber) => {
  let url = `PurchasingAndReceiving/GetActiveVendorsOrderByColumnNumber?columnNumber=${columnNumber}`;

  await ajaxGetAsync(url)
    .then(_vendors => vendors = _vendors)
    .catch(toastrError);
}

const radioButtons = document.querySelectorAll('[name="sortBy"]');

radioButtons.forEach(radioButton => {
  radioButton.addEventListener('change', (event) => {
    if (event.target.checked) {
      renderVendorOptions(event.target.value);
    }
  });
});

let deletePurchasingAndReceivingByPurchaseNo = async () => {
  let url = `PurchasingAndReceiving/DeletePurchasingAndReceivingByPurchaseNo?purchaseNo=${selectedPurchaseNo}`;

  await ajaxPostAsync(url, {})
    .then(async () => {
      selectedPurchaseNo = null;
      await loadPurchaseHeaders('O', 1);
    })
    .catch(toastrError);
}

let deletePurchasingAndReceivingButtonClicked = async () => {
  let title = $("#deletePurchaseOrder").val();
  let message = $("#youAreAboutToDeleteThePurchaseOrder").val().replace("{0}", selectedPurchaseNo);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      await deletePurchasingAndReceivingByPurchaseNo();
      toastr.success($("#purchaseOrderSuccessfullyDeleted").val());
    }
  });
}

let generatePurchasingAndReceiving = async () => {
  $("#spinner").show();
  let vendorData = $("#vendorOptions").val();
  let automatedRadioButton = document.getElementById('automated');

  let data = {
    VendorId: vendorData[0].split(' - ')[0],
    VendorName: vendorData[0].split(' - ')[1],
    UserCode: $("#userCode").val(),
    IsAutomated: (automatedRadioButton && automatedRadioButton.checked)
  };

  let url = "PurchasingAndReceiving/AddPurchasingAndReceiving";

  await ajaxPostAsync(url, data)
    .then(async () => {
      await loadPurchaseHeaders('O', 1);
      toastr.success($("#purchaseOrderSuccessfullyAdded").val());
      $("#generatePurchaseOrderModal").modal("hide");
    })
    .catch(toastrError);

  $("#spinner").hide();
}

$("#vendorOptions").on("change", function () {
  let selectedValues = $(this).val();

  if (!selectedValues || selectedValues.length === 0) {
    $(this).val($(this).find('option:first').val());
  } else if (selectedValues.length > 1) {
    $(this).val([selectedValues.pop()]);
  }
});


let onDetailButtonClicked = () => {
  const url = `PurchaseDetail?purchaseOrderNumber=${selectedPurchaseNo}`;
  safelyNavigateTo(url);
}

let displayPurchasingAndReceivingReport = () => {
  let windowName = "PurchasingAndReceivingReport";
  let width = 1000;
  let height = window.screen.height - 50;;
  let data = {};

  data.reportName = windowName;
  data.language = $('#language').val();
  data.reportWidth = "900px";
  data.reportHeight = window.screen.height - 150;
  data.purchaseNo = selectedPO;

  displayReport(windowName, width, height, data);
}

let purchasingAndReceivingPostButtonClicked = async () => {
  const selectedRow = purchaseHeaders.find(({ purchaseNo }) => purchaseNo == selectedPurchaseNo);
  shiftDate = selectedRow.purchaseDate.split("T")[0].replace(/-/g, "");
  await loadShiftIdOptions();
  await loadPODetail();

  document.getElementById("purchasingReceivingPostModalTitleId").innerHTML = $('#purchasingReceivingPostModalTitle').val() + ' ' + selectedPO;
  $("#purchasingAndReceivingPostModal").modal('show');
  $('.text-danger').prop('hidden', true);
}

let loadPODetail = async () => {
  let url = `PurchaseDetail/GetPurchaseHeadersByPurchaseNo?purchaseNo=${selectedPurchaseNo}`;
  await ajaxGetAsync(url)
    .then(async _purchaseHeader => {
      if (_purchaseHeader.shiftID) {
        let isCustom = true;
        if (_purchaseHeader.purchaseDate) {
          shiftDate = _purchaseHeader.purchaseDate.split("T")[0].replace(/-/g, "");
          isCustom = await checkIsShiftIdIsCustom(_purchaseHeader.shiftID, shiftDate);
        }

        if (isCustom) {
          $("#shift-id-input").val(`${_purchaseHeader.shiftID}`);
          await loadShiftNumberOptions(true);
        }
        else {
          $("#shift-id-input").val(`${_purchaseHeader.shiftID} - ${getFormattedDate(new Date(_purchaseHeader.purchaseDate))}`);
          await loadShiftNumberOptions(false);
        }
        $("#shift-number-select").val(_purchaseHeader.shiftNum.toString());
      }
      else {
        let isCustom = true;

        if (_purchaseHeader.purchaseDate) {
          shiftDate = _purchaseHeader.purchaseDate.split("T")[0].replace(/-/g, "");
          isCustom = await checkIsShiftIdIsCustom(null, shiftDate);
        }

        if (isCustom) {
          document.getElementById("shift-id-input").value = "";
          document.getElementById("shift-number-select").value = "";
          $("#shift-number-select").html(optionGeneratorDefault(document.getElementById("shiftNumber").value));
        }
      }

      if (_purchaseHeader.payType) {
        $("#payment-type-select").val(_purchaseHeader.payType);
      }
      else {
        $("#payment-type-select").val("CHARGE");
      }

      $("#discount").val(_purchaseHeader.surChargeTotal.toFixed(2))
      $("#gst-amount").val(_purchaseHeader.totalGST.toFixed(2))
      $("#sub-total").val(_purchaseHeader.subTotal.toFixed(2))
      $("#total").val(_purchaseHeader.purchaseTotal.toFixed(2))
    })
    .catch(toastrError);
}

let loadDataFromPurchaseHeaderController = async (url, data) => {
  return await $.ajax({
    type: 'GET',
    url: `/PurchasingAndReceiving/${url}`,
    dataType: 'JSON',
    data: data,
    success: function (result) {
      if (result === null) return;
      return result;
    }
  });
}

let checkIsShiftIdIsCustom = async (shiftId = null, shiftDate) => {
  let isCustom = true;
  await loadDataFromPurchaseHeaderController(`GetShiftIdAndDateByShiftDate?shiftDate=${shiftDate}`, {}).then((data) => {
    if (data.length > 0) {
      if (shiftId) {
        let dataExists = Object.values(data).some(item => item.shiftID === shiftId);

        if (dataExists) {
          isCustom = false;
        } else {
          isCustom = true;
        }
      }
      else {
        isCustom = false;
      }
    }
  });

  return isCustom;
}

let loadShiftIdOptions = async () => {
  $("#shift-id-select").html(optionGeneratorDefault(document.getElementById("shiftID").value));
  await loadDataFromPurchaseHeaderController(`GetShiftIdAndDateByShiftDate?shiftDate=${shiftDate}`, {}).then((data) => {
    if (data) {
      data.forEach(option => $("#shift-id-select").append(optionGeneratorForSingleElement(`${option.shiftID} - ${getFormattedDate(new Date(option.shiftDate))}`)));
    }
    if ($('#shift-id-select').children()[2]) {
      $($('#shift-id-select').children()[2]).attr('selected', true);
      shiftIDSelectChanged($('#shift-id-select').children()[2].value);
    }
  });
}

let shiftIdFormat = (e) => {
  return ((e.charCode >= 47 && e.charCode <= 57) || e.charCode === 45 || e.charCode === 32); //  '-' = 45, '/' = 47, ' ' = 32
}

let getShiftIdAndDateAndValidate = () => {
  let shiftIdAndDate = $("#shift-id-input").val().replaceAll(' ', '').split('-');
  let isValid = true;

  if (shiftIdAndDate.length == 1) {
    selectedShiftID = shiftIdAndDate;
    isValid = !isNaN(selectedShiftID) && selectedShiftID != "";
    selectedShiftDate = "";
  }
  else if (shiftIdAndDate.length == 2) {
    selectedShiftID = shiftIdAndDate[0];
    selectedShiftDate = convertMMDDYYYYToUtc(shiftIdAndDate[1]);
    isValid = !isNaN(selectedShiftID) && selectedShiftID != "";
  }
  else {
    selectedShiftID = shiftIdAndDate[0];
    selectedShiftDate = NaN;
    isValid = false;
  }

  if (parseInt(selectedShiftID) > 2147483647) {
    isValid = false;
  }

  return [selectedShiftID, selectedShiftDate, isValid];
}

let isShiftIdAndDateValid = () => {
  let getShiftIdAndDate = getShiftIdAndDateAndValidate();
  shiftDate = getShiftIdAndDate[1];
  let isValid = getShiftIdAndDate[2];
  let date = new Date(shiftDate);

  // shiftDate is optional
  let isShiftDateValid = selectedShiftDate === "" || ((date instanceof Date) && !isNaN(date));
  $('#validation-for-shift-id').prop('hidden', isValid && isShiftDateValid);

  return isValid && isShiftDateValid;
}

async function loadShiftNumberOptions(customInput = false) {
  if (customInput) {
    $($('#shift-id-select').children()[0]).removeAttr('selected');
    $($('#shift-id-select').children()[0]).attr('selected', true);
  }

  if (!isShiftIdAndDateValid()) {
    return;
  }

  $("#shift-number-select").html(optionGeneratorDefault(document.getElementById("shiftNumber").value));
  await loadDataFromPurchaseHeaderController(`GetShiftNumberByShiftDate?shiftDate=${selectedShiftDate}`, {}).then((data) => {
    if (data) {
      data.forEach(option => $("#shift-number-select").append(optionGeneratorForDifferentIdAndData(option.split(" - ")[0], option)));
    }

    if ($('#shift-number-select').children()[2]) {
      $($('#shift-number-select').children()[2]).attr('selected', true)
      shiftNumberSelectChanged($('#shift-number-select').val());
    }
  });
}

async function postForm() {
  const formData = preparePostFormData();
  let isValid = validatePostFormData(formData);
  if (isValid) {
    let title = $("#priceChange").val();
    let message = $("#priceChangeModalMessage").val();
    let costChange = false;

    if (policies.pChangeInPO) { // only if PO supports price change - Mr.Gas don't want automatic price change
      if (policies.defaultMargin) { // margin based
        if (policies.confirmPC) {
          // yes = Change the price based on new cost using the current margin( if margin is zero, cost and price will  be the same)
          // no = donot change the price, but reclculate the margin based on Cost and price
          costChange = await showConfirmModal(title, message);
        }
        else
          costChange = true; // No need for comnfirmation-Change the price based on new cost using the current margin
      }
      else
        costChange = false; // Do not change the price, but recalculate the margin
    }

    $("#purchasingAndReceivingPostModal").modal('hide');

    $("#btn-post").prop("disabled", true);
    $("#post-spinner").show();

    let url = `PurchasingAndReceiving/POPayment?costChange=${costChange}`;

    await ajaxPostAsync(url, formData)
      .then(async (transferPOFile) => {
        if ($("#isAutoImportExport").val().toLowerCase() === "true") {
          let url = "File/SaveFilePO";

          await ajaxPostAsync(url, transferPOFile)
            .then(() => toastr.success($("#fileCreatedSuccessfully").val()))
            .catch(() => toastr.error($("#anErrorHasOccurredWhileSavingFile").val()));
        }
        else {
          download(transferPOFile.headerFileName, transferPOFile.headerFileContent);
          if (transferPOFile.detailFileContent) {
            download(transferPOFile.detailFileName, transferPOFile.detailFileContent);
          }
        }
        selectedPurchaseNo = null;
        await loadPurchaseHeaders($("#poStatus").val(), 1);
      })
      .catch(() => toastr.error($("#anErrorHasOccurred").val()));

    $("#btn-post").prop("disabled", (purchaseHeaders.length === 0));
    $("#post-spinner").hide();
  }
}

function preparePostFormData() {
  let ShiftID = parseInt(selectedShiftID);
  let ShiftNumberString = document.getElementById("shift-number-select").value
  let ShiftNumber = ShiftNumberString;
  if (ShiftNumberString.includes('-')) {
    ShiftNumber = ShiftNumberString.substring(0, ShiftNumberString.indexOf("-") - 1);
  }

  let SubTotal = document.getElementById("sub-total");
  let Discount = document.getElementById("discount");
  let GSTAmount = document.getElementById("gst-amount");
  let Total = document.getElementById("total");
  let PaymentType = document.getElementById("payment-type-select");
  let IsPaid = document.getElementById("paid-select");

  const formData = {
    PurchaseNo: selectedPurchaseNo,
    ShiftID: ShiftID,
    ShiftNumber: ShiftNumber,
    SubTotal: SubTotal === null ? 0 : SubTotal.value === "" ? 0 : SubTotal.value,
    Discount: Discount === null ? 0 : Discount.value === "" ? 0 : Discount.value,
    GSTAmount: GSTAmount === null ? 0 : GSTAmount.value === "" ? 0 : GSTAmount.value,
    Total: Total === null ? 0 : Total.value === "" ? 0 : Total.value,
    PaymentType: PaymentType.value,
    IsPaid: IsPaid.value === "Yes"
  }

  return formData;
}

let validatePostFormData = (formData) => {
  if (!isShiftIdAndDateValid()) {
    $('#validation-for-shift-id').prop('hidden', false);
    return false;
  }

  if (!formData.ShiftNumber) {
    $('#validation-for-shift-number').prop('hidden', false);
    return false;
  }

  return true;
}

async function loadPurchaseAndReceivingPolicies() {
  const url = `PurchasingAndReceiving/GetPurchasingAndReceivingPolicies`;
  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

async function shiftIDSelectChanged(shiftID) {
  $("#shift-id-input").val(shiftID);
  await loadShiftNumberOptions();
}

function shiftNumberSelectChanged(shiftNumber) {
  $('#validation-for-shift-number').prop('hidden', shiftNumber);
}
