let productRestrictionDatatable;
let productRestrictions;
let selectedProductRestriction;
let profileId;
let profileName;
let bankCardId = '';
let cardDescription = '';
let cardCode = '';
let tendCodeData = '';
let fromGrid = '';
let action = '';
const URL_PRODUCT_RESTRICTION = '/CardProductRestriction';

$(async function () {
  const urlParams = new URLSearchParams(window.location.search);
  profileId = urlParams.get('profileId');
  profileName = urlParams.get('profileName');
  let isShowToaster = urlParams.get('ShowToaster');
  action = urlParams.get('actionMode');

  selectedProductRestriction = null;
  if ($("#isProductRestrictionDeleted").val() === "True") {
    toastr.error($("#failedToDeleteProductRestriction").val());
  }

  const proRestrictoinFor = profileId ? $('#productRestrictionForProfile').val() : $('#productRestrictionFor').val();
  const bankCardId = $('#bankCardId').val();

  await loadProcductRestrictions($("#bankCardId").val());
  await createProductRestrictionsDatatable();

  if (isShowToaster == "Add" || isShowToaster == "Edit" || isShowToaster == "Delete") {

    if (isShowToaster == "Add") toastr.success($("#productRestrictionProfileCreatedSuccessfully").val());
    else if(isShowToaster == "Edit") toastr.success($("#productRestrictionProfileUpdatedSuccessfully").val());
    else toastr.success($("#productRestrictionProfileDeletedSuccessfully").val());

    let newUrl = removeURLParameter(window.location.search, 'ShowToaster');
    window.history.replaceState(null, null, newUrl);
  }

  if (profileId) {
    $('#productRestrictionForHeader').text(`${proRestrictoinFor} ${profileId}`);
  }
  else {
    $('#productRestrictionForHeader').text(`${proRestrictoinFor} ${bankCardId}`);
  }
});

let createProductRestrictionsDatatable = async () => {
  if (productRestrictionDatatable) {
    productRestrictionDatatable.state.clear();
    productRestrictionDatatable.clear();
    productRestrictionDatatable.destroy();
  }

  let props = createDataTableProp();
  props.data = productRestrictions;
  props.searching = false;
  props.paging = false;
  props.scrollY = '440px';
  props.bInfo = false;
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9] }
  ];
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "bankCardId", },
    { "data": "cardDescription", },
    { "data": "cardProductCode", },
    { "data": "cardProductDescription", },
    { "data": "dept", },
    { "data": "subDept", },
    { "data": "subDetail", },
    { "data": "stockCode", },
    { "data": "description", },
    { "data": "amount", },
  ];

  if (profileId) {
    props.columnDefs = [
      { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6] }
    ];

    props.columns = [
      { "data": "profileId", },
      { "data": "cardDescription", },
      { "data": "dept", },
      { "data": "subDept", },
      { "data": "subDetail", },
      { "data": "stockCode", },
      { "data": "description", },
    ];
  }

  productRestrictionDatatable = createDataTableGeneric('productRestriction-grid', props)

  $('#productRestriction-grid tbody')
    .off() //turn off the event before turning it on to prevent firing click event multiple times
    .on('click', 'tr', function () {
      if ($(this).hasClass('selected-grid-row')) {
        $(this).removeClass('selected-grid-row');
        selectedProductRestriction = null;
      } else {
        productRestrictionDatatable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');

        if (profileId) {
          selectedProductRestriction = {
            "profileId": $(this).find("td")[0].innerText,
            "cardDescription": $(this).find("td")[1].innerText,
            "dept": $(this).find("td")[2].innerText,
            "subDept": $(this).find("td")[3].innerText,
            "subDetail": $(this).find("td")[4].innerText,
            "stockCode": $(this).find("td")[5].innerText,
            "description": $(this).find("td")[6].innerText,
          }
        }
        else {
          selectedProductRestriction = {
            "bankCardId": $(this).find("td")[0].innerText,
            "cardDescription": $(this).find("td")[1].innerText,
            "cardProductCode": $(this).find("td")[2].innerText,
            "cardProductDescription": $(this).find("td")[3].innerText,
            "dept": $(this).find("td")[4].innerText,
            "subDept": $(this).find("td")[5].innerText,
            "subDetail": $(this).find("td")[6].innerText,
            "stockCode": $(this).find("td")[7].innerText,
            "description": $(this).find("td")[8].innerText,
            "amount": $(this).find("td")[9].innerText,
            "cardNumber": $(this).find("td")[10]?.innerText ?? null,
            "profileId": $(this).find("td")[11]?.innerText ?? null,
          }
        }
      }
    });

  $('#button').on('click', function () {
    productRestrictionDatatable.row('.selected-grid-row').remove().draw(false);
  });
}

