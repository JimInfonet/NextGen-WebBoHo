var serverDateTime;
var salesReportDate;
var saleReportStore;

Chart.plugins.register({
    afterDraw: function (chart) {
        if (chart.data.datasets[0].data.every(item => item === 0)) {
            let ctx = chart.chart.ctx;
            let width = chart.chart.width;
            let height = chart.chart.height;

            chart.clear();
            ctx.save();
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText('No data to display', width / 2, height / 2);
            ctx.restore();
        }
    }
});

function loadSalesReport() {
    var date = $("#date").val();
    date = moment(date).format('MM/DD/YYYY');
    $('#salesDate').html("Date: " + date);
    salesReportDate = date;
    saleReportStore = $("#drpStoreSelect").length != 0 ? $("#drpStoreSelect option:selected").text() : "";

    var store = $("#drpStoreSelect").length != 0 ? $("#drpStoreSelect").val() : "";
    $.ajax({
        url: getSalesReportData + "?date=" + date + "&store=" + store,
        method: "GET", success: function (data) {
            dataValue = jQuery.parseJSON(data);

            var fuelData = dataValue["FuelSales"];
            var cStoreData = dataValue["CStoreSales"]
            var taxData = dataValue["SalesTaxes"];
            var chargeData = dataValue["SalesCharges"];
            var receiptData = dataValue["SalesReceipts"];
            serverDateTime = new Date(dataValue["DateTime"]);

            loadFuelSales(fuelData);
            loadCStoreSales(cStoreData);
            loadSalesTotal(fuelData, cStoreData);

            loadSaleTaxTable(taxData);
            loadSaleChargeTable(chargeData);
            loadSaleReceipts(receiptData);

            loadDailySalesTotal(fuelData, cStoreData, taxData, chargeData, receiptData);
        }
    });
};

function loadFuelSales(data) {
    loadFuelSalesTable(data);
    loadFuelSalesChart(data);
}

function loadCStoreSales(data) {
    loadCStoreTable(data);
    loadCStoreChart(data);
}

function loadCStoreTable(data) {
    var trHTML = '';
    var totalSales = 0;
    var totalQty = 0;

    $.each(data, function (i, item) {
        totalSales += item.Sales;
        totalQty += item.QTY;
        salesAmt = formatMoney(item.Sales);
        var qty = formatQty(item.QTY);
        item.DeptName = item.DeptName.replace('<', '&lt');  
        item.DeptName = item.DeptName.replace('>', '&gt');

        trHTML += '<tr data-toggle=modal data-target=#salesDetailModal data-id="' + item.DeptID + '" data-dept="' + item.DeptName + '" data-tbl=cstore>' +
            '<td>' + item.DeptName + '</td>' +
            '<td>' + qty + '</td>' +
            '<td class="align-right">' + salesAmt + '</td>' +
            '</tr>';
    });

    totalQty = formatQty(totalQty);
    totalSales = formatMoney(totalSales);

    trHTML += '<tr class="info font-bold">' +
        '<td>Total:</td>' +
        '<td>' + totalQty + '</td>' +
        '<td class="align-right">' + totalSales + '</td>' +
        '</tr > ';

    $('#cStoreTableBody').append(trHTML);
}

function loadCStoreChart(dataValue) {
    var chDiv = document.getElementById('cStoreSalesChart');
    var labels = dataValue.map(function (item) {
        return item.DeptID;
    });

    var data = dataValue.map(function (item) {
        return item.Sales;
    });

    myChart = new Chart(chDiv, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                borderWidth: 1,
                backgroundColor: darkColorArray[29],
            }]
        },
        options: {
            plugins: {
                datalabels: {
                    backgroundColor: function (context) {
                        return context.dataset.backgroundColor;
                    },
                    borderColor: 'white',
                    borderRadius: 25,
                    borderWidth: 2,
                    color: 'white',
                    font: {
                        weight: 'bold'
                    },
                    align: 'end',
                    padding: 6,
                    formatter: function (value) {
                        return formatMoney(value);
                    }
                }
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var value = data.datasets[0].data[tooltipItem.index];
                        return formatMoney(value);
                    }
                }
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'C-Store Sales',
                fontFamily: 'Franklin Gothic Medium',
                fontSize: 16
            },
            legend: {
                display: false,
            },
            scales: {
                yAxes: [{
                    ticks: {
                        callback: function (value) {
                            return formatMoney(value);
                        },
                        beginAtZero: true
                    },
                    scaleLabel: {
                        display: true,
                        labelString: 'Dollar'
                    }
                }],
                xAxes: [{
                    scaleLabel: {
                        display: true,
                        labelString: 'Department'
                    }
                }]
            }
        }
    });
}

