let userGridDataTable;
let userDetails;
let _globalSelectedUserCode;
const timeSheetsAndHaveUserIdPolicy = document.getElementById("timeSheetsAndHaveUserIdPolicy").value === "True";
const groupCreate = "createGroup"

$(async function () {
  setGlobalEditingState(false);

  const urlParams = new URLSearchParams(window.location.search);
  const action = urlParams.get('actionMode');
  const isShowToaster = urlParams.get('showToaster');

  await loadUserDetails();
  await loadGroupInfoDetails();
  await loadOptionsForAddGroup();
  await loadUserGroupData();
  await loadAndCreateGroupMembersGridDataTable();

  if (isShowToaster) {
    if (action === groupCreate) {
      toastr.success($("#groupCreatedSuccessfully").val());
    }

    const newUrl = removeURLParameter(window.location.search, 'showToaster');
    window.history.replaceState(null, null, newUrl);
  }

  if (!changePassPolicy) {
    $('#btn-change-password').prop('disabled', true);
  }
});

let createUserGridDataTable = () => {
  destroyDataTableIfExists(userGridDataTable);

  let props = createDataTableProp();
  props.data = userDetails;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  props.columns = [
    { "data": "code", },
    { "data": "name", }
  ];

  if (timeSheetsAndHaveUserIdPolicy) { // Add Id and Type column if the TimeSheets and HaveUserId policy is true
    props.columnDefs[0].targets = props.columnDefs[0].targets.concat([2, 3]);

    props.columns = props.columns.concat([
      { "data": "id", },
      { "data": "type", }
    ]);
  }

  userGridDataTable = createDataTableGeneric('data-table-user-grid', props);

  $('#data-table-user-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (userDetails.length === 0) {
        return;
      }

      // Both Add to Group and Remove from Group will be hidden when clicked to a user
      $("#add-to-group").hide();
      $("#btn-remove-from-group").hide();
      $('#add-to-group').find('option[value=""]').prop('selected', true);

      // Remove the selected row of UserGroup data table
      userGroupGridDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
      _globalSelectedUserGroupCode = null;
      await loadAndCreateGroupMembersGridDataTable();

      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        _globalSelectedUserCode = null;
        _globalGroupInfo = null;
        createGroupInfoGridDataTable();
      } else {
        userGridDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        _globalSelectedUserCode = $(this).find("td:first")[0].innerText;
        await loadGroupInfoDetails();
        const groupInfoGridFirstRow = groupInfoGridDataTable.row(0).data();
        if (isNullOrUndefined(groupInfoGridFirstRow)) {
          $("#add-to-group").show();
          _globalSelectedUserGroupCode = null;
        } else {
          $("#btn-remove-from-group").show();
          // Select the appropriate row of UserGroup data table of which the selected User belongs to
          await selectRelevantRowOfWhichTheSelectedUserBelongs(groupInfoGridFirstRow.group);
        }
      }
    })
    .on('dblclick', 'tr', async function () {
      if (userDetails.length === 0) {
        return;
      }

      _globalSelectedUserCode = $(this).find("td:first")[0].innerText;
      await loadGroupInfoDetails();
      const groupInfoGridFirstRow = groupInfoGridDataTable.row(0).data();

      if (!isNullOrUndefined(groupInfoGridFirstRow)) {
        _globalSelectedUserGroupCode = groupInfoGridFirstRow.group;
        if (checkIfSelectedUserGroupHasHigherPriorityThanLoggedInUser()) {
          if (!$(this).hasClass('selected-grid-row')) {
            _globalGroupInfo = null;
            _globalSelectedUserGroupCode = null;
            _globalSelectedUserCode = null;
            createGroupInfoGridDataTable();
          }
          showUnAuthorizedOperationModal();
          return;
        }
      }

      const url = `/UserDetailsForm/Index?code=${_globalSelectedUserCode}`;
      await safelyNavigateTo(url);
    });
}

let loadUserDetails = async () => {
  const url = "UserGrid/GetAllUserDetails";
  const errorMessage = $("#anErrorHasOccurredWhileLoadingUserGrid").val();

  await ajaxGetAsync(url)
    .then((_userDetails) => {
      userDetails = _userDetails;
      createUserGridDataTable();
    })
    .catch(() => toastr.error(errorMessage));
}

$("#btn-user-grid-add").on("click", async function () {
  const url = `/UserDetailsForm/Index?code=`;
  await safelyNavigateTo(url);
});

$("#btn-user-grid-edit").on("click", async function () {
  if (!_globalSelectedUserCode) {
    toastr.warning($("#selectAUser").val());
    return;
  }
  if (checkIfSelectedUserHasHigherPriorityThanLoggedInUser()) {
    showUnAuthorizedOperationModal();
    return;
  }
  const url = `/UserDetailsForm/Index?code=${_globalSelectedUserCode}&actionMode=view`;
  await safelyNavigateTo(url);
});

$("#btn-user-grid-delete").on("click", function () {
  if (!_globalSelectedUserCode) {
    toastr.warning($("#selectAUser").val());
    return;
  }
  else if (_globalSelectedUserCode === $("#loggedInUserCode").val()) {
    showUnAuthorizedOperationModal();
    return;
  }
  if (checkIfSelectedUserHasHigherPriorityThanLoggedInUser()) {
    showUnAuthorizedOperationModal();
    return;
  }
  const title = document.getElementById("removeUserModalTitle").value;
  const message = document.getElementById("confirmDeleteUserDetails").value.replace("{0}", _globalSelectedUserCode);

  showConfirmModal(title, message).then(async (result) => {
    if (result) {
      const url = "UserDetailsForm/DeleteUserDetails";
      await ajaxPostAsync(url, _globalSelectedUserCode)
        .then(async () => {
          toastr.success($("#userDeletedSuccessfully").val());
          await loadUserDetails();
          await loadGroupInfoDetails();
          _globalSelectedUserGroupCode = null;
          await loadAndCreateGroupMembersGridDataTable();
          $("#add-to-group").hide();
          $("#btn-remove-from-group").hide();
          userGroupGridDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        })
        .catch(() => {
          toastr.error($("#failedToDeleteUser").val());
        });
    }
  });
});

let checkIfSelectedUserHasHigherPriorityThanLoggedInUser = () => {
  const groupInfoGridFirstRow = groupInfoGridDataTable.row(0).data();
  if (!isNullOrUndefined(groupInfoGridFirstRow)) {
    return getSecurityLevelByGroupCode(groupInfoGridFirstRow.group) > parseInt($("#loggedInUserSecurityLevel").val());
  }
  return false;  
}
