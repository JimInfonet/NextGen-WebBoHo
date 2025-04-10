let storeShiftsData = [];
let reformedStoreShiftsData = [];
let storeShiftEditedList = [];
let selectedStoreShift;
let selectedStoreShiftRow;
let storeShiftsDataTable;
let shiftDayPolicy;
const defaultTime = "1899-12-30T00:00:00";

$(async function () {
  await loadPolicies();

  let language = $("#language").val();
  $('.text-danger').prop('hidden', true);
  let policyNameinFr = (shiftDayPolicy === "Current" ? "Courant" : "Suivant");
  let shiftCurrentDayStatement = $("#shiftCurrentDayStatement").val().replace("{0}", (language === "en" ? shiftDayPolicy : policyNameinFr));
  $("#localizedShiftCurrentDayStatement").text(shiftCurrentDayStatement);

  await loadStoreShiftsData(0);
});

async function loadPolicies() {
  const url = "StoreShifts/GetPolicies";
  await ajaxGetAsync(url)
    .then(_policies => shiftDayPolicy = _policies)
    .catch(toastrError);
}

let loadStoreShiftsData = async (selectedRow) => {
  let url = "StoreShifts/GetStoreShifts";

  await ajaxGetAsync(url)
    .then(_storeShiftsData => {
      if (_storeShiftsData) {
        storeShiftsData = _storeShiftsData;
        reformedStoreShiftsData = JSON.parse(JSON.stringify(storeShiftsData));

        storeShiftDataReFormation(); // boolean and datatime reformation

        $('#btn-delete').prop('disabled', (storeShiftsData.length === 0));

        if (storeShiftsData.length > 0) {
          selectedStoreShift = reformedStoreShiftsData[selectedRow].shiftNumber;
          selectedStoreShiftRow = reformedStoreShiftsData[selectedRow];
        }

        createStoreShiftsDataTable(selectedRow);
        setDataInEditSection();
      }
    })
    .catch(toastrError);
}

let createStoreShiftsDataTable = (alreadySelectedRow) => {
  destroyDataTableIfExists(storeShiftsDataTable);

  let props = createDataTableProp();
  props.data = reformedStoreShiftsData;
  props.scrollY = '440px';
  props.paging = true;
  props.searching = false;
  props.columns = [
    //the first letter of the column name should be lowercase and first letter of the next words is uppercase
    { "data": "shiftNumber", },
    { "data": "startTime", },
    { "data": "activeStatus", },
  ];
  props.columnDefs = [
    { className: 'text-center', targets: [0, 1, 2] }
  ];

  storeShiftsDataTable = createDataTableGeneric('storeShifts-grid', props);

  if (storeShiftsData.length > 0) {
    $(`#storeShifts-grid tbody tr:eq(${alreadySelectedRow})`).addClass('selected-grid-row');
  }

  $('#storeShifts-grid tbody')
    .off()
    .on('click', 'tr', async function () {
      if (storeShiftsData.length === 0) {
        return;
      }
      if (!$(this).hasClass('selected-grid-row')) {
        storeShiftsDataTable.$('tr.selected-grid-row').removeClass('selected-grid-row');
        $(this).addClass('selected-grid-row');
        selectedStoreShift = $(this).find("td:first")[0].innerText;
        selectedStoreShiftRow = storeShiftsDataTable.row(this).data();

        setDataInEditSection();
      }
    });
}

let onDeleteShiftBtnClick = async () => {
  await updateStoreShifts(true);

  if (!activeStatusCheck()) {
    return;
  }

  if (storeShiftsData.length) {
    let title = $("#deleteStoreShiftTitle").val();
    let message = $("#deleteStoreShiftMessage").val() + " ";
    message += selectedStoreShift;
    message += "?";

    showConfirmModal(title, message).then(async (result) => {
      if (result === true) {
        let url = `StoreShifts/DeleteStoreShift?shiftNumber=${selectedStoreShift}`;
        await ajaxGetAsync(url)
          .then(async () => {
            await loadStoreShiftsData(0);
            await reNumberShift();
            toastr.success($("#storeShiftDeletedSuccessfully").val());
          })
          .catch(toastrError);
      }
    });
  } else {
    toastr.warning($("#mustHaveOneShiftDefined").val());
  }
}

