/*:::::::::::::::: Global ::::::::::::::::*/

const STRING_TABLE = TIMESHEET_REPORTS_STRING_TABLE;
let policies = {};
let userCode = "";
let userId = "";
let userDetails = {};
let allowEdit = false;
let isEditClicked = false;
let isViewCurrent = true;
let employees = [];
let activeRow = null;
let selectedEmployee = null;
let selectedEmployeeHeaderDetails = null;

let dataTableEmployeeList;
let dataTableTimesheetDetails;
let dataTableOutstandingShortages;
let companyName;

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  userCode = urlParams.get('userCode');
  isViewCurrent = urlParams.get('isViewCurrent') === "true";

  verifyCheckedInUser();
  await loadUserDetails();
  await loadPolicies();
  await handleViewReports();
  companyName = await getCompanyName();
});

function verifyCheckedInUser() {
  const checkedInUser = localStorage.getItem("checkedInUser");

  if (isNullOrUndefined(checkedInUser) || userCode !== checkedInUser) {
    safelyNavigateTo(STRING_TABLE.URL.CHECK_IN_CHECK_OUT);
  }
}

async function loadUserDetails() {
  const params = new URLSearchParams();
  params.append(GENERAL_STRING_TABLE.PARAMS.USER_CODE, userCode);
  const url = `${GENERAL_STRING_TABLE.URL.GET_USER_DETAILS}?${params.toString()}`;

  await ajaxGetAsync(url)
    .then(_userDetails => userDetails = _userDetails)
    .catch(toastrError);
}

