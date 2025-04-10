let positiveCardsData;
let positiveCardsDatatable;
let selectedPositiveCardData;
const positiveCards = CONSTANT_TEXTS.POSITIVE_CARDS;
let negativeCardsData;
let negativeCardsDatatable;
let selectedNegativeCardData;
const negativeCards = CONSTANT_TEXTS.NEGATIVE_CARDS;
let messagesDatatable;
const messages = CONSTANT_TEXTS.MESSAGES;
let cardMessagesData;
let selectedMessageData;
let messageLengthUpperLimit = 1000;
let messageLengthLowerLimit = 1;
let isBackOffice = document.getElementById("isBackOffice").value === "True";
let hoOrBOText = isBackOffice ? $("#boText").val() : $("#hoText").val();
let headOfficeOrBackOfficeText = isBackOffice ? $("#backOffice").val() : $("#headOffice").val();

$(async function () {
  await loadPositiveCardsDataTable();
  await loadNegativeCardsDataTable();
  await loadMessagesDataTable();
});

let loadPositiveCardsDataTable = async () => {
  await loadPositiveCardsData();
  createDatatable(positiveCards);

  $("#positiveCardDeleteButtonId").prop("disabled", positiveCardsData.length === 0);

  $('#positiveCardsTitle').text(`${$("#positiveCards").val()} ${positiveCardsData.length}`);
  $("#activeYNSelect").empty();
  $("#activeYNSelect").append(optionGeneratorDefault($("#activeYNText").val()));
  $("#activeYNSelect").append(optionGeneratorForSingleElement($("#yesText").val()));
  $("#activeYNSelect").append(optionGeneratorForSingleElement($("#noText").val()));
}

let loadPositiveCardsData = async () => {
  let url = "NegativePositiveCards/GetAllPositiveCardsData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        positiveCardsData = response;
      }
    })
    .catch(toastrError);
}

let assignSelectedRowData = (type, selectedRowData) => {
  if (type === positiveCards) {
    selectedPositiveCardData = selectedRowData;
  }
  else if (type === negativeCards) {
    selectedNegativeCardData = selectedRowData;
  }
  else if (type === messages) {
    selectedMessageData = selectedRowData;
  }
}

let assignDataTable = (type, dataTable) => {
  if (type === positiveCards) {
    positiveCardsDatatable = dataTable;
  }
  else if (type === negativeCards) {
    negativeCardsDatatable = dataTable;
  }
  else if (type === messages) {
    messagesDatatable = dataTable;
  }
}

let createDatatable = async (type) => {
  let dataTable;
  let data;
  let tableId;
  let dataTableColumns;

  if (type === positiveCards) {
    dataTable = positiveCardsDatatable;
    data = positiveCardsData;
    tableId = "positiveCards-grid";
    dataTableColumns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "cardNumber", },
      {
        "data": "active",
        "render": function (data, type, row) {
          if (type === "display") {
            return (data === true) ? $("#yesText").val() : $("#noText").val();
          }
          return data;
        }
      },
      { "data": "pin", }
    ];
  }
  else if (type === negativeCards) {
    dataTable = negativeCardsDatatable;
    data = negativeCardsData;
    tableId = "negativeCards-grid";
    dataTableColumns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "cardNumber", },
      { "data": "messageCode", },
      {
        "data": "entryDate",
        "render": function (data, type, row) {
          if (type === "display") {
            return formateDateTimeStringToMMDDYY(data);
          }
          return data;
        }
      },
      {
        "data": "expiryDate",
        "render": function (data, type, row) {
          if (type === "display") {
            return formateDateTimeStringToMMDDYY(data);
          }
          return data;
        }
      }
    ];
  }
  else if (type === messages) {
    dataTable = messagesDatatable;
    data = cardMessagesData;
    tableId = "messages-grid";
    dataTableColumns = [
      //the first letter of the column name should be lowercase and first letter of the next words is uppercase
      { "data": "code", },
      { "data": "message", },
    ];
  }

  destroyDataTableIfExists(dataTable);

  let props = createDataTableProp();
  props.data = data;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = false;
  props.columns = dataTableColumns;
  props.columnDefs = [
    { className: 'text-center', targets: '_all' }
  ];
  dataTable = createDataTableGeneric(tableId, props);

  let selectedRowData;
  if (dataTable.data().count() > 0) {
    $(`#${tableId} tbody tr:first-child`).addClass('selected-grid-row');
    selectedRowData = dataTable.row(':eq(0)').data();
  }
  else {
    selectedRowData = null;
  }

  assignSelectedRowData(type, selectedRowData);

  $(`#${tableId} tbody`)
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', async function () {
      if (!$(this).hasClass('selected-grid-row')) {
        dataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');

        selectedRowData = dataTable.row(this).data();
        assignSelectedRowData(type, selectedRowData);
      }
    });

  assignDataTable(type, dataTable)
}