let onAddShiftBtnClick = async () => {
  await updateStoreShifts(true);

  let newShift = storeShiftsData.length;
  newShift = (newShift ? newShift + 1 : 1);

  let addNewShiftData = prepareShiftData(newShift);
  let url = `StoreShifts/AddStoreShift`;

  await ajaxPostAsync(url, addNewShiftData)
    .then(async () => {
      toastr.success($("#storeShiftAddedSuccessfully").val());
      await loadStoreShiftsData(0);
    })
    .catch(toastrError);
}

let updateStoreShifts = async (fromEditSection) => {
  let url = `StoreShifts/UpdateStoreShifts?isUpdateAll=${!fromEditSection}`;
  let datas = (fromEditSection ? storeShiftEditedList : storeShiftsData);

  await ajaxPostAsync(url, datas)
    .then(async () => {
      await loadStoreShiftsData(fromEditSection ? parseInt(selectedStoreShift) - 1 : 0);
    })
    .catch(toastrError);
}

let reNumberShift = async () => {
  let newIdx = 1;

  for (let i = 0; i < storeShiftsData.length; i++) {
    storeShiftsData[i].shiftNumber = newIdx++;

    if (i === 0) {
      storeShiftsData[0].currentDay = updateDate();
    }
  }

  await updateStoreShifts(false);
}

let setShiftValues = (isByStartTime) => {
  let updatedData = JSON.parse(JSON.stringify(selectedStoreShiftRow));
  let actualTime = $("#set-or-change-start-time").val();
  updatedData.startTime = `1899-12-30T${actualTime}:00`;
  
  if (isByStartTime) {
    let convertedTime = convert24to12Hour($("#set-or-change-start-time").val());
    storeShiftsDataTable.row($('.selected-grid-row')).data().startTime = convertedTime;
    $('.selected-grid-row td:eq(1)').text(convertedTime);
    updatedData.activeStatus = (updatedData.activeStatus === "Yes" ? true : false);
  } else {
    let isActive = $("#shift-active").is(':checked');

    if (!isActive && !activeStatusCheck()) {
      $("#shift-active").prop('checked', true);
      return;
    } else {
      updatedData.activeStatus = isActive;
      storeShiftsDataTable.row($('.selected-grid-row')).data().activeStatus = (isActive ? "Yes" : "No");
      $('.selected-grid-row td:eq(2)').text(isActive ? "Yes" : "No");
    }
  }

  storeShiftEditedList[storeShiftEditedList.length] = updatedData;
}

let prepareShiftData = (shiftNo) => {
  const addShiftData = {
    shiftNumber: parseInt(shiftNo),
    startTime: defaultTime,
    activeStatus: true,
    currentDay: false
  };

  if (shiftNo === 1) {
    addShiftData.currentDay = updateDate();
    $("#shift-active").prop('checked', true);
  }

  return addShiftData;
}

let updateDate = () => {
  return (shiftDayPolicy === "Current" ? true : false);
}

let storeShiftDataReFormation = () => {
    for (let storeShift of reformedStoreShiftsData) {
    // boolean reformation
    storeShift.activeStatus = (storeShift.activeStatus ? "Yes" : "No");

    // Datetime extraction begins
    if (storeShift.startTime) {
      let splitStartTime = storeShift.startTime.split('T')[1];
      storeShift.startTime = convert24to12Hour(splitStartTime);
    }
  }
}

let setDataInEditSection = () => {
  let setTime = convert12to24Hour(selectedStoreShiftRow.startTime);
  $("#set-or-change-start-time").val(setTime);
  $("#shift-active").prop('checked', selectedStoreShiftRow.activeStatus === "Yes");
}

let activeStatusCheck = () => {
  let isAnyShiftActive;

  for (let i = 0; i < storeShiftsData.length; i++) {
    if (storeShiftsData[i].activeStatus && selectedStoreShiftRow !== reformedStoreShiftsData[i]) {
      isAnyShiftActive = true;
      break;
    }
  }

  if (!isAnyShiftActive) {
    toastr.warning($("#atLeastOneShiftMustBeActive").val());
    return false;
  }

  return true;
}

window.addEventListener('beforeunload', handleBeforeUnload);

async function handleBeforeUnload(event) {
  await updateStoreShifts(true);

  event.preventDefault(); // For modern browsers

  return undefined; // For older browsers
}
