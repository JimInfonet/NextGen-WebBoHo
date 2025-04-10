let pictureFolderImagesPathData;
let yellowButtonUpName;
let yellowButtonDownName;
let yellowButtonUpData;
let yellowButtonDownData;
let groupButtonsData;
let dragedGroupButtonId;
let selectedButtonId;
let selectedButtonData;
let selectedProductCode;
let imageNameConfig;
let base64ImagePrefix = GENERAL_STRING_TABLE.URL.BASE64_IMAGE_PREFIX;

$(async function () {
  $("#changeButtonSettingButtons").removeAttr("hidden");
  $("#changeButtonDefineButton").removeAttr("hidden");
  $("#changeButtonSettingTextContainer").removeAttr("hidden");

  $("#changeButtonSettingButtons").hide();
  $("#changeButtonDefineButton").hide();
  $("#changeButtonSettingTextContainer").hide();

  await loadImageNameConfig();
  yellowButtonUpName = imageNameConfig.yellowButtonUpImageName;
  yellowButtonDownName = imageNameConfig.yellowButtonDownImageName;

  await loadGroupButtonImagesPathData();
  if (pictureFolderImagesPathData && pictureFolderImagesPathData.length > 0) {
    yellowButtonUpData = pictureFolderImagesPathData.find(obj => obj.name.toLowerCase() === yellowButtonUpName.toLowerCase());
    yellowButtonDownData = pictureFolderImagesPathData.find(obj => obj.name.toLowerCase() === yellowButtonDownName.toLowerCase());
  }
  else {
    toastr.error($("#invalidImageFolderPath").val());
  }
  await loadGroupButtonsData();
  renderGroupButtons();
  await loadStockCodesForGroupButtons();

  let defaultImageUrl = yellowButtonUpData.base64String;
  $('#defaultButton').css('background-image', `url(${base64ImagePrefix}${defaultImageUrl})`);
  $("#changeButtonSettingButtons").hide();
  $("#changeButtonDefineButton").hide();
  $("#changeButtonSettingTextContainer").hide();

  $("#buttonType-select").prop("disabled", true);
  enableAllInputFeilds(false);
});

let enableAllInputFeilds = (enable) => {
  $("#firstLine").prop("disabled", !enable);
  $("#secondLine").prop("disabled", !enable);
}

let loadImageNameConfig = async () => {
  const url = `GroupButtons/GetImageNameConfig`;

  await ajaxGetAsync(url)
    .then(_ => imageNameConfig = _)
    .catch(toastrError);
}

let loadGroupButtonImagesPathData = async () => {
  const url = `FunctionButtons/GetFunctionButtonImagesPathData`;

  await ajaxGetAsync(url)
    .then(_ => pictureFolderImagesPathData = _)
    .catch(toastrError);
}

let loadGroupButtonsData = async () => {
  let url = "GroupButtons/GetAllGroupButtonsData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        groupButtonsData = response;
      }
    })
    .catch(toastrError);
}

let renderGroupButtons = () => {
  const buttonContainer = $('#groupButtonsContainer');
  buttonContainer.empty();

  for (let i = 1; i <= 12; i++) {
    if (!yellowButtonUpData) {
      return;
    }

    let imageUrl = yellowButtonUpData.base64String;
    let buttonData = groupButtonsData.find(obj => obj.buttonId === i);
    let displayName = i;

    if (buttonData) {
      displayName = `${buttonData.description1}<br>${buttonData.description2}`
    }

    const button = $('<button>', {
      class: 'groupButton',
      id: `button-${i}`,
      draggable: 'true',
      html: displayName
    });
    
    button.css({
      'background-image': `url(${base64ImagePrefix}${imageUrl})`,
      'background-size': 'cover',
      'width': screen.width <= 1400 ? '140px' : '180px',
      'height': screen.width <= 1400 ? '140px' : '180px',
      'font-size': screen.width <= 1400 ? '13px' : '16px'
    });

    button.on('dragstart', function (event) {
      dragedGroupButtonId = event.target.id.split('-')[1];
    });

    button.on('dragover', function (event) {
      event.preventDefault();
    });

    button.on('drop', async function (event) {
      event.preventDefault();
      await buttonDragDrop(event.target.id.split('-')[1]);
    });

    button.on('click', async function () {
      $("#buttonType-select").prop("disabled", false);
      button.css({
        'background-image': `url(${base64ImagePrefix}${yellowButtonDownData.base64String})`
      });

      await renderSelectedButtonData(i);
    });

    button.on('dblclick', function () {
      button.css({
        'background-image': `url(${base64ImagePrefix}${yellowButtonUpData.base64String})`
      });
    });

    buttonContainer.append(button);
  }
}

let buttonDragDrop = async (currentId) => {
  if (currentId === dragedGroupButtonId) {
    return;
  }
  await swapGroupButtonsData(dragedGroupButtonId, currentId);
  await loadGroupButtonsData();
  renderGroupButtons();
  await renderSelectedButtonData(parseInt(currentId));
}

