const URL_GET_CARD_PRODUCT_RESTRICTION = "/CardProductRestriction";
let department;
let subDepartment;
let subDetail;
let stockCodes;
let profileId;
let profileName;
let action;
let isProfileRestrctionForm;
let actionMode = '';

$(async function () {
  await initProductRestrictionFormPage();
});

let initProductRestrictionFormPage = async () => {

  const urlParams = new URLSearchParams(window.location.search);
  profileId = urlParams.get('profileId');
  profileName = urlParams.get('profileName');
  action = urlParams.get('mode');
  actionMode = urlParams.get('actionMode');
  isProfileRestrctionForm = document.getElementById("isProfileRestrctionForm").value;

  if (!isNullUndefinedOrWhiteSpace(isProfileRestrctionForm) && action == "Add") {
    $("#cardDescription").val(profileName);
    $("#cardDescriptionField").val(profileName);
  }
  else {
    let cardDescriptionValue = $("#cardDescription").val();
    $("#cardDescriptionField").val(cardDescriptionValue);
  }

  if ($("#isDepartmentAndStockCodeNull").val() === "True") {
    toastr.warning($("#pleaseSelectDeptSubDeptSubDetailStockCode").val());
  }
  if ($("#isProductRestrictionExist").val() === "True") {
    toastr.warning($("#productLinkAlreadyExists").val());
  }
  if ($("#isProductRestrictionAdded").val() === "False") {
    toastr.error($("#failedToAddProductRestriction").val());
  }

  await loadDepartmentDropdown();

  if ($('#mode').val() === 'Edit') {
    $('#productRestrictionForm').attr('action', '/CardProductRestriction/UpdateProductRestriction');

    if ($('#stockCodeHidden').val()) {
      $('#stockCode').val($('#stockCodeHidden').val());
    }
    else {
      await loadSubDepartmentDropdown();
      if ($('#subDeptHidden').val()) {
        await loadSubDetailDropdown();
      }
    }
  }
  else {
    $('#productRestrictionForm').attr('action', '/CardProductRestriction/AddProductRestriction');
  }

  document.getElementById("btn-cancel").disabled = false;
  document.getElementById("btn-complete").disabled = false;
}

