let selectedHotButtonId;
let hotButtonsData;
let hotButtonsIdRange;
let pageNumber = 1;
let currentPageName;
let isHotButtons = ($("#pageType").val() === "True");
let bottleReturnStart = HOT_BUTTONS_AND_BOTTLE_RETURN_STRING_TABLE.BOTTLE_RETURN_START; // Hard coded in desktop application
let numberOfHotButtons = HOT_BUTTONS_AND_BOTTLE_RETURN_STRING_TABLE.NUMBER_OF_HOT_BUTTONS; // Hard coded in desktop application
let defualtColor = HOT_BUTTONS_AND_BOTTLE_RETURN_STRING_TABLE.DEFUALT_COLOR; // Hard coded in desktop application
let hotButtonsPagesData;
let selectedButtonData;
let bottlePriceLowerLimit = 0;
let bottlePriceUpperLimit = MODEL_VALIDATION_DATA_LIMIT.REAL_UPPER_LIMIT;
let selectedProductCode;
let forSaveEventHandler;

$(async function () {
  $("#changeButtonDefineButton").removeAttr("hidden");
  $("#changeButtonSettingButtons").removeAttr("hidden");

  if ($("#changeButtonSettingTextContainer")) {
    $("#changeButtonSettingTextContainer").removeAttr("hidden");
  }

  if ($("#inputProductNameReturningBottleText")) {
    $("#inputProductNameReturningBottleText").removeAttr("hidden");
  }

  resetHotButtonFormSettings();

  $("#pageIdOptions").prop('disabled', true);
  if (isHotButtons) {
    // Hard coded in desktop application
    hotButtonsIdRange = {
      startButtonId: 1,
      endButtonId: 36
    };

    await loadStockCodesForHotButtons();
  }
  else {
    // Hard coded in desktop application
    hotButtonsIdRange = {
      startButtonId: 1 + bottleReturnStart,
      endButtonId: 36 + bottleReturnStart
    };
  }

  await pageNumberChanged();
});

let resetHotButtonFormSettings = () => {
  if ($("#changeButtonSettingTextContainer")) {
    $("#changeButtonSettingTextContainer").hide();
  }
  $("#changeButtonSettingButtons").hide();
  $("#changeButtonDefineButton").hide();
  $("#changeButtonSettingUndefinedTextContainer").hide();
  $("#changeButtonSettingDescriptionContainer").hide();
  $("#inputProductNameReturningBottleText").hide();
  $("#productText").text($("#undefined").val());
  $("#selectedButtonTitleData").text($("#noButtonSelected").val());

  resetAllInputs();
  enableAllInputFeilds(false);
  $("#defaultQuantity").val("");
}

let renderPageTitle = () => {
  let pageNumberAndNameSeparator = ' - ';
  if (!isHotButtons) {
    pageNumberAndNameSeparator = " ";
  }

  let pageTitleText = `${$("#pageText").val()}${pageNumberAndNameSeparator}${pageNumber}`;
  currentPageName = `${$("#pageText").val()}  ${pageNumber}`;

  if (isHotButtons) {
    let currentPageData = hotButtonsPagesData.find(obj => obj.pageID === pageNumber);

    if (currentPageData) {
      pageTitleText = currentPageData.pageName;
      currentPageName = currentPageData.pageName;
    }
  }

  $("#pageTitleText").html(pageTitleText);
}

let loadHotButtonsByButtonIdRange = async () => {
  let url = "HotButtonsAndBottleReturn/GetHotButtonsByButtonIdRange";
  await ajaxGetAsync(url, hotButtonsIdRange)
    .then(async (response) => {
      if (response) {
        hotButtonsData = response;
        renderHotButtonsData();
      }
    })
    .catch(toastrError);
}

let loadHotButtonsPagesData = async () => {
  let url = "HotButtonsAndBottleReturn/GetAllHotButtonPages";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        hotButtonsPagesData = response;
      }
    })
    .catch(toastrError);
}

