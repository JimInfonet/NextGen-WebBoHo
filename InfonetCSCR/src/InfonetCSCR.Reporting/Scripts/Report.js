var loading = false;
var reportName;

window.addEventListener('message', function (event) {
  if (!event || !event.data)
    return;

  if (event.data.event !== undefined) {
    if (loading && $("#loaderSection")) {
      $("#loaderSection").hide();
    }
    loading = false;
  }

  var webBaseUrl = $("#webBaseUrl")[0].innerText;
  if (event?.origin !== webBaseUrl)
    return;

  if (event.data.reportName !== undefined && event.data.reportName !== null) {
    reportName = event.data.reportName;
    loading = true;
    if ($("#loaderSection")) {
      $("#loaderSection").show();
    }
  }
  else {
    return;
  }
  let url;
  let language = event.data.language;
  let reportBlock = this.document.getElementById("reportBlock");
  let width = "725px";
  let height = "1100px";
  reportBlock.width = "1000px";
  reportBlock.height = "1150px";

  switch (reportName) {
    case "StockItemHistoryReport":
      url = document.getElementById("getStockItemHistoryReportUrl").value;
      url = `${url}?stockCode=${event.data.stockCode}&stockDescription=${event.data.stockDescription}&language=${language}&startDate=${event.data.startDate}&endDate=${event.data.endDate}&companyName=${event.data.companyName}`;
      width = "785px";
      break;

    case "SalesAnalysisByProductReport":
      url = document.getElementById("getSalesAnalysisByProductReportUrl").value;
      url = `${url}?stockCode=${event.data.stockCode}&stockDescription=${event.data.stockDescription}&language=${language}&startDate=${event.data.startDate}&endDate=${event.data.endDate}&companyName=${event.data.companyName}`;
      width = "980px";
      break;

    case "VendorPurchasesByProductReport":
      url = document.getElementById("getVendorPurchasesByProductReportUrl").value;
      url = `${url}?stockCode=${event.data.stockCode}&stockDescription=${event.data.stockDescription}&language=${language}&startDate=${event.data.startDate}&endDate=${event.data.endDate}&companyName=${event.data.companyName}`;
      break;

    case "PriceChangeReport":
      url = `${document.getElementById("getPriceChangeReportUrl").value}?language=${language}&isBackOffice=${event.data.isBackOffice}`;
      width = "1100px";
      height = "1200px";
      break;

    case "StockTransferReport":
      url = `${document.getElementById("getStockTransferReportUrl").value}?language=${language}`;
      width = "1100px";
      height = "1300px";
      break;

    case "HotButtonsReport":
      url = document.getElementById("getHotButtonsReportUrl").value;
      url = `${url}?language=${language}`;
      width = "1100px";
      height = "1300px";
      break;

    case "CostChangeReport":
      url = document.getElementById("getCostChangeReportUrl").value;
      url = `${url}?language=${language}&isBackOffice=${event.data.isBackOffice}`;
      width = "1100px";
      height = "1250px";
      break;

    case "CommodityTransferReport":
      url = `${document.getElementById("getCommodityTransferReportUrl").value}?language=${language}`;
      width = "1100px";
      height = "1250px";
      break;

    case "PromotionalDiscountTransferReport":
      url = `${document.getElementById("getPromotionalDiscountTransferReportUrl").value}?language=${language}`;
      width = "1100px";
      height = "1250px";
      break;

    case "StockReceivingReport":
      url = `${document.getElementById("getStockTakesAndReceivingReport").value}?language=${language}&purpose=${event.data.purpose}&companyName=${event.data.companyName}`;
      width = "810px";
      height = "1250px";
      break;

    case "StockTakingReport":
      url = `${document.getElementById("getStockTakesAndReceivingReport").value}?language=${language}&purpose=${event.data.purpose}&companyName=${event.data.companyName}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "TimesheetReport":
      url = `${document.getElementById("getTimeSheetReport").value}?payPeriod=${event.data.payPeriod}&startingDate=${event.data.startingDate}&language=${language}&reportId=${event.data.reportId}&companyName=${event.data.companyName}&userCode=${event.data.userCode}&priorOS=${event.data.priorOS}&overtimeHPolicy=${event.data.overtimeHPolicy}&overtimeSpolicy=${event.data.overtimeSPolicy}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "PurchasingAndReceivingReport":
      url = `${document.getElementById("getPurchasingAndReceivingReport").value}?language=${language}&purchaseNo=${event.data.purchaseNo}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "FuelPriceChangeReport":
      let storeIdList = event.data.storeIds;
      let storeIds = storeIdList.join(',');

      url = `${document.getElementById("getFuelPriceChangeReport").value}?storeIds=${storeIds}&isFuelCp=${event.data.isFuelCp}&emplId=${event.data.emplId}&language=${language}&companyName=${event.data.companyName}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "ElectronicJournalReport":
      url = `${document.getElementById("getElectronicJournalReport").value}?saleNo=${event.data.saleNo}&isFromTills=${event.data.isFromTills}&language=${language}&companyName=${event.data.companyName}&storeID=${event.data.storeID}`;
      break;

    case "ElectronicJournalSummaryReport":
      url = `${document.getElementById("getElectronicJournalSummaryReport").value}?language=${language}&companyName=${event.data.companyName}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "ElectronicJournalDetailsReport":
      url = `${document.getElementById("getElectronicJournalDetailsReport").value}?language=${language}&companyName=${event.data.companyName}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "ElectronicJournalTenderDetailsReport":
      url = `${document.getElementById("getElectronicJournalTenderDetailsReport").value}?saleNo=${event.data.saleNo}&language=${language}&companyName=${event.data.companyName}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;

    case "LabelPrintingReport":
      url = `${document.getElementById("getLabelPrintingReport").value}?language=${language}&format=${event.data.format}&format=${event.data.orientation}`;
      width = event.data.reportWidth;
      height = event.data.reportHeight;
      break;
  }

  $.ajax({
    url: url,
    data: JSON.stringify(event.data),
    type: 'POST',
    contentType: 'application/json;',
    dataType: 'html',
    success: function (result) {
      if (result === null) return;
      $('#reportBlock').html(result);
      let iframeId = ReportViewerForMvc.getIframeId();
      $(iframeId).css({
        "width": width,
        "min-width": width,
        "max-height": "1500px",
        "min-height": height,
        "height": height,
      });
    },
    error: function (err) {
      alert("there was an error");
      console.log(err);
    }
  });
}, false);
