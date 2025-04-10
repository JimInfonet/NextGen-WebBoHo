const STRING_TABLE = CARD_RESTRICTION_PROFILE_STRING_TABLE;
let purchaseRestrictionDatatable;
let purchaseRestrictionData;
let promptTableData = [];
let promptDatatable;
let profileId;
let placeHolderForTime = "--:--";
let defaultDateOfTimeRestriction = "1899-12-30";
let action;
let availablePrompts;
let selectedPromptId;
let selectedSequence;
let create = CONSTANT_TEXTS.CREATE;
let update = CONSTANT_TEXTS.UPDATE;
let viewMode = CONSTANT_TEXTS.VIEW;
let copy = CONSTANT_TEXTS.COPY;
let isEditMode = true;

let sunday = document.getElementById("sunday").value;
let monday = document.getElementById("monday").value;
let tuesday = document.getElementById("tuesday").value;
let wednesday = document.getElementById("wednesday").value;
let thursday = document.getElementById("thursday").value;
let friday = document.getElementById("friday").value;
let saturday = document.getElementById("saturday").value;

let daysOfWeek = [sunday, monday, tuesday, wednesday, thursday, friday, saturday];

let defaultPurchaseRestrictionData = [];

const dayOfWeekDictionary = {
  1: sunday,
  2: monday,
  3: tuesday,
  4: wednesday,
  5: thursday,
  6: friday,
  7: saturday,
};

for (let i = 1; i <= daysOfWeek.length; i++) {
  const dayObject = {
    dayOfWeek: i,
    allowPurchase: true,
    timeRestriction: false,
    startTime: null,
    endTime: null,
  };

  defaultPurchaseRestrictionData.push(dayObject);
}

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  profileId = urlParams.get('profileId');
  action = urlParams.get('ActionMode');
  let isShowToaster = urlParams.get('ShowToaster');

  document.getElementById("prompt-form").style.display = "none";

  if (action === create || action === update) {
    setEditMode(true);
  }
  else if (action === viewMode) {
    setEditMode(false);
  }

  if (action === create) {
    $('#btn-edit').hide();
    $('#btn-cancel').hide();
  }

  if (isShowToaster) {
    toastr.success(isShowToaster === "create" ? $("#cardRestrictionProfileCreatedSuccessfully").val() : $("#cardRestrictionProfileUpdatedSuccessfully").val());
    let newUrl = removeURLParameter(window.location.search, 'ShowToaster');
    window.history.replaceState(null, null, newUrl);
  }

  purchaseRestrictionData = defaultPurchaseRestrictionData;

  if (!isNullUndefinedOrWhiteSpace(profileId)) {
    $('.not-editable').prop('readOnly', true);
    await loadRestrictionTableData();
  }

  setEditMode(true);

  if (action === viewMode) {
    setEditMode(false);
  }
  else if (action === copy) {
    $('.not-editable').prop('readOnly', false);
    document.getElementById("profile-id").value = "";
    document.getElementById("profile-name").value = "";
  }

  createTimeOfPurchaseRestrictionTable();
  await loadPromptTable();
});

let togglePromptButtons = () => {
  let promptForFuelCheckbox = document.getElementById("PromptForFuelCheckbox").checked;

  if (action !== create && action !== copy && promptForFuelCheckbox && isEditMode) {
    $('#prompt-btn-add').prop('disabled', false);
    $('#prompt-btn-delete').prop('disabled', promptTableData.length === 0);
  }
  else {
    $('#prompt-btn-add').prop('disabled', true);
    $('#prompt-btn-delete').prop('disabled', true);
  }
}

let setEditMode = (editMode) => {
  _globalIsEditing = isEditMode = editMode;
  setGlobalEditingState(_globalIsEditing);
  $('.edit-mode-control').prop('disabled', !editMode);
  togglePromptButtons();

  if (!isEditMode) {
    $('#btn-cancel').hide();
    $('#btn-save').hide();
    $('#btn-edit').show();
  }
  else {
    if (action !== create)
      $('#btn-cancel').show();

    $('#btn-save').show();
    $('#btn-edit').hide();
  }
}

