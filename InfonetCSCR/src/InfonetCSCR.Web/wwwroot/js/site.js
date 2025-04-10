// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

toastr.options.preventDuplicates = true;
toastr.options.progressBar = true;

let cardDefinitionGridUrl = "CardDefinitionGrid";
let purchasingAndReceivingPolicies;
let previousValue;

$(async function () {
  await init();
});

let selectNavItem = async (navItemId) => {
  let current = document.getElementsByClassName("selected");
  let currentSelectedNavItemMenu = document.getElementsByClassName("nav-selected-item-menu");

  if (current.length > 0) {
    current[0].className = current[0].className.replace(" selected", "");
  }

  if (currentSelectedNavItemMenu.length > 0) {
    currentSelectedNavItemMenu[0].className = currentSelectedNavItemMenu[0].className.replace(" nav-selected-item-menu", " ");
  }

  let currentItem = document.getElementById(navItemId);
  let currentMenu = currentItem.getElementsByClassName("collapse-item");
  localStorage.setItem("selectedMenu", currentMenu[0].id);
  localStorage.setItem("menuClicked", "yes");

  let elementToSelect = document.getElementById(navItemId);
  elementToSelect.className += " selected";
  elementToSelect.getElementsByClassName("nav-item-menu")[0].className += ' nav-selected-item-menu';
}

let selectNavLink = (navLinkId) => {
  let current = document.getElementsByClassName("selected-nav-link");

  if (current.length > 0) {
    current[0].className = current[0].className.replace(" selected-nav-link", "");
  }

  let elementToSelect = document.getElementById(navLinkId);
  elementToSelect.className += " selected-nav-link";
  localStorage.setItem("selectedNavLink", elementToSelect.id);
}

let init = async () => {
  setupSidebarMenu();
  let currentUrl = window.location.href;

  let current = document.getElementsByClassName("selected");
  if (currentUrl.endsWith(cardDefinitionGridUrl) && (current.length > 0 && current[0].id === "cardDefinitionGrid")) {
    selectNavItem("cardDefinitionGrid");
    selectNavLink("cardDefinitionGridNavLink");
  }
}

$(document).ajaxError(function (event, xhr) {
  if (xhr.status === 401) {
    const url = '/Auth';
    safelyNavigateTo(url);
  }
});

function navigateToSelectedNavLink(event, selectedNavLink) {
  event.preventDefault();
  let url = `/${selectedNavLink}`;
  safelyNavigateTo(url);
}

let setupSidebarMenu = () => {
  let menu = document.getElementById("menu");

  if (menu) {
    if (localStorage.getItem("menuClicked") === "yes" && localStorage.getItem("selectedMenu") !== null) {
      let selectedMenu = document.getElementById(localStorage.getItem("selectedMenu"));

      if (selectedMenu) {
        selectedMenu.className += " show";
        selectedMenu.parentElement.className += " selected";
        selectedMenu.parentElement.getElementsByClassName("nav-item-menu")[0].className += ' nav-selected-item-menu';
      }

      if (localStorage.getItem("selectedNavLink")) {
        document.getElementById(localStorage.getItem("selectedNavLink")).className += ' selected-nav-link';
      }
    }

    let navItems = menu.getElementsByClassName("nav-item");
    let navLinks = menu.getElementsByClassName("nav-link");

    for (let i = 0; i < navItems.length; i++) {
      navItems[i].addEventListener("click", function () {
          let current = document.getElementsByClassName("selected");
          let currentSelectedNavItemMenu = document.getElementsByClassName("nav-selected-item-menu");

          if (current.length > 0) {
            current[0].className = current[0].className.replace(" selected", "");
          }

          if (currentSelectedNavItemMenu.length > 0) {
            currentSelectedNavItemMenu[0].className = currentSelectedNavItemMenu[0].className.replace(" nav-selected-item-menu", " ");
          }

          let currentMenu = this.getElementsByClassName("collapse-item");
          localStorage.setItem("selectedMenu", currentMenu[0].id);
          localStorage.setItem("menuClicked", "yes");

          this.className += " selected";
          this.getElementsByClassName("nav-item-menu")[0].className += ' nav-selected-item-menu';
        
        recentClickedMenu = currentMenu[0].id;

      });
    }

    for (let i = 0; i < navLinks.length; i++) {
      navLinks[i].addEventListener("click", function () {
        if (_globalIsEditing == false) {
          let current = document.getElementsByClassName("selected-nav-link");

          if (current.length > 0) {
            current[0].className = current[0].className.replace(" selected-nav-link", "");
          }

          this.className += " selected-nav-link";
          localStorage.setItem("selectedNavLink", this.id);
        }

        recentClickedNavLink = this.id;
      });
    }
  }
}