let loadDepartmentDropdown = async () => {
  let url = "/StockItems/GetDepartments";
  ajaxGetAsync(url, {})
    .then(async (response) => {
      if (response) {
        department = response;
        let initialOptions = [
          { text: $("#select").val(), value: "" },
        ];
        await renderCategoryDropdown("department", response, initialOptions, "department")
        let selectedDepartment = $('#departmentHidden').val();
        
        if (selectedDepartment) {
          $(`#department option[value=${selectedDepartment}]`).attr('selected', 'selected')
        }
        
      }
      else {
        toastr.error($("#failedToLoadDepartments").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingDepartment").val());
    });
}

let loadSubDepartmentDropdown = async () => {
  let department = $('#department').val() || $('#departmentHidden').val();
  if (!department) return;

  let url = `/StockItems/GetSubDepartments?department=${department}`
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      if (response) {
        subDepartment = response;
        let initialOptions = [
          { text: $("#select").val(), value: "" },
        ];

        await renderCategoryDropdown("subDept", response, initialOptions, "sub-department");
        let selectedSubDept = $('#subDeptHidden').val();

        if (selectedSubDept) {
          $(`#subDept option[value=${selectedSubDept}]`).attr('selected', 'selected')
        }
      }
      else {
        toastr.error($("#failedToLoadSubDepartment").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingSubDepartment").val());
    });
}

let loadSubDetailDropdown = async () => {
  let department = $("#department").val() || $('#departmentHidden').val();
  let subDepartment = $("#subDept").val() || $('#subDeptHidden').val();
  if (!department || !subDepartment) return;
  let url = `/StockItems/GetSubDetails?department=${department}&subDepartment=${subDepartment}`
  await ajaxGetAsync(url, {})
    .then(async (response) => {
      if (response) {
        subDetail = response;
        let initialOptions = [
          { text: $("#select").val(), value: "" },
        ];

        await renderCategoryDropdown("subDetail", response, initialOptions, "sub-detail");
        let selectedSubDetail = $('#subDetailHidden').val();
     
        if (selectedSubDetail) {
          $(`#subDetail option[value=${selectedSubDetail}]`).attr('selected', 'selected')
        }
      }
      else {
        toastr.error($("#failedToLoadSubDetails").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingSubDetails").val());
    });
}

// On Cancel button click
$("#btn-cancel").on('click', function () {
  let bankCardId = $('#bankCardId').val();
  let cardDescription = $('#cardDescription').val();
  let cardCode = $("#cardCode").val();
  let tendCodeData = $("#tendCodeData").val();
  let fromGrid = $("#fromGrid").val();

  let url = `${URL_GET_CARD_PRODUCT_RESTRICTION}?bankCardId=${bankCardId}&cardDescription=${cardDescription}&cardCode=${cardCode}&tendCodeData=${tendCodeData}&fromGrid=${fromGrid}`;

  let newProfileId = $("#profileId").val();
  let newProfileName = $("#cardDescription").val();

  if (!isNullUndefinedOrWhiteSpace(isProfileRestrctionForm))
    url = `${URL_GET_CARD_PRODUCT_RESTRICTION}?profileId=${encodeURIComponent(newProfileId)}&profileName=${newProfileName}&actionMode=${actionMode}`;

  safelyNavigateTo(url);
})

// On Department change
$("#department").on('change', async function () {
  $('#stockCode').val("");
  if ($('#department').val()) {
    let description = department.filter(x => x.deptId === $('#department').val())[0]?.deptName;
    $('#description').val(description);
    await loadSubDepartmentDropdown();

    let subDetailSelection = $('#subDetail');
    subDetailSelection.find('option').remove();
    subDetailSelection.append(`<option value="">${$("#select").val()}</option>`);
  }
  $('#subDept').val("");
  $('#subDetail').val("");
  $("#stockCode").val();
})

// On SubDepartment change
$("#subDept").on('change', async function () {
  if ($('#department').val() && $('#subDept').val()) {
    let description = subDepartment.filter(x => x.subDeptId === $('#subDept').val())[0]?.subDeptName;
    $('#description').val(description);
    await loadSubDetailDropdown();
  }
  $('#subDetail').val("");
})

// On SubDetail change
$("#subDetail").on('change', async function () {
  let description = subDetail.filter(x => x.subDetailId === $('#subDetail').val())[0]?.subDetailName;
  $('#description').val(description);
})

// On StockCode change
let onStockCodeChange = () => {
  $('#department').val("");
  $('#subDept').val("");
  $('#subDetail').val("");

  let subDeptSelection = $('#subDept');
  subDeptSelection.find('option').remove();
  subDeptSelection.append(`<option value="">${$("#select").val()}</option>`);

  let subDetailSelection = $('#subDetail');
  subDetailSelection.find('option').remove();
  subDetailSelection.append(`<option value="">${$("#select").val()}</option>`);
}

// On Card Product Code change
$("#cardProductCode").on('change', function () {
  $('#cardProductCodeValidationText').prop("hidden", false);
})

let isCardRestrictionFormValid = () => {
  if ($('#cardProductCode').val() === "" || $('#cardProductCode').val().length > 16) {
    return false;
  }
  if ($('#cardProductName').val().length > 16) {
    return false;
  }
  if ($('#amount').val() < -214748.3648 || $('#amount').val() > 214748.3647) {
    return false;
  }
  return true;
}

document.getElementById('productRestrictionForm').addEventListener('submit', async function (event) {
  event.preventDefault(); // Prevents the default form submission behavior

  if (!$("#department").val() && !$("#subDept").val() && !$("#subDetail").val() && !$("#stockCode").val()) {
    toastr.warning($("#pleaseSelectDeptSubDeptSubDetailStockCode").val());
    return false;
  }

  if (!isNullUndefinedOrWhiteSpace(isProfileRestrctionForm)) {
    // Product Restriction via Card Definitions
    const formObject = {
      profileID: $("#profileId").val(),
      dept: $("#department").val(),
      subDept: $("#subDept").val(),
      subDetail: $("#subDetail").val(),
      stockCode: $("#stockCode").val()
    };

    const preFormObject = {
      profileID: $("#preProfileId").val(),
      dept: $("#preDepartment").val(),
      subDept: $("#preSubDept").val(),
      subDetail: $("#preSubDetail").val(),
      stockCode: $("#preStockCode").val()
    };

    if (JSON.stringify(formObject) !== JSON.stringify(preFormObject)) {
      let url = "/CardProductRestriction/IsProductRestrictionExist";
      await ajaxGetAsync(url, formObject)
        .then(_alreadyExists => {
          if (_alreadyExists) {
            
            toastr.warning($("#productLinkAlreadyExists").val());
            return false;
          }
          else {
            $('#description').prop("disabled", false);
            this.submit();
          }
        });
    }
    else {
      let x=$("#cardDescriptionField").val();
      let url = `/CardProductRestriction?profileId=${encodeURIComponent(formObject.profileID)}&profileName=${x}&actionMode=null&ShowToaster=Edit`
      safelyNavigateTo(url);
    }
  }
  else {
    // Product Restriction via Card Restriction Profile

    $('#cardProductCodeValidationText').prop("hidden", false);

    if (!isCardRestrictionFormValid()) {
      return;
    }
    
    const formObject = {
      profileID: $("#profileId").val(),
      bankCardID: $("#bankCardId").val(),
      cardProductCode: $("#cardProductCode").val(),
      cardDescription: $("#cardDescriptionField").val(),
      cardProductDescription: $("#cardProductName").val(),
      dept: $("#department").val(),
      subDept: $("#subDept").val(),
      subDetail: $("#subDetail").val(),
      stockCode: $("#stockCode").val(),
      description: $("#description").val(),
      cardNumber: $("#cardNumber").val(),
      amount: $("#amount").val()
    };

    const preFormObject = {
      profileID: $("#preProfileId").val(),
      bankCardID: $("#preBankCardId").val(),
      cardProductCode: $("#preCardProductCode").val(),
      cardDescription: $("#cardDescriptionField").val(),
      cardProductDescription: $("#preCardProductDescription").val(),
      dept: $("#preDepartment").val(),
      subDept: $("#preSubDept").val(),
      subDetail: $("#preSubDetail").val(),
      stockCode: $("#preStockCode").val(),
      description: $("#preDescription").val(),
      cardNumber: $("#cardNumber").val(),
      amount: $("#preAmount").val()
    };

    if (JSON.stringify(formObject) !== JSON.stringify(preFormObject)) {
      let url = "/CardProductRestriction/IsProductRestrictionExist";
      await ajaxGetAsync(url, formObject)
        .then(_alreadyExists => {
          if (_alreadyExists) {
            toastr.warning($("#productLinkAlreadyExists").val());
            return false;
          }
          else {
            $('#bankCardId').prop("disabled", false);
            $('#description').prop("disabled", false);
            this.submit();
          }
        });
    }
    else {
      let cardCode = $("#cardCode").val();
      let tendCodeData = $("#tendCodeData").val();
      let fromGrid = $("#fromGrid").val();
      
      let url = `/CardProductRestriction?bankCardId=${formObject.bankCardID}&cardDescription=${formObject.cardDescription}&cardCode=${cardCode}&tendCodeData=${tendCodeData}&fromGrid=${fromGrid}&actionMode=update&ShowToaster=Edit`
      safelyNavigateTo(url);
    }
  }
});

let showStockTable = async () => {
  await loadStockCode();
  $("#stockCodeDescriptionDataTableModal").modal('show');
}

async function loadStockCode() {
  let url = "/CardProductRestriction/GetStockCodesForProductRestriction";
  await ajaxGetAsync(url)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);

  renderStockCodeDataTable(onStockCodeRowClick);
}

let onStockCodeRowClick = (rowData) => {
  $("#stockCode").val(rowData.stockCode);
  $("#description").val(rowData.description);
  $("#stockCodeDescriptionDataTableModal").modal('hide');

  onStockCodeChange();
}
