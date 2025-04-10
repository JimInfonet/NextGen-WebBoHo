const STRING_TABLE = REGISTERS_STRING_TABLE;
let printerList;
let deviceList;
let fontList;
let deviceMap = {};
let registers;
let isEditing = false;
let registerMap = {};
let formDataList = [];

$(async function () {
  setGlobalEditingState(false);
  toggleFields(false);
  $('#registers-select').prop('disabled', false);

  await loadRegistersDropdown();
  await loadCustomerDisplayDropdown();
  await loadPrinters();
  await loadDevices();
  await loadFonts();
  loadCashDrawerCodeDropdown();
  loadFontDropdown();
  loadFontSize();
  await loadRegisters();
});

let loadPrinters = async () => {
  const url = `Printer/GetPrinters`;
  await ajaxGetAsync(url)
    .then(_printerList => printerList = _printerList)
    .catch(toastrError);

  $("#report-printer-driver-select").html(optionGeneratorDefault(""));
  $("#receipt-printer-driver-select").html(optionGeneratorDefault(""));
  $("#label-printer-driver-select").html(optionGeneratorDefault(""));

  printerList.forEach(printer => {
    $("#report-printer-driver-select").append(optionGeneratorForSingleElement(printer));
    $("#receipt-printer-driver-select").append(optionGeneratorForSingleElement(printer));
    $("#label-printer-driver-select").append(optionGeneratorForSingleElement(printer));
  });
}

let loadDevices = async () => {
  const url = `Registers/GetDevices`;
  await ajaxGetAsync(url)
    .then(_deviceList => deviceList = _deviceList)
    .catch(toastrError);

  deviceList.forEach(device => { deviceMap[device.deviceID] = device.deviceName; });
}

let loadFonts = async () => {
  const url = `Printer/GetFonts`;
  await ajaxGetAsync(url)
    .then(_fontList => fontList = _fontList)
    .catch(toastrError);
}

let loadFontDropdown = () => {
  $("#report-printer-font-select").html(optionGeneratorDefault(""));
  fontList.forEach(font => {
    $("#report-printer-font-select").append(optionGeneratorForSingleElement(font));
  });
}

// Add font sizes in the font size dropdown
let loadFontSize = () => {
  $("#report-printer-font-size-select").html(optionGeneratorDefault(""));
  for (let i = 8; i < 26; i++) {
    $("#report-printer-font-size-select").append(optionGeneratorForSingleElement(i));
  }
}

let populateForm = (clear = false) => {
  deviceList.forEach(device => { fillUpForm(device.deviceName, convertToHyphenated(device.deviceName), clear); });
  useOPOSReceiptPrinterChanged();
  useOPOSLabelPrinterChanged();
  useOPOSScannerChanged();
  useOPOSMSRChanged();
  useOPOSScannerChanged();
  useOPOSCustomerDisplayChanged();
  useOPOSCashDrawerChanged();
  useScaleChanged();
}

let fillUpForm = (device, fieldId, clear = false) => {
  if (clear) {
    $(`#use-${fieldId}`).prop("checked", false);
    $(`#use-opos-${fieldId}`).prop("checked", false);
    $(`#${fieldId}-device`).val("");
    $(`#${fieldId}-port-num`).val("");
    $(`#${fieldId}-port-settings`).val("");

    $(`#${fieldId}-port-num-select option[value=""]`).prop('selected', true);
    $(`#${fieldId}-driver-select option[value=""]`).prop('selected', true);
    $(`#${fieldId}-font-select option[value=""]`).prop('selected', true);
    $(`#${fieldId}-font-size-select option[value=""]`).prop('selected', true);
  }
  else {
    $(`#use-${fieldId}`).prop("checked", registerMap[device]?.active);
    $(`#use-opos-${fieldId}`).prop("checked", registerMap[device]?.useOPOS);
    $(`#${fieldId}-device`).val(registerMap[device]?.deviceName);
    $(`#${fieldId}-port-num`).val(registerMap[device]?.portNum);
    $(`#${fieldId}-port-settings`).val(registerMap[device]?.portSetting);

    $(`#${fieldId}-port-num-select option[value="${registerMap[device]?.portNum}"]`).prop('selected', true);
    $(`#${fieldId}-driver-select option[value="${registerMap[device]?.driverName}"]`).prop('selected', true);
    $(`#${fieldId}-font-select option[value="${registerMap[device]?.fontName ?? ''}"]`).prop('selected', true);
    $(`#${fieldId}-font-size-select option[value="${registerMap[device]?.fontSize ?? ''}"]`).prop('selected', true);
  }
}