function loadFuelSalesTable(data) {
    var trHTML = '';
    var totalSales = 0;
    var totalQty = 0;

    $.each(data, function (i, item) {
        totalSales += item.Sales;
        totalQty += item.QTY;
        salesAmt = formatMoney(item.Sales);
        var qty = formatQty(item.QTY);

        trHTML += '<tr data-toggle=modal data-target=#salesDetailModal data-id=' + item.DeptID + ' data-stockcode="' + item.StockCode + '" data-tbl=fuel>' +
            '<td>' + item.StockCode + '</td>' +
            '<td>' + qty + '</td>' +
            '<td class="align-right">' + salesAmt + '</td>' +
            '</tr>';
    });

    totalQty = formatQty(totalQty);
    totalSales = formatMoney(totalSales);

    trHTML += '<tr class="info font-bold">' +
        '<td>Total:</td>' +
        '<td>' + totalQty + '</td>' +
        '<td class="align-right">' + totalSales + '</td>' +
        '</tr > ';

    $('#fuelTableBody').append(trHTML);
}

function loadFuelSalesChart(dataValue) {
    var chDiv = document.getElementById('fuelSalesChart');
    var labels = dataValue.map(function (item) {
        return item.StockCode;
    });

    var data = dataValue.map(function (item) {
        return item.Sales;
    });
    var backColors = darkColorArray.slice(25, 25 + data.length);
    const reducer = (accumulator, curr) => accumulator + curr;
    var total = dataValue.length > 0 ? data.reduce(reducer) : 0;

    myChart = new Chart(chDiv, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                borderWidth: 1,
                backgroundColor: backColors
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'Fuel Sales',
                fontFamily: 'Franklin Gothic Medium',
                fontSize: 16
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var label = data.labels[tooltipItem.index];
                        var value = data.datasets[0].data[tooltipItem.index];
                        if (label) {
                            label += ': ';
                        }

                        let percentage = ((value / total) * 100).toFixed(2) + '%';
                        label += percentage;
                        return label;
                    }
                }
            },
            plugins: {
                datalabels: {
                    backgroundColor: function (context) {
                        return context.dataset.backgroundColor;
                    },
                    borderColor: 'white',
                    borderRadius: 25,
                    borderWidth: 2,
                    color: 'white',
                    formatter: function (value) {
                        let percentage = ((value / total) * 100).toFixed(2) + '%';
                        return percentage;
                    },
                    font: {
                        weight: 'bold'
                    },
                    padding: 6
                }
            }
        }
    });
}

function loadSalesTotal(fuelSales, cstoreSales) {
    var totalSales = 0;
    var totalQty = 0;

    $.each(fuelSales, function (i, item) {
        totalSales += item.Sales;
        totalQty += item.QTY;
    });

    $.each(cstoreSales, function (i, item) {
        totalSales += item.Sales;
        totalQty += item.QTY;
    });

    totalQty = formatQty(totalQty);
    totalSales = formatMoney(totalSales);

    var trHtml = '<tr class="success font-bold">' +
        '<td>TOTAL SALES:</td>' +
        '<td>' + totalQty + '</td>' +
        '<td class="align-right">' + totalSales + '</td>' +
        '</tr>';

    $('#salesTotalTableBody').append(trHtml);
}

function loadSaleTaxTable(data) {
    var trHTML = '';
    $.each(data, function (i, item) {
        var included = formatMoney(item.Included);
        var added = formatMoney(item.Added);
        var total = formatMoney(item.Total);
        trHTML += '<tr data-toggle=modal data-target=#salesDetailModal data-taxname="' + item.TaxName + '" data-tbl=tax>' +
            '<td>' + item.TaxName + '</td>' +
            '<td class="sales-amt">' + included + '</td>' +
            '<td class="sales-amt">' + added + '</td>' +
            '<td class="sales-amt">' + total + '</td>' +
            '</tr>';
    });

    $('#salesTaxTableBody').append(trHTML);
}

