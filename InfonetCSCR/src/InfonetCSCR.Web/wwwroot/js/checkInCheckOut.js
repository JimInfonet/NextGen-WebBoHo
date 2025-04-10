/*:::::::::::::::: Global ::::::::::::::::*/

const STRING_TABLE = CHEKC_IN_CHECK_OUT_STRING_TABLE;
let policies = {};
let numRec = 0;
let isValidUser = false;
let isPassChecked = false;
let isCommentNeeded = false;
let checkOutPreviousDay = false;

let currentUsers = [];
const checkedInUserKey = "checkedInUser";

$(async function () {
  await loadPolicies();
  enableButtons(false);
  await loadCurrentUsers().then(() => renderCurrentUsers());

  if (localStorage.getItem(checkedInUserKey)) {
    enableButtons(true);
    await determineInOrOut();
    $("#user-id__input").val(localStorage.getItem(checkedInUserKey));
    $("#btn-verify").text($("#_verified").val()).prop("disabled", true);
    $("#btn-change-user").show();
  }
  else $("#btn-change-user").hide();
});

async function loadPolicies() {
  const url = STRING_TABLE.URL.GET_POLICIES;

  await ajaxPostAsync(url)
    .then(_policies => policies = _policies)
    .catch(toastrError);
}

async function loadCurrentUsers() {
  const url = STRING_TABLE.URL.GET_CURRENT_USERS;

  await ajaxGetAsync(url)
    .then(users => currentUsers = users)
    .catch(toastrError);
}

function renderCurrentUsers() {
  const container = $("#user-list");
  container.empty();

  currentUsers.forEach(user => {
    container.append(`<option>${user.userName} ${user.checkedOut ? "" : " - " + getFormattedDate(new Date(user.shiftDate))}</option>`);
  });
}

function enableButtons(flag) {
  $('#lbl-timesheets').prop('hidden', !flag);
  $('#btn-check-in').prop('hidden', !flag);
  $('#btn-view-current').prop('hidden', !flag);
  $('#btn-view-history').prop('hidden', !flag);

  $('#comment__input-group').hide();
}

async function validateUser() {
  if (policies.haveUserId) {
    const url = `${STRING_TABLE.URL.GET_USER_ID}?userCode=${$("#user-id__input").val()}`;
    let userId = await ajaxGetAsync(url);

    if (userId === null || userId === 0) {
      const userIdIsNotDefinedWarning = $("#_userIdIsNotDefined").val();
      toastr.warning(userIdIsNotDefinedWarning);
      $("#user-id__input").focus().select();
      return false;
    }
  }

  const url = `${STRING_TABLE.URL.IS_VALID_USER}?code=${$("#user-id__input").val()}`;
  await ajaxGetAsync(url)
    .then(_isValidUser => isValidUser = _isValidUser)
    .catch(toastrError);

  if (isValidUser) {
    return true;
  }
  else {
    enableButtons(false);
    const userIdDoesNotExistWarning = $("#_userIdDoesNotExist").val();
    toastr.warning(userIdDoesNotExistWarning);
    $("#user-id__input").focus().select();
    return false;
  }
}

async function validatePassword() {
  if (!isValidUser) {
    enableButtons(false);
    const userIdDoesNotExistWarning = $("#_userIdDoesNotExist").val();
    toastr.warning(userIdDoesNotExistWarning);
    $("#user-id__input").focus().select();
    return false;
  }

  let isAuthenticated = false;
  const url = STRING_TABLE.URL.AUTHENTICATE_USER;
  const params = { code: $("#user-id__input").val(), password: $("#password__input").val() };

  await ajaxPostAsync(url, params)
    .then(_isAuthenticated => isAuthenticated = _isAuthenticated)
    .catch(toastrError);

  if ($("#password__input").val() === "" || !isAuthenticated) {
    enableButtons(false);
    const invalidPasswordWarning = $("#_invalidPassword").val();
    toastr.warning(invalidPasswordWarning);
    $("#password__input").focus().select();
    return false;
  }
  else {
    enableButtons(true);
    $("#btn-view-current").prop("disabled", false).focus();
    return true;
  }
}

async function verifyUser(e) {
  e.preventDefault();
  if ($("#user-id__input").val() === "") {
    toastr.warning($("#userNameRequired").val());
    $("#user-id__input").focus();
    return;
  }
  else if ($("#password__input").val() === "") {
    toastr.warning($("#passwordRequired").val());
    $("#password__input").focus();
    return;
  }

  if (await validateUser() && await validatePassword()) {
    localStorage.setItem(checkedInUserKey, $("#user-id__input").val());
    $("#btn-change-user").show();
    await determineInOrOut();
    $("#btn-verify").text($("#_verified").val()).prop("disabled", true);
  }
  else {
    $("#btn-verify").text($("#_verify").val()).prop("disabled", false);
  }
}

