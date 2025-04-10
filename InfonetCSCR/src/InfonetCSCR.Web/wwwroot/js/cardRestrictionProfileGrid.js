let cardRestrictions;
let cardRestrictionDatatable;
let selectedCardRestriction;
let selectedProfileId;
let selectedProfileName;

let create = CONSTANT_TEXTS.CREATE;
let update = CONSTANT_TEXTS.UPDATE;
let viewMode = CONSTANT_TEXTS.VIEW;
let copy = CONSTANT_TEXTS.COPY;

$(async function () {
  await loadCardRestrictions();
});

let loadCardRestrictions = async () => {
  let url = "Customer/GetAllRestrictionProfiles";
  await ajaxGetAsync(url)
    .then(_cardRestrictions => {
      cardRestrictions = _cardRestrictions;

      $("#btn-edit").prop("disabled", (cardRestrictions.length === 0));
      $("#btn-delete").prop("disabled", (cardRestrictions.length === 0));
      $("#btn-copy").prop("disabled", (cardRestrictions.length === 0));
      $("#btn-product-restriction").prop("disabled", (cardRestrictions.length === 0));

      if (cardRestrictions.length > 0) {
        selectedProfileId = cardRestrictions[0].profileID;
        selectedProfileName = cardRestrictions[0].profileName;
      }
      createCardRestrictionsDatatable();
    })
    .catch(() => toastr.error($("#anErrorHasOccuredWhileLoadingCardRestrictions").val()));
}

let createCardRestrictionsDatatable = () => {
  destroyDataTableIfExists(cardRestrictionDatatable)

  let props = createDataTableProp();
  props.data = cardRestrictions;
  props.scrollY = '440px';
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  props.columns = [
    { "data": "profileID", },
    { "data": "profileName", },
    {
      "data": "restrictProducts",
      render: function (restrictProducts) {
        return restrictProducts ? $("#yes").val() : $("#no").val() ;
      },
    },
  ];

  cardRestrictionDatatable = createDatatableWithSearching('card-restriction-profile-grid', props);

  if (cardRestrictions.length > 0) {
    $(`#card-restriction-profile-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#card-restriction-profile-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (cardRestrictions.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        cardRestrictionDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedProfileId = $(this).find("td:first")[0].innerText;
        selectedProfileName = $(this).find("td")[1].innerText;
      }
    })
    .on('dblclick', 'tr', function () {
      if (cardRestrictions.length === 0) {
        return;
      }

      selectedProfileId = $(this).find("td:first")[0].innerText;
      selectedProfileName = $(this).find("td")[1].innerText;
      const url = `CardRestrictionProfile?profileId=${encodeURIComponent(selectedProfileId)}&ActionMode=${viewMode}`;
      safelyNavigateTo(url);
    });
}

let onCardProfileAddClick = () => {
  const url = `CardRestrictionProfile?ActionMode=${create}`;
  safelyNavigateTo(url);
}

let onCardProfileEditClick = () => {
  const url = `CardRestrictionProfile?profileId=${encodeURIComponent(selectedProfileId)}&ActionMode=${update}`;
  safelyNavigateTo(url);
}

let onCardProfileDeleteClick = async () => {
  let title = $("#deleteCardRestrictionProfileTitle").val();
  let message = $("#deleteCardRestrictionProfileMessage").val().replace("{0}", selectedProfileId);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `CardRestrictionProfile/DeleteCardRestrictionProfile?profileId=${encodeURIComponent(selectedProfileId)}`;
      await ajaxGetAsync(url)
        .then(async _ => {
          toastr.success($("#cardRestrictionProfileDeletedSuccessfully").val())
          await loadCardRestrictions();
        })
        .catch(toastrError);
    }
  });
}

let copyRestrictionProfile = () => {
  const url = `CardRestrictionProfile?profileId=${encodeURIComponent(selectedProfileId)}&ActionMode=${copy}`;
  safelyNavigateTo(url);
}

let goToProductRestrictionPage = () => {
  const url = `/CardProductRestriction?profileId=${encodeURIComponent(selectedProfileId)}&profileName=${encodeURIComponent(selectedProfileName)}`;
  safelyNavigateTo(url);
}
