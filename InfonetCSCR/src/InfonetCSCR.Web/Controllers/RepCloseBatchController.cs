using InfonetCSCR.Core.Dtos;
using System.Data;
//using LocalReport = Microsoft.Reporting.WebForms.LocalReport;
using AspNetCore.Reporting;
using System.IO;
using InfonetCSCR.Web.Models;

namespace InfonetCSCR.Web.Controllers
{
    public class RepCloseBatchController : Controller
    {
        private readonly ILogger<RepCloseBatchController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPolicyService _policyService;
        private readonly IApplicationConfig _applicationConfig;
        private readonly IRepCloseBatchService _repCloseBatchService;
        private readonly IHttpService _http;

        public RepCloseBatchController(ILogger<RepCloseBatchController> logger, IHttpContextAccessor contextAccessor, IPolicyService policyService,
              IApplicationConfig applicationConfig, IRepCloseBatchService repCloseBatchService, IHttpService http)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _policyService = policyService ?? throw new ArgumentNullException(nameof(policyService));
            _applicationConfig = applicationConfig ?? throw new ArgumentNullException(nameof(applicationConfig));
            _repCloseBatchService = repCloseBatchService ?? throw new ArgumentNullException(nameof(repCloseBatchService));
            _http = http ?? throw new ArgumentNullException(nameof(http));
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        }

        public IActionResult Index()
        {
            var policies = _repCloseBatchService.GetPolicies();

            ViewData["CloseBDays"] = policies.CloseBDays;
            return View();
        }
        public async Task<IActionResult> GetRepCloseBatches(string date1, string date2)
        {
            DateTime dDate1 = DateTime.Parse(date1);
            DateTime dDate2 = DateTime.Parse(date2);
            var repCloseBatches = await _repCloseBatchService.GetRepCloseBatches(dDate1, dDate2);

            return Json(repCloseBatches);

        }

        public async Task<string?> GetReportById(string id)
        {
            var report =  await _repCloseBatchService.GetReportById(id);

            return report.ToString();
        }


        public async Task<IActionResult> GenerateReportData([FromBody] GetRepCloseBatchDto model)
        {
            //string path = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "MyReport.rdlc");
            String reportName = "CloseBatchDetailReport";
            string path = "Reports/" + reportName + ".rdlc";
            LocalReport report = new(path);
            //localReport.LoadReportDefinition(new FileStream("Reports/" + reportName + ".rdlc", FileMode.Open, FileAccess.Read));


            //var data = GetMyData(); // Fetch your report data
            DataTable reportData = await _repCloseBatchService.GenerateReportData(model.BatchID, model.BatchNumber, model.TerminalId);
            //reportData = await _repCloseBatchService.GenerateReportData(model.BatchID, model.BatchNumber, model.TerminalId);

            report.AddDataSource("MyDataSet", reportData);

            var result = report.Execute(RenderType.Pdf, 1, null, "");
            
            return File(result.MainStream, "application/pdf", "MyReport.pdf");

            //DataTable reportData;
            //try
            //{
            //    //byte[] reportData = await _repCloseBatchService.GenerateReportData(model);
            //    reportData = await _repCloseBatchService.GenerateReportData(model.BatchID, model.BatchNumber, model.TerminalId);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(message: ex.Message);
            //    return StatusCode((int)HttpStatusCode.InternalServerError);
            //}

            //// Load the RDLC report

            //var localReport = new LocalReport();
            ////ReportViewer reportViewer = new ReportViewer();
            //String reportName = "CloseBatchDetailReport";
            //localReport.LoadReportDefinition(new FileStream("Reports/" + reportName + ".rdlc", FileMode.Open, FileAccess.Read));

            //reportViewer.LocalReport.ReportPath = "~/Reports/" + reportName + ".rdlc";
            //reportViewer.LocalReport.DataSources.Clear();
            //List<ReportParameter> paramList = new List<ReportParameter>();
            //var rds = new ReportDataSource();
            //rds.Name = reportName;
            //rds.Value = reportData;
            ////ReportDataSource rdc = new ReportDataSource(reportName, reportData);
            //reportViewer.LocalReport.DataSources.Add(rds);
            //reportViewer.LocalReport.Refresh();
            //reportViewer.SizeToReportContent = true;
            ////List<ReportParameter> paramList = new List<ReportParameter>();
            ////paramList.Add(new ReportParameter("reportName", reportName));
            //reportViewer.LocalReport.SetParameters(paramList);
            //reportViewer.LocalReport.Refresh();

            //ViewBag.RDLCReport = reportViewer;
            //return PartialView("_RDLCView");

            // Render the report as PDF
            //var result = localReport.Render("PDF");

            // Return as a file
            //return File(result, "application/pdf", "Report.pdf");
            //return View();
        }
        public RepCloseBatchPoliciesViewModel GetPolicies()
        {
            RepCloseBatchPoliciesViewModel pol = _repCloseBatchService.GetPolicies();

            return pol;
        }

    }
}