let loadRestrictionTableData = async () => {
  let url = `${STRING_TABLE.URL.GET_TIME_OF_PURCHASE_RESTRICTION}?profileId=${encodeURIComponent(profileId)}`;

  await ajaxGetAsync(url)
    .then(_purchaseRestrictionData => {
      purchaseRestrictionData = _purchaseRestrictionData;
    })
    .catch(toastrError);
}

let loadPromptTable = async () => {
  let url = `${STRING_TABLE.URL.GET_ALL_PROFILE_PROMPTS}?profileId=${encodeURIComponent(profileId)}`;

  await ajaxGetAsync(url)
    .then(_promptTableData => {
      promptTableData = _promptTableData;
      createPromptTable();
    })
    .catch(toastrError);
}

let createTimeOfPurchaseRestrictionTable = () => {
  destroyDataTableIfExists(purchaseRestrictionDatatable)

  let props = createDataTableProp();
  props.data = purchaseRestrictionData;
  props.scrollY = '440px';
  props.paging = false;
  props.bInfo = false;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4] }
  ];

  props.columns = [
    {
      "data": "dayOfWeek",
      render: function (dayOfWeek) {
        return dayOfWeekDictionary[dayOfWeek];
      }
    },
    {
      "data": STRING_TABLE.TABLE.COLUMN_NAME.ALLOW_PURCHASE,
      render: function (allowPurchase) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${allowPurchase ? $("#yes").val() : $("#no").val() }</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleCellClick(td, cellData, rowData, STRING_TABLE.TABLE.COLUMN_NAME.ALLOW_PURCHASE);
      }
    },
    {
      "data": STRING_TABLE.TABLE.COLUMN_NAME.TIME_RESTRICTION,
      render: function (timeRestriction) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${timeRestriction ? $("#yes").val() : $("#no").val()}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleCellClick(td, cellData, rowData, STRING_TABLE.TABLE.COLUMN_NAME.TIME_RESTRICTION);
      }
    },
    {
      "data": STRING_TABLE.TABLE.COLUMN_NAME.START_TIME,
      render: function (startTime) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${startTime == null ? placeHolderForTime : startTime}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleCellClick(td, cellData, rowData, STRING_TABLE.TABLE.COLUMN_NAME.START_TIME);
      }
    },
    {
      "data": STRING_TABLE.TABLE.COLUMN_NAME.END_TIME,
      render: function (endTime) {
        return `
          <div class="cell-container">
            <span class="text-span" style="display: block; width: 100%;">${endTime == null ? placeHolderForTime : endTime}</span>
          </div>
        `;
      },
      createdCell: function (td, cellData, rowData, row, col) {
        handleCellClick(td, cellData, rowData, STRING_TABLE.TABLE.COLUMN_NAME.END_TIME);
      }
    },
  ];

  purchaseRestrictionDatatable = createDataTableGeneric('time-of-purchase-restriction-table', props);
}

let createPromptTable = () => {
  destroyDataTableIfExists(promptDatatable)

  let props = createDataTableProp();
  props.data = promptTableData;
  props.scrollY = '440px';
  props.paging = false;
  props.bInfo = false;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1] }
  ];

  props.columns = [
    { "data": "promptSeq", },
    { "data": "promptMessage", },
  ];

  promptDatatable = createDataTableGeneric('prompt-table', props);
  $('#prompt-btn-delete').prop('disabled', true);

  if (promptTableData.length > 0) {
    $(`#prompt-table tbody tr:eq(0)`).addClass('selected-grid-row');
    togglePromptButtons();

    if (promptTableData.length > 0) {
      selectedSequence = promptTableData[0].promptSeq;
    }

    promptTableData.forEach(promptData => {
      if (promptData.promptSeq == selectedSequence) {
        selectedPromptId = promptData.promptID;
      }
    });
  }

  $('#prompt-table tbody')
    .off()
    .on('click', 'tr', function () {
      if (promptTableData.length === 0 || !isEditMode) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        promptDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedSequence = $(this).find("td:first")[0].innerText;

        promptTableData.forEach(promptData => {
          if (promptData.promptSeq == selectedSequence) {
            selectedPromptId = promptData.promptID;
          }
        });
      }
    })
}