let renderHotButtons = () => {
  const buttonContainer = $('#hot-button-container');
  buttonContainer.empty();

  for (let i = 1; i <= 36; i++) {
    const button = $('<button>', {
      text: `${i}`,
      class: 'hot-button',
      id: `button-${i}`
    });

    button.on('click', async function () {
      selectedHotButtonId = parseInt($(this).attr('id').split('-')[1]);
      selectedButtonData = hotButtonsData.find(obj => obj.buttonId === getHotButtonRealIdFromCurrentButtonId(selectedHotButtonId));
      await renderButtonDataOnButtonSettingsForm();
    });

    buttonContainer.append(button);
  }
}

let checkProductCodeExistsInPLUMustTable = async () => {
  let isExists = false;
  let url = `HotButtonsAndBottleReturn/GetPLUMastByPLUCode?pluCode=${selectedButtonData.productCode}`;
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        isExists = true;
      }
    })
    .catch(toastrError);

  return isExists;
}

let checkBottleReturnExistsForProductCode = async () => {
  let isExists = false;
  let url = `HotButtonsAndBottleReturn/GetBottleReturnDataByProductCode?productCode=${selectedButtonData.productCode}`;
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        isExists = true;
      }
    })
    .catch(toastrError);

  return isExists;
}

let resetAllInputs = () => {
  $("#productCode").val("");
  $("#firstLine").val("");
  $("#secondLine").val("");
  $("#defaultQuantity").val("1");
  $("#price").val("0.00");
}

let enableAllInputFeilds = (enable) => {
  $("#productCode").prop("disabled", !enable);
  $("#firstLine").prop("disabled", !enable);
  $("#secondLine").prop("disabled", !enable);
  $("#defaultQuantity").prop("disabled", !enable);
  $("#price").prop("disabled", !enable);
}

let renderButtonDataOnButtonSettingsForm = async () => {
  if ($("#changeButtonSettingTextContainer")) {
    $("#changeButtonSettingTextContainer").show();
  }

  $("#selectedButtonTitleData").text($("#buttonOnPage").val().replace("{0}", selectedHotButtonId).replace("{1}", pageNumber));
  if (selectedButtonData) {
    let isProductCodeExists = await checkProductCodeExistsInPLUMustTable();

    if (isProductCodeExists || !isHotButtons) {
      $("#changeButtonSettingDescriptionContainer").show();
      $("#changeButtonSettingUndefinedTextContainer").hide();

      $("#changeButtonSettingButtonTextFirstLine").text(selectedButtonData.description1);
      $("#changeButtonSettingButtonTextSecondLine").text(selectedButtonData.description2);
      $("#productText").text(selectedButtonData.productCode);
    }
    else {
      $("#changeButtonSettingDescriptionContainer").hide();
      $("#changeButtonSettingUndefinedTextContainer").show();

      $("#changeButtonSettingUndefinedText").text($("#stockNotFound").val());
      $("#productText").text($("#undefined").val());
    }

    $("#changeButtonDefineButton").hide();
    $("#changeButtonSettingButtons").show();
    if ($("#inputProductNameReturningBottleText")) {
      $("#inputProductNameReturningBottleText").hide();
    }

    if ($("#selectStockItemTextContainer")) {
      $("#selectStockItemTextContainer").hide();
    }

    if ($("#showStockItemDataContainer") && isHotButtons) {
      let selectedButtonProductData = _globalStockCodeData.find(obj => obj.stockCode === selectedButtonData.productCode);
      $("#showStockItemDataContainer").show();

      if (isProductCodeExists) {
        $("#changeButtonSettingDescription").text(selectedButtonProductData.description);
      }
      else {
        $("#changeButtonSettingDescription").text($("#undefined").val());
      }
    }

    $("#productCode").val(selectedButtonData.productCode);
    $("#firstLine").val(selectedButtonData.description1);
    $("#secondLine").val(selectedButtonData.description2);
    $("#defaultQuantity").val(selectedButtonData.defaultQuantity);
    let selectedBottlePrice = parseFloat(selectedButtonData.bottlePrice);
    $("#price").val(selectedBottlePrice.toFixed(2));

    enableAllInputFeilds(false);
  }
  else {
    selectedProductCode = null;
    $("#changeButtonSettingDescriptionContainer").hide();
    $("#changeButtonSettingUndefinedTextContainer").show();

    $("#changeButtonSettingUndefinedText").text($("#undefined").val());

    if ($("#inputProductNameReturningBottleText")) {
      $("#inputProductNameReturningBottleText").show();
    }

    if ($("#selectStockItemTextContainer")) {
      $("#selectStockItemTextContainer").show();
    }

    if ($("#showStockItemDataContainer")) {
      $("#showStockItemDataContainer").hide();
    }

    $("#changeButtonDefineButton").show();
    $("#changeButtonSettingButtons").hide();
    $("#defineButtonId").text($("#defineButtonOnPage").val().replace("{0}", selectedHotButtonId).replace("{1}", pageNumber));
    $("#productText").text($("#undefined").val());

    resetAllInputs();
    enableAllInputFeilds(true);
  }
}

