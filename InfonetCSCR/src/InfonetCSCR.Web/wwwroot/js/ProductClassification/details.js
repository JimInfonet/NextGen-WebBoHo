let subDetailDatatable;
let subDetails;
let selectedDetailRowData;
let selectedDetail;

let loadSubDetails = async () => {
  let url = `StockItems/GetSubDetails?department=${selectedDepartment}&subDepartment=${selectedSubDepartment}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response) {
        subDetails = response;

        if (subDetails.length > 0) {
          selectedDetail = subDetails[0].subDetailId;
        }

        createDatatable(detailText);

        let noDetailExists = (subDetails.length === 0);
        if ($("#detailDeleteButtonId")) {
          $("#detailDeleteButtonId").prop("disabled", noDetailExists)
        }
        if ($("#taxesDetailButtonId")) {
          $("#taxesDetailButtonId").prop("disabled", noDetailExists)
        }
        if ($("#chargesDetailButtonId")) {
          $("#chargesDetailButtonId").prop("disabled", noDetailExists)
        }
        if ($("#taxExemptDetailButtonId")) {
          $("#taxExemptDetailButtonId").prop("disabled", noDetailExists)
        }
        if ($("#productDiscountCodeDetailButtonId")) {
          $("#productDiscountCodeDetailButtonId").prop("disabled", noDetailExists)
        }
      }
    })
    .catch((error) => {
      toastr.error(error);
    });
}

let newDetailButtonClick = () => {
  $("#detailCode").val("");
  $("#detailName").val("");
  $("#addNewDetailModal").modal("show");
}

let checkDetailIdExistsForDepartmentAndSubDepartment = async (deptId, subDeptId, detailId) => {
  let isDetailIdExists = false;
  let url = `ProductClassification/CheckDetailIdExistsForDepartmentAndSubDepartment?departmentId=${deptId}&subDepartmentId=${subDeptId}&detailId=${detailId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response === true) {
        isDetailIdExists = true;
      }
    })
    .catch((error) => {
      toastr.error(error);
    });

  return isDetailIdExists;
}

let addNewDetail = async () => {
  let detailCode = $("#detailCode").val();
  let detailName = $("#detailName").val();

  if (!detailCode) {
    toastr.warning($("#theDetailCodeMustNotBeBlank").val());
    return;
  }
  else if (!detailName) {
    toastr.warning($("#theDetailNameMustNotBeBlank").val());
    return;
  }

  let isDetailIdExists = await checkDetailIdExistsForDepartmentAndSubDepartment(
    selectedDepartment,
    selectedSubDepartment,
    detailCode
  );

  if (isDetailIdExists) {
    toastr.warning($("#detailCodeAlreadyExistsInputUniqueDetailCode").val().replace("{0}", detailCode));
    return;
  }

  const url = `/ProductClassification/AddDetail`;
  const data = {
    deptId: selectedDepartment,
    subDeptId: selectedSubDepartment,
    subDetailId: detailCode,
    subDetailName: detailName
  };

  await ajaxPostAsync(url, data)
    .then((response) => {
      toastr.success($('#subDetailSuccessfullyAdded').val());
    })
    .catch((error) => {
      toastr.error(error);
    });

  await loadSubDetails();
  $("#detailCode").val("");
  $("#detailName").val("");
  $("#addNewDetailModal").modal("hide");
}

let getCountOfProductsInDetail = async (departmentId, subDepartmentId, detailId) => {
  let countOfItems = 0;
  let url = `ProductClassification/GetCountOfProductsInDetail?departmentId=${departmentId}&subDepartmentId=${subDepartmentId}&detailId=${detailId}`;
  await ajaxGetAsync(url, {})
    .then((response) => {
      countOfItems = response;
    })
    .catch((error) => {
      toastr.error(error);
    });

  return countOfItems;
}

let deleteDetail = async () => {
  let isDepartmentExistsInFuelDept = await CheckDepartmentExistsInFuelDept(selectedDepartment);
  if (isDepartmentExistsInFuelDept) {
    toastr.warning($("#categoryIsTheFuelCategoryAndCannotBeDeleted").val().replace("{0}", selectedDepartment));
    return;
  }

  let countOfItems = await getCountOfProductsInDetail(selectedDepartment, selectedSubDepartment, selectedDetail);
  let confimedDelete = true;

  let title = $('#confirmDelete').val();
  let message = $('#areYouSureYouWantToDeleteSubDetail').val().replace("{0}", selectedDetail);

  if (countOfItems !== 0) {
    message = $('#subDetailIsUsedForStockItems').val().replace("{0}", selectedDetail).replace("{1}", countOfItems);
  }

  await showConfirmModal(title, message).then(async (result) => {
    if (result === false) {
      confimedDelete = false;
    }
  });

  if (confimedDelete) {
    url = `ProductClassification/DeleteDetail?departmentId=${selectedDepartment}&subDepartmentId=${selectedSubDepartment}&detailId=${selectedDetail}`;
    await ajaxPostAsync(url)
      .then((response) => {
        toastr.success($('#subDetailSuccessfullyDeleted').val().replace("{0}", selectedDetail));
      })
      .catch((error) => {
        toastr.error(error);
      });

    await loadSubDetails();
  }
}
