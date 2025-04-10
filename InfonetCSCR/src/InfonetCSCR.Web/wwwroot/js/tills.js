let policies = {};
let selectedTillRow;
let selectedTill;
let tillsDataTable;
let pumpFeatures;
let maxTill;
let lastTill;
let payPumpCredit;
let payPumpDebit;
let tRec;
let till90 = TILLS.CONSTANT_VALUES.TILL90;
let payPumpTill = TILLS.CONSTANT_VALUES.PAYPUMPTILL;
let trainFirstTill = TILLS.CONSTANT_VALUES.TRAINFIRSTTILL;
let trainLastTill = TILLS.CONSTANT_VALUES.TRAINLASTTILL;
let tillsData = [];
let reformedTillsData = []; // Reformed data that will show in the UI only

$(async function () {
  await loadPolicies();
  await loadPumpFeatures();

  let tmpPumpFeatures;

  if (pumpFeatures.length > 16) {
    tmpPumpFeatures = pumpFeatures.substring(0, 16);
  } else if (pumpFeatures.length < 16) {
    tmpPumpFeatures = pumpFeatures + "0".repeat(16 - pumpFeatures.length);
  } else {
    tmpPumpFeatures = pumpFeatures;
  }

  pumpFeatures = tmpPumpFeatures;
  payPumpCredit = tmpPumpFeatures.substring(1, 2) === "0" ? false : true;
  payPumpDebit = tmpPumpFeatures.substring(2, 3) === "0" ? false : true;

  await loadTillsData();
});

async function loadPolicies() {
  const url = "Tills/GetPolicies";
  await ajaxGetAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

async function loadPumpFeatures() {
  let url = `Tills/GetPumpFeatures`;
  await ajaxGetAsync(url)
    .then(_pumpFeatures => pumpFeatures = _pumpFeatures)
    .catch(toastrError);
}


let loadTillsData = async () => {
  if (payPumpCredit || payPumpDebit) {
    maxTill = 100;
  } else {
    maxTill = 99;
    $("#payAtThePumpDiv").prop('hidden', true);
    
    $("#trainingModeDiv").removeClass('col-md-6');
    $("#trainingModeDiv").removeClass('col-xl-3');
    $("#changeStatusDiv").removeClass('col-md-6');
    $("#changeStatusDiv").removeClass('col-xl-5');

    $("#trainingModeDiv").addClass('col-md-12');
    $("#trainingModeDiv").addClass('col-xl-6');
    $("#changeStatusDiv").addClass('col-md-12');
    $("#changeStatusDiv").addClass('col-xl-6');
  }

  let maxTillWithin90 = await loadMaxTill(till90);
  lastTill = (maxTillWithin90 ? maxTillWithin90 : 0);

  let url = `Tills/GetTills?maxTill=${maxTill}`;
  await ajaxGetAsync(url)
    .then((_tillsData) => {
      if (_tillsData) {
        tillsData = _tillsData;
        reformedTillsData = JSON.parse(JSON.stringify(tillsData));

        dataReFormation(); // datetime extraction and reformation, bool data reformation, null data reformation

        $('#btn-delete').prop('disabled', (tillsData.length === 0));
        $('#flip-active-btn').prop('disabled', (tillsData.length === 0));
        $('#flip-processing-btn').prop('disabled', (tillsData.length === 0));

        if (tillsData.length > 0) {
          selectedTill = reformedTillsData[0].tillNum;
          selectedTillRow = reformedTillsData[0];
        }

        createTillsDataTable();
      }
    })
    .catch(toastrError);
}

let loadMaxTill = async (tillNo) => {
  let url = `Tills/GetMaxTill?tillNo=${tillNo}`;
  let mTill;

  await ajaxGetAsync(url)
    .then(_mTill => mTill = _mTill)
    .catch(toastrError);

  return mTill;
}

let createTillsDataTable = () => {
  destroyDataTableIfExists(tillsDataTable);

  let props = createDataTableProp();
  props.data = reformedTillsData;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = false;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "tillNum", },
    { "data": "active", },
    { "data": "processing", },
    { "data": "float", },
    { "data": "cash", },
    { "data": "cashBonusFloat", "visible": (policies.cashBonus) },
    { "data": "cashBonus", "visible": (policies.cashBonus) },
    { "data": "openDate", "visible": (!policies.cashBonus || policies.useShifts) },
    { "data": "openTime", "className": "openTimeData", "visible": (!policies.cashBonus || policies.useShifts) },
    { "data": "shift", "visible": (policies.cashBonus || policies.useShifts) },
    { "data": "shiftDate", "visible": (policies.cashBonus || policies.useShifts) },
    { "data": "userLoggedOn", },
    { "data": "posid", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12] }
  ];

  tillsDataTable = createDataTableGeneric('tills-grid', props);
  
  if (tillsData.length > 0) {
    $(`#tills-grid tbody tr:eq(0)`).addClass('selected-grid-row');
  }

  $('#tills-grid tbody')
    .off()
    .on('click', 'tr', function () {
      if (tillsData.length === 0) {
        return;
      }

      if (!$(this).hasClass('selected-grid-row')) {
        tillsDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedTill = $(this).find("td:first")[0].innerText;
        selectedTillRow = tillsDataTable.row(this).data();
      }
    });
}

