const URL_GET_REP_CLOSE_BATCH = "/RepCloseBatch/";
let policies;
let closeBatchesDatatable;
let closeBatches = [];
let selectedCode;
//let CloseBatchDays = document.getElementById("CloseBDays");

let rowDoubleClick = CONSTANT_TEXTS.ROW_DOUBLE_CLICK;

$(async function () {
    await loadCloseBatches();
});

let createCloseBatchesDatatable = async () => {
    if (closeBatchesDatatable) {
        closeBatchesDatatable.state.clear();
        closeBatchesDatatable.clear();
        closeBatchesDatatable.destroy();
    }

    let props = createDataTableProp();
    props.data = closeBatches;
    props.scrollY = '440px';
    props.columnDefs = [
        //{ className: 'text-center', targets: [0, 1, 2, 3, 4, 5] }
        { className: 'text-left', targets: [0, 1, 2, 3, 4] }
    ];

    props.columns = [
        //the first letter of the column name should be lowercase and first letter of the next words is uppercase
        { "data": "id", },
        { "data": "batchNumber", },
        { "data": "batchDate", },
        { "data": "batchTime", },
        { "data": "terminalID", },
        //    { "data": "report", },
    ];

    closeBatchesDatatable = createDatatableWithSearching('closeBatches-grid', props);

    $('#closeBatches-grid tbody')
        .off() //turn off the event before turning it on to prevent firing click event multiple times
        .on('click', 'tr', function () {
            const tableinfo = closeBatchesDatatable.page.info();

            if (tableinfo.recordsDisplay === 0 || closeBatches.length === 0) {
                return;
            }

            if ($(this).hasClass('selected-grid-row')) {
                $(this).removeClass('selected-grid-row');
                selectedCode = null;
                //$("#btn-customer-card").prop("disabled", true);
            } else {
                closeBatchesDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
                $(this).addClass('selected-grid-row');
                selectedCode = $(this).find("td:first")[0].innerText;
                //$("#btn-customer-card").prop("disabled", false);
            }
            fetchData(selectedCode)

            // Enable buttons
            document.getElementById('btn-export').disabled = false;
            document.getElementById('btn-print').disabled = false;
        })
        .on('dblclick', 'tr', function () {
            const tableinfo = closeBatchesDatatable.page.info();

            if (tableinfo.recordsDisplay === 0 || closeBatches.length === 0) {
                return;
            }

            selectedCode = $(this).find("td:first")[0].innerText;
            const url = `/RepCloseBatch?Code=${selectedCode}&ActionMode=${rowDoubleClick}`;
            safelyNavigateTo(url);
        });
}


async function fetchData(id) {
    let errorMessage = "";
    let report = "";
    const url = URL_GET_REP_CLOSE_BATCH + "GetReportById?id=" + id;

    try {
        showHourglassCursor(); // Show loading

        let resultString = await ajaxGetAsync(url);

        //if (!response.ok) {
        //    throw new Error(`HTTP error! status: ${response.status}`);
        //}
        const textarea = document.getElementById("CloseBatchTextArea");

        if (textarea) {
            textarea.value = "";
        }
        textarea.style.fontFamily = "Courier New, monospace";

        // Loop through the array and append each element to the textarea with a newline
        //for (let i = 0; i < resultString.length; i++) {
        //    textarea.value += resultString[i] + '\n';
        //}
        if (Array.isArray(resultString)) {
            textarea.value = resultString.join('\n');
        } else {
            textarea.value = String(resultString);
        }
    } catch (err) {
        /*toastr.error(err.message || "Unknown fetch error");*/
        const textarea = document.getElementById("CloseBatchTextArea");

        textarea.value = err.responseText;
    } finally {
        hideHourglassCursor(); // Always hide loading
    }
}

async function exportButtonClicked() {
    const url = URL_GET_REP_CLOSE_BATCH + "GetExportToExcel";
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
async function printStatementClicked() {
    const textarea = document.getElementById("CloseBatchTextArea");
    // Open a new print window
    const printWindow = window.open("", "_blank", "width=600,height=400");
    printWindow.document.write(`
        <html>
            <head>
                <title>Print</title>
                <style>
                    body { font-family: Arial, sans-serif; padding: 20px; }
                    pre { white-space: pre-wrap; word-wrap: break-word; }
                </style>
            </head>
            <body>
                <pre>${textarea.value}</pre>
                <script>
                    window.onload = function() {
                        window.print();
                        window.onafterprint = function() { window.close(); }
                    }
                <\/script>
            </body>
        </html>
    `);
    printWindow.document.close();
}
let loadCloseBatches = async () => {
    var days = document.getElementById("CloseBDays").value;
    let date1 = new Date();
    let date2 = new Date();
    date1.setDate(date2.getDate() - days);

    let url = URL_GET_REP_CLOSE_BATCH + "GetRepCloseBatches?date1=" + formatDateToMMDDYYYY(date1) + "&date2=" + formatDateToMMDDYYYY(date2);
    //let errorMessage = CloseBatchDays ? $("#anErrorHasOccurredWhileLoadingMembers").val()
    //    : $("#anErrorHasOccurredWhileLoadingCustomers").val();

    await ajaxGetAsync(url)
        .then(_closeBatches => {
            closeBatches = _closeBatches;
            createCloseBatchesDatatable();
        })
        .catch(() => toastr.error(errorMessage));
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