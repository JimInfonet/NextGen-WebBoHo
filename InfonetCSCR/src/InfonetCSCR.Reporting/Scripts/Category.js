function loadReport(url) {
    $('#loading').show();

    $.ajax({
        url: url,
        success: function (response) {
            $("#reportBlock").html(response);
            $("#reportBlock").show();
            $('#loading').hide();

            var reportName = $("#drpReportSelect").val();
            if (reportName != "SA7010") {
                var iframeId = ReportViewerForMvc.getIframeId();
                $(iframeId).css({
                    //"min-height": "500px",
                    //"min-width": "867px"
                    "min-height": "600px",
                    "min-width": "1000px"
                });
            }
        }
    });
}

function validateRangeFilter(startDate, endDate) {
    if (!startDate || !endDate || new Date(startDate) < new Date("01/01/1753") || new Date(endDate) < new Date("01/01/1753")) {
        $("#errorMessage").html("Please enter a valid date");
        $("#error").show();

        return false;
    }

    if (Date.parse(endDate) < Date.parse(startDate)) {
        $("#errorMessage").html("End date should be greater than the Start date");
        $("#error").show();
        return false;
    }
    return true;
}

function validateFilter(date) {
    if (!date || new Date(date) < new Date("01/01/1753")) {
        $("#errorMessage").html("Please enter a valid date");
        $("#error").show();

        return false;
    }

    return true;
}

function hideGroupIdFilters() {
    $(".singleGrpIdFilter").hide();
    $(".rangeGrpIdFilter").hide();
}

function showGroupIdFilter(type) {
    if ((type || "").toUpperCase().trim() == "RANGE") {
        $(".rangeGrpIdFilter").show();
    }
    else {
        $(".singleGrpIdFilter").show();
    }
}

function loadDropDown(data, dropDownName) {
    $(dropDownName).empty();

    $.each(data, function (i, member) {
        $(dropDownName).append('<option value="'
            + member.Id + '">'
            + member.Id + " -- " + member.Name + '</option>');
    });
}

function loadCategoryDropDown(data) {
    var categorySelect = $('#drpCategorySelect');

    categorySelect.empty().append('<option value="All">All</option>');

    $.each(data || [], function (i, category) {
        //categorySelect.empty();
        categorySelect.append($('<option>', {
            value: category.Id,
            text: category.Name
        }));
    });
}