let dataReFormation = () => {
  for (let till of reformedTillsData) {
    // Number and boolean reformation
    till.active = (till.active ? "Yes" : "No");
    till.processing = (till.processing ? "Yes" : "No");
    till.cashBonusFloat = (till.cashBonusFloat ? till.cashBonus : 0);
    till.cashBonus = (till.cashBonus ? till.cashBonus : 0);
    till.float = (till.float ? till.float : 0);
    till.cash = (till.cash ? till.cash : 0);
    till.shift = (till.shift ? till.shift : 0);
    till.posid = (till.posid ? till.posid : 0);

    // Datetime extraction begins
    if (till.openDate) {
      let openDate = till.openDate.split('T')[0];
      let openDateYear = parseInt(openDate.split('-')[0], 10);
      let openDateMonth = parseInt(openDate.split('-')[1], 10);
      let openDateDay = parseInt(openDate.split('-')[2], 10);
      till.openDate = `${openDateMonth}/${openDateDay}/${openDateYear}`;
    }

    if (till.openTime) {
      let openTime = till.openTime.split('T')[1];
      let openTimeHour = parseInt(openTime.split(':')[0], 10);
      let openTimeMinute = openTime.split(':')[1];
      let openTimeSecond = openTime.split(':')[2];
      // Format the time in 12 hour 
      openTimeHour = (openTimeHour === 0 ? 24 : openTimeHour);
      const period = openTimeHour < 12 || openTimeHour === 24 ? "AM" : "PM";
      const adjustedHour = openTimeHour % 12 || 12;
      till.openTime = `${adjustedHour}:${openTimeMinute}:${openTimeSecond} ${period}`;
    }

    if (till.shiftDate) {
      let shiftDate = till.shiftDate.split('T')[0];
      let shiftDateYear = parseInt(shiftDate.split('-')[0], 10);
      let shiftDateMonth = parseInt(shiftDate.split('-')[1], 10);
      let shiftDateDay = parseInt(shiftDate.split('-')[2], 10);
      till.shiftDate = `${shiftDateMonth}/${shiftDateDay}/${shiftDateYear}`;
    }
  }
}

let onAddTillBtnClick = async () => {
  if (lastTill >= 90) {
    toastr.warning($("#tooManyTillsAreDefined").val());
    return;
  }

  $('#btn-add').prop('disabled', true);
  $('#btn-add-spinner').prop('hidden', false);

  lastTill++;
  let addTillData = prepareAddTillData(lastTill, true);
  let url = `Tills/AddTill`;

  await ajaxPostAsync(url, addTillData)
    .then(async () => {
      await loadTillsData();
      $('#btn-add').prop('disabled', false);
      $('#btn-add-spinner').prop('hidden', true);
      toastr.success($("#tillAddedSuccessfully").val().replace("{0}", addTillData.tillNum));
    })
    .catch(toastrError);

}

let onDeleteTillBtnClick = async () => {
  if (selectedTillRow.active === "Yes" || selectedTillRow.processing === "Yes") {
    toastr.warning($("#cannotDeleteOnActiveOrProcessingIsTrue").val());
    return;
  }

  await loadTillCount();
  tRec = (tRec ? tRec : 0);

  if (tRec) {
    toastr.warning($("#cannotDeleteDueToTransactions").val());
    return;
  }

  await deleteTill()
}

let onTrainingModeAddBtnClick = async () => {
  $('#training-mode-add-btn').prop('disabled', true);
  $('#training-mode-add-btn-spinner').prop('hidden', false);

  let mTill = await loadMaxTill(trainLastTill);
  let trainTill = (mTill ? mTill : trainFirstTill - 1);

  if (trainTill >= trainLastTill) {
    $('#training-mode-add-btn').prop('disabled', false);
    $('#training-mode-add-btn-spinner').prop('hidden', true);
    toastr.warning($("#tooManyTillsAreDefined").val());
    return;
  }

  let addTMTillData = prepareAddTillData(trainTill + 1, true);
  let url = `Tills/AddTill`;

  await ajaxPostAsync(url, addTMTillData)
    .then(async () => {
      await loadTillsData();
      $('#training-mode-add-btn').prop('disabled', false);
      $('#training-mode-add-btn-spinner').prop('hidden', true);
      toastr.success($("#trainingModeTillAddedSuccessfully").val().replace("{0}", addTMTillData.tillNum));
    })
    .catch(toastrError);
}

