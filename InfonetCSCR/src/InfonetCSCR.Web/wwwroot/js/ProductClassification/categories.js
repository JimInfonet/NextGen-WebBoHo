let departmentDatatable;
let departments;
let selectedDepartment;
let selectedDepartmentTableRow = 0;
let selectedDepartmentRowData;
let eodGroupData;

let loadDepartments = async () => {
  let url = "StockItems/GetDepartments";
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      if (response) {
        departments = response;
        if (departments.length > 0 && !selectedDepartment) {
          selectedDepartment = departments[0].deptId;
        }

        createDatatable(departmentText);

        let noDepartmentExists = (departments.length === 0);
        if ($("#taxesDepartmentButtonId")) {
          $("#taxesDepartmentButtonId").prop("disabled", noDepartmentExists)
        }
        if ($("#chargesDepartmentButtonId")) {
          $("#chargesDepartmentButtonId").prop("disabled", noDepartmentExists)
        }
        if ($("#taxExemptDepartmentButtonId")) {
          $("#taxExemptDepartmentButtonId").prop("disabled", noDepartmentExists)
        }
        if ($("#categoryDeleteButtonId")) {
          $("#categoryDeleteButtonId").prop("disabled", noDepartmentExists)
        }
        if ($("#productDiscountCodeDepartmentButtonId")) {
          $("#productDiscountCodeDepartmentButtonId").prop("disabled", noDepartmentExists)
        }

        await loadSubDepartments();
      }
    })
    .catch((error) => {
      toastr.error(error);
    });
}

$('#categories-grid').on('click', 'td', function () {
  let cellIndex = $(this).index();
  let cellData = $(this).text();

  let row = $(this).closest('tr');
  row.trigger('click');

  if (cellIndex === 2) {
    $("#editDetailModal").modal('show');
    $("#editDetails").val(cellData);
  }
  else if (cellIndex === 4) {
    $("#editCountDetailModal").modal('show');
    $("#editCountDetails").val(cellData);
  }
});

let editDetailsForDept = async () => {
  let details = $("#editDetails").val();
  let detailsValue = parseInt(details);

  if (!(detailsValue >= 1 && detailsValue <= 4)) {
    toastr.warning($("#theDetailsMustBeBetween1And4").val());
    return;
  }

  url = `ProductClassification/UpdateEodDetailByDept?eodDetail=${details}&dept=${selectedDepartment}`;
  await ajaxPostAsync(url)
    .then((response) => {
    })
    .catch((error) => {
      toastr.error(error);
    });

  await loadDepartments();
  $("#editDetailModal").modal('hide');
}

let editCountDetailsForDept = async () => {
  let countDetails = $("#editCountDetails").val();
  let countDetailsValue = parseInt(countDetails);

  if (!(countDetailsValue >= 1 && countDetailsValue <= 9)) {
    toastr.warning($("#theDetailsMustBeBetween1And9").val());
    return;
  }

  url = `ProductClassification/UpdateEodCountDetailByDept?eodCountDetail=${countDetails}&dept=${selectedDepartment}`;
  await ajaxPostAsync(url)
    .then((response) => {
    })
    .catch((error) => {
      toastr.error(error);
    });

  await loadDepartments();
  $("#editCountDetailModal").modal('hide');
}

let eodGroupFormButtonEnable = (enable) => {
  $("#newGroupButtonId").prop("disabled", !enable);
  $("#deleteGroupButtonId").prop("disabled", !enable);
  $("#linkGroupButtonId").prop("disabled", !enable);
  $("#eodGroupModalCancelButtonId").prop("disabled", !enable);
}

let eodGroupButtonClick = () => {
  $("#eodGroupModal").modal('show');
  $("#newEodGroupFormId").hide();
  $("#deleteEodGroupFormId").hide();
  $("#linkEodGroupFormId").hide();
  eodGroupFormButtonEnable(true);
}

let newGroupButtonClicked = () => {
  $("#newEodGroupFormId").show();
  $("#eodGroupId").val("");
  $("#eodGroupName").val("");
  eodGroupFormButtonEnable(false);
}