let swapGroupButtonsData = async (sourceId, targetId) => {
  let url = `GroupButtons/SwapGroupButtonData?sourceButtonId=${sourceId}&targetButtonId=${targetId}`;
  await ajaxPostAsync(url, null)
    .then(async (response) => {
      if (response) {
        await loadGroupButtonsData();
        renderGroupButtons();
        toastr.success($("#buttonSettingsExchangedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let selectStockClicked = () => {
  renderStockCodeDataTable(onStockCodeRowClick);
  $("#stockCodeDescriptionDataTableModal").modal('show');
}

let onStockCodeRowClick = (rowData) => {
  $("#productText").text(rowData.stockCode);
  $("#changeButtonSettingTextContainer").hide();
  $("#stockCodeDescriptionDataTableModal").modal('hide');
  enableAllInputFeilds(true);
  selectedProductCode = rowData.stockCode;
}

let loadStockCodesForGroupButtons = async () => {
  let url = "HotButtonsAndBottleReturn/GetStockCodesForHotButtons";
  await ajaxGetAsync(url)
    .then(_stockCodeData => _globalStockCodeData = _stockCodeData)
    .catch(toastrError);
}

let groupButtonsStockCodesModalCancelButtonClicked = () => {
  $("#stockCodeDescriptionDataTableModal").modal('hide');
}

let checkProductCodeExistsInPLUMastTable = async () => {
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

let addOrUpdateGroupButton = async (isAdd) => {
  let data = {
    buttonId: selectedButtonId,
    buttonType: $("#buttonType-select").val() === "" ? 1 : parseInt($("#buttonType-select").val()),
    description1: $("#firstLine").val(),
    description2: $("#secondLine").val(),
    productCode: selectedProductCode
  };

  if (!data.productCode) {
    toastr.warning($("#pleaseSelectStockItem").val());
    return;
  }

  let title = $("#noButtonDescription").val();
  let message = $("#thisButtonWillNotDisplayProductDescriptionIsThatOk").val() + '\n' + $("#isThatOk").val();

  let isAddOrUpdate = true;
  if (!data.description1 && !data.description2) {
    await showConfirmModal(title, message).then((result) => {
      isAddOrUpdate = result;
    });
  }

  if (isAddOrUpdate) {
    if (isAdd) {
      await insertGroupButton(data);
    }
    else if (groupButtonDataChanged(data)) {
      await updateGroupButton(data);
    }
  }
}

let insertGroupButton = async (data) => {
  let url = `GroupButtons/InsertGroupButton`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadGroupButtonsData();
        renderGroupButtons();
        await renderSelectedButtonData(selectedButtonId);
        toastr.success($("#buttonDefinedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let updateGroupButton = async (data) => {
  let url = `GroupButtons/UpdateGroupButton`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadGroupButtonsData();
        renderGroupButtons();
        await renderSelectedButtonData(selectedButtonId);
        toastr.success($("#buttonUpdatedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let deleteGroupButton = async () => {
  let url = `GroupButtons/DeleteGroupButton?buttonId=${selectedButtonId}`;
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        await loadGroupButtonsData();
        renderGroupButtons();
        await renderSelectedButtonData(selectedButtonId);
        toastr.success($("#buttonSuccessfullyDeleted").val());
      }
    })
    .catch(toastrError);
}

let renderSelectedButtonData = async (buttonId) => {
  $("#buttonType-select").prop("disabled", false);
  selectedButtonId = buttonId;
  selectedButtonData = groupButtonsData.find(obj => obj.buttonId === buttonId);

  $("#selectedButtonTitleData").text(`${$("#buttonText").val()} ${buttonId}`);
  if (selectedButtonData) {
    let isProductCodeExists = await checkProductCodeExistsInPLUMastTable();
    if (isProductCodeExists) {
      $("#productText").text(selectedButtonData.productCode);
      selectedProductCode = selectedButtonData.productCode;
      enableAllInputFeilds(true);

      let displayName = `${selectedButtonData.description1}<br>${selectedButtonData.description2}`
      $("#defaultButton").html(displayName);
    }
    else {
      $("#productText").text(`${$("#nonDefine").val()}`);
      enableAllInputFeilds(false);

      $("#defaultButton").html($("#stockNotFound").val());
    }

    $("#changeButtonSettingTextContainer").hide();
    $("#changeButtonSettingButtons").show();
    $("#changeButtonDefineButton").hide();


    $("#buttonType-select").val(selectedButtonData.buttonType);
    $("#firstLine").val(selectedButtonData.description1);
    $("#secondLine").val(selectedButtonData.description2);
  }
  else {
    selectedProductCode = null;
    $("#productText").text(`${$("#nonDefine").val()}`);
    $("#changeButtonSettingTextContainer").show();
    $("#defaultButton").html("");
    $("#changeButtonSettingButtons").hide();
    $("#changeButtonDefineButton").show();
    $("#defineButtonId").text(`${$("#defineButton").val()} ${buttonId}`);
    enableAllInputFeilds(true);

    $("#defaultButton").html($("#undefinedText").val());
    $("#firstLine").val("");
    $("#secondLine").val("");
  }
}

let groupButtonDataChanged = (data) => {
  let buttonData = groupButtonsData.find(obj => obj.buttonId === selectedButtonId);
  return !isObjectsEqual(buttonData, data);
}