/* Report No:
 * 
 * Fuel Litres Summary Report = 1           -> optFuelLitre in VB
 * Fuel Dollars Summary = 2                 -> optFuelDollar in VB
 * Departmental Sales Summary = 3           -> optOther in VB
 * Sales Summary Family Group = 4           -> optFamilySum in VB 
 * Sales Detail Family Group = 5            -> OptFamilDetail in VB 
 * Sales Summary Fuel Products = 6          -> optSalesSumFuel in VB
 * North 60 Fleet Card Report = 7           -> optRegistre in VB
 * YGT Fleet cards report = 8               -> optFleetCards in VB
 * Fuel Daily Sales Report = 9              -> optFuelSales in VB
 * Sales By Hour Report = 10                -> optSalesByHour in VB
 * FNGTR Fuel Sales Report = 11             -> optFngtr_report in VB
 * 
 */

let policies;
let isBackOffice = document.getElementById("isBackOffice").value === "True";
let checkInterval;
let selectedReportNo;
let memberNo;
let teType; // Tax Exempt Type

const fetchingDataText = document.getElementById("fetchingData").value;
const generateText = document.getElementById("generate").value;
const downloadingText = document.getElementById("downloading").value;

$(async function () {
  await loadDepartmentOptions();
  await loadGroupsOptions();

  if (!isBackOffice) {
    await loadStoresOptions();
  }

  await initPolicy();

  if (policies.taxExemptType === 'SITE' && policies.teByRate) {
    teType = "BITE";
  } else {
    teType = policies.taxExemptType;
  }

  $('input[type=radio][name=ReportNo]').each(function () {
    if (this.value === "1" || this.value === "2" || this.value === "3" || this.value === "4" || this.value === "5") {
      $(this).prop("disabled", !(policies.isTaxExempt === true && (policies.taxExemptType === "SITE" || policies.taxExemptType === "AITE")));
    }

    if (this.value === "6") {
      $(this).prop("disabled", !(policies.isTaxExempt === true && policies.taxExemptType === "QITE"));
    }

    if ((policies.isTaxExempt === true && policies.taxExemptType === "QITE")) {
      $('input[type=radio][name=ReportNo][value=9]').prop('checked', true).change();
    }

    if (!($(this).prop("disabled"))) {
      if (isNullUndefinedOrWhiteSpace(selectedReportNo)) {
        $(this).prop('checked', true).change();
        selectedReportNo = this.value;
      } 
    }
  });
});

let initPolicy = async () => {
  const url = `ExcelReports/GetExcelReportsPolicies`;

  await ajaxGetAsync(url)
    .then(_ => policies = _)
    .catch(toastrError);
}

let checkIsDownloadStarted = async () => {
  let isDownloadStarted = await checkReportDownloadStarted();
  if (isDownloadStarted) {
    clearInterval(checkInterval);
    toastr.success($("#downloadStartedSuccessfully").val());
    startOrEndLoader("generateButton", generateText, false);
  }
}

$('#generateButton').on('click', async (event) => {
  event.preventDefault();

  let selectedDate = $('#selectedDate').val();
  let startDate = $('#startDate').val();
  let endDate = $('#endDate').val();
  let selectedDepartment = $('#department-select').val();
  let selectedGroup = $('#groupId-select').val();
  let selectedStore = $('#stores-select').val();
  let isValidDate = true;
  let modelData = {
    selectedDate: new Date(selectedDate),
    startDate: new Date(startDate),
    endDate: new Date(endDate),
    department: selectedDepartment,
    groupId: selectedGroup,
    storeId: selectedStore,
    taxExemptType: teType,
    reportNo: parseInt(selectedReportNo)
  }

  let minDate = '1753-01-01';
  let maxDate = '9999-12-31';

  if (selectedReportNo === "9") {
    if (!(selectedDate >= minDate && selectedDate <= maxDate)) {
      isValidDate = false;
    }
  } else if (selectedReportNo === "1" || selectedReportNo === "2" || selectedReportNo === "3" || selectedReportNo === "4" || selectedReportNo === "5" || selectedReportNo === "6" || selectedReportNo === "7" || selectedReportNo === "8" || selectedReportNo === "10" || selectedReportNo === "11") {
    if (!(startDate >= minDate && endDate <= maxDate && startDate <= endDate)) {
      isValidDate = false;
    }
  }

  if (selectedReportNo === "3") {
    if (isNullUndefinedOrWhiteSpace(selectedDepartment)) {
      toastr.warning($("#pleaseSelectDepartment").val());
      return;
    }
  }

  if (selectedReportNo === "4" || selectedReportNo === "5") {
    if (isNullUndefinedOrWhiteSpace(selectedGroup)) {
      toastr.warning($("#pleaseSelectGroup").val());
      return;
    }
  }

  // HO only
  if ((selectedReportNo === "10" || selectedReportNo == "9") && !isBackOffice) {
    if (isNullUndefinedOrWhiteSpace(selectedStore)) {
      toastr.warning($("#pleaseSelectStore").val());
      return;
    }
  }

  if (!isValidDate) {
    toastr.warning($("#dateShouldBeBetween").val().replace("{0}", getFormattedDate(new Date(minDate))).replace("{1}", getFormattedDate(new Date(maxDate))));
    return;
  }

  startOrEndLoader("generateButton", fetchingDataText, true);
  let isSaleDataExists = await checkSaleDataExists(modelData);

  if (isSaleDataExists) {
    $('#generateButton').closest('form').submit();
    startOrEndLoader("generateButton", downloadingText, true);

    checkInterval = setInterval(checkIsDownloadStarted, 500);
  }
  else {
    startOrEndLoader("generateButton", generateText, false);
    toastr.warning(selectedReportNo === "6" || selectedReportNo == "7" ? $("#noDataAvailableForSelectedDateRange").val() : $("#thereIsNoDataBasedOnTheCurrentSelectionCriteria").val());
  }

});