let saveButtonClicked = async () => {
  let newRegNo = $("#registers-input").val();
 
  if (isEditing === false) {
    if (isNullUndefinedOrWhiteSpace(newRegNo)) {
      toastr.warning($("#registerCanNotBeEmpty").val());
      return;
    }

    if (newRegNo < 0 || newRegNo > 32767) {
      toastr.warning($("#registerRange").val());
      return;
    }

    const url = `Registers/IsRegisterExists`;
    let alreadyExists = false;
    await ajaxPostAsync(url, newRegNo)
      .then(_alreadyExists => alreadyExists = _alreadyExists)
      .catch(toastrError)

    if (alreadyExists) {
      toastr.warning($("#registerAlreadyExists").val());
      return;
    }
  }

  let register = prepareData();
  isRegisterValid = registerValidation(register);

  if (!isRegisterValid)
    return;

  const url = `Registers/SaveRegister?isUpdate=${isEditing}`;
  await ajaxPostAsync(url, register)
    .then(async _ => {
      toastr.success(isEditing ? $("#registerUpdatedSuccessfully").val() : $("#registerAddedSuccessfully").val());

      $(`#registers-input`).val("");

      if (!isEditing) {
        await loadRegistersDropdown();
        $(`#registers-select option[value="${newRegNo}"]`).prop('selected', true);
        await loadRegisters();
      }

      isEditing = true;
      _globalIsEditing = false;
      setGlobalEditingState(_globalIsEditing);
      toggleFields(false);
      $('#registers-input').prop('hidden', true);
      $('#registers-select').prop('hidden', false);
      $('#registers-select').prop('disabled', false);
    })
    .catch(toastrError)
}

let registerValidation = (register) => {
  isValid = true;

  register.forEach(data => {
    if (data.PortNum && data.PortNum > 255) {
      toastr.warning($("#portNumRange").val());
      isValid = false;
    }
  })
  return isValid;
}

let prepareData = () => {
  formDataList = [];
  deviceList.forEach(device => {
    let fieldId = convertToHyphenated(device.deviceName);
    let newItem = {
      RegNo: parseInt(isEditing ? $("#registers-select").val() : $("#registers-input").val()),
      DeviceID: device.deviceID,
      Active: $(`#use-${fieldId}`)[0]?.checked !== false, // undefined should be true for report printer
      UseOPOS: $(`#use-opos-${fieldId}`)[0]?.checked || false,
      FontName: $(`#${fieldId}-font-select`).val() || null,
      FontSize: parseInt($(`#${fieldId}-font-size-select`).val()) || 10,
      WritePosLog: false
    }

    // OPOS Scale is saved regardless of active or not
    if (newItem.UseOPOS || device.deviceID === 7) {
      newItem.DeviceName = $(`#${fieldId}-device`).val() || null;
    }
    else {
      newItem.PortNum = parseInt($(`#${fieldId}-port-num-select`).val() || $(`#${fieldId}-port-num`).val()) || 0;
      newItem.PortSetting = $(`#${fieldId}-port-settings`) ? $(`#${fieldId}-port-settings`).val() : null;
      newItem.DriverName = $(`#${fieldId}-driver-select`).val();
    }

    formDataList.push(newItem);
  })

  return formDataList;
}

let loadCashDrawerCodeDropdown = () => {
  $("#cash-drawer-port-num-select").append(optionGeneratorForSingleElement(0));
  $("#cash-drawer-port-num-select").append(optionGeneratorForSingleElement(1));
}

let loadRegisters = async () => {
  let regNo = $("#registers-select").val()
  const url = `${STRING_TABLE.URL.GET_REGISTERS_BY_REG_NO}`;
  await ajaxPostAsync(url, regNo)
    .then(_registers => registers = _registers)
    .catch(toastrError);

  registerMap = {};

  if (registers.length) {
    registers.forEach(register => { registerMap[deviceMap[register.deviceID]] = register; })
    populateForm();
  }
}

let loadRegistersDropdown = async () => {
  $("#registers-select").empty();
  const url = `${STRING_TABLE.URL.GET_REGISTERS}`;
  await ajaxGetAsync(url)
    .then((data) => {
      data.forEach(option => $("#registers-select").append(optionGeneratorForSingleElement(option)));

      if (data.length === 0)
        $("#registers-select").append(optionGeneratorDefault(""));
    })
    .catch(toastrError);
}

let loadCustomerDisplayDropdown = async () => {
  $("#customer-display-driver-select").html(optionGeneratorDefault(""));
  const url = `${STRING_TABLE.URL.GET_CUSTOMER_DIPLAY}`;
  await ajaxGetAsync(url)
    .then((data) => {
      data.forEach(option => $("#customer-display-driver-select").append(optionGeneratorForSingleElement(`${option.regdisCode} - ${option.description}`)));
    })
    .catch(toastrError);
}

