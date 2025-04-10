let customerGroupDataTable;
let customerGroups;
let selectedCustomerGroupID;
const custMem = document.getElementById("CUST_MEM").value !== "Customer";

$(async function () {
  await loadCustomerGroups();
});

let createCustomerGroupDataTable = () => {
  destroyDataTableIfExists(customerGroupDataTable)

  let props = createDataTableProp();
  props.data = customerGroups;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5] },
    {
      "targets": 5, // Index of the "Loyalty Footer" column
      "render": function (data, type, row, meta) {
        const maxCharactersPerLine = 20;
        return (data.length > maxCharactersPerLine) ?
          addSeeMoreBtnInGridData(data, type, row, meta, maxCharactersPerLine) : data;
      }
    }
  ];

  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "groupID", },
    { "data": "groupName", },
    { "data": "discountType", },
    { "data": "discountRate", },
    { "data": "discountName", },
    { "data": "loyaltyFooter", }
  ];

  customerGroupDataTable = createDataTableGeneric('customer-group', props);

  $('#customer-group tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if (customerGroups.length === 0) {
        return;
      }

      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        selectedCustomerGroupID = null;
      } else {
        customerGroupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedCustomerGroupID = $(this).find("td:first")[0].innerText;
      }
    })
    .on('dblclick', 'tr', async function () {
      if (customerGroups.length === 0) {
        return;
      }

      selectedCustomerGroupID = $(this).find("td:first")[0].innerText;
      const url = `/CustomerGroup/CustomerGroupForm?groupID=${selectedCustomerGroupID}&actionMode=view`;
      await safelyNavigateTo(url);
    });
}

let loadCustomerGroups = async () => {
  let url = "CustomerGroup/GetAllCustomerGroups";
  let errorMessage = $("#anErrorHasOccurredWhileLoadingCustomerGroups").val();

  await ajaxGetAsync(url)
    .then((_customerGroups) => {
      customerGroups = _customerGroups;
      createCustomerGroupDataTable();
    })
    .catch(() => toastr.error(errorMessage));
}

$("#btn-add").on("click", async function () {
  const url = `/CustomerGroup/CustomerGroupForm?groupID=`;
  await safelyNavigateTo(url);
});

$("#btn-edit").on("click", async function () {
  if (!selectedCustomerGroupID) {
    custMem ? toastr.warning($("#selectAMemberGroup").val()) : toastr.warning($("#selectACustomerGroup").val());
    return;
  }
  const url = `/CustomerGroup/CustomerGroupForm?groupID=${selectedCustomerGroupID}`;
  await safelyNavigateTo(url);
});

$("#btn-delete").on("click", function () {
  if (!selectedCustomerGroupID) {
    custMem ? toastr.warning($("#selectAMemberGroup").val()) : toastr.warning($("#selectACustomerGroup").val());
    return;
  }
  let title = document.getElementById("delete").value;
  let message = `${document.getElementById("confirmDeleteCustomerGroup").value} ${selectedCustomerGroupID} ?`;
  if (custMem)
    message = `${document.getElementById("confirmDeleteMemberGroup").value} ${selectedCustomerGroupID} ?`;

  showConfirmModal(title, message).then(async (result) => {
    if (result) {
      let url = "CustomerGroup/DeleteCustomerGroup";
      await ajaxPostAsync(url, selectedCustomerGroupID)
        .then(async () => {
          custMem ? toastr.success($("#memberGroupDeletedSuccessfully").val()) : toastr.success($("#customerGroupDeletedSuccessfully").val());
          await loadCustomerGroups();
        })
        .catch(() => {
          toastr.error(custMem ? $("#failedToDeleteMemberGroup").val() : $("#failedToDeleteCustomerGroup").val());
        });
    }
  });
});

$("#btn-copy").on("click", async function () {
  if (!selectedCustomerGroupID) {
    custMem ? toastr.warning($("#selectAMemberGroup").val()) : toastr.warning($("#selectACustomerGroup").val());
    return;
  }
  const url = `/CustomerGroup/CustomerGroupForm?groupID=${selectedCustomerGroupID}&isCopyCustomerGroup=${true}`;
  await safelyNavigateTo(url);
});

// Handle click event for "See more" link of the "Loyalty Footer" column
$('#customer-group').on('click', '.see-more', function () {
  let $cell = $(this).closest('td');
  let fullContent = $(this).data('full-content');
  // Replace "See more" link with "See less" link
  $cell.html(fullContent + ` <span class="see-less" data-full-content="${fullContent}">
                                <a href="#">${$("#seeLess").val()}</a>
                             </span>`);
  customerGroupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
});

// Handle click event for "See less" link of the "Loyalty Footer" column
$('#customer-group').on('click', '.see-less', function () {
  let $cell = $(this).closest('td');
  let fullContent = $(this).data('full-content');
  let truncatedData = fullContent.substring(0, 20);
  // Replace "See less" link with "See more" link
  $cell.html(truncatedData + ` <span class="see-more" data-full-content="${fullContent}">
                                  <a href="#">${$("#seeMore").val()}</a>
                               </span>`);
  customerGroupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
});