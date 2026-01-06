const URL_GET_REP_AR_STATEMENT = "/RepArStat/";
const URL_GET_REP_TILL_CLOSE = "/RepTillClose/";

const STRING_TABLE = REP_ARSTATEMENT_STRING_TABLE;
let customersDatatable;
let customers;
let arStatmenets = [];
//var arStatements;
//{
//    new ArStatementDto
//    {
//    Header = new CompanyDto { /* set props */ },
//    Details = new List < ArSalesDto >
//    {
//        new ArSalesDto { /* set props */ },
//    new ArSalesDto { /* set props */ }
//}
//    },
//new ArStatementDto
//{
//    Header = new CompanyDto { /* another company */ },
//    Details = new List < ArSalesDto >
//    {
//        new ArSalesDto { /* more props */ }
//}
//    }
//};

let selectedCode;
let custMem = document.getElementById("CUST_MEM").value !== "Customer";

let rowDoubleClick = CONSTANT_TEXTS.ROW_DOUBLE_CLICK;

$(async function () {
    await loadCustomers();
});

let createCustomersDatatable = async () => {
    if (customersDatatable) {
        customersDatatable.state.clear();
        customersDatatable.clear();
        customersDatatable.destroy();
    }

    let props = createDataTableProp();
    props.data = customers;
    props.scrollY = '440px';   // fixed pixel height
    //props.scrollCollapse = false; // optional
    props.columnDefs = [
        { className: 'text-left', targets: [0, 1, 2] }
    ];

    props.columns = [
        //the first letter of the column name should be lowercase and first letter of the next words is uppercase
        { "data": "code", },
        { "data": "name", },
        { "data": "phone", },
    ];

    customersDatatable = createDatatableWithSearching('customers-grid', props);

    $('#customers-grid tbody')
        .off() //turn off the event before turning it on to prevent firing click event multiple times
        .on('click', 'tr', function () {
            const tableinfo = customersDatatable.page.info();

            if (tableinfo.recordsDisplay === 0 || customers.length === 0) {
                return;
            }

            selectedCode = null;
            if ($(this).hasClass('selected-grid-row')) {
                $(this).removeClass('selected-grid-row');
                $("#btn-customer-card").prop("disabled", true);
            } else {
                customersDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
                $(this).addClass('selected-grid-row');
                $("#btn-customer-card").prop("disabled", false);
            }
            selectedCode = $(this).find("td:first")[0].innerText;

            // Enable buttons
            document.getElementById('btn-export').disabled = false;
            document.getElementById('btn-display').disabled = false;
            document.getElementById('btn-print').disabled = false;
            //    fetchData(selectedCode)
        })
        .on('dblclick', 'tr', function () {
            const tableinfo = customersDatatable.page.info();

            if (tableinfo.recordsDisplay === 0 || customers.length === 0) {
                return;
            }

            selectedCode = $(this).find("td:first")[0].innerText;
            const url = `/Customer?Code=${selectedCode}&ActionMode=${rowDoubleClick}`;
            safelyNavigateTo(url);
        });
}

let loadCustomers = async () => {
    let url = URL_GET_REP_AR_STATEMENT + "GetAllCustomers";
    let errorMessage = custMem ? $("#anErrorHasOccurredWhileLoadingMembers").val()
        : $("#anErrorHasOccurredWhileLoadingCustomers").val();

    await ajaxGetAsync(url)
        .then(_customers => {
            customers = _customers;
            createCustomersDatatable();
        })
        .catch(() => toastr.error(errorMessage));
}