let loadProcductRestrictions = async (bankCardId) => {
  let url = `${URL_PRODUCT_RESTRICTION}/GetProductRestrictions?bankCardId=${bankCardId}`;
  if (profileId) url = `${URL_PRODUCT_RESTRICTION}/GetProductRestrictions?profileId=${encodeURIComponent(profileId)}`;

  await ajaxGetAsync(url, {})
    .then((response) => {
      if (!isNullOrUndefined(response)) {
        productRestrictions = response;
        createProductRestrictionsDatatable();
      }
      else {
        toastr.error(`${document.getElementById("failedToLoadRegionsStockPrice").value}.`);
      }
    })
    .catch(() => toastr.error($("#failedToLoadProductRestrictions").val()));
}

// On Add button click
$('#btn-add').on('click', function () {
  bankCardId = $('#bankCardId').val();
  cardDescription = $('#cardDescription').val();
  cardCode = $("#cardCode").val();
  tendCodeData = $("#tendCodeData").val();
  fromGrid = $("#fromGrid").val();

  let params = new URLSearchParams();
  params.append("bankCardId",bankCardId);
  params.append("cardDescription", cardDescription);
  params.append("cardCode", cardCode);
  params.append("tendCodeData", tendCodeData);
  params.append("fromGrid", fromGrid);
  params.append("actionMode",action);
  params.append("mode","Add");

  let url = `${URL_PRODUCT_RESTRICTION}/ProductRestrictionForm?${params.toString()}`;

  if (profileId) {
    let params = new URLSearchParams();
    params.append("profileId", profileId);
    params.append("profileName", profileName);
    params.append("mode", "Add");
    url = `${URL_PRODUCT_RESTRICTION}/ProductRestrictionForm?${params.toString()}`;
  }
  safelyNavigateTo(url);
});

// On Delete button click
$('#btn-delete').on('click', function () {
  if (selectedProductRestriction == null) {
    toastr.warning($("#pleaseSelectAProductRestriction").val());
    return;
  }

  $("#confirmModalTitleId").text($("#deleteProductRestriction").val());
  $("#confirmModalMessageId").text($("#areYouSureYouWantToDeleteThisCard").val());
  $("#confirmModalId").modal('show');
});

// On modal yes button click
$('#confirmModalYesButtonId').on('click', function () {
  $('#productRestrictionForm').attr('action', '/CardProductRestriction/DeleteProductRestriction');
  FillProductRestrictionForm();
  $('#submitForm').trigger('click');
});

// On Edit button click
$("#btn-edit").on('click', function () {
  if (selectedProductRestriction != null) {
    FillProductRestrictionForm();
    $('#submitForm').trigger('click');
  }
  else {
    toastr.warning($("#pleaseSelectAProductRestriction").val());
  }
})

// On Back button click
$("#btn-back").on('click', function () {
  cardCode = $("#cardCode").val();
  tendCodeData = $("#tendCodeData").val();
  fromGrid = $("#fromGrid").val();
  
  if (action == null || action == "") {
    action = "update";
  }

  let params = new URLSearchParams();
  params.append("cardCode", cardCode);
  params.append("tendCodeData", tendCodeData);
  params.append("fromGrid", fromGrid);
  params.append("actionMode", action);

  let url = `/CardDefinition?${params.toString()}`;
  if (profileId) url = "CardRestrictionProfileGrid";

  safelyNavigateTo(url);
})

let FillProductRestrictionForm = () => {
  if (!selectedProductRestriction) return;
  $('#bankCardIdForm').val(selectedProductRestriction.bankCardId);
  $('#cardDescriptionForm').val(selectedProductRestriction.cardDescription);
  $('#cardProductCodeForm').val(selectedProductRestriction.cardProductCode);
  $('#cardProductDescriptionForm').val(selectedProductRestriction.cardProductDescription);
  $('#departmentForm').val(selectedProductRestriction.dept);
  $('#subDeptForm').val(selectedProductRestriction.subDept);
  $('#subDetailForm').val(selectedProductRestriction.subDetail);
  $('#stockCodeForm').val(selectedProductRestriction.stockCode);
  $('#descriptionForm').val(selectedProductRestriction.description);
  $('#amountForm').val(selectedProductRestriction.amount);
  $('#cardNumberForm').val(selectedProductRestriction.cardNumber);
  $('#profileIdForm').val(selectedProductRestriction.profileId);
  if (profileId) $('#cardDescriptionForm').val(profileName); 
  loadSelectFromModel(selectedProductRestriction.dept, "department");
}

let loadSelectFromModel = (inputData, selectId) => {
  if (inputData) {
    $(`#${selectId} option[value=${inputData}]`).attr('selected', 'selected');
  }
}
