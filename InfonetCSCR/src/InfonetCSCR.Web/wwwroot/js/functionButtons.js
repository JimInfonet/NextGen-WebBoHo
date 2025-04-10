let dragedFunctionButtonId;
let functionButtonsData;
let functionListData;
let functionButtonImagesPathData;
let defaultImageData;
let defaultImageName;
let isButtonClicked = new Array(17).fill(false);
let base64ImagePrefix = GENERAL_STRING_TABLE.URL.BASE64_IMAGE_PREFIX;
let selectedButtonType;
let selectedFunctionId;
let functionListDataForSelectedButtonType;
let selectedButtonData;
let selectedButtonId;
let selectedPictureId;
let imageNameConfig;

$(async function () {
  $("#functionButtonSettingButtons").removeAttr("hidden");
  $("#functionButtonDefineButton").removeAttr("hidden");
  $("#imageButton").removeAttr("hidden");

  $("#functionButtonSettingButtons").hide();
  $("#functionButtonDefineButton").hide();
  $("#imageButton").hide();

  await loadImageNameConfig();
  defaultImageName = imageNameConfig.greenButtonUpImageName;

  await loadFunctionButtonImagesPathData();
  if (functionButtonImagesPathData && functionButtonImagesPathData.length > 0) {
    defaultImageData = functionButtonImagesPathData.find(obj => obj.name.toLowerCase() === defaultImageName.toLowerCase());
  }
  else {
    toastr.error($("#invalidImageFolderPath").val());
  }
  await loadFunctionButtonsData();
  await loadFunctionListData();
  renderFunctionButtons();
  await renderSelectedButtonData(1);
});

let clearChangeButtonSettingsForm = () => {
  $('#defineButtonId').text(`${$('#defineButton').val()}`);
  $("#functionName").text($('#undefinedText').val());
  $("#buttonId").text($("#noButtonSelected").val());
  $('#defaultButton').text("");
  $('#imageButton').hide();

  $('#functionButtonSettingButtons').hide();
  $('#functionButtonSettingsWarning').show();
  $('#functionButtonDefineButton').show();

  $("#selectFunction-select").val("");
  selectedPictureId = null;
  renderStyleForSelectedButton(selectedPictureId);
}

let loadImageNameConfig = async () => {
  const url = `GroupButtons/GetImageNameConfig`;

  await ajaxGetAsync(url)
    .then(_ => imageNameConfig = _)
    .catch(toastrError);
}

let loadFunctionButtonImagesPathData = async () => {
  const url = `FunctionButtons/GetFunctionButtonImagesPathData`;

  await ajaxGetAsync(url)
    .then(_ => functionButtonImagesPathData = _)
    .catch(toastrError);
}

let loadFunctionButtonsData = async () => {
  let url = "FunctionButtons/GetAllFunctionButtonsData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        functionButtonsData = response;
      }
    })
    .catch(toastrError);
}

let loadFunctionListData = async () => {
  let url = "FunctionButtons/GetAllFunctionListData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        functionListData = response;
      }
    })
    .catch(toastrError);
}

