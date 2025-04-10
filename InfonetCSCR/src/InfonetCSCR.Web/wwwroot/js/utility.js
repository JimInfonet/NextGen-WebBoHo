let _globalIsEditing = localStorage.getItem("_globalIsEditing") == 'true' ? true : false;
let _reportPopUpWindow = null;
let _searchValue = "";
let _stockCodeDatatable;
let _globalStockCodeData;
let recentClickedNavLink;
let recentClickedMenu;
let modalTableRowHeight = ADD_ITEMS_TO_THE_PO_STRING_TABLE.MODAL_DATA_TABLE_ROW_DIMENSION.HEIGHT;
const _globalIsBackOffice = $("#_globalIsBackOffice").val().toLowerCase() === 'true' ? true : false;

let renderCheckBox = (control, data, allDisabled = false) => {
  let id = control[0].id;
  data.forEach((element, index) => {
    const checkBoxControl =
      `<div class="form-check">
        <input class="form-check-input" type="checkbox" value="${element.value}" id="${id}_${element.value}" ${(element.isSelected ? 'checked' : '')} onclick="${id}_Click(this)" ${(allDisabled ? `disabled` : ``)}>
        <label class="form-check-label" for="${id}_${element.value}">${element.value} - ${element.name}</label>
      </div>`;
    control.append(checkBoxControl);
  })
}

let makeFixedDecimalDigits = (value, numberOfDigits) => {
  const decimalIndex = value.toString().indexOf(".");
  if (decimalIndex === -1) {
    value = value.toString() + "." + "0".repeat(numberOfDigits);
  } else {
    value = value.toString().padEnd(decimalIndex + numberOfDigits + 1, "0");
  }

  return value;
}

let renderSelectOptionDefault = (control, title, isDefaultSelected = false) => {
  const option = `<option disabled ${(isDefaultSelected ? 'selected' : '')}>${title}</option>`;
  control.append(option);
}

let renderSelectOptions = (control, data) => {
  data.forEach((element) => {
    let label = (element.value === 'ALL') ? $("#allText").val() : `${element.value}${(element.name ? ` - ${element.name}` : ``)}`;
    control.append(`<option value="${element.value}" ${(element.isSelected ? 'selected' : '')}>${label}</option>`);
  });
}

let ajaxGetAsync = async (url, data) => {
  return await $.ajax({
    type: 'GET',
    url: `${url}`,
    dataType: 'json',
    contentType: "application/json; charset=utf-8",
    data: data,
    success: function (response) {
      return response;
    },
    error: function (error) {
      return error;
    }
  });
}

let ajaxGetAsyncAsString = async (url, data) => {
  return await $.ajax({
    type: 'GET',
    url: `${url}`,
    dataType: 'text',
    contentType: "application/json; charset=utf-8",
    data: data,
    success: function (response) {
      return response;
    },
    error: function (error) {
      return error;
    }
  });
}

let ajaxPostAsync = async (url, data) => {
  return await $.ajax({
    type: 'POST',
    url: `${url}`,
    dataType: 'json',
    contentType: "application/json; charset=utf-8",
    data: JSON.stringify(data),
    success: function (response) {
      return response;
    },
    error: function (error) {
      return error;
    }
  });
}

function getCurrentDate() {
  const today = new Date();
  let dd = today.getDate();
  let mm = today.getMonth() + 1;
  const yyyy = today.getFullYear();

  if (dd < 10) {
    dd = '0' + dd;
  }

  if (mm < 10) {
    mm = '0' + mm;
  }

  return yyyy + '-' + mm + '-' + dd;
}

//format date to mm/dd/year
function getFormattedDate(date) {
  return (((date.getMonth() > 8) ? (date.getMonth() + 1) : ('0' + (date.getMonth() + 1))) + '/' + ((date.getDate() > 9) ? date.getDate() : ('0' + date.getDate())) + '/' + date.getFullYear());
}

//format date to year-mm-dd
function getFormattedDateyyyyMMdd(date) {
  return (
    date.getFullYear() +
    "-" +
    ((date.getMonth() + 1) < 10 ? "0" : "") +
    (date.getMonth() + 1) +
    "-" +
    (date.getDate() < 10 ? "0" : "") +
    date.getDate()
  );
}

