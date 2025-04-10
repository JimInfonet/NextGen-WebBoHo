$(async function () {
  await loadSecurityLevelsForAddGroup();
  _globalIsEditing = true;
  setGlobalEditingState(_globalIsEditing);
  if ($("#isUserGroupAlreadyExists").val() === "true") {
    const message = document.getElementById("userGroupAlreadyExists").value.replace("{0}", $("#Code").val());
    toastr.error(message);
    $("#SecurityLevel").val($("#selectedSecurityLevel").val());
  }
});

let loadSecurityLevelsForAddGroup = async () => {
  const url = `GetSecurityLevels`;

  await ajaxGetAsync(url)
    .then((securityLevels) => {
      const securityLevelsData = securityLevels.map((securityLevel, index) => ({
        value: securityLevel.groupId,
        name: securityLevel.groupName,
        isSelected: false
      }));
      $('#SecurityLevel option:not(:first-child)').remove();
      let control = $("#SecurityLevel");
      renderSelectOptions(control, securityLevelsData);
    });
}

$("#btn-cancel").on("click", async function () {
  const url = "/UserGrid";
  await safelyNavigateTo(url);
});
