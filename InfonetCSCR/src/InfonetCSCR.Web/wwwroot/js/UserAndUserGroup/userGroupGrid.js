let userGroupGridDataTable;
let userGroupData;
let _globalSelectedUserGroupCode;

let createUserGroupGridDataTable = () => {
  destroyDataTableIfExists(userGroupGridDataTable);

  let props = createDataTableProp();
  props.data = userGroupData;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  props.columns = [
    { "data": "code", },
    { "data": "name", },
    { "data": "securityLevel", }
  ];

  userGroupGridDataTable = createDataTableGeneric('data-table-user-groups-grid', props);

  $('#data-table-user-groups-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (userGroupData.length === 0) {
        return;
      }

      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        _globalSelectedUserGroupCode = null;
        await loadAndCreateGroupMembersGridDataTable();
      } else {
        userGroupGridDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        _globalSelectedUserGroupCode = $(this).find("td:first")[0].innerText;
        await loadAndCreateGroupMembersGridDataTable();
      }
    });
}

let loadUserGroupData = async () => {
  const url = "UserGroupGrid/GetAllUserGroups";
  const errorMessage = $("#anErrorHasOccurredWhileLoadingUserGroupGrid").val();

  await ajaxGetAsync(url)
    .then((_userGroupData) => {
      userGroupData = _userGroupData;
      createUserGroupGridDataTable();
    })
    .catch(() => toastr.error(errorMessage));
}

$("#btn-user-group-grid-add").on("click", async function () {
  const url = `/UserGroupForm/Index`;
  await safelyNavigateTo(url);
});

$("#btn-user-group-grid-delete").on("click", function () {
  if (isNullOrUndefined(_globalSelectedUserGroupCode)) {
    toastr.warning($("#selectAUserGroup").val());
    return;
  }
  if (checkIfSelectedUserGroupHasHigherPriorityThanLoggedInUser()) {
    showUnAuthorizedOperationModal();
    return;
  }
  const title = document.getElementById("removeGroupModalTitle").value;
  const message = document.getElementById("confirmDeleteUserGroup").value.replace("{0}", _globalSelectedUserGroupCode);

  showConfirmModal(title, message).then(async (result) => {
    if (result) {
      const url = "UserGroupGrid/RemoveUserGroup";
      await ajaxPostAsync(url, _globalSelectedUserGroupCode)
        .then(async () => {
          toastr.success($("#groupDeletedSuccessfully").val());
          await loadUserGroupData();
          await loadAndCreateGroupMembersGridDataTable();
          await loadOptionsForAddGroup();
          // If the group selected by user is deleted, then refresh the groupInfoGrid
          const groupInfoGridFirstRow = groupInfoGridDataTable.row(0).data();
          if (!isNullOrUndefined(groupInfoGridFirstRow) && groupInfoGridFirstRow.group === _globalSelectedUserGroupCode) {
            _globalGroupInfo = null;
            createGroupInfoGridDataTable();
            $("#btn-remove-from-group").hide();
            $('#add-to-group').find('option[value=""]').prop('selected', true);
            $("#add-to-group").show();
          }
        })
        .catch(() => {
          toastr.error($("#failedToDeleteGroup").val());
        });
    }
  });
});

let getSecurityLevelByGroupCode = (groupCode) => {
  let securityLevel = 0;
  userGroupGridDataTable.rows().every(function () {
    const rowData = this.data();
    if (rowData.code === groupCode) {
      securityLevel = parseInt(rowData.securityLevel);
      return false;
    }
    return true;
  });

  return securityLevel;
}

let showUnAuthorizedOperationModal = () => {
  const title = document.getElementById("unAuthorizedOperation").value;
  const message = document.getElementById("notAuthorizedMessage").value;
  showAlertModal(title, message);
}

let checkIfSelectedUserGroupHasHigherPriorityThanLoggedInUser = () => {
  return getSecurityLevelByGroupCode(_globalSelectedUserGroupCode) > parseInt($("#loggedInUserSecurityLevel").val());
}