function handleCellClick(td, cellData, rowData, cellType) {
  const cellContainer = $(td).find('.cell-container');
  const textSpan = $(td).find('.text-span');

  textSpan.on('click', function () {
    let isLimitDay = document.getElementById("LimitTimeofPurchaseCheckbox").checked;
    if (!isLimitDay) return;

    if ($('.edit-input').length > 0) return; // An element with class ".edit-input" already exists

    let shiftFormDto = { ...rowData, cellType };
    if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.START_TIME
      || cellType === STRING_TABLE.TABLE.COLUMN_NAME.END_TIME)
      if (purchaseRestrictionData[shiftFormDto.dayOfWeek - 1].allowPurchase === false
        || purchaseRestrictionData[shiftFormDto.dayOfWeek - 1].timeRestriction === false) {
        return;
      }

    textSpan.hide();
    let editInput = prepareInputField(cellType, cellData, cellContainer);
    cellContainer.append(editInput);
    editInput.focus().select();

    if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.ALLOW_PURCHASE
      || cellType === STRING_TABLE.TABLE.COLUMN_NAME.TIME_RESTRICTION) {
      editInput.click();
    }

    editInput.on('blur', function () {
      textSpan.show();
      editInput.remove();
    });

    if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.ALLOW_PURCHASE
      || cellType === STRING_TABLE.TABLE.COLUMN_NAME.TIME_RESTRICTION) {
      editInput.on('change keyup', async function (event) {
        let newValue = editInput.val();
        if (handleSavingShiftForm(td, rowData, cellType, newValue)) {
          cellData = newValue == "true"; // INFO: val function returns a string
          textSpan.text(cellData === true ? $("#yes").val() : $("#no").val()).show();
          editInput.remove();
        }
      });
    }
    else {
      editInput.on('focusout keypress', async function (e) {
        if (e.type === "keypress" && e.key !== "Enter") return;

        let newValue = editInput.val();
        cellData = newValue;
        editInput.remove();

        if (newValue.length === 0)
          newValue = "12:00 AM";

        newValue = convert24to12Hour(newValue);

        if (handleSavingShiftForm(td, rowData, cellType, newValue)) {
          textSpan.html(newValue).show();
        }
      });
    }
  });
}

let prepareInputField = (cellType, cellData, cellContainer) => {
  let editInput;
  let elementType;
  if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.ALLOW_PURCHASE
    || cellType === STRING_TABLE.TABLE.COLUMN_NAME.TIME_RESTRICTION) {
    elementType = "select";

    editInput = $('<select>', { class: 'edit-input form-select-sm' });
    const optionYes = $('<option>', { value: true, text: $("#yes").val(), selected: cellData ? true : false });
    const optionNo = $('<option>', { value: false, text: $("#no").val(), selected: !cellData ? true : false });
    editInput.append(optionYes, optionNo);
  }
  else {
    elementType = "time";
    editInput = $('<input>', { type: elementType, class: 'edit-input form-control-sm', value: cellData, maxLength: 30 });
  }
  return editInput;
}

let handleSavingShiftForm = (td, rowData, cellType, newValue) => {
  let shiftFormDto = { ...rowData, cellType };
  shiftFormDto = purchaseRestrictionData[shiftFormDto.dayOfWeek - 1]

  if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.START_TIME) {
    shiftFormDto.startTime = newValue;
  }
  else if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.END_TIME) {
    shiftFormDto.endTime = newValue;
  }
  else if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.ALLOW_PURCHASE) {
    newValue = newValue == "true" ? true : false
    shiftFormDto.allowPurchase = newValue;
  }
  else if (cellType === STRING_TABLE.TABLE.COLUMN_NAME.TIME_RESTRICTION) {
    newValue = newValue == "true" ? true : false
    shiftFormDto.timeRestriction = newValue;
  }

  purchaseRestrictionData[shiftFormDto.dayOfWeek - 1] = shiftFormDto;
  return true;
}

let goToCardRestrictionGridPage = async () => {
  const url = STRING_TABLE.URL.CARD_RESTRICTION_PROFILE_GRID;
  await safelyNavigateTo(url);
}