async function loadPolicies() {
  const url = STRING_TABLE.URL.GET_POLICIES;

  await ajaxPostAsync(url, userDetails)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

async function handleViewReports() {
  const params = { policies, isViewCurrent, userCode }
  employees = await ajaxPostAsync(STRING_TABLE.URL.GET_REPORTS, params);

  if (employees.length === 0) {
    $("#btn-preview").prop("disabled", true);
    $("#btn-post").prop("disabled", true);
    $("#btn-edit").prop("disabled", true);
  }
  else {
    $("#btn-preview").prop("disabled", false);

    if (policies.editPostRP) {
      $("#btn-post").prop("disabled", false);
      $("#btn-edit").prop("disabled", false);
      allowEdit = true;
    }
    else {
      $("#btn-post").prop("disabled", true);
      $("#btn-edit").prop("disabled", true);
      allowEdit = false;
    }
  }

  selectedEmployee = employees.length === 0 ? null : employees[0];
  await renderEmployeeList();
  await handleSelectedEmployeeChange(selectedEmployee);
}

function renderEmployeeList() {
  let columns = [
    { data: STRING_TABLE.TABLE.EMPLOYEE_LIST.COLUMN_NAME.USER_CODE },
    {
      data: STRING_TABLE.TABLE.EMPLOYEE_LIST.COLUMN_NAME.PERIOD_STARTING,
      render: periodStarting => new Date(periodStarting).toLocaleDateString(),
    },
    {
      data: STRING_TABLE.TABLE.EMPLOYEE_LIST.COLUMN_NAME.PERIOD_ENDING,
      render: periodEnding => new Date(periodEnding).toLocaleDateString(),
    },
    { data: STRING_TABLE.TABLE.EMPLOYEE_LIST.COLUMN_NAME.STATUS },
  ];

  const height = $('#employee-details__wrapper').height() + "px";
  const props = { columns, data: employees, bInfo: false, bLengthChange: false, ordering: false, paginate: false, scrollY: height };
  destroyDataTableIfExists(dataTableEmployeeList);
  dataTableEmployeeList = createDataTableGeneric("table--employee-list", props, true);

  // Make the first row active
  activeRow = dataTableEmployeeList.row(0).node();
  $(activeRow).addClass('active');
}

// Trigger: clicking on employee list table row
$('#table--employee-list tbody').on('click', 'tr', async function () {
  let employee = dataTableEmployeeList.row(this).data();
  await handleSelectedEmployeeChange(employee);

  if (activeRow) {
    $(activeRow).removeClass('active');
  }
  activeRow = this;
  $(this).addClass('active');
});

async function renderSelectedEmployeeDetails() {
  if (selectedEmployee == null) return;

  let url = `${STRING_TABLE.URL.GET_TIMESHEET_HEADER_DETAILS}?reportId=${selectedEmployee.reportId}`;
  const headerDetails = await ajaxGetAsync(url);
  if (headerDetails === null) return;
  selectedEmployeeHeaderDetails = headerDetails;
  userId = headerDetails.userId !== 0 ? headerDetails.userId : "";

  if (policies.haveUserId) {
    $("#details--employee-id").text(userId);
  }
  else $("#details--employee-id").text(headerDetails.userCode);

  $("#details--employee-name").text(headerDetails.userName);

  $("#details--pay-date").text(new Date(headerDetails.payDate).toLocaleDateString());
  $("#details--period-starting").text(new Date(headerDetails.periodStarting).toLocaleDateString());
  $("#details--period-ending").text(new Date(headerDetails.periodEnding).toLocaleDateString());
  $("#details--uniform-purchase__value").text(headerDetails.uniformPurchases.toFixed(2));

  // Load Detail related data
  // Update shortages first from shift recon. for CURRENT reports (not finalized)
  await ajaxPostAsync(STRING_TABLE.URL.UPDATE_SHORTAGES, {})
    .then(() => { })
    .catch(toastrError);

  // Load Details
  url = `${STRING_TABLE.URL.GET_TIMESHEET_DETAILS}?userCode=${headerDetails.userCode}&reportId=${headerDetails.reportId}`;
  const details = await ajaxGetAsync(url);
  await renderSelectedEmployeeReport(details);

  const totalHours = details.reduce((accumulator, { hours }) => {
    return accumulator + hours;
  }, 0);

  const totalCommissions = details.reduce((accumulator, { commission }) => {
    return accumulator + commission;
  }, 0);

  const totalShortages = details.reduce((accumulator, { shortages }) => {
    return accumulator + shortages;
  }, 0);

  $("#details--total-hours").text(totalHours.toFixed(2));
  $("#details--commission").text(totalCommissions.toFixed(2));
  $("#details--total-shortages").text(totalShortages.toFixed(2));

  // Load Prior O/S and load new outstanding shortages
  // Sylvain wants to display shortages by shiftdate (so if there are two entries for the same day, he wants to group them

  // Update shortages
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.USER_CODE, headerDetails.userCode);
  params.append(STRING_TABLE.PARAMS.REPORT_ID, headerDetails.reportId);
  params.append(STRING_TABLE.PARAMS.PERIOD_STARTING, headerDetails.periodStarting);

  url = `${STRING_TABLE.URL.UPDATE_MISSING_REPORT_ID_IN_SHORTAGES}?${params.toString()}`;

  await ajaxPostAsync(url, {})
    .then(() => { })
    .catch(toastrError);

  // Load all outstanding shortages
  url = `${STRING_TABLE.URL.GET_OUTSTANDING_SHORTAGES}?${params.toString()}`;
  const outstandingShortages = await ajaxGetAsync(url);
  renderOutstandingShortages(outstandingShortages);

  // Update total prior o/s
  const priorOS = outstandingShortages.reduce((accumulator, { shortage }) => {
    return accumulator + shortage;
  }, 0);

  $("#details--prior-os").text(priorOS.toFixed(2));
}

function renderOutstandingShortages(outstandingShortages) {
  let columns = [
    {
      data: STRING_TABLE.TABLE.INSIDE_DETAILS.COLUMN_NAME.SHIFT_DATE,
      render: shiftDate => new Date(shiftDate).toLocaleDateString(),
    },
    {
      data: STRING_TABLE.TABLE.INSIDE_DETAILS.COLUMN_NAME.SHORTAGE,
      render: shortage => shortage.toFixed(2),
    },
  ];

  const props = { columns, data: outstandingShortages, bInfo: false, bLengthChange: false, paginate: false, ordering: false };
  destroyDataTableIfExists(dataTableOutstandingShortages);
  dataTableOutstandingShortages = createDataTableGeneric("table--inside-details", props, true);
}