let renderFunctionButtons = () => {
  const buttonContainer = $('#functionButtonsContainer');
  buttonContainer.empty();

  for (let i = 1; i <= 16; i++) {
    if (!defaultImageData) {
      return;
    }

    let imageUrl = defaultImageData.base64String;
    let buttonData = functionButtonsData.find(obj => obj.buttonID === i);
    let displayName = i;

    if (buttonData && buttonData.pictureID == 1) {
      let functionButtonListData = functionListData.find(obj => (obj.buttonType === buttonData.buttonType && obj.functionID === buttonData.functionID));
      displayName = functionButtonListData.displayName
    }
    else if (buttonData) {
      let functionButtonListData = functionListData.find(obj => (obj.buttonType === buttonData.buttonType && obj.functionID === buttonData.functionID));
      let imageData = functionButtonImagesPathData.find(obj => obj.name.toLowerCase() === functionButtonListData.pictureName.toLowerCase());
      imageUrl = imageData.base64String;

      if (!isButtonClicked[i]) {
        displayName = "";
      }
    }

    const button = $('<button>', {
      class: 'functionButton',
      id: `button-${i}`,
      draggable: 'true',
      text: displayName
    });

    button.css({
      'background-image': `url(${base64ImagePrefix}${imageUrl})`,
      'background-size': 'cover',
      'width': '90px',
      'height': '90px'
    });

    button.on('dragstart', function (event) {
      dragedFunctionButtonId = event.target.id.split('-')[1];
    });

    button.on('dragover', function (event) {
      event.preventDefault();
    });

    button.on('drop', async function (event) {
      event.preventDefault();
      await buttonDragDrop(event.target.id.split('-')[1]);
    });

    button.on('click', async function () {
      if ($(this).text().trim() === "") {
        $(this).text(i);
        isButtonClicked[i] = true;
      }
      await renderSelectedButtonData(i);
    });

    buttonContainer.append(button);
  }
}

let buttonDragDrop = async (currentId) => {
  if (currentId === dragedFunctionButtonId) {
    return;
  }

  await swapFunctionButtonsData(currentId, dragedFunctionButtonId);
  await renderSelectedButtonData(parseInt(currentId));
}