let saveCardRestrictionProfile = async () => {
  let profileIdValue = document.getElementById("profile-id").value;
  let profileNameValue = document.getElementById("profile-name").value;
  let singleTransactionValue = document.getElementById("single-transaction").value;
  let dailyTransactionValue = document.getElementById("daily-transaction").value;
  let monthlyTransactionValue = document.getElementById("monthly-transaction").value;
  let transactionPerDayValue = document.getElementById("transaction-per-day").value;
  
  if (isNullUndefinedOrWhiteSpace(profileIdValue) || isNullUndefinedOrWhiteSpace(profileNameValue))
    return false;

  if (singleTransactionValue > MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LIMIT || singleTransactionValue < MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LOWER_LIMIT)
    return false;

  if (dailyTransactionValue > MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LIMIT || dailyTransactionValue < MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LOWER_LIMIT)
    return false;

  if (monthlyTransactionValue > MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LIMIT || monthlyTransactionValue < MODEL_VALIDATION_DATA_LIMIT.SMALL_DOUBLE_LOWER_LIMIT)
    return false;

  if (transactionPerDayValue > MODEL_VALIDATION_DATA_LIMIT.SMALL_INTEGER_LIMIT || transactionPerDayValue < MODEL_VALIDATION_DATA_LIMIT.SMALL_INTEGER_LOWER_LIMIT)
    return false;

  let limitTimeofPurchaseCheckbox = document.getElementById("LimitTimeofPurchaseCheckbox").value;

  if (action === copy) {
    await copyCardProfilePrompts();
  }

  //we can ignore the time restriction table when it is not checked, unless we are creating the profile for first time
  if (!limitTimeofPurchaseCheckbox && action !== create && action !== copy)
    return true;

  let newProfileID = document.getElementById("profile-id").value;
  await purchaseRestrictionData.forEach(purchaseRestriction => {
    if (purchaseRestriction.allowPurchase === true && purchaseRestriction.timeRestriction === true) {
      if (!purchaseRestriction.startTime)
        purchaseRestriction.startTime = "12:00 AM";
      if (!purchaseRestriction.endTime)
        purchaseRestriction.endTime = "11:59 PM";

      purchaseRestriction.startTime = `${defaultDateOfTimeRestriction} ${purchaseRestriction.startTime}`
      purchaseRestriction.endTime = `${defaultDateOfTimeRestriction} ${purchaseRestriction.endTime}`
    }
    else {
      purchaseRestriction.startTime = null;
      purchaseRestriction.endTime = null;
    }

    purchaseRestriction.profileID = newProfileID;
    const url = STRING_TABLE.URL.SAVE_TIME_OF_PURCHASE_RESTRICTION;
    ajaxPostAsync(url, purchaseRestriction)
      .catch(toastrError);
  });

  return true;
}

let copyCardProfilePrompts = async () => {
  let newProfileId = document.getElementById("profile-id").value;
  let url = STRING_TABLE.URL.ADD_PROFILE_PROMPT;
  promptTableData.forEach(async promptData => {
    promptData.profileID = newProfileId;
    await ajaxPostAsync(url, promptData)
      .catch(toastrError);
  });
}

let loadPromptMessages = async () => {
  let url = `${STRING_TABLE.URL.GET_PROMPT_MESSAGE}?profileId=${encodeURIComponent(profileId)}`;

  $("#message-select").empty();
  await ajaxGetAsync(url)
    .then((_prompts) => {
      availablePrompts = _prompts;
      _prompts.forEach(option => $("#message-select").append(optionGenerator(option.promptID, option.promptMessage)));
    });
}

let promptAddClick = async () => {

  await loadPromptMessages();
  if (availablePrompts.length === 0) {
    toastr.warning($("#noMorePromptsCanBeAdded").val());
  }
  else {
    document.getElementById("prompt-form").style.display = "block";
    $('.btn').prop('disabled', true);
    $('#prompt-btn-save').prop('disabled', false);
    $('#prompt-btn-cancel').prop('disabled', false);
  }
}

let promptDeleteClick = async () => {
  let url = STRING_TABLE.URL.DELETE_PROFILE_PROMPT;

  const promptObject = {
    promptID: selectedPromptId,
    profileID: profileId,
    type: 'F',
  };

  let title = $("#deletePromptTitle").val();
  let message = $("#deletePromptMessage").val();

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      await ajaxPostAsync(url, promptObject)
        .then(() => {
          toastr.success($("#promptDeletedSuccessfully").val());
          loadPromptMessages();
          loadPromptTable();
        })
        .catch(toastrError);
    }
  });
}

