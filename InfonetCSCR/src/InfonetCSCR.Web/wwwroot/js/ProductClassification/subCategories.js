let subDepartmentDatatable;
let subDepartments;
let selectedSubDepartment;
let selectedSubDepartmentRowData;

let loadSubDepartments = async () => {
  let url = `StockItems/GetSubDepartments?department=${selectedDepartment}`;
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      if (response) {
        subDepartments = response;
        if (subDepartments.length > 0) {
          selectedSubDepartment = subDepartments[0].subDeptId;
        }

        createDatatable(subDepartmentText);

        let noSubDepartmentExists = (subDepartments.length === 0);
        if ($("#taxesSubDepartmentButtonId")) {
          $("#taxesSubDepartmentButtonId").prop("disabled", noSubDepartmentExists)
        }
        if ($("#chargesSubDepartmentButtonId")) {
          $("#chargesSubDepartmentButtonId").prop("disabled", noSubDepartmentExists)
        }
        if ($("#taxExemptSubDepartmentButtonId")) {
          $("#taxExemptSubDepartmentButtonId").prop("disabled", noSubDepartmentExists)
        }
        if ($("#subCategoryDeleteButtonId")) {
          $("#subCategoryDeleteButtonId").prop("disabled", noSubDepartmentExists)
        }
        if ($("#productDiscountCodeSubDepartmentButtonId")) {
          $("#productDiscountCodeSubDepartmentButtonId").prop("disabled", noSubDepartmentExists)
        }

        await loadSubDetails();
      }
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let newSubCategoryButtonClick = () => {
  $("#subCode").val("");
  $("#subName").val("");
  $("#addNewSubCategoryModal").modal("show");
}

let checkSubDepartmentIdExistsForDepartment = async (deptId, subDeptId) => {
  let isSubDepartmentIdExists = false;
  let url = `ProductClassification/checkSubDepartmentIdExistsForDepartment?departmentId=${deptId}&subDepartmentId=${subDeptId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response === true) {
        isSubDepartmentIdExists = true;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  return isSubDepartmentIdExists;
}

let addNewSubCategory = async () => {
  let subCode = $("#subCode").val();
  let subName = $("#subName").val();

  if (!subCode) {
    toastr.warning($("#theSubCategoryCodeMustNotBeBlank").val());
    return;
  }
  else if (!subName) {
    toastr.warning($("#theSubCategoryNameMustNotBeBlank").val());
    return;
  }

  let isSubDepartmentIdExists = await checkSubDepartmentIdExistsForDepartment(selectedDepartment, subCode);
  if (isSubDepartmentIdExists) {
    toastr.warning($("#subCategoryCodeAlreadyExists").val().replace("{0}", subCode));
    return;
  }

  const url = `/ProductClassification/AddSubDepartment`;
  const data = {
    deptId: selectedDepartment,
    SubDeptId: subCode,
    SubDeptName: subName
  };

  await ajaxPostAsync(url, data)
    .then((response) => {
      toastr.success($('#subCategorySuccessfullyAdded').val());
    })
    .catch((error) => {
      toastr.error(error);
    });

  await loadSubDepartments();
  $("#subCode").val("");
  $("#subName").val("");
  $("#addNewSubCategoryModal").modal("hide");
}

let getCountOfProductsInSubDepartment = async (departmentId, subDepartmentId) => {
  let countOfItems = 0;
  let url = `ProductClassification/GetCountOfProductsInSubDepartment?departmentId=${departmentId}&subDepartmentId=${subDepartmentId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      countOfItems = response;
    })
    .catch((error) => {
      toastr.error(error);
    });

  return countOfItems;
}

let deleteSubCategory = async () => {
  let isDepartmentExistsInFuelDept = await CheckDepartmentExistsInFuelDept(selectedDepartment);
  if (isDepartmentExistsInFuelDept) {
    toastr.warning($("#categoryIsTheFuelCategoryAndCannotBeDeleted").val().replace("{0}", selectedDepartment));
    return;
  }

  let countOfItems = await getCountOfProductsInSubDepartment(selectedDepartment, selectedSubDepartment);
  let confimedDelete = true;

  let title = $('#confirmDelete').val();
  let message = $('#areYouSureYouWantToDeleteSubCategory').val().replace("{0}", selectedSubDepartment);

  if (countOfItems !== 0) {
    message = $('#subCategoryIsUsedForStockItems').val().replace("{0}", selectedSubDepartment).replace("{1}", countOfItems);
  }

  await showConfirmModal(title, message).then(async (result) => {
    if (result === false) {
      confimedDelete = false;
    }
  });

  if (confimedDelete) {
    url = `ProductClassification/DeleteSubDepartment?departmentId=${selectedDepartment}&subDepartmentId=${selectedSubDepartment}`;
    await ajaxPostAsync(url)
      .then((response) => {
        toastr.success($('#subCategorySuccessfullyDeleted').val().replace("{0}", selectedSubDepartment));
      })
      .catch((error) => {
        toastr.error(error);
      });

    await loadSubDepartments();
  }
}
