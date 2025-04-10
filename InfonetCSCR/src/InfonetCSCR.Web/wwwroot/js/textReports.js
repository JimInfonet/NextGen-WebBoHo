$(async function () {
  $('#textReportsList').on('change', handleTextReportsChange);
  $('#btnRunReport').on('click', await runReport);
  $('#btnPrintReport').on('click', await generateTextReportPDF);
});

function handleTextReportsChange() {
  $('#textReportViewer').empty();
  $('#textReportsFilterContainer').removeAttr("hidden");
  $('.text-reports-filter-fields').hide();
  $('#btnExportToExcel').hide();
  $('#textReportFooterButtonContainer button').prop('disabled', true);
  const selectedTextReport = $('#textReportsList').val();

  switch (selectedTextReport) {
    case 'T0001':
      $('#rowReadingDate').show();
      break;
    case 'T0002':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowTerminalID').show();
      break;
    case 'T0003':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartCard').show();
      $('#rowEndCard').show();
      break;
    case 'T0004':
      $('#rowSaleDate').show();
      break;
    case 'T0005':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartCustomer').show();
      $('#rowEndCustomer').show();
      break;
    case 'T0006':
      $('#rowSaleDate').show();
      break;
    case 'T0007':
      $('#rowSaleDate').show();
      $('#rowSummaryByGrade').show();
      break;
    case 'T0008':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartEmployee').show();
      $('#rowEndEmployee').show();
      break;
    case 'T0009':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartCard').show();
      $('#rowEndCard').show();
      $('#btnExportToExcel').show();
      break;
    case 'T0010':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      break;
    case 'T0011':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      break;
    case 'T0012':
      $('#rowStartDepartment').show();
      $('#rowEndDepartment').show();
      break;
    case 'T0013':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      break;
    case 'T0014':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartCustomer').show();
      $('#rowEndCustomer').show();
      break;
    case 'T0015':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      break;
    case 'T0016':
      $('#rowSaleDate').show();
      break;
    case 'T0017':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartEmployee').show();
      $('#rowEndEmployee').show();
      break;
    case 'T0018':
    case 'T0019':
      $('#rowStartDate').show();
      $('#rowEndDate').show();
      $('#rowStartCustomer').show();
      $('#rowEndCustomer').show();
      $('#btnExportToExcel').show();
      break;
  }
}

async function runReport() {
  $('#btnRunReport').prop('disabled', true);
  $('#textReportViewer').html(`<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>`);
  await generateTextReport();

  if ($('#textReportViewer').text()?.trim() !== "") {
    $('#textReportFooterButtonContainer button').prop('disabled', false);
  }
}

function getReportParams() {
  return {
    ReportName: $('#textReportsList').val(),
    StoreID: $('#storesList').val(),
    StoreName: $("#storesList option:selected").text(),
    ReadingDate: $('#readingDate').val(),
    StartDate: $('#startDate').val(),
    EndDate: $('#endDate').val(),
    SaleDate: $('#saleDate').val(),
    TerminalID: $('#terminalID').val(),
    StartCard: $('#startCard').val(),
    EndCard: $('#endCard').val(),
    StartCustomer: $('#startCustomer').val(),
    EndCustomer: $('#endCustomer').val(),
    StartEmployee: $('#startEmployee').val(),
    EndEmployee: $('#endEmployee').val(),
    StartDepartment: $('#startDepartment').val(),
    EndDepartment: $('#endDepartment').val(),
    IsSummaryByGrade: $('#chkInput').is(":checked")
  }
}

async function generateTextReport() {
  const url = `TextReports/GetTextReport`;
  let data = getReportParams();

  await ajaxGetAsyncAsString(url, data)
    .then(async (_reportContent) => {
      $('#textReportViewer').text(_reportContent);
      $('#btnRunReport').prop('disabled', false);
    })
    .catch(toastrError);
}

async function generateTextReportPDF() {
  const url = `TextReports/GenerateTextReportPDF`;
  const data = getReportParams(); // Get the report parameters
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
    link.download = 'TextReport.pdf'; // Set the filename for the download
    link.click(); // Trigger the download

    // Clean up by revoking the object URL after download
    URL.revokeObjectURL(link.href);
  } else {
    toastrError('Error generating PDF');
  }
}