async function fetchData(selectedCode) {
    const padC = (val, len) => (val ?? "").toString().padC(len);
    const padL = (val, len) => (val ?? "").toString().padStart(len);
    const padR = (val, len) => (val ?? "").toString().padEnd(len);
    let dMonth = parseInt(document.getElementById("monthSelect").value, 10);
    let dYear = parseInt(document.getElementById("yearInput").value, 10);
    // JavaScript months are 0-based (0 = January, 11 = December)
    //let dDate = new Date(dYear, dMonth - 1, 1);
    let dLastDate = new Date(dYear, dMonth, 0);
    //let dDate =new Date();
    //let dMonth = dDate.toLocaleString("default", { month: "long", year: "numeric" });
    let sMonth = new Intl.DateTimeFormat(undefined, { month: "long" }).format(dLastDate);

    try {
        showHourglassCursor(); // Show loading

        //let url = URL_GET_REP_AR_STATEMENT + "GetRepArStat?client=" + selectedCode + "&dDate=" + dDate.toISOString();
        let url = URL_GET_REP_AR_STATEMENT + "GetRepArStat?client=" + selectedCode + "&dDate=" + formatDateToMMDDYYYY(dLastDate);

        let response = await ajaxGetAsync(url);
        const { header, details } = response;

        const textarea = document.getElementById("ArStatementTextArea");
        textarea.style.fontFamily = "Courier New, monospace";
        textarea.value = ""; // clear previous content

        // --- HEADER ---
        textarea.value += `\n\t\tCUSTOMER STATEMENT\n\n`;
        if (header) {
            textarea.value += `\t\t${header.comp || ""}\n`;
            textarea.value += `\t\t${header.addr1 || ""}\n`;
            textarea.value += `\t\t${header.addr2 || ""}\n\n`;

            //const row = padR(`Account#: ${selectedCode}n\`, 40);
            //textarea.value += row + `\n`;
            textarea.value += `Account#: ${selectedCode}\n`;
            textarea.value += `\n\n`;
            textarea.value += `Statement for the month of ${sMonth} in ${dYear}\n`;
            textarea.value += `Printed on ${new Date().toLocaleDateString()}\n\n`;
        }

        // --- TABLE HEADER ---
        textarea.value += `T_Type\tSale_No\tSale_Date\tAmount\tSERNUM\tCCARD_NUM\n`;
        `${padR("T_Type", 8)}` +
            `${padR("Sale_No", 10)}` +
            `${padR("Sale_Date", 12)}` +
            `${padR("Amount", 10)}` +
            `${padR("SERNUM", 12)}` +
            `${padR("CCARD_NUM", 12)}`;
        textarea.value += `------\t-------\t----------\t------\t------\t-------------\n`;

        // --- DETAILS ---
        if (details && details.length > 0) {
            details.forEach(item => {
                const amount = item.amount != null ? Number(item.amount).toFixed(2) : "0.00";
                const saleDate = item.sale_Date ? new Date(item.sale_Date).toLocaleDateString() : "";

                //const row = `${item.t_Type || ""}\t${item.sale_No || ""}\t${item.sale_Date ? new Date(item.sale_Date).toLocaleDateString() : ""}\t${item.amount?.toFixed(2) || "0.00"}\t${item.sernum || ""}\t${item.ccarD_NUM || ""}`;
                const row =
                    `${padR(item.t_Type, 6)}` +
                    `${padL(item.sale_No, 9)}` +
                    `${padL(saleDate, 11)}` +
                    `${padL(amount, 12)}` +
                    `${padR(item.sernum, 8)}` +
                    `${padL(item.ccarD_NUM, 13)}`;
                textarea.value += row + "\n";
            });
        } else {
            textarea.value += "No records found.\n";
        }

        //// Enable buttons
        //document.getElementById('btn-export').disabled = false;
        //document.getElementById('btn-print').disabled = false;

    } catch (error) {
        const textarea = document.getElementById("ArStatementTextArea");
        textarea.value = error.responseText || error.message || "An unexpected error occurred.";
        toastr.error($('#error-message-for-list-load').val());
    } finally {
        hideHourglassCursor();
    }
}

async function monthClosingClicked() {
    let dMonth = parseInt(document.getElementById("monthSelect").value, 10);
    let dYear = parseInt(document.getElementById("yearInput").value, 10);
    let dLastDate = new Date(dYear, dMonth, 0);

    let url = URL_GET_REP_AR_STATEMENT + "GenMonthlyClosing?client=" + selectedCode + "&dDate=" + formatDateToMMDDYYYY(dLastDate)

    //let dDate =new Date();
    //let dMonth = dDate.toLocaleString("default", { month: "long", year: "numeric" });
    let sMonth = new Intl.DateTimeFormat(undefined, { month: "long" }).format(dLastDate);

    showProgress();

    let response = await ajaxGetAsync(url);

    // simulate progress
    let percent = 0;
    const interval = setInterval(() => {
        percent += 10;
        const bar = document.getElementById("jobBar");
        bar.style.width = percent + "%";
        bar.innerText = percent + "%";

        if (percent >= 100) {
            clearInterval(interval);
            // Optionally hide the progress bar when done
            // hideProgress();
        }
    }, 500);

}
function showProgress() {
    document.getElementById("progressContainer").style.display = "block";
}

// Hide the progress bar
function hideProgress() {
    document.getElementById("progressContainer").style.display = "none";
}


async function displayStatementClicked() {
    hideProgress();
    fetchData(selectedCode)
}

async function printStatementClicked() {
    const textarea = document.getElementById("ArStatementTextArea");
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

function resizeTextareaToContent(textarea) {
    if (!textarea) return;

    // Split content into lines
    const lines = textarea.value.split("\n");

    // Create a temporary element to measure text width
    const span = document.createElement("span");
    span.style.visibility = "hidden";
    span.style.whiteSpace = "pre"; // preserves spaces and tabs
    span.style.fontFamily = getComputedStyle(textarea).fontFamily;
    document.body.appendChild(span);

    let maxWidth = 0;

    lines.forEach(line => {
        span.textContent = line;
        const width = span.offsetWidth;
        if (width > maxWidth) maxWidth = width;
    });

    document.body.removeChild(span);

    // Add a little padding
    textarea.style.width = (maxWidth + 20) + "px"; // 20px for extra spacing
}

// Show the hourglass cursor
function showHourglassCursor() {
    $('body').addClass('hourglass-cursor');
}

// Hide the hourglass cursor
function hideHourglassCursor() {
    $('body').removeClass('hourglass-cursor');
}

function formatDateToMMDDYYYY(dDate) {
    let month = (dDate.getMonth() + 1).toString().padStart(2, '0'); // Months are 0-based, so add 1
    let day = dDate.getDate().toString().padStart(2, '0');
    let year = dDate.getFullYear();
    return `${month}/${day}/${year}`;
}
function formatDateToYYYYMMDD(dDate) {
    let month = (dDate.getMonth() + 1).toString().padStart(2, '0'); // Months are 0-based, so add 1
    let day = dDate.getDate().toString().padStart(2, '0');
    let year = dDate.getFullYear();
    return `${year}/${month}/${day}`;
}