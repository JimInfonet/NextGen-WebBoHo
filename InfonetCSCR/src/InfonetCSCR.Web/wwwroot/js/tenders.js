let tendersDatatable;
let tenders;
let selectedTendCode;
let cardCode = null;
const URL_GET_TENDERS = "/Tenders";
const edit = CONSTANT_TEXTS.EDIT;
const view = CONSTANT_TEXTS.VIEW; 

$(async function () {
  await initTenderPage();
});

let initTenderPage = async (tenderAddOrUpdate = true) => {
  await loadTenders();
  await createTendersDatatable();
  if (tenderAddOrUpdate) {
    if ($("#isTenderDetailsUpdated").val()?.toLowerCase() === "false") {
      toastr.error($("#failedToUpdateTenderDetails").val());
    } else if ($("#isTenderDetailsUpdated").val()?.toLowerCase() === "true") {
      toastr.success($("#tenderUpdatedSuccessfully").val());
    }

    if ($("#isTenderAdded").val()?.toLowerCase() === "false") {
      toastr.error($("#failedToAddTender").val())
    } else if ($("#isTenderAdded").val()?.toLowerCase() === "true") {
      toastr.success($("#tenderAddedSuccessfully").val());
    }
  }

  selectedTendCode = "";
  $("#btn-editCardDefinition").prop('hidden', true);
}

let createTendersDatatable = async () => {
  if (tendersDatatable) {
    tendersDatatable.state.clear();
    tendersDatatable.clear();
    tendersDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = tenders;
  props.scrollY = '440px';
  props.paging = false;
  props.searching = false;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "code", },
    { "data": "shortName", },
    { "data": "fullName", },
    { "data": "class", },
    { "data": "exchangeRate", },
    { "data": "sequence", },
    { "data": "cardCode", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6] }
  ];

  tendersDatatable = createDataTableGeneric('tenders-grid', props);

  $('#tenders-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
      } else {
        tendersDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
      }
      selectedTendCode = $(this).find("td:first")[0].innerText;
      cardCode = tenders.filter(x => x.code === selectedTendCode)[0]?.cardCode;

      if (cardCode) {
        $("#btn-editCardDefinition").prop('hidden', false);
      }
      else {
        $("#btn-editCardDefinition").prop('hidden', true);
      }
    })
    .on('dblclick', 'tr', function () {
      let tendCode = $(this).find("td:first")[0].innerText;
      const url = `/Tenders/TenderDetails?tendCode=${tendCode}&action=${view}`;
      safelyNavigateTo(url);
    });

  $('#button').on('click', function () {
    tendersDatatable.row('.selected-grid-row').remove().draw(false);
  });
}

let onClickEditCardDefinition = () => {
  if (cardCode) {
    const url = `/CardDefinition?cardCode=${cardCode}&tendCodeData=${selectedTendCode}&fromGrid=${false}`;
    safelyNavigateTo(url);
  }
}

let loadTenders = async () => {
  let url = URL_GET_TENDERS + "/GetTendersWithCardCode";
  await ajaxGetAsync(url)
    .then(_tenders => {
      tenders = _tenders;
      for (let i = 0; i < tenders.length; i++) {
        if (tenders[i].cardCode == 0) {
          tenders[i].cardCode = null;
        }
      }
      createTendersDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingTenders").val()));
}

// On Add button click
$("#btn-add").on('click', function () {
  const url = `${URL_GET_TENDERS}/TenderForm`;
  safelyNavigateTo(url);
})

// On edit button click
$("#btn-edit").on('click', function () {
  if (selectedTendCode) {
    const url = `${URL_GET_TENDERS}/TenderDetails?tendCode=${selectedTendCode}&action=${edit}`;
    safelyNavigateTo(url);
  }
  else {
    toastr.warning($("#pleaseSelectATender").val());
  }
})

// On Delete button click
$("#btn-delete").on('click', async function () {
  if (!selectedTendCode?.length) {
    toastr.warning($("#pleaseSelectATender").val());
    return;
  }

  let tendFullName = tenders.filter(x => x.code === selectedTendCode)[0]?.fullName;
  let url = `${URL_GET_TENDERS}/CheckTranscationExistsForTendName?tendName=${tendFullName}`;

  await ajaxGetAsync(url)
    .then(exist => {
      if (!exist) {
        $("#deleteTenderConfirmModal").modal('show');
        return;
      }
      let title = document.getElementById("tenderCannotBeDeletedModalTitle").value;
      let message = document.getElementById("tenderCannotBeDeleted").value;
      showAlertModal(title, message);
    })
    .catch(() => toastr.error(document.getElementById("someErrorOccuredRefreshAndTryAgain")));
})

// On Yes button click in delete tender confirmation modal
$("#deleteTenderConfirmModalYesButtonControl").on('click', async function () {
  let cardCode = tenders.filter(x => x.code === selectedTendCode)[0]?.cardCode;
  let url = `${URL_GET_TENDERS}/DeleteTender?TendCode=${selectedTendCode}&CardCode=${cardCode}`;

  await ajaxGetAsync(url)
    .then(async () => {
      initTenderPage(false);
      toastr.success($("#tenderDeletedSuccessfully").val());
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileDeletingTender").val()));

  $("#deleteTenderConfirmModal").modal('hide');
})

// On Reset Sequence button click
$("#btn-resetSequence").on('click', async function () {
  let url = `${URL_GET_TENDERS}/ResetSequence`;

  await ajaxGetAsync(url)
    .then(async () => initTenderPage(false))
    .catch(() => toastr.error($("#anErrorHasOccuredWhileUpdatingResetSequence").val()));
})