function loadSaleChargeTable(data) {
    var trHTML = '';
    $.each(data, function (i, item) {
        var amt = formatMoney(item.Amount);
        trHTML += '<tr data-toggle=modal data-target=#salesDetailModal data-chargename="' + item.Name + '" data-tbl=charge>' +
            '<td>' + item.Name + '</td>' +
            '<td class=sales-amt>' + amt + '</td>' +
            '</tr>';
    });

    $('#salesChargesTableBody').append(trHTML);
}

function loadSaleReceipts(data) {
    loadSaleReceiptsTable(data);
    loadSaleReceiptsChart(data);
}

function loadSaleReceiptsTable(data) {
    var trHTML = '';
    $.each(data, function (i, item) {
        var amt = formatMoney(item.Amount);
        var tendName = item.Name;
        if (item.IsChartData == 1 || tendName == 'Overpayments' || tendName == 'Payouts') {
            trHTML += '<tr data-toggle=modal data-target=#salesDetailModal data-tendName="' + tendName + '" data-tbl=receipt>' +
                '<td>' + tendName + '</td>' +
                '<td class=sales-amt>' + amt + '</td>' +
                '</tr>';
        }
        else {
            trHTML += '<tr>' +
                '<td>' + tendName + '</td>' +
                '<td class=sales-amt>' + amt + '</td>' +
                '</tr>';
        }
    });

    $('#salesReceiptsTableBody').append(trHTML);
}

function loadSaleReceiptsChart(dataValue) {
    dataValue = dataValue.filter(function (e) {
        return e.IsChartData == true;
    });

    var labels = dataValue.map(function (item) {
        return item.Name;
    });

    var data = dataValue.map(function (item) {
        return item.Amount;
    });

    var backColors = darkColorArray.slice(24, 24 + data.length);
    const reducer = (accumulator, curr) => accumulator + curr;
    var total = dataValue.length > 0 ? data.reduce(reducer) : 0;

    var chDiv = document.getElementById('salesReceiptChart');

    myChart = new Chart(chDiv, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                borderWidth: 1,
                backgroundColor: backColors
            }]
        },
        options: {
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var label = data.labels[tooltipItem.index];
                        var value = data.datasets[0].data[tooltipItem.index];
                        if (label) {
                            label += ': ';
                        }

                        let percentage = ((value / total) * 100).toFixed(2) + '%';
                        label += percentage;
                        return label;
                    }
                }
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: 'Receipts by Type',
                fontFamily: 'Franklin Gothic Medium',
                fontSize: 16
            },

            plugins: {
                datalabels: {
                    backgroundColor: function (context) {
                        return context.dataset.backgroundColor;
                    },
                    borderColor: 'white',
                    borderRadius: 25,
                    borderWidth: 2,
                    color: 'white',
                    formatter: function (value) {
                        let percentage = ((value / total) * 100).toFixed(2) + '%';
                        return percentage;
                    },
                    font: {
                        weight: 'bold'
                    },
                    padding: 6,
                }
            }
        }
    });
}

function loadDailySalesTotal(fuelData, cStoreData, taxData, chargeData, receiptData) {
    var totalSales = 0;

    $.each(fuelData, function (i, item) {
        totalSales += item.Sales;
    });

    $.each(cStoreData, function (i, item) {
        totalSales += item.Sales;
    });

    $.each(taxData, function (i, item) {
        totalSales += item.Added;
    });

    $.each(chargeData, function (i, item) {
        totalSales += item.Amount;
    });

    var totalReceipts = 0;
    $.each(receiptData, function (i, item) {
        if (item.IsChartData || item.Name == 'Penny Rounding' || item.Name == 'Payouts') {
            totalReceipts += item.Amount;
        } else {
            totalReceipts -= item.Amount;
        }
    });

    var totalShort = totalSales - totalReceipts;

    var trHtml = '<tr class="success">' +
        '<td>Total Receipts</td>' +
        '<td class="align-right">' + formatMoney(totalReceipts) + '</td>' +
        '</tr>'
        +
        '<tr class="success">' +
        '<td>Total Sales</td>' +
        '<td class="align-right">' + formatMoney(totalSales) + '</td>' +
        '</tr>'
        +
        '<tr class="success">' +
        '<td>Over/Short</td>' +
        '<td class="align-right">' + formatMoney(totalShort) + '</td>' +
        '</tr>';

    $('#totalDailySales').append(trHtml);
}
/* modal section */

