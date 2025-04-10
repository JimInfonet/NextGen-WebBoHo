let groupMembersGridDataTable;
let groupMembersData;

let createGroupMembersGridDataTable = () => {
  destroyDataTableIfExists(groupMembersGridDataTable);

  let props = createDataTableProp();
  props.data = groupMembersData;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  props.columns = [
    { "data": "code", },
    { "data": "name", }
  ];

  groupMembersGridDataTable = createDataTableGeneric('data-table-group-members-grid', props);
}

let loadGroupMembersData = async () => {
  const url = `UserGroupGrid/GetAllUsersByGroup?groupCode=${_globalSelectedUserGroupCode}`;
  const errorMessage = $("#anErrorHasOccurredWhileLoadingGroupMembers").val();

  await ajaxGetAsync(url)
    .then((_groupMembersData) => {
      groupMembersData = _groupMembersData;
    })
    .catch(() => toastr.error(errorMessage));
}

let loadAndCreateGroupMembersGridDataTable = async () => {
  if (!isNullOrUndefined(_globalSelectedUserGroupCode)) {
    await loadGroupMembersData();
  } else {
    groupMembersData = null;
  }
  createGroupMembersGridDataTable();
}
