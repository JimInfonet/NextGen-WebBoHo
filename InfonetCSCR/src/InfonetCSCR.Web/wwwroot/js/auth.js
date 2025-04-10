const RETURN_URL_PARAM = "ReturnUrl";
const LOGOUT_URL = "/Auth/Logout";
let returnUrl = "/";

$(function () {
  const urlParams = new URLSearchParams(window.location.search);
  returnUrl = urlParams.get(RETURN_URL_PARAM);

  if (isNullUndefinedOrWhiteSpace(returnUrl) || returnUrl === LOGOUT_URL) {
    returnUrl = "/";
  }

  $("#errorMessage").hide();
  $("#errorMessageCode").hide();
  $("#errorMessagePassword").hide();
})

let submitLoginForm = async (e) => {
  e.preventDefault();
  const code = $("#codeInputControl").val();
  const password = $("#passwordInputControl").val();

  if (!validateLoginForm(code, password)) {
    return;
  }

  const data = {
    code: code,
    password: password
  }

  const url = `Auth/Login`;
  await ajaxPostAsync(url, data)
    .then((response) => {
      if (response.status === "success") {
        window.location.href = returnUrl;
        localStorage.clear()
      }
      else {
        showErrorMessage(response.message.value);
        $("#codeInputControl").val("");
        $("#passwordInputControl").val("");
      }
    })
    .catch((error) => {
      showErrorMessage("Some error occured.");
      $("#codeInputControl").val("");
      $("#passwordInputControl").val("");
    })
};

let showErrorMessage = (message) => {
  $("#errorMessage").show();
  $("#errorMessage").text(message);
}

let validateLoginForm = (code, password) => {
  $("#errorMessage").hide();
  $("#errorMessageCode").hide();
  $("#errorMessagePassword").hide();

  if (code === undefined || code === null || code === "") {
    $("#errorMessageCode").show();
    return false;
  }

  if (password === undefined || password === null || password === "") {
    $("#errorMessagePassword").show();
    return false;
  }

  return true;
}