$(document).ready(function () {

    var dropDownData;
    $("#error").hide();
    $("#date").hide()
    $("#stockMovementFilterContainer").hide();
    $("#shelfLableContainer").hide();
    $("#printBtn").hide();
    $(".singleTaxFilter").hide();
    $(".rangeTaxFilter").hide();
    $(".singleDeptFilter").hide();
    $(".rangeDeptFilter").hide();
    $(".singleSubDeptFilter").hide();
    $(".rangeSubDeptFilter").hide();
    $(".singleSubDetailFilter").hide();
    $(".rangeSubDetailFilter").hide();
    $(".singleCustFilter").hide();
    $(".rangeCustFilter").hide();
    $(".singleEmpFilter").hide();
    $(".rangeEmpFilter").hide();
    $(".singleVendorFilter").hide();
    $(".rangeVendorFilter").hide();
    $(".singleTillFilter").hide();
    $(".rangeTillFilter").hide();
    $(".singleStockFilter").hide();
    $(".rangeStockFilter").hide();
    $(".singlePclassFilter").hide();
    $(".rangePclassFilter").hide();
    $(".singleTclassFilter").hide();
    $(".rangeTclassFilter").hide();
    $(".singleMovDateFilter").hide();
    $(".singleQtyOhFilter").hide();
    $(".singleQtyNohFilter").hide();
    $(".singleDateFilter").hide();
    $(".rangeDateFilter").hide();
    $(".singleOrderNoFilter").hide();
    $(".storeFilter").hide();

    $.ajax({
        type: 'POST',
        url: getReportFilter,
        dataType: 'json',
        success: function (data) {
            dropDownData = data;
            loadCategoryDropDown(dropDownData["Categories"]);
            loadDropDown(dropDownData["Stores"], '#drpStoreSelect');
            loadDropDown(dropDownData["DeptWithFuel"], '#deptSelect');
            loadDropDown(dropDownData["DeptWithFuel"], '#drpDeptSelect');
            loadDropDown(dropDownData["DeptWithFuel"], '#startDeptSelect');
            loadDropDown(dropDownData["DeptWithFuel"], '#endDeptSelect');
            loadDropDown(dropDownData["SubDept"], '#drpSubDeptSelect');
            loadDropDown(dropDownData["SubDept"], '#startSubDeptSelect');
            loadDropDown(dropDownData["SubDept"], '#endSubDeptSelect');
            loadDropDown(dropDownData["SubDetail"], '#drpSubDetailSelect');
            loadDropDown(dropDownData["SubDetail"], '#startSubDetailSelect');
            loadDropDown(dropDownData["SubDetail"], '#endSubDetailSelect');
            loadDropDown(dropDownData["StockCode"], '#drpStockCodeSelect');
            //loadDropDown(dropDownData["Dept"], '#deptmentSelect');          //for ShelfLabelEAN13LReport
            //loadDropDown(dropDownData["SubDept"], '#subdeptmentSelect');    //for ShelfLabelEAN13LReport
            loadDropDown(dropDownData["Vendor"], '#vendorSelect');
            loadDropDown(dropDownData["Reports"], '#drpReportSelect');

            $("#drpReportSelect").trigger('change')
        }
    });

    $('#loading').hide();

    $("[type='date']").change(function (event) {
        $("#error").hide();
    });

    $("#drpCategorySelect").change(function (event) {
        $("#reportBlock").hide();
        $("#error").hide();
        $("#date").hide()
        $("#stockMovementFilterContainer").hide();
        $("#shelfLableContainer").hide();
        $("#deptFilter").hide();
        $("#printBtn").hide();
        $(".singleTaxFilter").hide();
        $(".rangeTaxFilter").hide();
        $(".singleDeptFilter").hide();
        $(".rangeDeptFilter").hide();
        $(".singleSubDeptFilter").hide();
        $(".rangeSubDeptFilter").hide();
        $(".singleSubDetailFilter").hide();
        $(".rangeSubDetailFilter").hide();
        $(".singleCustFilter").hide();
        $(".rangeCustFilter").hide();
        $(".singleEmpFilter").hide();
        $(".rangeEmpFilter").hide();
        $(".singleVendorFilter").hide();
        $(".rangeVendorFilter").hide();
        $(".singleTillFilter").hide();
        $(".rangeTillFilter").hide();
        $(".singleStockFilter").hide();
        $(".rangeStockFilter").hide();
        $(".singlePclassFilter").hide();
        $(".rangePclassFilter").hide();
        $(".singleTclassFilter").hide();
        $(".rangeTclassFilter").hide();
        $(".singleMovDateFilter").hide();
        $(".singleQtyOhFilter").hide();
        $(".singleQtyNohFilter").hide();
        $(".singleDateFilter").hide();
        $(".rangeDateFilter").hide();
        $(".singleOrderNoFilter").hide();
        $(".storeFilter").hide();

        var cate = $("#drpCategorySelect").val();
        var subReport = dropDownData["Reports"].filter(function (e) {
            return cate == "All" || e.GID == cate || e.Id == "All";
        });
        loadDropDown(subReport, '#drpReportSelect');
        $("#drpReportSelect").trigger('change')
    });
    //$("#drpSelectLanguage").change(function (event) {
    //    ViewBag.Language = $("#drpSelectLanguage").val;
    //});
    $("#drpSelectLanguage").change(function (event) {
        var lang = $(this).val();
        $.post('/Home/SetLanguage', { language: lang });
    });
    $("#drpReportSelect").change(function (event) {
        var reportName = $(this).val();
        if (reportName == null)
            return;

        $("#reportBlock").hide();
        $("#error").hide();
        $("#date").hide()
        $("#stockMovementFilterContainer").hide();
        $("#shelfLableContainer").hide();
        $(".singleTaxFilter").hide();
        $(".rangeTaxFilter").hide();
        $(".singleDeptFilter").hide();
        $(".rangeDeptFilter").hide();
        $(".singleSubDeptFilter").hide();
        $(".rangeSubDeptFilter").hide();
        $(".singleSubDetailFilter").hide();
        $(".rangeSubDetailFilter").hide();
        $(".singleCustFilter").hide();
        $(".rangeCustFilter").hide();
        $(".singleEmpFilter").hide();
        $(".rangeEmpFilter").hide();
        $(".singleVendorFilter").hide();
        $(".rangeVendorFilter").hide();
        $(".singleTillFilter").hide();
        $(".rangeTillFilter").hide();
        $(".singleStockFilter").hide();
        $(".rangeStockFilter").hide();
        $(".singlePclassFilter").hide();
        $(".rangePclassFilter").hide();
        $(".singleTclassFilter").hide();
        $(".rangeTclassFilter").hide();
        $(".singleMovDateFilter").hide();
        $(".singleQtyOhFilter").hide();
        $(".singleQtyNohFilter").hide();
        $(".singleDateFilter").hide();
        $(".rangeDateFilter").hide();
        $(".singleOrderNoFilter").hide();
        $(".storeFilter").hide();
        hideGroupIdFilters();

        //Only DailySalesReport uses Printing PDF button
        if (reportName == "SA7010")
            $("#printBtn").show();

        let ReportFilters = [];
        for (let i = 0; i < dropDownData["Filters"].length; i++) {
            //if (dropDownData["Filters"][i].Report_Name.trim() == reportName.trim()) {     //ReportName with first 6 chars
            if (dropDownData["Filters"][i].Report_Name.trim() == reportName.substr(0, 6).trim()) {
                ReportFilters.push(dropDownData["Filters"][i]);
            }
        }
        $.each(ReportFilters, function (i, member) {
            if (member.Field.toUpperCase() == "LAST_SALE") {
                if (member.Type.toUpperCase() == "<=") {
                    $(".singleMovDateFilter").show();
                    if ($("#singleMovDate").val() == "") $("#singleMovDate")[0].valueAsDate = new Date();
                }
                //    $("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "DEL_DATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "SHIFTDATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "DATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "DATE_TIME") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "SALE_DATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "PY_DATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "TRANSACTIONDATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "DRAW_DATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "BATCHDATE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDateFilter").show();
                    if ($("#startDate").val() == "") $("#startDate")[0].valueAsDate = new Date();
                    if ($("#endDate").val() == "") $("#endDate")[0].valueAsDate = new Date();
                } else {
                    $(".singleDateFilter").show();
                    if ($("#singleDate").val() == "") $("#singleDate")[0].valueAsDate = new Date();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "TAX_NAME") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeTaxFilter").show();
                }
                else {
                    $(".singleTaxFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "DEPT") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeDeptFilter").show();
                }
                else {
                    $(".singleDeptFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "SUB_DEPT") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeSubDeptFilter").show();
                }
                else {
                    $(".singleSubDeptFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "SUB_DETAIL") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeSubDetailFilter").show();
                }
                else {
                    $(".singleSubDetailFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "STOCK_CODE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeStockFilter").show();
                }
                else {
                    $(".singleStockFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "STOCKCODE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeStockFilter").show();
                }
                else {
                    $(".singleStockFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "P_CLASS") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangePclassFilter").show();
                }
                else {
                    $(".singlePclassFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "TENDCLASS") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeTclassFilter").show();
                }
                else {
                    $(".singleTclassFilter").show();
                }
                //$("#printBtn").show();
            }
            //if (member.Field.toUpperCase() == "PY_CLIENT") {
            //    if (member.Type.toUpperCase() == "RANGE") {
            //        $(".rangeCustFilter").show();
            //    }
            //    else {
            //        $(".singleCustFilter").show();
            //    }
            //    //$("#printBtn").show();
            //}
            if (member.Field.toUpperCase() == "PY_CLIENT" || member.Field.toUpperCase() == "CLIENT" || member.Field.toUpperCase() == "CL_CODE") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeCustFilter").show();
                }
                else {
                    $(".singleCustFilter").show();
                }
                //$("#printBtn").show();
            }
            //if (member.Field.toUpperCase() == "CLIENT") {
            //    if (member.Type.toUpperCase() == "RANGE") {
            //        $(".rangeCustFilter").show();
            //    }
            //    else {
            //        $(".singleCustFilter").show();
            //    }
            //    //$("#printBtn").show();
            //}
            if (member.Field.toUpperCase() == "GROUPID") {
                showGroupIdFilter(member.Type);
            }
            if (member.Field.toUpperCase() == "USER") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeEmpFilter").show();
                }
                else {
                    $(".singleEmpFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "EMPLOYEEID") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeEmpFilter").show();
                }
                else {
                    $(".singleEmpFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "VENDOR") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeVendorFilter").show();
                }
                else {
                    $(".singleVendorFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "TILL") {
                if (member.Type.toUpperCase() == "RANGE") {
                    $(".rangeTillFilter").show();
                }
                else {
                    $(".singleTillFilter").show();
                }
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "ORDERNO") {
                $(".singleOrderNoFilter").show();
                //$("#printBtn").show();
            }
            if (member.Field.toUpperCase() == "IN_STOCK") {
                if (member.Type.toUpperCase() == "<=")
                    $(".singleQtyOhFilter").show();
                else
                    $(".singleQtyNohFilter").show();
                //$("#printBtn").show();
            }
        });
    });

    $("#submitBtn").on("click", function (event) {
        event.preventDefault();
        $("#error").hide();

        var args = '';
        var reportUrl = '';
        var reportName = $("#drpReportSelect").val();
        var store = $("#drpStoreSelect").length != 0 ? $("#drpStoreSelect").val() : "";
        var isValidated;

        if (reportName == "SA7010") {
            reportUrl = getDailySalesReport;
            reportName = "SalesReport";
            var date1 = $("#singleDate").val();
            //var isValidated = validateFilter(date);

            //if (isValidated) {
            args = "?date=" + date1 + "&reportName=" + reportName;
            //    }
        }
        else {
            reportUrl = getReportUrl;
            args = "?reportName=" + reportName.trim();
            if ($(".singleMovDateFilter").is(":visible")) {
                var date1 = $("#singleMovDate").val();
                args = args + "&date1=" + date1;
            }

            if ($(".rangeDateFilter").is(":visible")) {
                var date1 = $("#startDate").val();
                var date2 = $("#endDate").val();
                args = args + "&date1=" + date1 + "&date2=" + date2;
            }
            else if ($(".singleDateFilter").is(":visible")) {
                var date1 = $("#singleDate").val();
                args = args + "&date1=" + date1;
            }

            if ($(".rangeDeptFilter").is(":visible")) {
                //var dept1 = $("#startDeptSelect").val();
                //var dept2 = $("#endDeptSelect").val();
                var dept1 = "All";
                var dept2 = "All";
                if ($("#startDeptSelect").val() != "")
                    dept1 = $("#startDeptSelect").val();
                if ($("#endDeptSelect").val() != "")
                    dept2 = $("#endDeptSelect").val();

                args = args + "&dept1=" + dept1 + "&dept2=" + dept2;
            }
            else if ($(".singleDeptFilter").is(":visible")) {
                //var dept1 = $("#deptSelect").val();
                var dept1 = "All";
                if ($("#deptSelect").val() != "")
                    dept1 = $("#deptSelect").val();
                args = args + "&dept1=" + dept1;
            }

            if ($(".rangeSubDeptFilter").is(":visible")) {
                //var subdept1 = $("#startSubDeptSelect").val();
                //var subdept2 = $("#endSubDeptSelect").val();
                var subdept1 = "All";
                var subdept2 = "All";
                if ($("#startSubDeptSelect").val() != "")
                    subdept1 = $("#startSubDeptSelect").val();
                if ($("#endSubDeptSelect").val() != "")
                    subdept2 = $("#endSubDeptSelect").val();
                args = args + "&subdept1=" + subdept1 + "&subdept2=" + subdept2;
            }
            else if ($(".singleSubDeptFilter").is(":visible")) {
                //var subdept1 = $("#subdeptSelect").val();
                var subdept1 = "All";
                if ($("#subdeptSelect").val() != "")
                    subdept1 = $("#subdeptSelect").val();
                args = args + "&subdept1=" + subdept1;
            }

            if ($(".rangeSubDetailFilter").is(":visible")) {
                //var subdetail1 = $("#startSubDetailSelect").val();
                //var subdetail2 = $("#endSubDetailSelect").val();
                var subdetail1 = "All";
                var subdetail2 = "All";
                if ($("#startSubDetailSelect").val() != "")
                    subdetail1 = $("#startSubDetailSelect").val();
                if ($("#endSubDetailSelect").val() != "")
                    subdetail2 = $("#endSubDetailSelect").val();
                args = args + "&subdetail1=" + subdetail1 + "&subdetail2=" + subdetail2;
            }
            else if ($(".singleSubDetailFilter").is(":visible")) {
                //var subdetail1 = $("#subDetailSelect").val();
                var subdetail1 = "All";
                if ($("#subDetailSelect").val() != "")
                    subdetail1 = $("#subDetailSelect").val();
                args = args + "&subdetail1=" + subdetail1;
            }

            if ($(".rangeStockFilter").is(":visible")) {
                var stock1 = "All";
                var stock2 = "All";
                if ($("#startStockSelect").val() != "")
                    stock1 = $("#startStockSelect").val();
                if ($("#endStockSelect").val() != "")
                    stock2 = $("#endStockSelect").val();
                args = args + "&stock1=" + stock1 + "&stock2=" + stock2;
            }
            else if ($(".singleStockFilter").is(":visible")) {
                var stock1 = "All";
                if ($("#stockSelect").val() != "")
                    stock1 = $("#stockSelect").val();
                args = args + "&stock1=" + stock1;
            }

            if ($(".rangePclassFilter").is(":visible")) {
                var class1 = "All";
                var class2 = "All";
                if ($("#startPclassSelect").val() != "")
                    class1 = $("#startPclassSelect").val();
                if ($("#endPclassSelect").val() != "")
                    class2 = $("#endPclassSelect").val();
                args = args + "&class1=" + class1 + "&class2=" + class2;
            }
            else if ($(".singlePclassFilter").is(":visible")) {
                var class1 = "All";
                if ($("#pClassSelect").val() != "")
                    class1 = $("#pClassSelect").val();
                args = args + "&class1=" + class1;
            }

            if ($(".rangeTclassFilter").is(":visible")) {
                var class1 = "All";
                var class2 = "All";
                if ($("#startTclassSelect").val() != "")
                    class1 = $("#startTclassSelect").val();
                if ($("#endTclassSelect").val() != "")
                    class2 = $("#endTclassSelect").val();
                args = args + "&class1=" + class1 + "&class2=" + class2;
            }
            else if ($(".singleTclassFilter").is(":visible")) {
                var class1 = "All";
                if ($("#tClassSelect").val() != "")
                    class1 = $("#tClassSelect").val();
                args = args + "&class1=" + class1;
            }

            if ($(".rangeCustFilter").is(":visible")) {
                var cust1 = "All";
                var cust2 = "All";
                if ($("#startCustSelect").val() != "")
                    cust1 = $("#startCustSelect").val();
                if ($("#endCustSelect").val() != "")
                    cust2 = $("#endCustSelect").val();
                args = args + "&cust1=" + cust1 + "&cust2=" + cust2;
            }
            else if ($(".singleCustFilter").is(":visible")) {
                //var cust1 = $("#custSelect").val();
                var cust1 = "All";
                if ($("#custSelect").val() != "")
                    cust1 = $("#custSelect").val();
                args = args + "&cust1=" + cust1;
            }

            if ($(".rangeEmpFilter").is(":visible")) {
                var emp1 = "All";
                var emp2 = "All";
                if ($("#startEmpSelect").val() != "")
                    emp1 = $("#startEmpSelect").val();
                if ($("#endEmpSelect").val() != "")
                    emp2 = $("#endEmpSelect").val();
                args = args + "&emp1=" + emp1 + "&emp2=" + emp2;
            }
            else if ($(".singleEmpFilter").is(":visible")) {
                var emp1 = "All";
                if ($("#empSelect").val() != "")
                    emp1 = $("#empSelect").val();
                args = args + "&emp1=" + emp1;
            }

            if ($(".rangeVendorFilter").is(":visible")) {
                var vendor1 = "All";
                var vendor2 = "All";
                if ($("#startVendorSelect").val() != "")
                    vendor1 = $("#startVendorSelect").val();
                if ($("#endVendorSelect").val() != "")
                    vendor2 = $("#endVendorSelect").val();
                args = args + "&vendor1=" + vendor1 + "&vendor2=" + vendor2;
            }
            else if ($(".singleVendorFilter").is(":visible")) {
                var vendor1 = "All";
                if ($("#vendorSelect").val() != "")
                    vendor1 = $("#vendorSelect").val();
                args = args + "&vendor1=" + vendor1;
            }

            if ($(".rangeTillFilter").is(":visible")) {
                var till1 = "All";
                var till2 = "All";
                if ($("#startTillSelect").val() != "")
                    till1 = $("#startTillSelect").val();
                if ($("#endTillSelect").val() != "")
                    till2 = $("#endTillSelect").val();
                args = args + "&till1=" + till1 + "&till2=" + till2;
            }
            else if ($(".singleTillFilter").is(":visible")) {
                var till1 = "All";
                if ($("#tillSelect").val() != "")
                    till1 = $("#tillSelect").val();
                args = args + "&till1=" + till1;
            }

            if ($(".rangeTaxFilter").is(":visible")) {
                var tax1 = "All";
                var tax2 = "All";
                if ($("#startTaxSelect").val() != "")
                    tax1 = $("#startTaxSelect").val();
                if ($("#endTaxSelect").val() != "")
                    tax2 = $("#endTaxSelect").val();
                args = args + "&tax1=" + tax1 + "&tax2=" + tax2;
            }
            else if ($(".singleTaxFilter").is(":visible")) {
                var tax1 = "All";
                if ($("#taxSelect").val() != "")
                    tax1 = $("#taxSelect").val();
                args = args + "&tax1=" + tax1;
            }

            if ($(".singleOrderNoFilter").is(":visible")) {
                var orderNo = "All";
                if ($("#orderNoSelect").val() != "")
                    orderNo = $("#orderNoSelect").val();
                args = args + "&orderNo=" + orderNo;
            }

            if ($(".singleQtyOhFilter").is(":visible")) {
                var qtyoh = "All";
                if ($("#qytOhSelect").val() != "")
                    qtyoh = $("#qytOhSelect").val();
                args = args + "&qtyoh=" + qtyoh;
            }
            else if ($(".singleQtyNohFilter").is(":visible")) {
                var qtyoh = "All";
                if ($("#qytNohSelect").val() != "")
                    qtyoh = $("#qytNohSelect").val();
                args = args + "&qtyoh=" + qtyoh;
            }
            if ($(".rangeGrpIdFilter").is(":visible")) {
                var grpId1 = "All";
                var grpId2 = "All";
                if ($("#startGrpIdSelect").val() != "")
                    grpId1 = $("#startGrpIdSelect").val();
                if ($("#endGrpIdSelect").val() != "")
                    grpId2 = $("#endGrpIdSelect").val();
                args = args + "&group1=" + grpId1 + "&group2=" + grpId2;
            }
            else if ($(".singleGrpIdFilter").is(":visible")) {
                var grpId = "All";
                if ($("#grpIdSelect").val() != "")
                    grpId = $("#grpIdSelect").val();
                args = args + "&group1=" + grpId;
            }
        }
        args += "&store=" + store;
        loadReport(reportUrl + args);
    });

    //$("#drpReportSelect").trigger("change");

    $("#drpDeptSelect").change(function (event) {
        var dept = $("#drpDeptSelect").val();
        var subDeptData = dropDownData["SubDept"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });
        var subDetailData = dropDownData["SubDetail"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });
        var stockCodeData = dropDownData["StockCode"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });

        loadDropDown(subDeptData, '#drpSubDeptSelect');
        loadDropDown(subDetailData, '#drpSubDetailSelect');
        loadDropDown(stockCodeData, '#drpStockCodeSelect');
    });
    $("#deptSelect").change(function (event) {
        var dept = $("#deptSelect").val();
        var subDeptData = dropDownData["SubDept"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });
        var subDetailData = dropDownData["SubDetail"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });
        var stockCodeData = dropDownData["StockCode"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });
        loadDropDown(subDeptData, '#drpSubDeptSelect');
        loadDropDown(subDetailData, '#drpSubDetailSelect');
        loadDropDown(stockCodeData, '#drpStockCodeSelect');
    });
    $("#deptmentSelect").change(function (event) {
        var dept = $("#deptmentSelect").val();
        var subDeptData = dropDownData["SubDept"].filter(function (e) {
            return dept == "All" || e.DeptId == dept || e.Id == "All";
        });

        loadDropDown(subDeptData, '#subdeptmentSelect');

    });
    $("#printBtn").on("click", function (event) {
        event.preventDefault();
        if ($("#dailySalesReportContainer").length == 0 || $("#dailySalesReportContainer").is(':hidden')) {
            $("#errorMessage").html("Please load the report first.");
            $("#error").show();

            return;
        }
        makePDF();
    });
});