let newGroupCompleteButtonClicked = async () => {
  let eodGroupIdValue = $("#eodGroupId").val();
  let eodGroupId = parseInt(eodGroupIdValue);

  if (!(eodGroupId > 0 && eodGroupId <= 255)) {
    toastr.warning($("#eodGroupIdShouldBeNumericValueBetween0And255").val());
    return;
  }

  let eodGroupNameValue = $("#eodGroupName").val();
  if (eodGroupNameValue === "") {
    toastr.warning($("#eodGroupNameShouldNotBeBlank").val());
    return;
  }

  $("#newEodGroupFormId").hide();
  eodGroupFormButtonEnable(true);

  let isEodGroupAlreadyExists = false;
  let url = `ProductClassification/CheckGroupIdExists?eodGroupId=${eodGroupId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response === true) {
        isEodGroupAlreadyExists = true;
        toastr.warning($("#thisEodGroupIdAlreadyExists").val());
        return;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  if (isEodGroupAlreadyExists) {
    return;
  }

  let data = {
    eodGroupID: eodGroupId,
    eodGroupName: eodGroupNameValue
  }

  url = `ProductClassification/AddEODGroup`;
  await ajaxPostAsync(url, data)
    .then((response) => {
    })
    .catch((error) => {
      toastr.error(error);
    });

  $("#eodGroupModal").modal('hide');
}

let getSelectedEodGroupIds = () => {
  let selectedEodGroupIds = [];
  $(".eodGroupFormInput").each(function () {
    if ($(this).prop("checked")) {
      selectedEodGroupIds.push($(this).val());
    }
  });

  return selectedEodGroupIds;
}

let deleteEodGroupFormDeleteButtonClicked = async () => {
  let selectedEodGroupIds = getSelectedEodGroupIds();
  if (selectedEodGroupIds.length === 0) {
    toastr.warning($("#selectBeforeDeletingMessage").val());
    return;
  }

  let title = $('#deleteEodGroup').val();
  let message = $('#allSelectedEODGroupsWillBeDeletedContinue').val();

  await showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let numberOfGroupsDeleted = 0;
      for (let i = 0; i < selectedEodGroupIds.length; i++) {
        let selectedEodGroupId = selectedEodGroupIds[i];
        let isDepartmentExistsByEodGroupId = false;
        let eodGroupId = selectedEodGroupId.split(" -- ")[0];
        let url = `ProductClassification/CheckDepartmentExistsByEodGroupId?eodGroupId=${eodGroupId}`;
        await ajaxGetAsync(url, {})
          .then((response) => {
            if (response === true) {
              isDepartmentExistsByEodGroupId = true;
            }
          })
          .catch((error) => {
            toastr.error(error);
          });

        if (isDepartmentExistsByEodGroupId) {
          let title = $('#deleteEodGroup').val();
          let message = $('#queryToConinueDeleteEodGroup').val().replace("{0}", selectedEodGroupId);

          await new Promise(resolve => setTimeout(resolve, 100));

          await showConfirmModal(title, message).then(async (result) => {
            if (result === true) {
              url = `ProductClassification/DeleteEodGroupById?isDeptExistsForEodGroupId=true&eodGroupId=${eodGroupId}`;
              await ajaxPostAsync(url)
                .then((response) => {
                  numberOfGroupsDeleted++;
                })
                .catch((error) => {
                  toastr.error(error);
                });
            }
          });
        }
        else {
          url = `ProductClassification/DeleteEodGroupById?isDeptExistsForEodGroupId=false&eodGroupId=${eodGroupId}`;
          await ajaxPostAsync(url)
            .then((response) => {
              numberOfGroupsDeleted++;
            })
            .catch((error) => {
              toastr.error(error);
            });
        }
      }
      if (numberOfGroupsDeleted === selectedEodGroupIds.length) {
        $("#eodGroupModal").modal('hide');
        deleteEodGroupFormCancelButtonClicked();
      }
    }
  });

  await loadDepartments();
  await deleteEodGroupButtonClicked();
}

let renderEodGroupDataForDelete = () => {
  let eodGroupCheckboxes = $("#eodGroupCheckboxesId");
  eodGroupCheckboxes.html("");
  eodGroupData.forEach(eodGroup => {
    eodGroupCheckboxes.append(
      `<div class="form-check mt-2">
        <input class="form-check-input stock-item-form-input eodGroupFormInput" value="${eodGroup.eodGroupID} -- ${eodGroup.eodGroupName}" type="checkbox">
        <label class="form-check-label ms-1">
          ${eodGroup.eodGroupID} -- ${eodGroup.eodGroupName}
        </label>
      </div>`);
  });
}

let renderEodGroupDataFormSelect = () => {
  $("#linkEodGroupFormSelect").html(optionGeneratorDefault($("#eodGroupIdText").val()));
  eodGroupData.forEach(eodGroup => {
    $("#linkEodGroupFormSelect").append(optionGenerator(eodGroup.eodGroupID, eodGroup.eodGroupName));
  });
}

let loadEodGroupData = async () => {
  let url = "ProductClassification/GetGroups";
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response) {
        eodGroupData = response;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let newEodGroupFormCancelButtonClicked = () => {
  $("#newEodGroupFormId").hide();
  eodGroupFormButtonEnable(true);
}

let deleteEodGroupFormCancelButtonClicked = () => {
  $("#deleteEodGroupFormId").hide();
  eodGroupFormButtonEnable(true);
}

let linkEodGroupFormCancelButtonClicked = () => {
  $("#linkEodGroupFormId").hide();
  eodGroupFormButtonEnable(true);
}

let deleteEodGroupButtonClicked = async () => {
  $("#deleteEodGroupFormId").show();
  eodGroupFormButtonEnable(false);
  await loadEodGroupData();
  renderEodGroupDataForDelete()
}

let linkEodGroupButtonClicked = async () => {
  $("#linkEodGroupFormId").show();
  $("#linkEodGroupApplyButtonId").prop("disabled", true);
  eodGroupFormButtonEnable(false);
  await loadEodGroupData();
  renderEodGroupDataFormSelect()
}

let linkEodGroupSelectChanged = (id) => {
  if (id) {
    $("#linkEodGroupApplyButtonId").prop("disabled", false);
  }
  else {
    $("#linkEodGroupApplyButtonId").prop("disabled", true);
  }
}

let linkEodGroupFormApplyButtonClicked = async () => {
  let eodGroupId = $("#linkEodGroupFormSelect").val();

  url = `ProductClassification/UpdateEODGroupByDept?eodGroupId=${eodGroupId}&dept=${selectedDepartment}`;
  await ajaxPostAsync(url)
    .then((response) => {
    })
    .catch((error) => {
      toastr.error(error);
    });

  await loadDepartments();
  $("#eodGroupModal").modal('hide');
}

let clearNewCategoryForm = () => {
  $("#addNewCategoryCode").val("");
  $("#addNewCategoryName").val("");
  $("#addNewCategoryDetails").val("");
  $("#addNewCategoryCountDetails").val("");
}

let newCategoryButtonClick = () => {
  clearNewCategoryForm();
  $("#addNewCategoryModal").modal("show");
}

let checkDepartmentIdExists = async (deptId) => {
  let isDepartmentIdExists = false;
  let url = `ProductClassification/CheckDepartmentIdExists?departmentId=${deptId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response === true) {
        isDepartmentIdExists = true;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  return isDepartmentIdExists;
}

let addNewCategory = async () => {
  let code = $("#addNewCategoryCode").val();
  let categoryName = $("#addNewCategoryName").val();
  let details = $("#addNewCategoryDetails").val();
  let countDetails = $("#addNewCategoryCountDetails").val();

  if (!code) {
    toastr.warning($("#theCategoryCodeMustNotBeBlank").val());
    return;
  }
  else if (!categoryName) {
    toastr.warning($("#theCategoryNameMustNotBeBlank").val());
    return;
  }
  else if (!details) {
    toastr.warning($("#theDetailsMustBeBetween1And4").val());
    return;
  }
  else if (!countDetails) {
    toastr.warning($("#theCountDetailsMustBeBetween1And9").val());
    return;
  }

  let detailsValue = parseInt(details);

  if (!(detailsValue >= 1 && detailsValue <= 4)) {
    toastr.warning($("#theDetailsMustBeBetween1And4").val());
    return;
  }
  
  let isDepartmentIdExists = await checkDepartmentIdExists(code);
  if (isDepartmentIdExists) {
    toastr.warning($("#categoryCodeAlreadyExists").val().replace("{0}", code));
    return;
  }

  const url = `/ProductClassification/AddDepartment`;
  const data = {
    deptId: code,
    deptName: categoryName,
    eODDetail: details,
    cntDetail: countDetails
  };

  await ajaxPostAsync(url, data)
    .then((response) => {
      toastr.success($('#categorySuccessfullyAdded').val());
    })
    .catch((error) => {
      toastr.error(error);
    });

  selectedDepartmentTableRow = 0;
  selectedDepartment = null;
  await loadDepartments();
  clearNewCategoryForm();
  $("#addNewCategoryModal").modal("hide");
}

let CheckDepartmentExistsInFuelDept = async (departmentId) => {
  let isDepartmentExistsInFuelDept = false;
  let url = `ProductClassification/CheckDepartmentExistsInFuelDept?departmentId=${departmentId}`;

  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response === true) {
        isDepartmentExistsInFuelDept = true;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  return isDepartmentExistsInFuelDept;
}

let getCountOfProductsInDepartment = async (departmentId) => {
  let countOfItems = 0;
  let url = `ProductClassification/GetCountOfProductsInDepartment?departmentId=${departmentId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      countOfItems = response;
    })
    .catch((error) => {
      toastr.error(error);
    });

  return countOfItems;
}

let deleteCategory = async () => {
  let isDepartmentExistsInFuelDept = await CheckDepartmentExistsInFuelDept(selectedDepartment);
  if (isDepartmentExistsInFuelDept) {
    toastr.warning($("#categoryIsTheFuelCategoryAndCannotBeDeleted").val().replace("{0}", selectedDepartment));
    return;
  }

  let countOfItems = await getCountOfProductsInDepartment(selectedDepartment);
  let confimedDelete = true;

  let title = $('#confirmDelete').val();
  let message = $('#categoryIsUsedForStockItems').val().replace("{0}", selectedDepartment).replace("{1}", countOfItems);

  if (countOfItems === 0) {
    message = $('#areYouSureYouWantToDeleteCategory').val().replace("{0}", selectedDepartment);
  }

  await showConfirmModal(title, message).then(async (result) => {
    if (result === false) {
      confimedDelete = false;
    }
  });

  if (confimedDelete) {
    url = `ProductClassification/DeleteCategoryByDepartmentId?departmentId=${selectedDepartment}`;
    await ajaxPostAsync(url)
      .then((response) => {
        toastr.success($('#categorySuccessfullyDeleted').val().replace("{0}", selectedDepartment));
      })
      .catch((error) => {
        toastr.error(error);
      });

    selectedDepartmentTableRow = 0;
    selectedDepartment = null;
    await loadDepartments();
  }
}
