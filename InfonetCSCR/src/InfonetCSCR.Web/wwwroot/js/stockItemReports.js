let companyName;

$(async function () {
  document.getElementsByName('SelectReport')[0].checked = true;
  let today = new Date();
  let startDate = document.getElementById('startDate');
  let endDate = document.getElementById('endDate');
  let iFrame = document.getElementById('IframeReportViewer');

  iFrame.addEventListener("load", function () {
    let loadReportBtnText = document.getElementById('loadReportBtnText').value;
    document.getElementById("loadReportButton").disabled = false;
    document.getElementById("loadReportButton").innerText = loadReportBtnText;
  });

  startDate.addEventListener('input', function () {
    if (!startDate.value) {
      startDate.value = new Date().toISOString().substr(0, 10);
    }
  });

  endDate.addEventListener('input', function () {
    if (!endDate.value) {
      endDate.value = new Date().toISOString().substr(0, 10);
    }
  });

  startDate.value = today.toISOString().substr(0, 10);
  endDate.value = today.toISOString().substr(0, 10);
  companyName = await getCompanyName();
});

$("#startDate").on('keypress', function (event) {
  let date = $(this).val();
  let year = date.split('-')[0];

  if (Number(year) > 999) {
    year = new Date().toISOString().substr(0, 4);
    let newDate = year + date.substr(4, 10);
    $(this).val(newDate);
  }
});

$("#endDate").on('keypress', function (event) {
  let date = $(this).val();
  let year = date.split('-')[0];

  if (Number(year) > 999) {
    year = new Date().toISOString().substr(0, 4);
    let newDate = year + date.substr(4, 10);
    $(this).val(newDate);
  }
});

function displayStockItemReport() {
  let stockCode = document.getElementById('stockCode').value;
  let stockDescription = document.getElementById('stockDescription').value;
  let reports = document.getElementsByName('SelectReport');
  let startDate = !document.getElementById('startDate').value ? new Date().toLocaleDateString() : document.getElementById('startDate').value;
  let endDate = !document.getElementById('endDate').value ? new Date().toLocaleDateString() : document.getElementById('endDate').value;
  if (new Date(startDate) > new Date(endDate)) {
    let message = document.getElementById("endDateAheadOfStartDate").value;
    toastr.warning(message);
    return;
  }
  let iFrame = document.getElementById('IframeReportViewer');
  let language = document.getElementById('language').value;
  let reportName;

  iFrame.height = "1250px"
  let url = "/stockItemReports/RefreshToken";

  if (reports[0].checked) {
    reportName = "StockItemHistoryReport"
  }
  else if (reports[1].checked) {
    reportName = "SalesAnalysisByProductReport";
  }
  else {
    reportName = "VendorPurchasesByProductReport";
  }

  $.ajax({
    type: "GET",
    dataType: 'json',
    url: url,

    success: function (result) {
      if (result === null) return;

      result = {};
      let iFrame = document.getElementById('IframeReportViewer');
      result.reportName = reportName;
      result.stockCode = stockCode;
      result.stockDescription = stockDescription;
      result.language = language;
      result.startDate = startDate;
      result.endDate = endDate;
      result.companyName = companyName;
      iFrame.contentWindow.postMessage(result, $("#_reportBaseUrl").val());
      iFrame.removeAttribute("hidden");
    },
    error: function () {
      let message = document.getElementById("errorMessage").value;
      toastr.error(message);
    }
  })
}

function loadReport() {
  let url = document.getElementById("reportBaseUrl").value;
  n = window.open(url, '_blank', 'toolbar=0,location=0,directories=0,status=1,menubar=0,titlebar=0,scrollbars=1,resizable=1');
  if (n == null) {
    return true;
  }
  return false;
}

let ShowDetails = (year, month, method) => {
  let stockCode = document.getElementById("stockCode").value;
  let url = `/stockItemReports/${method}?stockCode=${stockCode}&year=${year}&month=${month}`;

  $.ajax({
    type: "GET",
    dataType: 'json',
    url: url,

    success: function (result) {
      let DetailsSection = document.getElementById("stockDetails");
      if (result.length !== 0) {
        //clear detailsData if available
        const elements = document.getElementsByClassName("detailsData");
        while (elements.length > 0) {
          elements[0].parentNode.removeChild(elements[0]);
        }

        DetailsSection.hidden = false;
        let labelTemplate = $('<label class="col text-end"></label>');
        $('#modal').modal('toggle')
        for (let i = 0; i < result.length; i++) {
          let divContainer = $('<div class="border border-1 d-flex flex-row tableData detailsData"></div>');
          let tableColumn = labelTemplate.clone();
          tableColumn.append(result[i].date);
          divContainer.append(tableColumn[0]);

          if (method === "GetSaleDetails") {
            tableColumn = labelTemplate.clone();
            tableColumn.append(result[i].saleNo);
            divContainer.append(tableColumn[0]);

            tableColumn = labelTemplate.clone();
            tableColumn.append(result[i].quantity);
            divContainer.append(tableColumn[0]);

            document.getElementById("purchaseModalTH").hidden = true;
            document.getElementById("saleModalTH").hidden = false;
          }
          else {
            tableColumn = labelTemplate.clone();
            tableColumn.append(result[i].purchaseNo);
            divContainer.append(tableColumn[0]);

            tableColumn = labelTemplate.clone();
            tableColumn.append(result[i].receivedQuantity);
            divContainer.append(tableColumn[0]);

            document.getElementById("purchaseModalTH").hidden = false;
            document.getElementById("saleModalTH").hidden = true;
          }
          tableColumn = labelTemplate.clone();
          tableColumn.append(result[i].amount);
          divContainer.append(tableColumn[0]);

          DetailsSection.append(divContainer[0]);
        }
      }
      else {
        DetailsSection.hidden = true;
      }
    },
    error: function () {
      let message = document.getElementById("errorMessage").value;
      toastr.error(message);
    }
  })
}