let addNewPositiveCardButtonClicked = () => {
  $("#addCardNumber").val("");
  $("#addPIN").val("");
  document.getElementById('activeYNSelect').selectedIndex = 0;

  $("#addNewPositiveCardModal").modal("show");
}

let insertPositiveCard = async () => {
  let data = {
    cardNumber: $("#addCardNumber").val(),
    active: $("#activeYNSelect").val().toLowerCase() === $("#yesText").val().toLowerCase(),
    pin: $("#addPIN").val()
  }

  if (!data.cardNumber) {
    toastr.warning($("#theCardNumberMustNotBlank").val());
    return;
  }

  if ($("#activeYNSelect").val() === "") {
    toastr.warning(`${$("#pleaseSelectActiveDeactive").val()}`);
    return;
  }

  let existsData = positiveCardsData.find(obj => obj.cardNumber === data.cardNumber);

  if (existsData) {
    toastr.warning(`${$("#cardNumberAlreadyExists").val().replace("{0}", data.cardNumber)}\n${$("#useUniqueCardNumber").val()}`);
    return;
  }

  let url = `NegativePositiveCards/InsertPositiveCard`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadPositiveCardsDataTable();
        $("#addNewPositiveCardModal").modal("hide");
        toastr.success($("#positiveCardAddedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let deletePositiveCardButtonClicked = async () => {

  let title = $("#confirmDelete").val();
  let message = $("#areYouSureYouWantToDeleteCardNumber").val().replace("{0}", selectedPositiveCardData.cardNumber);
  let isDelete = false;

  await showConfirmModal(title, message).then((result) => {
    isDelete = result;
  });

  if (isDelete) {
    let url = `NegativePositiveCards/DeletePositiveCard`;
    await ajaxPostAsync(url, selectedPositiveCardData)
      .then(async (response) => {
        if (response) {
          await loadPositiveCardsDataTable();
          toastr.success($("#positiveCardDeletedSuccessfully").val());
        }
      })
      .catch(toastrError);
  }
}

let loadNegativeCardsDataTable = async () => {
  await loadNegativeCardsData();
  createDatatable(negativeCards);

  $("#negativeCardDeleteButtonId").prop("disabled", negativeCardsData.length === 0);
  $('#negativeCardsTitle').text(`${$("#negativeCards").val()} ${negativeCardsData.length}`);
}

let loadNegativeCardsData = async () => {
  let url = "NegativePositiveCards/GetAllNegativeCardsData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        negativeCardsData = response;
      }
    })
    .catch(toastrError);
}

let formateDateTimeStringToMMDDYY = (originalDateTime) => {
  if (!originalDateTime) return "";
  const dateObj = new Date(originalDateTime);

  const year = dateObj.getFullYear();
  const month = ('0' + (dateObj.getMonth() + 1)).slice(-2);
  const day = ('0' + dateObj.getDate()).slice(-2);

  const formattedDate = month + '/' + day + '/' + year;

  return formattedDate;
}

let getTodaysDateInYYYYMMDDFormate = () => {
  let today = new Date();
  let yyyy = today.getFullYear();
  let mm = String(today.getMonth() + 1).padStart(2, '0');
  let dd = String(today.getDate()).padStart(2, '0');
  let formattedDate = yyyy + '-' + mm + '-' + dd;
  return formattedDate;
}

let addNewNegativeCardButtonClicked = () => {
  $("#negativeCardNumber").val("");
  $("#messageCodeSelect").val("");
  $("#entryDate").val("");
  $("#entryDate").val("");

  let formattedDate = getTodaysDateInYYYYMMDDFormate();

  $('#entryDate').val(formattedDate);
  $('#expiryDate').val(formattedDate);

  renderMessagesInNegativeCardModal();
  $("#addNewNegativeCardModal").modal("show");
}

let deleteNegativeCardButtonClicked = async () => {
  let title = $("#confirmDelete").val();
  let message = $("#areYouSureYouWantToDeleteCardNumber").val().replace("{0}", selectedNegativeCardData.cardNumber);
  let isDelete = false;

  await showConfirmModal(title, message).then((result) => {
    isDelete = result;
  });

  if (isDelete) {
    let url = `NegativePositiveCards/DeleteNegativeCard`;
    await ajaxPostAsync(url, selectedNegativeCardData)
      .then(async (response) => {
        if (response) {
          await loadNegativeCardsDataTable();
          toastr.success($("#negativeCardDeletedSuccessfully").val());
        }
      })
      .catch(toastrError);
  }
}

