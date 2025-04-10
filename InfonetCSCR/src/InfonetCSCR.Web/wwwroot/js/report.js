window.addEventListener('message', function (event) {
  if (event?.origin !== document.getElementById("webBaseUrl").value) return;
  if (!event || !event.data || !event.data.reportName) return;

  let result = event.data;
  let iFrame = document.getElementById('IframeReportViewer');
  iFrame.height = event.data.reportHeight;
  iFrame.width = event.data.reportWidth;
  iFrame.contentWindow.postMessage(result, document.getElementById("reportBaseUrl").value);
});