let checkSaleDataExists = async (modelData) => {
  let url = "ExcelReports/CheckSaleDataExists";
  let isSaleDataExists = false;

  await ajaxPostAsync(url, modelData)
    .then(async (response) => {
      isSaleDataExists = response;
    })
    .catch(toastrError);

  return isSaleDataExists;
}

let checkReportDownloadStarted = async () => {
  let url = "ExcelReports/CheckReportDownLoadStarted";
  let isDownloadStarted = false;

  await ajaxGetAsync(url)
    .then(async (response) => {
      isDownloadStarted = response;
    })
    .catch(toastrError);

  return isDownloadStarted;
}

let reportSelectionChanged = (radio) => {
  selectedReportNo = radio.value;

  $("#endDate").prop('disabled', radio.value === "10");

  if (radio.value === "9") {
    $("#selectDateRangeWithOptionsDiv").attr("hidden", true);
    $("#selectDateDiv").attr("hidden", false);
  }
  else {
    $("#selectDateRangeWithOptionsDiv").attr("hidden", false);
    $("#selectDateDiv").attr("hidden", true);

    if (radio.value === "3") {
      $("#departmentSelectDiv").attr("hidden", false);
    }
    else {
      $("#departmentSelectDiv").attr("hidden", true);
    }

    if (radio.value === "4" || radio.value === "5") {
      $("#groupIdSelectDiv").attr("hidden", false);
    }
    else {
      $("#groupIdSelectDiv").attr("hidden", true);
    }

    if (radio.value === "10") {
      let startDate = $("#startDate").val();
      let fixedEndDate = new Date(startDate);
      fixedEndDate.setDate(fixedEndDate.getDate() + 6);
      const formattedEndDate = fixedEndDate.toISOString().split('T')[0];

      $("#endDate").val(formattedEndDate);
    }
  }
}

let loadDepartmentOptions = async () => {
  $("#department-select").html(optionGeneratorDefault(document.getElementById("departmentText").value));
  let url = "/StockItems/GetDepartments";

  await ajaxGetAsync(url)
    .then(_depts => {
      for (const dept of _depts) {
        $("#department-select").append(optionGenerator(dept.deptId, dept.deptName));
      }
    })
    .catch(toastrError);
}

let loadGroupsOptions = async () => {
  $("#groupId-select").html(optionGeneratorDefault(document.getElementById("groupIdText").value));
  let url = "StockDescriptionElements/GetAllFamilyGroups";

  await ajaxGetAsync(url)
    .then(groups => {
      for (const group of groups) {
        $("#groupId-select").append(optionGenerator(group.groupID, group.groupName));
      }
    })
    .catch(toastrError);
}

let loadStoresOptions = async () => {
  $("#stores-select").html(optionGeneratorDefault(document.getElementById("storeText").value));
  const url = "Store/GetStoresWithRegionAndPriceGroup";

  await ajaxGetAsync(url)
    .then(stores => {
      for (const store of stores) {
        $("#stores-select").append(optionGenerator(store.storeID, store.storeName));
      }
    })
    .catch(toastrError);
}

$("#startDate").on('change', function () {
  if (selectedReportNo !== "10") return;

  let startDate = $("#startDate").val();
  let fixedEndDate = new Date(startDate);
  fixedEndDate.setDate(fixedEndDate.getDate() + 6);
  const formattedEndDate = fixedEndDate.toISOString().split('T')[0];

  $("#endDate").val(formattedEndDate);
});