let loadCardMessagesData = async () => {
  let url = "NegativePositiveCards/GetAllCardMessagesData";
  await ajaxGetAsync(url)
    .then(async (response) => {
      if (response) {
        cardMessagesData = response;
      }
    })
    .catch(toastrError);
}

let renderMessagesInNegativeCardModal = () => {
  $("#messageCodeSelect").empty();
  $("#messageCodeSelect").append(optionGeneratorDefault($("#messageCode").val()));
  cardMessagesData.forEach(({ code, message }) => {
    $("#messageCodeSelect").append(optionGenerator(code, message));
  })
}

let insertNegativeCard = async () => {
  let data = {
    cardNumber: $("#negativeCardNumber").val(),
    messageCode: $("#messageCodeSelect").val(),
    entryDate: $("#entryDate").val(),
    expiryDate: $("#expiryDate").val(),
  }

  if (!data.cardNumber) {
    toastr.warning($("#theCardNumberMustNotBlank").val());
    return;
  }

  if (!validateDateRange(data.entryDate)) {
    toastr.warning($("#pleaseEnterEntryDateBetween").val());
    return;
  }

  if (!validateDateRange(data.expiryDate)) {
    toastr.warning($("#pleaseEnterExpiryDateBetween").val());
    return;
  }

  let existsData = negativeCardsData.find(obj => obj.cardNumber === data.cardNumber);

  if (existsData) {
    toastr.warning(`${$("#cardNumberAlreadyExists").val().replace("{0}", data.cardNumber)}\n${$("#useUniqueCardNumber").val()}`);
    return;
  }

  if (!data.messageCode) {
    data.messageCode = 0;
  }

  let url = `NegativePositiveCards/InsertNegativeCard`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadNegativeCardsDataTable();
        $("#addNewNegativeCardModal").modal("hide");
        toastr.success($("#negativeCardAddedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

$('#entryDate, #expiryDate').on('change', function () {
  if ($(this).val() === '') {
    let formattedDate = getTodaysDateInYYYYMMDDFormate();
    $(this).val(formattedDate);
  }
});

let validateDateRange = (inputDate) => {
  let maxDate = new Date('2079-06-06');
  let minDate = new Date('1900-01-01');
  let enteredDate = new Date(inputDate);

  if (enteredDate > maxDate || enteredDate < minDate) {
    let formattedDate = getTodaysDateInYYYYMMDDFormate();
    $(this).val(formattedDate);
    return false;
  }

  return true;
}

let loadMessagesDataTable = async () => {
  await loadCardMessagesData();
  createDatatable(messages);
  $("#deleteMessageButtonId").prop("disabled", cardMessagesData.length === 0);
}

let addNewMessageButtonClicked = () => {
  $("#messageCodeInput").val("");
  $("#messageInput").val("");

  $("#addNewMessageModal").modal("show");
}

let deleteMessageButtonClicked = async () => {
  let title = $("#confirmDelete").val();
  let message = $("#areYouSureYouWantToDeleteMessageCode").val().replace("{0}", selectedMessageData.code);
  let isDelete = false;

  let isMessageCodeUsedInNegativeCards = negativeCardsData.find(obj => obj.messageCode === selectedMessageData.code);

  if (isMessageCodeUsedInNegativeCards) {
    toastr.warning($("#thisMessageCodeIsUsedByNegativeCards").val());
    return;
  }

  await showConfirmModal(title, message).then((result) => {
    isDelete = result;
  });

  if (isDelete) {
    let url = `NegativePositiveCards/DeleteCardMessage`;
    await ajaxPostAsync(url, selectedMessageData)
      .then(async (response) => {
        if (response) {
          await loadMessagesDataTable();
          toastr.success($("#messageDeletedSuccessfully").val());
        }
      })
      .catch(toastrError);
  }
}

let insertMessage = async () => {
  let data = {
    code: parseInt($("#messageCodeInput").val()),
    message: $("#messageInput").val(),
  }
  if (!$("#messageCodeInput").val()) {
    toastr.warning($("#theMessageCodeMustNotBeBlank").val());
    return;
  }

  if (isNaN(data.code)) {
    toastr.warning($("#theMessageCodeMustBeNumeric").val());
    return;
  }

  if (!data.message) {
    toastr.warning($("#theMessageMustNotBeBlank").val());
    return;
  }

  if (data.code < messageLengthLowerLimit || data.code > messageLengthUpperLimit) {
    toastr.warning($("#pleaseEnterMessageCodeBetween").val());
    return;
  }

  let existsData = cardMessagesData.find(obj => obj.code === data.code);

  if (existsData) {
    toastr.warning(`${$("#messageCodeAlreadyExists").val().replace("{0}", data.code)}\n${$("#useUniqueMessageCode").val()}`);
    return;
  }

  let url = `NegativePositiveCards/InsertCardMessage`;
  await ajaxPostAsync(url, data)
    .then(async (response) => {
      if (response) {
        await loadMessagesDataTable();
        $("#addNewMessageModal").modal("hide");
        toastr.success($("#messageAddedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let readNegativeCardButtonClicked = async () => {
  let url = `NegativePositiveCards/ReadNegativeCard`;
  await ajaxPostAsync(url)
    .then(async (response) => {
      if (response) {

        if (response.errorFilePath) {
          let title = "";
          let message = `${$("#negativeFileDoesntExist").val().replace("{0}", response.errorFilePath)}\n`;
          showAlertModal(title, message).then((result) => { });

          return;
        }

        let title = `${$("#readInputFileFromHeadOffice").val().replace("{0}", headOfficeOrBackOfficeText)} ${response.filePath}`;
        let message = `(${response.numberOfLines}) ${$("#recordReadFromHoFile").val().replace("{0}", hoOrBOText)}\n`;
        message += `(${response.numberOfLinesAddedToNegativeCard}) ${$("#recordAddedToNegativeCardTable").val()}\n`;
        message += `(${response.numberOfLinesAddedToPositiveCard}) ${$("#recordsAddedToPositiveCardsFile").val()}\n`;
        message += `(${response.numberOfLinesDeletedFromNegativeCard}) ${$("#recordDeletedFromNegativeCardTable").val()}\n`;
        message += `(${response.numberOfLinesDeletedFromPositiveCard}) ${$("#recordDeletedFromPositiveCardsFile").val()}\n`;
        message += `(${response.numberOfLinesCannotAddedToNegativeCard}) ${$("#linesCannotBeAddedToNegativeCardsTable").val()}\n`;
        message += `(${response.numberOfLinesCannotAddedToPositiveCard}) ${$("#linesCannotBeAddedToPositiveCardsFile").val()}\n`;
        message += `(${response.numberOfLinesCannotDeletedFromNegativeCard}) ${$("#linesCannotBeDeletedFromNegativeCardsTable").val()}\n`;
        message += `(${response.numberOfLinesCannotDeletedFromPositiveCard}) ${$("#linesCannotBeDeletedFromPositiveCardsFile").val()}\n`;
        message += `(${response.numberOfLinesErrorFromFile}) ${$("#lineErrorFromHoFileWrongFormat").val().replace("{0}", hoOrBOText)}\n`;

        showAlertModal(title, message).then((result) => { });

        await loadPositiveCardsDataTable();
        await loadNegativeCardsDataTable();
      }
    })
    .catch(toastrError);
}

let readPositiveCardButtonClicked = async () => {
  let url = `NegativePositiveCards/ReadPositiveCard`;
  await ajaxPostAsync(url)
    .then(async (response) => {
      if (response) {

        if (response.errorFilePath) {
          let title = "";
          let message = `${$("#positiveFileDoesntExistIn").val().replace("{0}", response.errorFilePath)}\n`;
          showAlertModal(title, message).then((result) => { });

          return;
        }

        let title = `${$("#readInputFileFromHeadOffice").val().replace("{0}", headOfficeOrBackOfficeText)} ${response.filePath}`;
        let message = `(${response.numberOfLines}) ${$("#recordReadFromHoFile").val().replace("{0}", hoOrBOText)}\n`;
        message += `(${response.numberOfLinesAddedToPositiveCard}) ${$("#recordsAddedToPositiveCardsTable").val()}\n`;
        message += `(${response.numberOfLinesDeletedFromPositiveCard}) ${$("#recordDeletedFromPositiveCardsTable").val()}\n`;
        message += `(${response.numberOfLinesCannotAddedToPositiveCard}) ${$("#linesCannotBeAddedToPositiveCardsTable").val()}\n`;
        message += `(${response.numberOfLinesCannotDeletedFromPositiveCard}) ${$("#linesCannotBeDeletedFromPositiveCardsTable").val()}\n`;
        message += `(${response.numberOfLinesErrorFromFile}) ${$("#lineErrorFromHoFileWrongFormat").val().replace("{0}", hoOrBOText)}\n`;

        showAlertModal(title, message).then((result) => { });

        await loadPositiveCardsDataTable();
      }
    })
    .catch(toastrError);
}