//format date to HH:mm
function convertUtcToHHmm(utcTimeString) {
  const utcDate = new Date(Date.UTC(
    parseInt(utcTimeString.substr(0, 4)),   // Year
    parseInt(utcTimeString.substr(5, 2)) - 1, // Month (0-indexed)
    parseInt(utcTimeString.substr(8, 2)),      // Day
    parseInt(utcTimeString.substr(11, 2)),     // Hours
    parseInt(utcTimeString.substr(14, 2)),     // Minutes
    parseInt(utcTimeString.substr(17, 2))      // Seconds
  ));

  const hours = utcDate.getUTCHours();
  const minutes = utcDate.getUTCMinutes();

  const formattedHours = hours < 10 ? `0${hours}` : hours;
  const formattedMinutes = minutes < 10 ? `0${minutes}` : minutes;

  return `${formattedHours}:${formattedMinutes}`;
}

// Expected format hh:mm
function convert24to12Hour(time) {
  const timeArray = time.split(":");
  let hours = parseInt(timeArray[0]);
  let minutes = parseInt(timeArray[1]);

  const period = hours >= 12 ? "PM" : "AM";

  // Convert hours to 12-hour format
  hours = hours % 12 || 12;

  hours = hours < 10 ? `0${hours}` : hours;
  minutes = minutes < 10 ? `0${minutes}` : minutes;

  return hours + ":" + minutes + " " + period;
}

