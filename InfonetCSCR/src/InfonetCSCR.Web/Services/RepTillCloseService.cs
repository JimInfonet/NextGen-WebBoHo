using InfonetCSCR.Core.Dtos;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Drawing.Printing;
using LicenseContext = OfficeOpenXml.LicenseContext;
//using Excel = Microsoft.Office.Interop.Excel;

namespace InfonetCSCR.Web.Services
{
    public class RepTillCloseService : IRepTillCloseService
    {
        private readonly IHttpService _http;
        private readonly ILogger<PromotionalItemsService> _logger;
        private readonly IPolicyService _policy;
        private static StreamReader _streamToPrint;
        private static System.Drawing.Font _printFont;
        private Font printFont;
        private Font verdana10Font;
        StreamReader streamToPrint;

        public RepTillCloseService(IHttpService http, ILogger<PromotionalItemsService> logger, IPolicyService policy)
        {
            this._http = http;
            this._logger = logger;
            this._policy = policy;

            // Initialize non-nullable fields with default values  
            this.printFont = new Font("Arial", 10);
            this.verdana10Font = new Font("Verdana", 10);
            this.streamToPrint = new StreamReader(new MemoryStream());
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

        public async Task<IEnumerable<RepTillCloseViewModel>> GetRepTillCloses(DateTime date1, DateTime date2)
        {
            IEnumerable<RepTillCloseViewModel>? repTillCloses = null;

            var url = "RepTillClose/GetRepTillCloses?date1=" + date1 + "&date2=" + date2;
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                repTillCloses = await response.Content.ReadFromJsonAsync<IEnumerable<RepTillCloseViewModel>>();
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return repTillCloses;
        }

        public void printPolicies(GetRepTillCloseDto model)
        {
            _logger.LogInformation($"BaseCurr:{model.policies.BaseCurr}");
            _logger.LogInformation($"CBonusName:{model.policies.CBonusName}");
            _logger.LogInformation($"CBonusTend:{model.policies.CBonusTend}");
            _logger.LogInformation($"CashBonus:{model.policies.CashBonus}");
            _logger.LogInformation($"CcMode:{model.policies.CcMode}");
            _logger.LogInformation($"CombineCr:{model.policies.CombineCr}");
            _logger.LogInformation($"CombineFleet:{model.policies.CombineFleet}");
            _logger.LogInformation($"CountCash:{model.policies.CountCash}");
            _logger.LogInformation($"CountType:{model.policies.CountType}");
            _logger.LogInformation($"CloseIncluded:{model.policies.CloseIncluded}");
            _logger.LogInformation($"EodGroup:{model.policies.EodGroup}");
            _logger.LogInformation($"PrtTillClCol:{model.policies.PrtTillClCol}");
            _logger.LogInformation($"ShowAllExempt:{model.policies.ShowAllExempt}");
            _logger.LogInformation($"SupportEzi:{model.policies.SupportEzi}");
            _logger.LogInformation($"Show_NRGT:{model.policies.Show_NRGT}");
            _logger.LogInformation($"ShowCodes:{model.policies.ShowCodes}");
            _logger.LogInformation($"TaxRebate:{model.policies.TaxRebate}");
            _logger.LogInformation($"TaxExempt:{model.policies.TaxExempt}");
            _logger.LogInformation($"Te_Type:{model.policies.Te_Type}");
            _logger.LogInformation($"TidelSafe:{model.policies.TidelSafe}");
            _logger.LogInformation($"TillCDays:{model.policies.TillCDays}");
            _logger.LogInformation($"UseOvrPymnt:{model.policies.UseOvrPymnt}");
            _logger.LogInformation($"UseShifts:{model.policies.UseShifts}");

        }
        public async Task<IEnumerable<int>> GetShiftNos(DateTime shiftdate)
        {
            IEnumerable<int>? shiftNos = null;

            var url = "RepTillClose/GetShiftNos?ShiftDate=" + shiftdate;
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                shiftNos = await response.Content.ReadFromJsonAsync<IEnumerable<int>>();
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return shiftNos;
        }

        public async Task<String> GetRepByShift(GetRepTillCloseDto model)
        {
            var url = "RepTillClose/GetRepByShift";

            //printPolicies(model);
            var response = await _http.PostAsync(url, model);

            if (response.IsSuccessStatusCode)
            {
                string contentBody = await response.Content.ReadAsStringAsync();

                return contentBody;
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));

                // Handle the error case if the request was not successful
                throw new HttpRequestException($"HTTP request failed: {response.StatusCode}");
            }
        }

        public async Task<String> GetRepByDates(GetRepTillCloseDto model)
        {
            var url = "RepTillClose/GetRepByDates";
            //printPolicies(model);

            var response = await _http.PostAsync(url, model);

            if (response.IsSuccessStatusCode)
            {
                string contentBody = await response.Content.ReadAsStringAsync();

                return contentBody;
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));

