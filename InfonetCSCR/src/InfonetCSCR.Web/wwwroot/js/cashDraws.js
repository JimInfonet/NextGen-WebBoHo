let drawTypesData;
let buttonIdSerialForCoin = [0, 3, 1, 4, 2, 5];
let buttonIdSerialForBills = [0, 1, 2, 3];
let selectedButtonData;
let selectedButtonId = null;
let selectedCurrencyType;
let valueUpperLimit = MODEL_VALIDATION_DATA_LIMIT.SMALL_MONEY_UPPER_LIMIT;

$(async function () {
  await loadAllDrawTypesData();
  renderCashDrawsButtons(true);
  renderCashDrawsButtons(false);
});

let reloadCashDraw = async () => {
  await loadAllDrawTypesData();
  renderCashDrawsButtons(true);
  renderCashDrawsButtons(false);
  $(`#button-${selectedButtonId}-${selectedCurrencyType}`).trigger('click');
}

let loadAllDrawTypesData = async () => {
  let url = "CashDraws/GetAllDrawTypesData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        drawTypesData = response;
      }
    })
    .catch(toastrError);
}

let renderCashDrawsButtons = (isCoinRollsContainer) => {
  const buttonContainer = isCoinRollsContainer ? $('#cashDrawButtonsContainerLeft') : $('#cashDrawButtonsContainerRight');
  buttonContainer.empty();

  const currencyType = isCoinRollsContainer ? 'C' : 'B'; //Hard coded in VB codebase
  let buttonIdSerial = buttonIdSerialForBills;

  if (isCoinRollsContainer) {
    for (let i = 1; i <= 2; i++) {
      const button = $('<button>', {
        class: 'balancerButton cashDrawsButton',
        html: i
      });

      buttonContainer.append(button);
    }

    buttonIdSerial = buttonIdSerialForCoin;
  }

  buttonIdSerial.forEach(buttonId => {
    let buttonData = drawTypesData.find(obj => (obj.buttonNumber === buttonId && obj.currencyType === currencyType));
    let displayName = buttonId;

    if (buttonData) {
      displayName = buttonData.currencyName;
    }
    else {
      displayName = $("#notUsed").val();
    }

    const button = $('<button>', {
      class: 'cashDrawsButton',
      id: `button-${buttonId}-${currencyType}`,
      text: displayName
    });

    if (!buttonData) {
      button.addClass("notUsedButton");
    }

    button.on('click', async function () {
      $('.cashDrawsButton').removeClass('selectedButton');
      $(this).addClass('selectedButton');

      selectedButtonData = buttonData;
      selectedButtonId = buttonId;
      selectedCurrencyType = currencyType;


      $("#updateButton").prop("disabled", false);
      $("#disableButton").prop("disabled", (!selectedButtonData));

      renderSelectedButtonDataInForm();
    });

    buttonContainer.append(button);
  });
}

let renderSelectedButtonDataInForm = () => {
  if (selectedButtonData) {
    $("#nameOnButton").val(selectedButtonData.currencyName);
    $("#amountAddedPerClick").val(selectedButtonData.value.toFixed(2));
  }
  else {
    $("#nameOnButton").val($("#notUsed").val());
    $("#amountAddedPerClick").val("0.00");
  }
}

let addOrUpdateButtonClicked = async () => {
  if (isNullOrUndefined(selectedButtonId)) {
    toastr.warning($("#coinBillButtonHasToBeSelectedBeforeUpdate").val());
    return;
  }

  let nameOnButton = $("#nameOnButton").val();
  if (!nameOnButton || nameOnButton === $("#notUsed").val()) {
    toastr.warning($("#youHaveToEnterNameOnButton").val());
    return;
  }

  let amountAddedPerClick = parseFloat($("#amountAddedPerClick").val()).toFixed(2);

  if (isNaN(amountAddedPerClick)) {
    toastr.warning($("#pleaseEnterValidAmount").val());
    return;
  }
  else if (amountAddedPerClick < 0 || amountAddedPerClick > valueUpperLimit) {
    toastr.warning($("#theAmountMustBeBetween").val());
    return;
  }

  let dataExists = drawTypesData.find(obj => (obj.currencyName.toLowerCase() === nameOnButton.toLowerCase()));
  if (dataExists && (dataExists.buttonNumber !== selectedButtonId || dataExists.currencyType !== selectedCurrencyType)) {
    toastr.warning($("#theButtonNameAlreadyExists").val());
    return;
  }

  let data = {
    currencyName: nameOnButton,
    currencyType: selectedCurrencyType,
    buttonNumber: selectedButtonId,
    value: parseFloat(amountAddedPerClick)
  }

  if (selectedButtonData) {
    if (checkDrawTypeDataChanged(data)) {
      await updateCashDraws(data);
    }
  }
  else {
    await insertCashDraws(data);
  }
}

let disableButtonClicked = async () => {
  if (isNullOrUndefined(selectedButtonId)) {
    toastr.warning($("#coinBillButtonHasToBeSelectedBeforeDisable").val());
    return;
  }

  await deleteCashDraws(selectedButtonData);
}

let insertCashDraws = async (data) => {
  let url = `CashDraws/InsertDrawType`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await reloadCashDraw();
        toastr.success($("#cashDrawAddedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let updateCashDraws = async (data) => {
  let url = `CashDraws/UpdateDrawType`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await reloadCashDraw();
        toastr.success($("#cashDrawUpdatedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let deleteCashDraws = async (data) => {
  let url = `CashDraws/DeleteDrawType`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await reloadCashDraw();
        toastr.success($("#buttonSuccessfullyDisableld").val());
      }
    })
    .catch(toastrError);
}

$('#amountAddedPerClick').on('blur', function () {
  let inputValue = $(this).val();
  if (!isNaN(inputValue)) {
    $(this).val(parseFloat(inputValue).toFixed(2));
  }
});

let checkDrawTypeDataChanged = (data) => {
  let drawType = drawTypesData.find(obj => (obj.buttonNumber === selectedButtonId && obj.currencyType === selectedCurrencyType));
  return !isObjectsEqual(drawType, data);
}