//expected fromat mm/dd/yyyy and convert to yyyy-mm-dd
function convertMMDDYYYYToUtc(dateString) {
  if (dateString.split('/').length !== 3)
    return NaN;

  const [month, day, year] = dateString.split('/');
  const utcDateString = `${year.padStart(4, '0')}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;

  if (isNullUndefinedOrWhiteSpace(year) || isNullUndefinedOrWhiteSpace(month) || isNullUndefinedOrWhiteSpace(day))
    return NaN;

  return utcDateString;
}

function replaceDashWithSlash(text) {
  return text.replace(/-/g, '/');
}

function isDateInPast(selectedDate) {
  let currentDate = new Date();
  selectedDate.setHours(0, 0, 0, 0);
  currentDate.setHours(0, 0, 0, 0);

  return selectedDate < currentDate;
}

$('input[type="date"]').on("input", function () {
  const year = $(this).val().split("-")[0];
  if (year.length > 4) {
    // removing the first digit
    $(this).val($(this).val().substring(1));
  }
});

$('input[type="number"]').on("keypress", function (e) {
  if (e.key === 'Enter' && e.target.validity.badInput) {
    e.preventDefault();
    e.stopImmediatePropagation();
    toastr.warning($("#_invalidNumberWarningMessage").val());
    $(this).focus();
  }
});

function toastrError(message) {
  toastr.error($("#_defaultErrorMessage").val());
}

function toastrInfo(message) {
  toastr.info(message);
}

function setGlobalEditingState(globalIsEditing) {
  _globalIsEditing = globalIsEditing;
  localStorage.setItem("_globalIsEditing", globalIsEditing);
}

function isNullOrWhiteSpace(value) {
  return value === null || value.trim() === "";
}

function parseDoubleFromUI(selector, roundTo = 0) {
  let value = parseFloat($(selector).text().trim());
  return roundTo === 0 ? value : value.toFixed(roundTo);
}

function createDataTableProp() {
  return {
    columns: [],
    columndefs: [],
    data: null,
    serverSide: false,
    searching: true,
    ordering: false,
    paging: true,
    bInfo: true,
    pageLength: 25,
    stateSave: false,
    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "ALL"]], // [[# of entries], [labels]]
    ajax: null,
    callOnDrawCallback: null,
  };
}

function createDataTableGeneric(tableId, props = {}, limitHeight = true, pagination = true) {
  const zeroRecordsMessage = $("#zeroRecordsMessage").val();
  const emptyMessage = $("#emptyMessage").val();
  const loading = $("#loading").val();
  const infoMessage = pagination ? $("#info").val() : $("#infoForpaginationFalse").val();
  const infoEmptyMessage = pagination ? $("#infoEmpty").val() : $("#infoEmptyForPaginationFalse").val();
  const infoFilteredMessage = $("#infoFiltered").val();
  const lengthMenuMessage = $("#lengthMenu").val();
  const serverSide = props.serverSide;

  let callOnDrawCallback = null;
  if (props.hasOwnProperty('callOnDrawCallback')) {
    callOnDrawCallback = props.callOnDrawCallback;
    delete props.callOnDrawCallback;
  }

  let dataTable = $(`#${tableId}`).DataTable({
    processing: true,
    fixedHeader: {
      header: true,
      footer: true
    },
    scrollY: limitHeight ? 'auto' : '',
    scrollCollapse: limitHeight,
    language: {
      zeroRecords: zeroRecordsMessage,
      sEmptyTable: emptyMessage,
      info: infoMessage,
      infoEmpty: infoEmptyMessage,
      infoFiltered: infoFilteredMessage,
      lengthMenu: lengthMenuMessage,
      paginate: {
        previous: '<iconify-icon icon="akar-icons:arrow-left"></iconify-icon>',
        next: '<iconify-icon icon="akar-icons:arrow-right"></iconify-icon>'
      },
      processing: `<i class="col fa fa-spinner fa-spin fa-3x fa-fw justify-content-end text-right" style="color:#2a2b2b;"></i><span class="sr-only">${loading}...</span>`
    },
    drawCallback: function () {
      if (callOnDrawCallback != null) {
        callOnDrawCallback();
      }

      if (limitHeight) {
        const pageLength = parseInt(this.api().page.len());
        const pageNumber = this.api().page.info().page;
        const totalRows = this.api().rows({ search: 'applied' }).count();
        if (totalRows > 0) {
          let totalHeight = 0;
          let id = 1;
          let startId = serverSide ? 0 : pageNumber * pageLength;
          let endId = Math.min(startId + 10, totalRows);
          this.api().rows({ search: 'applied' }).every(function () {
            if (id > startId && id <= endId) {
              totalHeight += ($(this.node()).outerHeight() ? $(this.node()).outerHeight() : modalTableRowHeight);
            }
            id++;
          });

          //For longer datatable, DT gets a x-axis scrollbar which takes up some height and adds a y-axis scrollbar
          //we are adding some height to max-height for 1-10 rows so it does not change DT height when we change the entry count.
          if (totalRows <= 10)
            totalHeight += 20;

          $(`#${tableId}_wrapper .dataTables_scrollBody`).css('max-height', totalHeight);
        }
        else {
          $(`#${tableId}_wrapper .dataTables_scrollBody`).css('height', 'auto');
        }
      }
    },
    initComplete: function (settings, json) {
      $(`#${tableId}_wrapper .dataTables_scrollHead`).attr('id', `${tableId}__header`);
      $(`#${tableId}_wrapper .dataTables_scrollBody`).attr('id', `${tableId}__body`);
      $(`#${tableId}_wrapper .dataTables_scrollFoot`).attr('id', `${tableId}__footer`);
      $(`#${tableId}_wrapper .dataTables_scroll`).attr('id', `${tableId}__scroll`);

      let datatableHeader = document.querySelector(`#${tableId}__header`);
      let datatableBody = document.querySelector(`#${tableId}__body`);
      let datatableFooter = document.querySelector(`#${tableId}__footer`);
      let newDiv = document.createElement('div');

      newDiv.className = 'dataTable-head-body-container';
      newDiv.style.overflow = 'visible';

      if (datatableHeader !== null && datatableBody !== null) {
        datatableBody.onscroll = function () {
          datatableHeader.scrollLeft = this.scrollLeft;
        }
      }

      if (datatableHeader !== null) {
        datatableHeader.className = 'dataTable-head-container';
        datatableHeader.style.overflow = 'visible';

        newDiv.appendChild(datatableHeader);
      }
      if (datatableBody !== null) {
        datatableBody.id = 'dataTable-body-container';
        datatableBody.style.overflow = 'visible';
        newDiv.appendChild(datatableBody);
      }

      let newContainer = document.createElement('div');
      newContainer.className = 'dataTables_scroll';
      newContainer.appendChild(newDiv);
      if (datatableFooter !== null) {
        newContainer.appendChild(datatableFooter);
      }
      let existingContainer = document.querySelector(`#${tableId}__scroll`);
      if (existingContainer !== null) {
        existingContainer.parentNode.replaceChild(newContainer, existingContainer);
      }

      $(`#${tableId}_filter`).css({ "display": "none" });
    },
    ...props,
  });

  dataTable.columns.adjust();
  alignDataTableInfoAndPagination(tableId);
  return dataTable;
}