$('#salesDetailModal').on('show.bs.modal', function (e) {
    var tblName = $(event.target).closest('tr').data('tbl');
    var date = $("#date").val();
    date = moment(date).format('MM/DD/YYYY');
    var modalDate = 'Date: ' + date;
    var modalTitle = '';
    var url = '';

    if (tblName == 'fuel') {
        var deptID = $(event.target).closest('tr').data('id');
        var stockCode = $(event.target).closest('tr').data('stockcode');
        var args = "date=" + date + " &dept=" + deptID;
        modalTitle = 'Fuel Sales Details: ' + stockCode;
        url = getSalesDetails + "?" + args + "&stockCode=" + stockCode;
    }
    else if (tblName == 'cstore') {
        var deptID = $(event.target).closest('tr').data('id');
        var dept = $(event.target).closest('tr').data('dept');
        var args = "date=" + date + " &dept=" + deptID;
        modalTitle = 'C-Store Sales Details: ' + dept;
        url = getSalesDetails + "?" + args + "&stockCode=CSTORE";
    }
    else if (tblName == 'tax') {
        var taxName = $(event.target).closest('tr').data('taxname');
        var args = "date=" + date + " &taxName=" + taxName;
        modalTitle = 'Sales Taxes Details: ' + taxName;
        url = getSalesTaxesDetails + "?" + args;
    }
    else if (tblName == 'charge') {
        var chargeName = $(event.target).closest('tr').data('chargename');
        var args = "date=" + date + " &chargeName=" + chargeName;
        modalTitle = 'Sales Charges Details: ' + chargeName;
        url = getSalesChargesDetails + "?" + args;
    }
    else if (tblName == 'receipt') {
        var tendName = $(event.target).closest('tr').data('tendname');
        if (tendName == 'Overpayments' || tendName == "Payouts") {
            tblName = 'payReceipt';
        }
        var args = "date=" + date + " &tendName=" + tendName;
        modalTitle = 'Sales Receipt Details: ' + tendName;
        url = getSalesReceiptsDetails + "?" + args;
    }

    var store = $("#drpStoreSelect").length != 0 ? $("#drpStoreSelect").val() : "";
    url += "&store=" + store;
    $('#modal-title').html(modalTitle);
    $('#date-details').html(modalDate);

    $.ajax({
        url: url,
        method: "GET", success: function (data) {
            var dataValue = jQuery.parseJSON(data);
            loadSalesDetails(dataValue, tblName);
        }
    });
});

