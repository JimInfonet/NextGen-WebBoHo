const URL_GET_REP_TILL_CLOSE = "/RepTillClose/";
const STRING_TABLE = REP_TILL_CLOSE_STRING_TABLE;
let policies;
let tillClosesDatatable;
let tillCloses = [];
let CloseNo;
let TillNo;
let ShiftNo;

let rowDoubleClick = CONSTANT_TEXTS.ROW_DOUBLE_CLICK;

$(async function () {
    await loadTillCloses();
});

let loadTillCloses = async () => {
    var days = document.getElementById("TillCDays").value;
    let date1 = new Date();
    let date2 = new Date();
    date1.setDate(date2.getDate() - days);

    let url = URL_GET_REP_TILL_CLOSE + "GetRepTillCloses?date1=" + formatDateToMMDDYYYY(date1) + "&date2=" + formatDateToMMDDYYYY(date2);
    //let errorMessage = CloseBatchDays ? $("#anErrorHasOccurredWhileLoadingMembers").val()
    //    : $("#anErrorHasOccurredWhileLoadingCustomers").val();

    await ajaxGetAsync(url)
        .then(_tillCloses => {
            tillCloses = _tillCloses;
            createTillClosesDatatable();
        })
        .catch(() => {
            const errorMessage = "An error occurred while loading data";
            toastr.error(errorMessage);
        });

    // Initailize shift numbers of the shift date
    //await loadShifts();

    var sDate = document.getElementById("shift-date").value;
    if (!isNullOrUndefined(sDate) && sDate != '')
        GetShiftNos(sDate)
}
let loadShifts = async () => {
    return await $.ajax({
        type: "GET",
        dataType: 'json',
        url: STRING_TABLE.URL.GET_SHIFTS,

        success: function (result) {
            if (result === null) return;
            let shiftSelection = $('#shiftCombo');
            let optionTemplate = $('<option></option>');
            shiftSelection.val('All')

            //for (let i = 0; i < result.length; i++) {
            //    let option = optionTemplate.clone();
            //    option.append(result[i] + " - " + result[i]);
            //    option.attr('value', result[i]);
            //    shiftSelection.append(option);
            //}
        },
        error: function () {
            toastr.error(errorMessage);
        }
    })
}

document.addEventListener('DOMContentLoaded', function () {
    // Get the current date and time
    var now = new Date();

    // Format the date as YYYY-MM-DD
    var year = now.getFullYear();
    var month = String(now.getMonth() + 1).padStart(2, '0');
    var day = String(now.getDate()).padStart(2, '0');
    var formattedDate = year + '-' + month + '-' + day;

    // Format the time as HH:MM
    var hours = String(now.getHours()).padStart(2, '0');
    var minutes = String(now.getMinutes()).padStart(2, '0');
    var formattedTime = hours + ':' + minutes;

    //// Set the date and time in the input fields
    //document.getElementById('end-date').value = formattedDate;
    ////document.getElementById('end-time').value = formattedTime;
    //document.getElementById('end-time').value = "23:59";
    //document.getElementById('start-date').value = formattedDate;
    //document.getElementById('start-time').value = "00:00";
    //document.getElementById('shift-date').value = formattedDate;
});

// Add event listener to the date picker
const datePicker = document.getElementById('shift-date').value;
if (!isNullOrUndefined(datePicker))
    document.getElementById('shift-date').addEventListener('change', handleDateChange);

let loadStores = async () => {
    return await $.ajax({
        type: "GET",
        dataType: 'json',
        url: "/Store/GetAllStores",

        success: function (result) {
            if (result === null) return;
            let storeSelection = $('#selectStore');
            let optionTemplate = $('<option></option>');
            storeSelection.val('All')

            for (let i = 0; i < result.length; i++) {
                let option = optionTemplate.clone();
                option.append(result[i].store_ID + " - " + result[i].store_Desc);
                option.attr('value', result[i].store_ID.split(" ")[0]);
                storeSelection.append(option);
            }
        },
        error: function () {
            toastr.error(errorMessage);
        }
    })
}

function handleDateChange(event) {
    const selectedDate = event.target.value;
    console.log('Selected date:', selectedDate);

    var sDate = document.getElementById("shift-date").value;
    if (!isNullOrUndefined(sDate))
        GetShiftNos(sDate)
}