function createDatatableWithSearching(selector, props = {}, limitHeight = true) {
  let dataTable = createDataTableGeneric(selector, props, limitHeight);
  // Add a input element for each footer column having the class "input" 
  let id = 0;
  $($(dataTable.table().footer()).html()).find("th").each(function () {
    let title = $(dataTable.column(id).header()).text();
    $(dataTable.column(id).footer()).html(`<input class="form-control p-2" type="text" id="${title}" placeholder="${$("#search").val()} ${title}"/>`);
    id++;
  });

  dataTable.columns().every(function () {
    const dataTableColumn = this;
    const searchInput = $(this.footer()).find('input');

    searchInput.on('keyup change', function () {
      // Clear the search input fields of other columns
      dataTable.columns().every(function () {
        if (this.index() !== dataTableColumn.index()) {
          let searchString = $(this.footer()).find('input').val();
          if (searchString?.length) {
            $(this.footer()).find('input').val('');
            dataTable.column(this.index()).search('').draw();
          }
        }
      });

      _searchValue = searchInput.val().trim();

      // Perform the search on the current column with debouncing
      const performSubstringSearch = $(searchInput).closest('th').hasClass('substring-search');
      debouncedSearch(dataTableColumn, performSubstringSearch);
    });
  })
  return dataTable;
}

const debouncedSearch = debounce(function (dataTableColumn, performSubstringSearch = false) {
  dataTableColumn.search(performSubstringSearch ? _searchValue : '^' + _searchValue, !performSubstringSearch).draw();
});

function debounce(func, timeout = 1000) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => {
      func.apply(this, args);
    }, timeout);
  }
}

function destroyDataTableIfExists(dataTable) {
  if (dataTable != null) {
    dataTable.state.clear();
    dataTable.clear();
    dataTable.destroy();
  }
}

function removeURLParameter(url, parameter) {
  const urlParts = url.split('?');
  if (urlParts.length >= 2) {
    const prefix = encodeURIComponent(parameter) + '=';
    const params = urlParts[1].split(/[&;]/g);

    for (let i = params.length - 1; i >= 0; i--) {
      if (params[i].lastIndexOf(prefix, 0) !== -1) {
        params.splice(i, 1);
      }
    }

    return urlParts[0] + (params.length > 0 ? '?' + params.join('&') : '');
  } else {
    return url;
  }
}

function replaceNullAndNoneWithEmptyString(array) {
  array.forEach(obj => {
    Object.keys(obj).forEach(prop => {
      if (obj[prop] === null || obj[prop] === "<NONE>") {
        obj[prop] = '';
      }
    });
  });
}

function redirectToField(fieldId) {
  const field = document.getElementById(fieldId);
  if (field) {
    field.scrollIntoView({ behavior: "smooth", block: "center" });
    field.classList.add('highlight-for-error');
    field.focus();
    field.addEventListener('focusout', removeHighlight);
  }
}

function removeHighlight(event) {
  const field = event.target;
  field.classList.remove('highlight-for-error');
  field.removeEventListener('focusout', removeHighlight);
}

function isInteger(value) {
  const integerPattern = /^\d+$/;
  return integerPattern.test(value);
}

function caseInsensitiveComparison(a, b) {
  return a.toLowerCase() === b.toLowerCase();
}

let isNullOrUndefined = (value) => {
  return value === null || value === undefined;
}

let isNullUndefinedOrWhiteSpace = value => {
  return value === null || value === undefined || value === "";
}

async function getCompanyName() {
  let url = `${window.location.origin}/CompanySetup/GetCompanyName`;
  let companyName = await ajaxGetAsync(url, {});
  return encodeURIComponent(companyName);
}

let addEventListernerToDateField = (dateTimeInput) => {
  dateTimeInput.addEventListener('input', (e) => {
    if (dateTimeInput.value > '2079-06-06') {
      dateTimeInput.value = '2079-06-06';
    }
  });
}

let displayReport = (windowName, width, height, data) => {          // The data parameter must have reportName Property
  if (_reportPopUpWindow == null || _reportPopUpWindow.closed) {
    let left = (screen.width / 2) - (width / 2);
    let top = (screen.height / 2) - (height / 2);
    let options = "toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=" + width + ", height=" + height + ", top=" + top + ", left=" + left;
    _reportPopUpWindow = window.open(`${window.location.origin}/Report/ShowReport`, windowName, options);
    _reportPopUpWindow.focus();

    _reportPopUpWindow.onload = function () {
      _reportPopUpWindow.postMessage(data, $("#_webBaseUrl").val());
    };
  }
  else {
    let left = (screen.width / 2) - (width / 2);
    let top = (screen.height / 2) - (height / 2);
    _reportPopUpWindow.resizeTo(width, height);
    _reportPopUpWindow.moveTo(left, top);
    _reportPopUpWindow.focus();

    if (_reportPopUpWindow.document.readyState === "complete") {
      _reportPopUpWindow.postMessage(data, $("#_webBaseUrl").val());
    }
    else {
      _reportPopUpWindow.onload = function () {
        _reportPopUpWindow.postMessage(data, $("#_webBaseUrl").val());
      };
    }
  }
}