function getFileConfig(reportName) {
  let fileConfig = {
    folderPath: '',
    fileName: '',
    fileType: ''
  }

  switch (reportName) {
    case "T0009":
      fileConfig.folderPath = `${$('#appFolderPath').val()}\\SavedReports`;
      fileConfig.fileName = 'T0009_';
      fileConfig.fileType = GENERAL_STRING_TABLE.FILE_TYPE.FLEET_CARD_SALE_REPORT;
      break;
    case 'T0018':
      fileConfig.folderPath = `${$('#appFolderPath').val()}\\OUT`;
      fileConfig.fileName = 'ARSALES';
      fileConfig.fileType = GENERAL_STRING_TABLE.FILE_TYPE.CUST_SALES_DETAIL_REPORT;
      break;
    case 'T0019':
      fileConfig.folderPath = `${$('#appFolderPath').val()}\\OUT`;
      fileConfig.fileName = 'CUSTSALES';
      fileConfig.fileType = GENERAL_STRING_TABLE.FILE_TYPE.CUST_SALES_DETAIL_REPORT;
      break;
  }

  return fileConfig;
}

async function generateExcelReport() {
  const url = `ExcelReports/ConvertTextReportToExcel`;
  let data = getReportParams();

  startOrEndLoader('btnExportToExcel', $('#lclExportToExcel').val(), true);

  await ajaxGetAsyncAsString(url, data)
    .then(async (_reportContent) => {
      const today = new Date();
      const year = today.getFullYear() % 100;
      const month = String(today.getMonth() + 1).padStart(2, '0'); // Month is zero-based
      const day = String(today.getDate()).padStart(2, '0');
      const formattedDate = `${month}${day}${year}`;
      let fileConfig = getFileConfig(data.ReportName);
      fileConfig.fileName = `${fileConfig.fileName}${formattedDate}`;

      if (data.ReportName !== 'T0009') {
        fileConfig.fileName = `${fileConfig.fileName}.${data.StoreID}`;
      }

      fileConfig.fileName = `${fileConfig.fileName}.xlsx`;

      await exportFileBasedOnConfig(fileConfig.folderPath, fileConfig.fileName, _reportContent, fileConfig.fileType);
    })
    .catch(toastrError)
    .finally(() => startOrEndLoader('btnExportToExcel', $('#lclExportToExcel').val(), false));
}

function generatePDF() {
  let pdfDefinition = createPdfDefinition();
  pdfDefinition.styles.preformatted.fontSize = getFontSize();

  pdfDefinition.content.push({
    lineHeight: 1.5,
    text: $('#textReportViewer').text(),
    preserveLeadingSpaces: true,
    preserveTrailingSpaces: true,
    style: 'preformatted'
  });

  printWithPdfMake(pdfDefinition);
}

function getFontSize() {
  switch ($('#textReportsList').val().toUpperCase()) {
    case 'T0002':
    case 'T0009':
    case 'T0010':
    case 'T0012':
    case 'T0018':
    case 'T0019':
      return 5;
    case 'T0003':
    case 'T0005':
    case 'T0008':
    case 'T0011':
    case 'T0015':
    case 'T0016':
      return 6;
    case 'T0007':
    case 'T0014':
    case 'T0017':
      return 8;
    case 'T0001':
    case 'T0006':
    case 'T0013':
      return 9;
    case 'T0004':
      return 10;
    default:
      return 6;
  }
}

async function emailTextReport() {
  const recipientEmail = $('#recipientEmail').val();

  if (!recipientEmail || recipientEmail === "") {
    toastr.warning($("#lclInvalidRecipientEmail").val());
    return false;
  }

  const attachmentName = `${$('#textReportsList').val()}.txt`;
  const storeID = $('#storesList').val();
  let storeName = $('#storesList').text().trim();
  storeName = storeName.substring(storeName.indexOf('-') + 1);
  let url = `Mail/EmailTextReport`;
  let data = {
    RecipientEmail: recipientEmail,
    Subject: `Report File ${attachmentName} ${storeID} ${storeName}`,
    Body: `Attached is the report file ${attachmentName} from ${storeName}.`,
    AttachmentName: attachmentName,
    AttachmentContent: $('#textReportViewer').text()
  };

  startOrEndLoader('btnEmailReport', $('#lclEmail').val(), true);

  return ajaxPostAsync(url, data)
    .then(() => {
      toastr.success(`${$("#lclSuccessfullySentMailTo").val()} ${recipientEmail}`);
    })
    .catch(error => { toastr.error(error.responseText); })
    .finally(() => startOrEndLoader('btnEmailReport', $('#lclEmail').val(), false));
}
