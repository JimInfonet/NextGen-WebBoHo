let cardDefinitionDatatable;
let cards;
let selectedCardCode;
let rowDoubleClick = CONSTANT_TEXTS.ROW_DOUBLE_CLICK;

$(async function () {
  await loadCards();
  await createCardDefinitionDatatable();
});

let createCardDefinitionDatatable = async () => {
  if (cardDefinitionDatatable) {
    cardDefinitionDatatable.state.clear();
    cardDefinitionDatatable.clear();
    cardDefinitionDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = cards;
  props.scrollY = '440px';
  props.paging = false;
  props.searching = false;
  props.bInfo = false;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "cardCode", },
    { "data": "description", },
    { "data": "cardType", },
  ];

  cardDefinitionDatatable = createDataTableGeneric('cardDefinition-grid', props);

  $('#cardDefinition-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        $("#editCardDefintionButton").hide();
        selectedCardCode = null;
      } else {
        cardDefinitionDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        $("#editCardDefintionButton").show();
        selectedCardCode = $(this).find("td:first")[0].innerText;
      }
    })
    .on('dblclick', 'tr', function () {
      selectedCardCode = $(this).find("td:first")[0].innerText;
      const url = `/CardDefinition?cardCode=${selectedCardCode}&fromGrid=${true}&actionMode=${rowDoubleClick}`;
      safelyNavigateTo(url);
    });
}

let loadCards = async () => {
  let url = "CardDefinition/GetCards";
  await ajaxGetAsync(url, {})
    .then((response) => {
      if (response) {
        cards = response;
        createCardDefinitionDatatable();
      }
      else {
        toastr.error($("#failedToLoadTender").val());
      }
    })
    .catch((error) => {
      toastr.error($("#anErrorHasOccuredWhileLoadingTenders").val());
    });
}

let deleteCardDefinition = async () => {
  if (selectedCardCode) {
    let url = `/CardDefinitionGrid/DeleteCardDefinition?cardCode=${selectedCardCode}`;
    await ajaxGetAsync(url)
      .then(async (response) => {
        if (response === "Error") {
          toastr.error($("#anErrorOccurredWhileDeletingCard").val());
        }
        else if (response !== "Success") {
          let title = document.getElementById("cannotDeleteCard").value;
          let message = document.getElementById("cardCodeIsLinkedToTender").value.replace("{0}", selectedCardCode).replace("{1}", response);

          showAlertModal(title, message).then((result) => { });
        }
      })
      .catch((error) => {
        toastr.error($("#anErrorOccurredWhileDeletingCard").val());
      });
  }
  else {
    toastr.warning($("#pleaseSelectCardDefinitionBeforeDelete").val());
  }
}

let goToEditCardDefinition = () => {
  if (selectedCardCode) {
    const url = `/CardDefinition?cardCode=${selectedCardCode}&fromGrid=${true}`;
    safelyNavigateTo(url);
  }
  else {
    toastr.warning($("#pleaseSelectCardDefinitionBeforeEdit").val());
  }
}