$('.modal').on('shown.bs.modal', function () {
  $(this).on('keypress', function (e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      const okayButton = $(this).find('.btn-primary');

      if (okayButton.length > 0) {
        okayButton.click(); // Simulate a click on the "OK" button
      }
    }
  });
});

function getCookie(cookieName) {
  const cookies = document.cookie.split('; ');
  for (const cookie of cookies) {
    const parts = cookie.split('=');
    const name = parts[0];
    if (name === cookieName) {
      return parts[1]; // Return the cookie value
    }
  }
  return null; // Cookie not found
}

function openCenteredWindow(url, windowName, width, height) {
  const left = (window.innerWidth - width) / 2;
  const top = (window.innerHeight - height) / 2;
  const features = `width=${width},height=${height},left=${left},top=${top}`;
  return window.open(url, windowName, features);
}

async function safelyNavigateTo(route) {
  if (_globalIsEditing) {
    const title = $("#_goBackConfirmationTitle").val();
    const message = $("#_goBackConfirmationMessage").val();
    if (!(await showConfirmModal(title, message))) return;
  }

  window.location.href = route;
}

function createModalDatatableWithSearching(selector, props = {}) {
  let dataTable = createDataTableGeneric(selector, props, false);
  // Add a input element for each footer column having the class "input" 
  let id = 0;
  $($(dataTable.table().footer()).html()).find("th").each(function () {
    let title = $($(dataTable.table().header()).html()).find("th").eq($(this).index()).text();
    $(dataTable.column(id++).footer()).html(`<input class="form-control p-2" style="width: -webkit-fill-available" type="text" id="${title}" placeholder="${$("#search").val()} ${title}"/>`);
  })

  dataTable.columns().every(function () {
    const dataTableColumn = this;
    const searchInput = $(this.footer()).find('input');

    searchInput.on('keyup change', function () {
      // Clear the search input fields of other columns
      dataTable.columns().every(function () {
        if (this.index() !== dataTableColumn.index()) {
          let searchString = $(this.footer()).find('input').val();
          if (searchString?.length) {
            $(this.footer()).find('input').val('');
            dataTable.column(this.index()).search('').draw();
          }
        }
      });

      _searchValue = searchInput.val();
      // Perform the search on the current column with debouncing
      const performSubstringSearch = $(searchInput).closest('th').hasClass('substring-search');
      debouncedSearch(dataTableColumn, performSubstringSearch);

    });
  })
  return dataTable;
}

const debouncedSubstringSearch = debounce(function (dataTableColumn) {
  dataTableColumn.search(_searchValue).draw();
});

const debouncedPrefixSearch = debounce(function (dataTableColumn) {
  dataTableColumn.search('^' + _searchValue, true).draw();
});

let capitalizeFirstLetter = (str) => {
  return str.charAt(0).toUpperCase() + str.slice(1);
};

const addSeeMoreBtnInGridData = function (data, type, row, meta, maxCharactersPerLine) {
  // Construct the regular expression dynamically to add line break after consecutive maxCharactersPerLine characters
  let regex = new RegExp('(.{' + maxCharactersPerLine + '})', 'g');
  let formattedData = data.replace(regex, "$1<br>");
  let renderedData = formattedData.substring(0, maxCharactersPerLine);
  // Add "See more" link
  return renderedData + ` <span class="see-more" data-full-content="${formattedData}">
                            <a href="#">${$("#seeMore").val()}</a>
                          </span>`;
}

let isObjectsEqual = (obj1, obj2) => {
  const keys1 = Object.keys(obj1);
  const keys2 = Object.keys(obj2);
  if (keys1.length !== keys2.length) {
    return false;
  }

  for (let key of keys1) {
    if (obj1[key] !== obj2[key]) {
      return false;
    }
  }

  return true;
}

