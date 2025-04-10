const changePassPolicy = document.getElementById("U_CHGPASS").value === "True";

let changePasswordButtonClick = async () => {
  await loadOptionsForSelectUser();
  $("#select-a-user").val("");
  $("#input-password").val("");
  $("#input-confirm-password").val("");
  $("#changePasswordModal").modal("show");
}

let loadOptionsForSelectUser = async () => {
  const url = `UserGrid/GetUserCodes`;

  await ajaxGetAsync(url)
    .then((_userCodes) => {
      const userCodes = _userCodes.map((userCode, index) => ({
        value: userCode,
        name: "",
        isSelected: false
      }));

      $('#select-a-user option:not(:first-child)').remove();
      let control = $("#select-a-user");
      renderSelectOptions(control, userCodes);
    })
    .catch(toastrError);
}

async function changeUserPassword() {
  if (isNullUndefinedOrWhiteSpace($("#select-a-user").val())) {
    toastr.warning($("#selectAUser").val());
    return;
  }
  if (isNullUndefinedOrWhiteSpace($("#input-password").val())) {
    toastr.warning($("#emptyPasswordModalMessage").val());
    return;
  } else if ($("#input-password").val() !== $("#input-confirm-password").val()) {
    toastr.warning($("#passwordNotMatchedModalMessage").val());
    return;
  }

  const userDetails = {
    code: $("#select-a-user").val(),
    password: $("#input-password").val()
  };

  const url = `UserGrid/ChangeUserPassword`;
  await ajaxPostAsync(url, userDetails)
    .then(() => {
      const notification = document.getElementById("passwordChangedSuccessfully").value.replace("{0}", $("#select-a-user").val());
      toastr.success(notification);
      $("#changePasswordModal").modal("hide");
    })
    .catch(toastrError);
}
