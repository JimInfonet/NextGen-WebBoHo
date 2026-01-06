using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using System.Drawing.Printing;
//using Excel = Microsoft.Office.Interop.Excel;

namespace InfonetCSCR.Web.Services
{
    public class RepCloseBatchService : IRepCloseBatchService
    {
        private readonly IHttpService _http;
        private readonly ILogger<PromotionalItemsService> _logger;
        private readonly IPolicyService _policy;
        private static StreamReader _streamToPrint;
        private static System.Drawing.Font _printFont;
        private Font printFont;
        private Font verdana10Font;
        StreamReader streamToPrint;

        public RepCloseBatchService(IHttpService http, ILogger<PromotionalItemsService> logger, IPolicyService policy)
        {
            _http = http;
            _logger = logger;
            _policy = policy;
        }
        public RepCloseBatchPoliciesViewModel GetPolicies()
        {
            var policies = new RepCloseBatchPoliciesViewModel
            {
                CloseBDays = _policy.GetPolicy(PolicyName.CLOSEB_DAYS)
            };

            return policies;
        }

        private static void PrintPageHandler(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            string line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height / _printFont.GetHeight(ev.Graphics);

            // Print each line of the file.
            while (count < linesPerPage && ((line = _streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count * _printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, _printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
            {
                ev.HasMorePages = true;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }

        private dynamic GetExcelApplication()
        {
            Process[] processes = Process.GetProcessesByName("EXCEL");
            if (processes.Length > 0)
            {
                // Try to get the running Excel instance
                foreach (Process process in processes)
                {
                    try
                    {
                        return Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
                    }
                    catch (Exception)
                    {
                        // Ignore and try the next process
                    }
                }
            }

            // If no running instance is found, create a new one
            return Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
        }

        private void ReleaseComObject(object obj)
        {
            if (obj != null && Marshal.IsComObject(obj))
            {
                Marshal.ReleaseComObject(obj);
            }
        }


        public async Task<IEnumerable<RepCloseBatchViewModel>> GetRepCloseBatches(DateTime date1, DateTime date2)
        {
            IEnumerable<RepCloseBatchViewModel>? repCloseBatches = null;

            var url = "RepCloseBatch/GetRepCloseBatches?date1=" + date1 + "&date2=" + date2;
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                repCloseBatches = await response.Content.ReadFromJsonAsync<IEnumerable<RepCloseBatchViewModel>>();
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return repCloseBatches;
        }

        public async Task<string?> GetReportById(string id)
        {
            string? report = null;

            var url = "RepCloseBatch/GetReportById?id=" + id;
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                report = await response.Content.ReadAsStringAsync();
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return report;
        }

        //public Policies GetPolicies()
        //{
        //    Policies pol = new()
        //    {
        //        BaseCurr = _policy.GetPolicy(PolicyName.BASECURR),
        //        CBonusName = _policy.GetPolicy(PolicyName.CBONUSNAME),
        //        CBonusTend = _policy.GetPolicy(PolicyName.CBONUSTEND),
        //        CashBonus = _policy.GetPolicy(PolicyName.CASHBONUS),
        //        CcMode = _policy.GetPolicy(PolicyName.CC_MODE),
        //        CombineCr = _policy.GetPolicy(PolicyName.COMBINECR),
        //        CombineFleet = _policy.GetPolicy(PolicyName.COMBINEFLEET),
        //        CountCash = _policy.GetPolicy(PolicyName.COUNT_CASH),
        //        CountType = _policy.GetPolicy(PolicyName.COUNT_TYPE),
        //        CloseBDays = _policy.GetPolicy(PolicyName.CLOSEB_DAYS),
        //        CloseIncluded = _policy.GetPolicy(PolicyName.CLOSE_INCLUD),
        //        EodGroup = _policy.GetPolicy(PolicyName.EOD_GROUP),
        //        PrtTillClCol = _policy.GetPolicy(PolicyName.PRT_TILLCLCOL),
        //        ShowCodes = _policy.GetPolicy(PolicyName.SHOW_CODES),
        //        ShowAllExempt = _policy.GetPolicy(PolicyName.SHOW_ALL_EXEMPT),
        //        Show_NRGT = _policy.GetPolicy(PolicyName.Show_NRGT),
        //        SupportEzi = _policy.GetPolicy(PolicyName.SUPPORTEZI),
        //        TaxRebate = _policy.GetPolicy(PolicyName.TAX_REBATE),
        //        TaxExempt = _policy.GetPolicy(PolicyName.TAX_EXEMPT),
        //        Te_Type = _policy.GetPolicy(PolicyName.TE_TYPE),
        //        TidelSafe = _policy.GetPolicy(PolicyName.TIDELSAFE),
        //        TillCDays = _policy.GetPolicy(PolicyName.TILLC_DAYS)
        //    };

        //    return pol;
        //}


        public async Task<DataTable> GenerateReportData(string BatchId, string BatchNumber, string TerminalId)
        {
            var url = "RepCloseBatch/GenerateReportData?BatchId=" + BatchId + "&BatchNumber=" + BatchNumber + "&TerminalId=" + TerminalId;
            var response = await _http.GetAsync(url);
            DataTable data = new();

            if (response.IsSuccessStatusCode)
            {
                //data = await response.Content.ReadFromJsonAsync<DataTable>();
                var json = await response.Content.ReadAsStringAsync();  // Read as JSON string
                data = ConvertJsonToDataTable(json);                    // Convert JSON to DataTable            
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return data;
        }

        //public async Task<DataTable> GenerateReportData(GetRepCloseBatchDto model)
        //{
        //    var url = "RepCloseBatch/GenerateReportData";
        //    DataTable data = new DataTable();

        //    var response = await _http.PostAsync(url, model);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        //data = await response.Content.ReadFromJsonAsync<DataTable>();
        //        var json = await response.Content.ReadAsStringAsync();  // Read as JSON string
        //        data = ConvertJsonToDataTable(json);                    // Convert JSON to DataTable            
        //    }
        //    else
        //    {
        //        _logger.LogError(Convert.ToString(response.StatusCode));

        //        // Handle the error case if the request was not successful
        //        throw new HttpRequestException($"HTTP request failed: {response.StatusCode}");
        //    }

        //    return data;
        //}

        private DataTable ConvertJsonToDataTable(string json) =>
            // Use Newtonsoft.Json for JSON parsing (add via NuGet)
            JsonConvert.DeserializeObject<DataTable>(json);

        public byte[] GetExportToExcel()
        {
            string SourceDirectory = @"C:\inetpub\wwwroot\publish\infonetapi\";
            string TargetDirectory = @"C:\inetpub\wwwroot\publish\Files\";
            string sourceFile = "CloseBatch.txt";
            string destFile = "CloseBatch.xlsx";
            string SourcePath = Path.Combine(SourceDirectory, sourceFile);
            string DestPath = Path.Combine(TargetDirectory, destFile);
            int titleRows = 1;
            byte[] excelFile;

            try
            {
                // Check if the folder already exists
                if (!Directory.Exists(TargetDirectory))
                {
                    // Create the folder
                    Directory.CreateDirectory(TargetDirectory);
                    Console.WriteLine("Folder created successfully at: " + TargetDirectory);
                }
                else
                {
                    Console.WriteLine("Folder already exists at: " + TargetDirectory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            bool exportSuccess = false;
            try
            {
                // Ensure the EPPlus license context is set (NonCommercial for this example)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                using var reader = new StreamReader(SourcePath);
                string line;
                int rowNo = 1;
                int colNo = 1;

                while ((line = reader.ReadLine()) != null)
                {
                    if (rowNo <= titleRows || (line.ToUpper() == line && !int.TryParse(line[^6..], out _) && line != new string('_', 40)))
                    {
                        var range = worksheet.Cells[$"A{rowNo}:E{rowNo}"];
                        range.Merge = true;
                        worksheet.Cells[rowNo, colNo].Value = line;
                    }
                    else if (line.StartsWith(new string('-', 10)) || line.StartsWith(new string('_', 10)) || line == new string('_', 40))
                    {
                        var range = worksheet.Cells[$"A{rowNo}:E{rowNo}"];
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        range.Style.Border.Bottom.Color.SetColor(Color.Black);
                    }
                    else
                    {
                        int posInLine = 1;
                        string strWrite = "";
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (posInLine > line.Length)
                                break;

                            char strChar = line[posInLine - 1];
                            bool isNumeric = char.IsDigit(strChar);
                            char nextChar = (posInLine < line.Length) ? line[posInLine] : ' ';
                            strWrite += strChar;
                            posInLine++;

                            bool nextCol = (strChar == ' ' && nextChar == ' ') || (posInLine >= 33 && (nextChar == ' ' || (strChar == ' ' && char.IsDigit(nextChar))));

                            if (posInLine == line.TrimEnd().Length && !isNumeric && line.Length < 30)
                            {
                                var range = worksheet.Cells[$"A{rowNo}:E{rowNo}"];
                                range.Merge = true;
                                worksheet.Cells[rowNo, colNo].Value = strWrite + nextChar;
                                strWrite = "";
                                i++;
                            }
                            else if (nextCol)
                            {
                                if (posInLine == line.Length + 1 && strWrite.Length <= 10)
                                {
                                    colNo = 5;
                                }
                                else if (posInLine >= 20 && posInLine < 30 && strWrite.Length <= 11)
                                {
                                    colNo = 3;
                                }

                                worksheet.Cells[rowNo, colNo].Value = strWrite;
                                colNo++;
                                strWrite = "";

                                if (strChar == ' ' && nextChar == ' ')
                                {
                                    while (posInLine + 1 < line.Length && line[posInLine - 1] == ' ')
                                    {
                                        posInLine++;
                                    }
                                }
                            }
                        }
                    }

                    rowNo++;
                    colNo = 1;
                }

                worksheet.Cells.AutoFitColumns();
                //package.SaveAs(new FileInfo(DestPath));
                exportSuccess = true;

                excelFile = package.GetAsByteArray();

                return excelFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                excelFile = null;
            }

            return excelFile;
        }

    }
}