                // Handle the error case if the request was not successful
                throw new HttpRequestException($"HTTP request failed: {response.StatusCode}");
            }
        }

        public async Task<String> GetRepByTillNo(GetRepTillCloseDto model)
        {
            var url = "RepTillClose/GetRepByTillNo";
            var response = await _http.PostAsync(url, model);

            if (response.IsSuccessStatusCode)
            {
                string contentBody = await response.Content.ReadAsStringAsync();

                return contentBody;
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));

                // Handle the error case if the request was not successful
                throw new HttpRequestException($"HTTP request failed: {response.StatusCode}");
            }
        }

        public Policies GetPolicies()
        {
            Policies policies = new ()
            {
                BaseCurr = _policy.GetPolicy(PolicyName.BASECURR)
                ,CBonusName = _policy.GetPolicy(PolicyName.CBONUSNAME)
                ,CBonusTend = _policy.GetPolicy(PolicyName.CBONUSTEND)
                ,CashBonus = GetBool(_policy.GetPolicy(PolicyName.CASHBONUS))
                ,CcMode = _policy.GetPolicy(PolicyName.CC_MODE)
                ,CombineFleet = GetBool(_policy.GetPolicy(PolicyName.COMBINEFLEET))
                ,CountCash = GetBool(_policy.GetPolicy(PolicyName.COUNT_CASH))
                ,CountType = _policy.GetPolicy(PolicyName.COUNT_TYPE)
                ,CloseBDays = _policy.GetPolicy(PolicyName.CLOSEB_DAYS)
                ,CloseIncluded = GetBool(_policy.GetPolicy(PolicyName.CLOSE_INCLUD))
                ,EodGroup = GetBool(_policy.GetPolicy(PolicyName.EOD_GROUP))
                ,PrtTillClCol = GetBool(_policy.GetPolicy(PolicyName.PRT_TILLCLCOL))
                ,ShowCodes = GetBool(_policy.GetPolicy(PolicyName.SHOW_CODES))
                ,ShowAllExempt = GetBool(_policy.GetPolicy(PolicyName.SHOW_ALL_EXEMPT))
                ,Show_NRGT = GetBool(_policy.GetPolicy(PolicyName.SHOW_NRGT))
                ,SupportEzi = GetBool(_policy.GetPolicy(PolicyName.SUPPORTEZI))
                ,TaxExempt = GetBool(_policy.GetPolicy(PolicyName.TAX_EXEMPT))
                ,Te_Type = _policy.GetPolicy(PolicyName.TE_TYPE)
                ,TidelSafe = GetBool(_policy.GetPolicy(PolicyName.TIDELSAFE))
                ,TillCDays = _policy.GetPolicy(PolicyName.TILLC_DAYS)
                ,UseShifts = GetBool(_policy.GetPolicy(PolicyName.USE_SHIFTS))
                ,UseOvrPymnt = GetBool(_policy.GetPolicy(PolicyName.USE_OVR_PYMNT))
            };

            return policies;
        }

        public bool GetBool(object? value)
        {
            if (value == null)
                return false;

            if (value is bool b)
                return b;

            var str = value.ToString()?.Trim().ToUpper();

            return str == "YES"
                || str == "TRUE"
                || str == "1";
        }

        //using EPPlus
        public byte[] GetExportToExcel()
        {
            string SourceDirectory = @"C:\inetpub\wwwroot\publish\infonetapi\";
            string TargetDirectory = @"C:\inetpub\wwwroot\publish\Files\";
            string sourceFile = "TillClose.txt";
            string destFile = "TillClose.xlsx";
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

        public IActionResult GetPrintReport(GetRepTillCloseDto model)
        {
            try
            {
                string filePath = @"C:\inetpub\wwwroot\publish\infonetapi\TillClose.txt";

                // Check if the file exists
                if (File.Exists(filePath))
                {
                    streamToPrint = new StreamReader(filePath);

                    PrintDocument printDocument = new();
                    printDocument.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    printFont = new Font("System", 10);
                    printDocument.Print();

                    streamToPrint.Close();
                    return new StatusCodeResult(500);
                }
                else
                {
                    Console.WriteLine("File not found: " + filePath);
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while printing the report.");

                return new StatusCodeResult(500);
            }
        }

        //// The PrintPage event is raised for each page to be printed.
        private void pd_PrintPage(object sender, PrintPageEventArgs ppeArgs)
        {
            //Get the Graphics object  
            Graphics g = ppeArgs.Graphics;
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            //Read margins from PrintPageEventArgs  
            float leftMargin = ppeArgs.MarginBounds.Left;
            float topMargin = ppeArgs.MarginBounds.Top;
            string line = null;
            //Calculate the lines per page on the basis of the height of the page and the height of the font  
            linesPerPage = ppeArgs.MarginBounds.Height / printFont.GetHeight(g);
            //Now read lines one by one, using StreamReader  
            while (count < linesPerPage && ((line = streamToPrint.ReadLine()) != null))
            {
                //Calculate the starting position  
                yPos = topMargin + (count * printFont.GetHeight(g));
                //Draw text  
                g.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                //Move to next line  
                count++;
            }
            //If PrintPageEventArgs has more pages to print  
            if (line != null)
            {
                ppeArgs.HasMorePages = true;
            }
            else
            {
                ppeArgs.HasMorePages = false;
            }
        }

    }
}