let onPayAtThePumpeAddBtnClick = async () => {
  let isPayPumpTillExist = findPayPumpTillInTillsData();

  if (isPayPumpTillExist) {
    toastr.warning($("#payPumpTillAlreadyExists").val());
    return;
  }

  $('#pay-at-the-pump-add-btn').prop('disabled', true);
  $('#pay-at-the-pump-add-btn-spinner').prop('hidden', false);

  let addPPTillData = prepareAddTillData(payPumpTill, false);
  let url = `Tills/AddTill`;

  await ajaxPostAsync(url, addPPTillData)
    .then(async () => {
      await loadTillsData();
      $('#pay-at-the-pump-add-btn').prop('disabled', false);
      $('#pay-at-the-pump-add-btn-spinner').prop('hidden', true);
      toastr.success($("#payAtThePumpTillAddedSuccessfully").val());
    })
    .catch(toastrError);
}

let onFlipActiveBtnClick = async () => {
  let flipActiveData = tillsData.find(till => till.tillNum === parseInt(selectedTill));
  let url = `Tills/UpdateTillByFlipActiveOrFlipProcess`;

  if (selectedTillRow.active === "Yes") {
    flipActiveData.active = false;
    flipActiveData.processing = false;
    flipActiveData.posid = 0;
  } else {
    flipActiveData.active = true;
    flipActiveData.processing = (selectedTillRow.processing === "Yes" ? true : false);
  }

  await ajaxPostAsync(url, flipActiveData)
    .then((response) => {
      if (response) {
        $('.selected-grid-row td:eq(1)').text(flipActiveData.active ? "Yes" : "No");
        $('.selected-grid-row td:eq(2)').text(flipActiveData.processing ? "Yes" : "No");
        selectedTillRow.active = $('.selected-grid-row td:eq(1)').text();
        selectedTillRow.processing = $('.selected-grid-row td:eq(2)').text();
        toastr.success($("#flipActiveUpdatedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let onFlipProecssingBtnClick = async () => {
  let flipProcessData = tillsData.find(x => x.tillNum === parseInt(selectedTill));
  let url = `Tills/UpdateTillByFlipActiveOrFlipProcess`;

  if (selectedTillRow.processing === "No") {
    flipProcessData.active = true;
    flipProcessData.processing = true;
  } else {
    flipProcessData.processing = false;
    flipProcessData.active = (selectedTillRow.active === "Yes" ? true : false);
  }

  await ajaxPostAsync(url, flipProcessData)
    .then((response) => {
      if (response) {
        $('.selected-grid-row td:eq(1)').text(flipProcessData.active ? "Yes" : "No");
        $('.selected-grid-row td:eq(2)').text(flipProcessData.processing ? "Yes" : "No");
        selectedTillRow.active = $('.selected-grid-row td:eq(1)').text();
        selectedTillRow.processing = $('.selected-grid-row td:eq(2)').text();
        toastr.success($("#flipProcessUpdatedSuccessfully").val());
      }
    })
    .catch(toastrError);
}

let findPayPumpTillInTillsData = () => {
  for (let till of reformedTillsData) {
    if (till.tillNum === payPumpTill) {
      return true;
    }
  }
  return false;
}

let deleteTill = async () => {
  let title = $("#deleteTillTitle").val();
  let message = $("#deleteTillMessage").val();
  message += selectedTill;
  message += "?";

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Tills/DeleteTill?tillNo=${selectedTill}`;
      await ajaxGetAsync(url)
        .then(async () => {
          await loadTillsData();
          toastr.success($("#tillDeletedSuccessfully").val());
        })
        .catch(toastrError);
    }
  });
}

let loadTillCount = async () => {
  let url = `Tills/GetTillsCount?tillNo=${selectedTill}`;
  await ajaxGetAsync(url)
    .then(_tRec => tRec = _tRec)
    .catch(toastrError);
}

let prepareAddTillData = (tillNo, isPol) => {
  const addTillData = {
    tillNum: tillNo,
    active: false,
    processing: false,
    cash: 0.00,
    float: 0.00,
    shift: 0,
    posid: 0
  }

  if (policies.cashBonus && isPol) {
    addTillData.cashBonusFloat = 0.00;
    addTillData.cashBonus = 0.00;
  }

  return addTillData;
}
