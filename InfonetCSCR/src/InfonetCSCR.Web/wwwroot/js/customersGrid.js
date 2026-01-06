let customersDatatable;
let customers;
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
    props.scrollY = '440px';
    props.columnDefs = [
        { className: 'text-center', targets: [0, 1, 2] }
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

            if ($(this).hasClass('selected-grid-row')) {
                $(this).removeClass('selected-grid-row');
                selectedCode = null;
                $("#btn-customer-card").prop("disabled", true);
            } else {
                customersDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
                $(this).addClass('selected-grid-row');
                selectedCode = $(this).find("td:first")[0].innerText;
                $("#btn-customer-card").prop("disabled", false);
            }
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
    let url = "CustomersGrid/GetAllCustomers";
    let errorMessage = custMem ? $("#anErrorHasOccurredWhileLoadingMembers").val()
        : $("#anErrorHasOccurredWhileLoadingCustomers").val();

    await ajaxGetAsync(url)
        .then(_customers => {
            customers = _customers;
            createCustomersDatatable();
        })
        .catch(() => toastr.error(errorMessage));
}

// Triggers when delete button is clicked
$("#btn-delete").on("click", async function () {
    if (!selectedCode) {
        toastr.warning($("#selectACustomer").val());
        return;
    }
    let title = document.getElementById("delete").value;
    let message = document.getElementById("confirmDeleteCustomer").value.replace("{0}", selectedCode);
    if (custMem)
        message = document.getElementById("confirmDeleteMember").value;

    showConfirmModal(title, message).then((result) => {
        if (result === true) {
            let url = "CustomersGrid/DeleteCustomer";
            ajaxPostAsync(url, selectedCode)
                .then(async () => {
                    $("#btn-customer-card").prop("disabled", true);
                    await loadCustomers();
                    toastr.success(custMem ? $("#memberDeletedSuccessfully").val() : $("#customerDeletedSuccessfully").val());
                })
                .catch(() => {
                    toastr.error(custMem ? $("#failedToDeleteMember").val() : $("#failedToDeleteCustomer").val());
                });
        }
    });
});

// Triggers when add button is clicked
$("#btn-add").on("click", function () {
    const url = `/Customer?Code=`;
    safelyNavigateTo(url);
});

// Triggers when add button is clicked
$("#btn-edit").on("click", function () {
    if (!selectedCode) {
        toastr.warning($("#selectACustomer").val());
        return;
    }
    const url = `/Customer?Code=${selectedCode}`;
    safelyNavigateTo(url);
});

function goToCustomerCard() {
    const url = `/CustomerCard?Code=${selectedCode}`;
    safelyNavigateTo(url);
}