let createTillClosesDatatable = async () => {
    if (tillClosesDatatable) {
        tillClosesDatatable.state.clear();
        tillClosesDatatable.clear();
        tillClosesDatatable.destroy();
    }

    let props = createDataTableProp();
    props.data = tillCloses;
    props.scrollY = '440px';
    props.columnDefs = [
        { className: 'text-left', targets: [0, 1, 2, 3, 4, 5, 6, 7] }
    ];

    props.columns = [
        //the first letter of the column name should be lowercase and first letter of the next words is uppercase
        { "data": "close_Num", },
        { "data": "open_Date", },
        { "data": "open_Time", },
        { "data": "close_Date", },
        { "data": "close_Time", },
        { "data": "till_Num", },
        { "data": "shiftDate", },
        { "data": "shiftNumber", },
    ];

    tillClosesDatatable = createDatatableWithSearching('tillCloses-grid', props);

    $('#tillCloses-grid tbody')
        .off() //turn off the event before turning it on to prevent firing click event multiple times
        .on('click', 'tr', function () {

            const tableinfo = tillClosesDatatable.page.info();

            if (tableinfo.recordsDisplay === 0 || tillCloses.length === 0) {
                return;
            }

            if ($(this).hasClass('selected-grid-row')) {
                $(this).removeClass('selected-grid-row');
                selectedCode = null;
                //$("#btn-customer-card").prop("disabled", true);
            } else {
                tillClosesDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
                $(this).addClass('selected-grid-row');

                CloseNo = $(this).find('td:eq(0)').text();
                TillNo = $(this).find('td:eq(5)').text();
                ShiftNo = $(this).find('td:eq(7)').text();
            }
            fetchData(CloseNo, TillNo, ShiftNo)
        })
        .on('dblclick', 'tr', function () {
            const tableinfo = tillClosesDatatable.page.info();

            if (tableinfo.recordsDisplay === 0 || tillCloses.length === 0) {
                return;
            }

            selectedCode = $(this).find("td:first")[0].innerText;
            const url = `/RepTillClose?Code=${selectedCode}&ActionMode=${rowDoubleClick}`;
            safelyNavigateTo(url);
        });
}

async function fetchData(closeNo, tillNo, shiftNo) {
    let errorMessage = "";
    let report = "";
    const url = URL_GET_REP_TILL_CLOSE + "GetRepByTillNo";

    try {
        const params = { closeNo, tillNo, shiftNo }

        showHourglassCursor(); // Show loading

        let resultString = await ajaxPostAsync(url, params);

        const textarea = document.getElementById("TillCloseTextArea");

        if (textarea) {
            textarea.value = "";
            textarea.style.fontFamily = "Courier New, monospace";
        }

        //Loop through the array and append each element to the textarea with a newline
        for (let i = 0; i < resultString.length; i++) {
            textarea.value += resultString[i] + '\n';
        }
        document.getElementById('btn-export').disabled = false;
        document.getElementById('btn-print').disabled = false;

    } catch (error) {
        errorMessage = $('#error-message-for-list-load').val();
        //toastr.Error(errorMessage);
        const textarea = document.getElementById("TillCloseTextArea");
        textarea.value = error.responseText;

    } finally {
        hideHourglassCursor(); // Show loading
    }
}

function refreshTable(groupToShow) {
    var groupFrame1 = document.getElementById("groupTill");
    var groupFrame2 = document.getElementById("groupShift");
    var groupFrame3 = document.getElementById("groupDate");

    if (groupToShow === "option2") {
        var shiftDate = document.getElementById("shift-date").value;
        var shiftNo = document.getElementById("shiftCombo").value;
        var startdate = document.getElementById("start-date").value;
        var enddate = document.getElementById("end-date").value;
        runRepByShift(shiftDate, shiftNo, startdate, enddate);

    } else if (groupToShow === "option3") {
        let [hour, minute] = document.getElementById("end-time").value.split(':');

        hour = parseInt(hour, 10);
        let period = hour >= 12 ? 'PM' : 'AM';
        hour = hour % 12;
        hour = hour ? hour : 12;

        //var startdate = document.getElementById("start-date").value + "%20" + document.getElementById("start-time").value;
        //var enddate = document.getElementById("end-date").value + "%20" + hour + ":" + minute + "%20" + period;
        var startdate = document.getElementById("start-date").value;
        var starttime = document.getElementById("start-time").value;
        var enddate = document.getElementById("end-date").value;
        var endtime = document.getElementById("end-time").value;

        runRepByDates(startdate, starttime, enddate, endtime);
    }
}

async function exportButtonClicked() {
    const url = URL_GET_REP_TILL_CLOSE + "GetExportToExcel";
    window.location = url;
    //try {

    //    showHourglassCursor();

    //    const resultString = await ajaxGetAsync(url);
    //    hideHourglassCursor();

    //} catch (error) {
    //    hideHourglassCursor();
    //    errorMessage = $('#error-message-for-list-load').val();
    //    toastr.Error(errorMessage);

    //}

}

async function printButtonClicked() {
    const url = URL_GET_REP_TILL_CLOSE + "GetPrintReport";

    const params = { policies }

    try {

        showHourglassCursor();

        const resultString = await ajaxPostAsync(url, params);

        hideHourglassCursor();
    } catch (error) {
        hideHourglassCursor();
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }
}

