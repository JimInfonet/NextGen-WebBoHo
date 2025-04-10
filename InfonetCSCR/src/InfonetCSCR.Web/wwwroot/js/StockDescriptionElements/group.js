let groupData = [];
let groupDataTable;
let selectedGroup;
let selectedGroupRow;
let availableFamily = [];

$(async function () {
  $('.text-danger').prop('hidden', true);
  await loadGroupData();
});

let loadGroupData = async () => {
  let url = "StockDescriptionElements/GetAllGroups";
  await ajaxGetAsync(url)
    .then(_groupData => {
      if (_groupData) {
        groupData = _groupData;
        let isDisableDeleteButtons = true;

        if (groupData.length > 0) {
          selectedGroup = groupData[0].groupID;
          selectedGroupRow = groupData[0];
          isDisableDeleteButtons = false;
        }
        enableOrDisableGroupDeleteButtons(isDisableDeleteButtons);
        createGroupDataTable();
      }
    })
    .catch(toastrError);
}

let enableOrDisableGroupDeleteButtons = (isDisable) => {
  $('#group-btn-delete').prop('disabled', isDisable);
}

let createGroupDataTable = () => {
  destroyDataTableIfExists(groupDataTable);

  let props = createDataTableProp();
  props.data = groupData;
  props.scrollY = '440px';
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "groupID", },
    { "data": "groupName", },
    { "data": "familyID", },
    { "data": "description", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3] }
  ];

  groupDataTable = createDataTableGeneric('group-grid', props);

  if (groupData.length > 0) {
    $(`#group-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#group-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (groupData.length === 0 || $(this).find('.dataTables_empty').length > 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        groupDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedGroup = $(this).find("td:first")[0].innerText;
        selectedGroupRow = groupDataTable.row(this).data();
      }
    });
}

let findRestFamilyToAdd = () => {
  familyData.forEach(family => {
    let isExist = groupData.find(obj => obj.familyID == family.familyID);

    if (!isExist) availableFamily.push(family);
  })
}

let onGroupAddClick = () => {
  availableFamily = [];
  findRestFamilyToAdd();

  if (!availableFamily.length) {
    toastr.warning($('#noFamilyLeftToAdd').val());
    return;
  }

  fillGroupFields();
  $('#validation-for-group-id').prop('hidden', true);
  $('#validation-for-group-name').prop('hidden', true);
  $('#validation-for-family').prop('hidden', true);
  $("#groupFormModal").modal('show');
  $('#select-or-unselect-all').prop('checked', false);

  let families = $('#family').empty();

  availableFamily.forEach(family => {
    families.append(`
      <div class="form-check my-1">
        <input type="checkbox" class="form-check-input select-by-option family-option" id="family-${family.familyID}" 
                value="${family.familyID}" onchange="revertSelectOrUnselectAll()">
        <label class="form-check-label" for="family-${family.familyID}">${family.familyID} - ${family.description}</label>
      </div>`
    )
  })
}

let revertSelectOrUnselectAll = () => {
  let isChecked = true;

  availableFamily.forEach(family => {
    isChecked &= $(`#family-${family.familyID}`).prop('checked');
  });

  $('#select-or-unselect-all').prop('checked', isChecked);
}

let selectOrUnselectAll = () => {
  let isSelectAll = $('#select-or-unselect-all').prop('checked');

  availableFamily.forEach(family => {
    $(`#family-${family.familyID}`).prop('checked', isSelectAll);
  })
}

let onGroupDeleteClick = async () => {
  let title = $("#deleteGroupTitle").val();
  let deleteItem = `${selectedGroup} - ${selectedGroupRow.familyID}`;
  let message = `${$("#deleteMessage").val()} ${deleteItem}?`;

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `StockDescriptionElements/DeleteGroup`;
      await ajaxPostAsync(url, selectedGroupRow)
        .then(async (response) => {
          if (response) {
            toastr.success($("#groupDeletedSuccessfully").val());
            await loadGroupData();
          }
        })
        .catch(toastrError);
    }
  });
}

let onGroupCompleteClick = async () => {
  let groupFormData = prepareGroupFormData();
  let checkedFamilies = [];

  availableFamily.forEach(family => {
    let isChecked = $(`#family-${family.familyID}`).prop('checked');

    if (isChecked) checkedFamilies.push(family.familyID);
  });

  groupFormData.families = checkedFamilies;

  if (!isGroupFormValid(groupFormData)) return;

  let url = `StockDescriptionElements/AddGroup`;
  await ajaxPostAsync(url, groupFormData)
    .then(async (response) => {
      if (response) {
        toastr.success($("#groupAddedSuccessfully").val());
        await loadGroupData();
        $("#groupFormModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let prepareGroupFormData = () => {
  const formData = {
    groupID: $("#group-id").val(),
    groupName: $("#group-name").val()
  }
  return formData;
}

let isGroupFormValid = (groupFormData) => {
  let isValid = true;
  $('.text-danger').prop('hidden', true);

  if (isNullOrWhiteSpace(groupFormData.groupID)) {
    $('#validation-for-group-id').prop('hidden', false);
    isValid = false;
  }

  if (isNullOrWhiteSpace(groupFormData.groupName)) {
    $('#validation-for-group-name').prop('hidden', false);
    isValid = false;
  }

  if (!groupFormData.families.length) {
    $('#validation-for-family').prop('hidden', false);
    isValid = false;
  }

  return isValid;
}

let fillGroupFields = (formData = null) => {
  $("#group-id").val(formData?.groupID);
  $("#group-name").val(formData?.groupName);
}
