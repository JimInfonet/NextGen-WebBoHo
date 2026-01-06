const URL_GET_REP_TILL_CLOSE = "/RepTillClose/";
const STRING_TABLE = REP_TILL_CLOSE_STRING_TABLE;
let policies = {};
let repTillClosesOptions = [];
let shiftOptions = [];
let errorMessage = "";

$(document).ready(async function () {
    await loadPolicies();

    //await loadRepTillCloses().then(() => renderRepTillClosesOption());
    await loadRepTillCloses().then(() => renderRepTillClosesTable());

    errorMessage = document.getElementById("errorMessage").value;


    //isBackOffice = (document.getElementById("isBackOffice").innerText === "True");
    //if (!isBackOffice)
    //    await loadStores();

    // Initialize datepicker for Start Date
    //$('#start-date').datepicker({
    //    format: 'yyyy-mm-dd hh:ii:ss',
    //    autoclose: true,
    //    todayHighlight: true,
    //    setDate: new Date() // You can set a minimum date if needed
    //});

    //// Initialize datepicker for End Date
    //$('#end-date').datepicker({
    //    format: 'yyyy-mm-dd hh:ii:ss',
    //    autoclose: true,
    //    todayHighlight: true,
    //    setDate: new Date() // You can set a minimum date if needed
    //});

    // Initailize shift numbers of the shift date
    await loadShifts();

});

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
const datePicker = document.getElementById('shiftDate');
if (!isNullOrUndefined(datePicker))
    datePicker.addEventListener('change', handleDateChange);

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

async function loadPolicies() {
    const url = STRING_TABLE.URL.GET_POLICIES;

    await ajaxGetAsync(url)
        .then(_policies => policies = _policies)
        .catch(toastrError);
}
// Function to handle date change event
function handleDateChange(event) {
    const selectedDate = event.target.value;
    console.log('Selected date:', selectedDate);

    var sDate = document.getElementById("shift-date").value;
    if (!isNullOrUndefined(sDate))
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

            for (let i = 0; i < result.length; i++) {
                let option = optionTemplate.clone();
                option.append(result[i].shift + " - " + result[i].shift);
                option.attr('value', result[i].shift.split(" ")[0]);
                shiftSelection.append(option);
            }
        },
        error: function () {
            toastr.error(errorMessage);
        }
    })
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
async function loadRepTillCloses() {
    var days = policies.tillCDays;
    let date1 = new Date();
    let date2 = new Date();
    date1.setDate(date1.getDate() - days);

    //const url = URL_GET_REP_TILL_CLOSE + "GetRepTillCloses";
    const url = URL_GET_REP_TILL_CLOSE + "GetRepTillCloses?date1=" + formatDateToMMDDYYYY(date1) + "&date2=" + formatDateToMMDDYYYY(date2);

    try {
        const repTillCloses = await ajaxGetAsync(url);
        repTillClosesOptions = repTillCloses;

    } catch (error) {
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }

    //display first row as a default
    //SelectRow(0);
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

function renderRepTillClosesOption() {
    let repTillCloseContainer = $('#repTillClose-container-id');
    repTillCloseContainer.empty();
    try {
        repTillClosesOptions.forEach(repTillClose => {
            let openDate = new Date(repTillClose.open_Date);
            let openTime = new Date(repTillClose.open_Time);
            let closeDate = new Date(repTillClose.close_Date);
            let closeTime = new Date(repTillClose.close_Time);

            let option = $('<option>').attr('value', repTillClose.close_Num).text(`#${repTillClose.close_Num}: Open: ${openDate.toLocaleDateString()} ${openTime.toLocaleTimeString()}, Close: ${closeDate.toLocaleDateString()} ${closeTime.toLocaleTimeString()}, Till: #${repTillClose.till_Num}, Shift: #${repTillClose.shiftNo}`);
        });
    } catch (error) {
        errorMessage = $('#error-message-for-list-load').val();
        toastr.Error(errorMessage);
    }
}

function renderRepTillClosesTable() {
    let repTillCloseContainer = $('#repTillClose-container-id');
    repTillCloseContainer.empty();

    if (repTillClosesOptions != null) {

        try {
            repTillClosesOptions.forEach(repTillClose => {
                let openDate = new Date(repTillClose.open_Date);
                let openTime = new Date(repTillClose.open_Time);
                let closeDate = new Date(repTillClose.close_Date);
                let closeTime = new Date(repTillClose.close_Time);
                let shiftDate = new Date(repTillClose.shiftDate);

                let closedTill = `<div class='u_row' tabindex="0" contenteditable="true" onClick="selectRow(this)">
                                <div class='u_cell' id="closeNum">${repTillClose.close_Num}</div>
                                <div class='u_cell'>${openDate.toLocaleDateString()}</div>
                                <div class='u_cell'>${openTime.toLocaleTimeString()}</div>
                                <div class='u_cell'>${closeDate.toLocaleDateString()}</div>
                                <div class='u_cell'>${closeTime.toLocaleTimeString()}</div>
                                <div class='u_cell' id="tillNum">${repTillClose.till_Num}</div>
                                <div class='u_cell'>${shiftDate.toLocaleDateString()}</div>
                                <div class='u_cell' id="shiftNum">${repTillClose.shiftNumber}</div>
                              </div>`;

                repTillCloseContainer.append(closedTill);
            });

        } catch (error) {
            errorMessage = $('#error-message-for-list-load').val();
            toastr.Error(errorMessage);
        }
    }

    //    GetShiftNos(document.getElementById("shift-date").value);
}

function selectRow(row) {
    // You can perform actions when a row is selected
    //row.style.outline = "2px solid skyblue"; // Example: Highlight the selected row
    // Find cell IDs within the selected row
    var closeNo, tillNo, shiftNo;
    var cells = row.getElementsByClassName('u_cell');
    for (var i = 0; i < cells.length; i++) {
        if (cells[i].id == "closeNum") {
            closeNo = cells[i].outerText;
        }
        else if (cells[i].id == "tillNum") {
            tillNo = cells[i].outerText;
        }
        else if (cells[i].id == "shiftNum") {
            //if (!isNaN(Number(cell.outerText)))
            //    shiftNo = parseInt(cells[i].outerText,10);
            shiftNo = cells[i].outerText;
        }
    }

    fetchData(closeNo, tillNo, shiftNo);
};

async function fetchData(closeNo, tillNo, shiftNo) {
    let errorMessage = "";
    let report = "";
    const url = URL_GET_REP_TILL_CLOSE + "GetRepByTillNo";

    try {
        const params = { closeNo, tillNo, shiftNo }

        showHourglassCursor(); // Show loading

        let resultString = await ajaxPostAsync(url, params);

        //if (!response.ok) {
        //    throw new Error(`HTTP error! status: ${response.status}`);
        //}

        const textarea = document.getElementById("TillCloseTextArea");

        if (textarea) {
            textarea.value = "";
            textarea.style.fontFamily = "Courier New, monospace";
        }

        //if (Array.isArray(resultString)) {
        //    textarea.value = resultString.join('\n');
        //} else {
        //    textarea.value = String(resultString);
        //}

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
        hideHourglassCursor(); // Always hide loading
    }
}

// Show the hourglass cursor
function showHourglassCursor() {
    $('body').addClass('hourglass-cursor');
}

// Hide the hourglass cursor
function hideHourglassCursor() {
    $('body').removeClass('hourglass-cursor');
}