let promptSaveClick = async () => {
  if (isNullUndefinedOrWhiteSpace($("#sequence").val())) {
    toastr.warning($("#sequenceIsRequired").val());
    return;
  }

  let newSeq = parseInt($("#sequence").val()) || 0;
  let newPromptID = $("#message-select").val();

  if (newSeq < 1 || newSeq > 10) {
    toastr.warning($("#seqRange").val());
    return;
  }

  const promptObject = {
    promptSeq: newSeq,
    profileID: profileId,
    promptID: newPromptID,
    type: 'F',
  };

  let alreadyExists = await sequenceAlreadyUsed(promptObject);
  if (alreadyExists) {
    toastr.warning($("#seqAlreadyUsed").val());
    return;
  }

  let url = STRING_TABLE.URL.ADD_PROFILE_PROMPT
  await ajaxPostAsync(url, promptObject)
    .then(_ => {
      toastr.success($("#promptAddedSuccessfully").val());
      loadPromptTable();
      promptCancelClick();
    })
    .catch(toastrError);
}

let promptCancelClick = () => {
  $('.btn').prop('disabled', false);
  $('#prompt-btn-save').prop('disabled', true);
  $('#prompt-btn-cancel').prop('disabled', true);
  document.getElementById("sequence").value = "";
  document.getElementById("prompt-form").style.display = "none";
}

let sequenceAlreadyUsed = async (promptObject) => {
  let url = STRING_TABLE.URL.GET_PROFILE_PROMPT
  let sequenceAlreadyExists;

  await ajaxPostAsync(url, promptObject)
    .then(_restrictionProfile => {
      sequenceAlreadyExists = !isNullOrUndefined(_restrictionProfile)
    })
    .catch(toastrError);

  return sequenceAlreadyExists;
}

let profileIdAlreadyExists = async () => {
  if ($('#profile-id').prop('readOnly')) {
    return;
  }

  let newProfileId = document.getElementById("profile-id").value;
  let url = `${STRING_TABLE.URL.GET_CARD_RESTRICTION_PROFILE}?profileId=${encodeURIComponent(newProfileId)}`
  await ajaxGetAsync(url)
    .then(_restrictionProfile => {
      let alreadyExists = !isNullOrUndefined(_restrictionProfile);

      if (alreadyExists) {
        toastr.warning($("#profileIdAlreadyExists").val().replace("{0}", newProfileId));
      }

      $('#btn-save').prop('disabled', alreadyExists);
    })
    .catch(toastrError);
}

let cancelButtonClicked = async () => {
  if (action === copy) {
    return goToCardRestrictionGridPage();
  }

  $('.text-danger').prop('hidden', true);
  setEditMode(false);

  const modelDataInput = document.getElementById("modelData");
  let modelData = JSON.parse(modelDataInput.value);
  const form = document.getElementById("cardRestrictionProfileForm");

  for (let key in modelData) {
    if (modelData.hasOwnProperty(key)) {
      let capitalizedKey = capitalizeFirstLetter(key);
      const inputElement = form.querySelector(`[name="${capitalizedKey}"]`);
      if (inputElement) {
        inputElement.value = modelData[key];
      }
    }
  }

  $("#AskForPOCheckbox").prop("checked", modelData.askForPOCheckbox);
  $("#PromptForFuelCheckbox").prop("checked", modelData.promptForFuelCheckbox);
  $("#LimitTimeofPurchaseCheckbox").prop("checked", modelData.limitTimeofPurchaseCheckbox);

  if (modelData.restrictProducts) {
    $("#denyProducts").prop("checked", true);
  }
  else {
    $("#allowProducts").prop("checked", true);
  }

  let url = STRING_TABLE.URL.DELETE_PROFILE_PROMPT;

  const promptObject = {
    profileID: profileId,
    type: 'F',
  };

  if (!modelData.promptForFuelCheckbox) {
    await ajaxPostAsync(url, promptObject)
      .then(() => {
        loadPromptMessages();
        loadPromptTable();
      })
      .catch(toastrError);
  }

  await loadRestrictionTableData();
  createTimeOfPurchaseRestrictionTable();
}