let useOPOSReceiptPrinterChanged = () => {
  let checked = $("#use-opos-receipt-printer")[0].checked;
  $("#receiptPrinterSelect").prop("hidden", checked);
  $("#useOPOSReceiptPrinterText").prop("hidden", !checked);
}

let useOPOSLabelPrinterChanged = () => {
  let checked = $("#use-opos-label-printer")[0].checked;
  $("#labelPrinterDriverSelect").prop("hidden", checked);
  $("#useOPOSLabelPrinterText").prop("hidden", !checked);
}

let useMSRChanged = () => {
  let checked = $("#use-msr")[0].checked;
  $("#use-opos-msr").prop("checked", checked);
  $("#OPOSMSRDeviceName").prop("hidden", !checked);
}

let useOPOSMSRChanged = () => {
  let checked = $("#use-opos-msr")[0].checked;
  $("#use-msr").prop("checked", checked);
  $("#OPOSMSRDeviceName").prop("hidden", !checked);
}

let useOPOSScannerChanged = () => {
  let checked = $("#use-opos-scanner")[0].checked;
  $("#scannerPortSettings").prop("hidden", checked);
  $("#OPOSScannerDeviceName").prop("hidden", !checked);
}

let useOPOSCustomerDisplayChanged = () => {
  let checked = $("#use-opos-customer-display")[0].checked;
  $("#customerDisplayPortName").prop("hidden", checked);
  $("#OPOSCustomerDisplayDeviceName").prop("hidden", !checked);
}

let useCashDrawerChanged = () => {
  let checked = $("#use-cash-drawer")[0].checked;
  $("#use-opos-cash-drawer").prop("checked", checked);
  useOPOSCashDrawerChanged();
}

let useOPOSCashDrawerChanged = () => {
  let checked = $("#use-opos-cash-drawer")[0].checked;
  $("#cashDrawerOpenCode").prop("hidden", checked);
  $("#OPOSCashDrawerDeviceName").prop("hidden", !checked);
}

let useOPOSScaleChanged = () => {
  let checked = $("#use-scale")[0].checked;
  if (!checked)
    $("#use-scale").prop("checked", !checked);

  useScaleChanged();
}

let useScaleChanged = () => {
  let checked = $("#use-scale")[0].checked;
  let OPOSChecked = $("#use-opos-scale")[0].checked;
  $("#OPOSScaleDeviceName").prop("hidden", !checked && !OPOSChecked);
}

let convertToHyphenated = (text) => {
  return text.replace(/([a-z])([A-Z])/g, '$1-$2').replace(/([A-Z]+)([A-Z][a-z])/g, '$1-$2').toLowerCase();
}

let addButtonClicked = () => {
  isEditing = false;
  _globalIsEditing = true;
  setGlobalEditingState(_globalIsEditing);
  toggleFields(true);
  populateForm(true);
  $('#registers-input').prop('hidden', false);
  $('#registers-select').prop('hidden', true);
}

let deleteButtonClicked = async () => {
  let regNo = $("#registers-select").val()
  let title = $("#deleteRegisterTitle").val();
  let message = $("#deleteRegisterMessage").val().replace("{0}", regNo);

  showConfirmModal(title, message).then(async (result) => {
    if (result === true) {
      let url = `Registers/DeleteRegister`;
      await ajaxPostAsync(url, regNo)
        .then(async _ => {
          toastr.success($("#registerDeletedSuccessfully").val())

          await loadRegistersDropdown();
          await loadRegisters();
        })
        .catch(toastrError);
    }
  });
}

let editButtonClicked = () => {
  isEditing = true;
  _globalIsEditing = true;
  setGlobalEditingState(_globalIsEditing);
  toggleFields(true);
  $('#registers-select').prop('disabled', true);
}

let cancelButtonClicked = async () => {
  isEditing = true;
  _globalIsEditing = false;
  setGlobalEditingState(_globalIsEditing);
  await loadRegisters();
  toggleFields(false);
  $('#registers-input').prop('hidden', true);
  $('#registers-select').prop('hidden', false);
  $('#registers-select').prop('disabled', false);
}

let toggleFields = (enable) => {
  $('.infonet-input').prop('disabled', !enable);
  $('.form-check-input').prop('disabled', !enable);

  $('#btn-save').prop('hidden', !enable);
  $('#btn-cancel').prop('hidden', !enable);
  $('#btn-add').prop('hidden', enable);
  $('#btn-edit').prop('hidden', enable);
  $('#btn-delete').prop('hidden', enable);
}