function loadSalesDetails(data, tblName) {
    if (tblName == 'cstore' || tblName == 'fuel') {
        theader = '<tr><th>Sale #</th><th>Till #</th><th>Line #</th><th>Stock Code</th><th>Description</th><th>QTY</th><th class=sales-amt>Price</th><th class=sales-amt>Amount</th></tr>'
        trHTML = '';
        $.each(data, function (i, item) {
            amt = formatMoney(item.Amount);
            price = formatMoney(item.Price);
            qty = formatQty(item.QTY);
            trHTML += '<tr><td>' + item.SaleNo + '</td><td>' + item.TillNum + '</td><td>' + item.LineNum + '</td><td>' + item.StockCode + '</td><td>' + item.Descr + '</td><td>' + qty + '</td><td class=sales-amt>' + price + '</td><td class=sales-amt>' + amt + '</td></tr>';
        });
    }
    else if (tblName == 'tax') {
        theader = '<tr><th>Sale #</th><th>Till #</th><th>Tax Name</th><th class=sales-amt>Added</th><th class=sales-amt>Included</th></tr>'
        trHTML = '';
        $.each(data, function (i, item) {
            added = formatMoney(item.Added);
            included = formatMoney(item.Included);
            trHTML += '<tr><td>' + item.SaleNo + '</td><td>' + item.TillNum + '</td><td>' + item.TaxName + '</td><td class=sales-amt>' + added + '</td><td class=sales-amt>' + included + '</td></tr>';
        });
    }
    else if (tblName == 'charge') {
        theader = '<tr><th>Sale #</th><th>Till #</th><th>Line #</th><th>Description</th><th class=sales-amt>Amount</th></tr>'
        trHTML = '';
        $.each(data, function (i, item) {
            amt = formatMoney(item.Amount);
            trHTML += '<tr><td>' + item.SaleNo + '</td><td>' + item.TillNum + '</td><td>' + item.LineNum + '</td><td>' + item.Descr + '</td><td class=sales-amt>' + amt + '</td></tr>';
        });
    }
    else if (tblName == 'receipt') {
        theader = '<tr><th>Sale #</th><th>Till #</th><th>Sequence #</th><th>Tender Name</th><th class=sales-amt>Amount</th></tr>'
        trHTML = '';
        $.each(data, function (i, item) {
            amt = formatMoney(item.Amount);
            trHTML += '<tr><td>' + item.SaleNo + '</td><td>' + item.TillNum + '</td><td>' + item.Sequence + '</td><td>' + item.TendName + '</td><td class=sales-amt>' + amt + '</td></tr>';
        });
    }
    else if (tblName == 'payReceipt') {
        theader = '<tr><th>Sale #</th><th>Till #</th><th>Tender Name</th><th class=sales-amt>Amount</th></tr>'
        trHTML = '';
        $.each(data, function (i, item) {
            amt = formatMoney(item.Amount);
            trHTML += '<tr><td>' + item.SaleNo + '</td><td>' + item.TillNum + '</td><td>' + item.TendName + '</td><td class=sales-amt>' + amt + '</td></tr>';
        });
    }

    $('#salesDetailsHeader').html(theader);
    $('#salesDetailsBody').html(trHTML);
}


/* Export to PDF*/

function makePDF() {

    document.getElementsByTagName('meta')['viewport'].content = 'width=1400px';

    var element = $('#dailySalesReportContainer')[0];
    var pdf = new jsPDF('p', 'pt', 'a4');
    var pdfWidth = pdf.internal.pageSize.getWidth();
    var pdfHeight = pdf.internal.pageSize.getHeight();
    pdf.setFontSize(8);

    var footerText;
    if (saleReportStore == "") {
        footerText = "For the date: " + salesReportDate;
    }
    else {
        footerText = "For the date: " + salesReportDate + " (Store: " + saleReportStore + ")";
    }

    var prop = {
        scrollX: -window.scrollX,
        scrollY: -window.scrollY,
        windowWidth: document.documentElement.offsetWidth,
        windowHeight: document.documentElement.offsetHeight,
        'scale': 2
    }

    html2canvas(element, prop)
        .then((canvas) => {
            var widthRatio = pdfWidth / canvas.width;
            var totalHeight = canvas.height;

            var sX = 0;
            var sWidth = canvas.width;
            var sHeight = pdfHeight + ((pdfHeight - pdfHeight * widthRatio) / widthRatio);
            var dX = 0;
            var dY = 0;
            var dWidth = sWidth;
            var dHeight = sHeight;
            var pageCnt = 1;
            while (totalHeight > 0) {
                totalHeight -= sHeight;
                var sY = sHeight * (pageCnt - 1);
                var newCanvas = document.createElement("CANVAS");
                newCanvas.setAttribute("width", sWidth);
                newCanvas.setAttribute("height", sHeight);
                var ctx = newCanvas.getContext("2d");
                ctx.drawImage(canvas, sX, sY, sWidth, sHeight, dX, dY, dWidth, dHeight-100);
                if (pageCnt > 1) {
                    pdf.addPage();
                }
                pdf.setPage(pageCnt);
                pdf.addImage(newCanvas.toDataURL("image/png"), 'PNG', 0, 0, canvas.width * widthRatio, 0);
                pdf.text(10, 828, footerText);
                pdf.text(10, 838, "Printed on: " + serverDateTime.toLocaleDateString() + " " + serverDateTime.toLocaleTimeString());
                pdf.text(590, 838, "Page No: " + pageCnt.toString(), { align: 'right' });
                pageCnt++;
            }

            var date = $("#date").val();
            date = moment(date).format('MM/DD/YYYY');

            pdf.save('Daily_Sales_Report_' + `[${date}].pdf`);
        });
    document.getElementsByTagName('meta')['viewport'].content = 'width=device-width';
}