let optionGeneratorDefault = (optionData) => {
  return `<option value="">${document.getElementById("select").value} ${optionData}</option>`;
}

let optionGenerator = (optionId, optionData) => {
  return `<option value="${optionId}">${optionId} - ${optionData}</option>`;
}

let optionGeneratorForDifferentIdAndData = (optionId, optionData) => {
  return `<option value="${optionId}">${optionData}</option>`;
}

function optionGeneratorForSingleElement(optionId) {
  return `<option value="${optionId}">${optionId}</option>`;
}

let onlyDigitKeyPress = (e) => {
  return ((e.charCode >= 48 && e.charCode <= 57));
}

let onlyDigitPlusMinusKeyPress = (e) => {
  return (((e.charCode >= 48 && e.charCode <= 57) || e.charCode === 43 || e.charCode === 45));
}

let onlyDigitAndDecimalKeyPress = (e) => {
  return (((e.charCode >= 48 && e.charCode <= 57) || e.charCode === 46));
}

let onlyDigitDecimalPlusMinusKeyPress = (e) => {
  return (((e.charCode >= 48 && e.charCode <= 57) || e.charCode === 46 || e.charCode === 43 || e.charCode === 45));
}

let onlyUpperCaseCharacterKeyPress = (e) => {
  return ((e.charCode >= 65 && e.charCode <= 90));
}

let onlyLowerCaseCharacterKeyPress = (e) => {
  return ((e.charCode >= 97 && e.charCode <= 122));
}

let showConfirmModal = (title, message) => {
  $("#confirmModalYesButtonId").off("click");
  $("#confirmModalNoButtonId").off("click");

  document.getElementById("confirmModalTitleId").innerText = title;
  document.getElementById("confirmModalMessageId").innerText = message;
  $("#confirmModalId").fadeOut("slow", function () {
    $("#confirmModalId").modal("show");
  });

  return new Promise((resolve) => {
    $("#confirmModalYesButtonId").on('click', function () {
      $("#confirmModalId").modal("hide");
      _globalIsEditing = false;
      setGlobalEditingState(_globalIsEditing);

      if (!isNullOrUndefined(recentClickedMenu))
        localStorage.setItem("selectedMenu", recentClickedMenu);

      if (!isNullOrUndefined(recentClickedNavLink))
        localStorage.setItem("selectedNavLink", recentClickedNavLink);

      $('#' + recentClickedNavLink).click();
      resolve(true);
    });
    $("#confirmModalNoButtonId").on('click', function () {
      $("#confirmModalId").modal("hide");
      resolve(false);
    });
  });
}

let showAlertModal = (title, message) => {
  $("#alertModelOkButtonId").off("click");
  document.getElementById("alertModelTitleId").innerText = title;
  document.getElementById("alertModelMessageId").innerText = message;
  $("#alertModalId").modal("show");

  return new Promise((resolve) => {
    $("#alertModelOkButtonId").on('click', function () {
      resolve(true);
      $("#alertModalId").modal("hide");
    });
  });
}