// triggers when comment field lost focus
async function addComment() {
  if (!isCommentNeeded || $("#comment__input").val().length === 0) return;

  const url = STRING_TABLE.URL.ADD_COMMENT;
  const params = {
    userCode: localStorage.getItem(checkedInUserKey),
    checkOutPreviousDay: checkOutPreviousDay,
    comment: $("#comment__input").val()
  };

  await ajaxPostAsync(url, params)
    .then(async () => {
      let message = $("#_checkOutSuccessMessage").val().replace("_DATETIME_", new Date().toLocaleString());
      toastr.success(message);

      checkOutPreviousDay = false;
      isCommentNeeded = false;
      $('#comment__input-group').hide();
      $("#comment__input").val("");
      await determineInOrOut();
      await loadCurrentUsers().then(() => renderCurrentUsers());
    })
    .catch(toastrError);
}

async function determineInOrOut() {
  let params = new URLSearchParams();
  params.append(STRING_TABLE.PARAMS.POLICY_HOUR24STORE, policies.hour24Store);
  params.append(STRING_TABLE.PARAMS.USER_CODE, localStorage.getItem(checkedInUserKey));

  let urlWithParams = `${STRING_TABLE.URL.DETERMINE_IN_OR_OUT}?${params.toString()}`;

  await ajaxGetAsync(urlWithParams)
    .then(details => {
      numRec = details.numRec;
      checkOutPreviousDay = details.checkOutPreviousDay;

      if (details.checkedOut) $("#btn-check-in").text($("#_checkIn").val());
      else $("#btn-check-in").text($("#_checkOut").val());
    })
    .catch(toastrError);
}

function handleChangeUser(e) {
  localStorage.removeItem(checkedInUserKey);
  enableButtons(false);
  $("#btn-verify").text($("#_verify").val()).prop("disabled", false);
  $("#btn-change-user").hide();

  $("#user-id__input").val("");
  $("#password__input").val("");
  $("#user-id__input").focus();
}

async function handleCheckIn() {
  if (isCommentNeeded) {
    addComment();
    return;
  }

  let isCheckIn = false;

  if ($("#btn-check-in").text() === $("#_checkIn").val()) {
    isCheckIn = true;

    let title = $("#_checkIn").val();
    let question = $("#_checkInConfirmation").val();
    showConfirmModal(title, question).then(async (confirmed) => {
      if (confirmed) {
        let params = new URLSearchParams();
        params.append(STRING_TABLE.PARAMS.CODE, localStorage.getItem(checkedInUserKey));
        let urlWithParams = `${STRING_TABLE.URL.GET_USERNAME_BY_CODE}?${params.toString()}`;
        let username = localStorage.getItem(checkedInUserKey);

        await $.ajax({
          type: 'GET',
          url: urlWithParams,
          dataType: 'text',
          contentType: "application/json; charset=utf-8",
          success: async (_username) => {
            username = _username ?? username;
            if (policies.version !== "MVCC") {
              const question = $("#_agreementConfirmation").val().replace("_USERNAME_", username);
              const title = $("#_checkIn").val();

              setTimeout(async () => {
                showConfirmModal(title, question).then(async (result) => {
                  if (result)
                    await checkIn(isCheckIn);
                });
              }, 500);
            }
            else await checkIn(isCheckIn);
          },
          error: toastrError
        });
      };
    });
  }
  else {
    isCheckIn = false;
    let title = $("#_checkOut").val();
    let question = $("#_checkOutConfirmation").val();

    showConfirmModal(title, question).then(async (result) => {
      if (result)
        await checkIn(isCheckIn);
    });
  }
}

async function checkIn(isCheckIn) {
  const params = {
    policies,
    numRec,
    userCode: localStorage.getItem(checkedInUserKey),
    commentFieldValue: $('#comment__input').val(),
    checkOutPreviousDay,
    isCheckIn
  }

  await ajaxPostAsync(STRING_TABLE.URL.PERFORM_CHECK_OR_OUT, params)
    .then(async res => {
      if (res.success) {
        let message;
        if (res.successMessage === "CheckInSuccessMessage") {
          message = $("#_checkInSuccessMessage").val().replace("_DATETIME_", new Date().toLocaleString());
        }
        else if (res.successMessage === "CheckOutSuccessMessage") {
          message = $("#_checkOutSuccessMessage").val().replace("_DATETIME_", new Date().toLocaleString());
        }
        toastr.success(message);
        await determineInOrOut();
        await loadCurrentUsers().then(() => renderCurrentUsers());
      }
      else {
        if (res.commentNeeded) {
          isCommentNeeded = true;
          $('#comment__input-group').show();
          $('#comment__input').focus();
          const message = $("#_enterExplanationMessage").val();
          toastr.warning(message);
        }
        else if (res.warningMessage !== null) {
          const message = $("#_enterPayPeriodInformationMessage").val();
          toastr.warning(message);
        }
        else {
          const message = $("#_checkInOrOutUnsuccessfulMessage").val();
          toastr.error(message);
        }
      }
    })
    .catch(toastrError);
}

function handleViewReports(isViewCurrent) {
  const url = `/TimesheetReports?userCode=${localStorage.getItem(checkedInUserKey)}&isViewCurrent=${isViewCurrent}`;
  safelyNavigateTo(url);
}