function renderStockCodeDataTable(onStockCodeRowClick) {
  destroyDataTableIfExists(_stockCodeDatatable);

  let props = createDataTableProp();
  props.data = _globalStockCodeData;
  props.scrollY = '300px';
  props.paging = false;
  props.autoWidth = false;
  props.fixedColumns = true;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] },
    { "width": "205px", "targets": [0, 1] },
  ];
  props.columns = [
    { "data": "stockCode", },
    { "data": "description", },
  ];

  _stockCodeDatatable = createModalDatatableWithSearching('stockCodeDescriptionDataTable', props);

  const childId = "stockCodeDescriptionDataTable_info";
  const childDiv = document.getElementById(childId);

  if (childDiv) {
    const parentDiv = childDiv.parentElement;
    parentDiv.removeAttribute('class');
    parentDiv.classList.add('row');
  }

  $(`#stockCodeDescriptionDataTable_wrapper .dataTables_scrollBody`).css('overflow-x', 'hidden');
  $(`#stockCodeDescriptionDataTable_wrapper .dataTables_scrollFootInner`).css('width', '100%');
  $(`#stockCodeDescriptionDataTable_wrapper .dataTables_scrollFootInner .dataTable`).css('width', '100%');
  $(`#stockCodeDescriptionDataTable_wrapper .dataTables_scrollHeadInner`).css('width', '100%');
  $(`#stockCodeDescriptionDataTable_wrapper .dataTables_scrollHeadInner .dataTable`).css('width', '100%');

  $('#stockCodeDescriptionDataTable tbody')
    .off()
    .on('click', 'tr', function () {
      const tableinfo = _stockCodeDatatable.page.info();

      if (tableinfo.recordsDisplay === 0)
        return;

      _stockCodeDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
      $(this).addClass('selected-grid-row');

      let rowData = _stockCodeDatatable.row(this).data();
      onStockCodeRowClick(rowData);
    });
}

let alignDataTableInfoAndPagination = (gridName) => {
  const gridInfoDiv = document.getElementById(`${gridName}_info`);

  if (gridInfoDiv) {
    const parentgridInfoDiv = gridInfoDiv.parentNode;
    parentgridInfoDiv.classList.remove('col-md-5');
    parentgridInfoDiv.classList.add('col-md-12');
  }

  const gridPaginateDiv = document.getElementById(`${gridName}_paginate`);
  if (gridPaginateDiv) {
    const parentgridPaginateDiv = gridPaginateDiv.parentNode;
    parentgridPaginateDiv.classList.remove('col-md-7');
    parentgridPaginateDiv.classList.add('col-md-12');
    parentgridPaginateDiv.classList.add('py-2');
  }
}

function setButtonsHeight(element) {
  const buttons = document.querySelectorAll(element);
  let maxHeight = 0;

  for (const button of buttons) {
    const buttonHeight = button.offsetHeight;
    maxHeight = Math.max(maxHeight, buttonHeight);
  }

  for (const button of buttons) {
    button.style.height = `${maxHeight}px`;
  }
}

function convertTo12HourFormat(timeString) {
  const [hours, minutes, seconds] = timeString.split(':').map(Number);
  const date = new Date();
  date.setHours(hours, minutes, seconds ?? 0);

  return date.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: true
  });
}

function convertTo24HourFormat(timeString) {
  const [time, modifier] = timeString.split(' ');
  let [hours, minutes, seconds] = time.split(':').map(Number);;

  if (modifier === 'PM' && hours !== 12) {
    hours += 12;
  } else if (modifier === 'AM' && hours === 12) {
    hours = 0;
  }

  hours = String(hours).padStart(2, '0');
  minutes = String(minutes).padStart(2, '0');
  seconds = String(seconds).padStart(2, '0');

  return `${hours}:${minutes}:${seconds}`;
}

function startOrEndLoader(buttonId, buttonText, isStart) {
  if (isStart) {
    $(`#${buttonId}`).html(`<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> ${buttonText}...`);
  }
  else {
    $(`#${buttonId}`).text(buttonText);
  }

  $(`#${buttonId}`).prop('disabled', isStart);
}

let convert12to24Hour = (time12h) => {
  let [time, period] = time12h.split(' ');
  let [hours, minutes] = time.split(':');

  if (period === 'AM' && hours === '12') {
    hours = '00';
  } else if (period === 'PM' && hours !== '12') {
    hours = String(Number(hours) + 12);
  }

  return hours.padStart(2, '0') + ':' + minutes;
}

function createPdfDefinition() {
  return {
    content: [],
    defaultStyle: {
      font: 'Courier'
    },
    styles: {
      preformatted: {
        font: 'Courier',
        fontSize: 6,
        alignment: 'center'
      }
    },
    pageSize: 'A4',
    pageMargins: [30, 60, 30, 60]
  };
}

function printWithPdfMake(pdfDefinition) {
  pdfMake.createPdf(pdfDefinition).print();
}