let changeColorButtonClicked = () => {
  let selectedButtonColor = intToHexString(selectedButtonData.colorCode);
  if (selectedButtonData.colorCode === 0) {
    selectedButtonColor = defualtColor;
  }

  $('#colorInput').val(selectedButtonColor);
  forSaveEventHandler = false;
  pickr.setColor(selectedButtonColor);
  forSaveEventHandler = true;
  $('#colorPickerModal').modal('show');
}

const pickr = Pickr.create({
  el: '.color-picker',
  theme: 'nano',
  position: 'top-middle',

  swatches: [
    'rgba(244, 67, 54, 1)',
    'rgba(233, 30, 99, 1)',
    'rgba(156, 39, 176, 1)',
    'rgba(103, 58, 183, 1)',
    'rgba(63, 81, 181, 1)',
    'rgba(33, 150, 243, 1)',
    'rgba(3, 169, 244, 1)',
    'rgba(0, 188, 212, 1)',
    'rgba(0, 150, 136, 1)',
    'rgba(76, 175, 80, 1)',
    'rgba(139, 195, 74, 1)',
    'rgba(205, 220, 57, 1)',
    'rgba(255, 235, 59, 1)',
    'rgba(255, 193, 7, 1)'
  ],

  components: {

    // Main components
    preview: true,
    opacity: false,
    hue: true,

    // Input / output Options
    interaction: {
      input: true,
      clear: true,
      save: true
    }
  },
  i18n: {
    'btn:save': $('#ok').val(),
    'btn:clear': $('#cancel').val()
  }
});

pickr.on('save', async (color, source, instance) => {
  if (!color) return;
  if (forSaveEventHandler) {
    let selectedColor = color.toHEXA().toString();
    selectedButtonData.colorCode = hexColorCodeToIntColorCode(selectedColor);
    await updateHotButtonsColor();
  }
  pickr.hide();
  $('#colorPickerModal').modal('hide');
});

pickr.on('clear', async (color, source, instance) => {
  pickr.hide();
  $('#colorPickerModal').modal('hide');
});

let checkIsBottlePriceDataInvalid = (inputValue) => {
  let realData = parseFloat(inputValue);
  return (isNaN(realData) || realData <= bottlePriceLowerLimit || realData > bottlePriceUpperLimit);
}

let selectStockClicked = async () => {
  renderStockCodeDataTable(onStockCodeRowClick);
  $("#stockCodeDescriptionDataTableModal").modal('show');
}

