let download = async (filename, fileContent) => {
  try {
    let element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(fileContent));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();
    document.body.removeChild(element);
  }
  catch {
    toastr.error(errorMessage);
  }
}

async function exportFileBasedOnConfig(folderPath, fileName, fileContent, fileType) {
  let exported = false;
  let successMessage = `${$("#_successfullyExportedTo").val()} ${folderPath}\\${fileName}`;

  if ($("#_isAutoImportExport").val().toLowerCase() === "true") {
    url = `${GENERAL_STRING_TABLE.URL.SAVE_FILE}?fileName=${fileName}&fileType=${fileType}`;

    await ajaxPostAsync(url, fileContent)
      .then(() => {
        toastr.success(successMessage);
        exported = true;
      })
      .catch(() => {
        toastr.error($("#_anErrorHasOccurredWhileExportingFile").val());
        exported = false;
      });
  }
  else {
    await download(fileName, fileContent);
    exported = true;
  }

  return exported;
}

async function saveLogToFileBasedOnConfigAsync(logFolderPath, logFileName, logText) {
  let exported = false;
  let successMessage = `${$("#_lclLogAddSuccessMessage").val()} ${logFolderPath}\\${logFileName}.log`;

  if ($("#_isAutoImportExport").val().toLowerCase() === "true") {
    let data = {
      LogFolderPath: logFolderPath,
      LogFileName: logFileName,
      LogText: logText
    }

    await ajaxPostAsync(GENERAL_STRING_TABLE.URL.SAVE_LOG_WITH_BACKUP, data)
      .then(() => {
        toastr.success(successMessage);
        exported = true;
      })
      .catch(() => {
        toastr.error($("#_lclLogAddErrorMessage").val());
        exported = false;
      });
  }
  else {
    await download(`${logFileName}.log`, logText);
    exported = true;
  }

  return exported;
}
