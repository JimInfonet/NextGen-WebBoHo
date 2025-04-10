let groupInfoGridDataTable;
let _globalGroupInfo;

let createGroupInfoGridDataTable = () => {
  destroyDataTableIfExists(groupInfoGridDataTable);

  let props = createDataTableProp();
  props.data = _globalGroupInfo;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];
  props.paging = false;
  props.bInfo = false;
  props.columns = [
    { "data": "group", },
    { "data": "groupName", }
  ];

  groupInfoGridDataTable = createDataTableGeneric('data-table-group-info-grid', props);
}

let loadGroupInfoDetails = async () => {
  const url = `UserGrid/GetGroupInfo?userCode=${_globalSelectedUserCode}`;
  const errorMessage = $("#anErrorHasOccurredWhileLoadingGroupInfoGrid").val();

  await ajaxGetAsync(url)
    .then((_groupInfo) => {
      _globalGroupInfo = _groupInfo;
      createGroupInfoGridDataTable();
    })
    .catch(() => toastr.error(errorMessage));
}

let loadOptionsForAddGroup = async () => {
  const url = `UserGrid/GetGroupCodes`;

  await ajaxGetAsync(url)
    .then((_groupCodes) => {
      const groupCodes = _groupCodes.map((groupCode) => ({
        value: groupCode,
        name: "",
        isSelected: false
      }));

      $('#add-to-group option:not(:first-child)').remove();
      let control = $("#add-to-group");
      renderSelectOptions(control, groupCodes);
    });
}

$('#add-to-group').on('change', async function () {
  const selectedValue = $(this).val();
  const url = `/UserGrid/UpdateGroupInfo?user=${_globalSelectedUserCode}&group=${selectedValue}`;
  await ajaxPostAsync(url)
    .then(async () => {
      toastr.success($("#userAddedToTheGroupSuccessfully").val());
      await loadGroupInfoDetails();
      $("#add-to-group").hide();
      $("#btn-remove-from-group").show();
      await selectRelevantRowOfWhichTheSelectedUserBelongs(selectedValue);
    });
});

$("#btn-remove-from-group").on("click", async function () {
  const url = `/UserGrid/DeleteUIGRecord?user=${_globalSelectedUserCode}&group=${groupInfoGridDataTable.row(0).data().group}`;
  await ajaxPostAsync(url)
    .then(async (response) => {
      if (response) {
        toastr.success($("#userRemovedFromTheGroupSuccessfully").val());
        await loadGroupInfoDetails();
        _globalSelectedUserGroupCode = null;
        await loadAndCreateGroupMembersGridDataTable();
        $("#btn-remove-from-group").hide();
        $('#add-to-group').find('option[value=""]').prop('selected', true);
        $("#add-to-group").show();
        // Remove the selected row of UserGroup data table
        userGroupGridDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
      } else {
        showUnAuthorizedOperationModal();
      }
    });
});

async function selectRelevantRowOfWhichTheSelectedUserBelongs(groupCode) {
  userGroupGridDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
  await userGroupGridDataTable.rows().every(async function (rowIndex) {
    const rowData = this.data();
    if (rowData.code === groupCode) {
      _globalSelectedUserGroupCode = groupCode;
      const rowNode = userGroupGridDataTable.row(rowIndex).node();
      $(rowNode).addClass('selected-grid-row');
      await loadAndCreateGroupMembersGridDataTable();
      return false;
    }
    return true;
  });
}