let defineHotButton = async () => {
  selectedButtonData = {
    buttonId: getHotButtonRealIdFromCurrentButtonId(selectedHotButtonId),
    productCode: isHotButtons ? selectedProductCode : $("#productCode").val(),
    description1: $("#firstLine").val(),
    description2: $("#secondLine").val(),
    defaultQuantity: $("#defaultQuantity").val(),
    colorCode: 0,
    bottlePrice: isHotButtons ? 0 : $("#price").val()
  }

  if (!selectedButtonData.productCode) {
    if (isHotButtons) {
      toastr.warning($("#pleaseSelectStockItem").val());
    }
    else {
      toastr.warning($("#pleaseEnterProductName").val());
    }

    selectedButtonData = null;
    return;
  }

  if (!isHotButtons) {
    let isBottleReturnExists = await checkBottleReturnExistsForProductCode();
    if (isBottleReturnExists) {
      toastr.warning($("#productAlreadyExistsInTheSystem").val().replace("{0}", selectedButtonData.productCode));
      selectedButtonData = null;
      return;
    }
  }

  if (!selectedButtonData.defaultQuantity || isNaN(parseFloat(selectedButtonData.defaultQuantity))) {
    toastr.warning($("#pleaseEnterAnIntegerValueForQuantity").val());
    selectedButtonData = null;
    return;
  }

  if (!isHotButtons) {
    if (checkIsBottlePriceDataInvalid(selectedButtonData.bottlePrice)) {
      toastr.warning($("#pleaseEnterValidBottlePrice").val());
      selectedButtonData = null;
      return;
    }
  }

  let title = $("#noButtonDescription").val();
  let message = $("#thisButtonWillNotDisplayProductDescriptionIsThatOk").val() + '\n' + $("#isThatOk").val();

  let isAddHotButton = true;
  if (!selectedButtonData.description1 && !selectedButtonData.description2) {
    await showConfirmModal(title, message).then((result) => {
      isAddHotButton = result;
    });
  }

  if (isAddHotButton) {
    await insertHotButtons();
  }
  else {
    selectedButtonData = null;
  }
}

let undefineButtonClicked = async () => {
  await deleteHotButton();
}

let colorPickerOkButtonClicked = async () => {
  let selectedColor = $('#colorInput').val();
  selectedButtonData.colorCode = hexColorCodeToIntColorCode(selectedColor);
  await updateHotButtonsColor();
  $('#colorPickerModal').modal('hide');
}

let insertHotButtons = async () => {
  let url = `HotButtonsAndBottleReturn/InsertHotButtons`;
  await ajaxPostAsync(url, selectedButtonData)
    .then(async (response) => {
      renderHotButtons();
      await loadHotButtonsByButtonIdRange();
      await renderButtonDataOnButtonSettingsForm();
      toastr.success($("#buttonDefinedSuccessfully").val());
    })
    .catch(toastrError);
}

let updateHotButtonsColor = async () => {
  let url = `HotButtonsAndBottleReturn/UpdateHotButtonsColor`;
  await ajaxPostAsync(url, selectedButtonData)
    .then(async (response) => {
      renderHotButtons();
      await loadHotButtonsByButtonIdRange();
      await renderButtonDataOnButtonSettingsForm();
      toastr.success($("#buttonColorUpdatedSuccessfully").val());
    })
    .catch(toastrError);
}

let deleteHotButton = async () => {
  let url = `HotButtonsAndBottleReturn/DeleteHotButton`;
  await ajaxPostAsync(url, selectedButtonData)
    .then(async (response) => {
      selectedButtonData = null;
      renderHotButtons();
      await loadHotButtonsByButtonIdRange();
      await renderButtonDataOnButtonSettingsForm();
      toastr.success($("#buttonSuccessfullyDeleted").val());
    })
    .catch(toastrError);
}

let pageNumberChanged = async () => {
  renderHotButtons();
  await loadHotButtonsByButtonIdRange();
  await loadHotButtonsPagesData();
  renderPageTitle();

  $('#pageControlUpButton').prop('disabled', (pageNumber === 10));
  $('#pageControlDownButton').prop('disabled', (pageNumber === 1));
}

let renderPageChangeFormData = () => {
  $("#pageIdOptions").empty();
  $("#pageIdOptions").append(optionGeneratorForSingleElement(pageNumber));
  $("#pageName").val(currentPageName);
}

let changePageCompleteButtonClicked = async () => {
  let data = {
    pageID: $("#pageIdOptions").val(),
    pageName: $("#pageName").val()
  }

  let url = "HotButtonsAndBottleReturn/UpdateHotButtonPage";
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await pageNumberChanged();
        toastr.success($("#pageNameUpdatedSuccessfully").val());
        $("#hotButtonsPagesModal").modal('hide');
      }
    })
    .catch(toastrError);
}