let swapFunctionButtonsData = async (sourceId, targetId) => {
  let url = `FunctionButtons/SwapFunctionButtonData?sourceButtonId=${sourceId}&targetButtonId=${targetId}`;
  await ajaxPostAsync(url, null)
    .then(async (response) => {
      if (response) {
        await loadFunctionButtonsData();
        renderFunctionButtons();
        toastr.success($("#buttonSettingsExchangedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let loadFunctionListDataForButtonType = async () => {
  let url = `FunctionButtons/GetFunctionListByButtonType?buttonType=${selectedButtonType}`;
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        functionListDataForSelectedButtonType = response;
      }
    })
    .catch(toastrError);
}

let buttonTypeOptionChanged = async (selectedOption) => {
  selectedFunctionId = null;
  selectedButtonType = parseInt(selectedOption);
  await loadFunctionListDataForButtonType();
  renderFunctionListOptions();
}

let renderFunctionListOptions = () => {
  $("#selectFunction-select").html(optionGeneratorDefault($("#function").val()));
  functionListDataForSelectedButtonType.forEach(functionList => {
    $("#selectFunction-select").append(optionGenerator(functionList.functionID, functionList.functionName));
  });
}

let renderSelectedButtonDataInForm = () => {
  let functionButtonListData = functionListData.find(obj => (obj.buttonType === selectedButtonType && obj.functionID === selectedFunctionId));

  if (!functionButtonListData) {
    return;
  }

  $("#functionName").text(functionButtonListData.functionName);

  let imageData = functionButtonImagesPathData.find(obj => obj.name.toLowerCase() === functionButtonListData.pictureName.toLowerCase());

  $('#defaultButton').text(functionButtonListData.displayName);
  if (imageData) {
    let imageUrl = imageData.base64String;
    $('#imageButton').css('background-image', `url(${base64ImagePrefix}${imageUrl})`);
    $('#imageButton').show();
  }
  else {
    $('#imageButton').hide();
  }
}

let renderSelectedButtonData = async (buttonId) => {
  selectedButtonId = buttonId;
  selectedButtonData = functionButtonsData.find(obj => obj.buttonID === buttonId);
  let defaultImageUrl = defaultImageData.base64String;
  $('#defaultButton').css('background-image', `url(${base64ImagePrefix}${defaultImageUrl})`);
  $("#buttonId").text(`${$("#buttonText").val()} ${buttonId}`);

  if (selectedButtonData) {
    selectedButtonType = selectedButtonData.buttonType;
    renderSelectedButtonDataInForm();
    $('#functionButtonSettingButtons').show();
    $('#functionButtonSettingsWarning').hide();
    $('#functionButtonDefineButton').hide();

    $("#buttonType-select").val(selectedButtonType)
    await buttonTypeOptionChanged(selectedButtonType);

    selectedFunctionId = selectedButtonData.functionID;
    $("#selectFunction-select").val(selectedFunctionId);
    functionOptionChanged(selectedFunctionId);
    selectedPictureId = selectedButtonData.pictureID;
    renderStyleForSelectedButton(selectedPictureId);
  }
  else {
    $('#defaultButton').text($('#undefinedText').val());
    $('#defineButtonId').text(`${$('#defineButton').val()} ${buttonId}`);
    $("#functionName").text($('#undefinedText').val());

    $('#imageButton').hide();
    $('#functionButtonSettingButtons').hide();
    $('#functionButtonSettingsWarning').show();
    $('#functionButtonDefineButton').show();

    $("#buttonType-select").val("");
    await buttonTypeOptionChanged("");

    $("#selectFunction-select").val("");
    functionOptionChanged("");
    selectedPictureId = null;
    renderStyleForSelectedButton(selectedPictureId);
  }
}

let addOrUpdateFunctionButton = async (isAdd) => {
  let data = {
    buttonID: selectedButtonId,
    functionID: selectedFunctionId,
    pictureID: selectedPictureId,
    buttonType: selectedButtonType
  };

  if (!selectedButtonId) {
    toastr.warning($("#pleaseSelectButtonFirst").val());
    return;
  }

  if (!data.buttonType) {
    toastr.warning($("#pleaseSelectButtonTypeFirst").val());
    return;
  }

  if (!data.functionID) {
    toastr.warning($("#pleaseSelectFunctionFirst").val());
    return;
  }

  if (!data.pictureID) {
    data.pictureID = 1;
  }

  if (isAdd) {
    await insertFunctionButton(data);
  }
  else {
    if (checkFunctionButtonDataChanged(data)) {
      await updateFunctionButton(data);
    }
  }
}

let insertFunctionButton = async (data) => {
  let url = `FunctionButtons/InsertFunctionButton`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadFunctionButtonsData();
        renderFunctionButtons();
        await renderSelectedButtonData(selectedButtonId);
        toastr.success($("#buttonDefinedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let updateFunctionButton = async (data) => {
  let url = `FunctionButtons/UpdateFunctionButton`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadFunctionButtonsData();
        renderFunctionButtons();
        await renderSelectedButtonData(selectedButtonId);
        toastr.success($("#buttonUpdatedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let deleteFunctionButton = async () => {
  let url = `FunctionButtons/DeleteFunctionButton?buttonId=${selectedButtonId}`;
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        selectedButtonId = null;
        await loadFunctionButtonsData();
        renderFunctionButtons();
        clearChangeButtonSettingsForm();
        toastr.success($("#buttonSuccessfullyDeleted").val());
      }
    })
    .catch(toastrError);
}

let undefineButtonClicked = async () => {
  await deleteFunctionButton();
}

let functionOptionChanged = (selectedOption) => {
  selectedPictureId = null;
  renderStyleForSelectedButton(selectedPictureId);
  selectedFunctionId = parseInt(selectedOption);
  renderSelectedButtonDataInForm();
}

let pictureIdOneCliecked = () => {
  selectedPictureId = 1;
  renderStyleForSelectedButton(1);
}

let pictureIdTwoCliecked = () => {
  selectedPictureId = 2;
  renderStyleForSelectedButton(2);
}

let renderStyleForSelectedButton = (pictureId) => {
  $("#defaultButton").removeClass("selectedButton");
  $("#imageButton").removeClass("selectedButton");
  if (pictureId === 1) {
    $("#defaultButton").addClass("selectedButton");
  }
  else if (pictureId === 2) {
    $("#imageButton").addClass("selectedButton");
  }
}

let checkFunctionButtonDataChanged = (data) => {
  let buttonData = functionButtonsData.find(obj => obj.buttonID === selectedButtonId);
  return !isObjectsEqual(buttonData, data);
}