function renderSelectedEmployeeReport(details) {
  if (details === null || details.length === 0) {
    $("#btn-preview").prop("disabled", true);
  }
  else {
    $("#btn-preview").prop("disabled", false);
  }

  let columns = [
    { data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.DAY_OF_WEEK },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.SHIFT_DATE,
      render: shiftDate => new Date(shiftDate).toLocaleDateString(),
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.STAT,
      render: stat => stat ? "Yes" : "No",
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_IN,
      render: function (timeIn) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${timeIn.slice(0, 5)}</span>
          </div>`;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleShiftFormEdit(td, cellData, rowData, STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_IN);
      }
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_OUT,
      render: function (timeOut) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${timeOut.slice(0, 5)}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleShiftFormEdit(td, cellData, rowData, STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_OUT);
      }
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.HOURS,
      render: hours => `<span class="hours">${hours.toFixed(2)}</span>`,
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.COMMISSION,
      render: commission => `<span class="commission">${commission.toFixed(2)}</span>`,
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.SHORTAGES,
      render: shortages => shortages.toFixed(2),
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TRAINING,
      render: function (training) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${training ? "Yes" : "No"}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleShiftFormEdit(td, cellData, rowData, STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TRAINING);
      }
    },
    {
      data: STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.COMMENT,
      render: function (comment) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${comment ? comment.slice(0, 30) : '&nbsp;'}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleShiftFormEdit(td, cellData, rowData, STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.COMMENT);
      }
    },
  ];

  const props = { columns, data: details, ordering: false };
  destroyDataTableIfExists(dataTableTimesheetDetails);
  dataTableTimesheetDetails = createDataTableGeneric("table--timesheet-detail", props, true);
}

async function handleSelectedEmployeeChange(employee) {
  if (employee === null) return;
  selectedEmployee = employee;
  await renderSelectedEmployeeDetails();

  if (selectedEmployee.status === "Working" && allowEdit) {
    $("#btn-edit").prop("disabled", false);
    $("#btn-post").prop("disabled", false);
  }
  else {
    $("#btn-edit").prop("disabled", true);
    $("#btn-post").prop("disabled", true);
  }
}

$("#details--uniform-purchase__value").on("click", function () {
  if (!isEditClicked) return;

  $("#details--uniform-purchase__value").hide();
  $("#details--uniform-purchase__input").val($(this).text()).show().focus().select();
});

$("#details--uniform-purchase__input").on("keypress focusout", async function (e) {
  if (e.type === "keypress" && e.key !== "Enter") {
    const currentValue = $(this).val();
    const newValue = currentValue + e.key;
    const parsedValue = parseFloat(newValue);

    if (parsedValue > 999999 || parsedValue < -99999) {
      const message = $("#_valueOutOfRangeErrorMessage").val();
      toastr.warning(message);
      e.preventDefault();
    }
    return;
  }
  await updateUniformPurchase();
  $("#details--uniform-purchase__input").hide();
  $("#details--uniform-purchase__value").show();
});

async function updateUniformPurchase() {
  const uniformPurchases = parseFloat($("#details--uniform-purchase__input").val());
  if (isNaN(parseFloat(uniformPurchases))) {
    toastr.warning($("#_invalidNumberWarningMessage").val());
    return;
  }

  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.USER_CODE, selectedEmployeeHeaderDetails.userCode);
  params.append(STRING_TABLE.PARAMS.UNIFORM_PURCHASES, uniformPurchases);
  params.append(STRING_TABLE.PARAMS.PERIOD_STARTING, selectedEmployeeHeaderDetails.periodStarting);
  params.append(STRING_TABLE.PARAMS.PERIOD_ENDING, selectedEmployeeHeaderDetails.periodEnding);

  url = `${STRING_TABLE.URL.UDPATE_UNIFORM_PURCHASES}?${params.toString()}`;

  await ajaxPostAsync(url, {})
    .then(() => { })
    .catch(toastrError);

  await renderSelectedEmployeeDetails();
}

async function handleEditDetailsClick() {
  isEditClicked = !isEditClicked;

  $('#btn-preview').prop('disabled', isEditClicked);
  $('#btn-post').prop('disabled', isEditClicked);

  if (isEditClicked) {
    $("#btn-edit").text($("#_done").val());
    $("#table--employee-list__wrapper").addClass('data-table__disabled');
  }
  else {
    $("#btn-edit").text($("#_edit").val());
    $("#table--employee-list__wrapper").removeClass('data-table__disabled');
  }
}

async function handlePostButtonClick() {
  const title = $("#_post").val();
  const question = $("#_payPeriodPostConfirmation").val();
  let confirmed = await showConfirmModal(title, question);
  if (!confirmed) return;

  if (new Date(selectedEmployee.periodEnding) >= new Date()) {
    // Wrong pay period, this pay period is still active
    const warningMessage = $("#_activePayPeriodWarning").val();
    toastr.warning(warningMessage);
    return;
  }

  if (selectedEmployeeHeaderDetails.userCode === null) return;

  // Check if there are no missing check outs for the pay period
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.USER_CODE, selectedEmployeeHeaderDetails.userCode);
  params.append(STRING_TABLE.PARAMS.PERIOD_ENDING, selectedEmployeeHeaderDetails.periodEnding);

  let url = `${STRING_TABLE.URL.MISSING_CHECK_OUTS_EXIST}?${params.toString()}`;

  const missingCheckOutsExist = await ajaxGetAsync(url);
  if (missingCheckOutsExist) {
    const message = $("#_missingCheckOutsWarning").val();
    toastr.warning(message);
    return;
  }

  // Go through all the timesheets and ask for comment if more than 10 hours
  params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.USER_CODE, selectedEmployeeHeaderDetails.userCode);

  url = `${STRING_TABLE.URL.MISSING_COMMENT_EXIST}?${params.toString()}`;

  const missingCommentExist = await ajaxGetAsync(url);
  if (missingCommentExist) {
    const warningMessage = $("#_enterExplanationWarning").val();
    toastr.warning(warningMessage);
    return;
  }

  if (policies.pyFile) {
    if (await GeneratePYFile() === false) return;
  }

  // Now post it
  params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.USER_CODE, selectedEmployeeHeaderDetails.userCode);
  params.append(STRING_TABLE.PARAMS.PERIOD_STARTING, selectedEmployeeHeaderDetails.periodStarting);

  url = `${STRING_TABLE.URL.POST_PAY_PERIOD}?${params.toString()}`;

  const posted = await ajaxGetAsync(url);
  if (posted) {
    const successMessage = $("#_payPeriodPostingSuccessMessage").val();
    toastr.success(successMessage);

    await handleViewReports();
  }
  else {
    const errorMessage = $("#_errorPosting").val();
    toastr.error(errorMessage);
    return;
  }
}

async function GeneratePYFile() {
  let exported = false;
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.USER_CODE, selectedEmployeeHeaderDetails.userCode);
  params.append(STRING_TABLE.PARAMS.PERIOD_STARTING, selectedEmployeeHeaderDetails.periodStarting);
  params.append(STRING_TABLE.PARAMS.PERIOD_ENDING, selectedEmployeeHeaderDetails.periodEnding);
  params.append(STRING_TABLE.PARAMS.USER_ID, userId);

  url = `${STRING_TABLE.URL.GENERATE_PY_FILE}?${params.toString()}`;

  await ajaxPostAsync(url, policies)
    .then(async ({ content, storeCode }) => {
      const today = new Date();
      const year = today.getFullYear();
      const month = String(today.getMonth() + 1).padStart(2, '0'); // Month is zero-based
      const day = String(today.getDate()).padStart(2, '0');
      const formattedDate = year + month + day;

      let folderPath = $("#_exportFolderPath").val();
      let fileName = `PY${formattedDate}.${storeCode}`;
      let fileType = GENERAL_STRING_TABLE.FILE_TYPE.PY_FILE;

      exported = await exportFileBasedOnConfig(folderPath, fileName, content, fileType);
    })
    .catch(() => {
      toastr.error($("#_anErrorHasOccurredWhileExportingFile").val());
      exported = false;
    });

  return exported;
}

async function handleShiftFormEdit(td, cellData, rowData, cellType) {
  const cellContainer = $(td).find('.cell-container');
  const textSpan = $(td).find('.text-span');

  // Event listener to switch to edit mode when the span is clicked
  textSpan.on('click', function () {
    if (isEditClicked === false) return;

    if ((cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_IN
      || cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_OUT)
      && policies.changeCheck === false) {
      const message = $("#_noRightsToEditCheckInOutTimeMessage").val();
      toastr.warning(message);
      return;
    }

    if ($('.edit-input').length > 0) return; // An element with class ".edit-input" already exists

    textSpan.hide();
    let editInput = prepareInputField(cellType, cellData, cellContainer);
    cellContainer.append(editInput);
    editInput.focus().select();
    if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TRAINING) editInput.click();

    //Event listener to switch back to text mode
    editInput.on('blur', function () {
      textSpan.show();
      editInput.remove();
    });

    if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TRAINING) {
      editInput.on('change keyup', async function (event) {
        const newValue = editInput.val();
        if (await handleSavingShiftForm(td, rowData, cellType, newValue)) {
          cellData = newValue == "true"; // INFO: val function returns a string
          textSpan.text(cellData === true ? "Yes" : "No").show();
          editInput.remove();
        }
      });
    }
    else {
      editInput.on('focusout keypress', async function (e) {
        if (e.type === "keypress" && e.key !== "Enter") return;

        let newValue = editInput.val();
        if (await handleSavingShiftForm(td, rowData, cellType, newValue)) {
          cellData = newValue;
          editInput.remove();

          if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_IN
            || cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_OUT) {
            newValue += ":00";
            newValue = newValue.substring(0, 5);
          }
          else if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.COMMENT && newValue === "") newValue = '&nbsp;';
          textSpan.html(newValue).show();
        }
      });
    }
  });
}

function prepareInputField(cellType, cellData, cellContainer) {
  let editInput;
  let elementType;

  if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TRAINING) {
    elementType = "select";

    editInput = $('<select>', { class: 'edit-input form-select-sm' });
    const optionYes = $('<option>', { value: true, text: 'Yes', selected: cellData ? true : false });
    const optionNo = $('<option>', { value: false, text: 'No', selected: !cellData ? true : false });
    editInput.append(optionYes, optionNo);
  }
  else {
    if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.COMMENT) elementType = "text";
    else elementType = "time";

    editInput = $('<input>', { type: elementType, class: 'edit-input form-control-sm', value: cellData, maxLength: 30 });
  }

  if (elementType === "text") {
    const fullSizeOfParentConatiner = cellContainer.width();
    editInput.width(fullSizeOfParentConatiner);
  }

  return editInput;
}

async function handleSavingShiftForm(td, rowData, cellType, newValue) {
  let shiftFormDto = { ...rowData, policyRoundTime: policies.roundtime, cellType };

  if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_IN) {
    if (newValue.length < 6) newValue += ":00";
    if (rowData.timeIn.substring(0, 5) === newValue.substring(0, 5)) return true;
    shiftFormDto.newTimeIn = newValue;
  }
  else if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TIME_OUT) {
    if (newValue.length < 6) newValue += ":00";
    if (rowData.timeOut.substring(0, 5) === newValue.substring(0, 5)) return true;
    shiftFormDto.timeOut = newValue;
  }
  else if (cellType === STRING_TABLE.TABLE.TIMESHEET_DETAILS.COLUMN_NAME.TRAINING) {
    newValue = newValue == "true" ? true : false
    shiftFormDto.training = newValue;
  }
  else shiftFormDto.comment = newValue;

  let updated = false;
  const url = STRING_TABLE.URL.UPDATE_SHIFT_FORM;

  await ajaxPostAsync(url, shiftFormDto)
    .then(data => {
      if (data.updated) {
        rowData[cellType] = newValue;
        updated = data.updated;

        const row = $(td).closest('tr');
        row.find('.hours').text(data.hours.toFixed(2));
        row.find('.commission').text(data.commission.toFixed(2));
      }
    })
    .catch(toastrError);

  return updated;
}

function handleBackButtonClicked() {
  safelyNavigateTo(STRING_TABLE.URL.CHECK_IN_CHECK_OUT);
}

let displayTimesheetReport = () => {
  let language = $('#language').val();
  let reportName = 'TimesheetReport';
  let width = 1000;
  let height = window.screen.height - 100;
  let data = {};

  data.reportName = reportName;
  data.language = language;
  data.payPeriod = policies.payPeriod;
  data.startingDate = selectedEmployeeHeaderDetails.periodStarting;
  data.reportWidth = "950px";
  data.reportHeight = window.screen.height - 150;
  data.reportId = selectedEmployeeHeaderDetails.reportId;
  data.userCode = selectedEmployeeHeaderDetails.userCode;
  data.companyName = companyName;
  data.priorOS = $("#details--prior-os").text();
  data.overtimeHPolicy = policies.overtimeH;
  data.overtimeSPolicy = policies.overtimeS;
  displayReport(reportName, width, height, data);
}