let changePageButtonClicked = () => {
  renderPageChangeFormData();
  $("#hotButtonsPagesModal").modal('show');
}

let pageControlUpButtonClicked = async () => {
  pageNumber++;
  if (pageNumber > 10) {
    pageNumber = 10;
  }
  hotButtonsIdRange.startButtonId += 36;
  hotButtonsIdRange.endButtonId += 36;

  await pageNumberChanged();
  resetHotButtonFormSettings();
}

let pageControlDownButtonClicked = async () => {
  pageNumber--;
  if (pageNumber < 1) {
    pageNumber = 1;
  }
  hotButtonsIdRange.startButtonId -= 36;
  hotButtonsIdRange.endButtonId -= 36;

  await pageNumberChanged();
  resetHotButtonFormSettings();
}

function intToRGBString(colorCode) {
  let blue = (colorCode >> 16) & 0xFF;
  let green = (colorCode >> 8) & 0xFF;
  let red = colorCode & 0xFF;
  return 'rgb(' + red + ', ' + green + ', ' + blue + ')';
}

let hexColorCodeToIntColorCode = (hexColorCode) => {
  let red = parseInt(hexColorCode.substring(1, 3), 16);
  let green = parseInt(hexColorCode.substring(3, 5), 16);
  let blue = parseInt(hexColorCode.substring(5, 7), 16);

  red = parseInt(red, 10);
  green = parseInt(green, 10);
  blue = parseInt(blue, 10);

  let colorCode = (blue << 16) | (green << 8) | red;

  return colorCode;
}


let intToHexString = (colorCode) => {
  let blue = (colorCode >> 16) & 0xFF;
  let green = (colorCode >> 8) & 0xFF;
  let red = colorCode & 0xFF;

  blue = blue.toString(16).padStart(2, '0');
  green = green.toString(16).padStart(2, '0');
  red = red.toString(16).padStart(2, '0');

  const hexColorCode = `#${red}${green}${blue}`;
  return hexColorCode.toUpperCase();
}

let getButtonIdFromHotButtonData = (id) => {
  let buttonId = id - ((pageNumber - 1) * numberOfHotButtons);
  if (!isHotButtons) {
    buttonId -= bottleReturnStart;
  }

  return buttonId;
}

let getHotButtonRealIdFromCurrentButtonId = (id) => {
  let buttonId = id + ((pageNumber - 1) * numberOfHotButtons);
  if (!isHotButtons) {
    buttonId += bottleReturnStart;
  }

  return buttonId;
}

let renderHotButtonsData = () => {
  hotButtonsData.forEach((hotButton) => {
    let buttonId = getButtonIdFromHotButtonData(hotButton.buttonId);
    if (hotButton.colorCode !== 0) {
      $(`#button-${buttonId}`).css("background-color", intToRGBString(hotButton.colorCode));
    }
    else {
      $(`#button-${buttonId}`).css("background-color", defualtColor);
    }
  });
}

let loadStockCodesForHotButtons = async () => {
  let url = "HotButtonsAndBottleReturn/GetStockCodesForHotButtons";
  await ajaxGetAsync(url)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);
}

let onStockCodeRowClick = (rowData) => {
  $("#stockCodeDescriptionDataTableModal").modal('hide');
  $("#productText").text(rowData.stockCode);
  $("#changeButtonSettingDescription").text(rowData.description);

  $("#selectStockItemTextContainer").hide();
  $("#showStockItemDataContainer").show();
  selectedProductCode = rowData.stockCode;
}

let hotButtonsStockCodesModalCancelButtonClicked = () => {
  $("#stockCodeDescriptionDataTableModal").modal('hide');
}

$('#price').on('blur', function () {
  let inputValue = $(this).val();
  if (!isNaN(inputValue)) {
    $(this).val(parseFloat(inputValue).toFixed(2));
  }
});

$('#defaultQuantity').on('blur', function () {
  let inputValue = $(this).val();
  if (!isNaN(inputValue)) {
    $(this).val(Math.round(inputValue));
  }
});