//Gets the Shift#s based on the selected shiftdate
async function GetShiftNos(shiftdate) {
    const url = URL_GET_REP_TILL_CLOSE + "GetShiftNos?ShiftDate=" + shiftdate;

    try {
        const valuesArray = await ajaxGetAsync(url);

        let selectElement = document.getElementById('shiftCombo');

        if (valuesArray.length == 0) {
            emptySelectElement(selectElement);
            const option = document.createElement('option');
            option.value = '0';
            option.textContent = 'All';
            selectElement.appendChild(option);
        }
        else {
            emptySelectElement(selectElement);

            const option = document.createElement('option');
            option.value = '0';
            option.textContent = 'All';
            selectElement.appendChild(option);

            valuesArray.forEach(value => {
                const option = document.createElement('option');
                option.value = value;
                option.textContent = value;
                selectElement.appendChild(option);
            });
        }

        //// Add options to the select element
        //for (let i = 0; i < shiftOptions.length; i++) {
        //    const option = document.createElement('option');
        //    option.value = shiftOptions[i];
        //    option.textContent = shiftOptions[i];
        //    shiftSelection.append(option);
        //};


    } catch (error) {
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }

}
function emptySelectElement(selectElement) {
    while (selectElement.firstChild) {
        selectElement.removeChild(selectElement.firstChild);
    }
}
async function runRepByShift(shiftDate, shiftNo, startDate, endDate) {
    //const url = URL_GET_REP_TILL_CLOSE + "GetRepByShift?shiftdate=" + shiftdate + "&shift=" + shift;
    const url = URL_GET_REP_TILL_CLOSE + "GetRepByShift";

    try {

        showHourglassCursor();

        //const resultString = await ajaxGetAsync(url);
        const params = { policies, shiftDate, shiftNo, startDate, endDate }

        showHourglassCursor();

        const resultString = await ajaxPostAsync(url, params);

        var textarea = document.getElementById("TillCloseTextArea");
        textarea.value = "";

        // Set the fixed-size font for the textarea
        textarea.style.fontFamily = "Courier New, monospace";

        // Loop through the array and append each element to the textarea with a newline
        for (var i = 0; i < resultString.length; i++) {
            textarea.value += resultString[i] + '\n';
        }

        document.getElementById('btn-export').disabled = false;
        document.getElementById('btn-print').disabled = false;
        hideHourglassCursor();
    } catch (error) {
        hideHourglassCursor();
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }
}

async function runRepByDates(startDate, startTime, endDate, endTime) {
    //const url = URL_GET_REP_TILL_CLOSE + "GetRepByDates?date1=" + date1 + "&date2=" + date2;
    const url = URL_GET_REP_TILL_CLOSE + "GetRepByDates";

    try {

        showHourglassCursor();

        //const resultString = await ajaxGetAsync(url);
        const params = { policies, startDate, startTime, endDate, endTime }

        showHourglassCursor();

        const resultString = await ajaxPostAsync(url, params);

        var textarea = document.getElementById("TillCloseTextArea");
        textarea.value = "";

        // Set the fixed-size font for the textarea
        textarea.style.fontFamily = "Courier New, monospace";

        // Loop through the array and append each element to the textarea with a newline
        for (var i = 0; i < resultString.length; i++) {
            textarea.value += resultString[i] + '\n';
        }

        document.getElementById('btn-export').disabled = false;
        document.getElementById('btn-print').disabled = false;
        hideHourglassCursor();

    } catch (error) {
        hideHourglassCursor();
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }

}
//function updateGroupTill(shiftdate) {
//    var xhr = new XMLHttpRequest();
//    xhr.open("GET", URL_GET_REP_TILL_CLOSE + "GetGroupTillData?ShiftDate=" + shiftdate, true);
//    xhr.onreadystatechange = function () {
//        if (xhr.readyState === 4 && xhr.status === 200) {
//            document.getElementById("groupTill").innerHTML = xhr.responseText;
//        }
//    };
//    xhr.send();
//}

function formatDateToMMDDYYYY(date) {
    let month = (date.getMonth() + 1).toString().padStart(2, '0'); // Months are 0-based, so add 1
    let day = date.getDate().toString().padStart(2, '0');
    let year = date.getFullYear();
    return `${month}/${day}/${year}`;
}

function renderShiftOption() {
    let shiftContainer = $('#shiftCombo');
    shiftContainer.empty();
    try {
        shiftContainer.forEach(value => {
            const option = document.createElement('option');
            option.textContent = value;
            option.value = value.toLowerCase().replace(/\s+/g, '-'); // Optional: Set value attribute
            comboBox.appendChild(option);
        });
    } catch (error) {
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }
}

function formatDateToMMDDYYYY(date) {
    let month = (date.getMonth() + 1).toString().padStart(2, '0'); // Months are 0-based, so add 1
    let day = date.getDate().toString().padStart(2, '0');
    let year = date.getFullYear();
    return `${month}/${day}/${year}`;
}

function showHourglassCursor() {
    $('body').addClass('hourglass-cursor');
}

// Hide the hourglass cursor
function hideHourglassCursor() {
    $('body').removeClass('hourglass-cursor');
}