let renderCategoryDropdown = async (selectionId, data, initialOptions, categoryType) => {
  let departmentSelection = $(`#${selectionId}`);
  departmentSelection.empty();
  let optionTemplate = $('<option></option>');

  initialOptions.forEach(function (item) {
    departmentSelection.append(optionTemplate.clone().text(item.text).val(item.value));
  })

  data?.forEach(function (item) {
    let id = categoryType === "department" ? item.deptId : categoryType === "sub-department" ? item.subDeptId : item.subDetailId;
    let name = categoryType === "department" ? item.deptName : categoryType === "sub-department" ? item.subDeptName : item.subDetailName;

    departmentSelection.append(optionTemplate.clone().text(id + " - " + name).val(id));
  });
}

$('.clear-selected-input-on-keypress').on('keypress', function (e) {
  const inputElement = $(this)[0];
  if ($(this).hasClass("only-digit")) {
    if (!onlyDigitKeyPress(e)) {
      return false;
    }
  }

  if ($(this).hasClass("only-digit-decimal")) {
    if (!onlyDigitAndDecimalKeyPress(e)) {
      return false;
    }
  }

  if ($(this).hasClass("only-digit-plus-minus")) {
    if (!onlyDigitPlusMinusKeyPress(e)) {
      return false;
    }
  }

  if ($(this).hasClass("only-digit-decimal-plus-minus")) {
    if (!onlyDigitDecimalPlusMinusKeyPress(e)) {
      return false;
    }
  }

  if ($(this).hasClass("only-alpha-numerical")) {
    return onlyDigitKeyPress(e) || onlyUpperCaseCharacterKeyPress(e) || onlyLowerCaseCharacterKeyPress(e);
  }

  if ($(this).prop("type") === "number") {
    return;
  }

  if (document.getSelection().toString().length > 0) {
    const selectionStart = inputElement.selectionStart;
    const selectionEnd = inputElement.selectionEnd;
    const currentValue = inputElement.value.toString();
    const newValue = currentValue.substring(0, selectionStart) + e.key + currentValue.substring(selectionEnd);

    $(this).val(newValue);
    inputElement.setSelectionRange(selectionStart + 1, selectionStart + 1);
    e.preventDefault();
  }
});

$(".clear-selected-input-on-keypress").on('focus', function () {
  previousValue = $(this).val();
});

let convertToMax = (max) => {
  let result = ""
  max.split('').forEach(char => {
    if (!isNaN(parseInt(char))) {
      result += '9';
    }
    else {
      result += char;
    }
  });
  return result;
}

$(".clear-selected-input-on-keypress").on("input", function (e) {
  let element = $(this);
  if (element.prop("type") !== "number") {
    return;
  }

  let min = element.prop("min");
  let max = convertToMax(element.prop("max"));
  let value = element.val();

  if (value) {
    let numericValue = parseFloat(value);

    if (isNaN(numericValue) || numericValue < parseFloat(min) || numericValue > parseFloat(max)) {
      element.val(previousValue);
      value = previousValue
    }
  }
  else {
    element.val("");
    value = "";
  }
  previousValue = value;
});

const HttpStatusCode = {
  Ok: 200,
  Created: 201,
  Accepted: 202,
  NoContent: 204,
  BadRequest: 400,
  Unauthorized: 401,
  Forbidden: 403,
  NotFound: 404,
  MethodNotAllowed: 405,
  Conflict: 409,
  UnsupportedMediaType: 415,
  InternalServerError: 500,
}

let showDiscountTableNotUsedModal = () => {
  _globalIsEditing = false;
  setGlobalEditingState(_globalIsEditing);
  let title = $("#discountsNotBeingUsed").val();
  let message = $("#theDiscountTableIsNotBeingUsed").val();

  showAlertModal(title, message).then((result) => { });
}

let isAuthenticated = async () => {
  const url = '/Auth/IsAuthenticated';
  return await ajaxGetAsync(url)
    .then(response => {
      if (!response) {
        const url = '/Auth';
        safelyNavigateTo(url);
      }
      return response;
    })
    .catch(toastrError);
}