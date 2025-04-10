namespace InfonetCSCR.Web.Services;

public class ExcelReportsService : IExcelReportsService
{
  private readonly IStringLocalizer<ExcelReportsService> _localizer;
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;

  #region private properties
  private IList<string>? uniqueStocks;
  private IList<DateTime>? uniqueShiftDates;
  private CompanyViewModel? company;
  private IList<SaleDataViewModel>? saleData;
  private IList<FamilyViewModel>? families;
  private IList<StockItemViewModel>? stockMst;
  private string timeFormat;
  #endregion

  // Excel Report Column Numbers
  private readonly int A = 0, B = 1, C = 2, D = 3, E = 4, F = 5, G = 6, H = 7, I = 8, J = 9, K = 10, L = 11, M = 12, N = 13, O = 14, P = 15, Q = 16, R = 17;

  public ExcelReportsService(IStringLocalizer<ExcelReportsService> localizer, IHttpService http, IApplicationConfig config, IPolicyService policyService)
  {
    _http = http;
    _localizer = localizer;
    _config = config;
    _policyService = policyService;
  }

  #region Excel Functions

  private static string IntToExcelColumn(int index)
  {
    string column = string.Empty;
    int dividend = index;

    while (dividend >= 0)
    {
      int modulo = dividend % 26;
      column = (char)(modulo + 65) + column;
      dividend = (dividend / 26) - 1;
    }

    return column;
  }

  private static void AddDataToCell(ISheet sheet, int rowIndex, int colIndex, object value, ICellStyle? cellStyle = null)
  {
    IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
    ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);

    switch (value)
    {
      case string s:
        cell.SetCellValue(s);
        break;
      case double d:
        cell.SetCellValue(d);
        break;
      case int i:
        cell.SetCellValue(i);
        break;
      case bool b:
        cell.SetCellValue(b);
        break;
      case DateTime dt:
        cell.SetCellValue(dt);
        break;
      case decimal dec:
        cell.SetCellValue((double)dec);
        break;
      case null:
        cell.SetCellValue(string.Empty);
        break;
    }

    if (cellStyle != null)
    {
      cell.CellStyle = cellStyle;
    }
  }

  private static object GetCellValue(ISheet sheet, int rowIndex, int colIndex)
  {
    IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
    ICell cell = row.GetCell(colIndex) ?? row.CreateCell(colIndex);

    if (cell != null)
    {
      switch (cell.CellType)
      {
        case CellType.String:
          return cell.StringCellValue;

        case CellType.Numeric:
          return cell.NumericCellValue;

        case CellType.Boolean:
          return cell.BooleanCellValue;

        case CellType.Formula:
          return cell.CellFormula;

        default:
          return "";
      }
    }
    else
    {
      return null;
    }
  }

  private static void CreateCellStyles(IWorkbook workbook, ISheet sheet, XSSFCellStyle basestyle, int rowNo, int colNo, object? value = null,
                                       short? fontColor = null, short? bgColor = null, short? fontSize = null, string? fontName = null,
                                       bool? isBold = null, bool? isItalic = null, bool? isUnderline = null, bool? isWrapText = null, bool? isHyperLink = null,
                                       HorizontalAlignment? horizontalAlignment = null, VerticalAlignment? verticalAlignment = null,
                                       BorderStyle? leftBorder = null, BorderStyle? rightBorder = null, BorderStyle? topBorder = null, BorderStyle? bottomBorder = null,
                                       string? dataFormat = null)
  {
    ICell cell = GetCell(sheet, rowNo, colNo);
    AddDataToCell(sheet, rowNo, colNo, value);
    XSSFFont font = (XSSFFont)workbook.CreateFont();
    XSSFCellStyle style = (XSSFCellStyle)basestyle.Clone();

    if (value is decimal || value is int || value is double && horizontalAlignment == null)
    {
      horizontalAlignment = HorizontalAlignment.Right;
    }

    // set default values
    fontColor = fontColor ?? IndexedColors.Black.Index;
    bgColor = bgColor ?? IndexedColors.White.Index;
    fontSize = fontSize ?? 10; // Default font size
    fontName = fontName ?? "Arial";
    isBold = isBold ?? false;
    isItalic = isItalic ?? false;
    isUnderline = isUnderline ?? false;
    isWrapText = isWrapText ?? false;
    isHyperLink = isHyperLink ?? false;
    horizontalAlignment = horizontalAlignment ?? HorizontalAlignment.Left;
    verticalAlignment = verticalAlignment ?? VerticalAlignment.Center;
    leftBorder = leftBorder ?? BorderStyle.Hair;
    rightBorder = rightBorder ?? BorderStyle.Hair;
    topBorder = topBorder ?? BorderStyle.Hair;
    bottomBorder = bottomBorder ?? BorderStyle.Hair;
    dataFormat = dataFormat ?? "General"; // Default data format

    // set font
    font.FontName = fontName;
    font.Color = (short)fontColor;
    font.FontHeightInPoints = (double)fontSize;
    font.IsBold = (bool)isBold;
    font.IsItalic = (bool)isItalic;
    font.Underline = (bool)isUnderline ? FontUnderlineType.Single : FontUnderlineType.None;
    style.SetFont(font);

    // set bg color
    style.FillForegroundColor = (short)bgColor;
    style.FillPattern = FillPattern.SolidForeground;

    // set wrap text
    style.WrapText = (bool)isWrapText;

    // set alignment
    style.Alignment = (HorizontalAlignment)horizontalAlignment;
    style.VerticalAlignment = (VerticalAlignment)verticalAlignment;

    // set borders
    style.BorderBottom = (BorderStyle)bottomBorder;
    style.BorderTop = (BorderStyle)topBorder;
    style.BorderLeft = (BorderStyle)leftBorder;
    style.BorderRight = (BorderStyle)rightBorder;

    style.BottomBorderColor = IndexedColors.Black.Index;
    style.TopBorderColor = IndexedColors.Black.Index;
    style.LeftBorderColor = IndexedColors.Black.Index;
    style.RightBorderColor = IndexedColors.Black.Index;

    // set data format
    IDataFormat dataFormatHelper = workbook.CreateDataFormat();
    style.DataFormat = dataFormatHelper.GetFormat(dataFormat);

    // set hyperlink
    if ((bool)isHyperLink)
    {
      var hyperlink = workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.Email);
      string emailAddress = value?.ToString(); // Convert to string
      hyperlink.Address = $"mailto:{emailAddress}";
      hyperlink.Label = emailAddress;

      // Set the hyperlink to the cell
      cell.Hyperlink = hyperlink;
    }

    cell.CellStyle = style;
  }

  private static void ApplyCellFormula(ISheet sheet, int rowNum, int colNum, string formula)
  {
    ICell cell = GetCell(sheet, rowNum, colNum);
    cell.SetCellFormula(formula);
  }

  private static void ClearRangeContents(ISheet sheet, int startRow, int endRow, int startCol, int endCol)
  {
    for (int rowNum = startRow; rowNum <= endRow; rowNum++)
    {
      IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
      for (int colNum = startCol; colNum <= endCol; colNum++)
      {
        ICell cell = row.GetCell(colNum);
        if (cell != null)
        {
          cell.SetCellValue(string.Empty); // Clear cell content
          cell.CellStyle = sheet.Workbook.CreateCellStyle();
        }
      }
    }
  }

  private static ICell GetCell(ISheet sheet, int rowNum, int colNum)
  {
    IRow row = sheet.GetRow(rowNum) ?? sheet.CreateRow(rowNum);
    ICell cell = row.GetCell(colNum) ?? row.CreateCell(colNum);
    return cell;
  }

  private static FileBase64DataViewModel GetBase64Data(IWorkbook workbook)
  {
    XSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);
    FileBase64DataViewModel fileBase64Data = new();
    using (var stream = new MemoryStream())
    {
      workbook.Write(stream);
      fileBase64Data.ByteArray = stream.ToArray();
    }

    return fileBase64Data;
  }

  #endregion

  #region Public Methods
  public async Task<bool?> CheckSaleDataExists(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string url = "ExcelReports/CheckSaleDataExists";
    bool isSaleDataExists = await _http.PostAndParseAsync<bool>(url, excelReportGeneratorViewModel);
    return isSaleDataExists;
  }
  #endregion

  #region Fuel Daily Sales Report

  public async Task<FileBase64DataViewModel?> FuelDailySalesReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string url = "ExcelReports/GetFuelDailySalesReportData";
    ExcelReportDataDtoViewModel? excelReportData = await _http.PostAndParseAsync<ExcelReportDataDtoViewModel>(url, excelReportGeneratorViewModel);

    string reportName = _localizer.GetString("fuelDailySalesReport"); //"FUEL SALES DAILY REPORT"; // Hard coded in VB code

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("FUEL SALES DAILY REPORT"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

    /* :::::::::::::::::::::::::::::::::::::: Report Header Start :::::::::::::::::::::::::::::::::::::::::::::::::: */
    int headerRow = 1;

    CompanyViewModel? company = excelReportData.Company;
    DateTime shiftDate = (DateTime)excelReportGeneratorViewModel.SelectedDate;

    AddDataToCell(sheet, headerRow, 1, company.CompanyName);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow, headerRow, B, G));

    AddDataToCell(sheet, headerRow + 1, 1, reportName);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 1, headerRow + 1, B, G));

    AddDataToCell(sheet, headerRow + 3, 1, @$"{_localizer.GetString("store")} # {company.StoreCode}");
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 3, headerRow + 3, B, C));

    AddDataToCell(sheet, headerRow + 3, 5, @$"{_localizer.GetString("date")}: {excelReportGeneratorViewModel.SelectedDate:MM/dd/yyyy}");
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 3, headerRow + 3, F, G));

    int excelLine = headerRow + 3;

    // Report header styling
    for (int row = 1; row <= excelLine; row++)
    {
      for (int col = B; col <= G; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, IndexedColors.Grey25Percent.Index, (short?)(row == 2 ? 12 : 10), null,
                        true, false, false, false, false,
                        (row != excelLine ? HorizontalAlignment.Center : (col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right)), null,
                        (col == B ? BorderStyle.Thin : BorderStyle.None), (col == G ? BorderStyle.Thin : BorderStyle.None), (row == 1 ? BorderStyle.Thin : BorderStyle.None), (row == excelLine ? BorderStyle.Thin : BorderStyle.None),
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::::::: Report Body Start :::::::::::::::::::::::::::::::::::::::::::::::::: */
    string? fuelDept = excelReportData.FuelDept;
    IList<GradeViewModel>? grades = excelReportData.Grades;
    int fuelItemCount = grades.Count();

    IList<string?> stockCodes = grades?.Select(g => g.StockCode).ToList() ?? new List<string?>();
    IList<int?> gradeIds = grades?.Select(g => g.Id).ToList() ?? new List<int?>();

    IList<SaleHeadAndSaleLineViewModel>? saleHeadAndSaleLinesByStockCodes = excelReportData.SaleHeadAndSaleLinesByStockCodes;
    Dictionary<string, List<SaleHeadAndSaleLineViewModel>> saleHeadAndSaleLinesByStockCodesDictionary = saleHeadAndSaleLinesByStockCodes
            .GroupBy(sale => sale.StockCode)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(sale => sale.SD).ThenBy(sale => sale.SaleTime).ToList()
            );

    IList<SaleHeadAndSaleLineViewModel>? saleHeadAndSaleLinesByGradeIds = excelReportData.SaleHeadAndSaleLinesByGradeIds;
    Dictionary<int?, List<SaleHeadAndSaleLineViewModel>> saleHeadAndSaleLinesByGradeIdsDictionary = saleHeadAndSaleLinesByGradeIds
            .Where(sale => sale.GradeID.HasValue)
            .GroupBy(sale => sale.GradeID)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

    IList<FuelPriceHistoryViewModel>? fuelPriceHistories = excelReportData.FuelPriceHistories;
    Dictionary<int?, List<FuelPriceHistoryViewModel>> fuelPricesOrderByDateTimeDictionary = fuelPriceHistories
            .Where(fp => fp.GradeID.HasValue)
            .GroupBy(fp => fp.GradeID)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(fp => fp.DateTime).ToList()
            );

    Dictionary<int?, List<FuelPriceHistoryViewModel>> fuelPricesOrderByCashPriceDictionary = fuelPriceHistories
            .Where(fp => fp.GradeID.HasValue)
            .GroupBy(fp => fp.GradeID)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(fp => fp.CashPrice).ToList()
            );

    decimal totalDiscount = excelReportData.TotalDiscount;

    IList<decimal>[] FuelTypeAndPrice = Enumerable.Range(0, grades.Count())
                                                      .Select(_ => new List<decimal>())
                                                      .ToArray();

    decimal[] FuelItemTotalAmount = new decimal[grades.Count()];
    decimal[] FuelItemTotalVolum = new decimal[grades.Count()];
    int totalPriceReceived = 0;

    foreach (var gradeData in grades.Select((value, index) => new { index, value }))
    {
      GradeViewModel grade = gradeData.value;
      int i = gradeData.index;

      string stockCode = grade.StockCode;
      if (saleHeadAndSaleLinesByStockCodesDictionary.TryGetValue(stockCode, out List<SaleHeadAndSaleLineViewModel> saleHeadAndSaleLines))
      {
        DateTime rangeDateTimeFrom = shiftDate;
        DateTime rangeDateTimeTo = shiftDate;

        SaleHeadAndSaleLineViewModel firstSaleHeadAndSaleLine = saleHeadAndSaleLines.FirstOrDefault();
        SaleHeadAndSaleLineViewModel lastfirstSaleHeadAndSaleLine = saleHeadAndSaleLines.LastOrDefault();

        if (firstSaleHeadAndSaleLine != null)
        {
          DateTime saleDate = firstSaleHeadAndSaleLine.SaleDate;
          DateTime saleTime = firstSaleHeadAndSaleLine.SaleTime;
          rangeDateTimeFrom = new DateTime(saleDate.Year, saleDate.Month, saleDate.Day, saleTime.Hour, saleTime.Minute, saleTime.Second);
        }

        if (lastfirstSaleHeadAndSaleLine != null)
        {
          DateTime saleDate = lastfirstSaleHeadAndSaleLine.SaleDate;
          DateTime saleTime = lastfirstSaleHeadAndSaleLine.SaleTime;
          rangeDateTimeTo = new DateTime(saleDate.Year, saleDate.Month, saleDate.Day, saleTime.Hour, saleTime.Minute, saleTime.Second);
        }

        if (fuelPricesOrderByDateTimeDictionary.TryGetValue(grade.Id, out List<FuelPriceHistoryViewModel> fuelPrices))
        {
          List<FuelPriceHistoryViewModel> filteredFuelPrices = fuelPrices
            .Where(fp => fp.DateTime.HasValue && fp.DateTime.Value > rangeDateTimeFrom)
            .ToList();

          FuelPriceHistoryViewModel fuelPriceHistory = fuelPrices.FirstOrDefault();

          if (fuelPriceHistory != null)
          {
            rangeDateTimeFrom = (DateTime)fuelPriceHistory.DateTime;
          }
          else
          {
            rangeDateTimeFrom = new DateTime(1900, 1, 1, 0, 0, 0); // Hard coded in VB codebase
          }

          if (fuelPricesOrderByCashPriceDictionary.TryGetValue(grade.Id, out List<FuelPriceHistoryViewModel> fuelPricesFromDictionary))
          {
            List<FuelPriceHistoryViewModel> filteredFuelPricesByDateRange = fuelPricesFromDictionary
            .Where(fp => fp.DateTime.HasValue && fp.DateTime.Value >= rangeDateTimeFrom && fp.DateTime.Value <= rangeDateTimeTo)
            .ToList();

            totalPriceReceived = Math.Max(totalPriceReceived, filteredFuelPricesByDateRange.Count());

            foreach (FuelPriceHistoryViewModel fuelPriceHistoryByDateRange in filteredFuelPricesByDateRange)
            {
              FuelTypeAndPrice[i].Add((decimal)fuelPriceHistoryByDateRange.CashPrice);
            }
          }
        }
      }

      i++;
    }

    // Loop Fuel Items with Collected price range
    for (int j = 0; j < totalPriceReceived; j++)
    {
      excelLine += 2;

      // Hard coded in VB codebase
      AddDataToCell(sheet, excelLine, 3, _localizer.GetString("price").ToString()); // Hard coded in VB code
      AddDataToCell(sheet, excelLine, 4, _localizer.GetString("volume").ToString()); // Hard coded in VB code
      AddDataToCell(sheet, excelLine, 5, _localizer.GetString("sales").ToString()); // Hard coded in VB code

      bool totalQuantityAndAmountCount = false;

      for (int i = 0; i < fuelItemCount; i++)
      {
        if (saleHeadAndSaleLinesByGradeIdsDictionary.TryGetValue(gradeIds[i], out List<SaleHeadAndSaleLineViewModel> saleHeadAndSaleLines))
        {
          IList<SaleHeadAndSaleLineViewModel> filteredSaleHeadAndSaleLines = new List<SaleHeadAndSaleLineViewModel>();
          if (j < FuelTypeAndPrice[i].Count())
          {
            filteredSaleHeadAndSaleLines = saleHeadAndSaleLines.Where(sale => sale.Price == FuelTypeAndPrice[i][j]).ToList();
          }

          decimal sumOfFuelPriceAmount = filteredSaleHeadAndSaleLines.OrderBy(sale => sale.Price).Sum(sale => sale.Amount);
          decimal sumOfFuelTypeQuantity = filteredSaleHeadAndSaleLines.OrderBy(sale => sale.Price).Sum(sale => sale.Amount);

          excelLine++;

          FuelItemTotalAmount[i] += sumOfFuelPriceAmount;
          FuelItemTotalVolum[i] += sumOfFuelTypeQuantity;

          if (sumOfFuelTypeQuantity > 0 || sumOfFuelPriceAmount > 0)
          {
            totalQuantityAndAmountCount = true;
          }

          AddDataToCell(sheet, excelLine, 2, stockCodes[i]);
          if (j < FuelTypeAndPrice[i].Count())
          {
            AddDataToCell(sheet, excelLine, 3, FuelTypeAndPrice[i][j]);
          }
          AddDataToCell(sheet, excelLine, 4, sumOfFuelTypeQuantity);
          AddDataToCell(sheet, excelLine, 5, sumOfFuelPriceAmount);

          // row styling
          for (int col = B; col <= G; col++)
          {
            string cellDataFormat = null;

            if (col == D || col == E)
            {
              cellDataFormat = "###,##0.000;(###,##0.000)";
            }
            else if (col == F)
            {
              cellDataFormat = "###,##0.00;(###,##0.00)";
            }

            CreateCellStyles(workbook, sheet, style, excelLine, col, GetCellValue(sheet, excelLine, col),
                            IndexedColors.Black.Index, null, 10, null,
                            true, false, false, false, false,
                            (col == C ? HorizontalAlignment.Left : HorizontalAlignment.Right), null,
                            (col == B ? BorderStyle.Thin : null), (col == G ? BorderStyle.Thin : null), null, null,
                            cellDataFormat);
          }
        }
      }

      if (!totalQuantityAndAmountCount)
      {
        ClearRangeContents(sheet, excelLine - fuelItemCount - 1, excelLine, 1, 5);
        excelLine -= (fuelItemCount + 2);
      }
    }

    excelLine += 2;
    ClearRangeContents(sheet, excelLine + 1, excelLine + 2 + fuelItemCount, C, F);

    decimal fuelItemTotalGroupVolum = 0;
    decimal fuelItemTotalGroupAmount = 0;
    for (int i = 0; i < fuelItemCount; i++)
    {
      AddDataToCell(sheet, i + excelLine, 2, stockCodes[i]);
      AddDataToCell(sheet, i + excelLine, 3, _localizer.GetString("total").ToString()); // Hard coded in VB code
      AddDataToCell(sheet, i + excelLine, 4, FuelItemTotalVolum[i]);
      AddDataToCell(sheet, i + excelLine, 5, FuelItemTotalAmount[i]);

      // row styling
      for (int col = B; col <= G; col++)
      {
        string cellDataFormat = null;
        BorderStyle leftBorder = (col == B ? BorderStyle.Thin : (col == C ? BorderStyle.Double : BorderStyle.Hair));
        BorderStyle rightBorder = (col == G ? BorderStyle.Thin : (col == F ? BorderStyle.Double : BorderStyle.Hair));
        BorderStyle topBorder = (i == 0 && (col >= C && col <= F) ? BorderStyle.Double : BorderStyle.Hair);

        if (col == D || col == E)
        {
          cellDataFormat = "###,##0.000;(###,##0.000)";
        }
        else if (col == F)
        {
          cellDataFormat = "###,##0.00;(###,##0.00)";
        }

        CreateCellStyles(workbook, sheet, style, i + excelLine, col, GetCellValue(sheet, i + excelLine, col),
                        IndexedColors.Black.Index, null, 10, null,
                        col == F, false, false, false, false,
                        (col == C ? HorizontalAlignment.Left : HorizontalAlignment.Right), null,
                        leftBorder, rightBorder, topBorder, null,
                        cellDataFormat);
      }

      fuelItemTotalGroupVolum += FuelItemTotalVolum[i];
      fuelItemTotalGroupAmount += FuelItemTotalAmount[i];
    }

    excelLine = excelLine + fuelItemCount - 1;
    excelLine++;
    int totalDiscountExcelLine = -1;
    if (totalDiscount > 0)
    {
      excelLine++;
      AddDataToCell(sheet, excelLine, 5, totalDiscount);
      AddDataToCell(sheet, excelLine, 3, _localizer.GetString("discount").ToString()); // Hard coded in VB code
      totalDiscountExcelLine = excelLine;
    }

    excelLine++;
    AddDataToCell(sheet, excelLine, 2, _localizer.GetString("grandTotal").ToString()); // Hard coded in VB code
    sheet.AddMergedRegion(new CellRangeAddress(excelLine, excelLine, C, D));

    AddDataToCell(sheet, excelLine, 4, fuelItemTotalGroupVolum);
    AddDataToCell(sheet, excelLine, 5, fuelItemTotalGroupAmount - totalDiscount);

    // Footer rows styling
    for (int row = excelLine - 2; row <= excelLine + 2; row++)
    {
      for (int col = B; col <= G; col++)
      {
        string cellDataFormat = null;
        BorderStyle leftBorder = (col == B ? BorderStyle.Thin : (col == C && row <= excelLine ? BorderStyle.Double : BorderStyle.Hair));
        BorderStyle rightBorder = (col == G ? BorderStyle.Thin : (col == F && row <= excelLine ? BorderStyle.Double : BorderStyle.Hair));
        BorderStyle bottomBorder = (row == excelLine + 2 ? BorderStyle.Thin : (row == excelLine && (col >= C && col <= F) ? BorderStyle.Double : BorderStyle.Hair));

        if (col == D || col == E)
        {
          cellDataFormat = "###,##0.000;(###,##0.000)";
        }
        else if (col == F)
        {
          cellDataFormat = "###,##0.00;(###,##0.00)";
        }

        if (row == totalDiscountExcelLine)
        {
          cellDataFormat = "-#,##0.000;(#,##0.000)";
        }

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, 10, null,
                        (row == 11 || row == 12 || col == F ? true: false), false, false, false, false,
                        (col == C ? HorizontalAlignment.Left : HorizontalAlignment.Right), null,
                        leftBorder, rightBorder, null, bottomBorder,
                        cellDataFormat);
      }
    }

    sheet.SetColumnWidth(C, 15 * 256); // Set the width of column C (index 2) to 15 characters
    sheet.SetColumnWidth(D, 12 * 256); // Set the width of column D (index 3) to 12 characters
    sheet.SetColumnWidth(E, 12 * 256); // Set the width of column E (index 4) to 12 characters
    sheet.SetColumnWidth(F, 12 * 256); // Set the width of column F (index 5) to 12 characters

    // 6th row styling
    for (int col = B; col <= G; col++)
    {
      CreateCellStyles(workbook, sheet, style, 5, col, GetCellValue(sheet, 5, col),
                       null, null, 10, null,
                       false, false, false, false, false,
                       null, null,
                       (col == B ? BorderStyle.Thin : null), (col == G ? BorderStyle.Thin : null), null, null,
                       null);
    }

    return GetBase64Data(workbook);
  }

  #endregion

  #region Fuel Litres/Fuel Dollars/Departmental Sales Summary Report
  public async Task<FileBase64DataViewModel?> FLOrFDOrDSSummaryReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string url = "ExcelReports/GetFuelLitresOrDollarsSummaryReportData";
    FuelLitresSummaryReportDtoViewModel? getFuelLitresOrDollarsSummaryReportData = await _http.GetAndParseAsync<FuelLitresSummaryReportDtoViewModel?>(url);
    saleData = getFuelLitresOrDollarsSummaryReportData?.SalesData;
    company = getFuelLitresOrDollarsSummaryReportData?.CompanyDetails;

    uniqueShiftDates = saleData.Where(sd => sd.ShiftDate.HasValue)
                               .Select(sd => sd.ShiftDate.Value)
                               .Distinct()
                               .OrderBy(date => date)
                               .ToList();

    url = "ExcelReports/GetUniqueStockCodes";
    uniqueStocks = await _http.GetAndParseAsync<IList<string>?>(url);

    FileBase64DataViewModel fileBase64Data = new();

    if (excelReportGeneratorViewModel.ReportNo == 1)
    {
      // ReportNo = 1 (Fuel Litres Summary Report)
      fileBase64Data = FuelLitresSummaryReport(excelReportGeneratorViewModel);
    }
    else if (excelReportGeneratorViewModel.ReportNo == 2 || excelReportGeneratorViewModel.ReportNo == 3)
    {
      // ReportNo = 2 (Fuel Dollars Summary Report) Or ReportNo = 3 (Departmental Sales Summary Report)
      if (excelReportGeneratorViewModel.ReportNo == 3)
      {
        url = "Categories/Departments";
        IEnumerable<DepartmentViewModel>? departments = await _http.GetAndParseAsync<IEnumerable<DepartmentViewModel>?>(url);
        excelReportGeneratorViewModel.DepartmentName = departments?.ToList().Where(d => d.DeptId == excelReportGeneratorViewModel.Department)
                                                                  .Select(d => d.DeptName).FirstOrDefault();
      }

      fileBase64Data = FuelDollarsOrDeptSalesSummaryReport(excelReportGeneratorViewModel);
    }
    else if (excelReportGeneratorViewModel.ReportNo == 4 || excelReportGeneratorViewModel.ReportNo == 5)
    {
      url = "Family/GetAllFamily";
      families = await _http.GetAndParseAsync<IList<FamilyViewModel>?>(url);

      if (excelReportGeneratorViewModel.ReportNo == 5)
      {
        url = "ExcelReports/GetAllStockMaster";
        stockMst = await _http.GetAndParseAsync<IList<StockItemViewModel>?>(url);
        fileBase64Data = SalesDetailFamilyGroupReport(excelReportGeneratorViewModel);
      }
      else
      {
        fileBase64Data = SalesSummaryFamilyGroupReport(excelReportGeneratorViewModel);
      }
    }

    return fileBase64Data;
  }

  #endregion

  #region Fuel Litres Summary Report

  private FileBase64DataViewModel FuelLitresSummaryReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string taxExemptType = excelReportGeneratorViewModel.TaxExemptType;

    var resultDictionary = saleData
            .GroupBy(sd => new
            {
              sd.ShiftDate,
              sd.Stock,
            })
            .ToDictionary(
                group => new
                {
                  group.Key.ShiftDate,
                  group.Key.Stock,
                },
                group => new
                {
                  MemberVolume = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                     d.Stock == group.Key.Stock &&
                                                     d.TaxExempt == true &&
                                                     d.Member == true &&
                                                     taxExemptType == "BITE")
                                         .Sum(d => d.Volume) ?? 0,
                  NonMemberVolume = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                        d.Stock == group.Key.Stock &&
                                                        d.TaxExempt == true &&
                                                        d.Member == false &&
                                                        taxExemptType == "BITE")
                                            .Sum(d => d.Volume) ?? 0,
                  StatusVolume = saleData.Where(d => d.TaxExempt == true &&
                                                     d.ShiftDate == group.Key.ShiftDate &&
                                                     d.Stock == group.Key.Stock &&
                                                     taxExemptType != "BITE")
                                         .Sum(d => d.Volume) ?? 0,
                  NonStatusVolume = saleData.Where(d => d.TaxExempt == false &&
                                                        d.ShiftDate == group.Key.ShiftDate &&
                                                        d.Stock == group.Key.Stock)
                                            .Sum(d => d.Volume) ?? 0
                }
            );

    IList<StockCodeDetailsRecordViewModel> fuelLitresSummaryReportData = resultDictionary
            .Select(kvp => new StockCodeDetailsRecordViewModel
            {
              ShiftDate = kvp.Key.ShiftDate,
              StockCode = kvp.Key.Stock,
              Member = kvp.Value.MemberVolume,
              NonMember = kvp.Value.NonMemberVolume,
              Status = kvp.Value.StatusVolume,
              NonStatus = kvp.Value.NonStatusVolume
            })
            .ToList();

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("fuelLitresSummary");
    string? startDate = excelReportGeneratorViewModel.StartDate?.ToString("M/d/yyyy");
    string? endDate = excelReportGeneratorViewModel.EndDate?.ToString("M/d/yyyy");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("FUEL LITRES SUMMARY"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

    int mxcnt = uniqueStocks.Count();
    int mxcolumn;
    int lastColumn = 0;
    int columnNo;
    int headerRow;
    int minrow = 0;
    int rowNo = 0;
    int maxrow = 0;
    int footerRow = 0;
    int lCol = 0;
    int summaryRow = 0;
    int summaryMaxCol = 0;

    if (taxExemptType == "BITE")
    {
      mxcolumn = (mxcnt * 3) + 2;
      summaryMaxCol = 6;
    }
    else
    {
      mxcolumn = (mxcnt * 2) + 2;
      summaryMaxCol = 5;
    }

    headerRow = 1;

    /* :::::::::::::::::::::::::::::::::: Report title section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportNameAndPeriodStr = _localizer.GetString("fuelLitresSummaryReportPeriod");
    string reportTitle = company.CompanyName + " " + reportNameAndPeriodStr + startDate + " - " + endDate;

    CreateCellStyles(workbook, sheet, style, 0, 0, reportTitle,
                    IndexedColors.Black.Index, null, 10, null,
                    false, false, false, false, false,
                    null, null,
                    BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    null);

    /* :::::::::::::::::::::::::::::::::: Header section Start :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    AddDataToCell(sheet, headerRow, 0, _localizer.GetString("shiftDate").ToString());

    columnNo = 1;

    foreach (var stock in uniqueStocks)
    {
      if (taxExemptType == "BITE")
      {
        sheet.AddMergedRegion(new CellRangeAddress(headerRow, headerRow, columnNo, columnNo + 2));
      }
      else
      {
        sheet.AddMergedRegion(new CellRangeAddress(headerRow, headerRow, columnNo, columnNo + 1));
      }

      AddDataToCell(sheet, headerRow, columnNo, stock);
      AddDataToCell(sheet, headerRow + 1, columnNo, _localizer.GetString("nonStatus").ToString());
      AddDataToCell(sheet, headerRow + 1, columnNo + 1, _localizer.GetString("status").ToString());
      AddDataToCell(sheet, headerRow + 2, columnNo, _localizer.GetString("litres").ToString());
      AddDataToCell(sheet, headerRow + 2, columnNo + 1, _localizer.GetString("litres").ToString());

      if (taxExemptType == "BITE")
      {
        AddDataToCell(sheet, headerRow + 1, columnNo + 1, _localizer.GetString("member").ToString());

        columnNo++;

        AddDataToCell(sheet, headerRow + 1, columnNo + 1, _localizer.GetString("nonMember").ToString());
        AddDataToCell(sheet, headerRow + 2, columnNo + 1, _localizer.GetString("litres").ToString());
      }
      else
      {
        AddDataToCell(sheet, headerRow + 1, columnNo + 1, _localizer.GetString("status").ToString());
      }

      columnNo += 2;
    }

    AddDataToCell(sheet, headerRow, columnNo, _localizer.GetString("totalFLSR").ToString());
    AddDataToCell(sheet, headerRow + 1, columnNo, _localizer.GetString("litres").ToString());

    // header section styling
    for (int row = 1; row < 4; row++)
    {
      for (int col = A; col < mxcolumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Blue.Index, null, 7, null,
                        false, false, false, true, false,
                        HorizontalAlignment.Center, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report Body Start ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    minrow = headerRow + 3;
    rowNo = minrow;

    foreach (var shiftDate in uniqueShiftDates)
    {
      AddDataToCell(sheet, rowNo, 0, shiftDate.ToString("M/d/yyyy"));

      columnNo = 1;

      foreach (var stock in uniqueStocks)
      {
        StockCodeDetailsRecordViewModel? rowData = fuelLitresSummaryReportData.Where(data => data.ShiftDate == shiftDate &&
                                                                                             data.StockCode == stock)
                                                                              .ToList().FirstOrDefault();

        if (rowData == null)
        {
          rowData = new StockCodeDetailsRecordViewModel
          {
            ShiftDate = shiftDate,
            StockCode = stock,
            Status = 0,
            NonStatus = 0,
            Member = 0,
            NonMember = 0
          };
        }

        AddDataToCell(sheet, rowNo, columnNo, rowData.NonStatus);

        if (taxExemptType == "BITE")
        {
          // member
          AddDataToCell(sheet, rowNo, columnNo + 1, rowData.Member);

          // non member
          AddDataToCell(sheet, rowNo, columnNo + 2, rowData.NonMember);

          columnNo += 3;
        }
        else
        {
          // status
          AddDataToCell(sheet, rowNo, columnNo + 1, rowData.Status);

          columnNo += 2;
        }
      }

      lastColumn = columnNo;

      ApplyCellFormula(sheet, rowNo, lastColumn, $"SUM(B{rowNo + 1}:{IntToExcelColumn(columnNo - 1)}{rowNo + 1})");

      // full row styling
      for (int col = A; col < mxcolumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                        IndexedColors.Black.Index, null, 7, null,
                        false, false, false, false, false,
                        HorizontalAlignment.Right, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        col == A ? null : "###,##0.000"); // hard coded in VB
      }

      rowNo++;
    }

    rowNo--;

    /* :::::::::::::::::::::::::::::::::: Report Footer Start ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    maxrow = rowNo;
    footerRow = maxrow + 2;

    AddDataToCell(sheet, footerRow, 0, _localizer.GetString("total").ToString());

    for (int i = 1; i <= lastColumn; i++)
    {
      ApplyCellFormula(sheet, footerRow, i, $"SUM({IntToExcelColumn(i)}{minrow + 1}:{IntToExcelColumn(i)}{maxrow + 1})");
    }

    ApplyCellFormula(sheet, footerRow, lastColumn, $"SUM({IntToExcelColumn(lastColumn)}{minrow + 1}:{IntToExcelColumn(lastColumn)}{maxrow + 1})"); // summary of row summary

    // Footer rows styling
    for (int row = footerRow - 1; row <= footerRow; row++)
    {
      for (int col = A; col < mxcolumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, 7, null,
                        false, false, false, false, false,
                        col == A ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        col == A ? null : "###,##0.000"); // hard coded in VB
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report header Section Start ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    summaryRow = footerRow + 2;

    AddDataToCell(sheet, summaryRow, 2, _localizer.GetString("nonStatus").ToString());
    AddDataToCell(sheet, summaryRow, 3, _localizer.GetString("status").ToString());

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, summaryRow, 3, _localizer.GetString("member").ToString());
      AddDataToCell(sheet, summaryRow, 4, _localizer.GetString("nonMember").ToString());
      AddDataToCell(sheet, summaryRow + 1, 3, _localizer.GetString("litres").ToString());
      AddDataToCell(sheet, summaryRow + 1, 4, _localizer.GetString("litres").ToString());
      AddDataToCell(sheet, summaryRow, 5, _localizer.GetString("totalFLSR").ToString());
      AddDataToCell(sheet, summaryRow + 1, 5, _localizer.GetString("litres").ToString());
    }
    else
    {
      AddDataToCell(sheet, summaryRow + 1, 3, _localizer.GetString("litres").ToString());
    }

    // summary report header styling
    for (int row = summaryRow; row <= summaryRow + 2; row++)
    {
      for (int col = B; col < summaryMaxCol; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Blue.Index, null, 7, null,
                        false, false, false, true, false,
                        HorizontalAlignment.Center, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report body Section Start ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    rowNo = summaryRow + 3;
    var stockCodeSums = fuelLitresSummaryReportData
        .GroupBy(x => x.StockCode)
        .Select(group => new
        {
          StockCode = group.Key,
          TotalMember = group.Sum(x => x.Member ?? 0),
          TotalNonMember = group.Sum(x => x.NonMember ?? 0),
          TotalStatus = group.Sum(x => x.Status ?? 0),
          TotalNonStatus = group.Sum(x => x.NonStatus ?? 0)
        })
        .OrderBy(x => x.StockCode);

    foreach (var stock in stockCodeSums)
    {
      var statusSum = stock.TotalStatus;
      var memberSum = stock.TotalMember;
      var nonStatusSum = stock.TotalNonStatus;
      var nonMemberSum = stock.TotalNonMember;

      if (taxExemptType == "BITE")
      {
        if (statusSum > 0 || memberSum > 0 || nonMemberSum > 0 || nonStatusSum > 0)
        {
          AddDataToCell(sheet, rowNo, 1, stock.StockCode);
          AddDataToCell(sheet, rowNo, 2, nonStatusSum);
          AddDataToCell(sheet, rowNo, 3, memberSum);
          AddDataToCell(sheet, rowNo, 4, nonMemberSum);

          ApplyCellFormula(sheet, rowNo, 5, $"SUM(C{rowNo + 1}:E{rowNo + 1})"); // summary of row summary

          // summary report body styling
          for (int col = B; col < summaryMaxCol; col++)
          {
            CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                            IndexedColors.Black.Index, null, 7, null,
                            false, false, false, false, false,
                            col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                            BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                            col == B ? null : "###,##0.000"); // hard coded in VB
          }

          rowNo++;
        }
      }
      else
      {
        if (statusSum > 0 || nonStatusSum > 0)
        {
          AddDataToCell(sheet, rowNo, 1, stock.StockCode);
          AddDataToCell(sheet, rowNo, 2, nonStatusSum);
          AddDataToCell(sheet, rowNo, 3, statusSum);

          // summary report body styling
          for (int col = B; col < summaryMaxCol; col++)
          {
            CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                            IndexedColors.Black.Index, null, 7, null,
                            false, false, false, false, false,
                            col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                            BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                            col == B ? null : "###,##0.000"); // hard coded in VB
          }

          rowNo++;
        }
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report Footer (summary of row summary) ::::::::::::::::::::::::::::::::::::: */
    rowNo++;

    AddDataToCell(sheet, rowNo, 1, _localizer.GetString("total").ToString());

    ApplyCellFormula(sheet, rowNo, 2, $"SUM(C{summaryRow + 4}:C{rowNo})"); // summary of row summary
    ApplyCellFormula(sheet, rowNo, 3, $"SUM(D{summaryRow + 4}:D{rowNo})"); // summary of row summary

    if (taxExemptType == "BITE")
    {
      ApplyCellFormula(sheet, rowNo, 4, $"SUM(E{summaryRow + 4}:E{rowNo})"); // summary of row summary
      ApplyCellFormula(sheet, rowNo, 5, $"SUM(C{rowNo + 1}:E{rowNo + 1})"); // summary of row summary
    }
    else
    {
      ApplyCellFormula(sheet, rowNo, 4, $"SUM(C{rowNo + 1}:D{rowNo + 1})"); // summary of row summary
    }

    // summary row footer styling
    for (int row = rowNo - 1; row <= rowNo; row++)
    {
      for (int col = B; col < summaryMaxCol; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, 7, null,
                        false, false, false, false, false,
                        col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        col == B ? null : "###,##0.000"); // hard coded in VB
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report Footer (row percentages) ::::::::::::::::::::::::::::::::::::: */
    rowNo++;

    if (taxExemptType == "BITE")
    {
      ApplyCellFormula(sheet, rowNo, 2, $"C{rowNo}/F{rowNo}");
      ApplyCellFormula(sheet, rowNo, 3, $"D{rowNo}/F{rowNo}");
      ApplyCellFormula(sheet, rowNo, 4, $"E{rowNo}/F{rowNo}");
    }
    else
    {
      ApplyCellFormula(sheet, rowNo, 2, $"C{rowNo}/E{rowNo}");
      ApplyCellFormula(sheet, rowNo, 3, $"D{rowNo}/E{rowNo}");
    }

    // summary report footer row (percentages) styling
    for (int col = B; col < summaryMaxCol; col++)
    {
      CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                      IndexedColors.Black.Index, null, 7, null,
                      false, false, false, false, false,
                      HorizontalAlignment.Right, null,
                      BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                      "##0.00%"); // hard code in VB

    }

    for (int col = B; col < mxcolumn; col++)
    {
      sheet.AutoSizeColumn(col);
    }

    // Get base 64 data
    return GetBase64Data(workbook);
  }

  #endregion

  #region Fuel Dollars/Departmental Sales Summary Report

  private FileBase64DataViewModel FuelDollarsOrDeptSalesSummaryReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string taxExemptType = excelReportGeneratorViewModel.TaxExemptType;
    IList<StockCodeDetailsRecordViewModel> fuelDollarsOrDeptSalesSummaryReportData;
    IList<string> excelColumns;

    if (excelReportGeneratorViewModel.ReportNo == 2)
    {
      excelColumns = new List<string>(uniqueStocks);

      var resultDictionary = saleData
              .GroupBy(sd => new
              {
                sd.ShiftDate,
                sd.Stock,
              })
              .ToDictionary(
                  group => new
                  {
                    group.Key.ShiftDate,
                    group.Key.Stock,
                  },
                  group => new
                  {
                    MemberSales = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                      d.Stock == group.Key.Stock &&
                                                      d.TaxExempt == true &&
                                                      d.Member == true &&
                                                      taxExemptType == "BITE")
                                          .Sum(d => d.Sales) ?? 0,
                    MemberTax = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                   d.Stock == group.Key.Stock &&
                                                   d.TaxExempt == true &&
                                                   d.Member == true &&
                                                   taxExemptType == "BITE")
                                       .Sum(d => d.ExemptedTax) ?? 0,
                    NonMemberSales = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                         d.Stock == group.Key.Stock &&
                                                         d.TaxExempt == true &&
                                                         d.Member == false &&
                                                         taxExemptType == "BITE")
                                             .Sum(d => d.Sales) ?? 0,
                    NonMemberTax = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                      d.Stock == group.Key.Stock &&
                                                      d.TaxExempt == true &&
                                                      d.Member == false &&
                                                      taxExemptType == "BITE")
                                          .Sum(d => d.ExemptedTax) ?? 0,
                    StatusSales = saleData.Where(d => d.TaxExempt == true &&
                                                      d.ShiftDate == group.Key.ShiftDate &&
                                                      d.Stock == group.Key.Stock &&
                                                      taxExemptType != "BITE")
                                          .Sum(d => d.Sales) ?? 0,
                    StatusTax = saleData.Where(d => d.TaxExempt == true &&
                                                    d.ShiftDate == group.Key.ShiftDate &&
                                                    d.Stock == group.Key.Stock &&
                                                    taxExemptType != "BITE")
                                        .Sum(d => d.ExemptedTax) ?? 0,
                    NonStatusSales = saleData.Where(d => d.TaxExempt == false &&
                                                         d.ShiftDate == group.Key.ShiftDate &&
                                                         d.Stock == group.Key.Stock)
                                             .Sum(d => d.Sales) ?? 0
                  }
              );

      fuelDollarsOrDeptSalesSummaryReportData = resultDictionary
              .Select(kvp => new StockCodeDetailsRecordViewModel
              {
                ShiftDate = kvp.Key.ShiftDate,
                StockCode = kvp.Key.Stock,
                Member = kvp.Value.MemberSales,
                NonMember = kvp.Value.NonMemberSales,
                Status = kvp.Value.StatusSales,
                NonStatus = kvp.Value.NonStatusSales,
                MemberExemptedTax = kvp.Value.MemberTax,
                NonMemberExemptedTax = kvp.Value.NonMemberTax,
                StatusExemptedTax = kvp.Value.StatusTax
              })
              .ToList();

    }
    else
    {
      excelColumns = new List<string>();
      excelColumns.Add(excelReportGeneratorViewModel.Department);

      var resultDictionary = saleData
              .GroupBy(sd => new
              {
                sd.ShiftDate,
                sd.Dept,
              })
              .ToDictionary(
                  group => new
                  {
                    group.Key.ShiftDate,
                    group.Key.Dept,
                  },
                  group => new
                  {
                    MemberSales = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                      d.Dept == group.Key.Dept &&
                                                      d.TaxExempt == true &&
                                                      d.Member == true &&
                                                      taxExemptType == "BITE")
                                          .Sum(d => d.Sales) ?? 0,
                    MemberTax = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                   d.Dept == group.Key.Dept &&
                                                   d.TaxExempt == true &&
                                                   d.Member == true &&
                                                   taxExemptType == "BITE")
                                       .Sum(d => d.ExemptedTax) ?? 0,
                    NonMemberSales = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                         d.Dept == group.Key.Dept &&
                                                         d.TaxExempt == true &&
                                                         d.Member == false &&
                                                         taxExemptType == "BITE")
                                             .Sum(d => d.Sales) ?? 0,
                    NonMemberTax = saleData.Where(d => d.ShiftDate == group.Key.ShiftDate &&
                                                      d.Dept == group.Key.Dept &&
                                                      d.TaxExempt == true &&
                                                      d.Member == false &&
                                                      taxExemptType == "BITE")
                                          .Sum(d => d.ExemptedTax) ?? 0,
                    StatusSales = saleData.Where(d => d.TaxExempt == true &&
                                                      d.ShiftDate == group.Key.ShiftDate &&
                                                      d.Dept == group.Key.Dept &&
                                                      taxExemptType != "BITE")
                                          .Sum(d => d.Sales) ?? 0,
                    StatusTax = saleData.Where(d => d.TaxExempt == true &&
                                                    d.ShiftDate == group.Key.ShiftDate &&
                                                    d.Dept == group.Key.Dept &&
                                                    taxExemptType != "BITE")
                                        .Sum(d => d.ExemptedTax) ?? 0,
                    NonStatusSales = saleData.Where(d => d.TaxExempt == false &&
                                                         d.ShiftDate == group.Key.ShiftDate &&
                                                         d.Dept == group.Key.Dept)
                                             .Sum(d => d.Sales) ?? 0
                  }
              );

      fuelDollarsOrDeptSalesSummaryReportData = resultDictionary
              .Select(kvp => new StockCodeDetailsRecordViewModel
              {
                ShiftDate = kvp.Key.ShiftDate,
                Department = kvp.Key.Dept,
                Member = kvp.Value.MemberSales,
                NonMember = kvp.Value.NonMemberSales,
                Status = kvp.Value.StatusSales,
                NonStatus = kvp.Value.NonStatusSales,
                MemberExemptedTax = kvp.Value.MemberTax,
                NonMemberExemptedTax = kvp.Value.NonMemberTax,
                StatusExemptedTax = kvp.Value.StatusTax
              })
              .ToList();
    }

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("fuelDollarsSummary");
    string sheetName = "FUEL DOLLARS SUMMARY"; //Sheet name shouldn't be localized

    if (excelReportGeneratorViewModel.ReportNo == 3)
    {
      reportName = _localizer.GetString("departmentalSalesSummary");
      sheetName = "DEPARTMENTAL SALES SUMMARY"; //Sheet name shouldn't be localized
    }

    string? startDate = excelReportGeneratorViewModel.StartDate?.ToString("M/d/yyyy");
    string? endDate = excelReportGeneratorViewModel.EndDate?.ToString("M/d/yyyy");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet(sheetName); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

    int mxcnt = excelColumns.Count();
    int mxcolumn = (mxcnt * 3) + 2;
    int headerRow;
    int columnNo;
    int lastColumn;
    int minrow;
    int rowNo;
    int summaryMaxCol = 6;
    int maxrow = uniqueShiftDates.Count();

    mxcolumn += (taxExemptType == "BITE" ? 0 : 1);

    /* :::::::::::::::::::::::::::::::::: Report title section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportNameAndPeriodStr = _localizer.GetString("fuelDollarsSummaryReportPeriod");

    if (excelReportGeneratorViewModel.ReportNo == 3)
    {
      reportNameAndPeriodStr = _localizer.GetString("departmentalSalesSummaryReportPeriod");
    }

    string reportTitle = company.CompanyName + " " + reportNameAndPeriodStr + startDate + " - " + endDate;

    CreateCellStyles(workbook, sheet, style, 0, 0, reportTitle,
                    IndexedColors.Black.Index, null, 10, null,
                    false, false, false, false, false,
                    null, null,
                    BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                    null);

    /* :::::::::::::::::::::::::::::::::: Header section Start :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    headerRow = 1;
    AddDataToCell(sheet, headerRow, 0, _localizer.GetString("shiftDate").ToString());

    columnNo = 1;
    lastColumn = 0;

    foreach (var stock in excelColumns)
    {
      sheet.AddMergedRegion(new CellRangeAddress(headerRow, headerRow, columnNo, columnNo + 2));
      AddDataToCell(sheet, headerRow, columnNo, stock + (excelReportGeneratorViewModel.ReportNo == 2 ? "" : $" - {excelReportGeneratorViewModel.DepartmentName}"));

      AddDataToCell(sheet, headerRow + 1, columnNo, _localizer.GetString("nonStatus").ToString());
      AddDataToCell(sheet, headerRow + 2, columnNo, _localizer.GetString("dollar").ToString());

      if (taxExemptType == "BITE")
      {
        AddDataToCell(sheet, headerRow + 1, columnNo + 1, _localizer.GetString("member").ToString());
      }
      else
      {
        sheet.AddMergedRegion(new CellRangeAddress(headerRow + 1, headerRow + 1, columnNo + 1, columnNo + 2));
        AddDataToCell(sheet, headerRow + 1, columnNo + 1, _localizer.GetString("status").ToString());
      }

      AddDataToCell(sheet, headerRow + 2, columnNo + 1, _localizer.GetString("dollar").ToString());

      if (taxExemptType == "BITE")
      {
        AddDataToCell(sheet, headerRow + 1, columnNo + 2, _localizer.GetString("nonMember").ToString());
        AddDataToCell(sheet, headerRow + 2, columnNo + 2, _localizer.GetString("dollar").ToString());
      }
      else
      {
        AddDataToCell(sheet, headerRow + 2, columnNo + 2, _localizer.GetString("disc").ToString());
      }

      columnNo += 3;
    }

    if (taxExemptType != "BITE")
    {
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 1, headerRow + 1, columnNo, columnNo + 1));
    }

    AddDataToCell(sheet, headerRow + 1, columnNo, _localizer.GetString("totalFLSR").ToString());
    AddDataToCell(sheet, headerRow + 2, columnNo, _localizer.GetString("dollar").ToString());

    if (taxExemptType != "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 1, _localizer.GetString("disc").ToString());
    }

    // header section styling
    for (int row = 1; row <= 4; row++)
    {
      for (int col = A; col < mxcolumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Blue.Index, null, 7, null,
                        false, false, false, true, false,
                        HorizontalAlignment.Center, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Body section Start :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    minrow = headerRow + 4;
    rowNo = minrow;

    foreach (var shiftDate in uniqueShiftDates)
    {
      string rowDollarFormula = "";
      string rowTaxFormula = "";

      AddDataToCell(sheet, rowNo, 0, shiftDate.ToString("M/d/yyyy"));

      columnNo = 1;

      foreach (var column in excelColumns)
      {
        StockCodeDetailsRecordViewModel? rowData;

        if (excelReportGeneratorViewModel.ReportNo == 2)
        {
          rowData = fuelDollarsOrDeptSalesSummaryReportData.Where(data => data.ShiftDate == shiftDate &&
                                                                          data.StockCode == column)
                                                           .ToList().FirstOrDefault();

          if (rowData == null)
          {
            rowData = new StockCodeDetailsRecordViewModel
            {
              ShiftDate = shiftDate,
              StockCode = column,
              Status = 0,
              NonStatus = 0,
              Member = 0,
              NonMember = 0,
              MemberExemptedTax = 0,
              NonMemberExemptedTax = 0,
              StatusExemptedTax = 0
            };
          }
        }
        else
        {
          rowData = fuelDollarsOrDeptSalesSummaryReportData.Where(data => data.ShiftDate == shiftDate &&
                                                                          data.Department == column)
                                                           .ToList().FirstOrDefault();

          if (rowData == null)
          {
            rowData = new StockCodeDetailsRecordViewModel
            {
              ShiftDate = shiftDate,
              Department = column,
              Status = 0,
              NonStatus = 0,
              Member = 0,
              NonMember = 0,
              MemberExemptedTax = 0,
              NonMemberExemptedTax = 0,
              StatusExemptedTax = 0
            };
          }
        }

        AddDataToCell(sheet, rowNo, columnNo, rowData.NonStatus);

        if (taxExemptType == "BITE")
        {
          AddDataToCell(sheet, rowNo, columnNo + 1, rowData.Member);
          AddDataToCell(sheet, rowNo, columnNo + 2, rowData.NonMember);

          string rowFormula = $"{IntToExcelColumn(columnNo)}{rowNo + 1}:{IntToExcelColumn(columnNo + 2)}{rowNo + 1}";
          rowDollarFormula = rowDollarFormula + (string.IsNullOrWhiteSpace(rowDollarFormula) ? "" : ",") + rowFormula;
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 1, rowData.Status);
          AddDataToCell(sheet, rowNo, columnNo + 2, rowData.StatusExemptedTax);

          string rowFormula = $"{IntToExcelColumn(columnNo)}{rowNo + 1},{IntToExcelColumn(columnNo + 1)}{rowNo + 1}";
          rowDollarFormula = rowDollarFormula + (string.IsNullOrWhiteSpace(rowDollarFormula) ? "" : ",") + rowFormula;

          rowFormula = $"{IntToExcelColumn(columnNo + 2)}{rowNo + 1}";
          rowTaxFormula = rowTaxFormula + (string.IsNullOrWhiteSpace(rowTaxFormula) ? "" : ",") + rowFormula;
        }

        columnNo += 3;
      }

      lastColumn = columnNo;

      ApplyCellFormula(sheet, rowNo, lastColumn, $"SUM({rowDollarFormula})"); // row dollar total

      if (taxExemptType != "BITE")
      {
        ApplyCellFormula(sheet, rowNo, lastColumn + 1, $"SUM({rowTaxFormula})"); // row tax exempt total
      }

      // full row styling
      for (int col = A; col < mxcolumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                        IndexedColors.Black.Index, null, 7, null,
                        false, false, false, false, false,
                        HorizontalAlignment.Right, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        col == A ? null : "###,##0.000"); // hard coded in VB
      }

      rowNo++;
    }

    /* :::::::::::::::::::::::::::::::::: Report Footer Start ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    maxrow = rowNo - 1;
    int footerRow = maxrow + 2;
    AddDataToCell(sheet, footerRow, 0, _localizer.GetString("total").ToString());

    for (int i = 1; i < mxcolumn; i++)
    {
      ApplyCellFormula(sheet, footerRow, i, $"SUM({IntToExcelColumn(i)}{minrow + 1}:{IntToExcelColumn(i)}{maxrow + 1})");
    }

    ApplyCellFormula(sheet, footerRow, lastColumn, $"SUM({IntToExcelColumn(lastColumn)}{minrow + 1}:{IntToExcelColumn(lastColumn)}{maxrow + 1})"); // summary of row summary

    if (taxExemptType != "BITE")
    {
      ApplyCellFormula(sheet, footerRow, lastColumn + 1, $"SUM({IntToExcelColumn(lastColumn + 1)}{minrow + 1}:{IntToExcelColumn(lastColumn + 1)}{maxrow + 1})"); // summary of row summary
    }

    // Footer rows styling
    for (int row = footerRow - 1; row <= footerRow; row++)
    {
      for (int col = A; col < mxcolumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, 7, null,
                        false, false, false, false, false,
                        col == A ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        col == A ? null : "###,##0.000"); // hard coded in VB
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report Header Section Start :::::::::::::::::::::::::::::::::::::::::::::::::::: */
    int summaryRow = footerRow + 2;
    AddDataToCell(sheet, summaryRow, 2, _localizer.GetString("nonStatus").ToString());

    if (taxExemptType != "BITE")
    {
      sheet.AddMergedRegion(new CellRangeAddress(summaryRow, summaryRow, 3, 4));
      AddDataToCell(sheet, summaryRow, 3, _localizer.GetString("status").ToString());
      AddDataToCell(sheet, summaryRow + 1, 4, _localizer.GetString("disc").ToString());
    }
    else
    {
      AddDataToCell(sheet, summaryRow, 3, _localizer.GetString("member").ToString());
      AddDataToCell(sheet, summaryRow, 4, _localizer.GetString("nonMember").ToString());
      AddDataToCell(sheet, summaryRow + 1, 4, _localizer.GetString("dollar").ToString());
    }

    AddDataToCell(sheet, summaryRow + 1, 2, _localizer.GetString("dollar").ToString());
    AddDataToCell(sheet, summaryRow + 1, 3, _localizer.GetString("dollar").ToString());
    AddDataToCell(sheet, summaryRow + 1, 5, _localizer.GetString("totalDollarsCollected").ToString());

    // summary report header styling
    for (int row = summaryRow; row <= summaryRow + 2; row++)
    {
      for (int col = B; col < summaryMaxCol; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Blue.Index, null, 7, null,
                        false, false, false, true, false,
                        HorizontalAlignment.Center, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report Body Section Start :::::::::::::::::::::::::::::::::::::::::::::::::::: */
    rowNo = summaryRow + 3;

    var stockCodeSums = fuelDollarsOrDeptSalesSummaryReportData
        .GroupBy(x => (excelReportGeneratorViewModel.ReportNo == 2 ? x.StockCode : x.Department))
        .Select(group => new
        {
          ColumnKey = group.Key,
          TotalMember = group.Sum(x => x.Member ?? 0),
          TotalNonMember = group.Sum(x => x.NonMember ?? 0),
          TotalStatus = group.Sum(x => x.Status ?? 0),
          TotalNonStatus = group.Sum(x => x.NonStatus ?? 0),
          TotalStatusExemptedTax = group.Sum(x => x.StatusExemptedTax ?? 0),
          TotalMemberExemptedTax = group.Sum(x => x.MemberExemptedTax ?? 0),
          TotalNonMemberExemptedTax = group.Sum(x => x.NonMemberExemptedTax ?? 0)
        })
        .ToList()
        .OrderBy(x => x.ColumnKey);

    foreach (var column in stockCodeSums)
    {
      var memberSum = column.TotalMember;
      var nonStatusSum = column.TotalNonStatus;
      var nonMemberSum = column.TotalNonMember;
      var statusSum = column.TotalStatus;
      var statusDiscSum = column.TotalStatusExemptedTax;

      if (taxExemptType == "BITE")
      {
        if (memberSum > 0 || nonMemberSum > 0 || nonStatusSum > 0)
        {
          // row summary
          AddDataToCell(sheet, rowNo, 1, column.ColumnKey + (excelReportGeneratorViewModel.ReportNo == 2 ? "" : $" - {excelReportGeneratorViewModel.DepartmentName}"));
          AddDataToCell(sheet, rowNo, 2, nonStatusSum);
          AddDataToCell(sheet, rowNo, 3, memberSum);
          AddDataToCell(sheet, rowNo, 4, nonMemberSum);
          AddDataToCell(sheet, rowNo, 5, nonStatusSum + memberSum + nonMemberSum); // summary of row summary

          // summary report body styling
          for (int col = B; col < summaryMaxCol; col++)
          {
            CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                            IndexedColors.Black.Index, null, 7, null,
                            false, false, false, col == B, false,
                            col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                            BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                            col == B ? null : "###,##0.000"); // hard coded in VB
          }

          rowNo++;
        }
      }
      else
      {
        if (statusSum > 0 || nonStatusSum > 0)
        {
          AddDataToCell(sheet, rowNo, 1, column.ColumnKey + (excelReportGeneratorViewModel.ReportNo == 2 ? "" : $" - {excelReportGeneratorViewModel.DepartmentName}"));
          AddDataToCell(sheet, rowNo, 2, nonStatusSum);
          AddDataToCell(sheet, rowNo, 3, statusSum);
          AddDataToCell(sheet, rowNo, 4, statusDiscSum);

          // summary report body styling
          for (int col = B; col < summaryMaxCol; col++)
          {
            CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                            IndexedColors.Black.Index, null, 7, null,
                            false, false, false, col == B, false,
                            col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                            BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                            col == B ? null : "###,##0.000"); // hard coded in VB
          }


          rowNo++;
        }
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report Footer Section Start :::::::::::::::::::::::::::::::::::::::::::::::::::: */
    rowNo++;

    AddDataToCell(sheet, rowNo, 1, _localizer.GetString("total").ToString());
    ApplyCellFormula(sheet, rowNo, 2, $"SUM(C{summaryRow + 4}:C{rowNo - 1})"); // summary of row summary
    ApplyCellFormula(sheet, rowNo, 3, $"SUM(D{summaryRow + 4}:D{rowNo - 1})"); // summary of row summary
    ApplyCellFormula(sheet, rowNo, 4, $"SUM(E{summaryRow + 4}:E{rowNo - 1})"); // summary of row summary

    if (taxExemptType == "BITE")
    {
      ApplyCellFormula(sheet, rowNo, 5, $"SUM(C{rowNo + 1}:E{rowNo + 1})"); // summary of row summary
    }
    else
    {
      ApplyCellFormula(sheet, rowNo, 5, $"SUM(C{rowNo + 1}:D{rowNo + 1})"); // summary of row summary
    }

    // summary row footer styling
    for (int row = rowNo - 1; row <= rowNo; row++)
    {
      for (int col = B; col < summaryMaxCol; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, 7, null,
                        false, false, false, false, false,
                        col == B ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                        BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                        col == B ? null : "###,##0.000"); // hard coded in VB
      }
    }

    /* :::::::::::::::::::::::::::::::::: Summary Report Footer Section Start (row percentages) :::::::::::::::::::::::::::::::::::::::::::::::::::: */
    rowNo++;

    ApplyCellFormula(sheet, rowNo, 2, $"C{rowNo}/F{rowNo}");
    ApplyCellFormula(sheet, rowNo, 3, $"D{rowNo}/F{rowNo}");

    if (taxExemptType == "BITE")
    {
      ApplyCellFormula(sheet, rowNo, 4, $"E{rowNo}/F{rowNo}");
    }

    // summary report footer row (percentages) styling
    for (int col = B; col < summaryMaxCol; col++)
    {
      CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                      IndexedColors.Black.Index, null, 7, null,
                      false, false, false, false, false,
                      HorizontalAlignment.Right, null,
                      BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin, BorderStyle.Thin,
                      "##0.00%"); // hard coded in VB
    }

    // Base 64 file convertion
    return GetBase64Data(workbook);
  }

  #endregion

  #region Sales Summary Family Group Report 
  private FileBase64DataViewModel SalesSummaryFamilyGroupReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string taxExemptType = excelReportGeneratorViewModel.TaxExemptType;

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("salesSummaryFamilyGroup");
    string? startDate = excelReportGeneratorViewModel.StartDate?.ToString("MM/dd/yyyy");
    string? endDate = excelReportGeneratorViewModel.EndDate?.ToString("MM/dd/yyyy");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("SALES SUMMARY BY FAMILY GROUP"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
    BorderStyle noneBorder = BorderStyle.None;
    BorderStyle thinBorder = BorderStyle.Thin;
    BorderStyle thickBorder = BorderStyle.Thick;
    BorderStyle mediumBorder = BorderStyle.Medium;

    int maxrow = uniqueShiftDates.Count();
    int lastColRef = (taxExemptType == "BITE" ? O : L); // Column represented by Char in VB
    int lastColumn = 0;
    int prevColRef = 0;
    int columnNo;
    int maxColumn;
    int minRow;
    int rowNo;
    string rowDformula = "";
    string rowTformula = "";
    string rowCformula = "";

    /* :::::::::::::::::::::::::::::::::: Report title section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    int headerRow = 1;
    string reportTitle = company.CompanyName;
    string reportHeader = reportName + " - " + excelReportGeneratorViewModel.GroupId;

    AddDataToCell(sheet, headerRow - 1, 0, reportTitle);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow - 1, headerRow - 1, A, lastColRef));
    AddDataToCell(sheet, headerRow, 0, reportHeader);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow, headerRow, A, lastColRef));

    columnNo = 3;

    AddDataToCell(sheet, headerRow + 2, 0, _localizer.GetString("familyName").ToString());
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, A, C));
    AddDataToCell(sheet, headerRow + 2, columnNo, _localizer.GetString("nonStatus").ToString());
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo, columnNo + 2));

    AddDataToCell(sheet, headerRow + 4, columnNo, _localizer.GetString("quantity").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 1, _localizer.GetString("dollar").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 2, _localizer.GetString("cost").ToString());

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 3, _localizer.GetString("member").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 3, columnNo + 5));
    }
    else
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 3, _localizer.GetString("status").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 3, columnNo + 5));
    }

    AddDataToCell(sheet, headerRow + 4, columnNo + 3, _localizer.GetString("quantity").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 4, _localizer.GetString("dollar").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 5, _localizer.GetString("cost").ToString());

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 6, _localizer.GetString("nonMember").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 6, columnNo + 8));

      AddDataToCell(sheet, headerRow + 4, columnNo + 6, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 7, _localizer.GetString("dollar").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 8, _localizer.GetString("cost").ToString());
    }
    else
    {
      AddDataToCell(sheet, headerRow + 4, columnNo + 6, _localizer.GetString("disc").ToString());
    }

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 9, _localizer.GetString("totalFLSR").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 9, columnNo + 11));

      AddDataToCell(sheet, headerRow + 4, columnNo + 9, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 10, _localizer.GetString("dollar").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 11, _localizer.GetString("cost").ToString());

      lastColumn = columnNo + 11;
    }
    else
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 7, _localizer.GetString("totalFLSR").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 7, columnNo + 9));

      AddDataToCell(sheet, headerRow + 4, columnNo + 7, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 8, _localizer.GetString("dollar").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 9, _localizer.GetString("cost").ToString());

      lastColumn = columnNo + 9;
    }

    // Header section styling
    for (int row = 0; row < 7; row++)
    {
      for (int col = A; col <= lastColumn; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, IndexedColors.Grey50Percent.Index, (short?)(row == 1 ? 12 : 10), null,
                        true, false, false, false, false,
                        row == 3 && col == A ? HorizontalAlignment.Left : HorizontalAlignment.Center, null,
                        noneBorder, (col == lastColumn ? mediumBorder : noneBorder), noneBorder, (row == 6 ? mediumBorder : noneBorder),
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report body section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    maxColumn = columnNo - 1;
    minRow = headerRow + 7;
    rowNo = minRow - 1;

    foreach (var shiftDate in uniqueShiftDates)
    {
      rowDformula = "";
      rowTformula = "";
      rowCformula = "";
      int bodySectionStartRow = rowNo;

      AddDataToCell(sheet, rowNo, 0, _localizer.GetString("date").ToString() + ": " + shiftDate.ToString("MM/dd/yyyy"));
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

      rowNo++;
      minRow = rowNo;
      columnNo = 3;

      var rowLevels = saleData?
                    .Join(families,
                        saledata => saledata.Dept,
                        family => family.FamilyID,
                        (saledata, family) => new { saledata, family })
                    .Where(joined => joined.saledata.ShiftDate == shiftDate)
                    .Select(joined => new { joined.saledata.Dept, name = joined.family.Description })
                    .Distinct()
                    .OrderBy(result => result.Dept)
                    .ToList();

      foreach (var level in rowLevels)
      {
        AddDataToCell(sheet, rowNo, 0, level.name);
        sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

        var nonStatusData = saleData
              .Where(sale => sale.ShiftDate == shiftDate
                          && sale.Dept == level.Dept
                          && sale.TaxExempt == false)
              .GroupBy(sale => new { sale.ShiftDate, sale.Dept })
              .Select(g => new
              {
                Volume = g.Sum(s => s.Volume ?? 0),
                Sales = g.Sum(s => s.Sales ?? 0),
                Cost = g.Sum(s => s.Cost ?? 0)
              })
              .FirstOrDefault();

        if (nonStatusData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 1, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo, nonStatusData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 1, nonStatusData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 2, nonStatusData.Cost);
        }

        // row summary
        rowTformula = IntToExcelColumn(columnNo) + (rowNo + 1);
        rowDformula = IntToExcelColumn(columnNo + 1) + (rowNo + 1);
        rowCformula = IntToExcelColumn(columnNo + 2) + (rowNo + 1);

        if (taxExemptType == "BITE")
        {
          // Member Data
          var memberData = saleData
              .Where(sale => sale.ShiftDate == shiftDate
                          && sale.Dept == level.Dept
                          && sale.TaxExempt == true
                          && sale.Member == true)
              .GroupBy(sale => new { sale.ShiftDate, sale.Dept })
              .Select(g => new
              {
                Volume = g.Sum(s => s.Volume ?? 0),
                Sales = g.Sum(s => s.Sales ?? 0),
                ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
                Cost = g.Sum(s => s.Cost ?? 0)
              }).FirstOrDefault();

          if (memberData == null)
          {
            AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 5, 0); // Hard coded in VB
          }
          else
          {
            AddDataToCell(sheet, rowNo, columnNo + 3, memberData.Volume);
            AddDataToCell(sheet, rowNo, columnNo + 4, memberData.Sales);
            AddDataToCell(sheet, rowNo, columnNo + 5, memberData.Cost);
          }

          rowTformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
          rowDformula += "," + IntToExcelColumn(columnNo + 4) + (rowNo + 1);
          rowCformula += "," + IntToExcelColumn(columnNo + 5) + (rowNo + 1);

          // Non Member Data
          var nonMemberData = saleData
              .Where(sale => sale.ShiftDate == shiftDate
                          && sale.Dept == level.Dept
                          && sale.TaxExempt == true
                          && sale.Member == false)
              .GroupBy(sale => new { sale.ShiftDate, sale.Dept })
              .Select(g => new
              {
                Volume = g.Sum(s => s.Volume ?? 0),
                Sales = g.Sum(s => s.Sales ?? 0),
                ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
                Cost = g.Sum(s => s.Cost ?? 0)
              })
              .FirstOrDefault();

          if (nonMemberData == null)
          {
            AddDataToCell(sheet, rowNo, columnNo + 6, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 7, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 8, 0); // Hard coded in VB
          }
          else
          {
            AddDataToCell(sheet, rowNo, columnNo + 6, nonMemberData.Volume);
            AddDataToCell(sheet, rowNo, columnNo + 7, nonMemberData.Sales);
            AddDataToCell(sheet, rowNo, columnNo + 8, nonMemberData.Cost);
          }

          rowTformula += "," + IntToExcelColumn(columnNo + 6) + (rowNo + 1);
          rowDformula += "," + IntToExcelColumn(columnNo + 7) + (rowNo + 1);
          rowCformula += "," + IntToExcelColumn(columnNo + 8) + (rowNo + 1);
          lastColumn = columnNo + 8;
        }
        else
        {
          // status data
          var statusData = saleData
              .Where(sale => sale.ShiftDate == shiftDate
                          && sale.Dept == level.Dept
                          && sale.TaxExempt == true)
              .GroupBy(sale => new { sale.ShiftDate, sale.Dept })
              .Select(g => new
              {
                Volume = g.Sum(s => s.Volume ?? 0),
                Sales = g.Sum(s => s.Sales ?? 0),
                ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
                Cost = g.Sum(s => s.Cost ?? 0)
              })
              .FirstOrDefault();

          if (statusData == null)
          {
            AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
          }
          else
          {
            AddDataToCell(sheet, rowNo, columnNo + 2, statusData.Volume);
            AddDataToCell(sheet, rowNo, columnNo + 3, statusData.Sales);
            AddDataToCell(sheet, rowNo, columnNo + 4, statusData.ExemptedTax);
          }

          rowTformula += "," + IntToExcelColumn(columnNo + 2) + (rowNo + 1);
          rowDformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
          rowCformula += "," + IntToExcelColumn(columnNo + 4) + (rowNo + 1);
          lastColumn = columnNo + 4;
        }

        // row summary
        ApplyCellFormula(sheet, rowNo, lastColumn + 1, $"SUM({rowTformula})");
        ApplyCellFormula(sheet, rowNo, lastColumn + 2, $"SUM({rowDformula})");
        ApplyCellFormula(sheet, rowNo, lastColumn + 3, $"SUM({rowCformula})");

        lastColumn += 3; // we need to consider row total columns too
        rowNo++;
      }

      // summary for date
      maxrow = rowNo;
      rowNo++;

      AddDataToCell(sheet, rowNo, 0, _localizer.GetString("totalFor").ToString() + " " + shiftDate.ToString("MM/dd/yyyy"));
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

      // column total
      for (int i = 3; i <= lastColumn; i++)
      {
        ApplyCellFormula(sheet, rowNo, i, $"SUM({IntToExcelColumn(i)}{minRow + 1}:{IntToExcelColumn(i)}{maxrow + 1})");
      }

      lastColRef = lastColumn;
      prevColRef = lastColumn - 1;

      // full section styling for the date
      for (int row = bodySectionStartRow; row <= rowNo; row++)
      {
        for (int col = A; col <= lastColumn; col++)
        {
          string cellDataFormat = null;

          if (taxExemptType == "BITE")
          {
            if (col >= D && col <= lastColumn)
            {
              cellDataFormat = ((col == D || col == G || col == J || col == M) ? "###,##0;(###,##0)" : "$###,##0.00;($###,##0.00)"); // Hard coded in VB
            }
          }
          else
          {
            if (col >= D && col <= lastColumn)
            {
              if (col < E)
              {
                cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
              else if (col == E)
              {
                // exempted tax
                cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
              }
            }
          }

          CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                          (row == bodySectionStartRow || (row == rowNo && col == A)) ? IndexedColors.Blue.Index : IndexedColors.Black.Index, null, 10, null,
                          col >= M && col <= lastColumn, (row == bodySectionStartRow || (row == rowNo && col == A)), false, false, false,
                          col >= D && col <= lastColumn ? HorizontalAlignment.Right : null, null,
                          null, null, null, row == rowNo && col > C ? BorderStyle.Thick : null,
                          cellDataFormat);
        }
      }

      rowNo += 2;
    }

    rowNo += 2;
    maxrow = rowNo;

    /* :::::::::::::::::::::::::::::::::: Report footer section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    AddDataToCell(sheet, rowNo, 0, _localizer.GetString("totalFLSR").ToString() + " " + startDate + " to " + endDate);
    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, D));

    // summary by family for the period
    int footerSectionStartRow = rowNo;
    rowNo += 2;
    var footerRowLevels = saleData?
                    .Join(families,
                        saledata => saledata.Dept,
                        family => family.FamilyID,
                        (saledata, family) => new { saledata, family })
                    .Select(joined => new { joined.saledata.Dept, name = joined.family.Description })
                    .Distinct()
                    .OrderBy(result => result.Dept)
                    .ToList();

    foreach (var level in footerRowLevels)
    {
      rowTformula = "";
      rowDformula = "";
      rowCformula = "";

      AddDataToCell(sheet, rowNo, 0, level.name);
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

      var nonStatusData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == false)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .FirstOrDefault();

      if (nonStatusData == null)
      {
        AddDataToCell(sheet, rowNo, columnNo, 0); // Hard coded in VB
        AddDataToCell(sheet, rowNo, columnNo + 1, 0); // Hard coded in VB
        AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
      }
      else
      {
        AddDataToCell(sheet, rowNo, columnNo, nonStatusData.Volume);
        AddDataToCell(sheet, rowNo, columnNo + 1, nonStatusData.Sales);
        AddDataToCell(sheet, rowNo, columnNo + 2, nonStatusData.Cost);
      }

      // row summary
      rowTformula = IntToExcelColumn(columnNo) + (rowNo + 1);
      rowDformula = IntToExcelColumn(columnNo + 1) + (rowNo + 1);
      rowCformula = IntToExcelColumn(columnNo + 2) + (rowNo + 1);

      if (taxExemptType == "BITE")
      {
        // Member Data
        var memberData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == true
                        && sale.Member == true)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            }).FirstOrDefault();
        if (memberData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 5, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 3, memberData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 4, memberData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 5, memberData.Cost);
        }

        rowTformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
        rowDformula += "," + IntToExcelColumn(columnNo + 4) + (rowNo + 1);
        rowCformula += "," + IntToExcelColumn(columnNo + 5) + (rowNo + 1);

        // Non Member Data
        var nonMemberData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == true
                        && sale.Member == false)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .FirstOrDefault();

        if (nonMemberData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo + 6, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 7, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 8, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 6, nonMemberData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 7, nonMemberData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 8, nonMemberData.Cost);
        }

        rowTformula += "," + IntToExcelColumn(columnNo + 6) + (rowNo + 1);
        rowDformula += "," + IntToExcelColumn(columnNo + 7) + (rowNo + 1);
        rowCformula += "," + IntToExcelColumn(columnNo + 8) + (rowNo + 1);
        lastColumn = columnNo + 8;
      }
      else
      {
        // status data
        var statusData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == true)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .FirstOrDefault();

        if (statusData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 2, statusData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 3, statusData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 4, statusData.ExemptedTax);
        }

        rowTformula += "," + IntToExcelColumn(columnNo + 2) + (rowNo + 1);
        rowDformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
        lastColumn = columnNo + 4;
      }

      // row summary
      ApplyCellFormula(sheet, rowNo, lastColumn + 1, $"SUM({rowTformula})");
      ApplyCellFormula(sheet, rowNo, lastColumn + 2, $"SUM({rowDformula})");
      ApplyCellFormula(sheet, rowNo, lastColumn + 3, $"SUM({rowCformula})");

      lastColumn += 3; // we need to consider row total columns too
      rowNo++;
    }

    int footerRow = rowNo + 1;

    // full footer summary report styling
    for (int row = footerSectionStartRow; row <= footerRow; row++)
    {
      for (int col = A; col <= lastColumn; col++)
      {
        string cellDataFormat = null;

        // cell data format only for footer row
        if (row == footerRow)
        {
          if (taxExemptType == "BITE")
          {
            if (col >= D && col <= lastColumn)
            {
              cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
            }
          }
          else
          {
            if (col >= D && col <= lastColumn)
            {
              if (col < H)
              {
                cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
              else if (col == H)
              {
                // exempted tax
                cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
              }
              else
              {
                cellDataFormat = (col % 2 == 1) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
            }
          }
        }
        //except footer row
        else
        {
          if (taxExemptType == "BITE")
          {
            if (col >= D && col <= lastColumn)
            {
              cellDataFormat = ((col == D || col == G || col == J || col == M) ? "###,##0;(###,##0)" : "$###,##0.00;($###,##0.00)"); // Hard coded in VB
            }
          }
          else
          {
            if (col >= D && col <= lastColumn)
            {
              if (col < E)
              {
                cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
              else if (col == E)
              {
                // exempted tax
                cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
              }
            }
          }
        }

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, IndexedColors.Grey50Percent.Index, 10, null,
                        col >= M && col <= lastColumn, false, false, false, false,
                        col >= D && col <= lastColumn ? HorizontalAlignment.Right : null, null,
                        noneBorder, (col == lastColumn ? mediumBorder : noneBorder), (row == footerSectionStartRow ? mediumBorder : noneBorder), (row == footerRow ? mediumBorder : noneBorder),
                        cellDataFormat);
      }
    }

    // Base 64 file convertion
    return GetBase64Data(workbook);
  }
  #endregion

  #region Sales Detail Family Group Report

  private FileBase64DataViewModel SalesDetailFamilyGroupReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string taxExemptType = excelReportGeneratorViewModel.TaxExemptType;

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("salesDetailFamilyGroup");
    string? startDate = excelReportGeneratorViewModel.StartDate?.ToString("MM/dd/yyyy");
    string? endDate = excelReportGeneratorViewModel.EndDate?.ToString("MM/dd/yyyy");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("FAMILY GROUP SALES DETAIL"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
    BorderStyle noneBorder = BorderStyle.None;
    BorderStyle thinBorder = BorderStyle.Thin;
    BorderStyle thickBorder = BorderStyle.Thick;
    BorderStyle mediumBorder = BorderStyle.Medium;

    int maxrow = uniqueShiftDates.Count();
    int lastColRef = (taxExemptType == "BITE" ? O : J); // Column represented by Char in VB
    int lastColumn = 0;
    int prevColRef = 0;
    string rowDformula = "";
    string rowTformula = "";
    string rowCformula = "";

    /* :::::::::::::::::::::::::::::::::: Report title section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    int headerRow = 1;

    // Header
    string reportTitle = company.CompanyName;
    AddDataToCell(sheet, headerRow - 1, 0, reportTitle);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow - 1, headerRow - 1, A, lastColRef));

    string reportHeader = reportName + " - " + excelReportGeneratorViewModel.GroupId;
    AddDataToCell(sheet, headerRow, 0, reportHeader);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow, headerRow, A, lastColRef));

    // column heading
    int columnNo = 3;

    AddDataToCell(sheet, headerRow + 2, 0, _localizer.GetString("familyName").ToString());
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, A, C));

    AddDataToCell(sheet, headerRow + 4, 0, _localizer.GetString("stockCodeOrDesc").ToString());
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 4, headerRow + 4, A, C));

    AddDataToCell(sheet, headerRow + 2, columnNo, _localizer.GetString("nonStatus").ToString());
    sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo, columnNo + 2));

    AddDataToCell(sheet, headerRow + 4, columnNo, _localizer.GetString("quantity").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 1, _localizer.GetString("dollar").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 2, _localizer.GetString("cost").ToString());

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 3, _localizer.GetString("member").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 3, columnNo + 5));
    }
    else
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 3, _localizer.GetString("status").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 3, columnNo + 4));
    }

    AddDataToCell(sheet, headerRow + 4, columnNo + 3, _localizer.GetString("quantity").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 4, _localizer.GetString("dollar").ToString());
    AddDataToCell(sheet, headerRow + 4, columnNo + 5, _localizer.GetString("cost").ToString());

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 6, _localizer.GetString("nonMember").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 6, columnNo + 8));

      AddDataToCell(sheet, headerRow + 4, columnNo + 6, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 7, _localizer.GetString("dollar").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 8, _localizer.GetString("cost").ToString());
    }
    else
    {
      AddDataToCell(sheet, headerRow + 4, columnNo + 4, _localizer.GetString("disc").ToString());
    }

    if (taxExemptType == "BITE")
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 9, _localizer.GetString("totalFLSR").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 9, columnNo + 11));

      AddDataToCell(sheet, headerRow + 4, columnNo + 9, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 10, _localizer.GetString("dollar").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 11, _localizer.GetString("cost").ToString());
    }
    else
    {
      AddDataToCell(sheet, headerRow + 2, columnNo + 5, _localizer.GetString("totalFLSR").ToString());
      sheet.AddMergedRegion(new CellRangeAddress(headerRow + 2, headerRow + 2, columnNo + 5, columnNo + 6));

      AddDataToCell(sheet, headerRow + 4, columnNo + 5, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, headerRow + 4, columnNo + 6, _localizer.GetString("dollar").ToString());
    }

    int maxColumn = columnNo - 1;
    int minRow = headerRow + 7;

    // Report header styling
    for (int row = 0; row < 6; row++)
    {
      for (int col = A; col <= lastColRef; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, IndexedColors.Grey50Percent.Index, (short?)(row == 1 ? 14 : 10), null,
                        true, false, false, false, false,
                        ((row == 3 || row == 5) && col == A) ? HorizontalAlignment.Left : HorizontalAlignment.Center, null,
                        noneBorder, (col == lastColRef ? mediumBorder : noneBorder), noneBorder, (row == 5 ? mediumBorder : noneBorder),
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report body section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    int rowNo = minRow - 1;
    string[] ColGFormula = new string[12]; // Grand Total

    foreach (var shiftDate in uniqueShiftDates)
    {
      int bodySectionStartRow = rowNo;

      AddDataToCell(sheet, rowNo, 0, _localizer.GetString("date").ToString() + ": " + shiftDate.ToString("MM/dd/yyyy"));
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, B));

      rowNo++;
      columnNo = 3;
      string[] ColDFormula = new string[12]; // this for date summary
      var rowLevels = saleData?
                    .Join(families,
                        saledata => saledata.Dept,
                        family => family.FamilyID,
                        (saledata, family) => new { saledata, family })
                    .Where(joined => joined.saledata.ShiftDate == shiftDate)
                    .Select(joined => new { joined.saledata.Dept, name = joined.family.Description })
                    .Distinct()
                    .OrderBy(result => result.Dept)
                    .ToList();

      foreach (var level in rowLevels)
      {
        AddDataToCell(sheet, rowNo, 0, string.IsNullOrEmpty(level.name) ? "" : level.name);
        sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, B));

        rowNo += 2;
        minRow = rowNo;

        var rsStock = saleData
            .Join(stockMst,
                saledata => saledata.Stock,
                stockmst => stockmst.StockCode,
                (saledata, stockmst) => new { saledata, stockmst })
            .Where(joined => joined.saledata.ShiftDate == shiftDate &&
                             joined.saledata.Dept == level.Dept)
            .Select(joined => new
            {
              Stock = joined.saledata.Stock,
              StockName = joined.stockmst.Description
            })
            .Distinct()
            .ToList();

        foreach (var stock in rsStock)
        {
          AddDataToCell(sheet, rowNo, 0, string.IsNullOrEmpty(stock.Stock) ? "" : stock.StockName);
          sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

          // Non Status Data
          var nonStatusData = saleData
            .Where(sale => sale.ShiftDate == shiftDate
                           && sale.Dept == level.Dept
                           && sale.Stock == stock.Stock
                           && sale.TaxExempt == false)
            .GroupBy(sale => new { sale.ShiftDate, sale.Stock })
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .ToList()
            .FirstOrDefault();

          if (nonStatusData == null)
          {
            AddDataToCell(sheet, rowNo, columnNo, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 1, 0); // Hard coded in VB
            AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
          }
          else
          {
            AddDataToCell(sheet, rowNo, columnNo, nonStatusData.Volume);
            AddDataToCell(sheet, rowNo, columnNo + 1, nonStatusData.Sales);
            AddDataToCell(sheet, rowNo, columnNo + 2, nonStatusData.Cost);
          }

          // row summary
          rowTformula = IntToExcelColumn(columnNo) + (rowNo + 1);
          rowDformula = IntToExcelColumn(columnNo + 1) + (rowNo + 1);
          rowCformula = IntToExcelColumn(columnNo + 2) + (rowNo + 1);

          if (taxExemptType == "BITE")
          {
            // Member Data
            var memberData = saleData
            .Where(sale => sale.ShiftDate == shiftDate
                           && sale.Dept == level.Dept
                           && sale.Stock == stock.Stock
                           && sale.TaxExempt == true
                           && sale.Member == true)
            .GroupBy(sale => new { sale.ShiftDate, sale.Stock })
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .ToList()
            .FirstOrDefault();

            if (memberData == null)
            {
              AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
              AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
              AddDataToCell(sheet, rowNo, columnNo + 5, 0); // Hard coded in VB
            }
            else
            {
              AddDataToCell(sheet, rowNo, columnNo + 3, memberData.Volume);
              AddDataToCell(sheet, rowNo, columnNo + 4, memberData.Sales);
              AddDataToCell(sheet, rowNo, columnNo + 5, memberData.Cost);
            }

            rowTformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
            rowDformula += "," + IntToExcelColumn(columnNo + 4) + (rowNo + 1);
            rowCformula += "," + IntToExcelColumn(columnNo + 5) + (rowNo + 1);

            // Non Member Data
            var nonMemberData = saleData
            .Where(sale => sale.ShiftDate == shiftDate
                           && sale.Dept == level.Dept
                           && sale.Stock == stock.Stock
                           && sale.TaxExempt == true
                           && sale.Member == false)
            .GroupBy(sale => new { sale.ShiftDate, sale.Stock })
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .ToList()
            .FirstOrDefault();

            if (nonMemberData == null)
            {
              AddDataToCell(sheet, rowNo, columnNo + 6, 0); // Hard coded in VB
              AddDataToCell(sheet, rowNo, columnNo + 7, 0); // Hard coded in VB
              AddDataToCell(sheet, rowNo, columnNo + 8, 0); // Hard coded in VB
            }
            else
            {
              AddDataToCell(sheet, rowNo, columnNo + 6, nonMemberData.Volume);
              AddDataToCell(sheet, rowNo, columnNo + 7, nonMemberData.Sales);
              AddDataToCell(sheet, rowNo, columnNo + 8, nonMemberData.Cost);
            }

            rowTformula += "," + IntToExcelColumn(columnNo + 6) + (rowNo + 1);
            rowDformula += "," + IntToExcelColumn(columnNo + 7) + (rowNo + 1);
            rowCformula += "," + IntToExcelColumn(columnNo + 8) + (rowNo + 1);
            lastColumn = columnNo + 8;
          }
          else
          {
            // Status Data
            var statusData = saleData
            .Where(sale => sale.ShiftDate == shiftDate
                           && sale.Dept == level.Dept
                           && sale.Stock == stock.Stock
                           && sale.TaxExempt == true)
            .GroupBy(sale => new { sale.ShiftDate, sale.Stock })
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .ToList()
            .FirstOrDefault();

            if (statusData == null)
            {
              AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
              AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
              AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
            }
            else
            {
              AddDataToCell(sheet, rowNo, columnNo + 2, statusData.Volume);
              AddDataToCell(sheet, rowNo, columnNo + 3, statusData.Sales);
              AddDataToCell(sheet, rowNo, columnNo + 4, statusData.ExemptedTax);
            }

            rowTformula += "," + IntToExcelColumn(columnNo + 2) + (rowNo + 1);
            rowDformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
            lastColumn = columnNo + 4;
          }

          // row summary
          ApplyCellFormula(sheet, rowNo, lastColumn + 1, $"SUM({rowTformula})");
          ApplyCellFormula(sheet, rowNo, lastColumn + 2, $"SUM({rowDformula})");

          if (taxExemptType == "BITE")
          {
            ApplyCellFormula(sheet, rowNo, lastColumn + 3, $"SUM({rowCformula})");
          }

          lastColumn += 3; // we need to consider rew total columns too
          rowNo++;
        }

        maxrow = rowNo - 1;
        rowNo++;

        // Make border around stocks
        prevColRef = lastColumn - 1;

        // Level Summary
        AddDataToCell(sheet, rowNo, 0, _localizer.GetString("totalFLSR").ToString() + " " + level.name);
        sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

        lastColumn = lastColRef;

        // column total (level summary)
        for (int i = 3; i <= lastColumn; i++)
        {
          ApplyCellFormula(sheet, rowNo, i, $"SUM({IntToExcelColumn(i)}{minRow + 1}:{IntToExcelColumn(i)}{maxrow + 1})");

          // column summary (date)
          ColDFormula[i - 3] += (string.IsNullOrEmpty(ColDFormula[i - 3]) ? "" : ",") + $"{IntToExcelColumn(i)}{minRow + 1}:{IntToExcelColumn(i)}{maxrow + 1}";

          // column summary (grand)
          ColGFormula[i - 3] += (string.IsNullOrEmpty(ColGFormula[i - 3]) ? "" : ",") + $"{IntToExcelColumn(i)}{minRow + 1}:{IntToExcelColumn(i)}{maxrow + 1}";
        }

        // full section styling for the date
        for (int row = bodySectionStartRow; row <= rowNo; row++)
        {
          for (int col = A; col <= lastColumn; col++)
          {
            string cellDataFormat = null;
            bool isBold = false;

            if (taxExemptType == "BITE")
            {
              // cell data format
              if (col >= D && col <= lastColumn)
              {
                cellDataFormat = ((col == D || col == G || col == J || col == M) ? "###,##0;(###,##0)" : "$###,##0.00;($###,##0.00)"); // Hard coded in VB
              }

              // is bold
              isBold = (col >= M && col <= lastColumn);
            }
            else
            {
              // cell data format
              if (col >= D && col <= lastColumn)
              {
                if (col < E)
                {
                  cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
                }
                else if (col == E || col == J)
                {
                  // exempted tax
                  cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
                }
              }

              // is bold
              isBold = (col == lastColumn);
            }

            BorderStyle? leftBorder = (row == rowNo - 2 ? BorderStyle.Thin : null);
            BorderStyle? rightBorder = (row == rowNo - 2 ? BorderStyle.Thin : null);
            BorderStyle? topBorder = (row == rowNo - 2 ? BorderStyle.Thin : null);
            BorderStyle? bottomBorder = (row == rowNo - 2 ? BorderStyle.Thin : (row == rowNo && col > C ? BorderStyle.Thick : null));

            CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                            (row == bodySectionStartRow || (row == rowNo && col == A)) ? IndexedColors.Blue.Index : IndexedColors.Black.Index, null, 10, null,
                            isBold, (row == bodySectionStartRow || (row == rowNo && col == A)), false, false, false,
                            col >= D && col <= lastColumn ? HorizontalAlignment.Right : null, null,
                            leftBorder, rightBorder, topBorder, bottomBorder,
                            cellDataFormat);
          }
        }


        rowNo += 2;
      }

      // summary for date
      maxrow = rowNo;
      rowNo += 2;

      AddDataToCell(sheet, rowNo, 0, _localizer.GetString("totalFor").ToString() + " " + shiftDate.ToString("MM/dd/yyyy"));
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

      // column total
      for (int i = 3; i <= lastColumn; i++)
      {
        ApplyCellFormula(sheet, rowNo, i, $"SUM({ColDFormula[i - 3]})");
      }

      // 'Total for' row styling
      for (int col = A; col <= lastColumn; col++)
      {
        string cellDataFormat = null;
        bool isBold = false;

        if (taxExemptType == "BITE")
        {
          if (col >= D && col <= lastColumn)
          {
            cellDataFormat = ((col == D || col == G || col == J || col == M) ? "###,##0;(###,##0)" : "$###,##0.00;($###,##0.00)"); // Hard coded in VB
          }

          isBold = (col >= M && col <= lastColumn);
        }
        else
        {
          if (col >= D && col <= lastColumn)
          {
            if (col < E)
            {
              cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
            }
            else if (col == E || col == J)
            {
              // exempted tax
              cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
            }
          }

          isBold = (col == J);
        }

        CreateCellStyles(workbook, sheet, style, rowNo, col, GetCellValue(sheet, rowNo, col),
                        col == A ? IndexedColors.Blue.Index : IndexedColors.Black.Index, null, 10, null,
                        isBold, col == A, false, false, false,
                        col >= D && col <= lastColumn ? HorizontalAlignment.Right : null, null,
                        null, null, null, (col > C ? BorderStyle.Thick : null),
                        cellDataFormat);
      }

      rowNo += 2;
    }

    rowNo++;
    maxrow = rowNo;

    /* :::::::::::::::::::::::::::::::::: Report footer section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    AddDataToCell(sheet, rowNo, 0, _localizer.GetString("totalFLSR").ToString() + " " + startDate + " to " + endDate);
    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, D));

    // summary by family for the period
    int footerSectionStartRow = rowNo;
    rowNo += 2;

    var footerRowLevels = saleData?
                    .Join(families,
                        saledata => saledata.Dept,
                        family => family.FamilyID,
                        (saledata, family) => new { saledata, family })
                    .Select(joined => new { joined.saledata.Dept, name = joined.family.Description })
                    .Distinct()
                    .OrderBy(result => result.Dept)
                    .ToList();

    foreach (var level in footerRowLevels)
    {
      rowTformula = "";
      rowDformula = "";
      rowCformula = "";

      AddDataToCell(sheet, rowNo, 0, level.name);
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, B));

      var nonStatusData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == false)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .FirstOrDefault();

      if (nonStatusData == null)
      {
        AddDataToCell(sheet, rowNo, columnNo, 0); // Hard coded in VB
        AddDataToCell(sheet, rowNo, columnNo + 1, 0); // Hard coded in VB
        AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
      }
      else
      {
        AddDataToCell(sheet, rowNo, columnNo, nonStatusData.Volume);
        AddDataToCell(sheet, rowNo, columnNo + 1, nonStatusData.Sales);
        AddDataToCell(sheet, rowNo, columnNo + 2, nonStatusData.Cost);
      }

      // row summary
      rowTformula = IntToExcelColumn(columnNo) + (rowNo + 1);
      rowDformula = IntToExcelColumn(columnNo + 1) + (rowNo + 1);
      rowCformula = IntToExcelColumn(columnNo + 2) + (rowNo + 1);

      if (taxExemptType == "BITE")
      {
        // Member Data
        var memberData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == true
                        && sale.Member == true)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            }).FirstOrDefault();

        if (memberData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 5, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 3, memberData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 4, memberData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 5, memberData.Cost);
        }

        rowTformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
        rowDformula += "," + IntToExcelColumn(columnNo + 4) + (rowNo + 1);
        rowCformula += "," + IntToExcelColumn(columnNo + 5) + (rowNo + 1);

        // Non Member Data
        var nonMemberData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == true
                        && sale.Member == false)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .FirstOrDefault();

        if (nonMemberData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo + 6, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 7, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 8, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 6, nonMemberData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 7, nonMemberData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 8, nonMemberData.Cost);
        }

        rowTformula += "," + IntToExcelColumn(columnNo + 6) + (rowNo + 1);
        rowDformula += "," + IntToExcelColumn(columnNo + 7) + (rowNo + 1);
        rowCformula += "," + IntToExcelColumn(columnNo + 8) + (rowNo + 1);
        lastColumn = columnNo + 8;
      }
      else
      {
        var statusData = saleData
            .Where(sale => sale.Dept == level.Dept
                        && sale.TaxExempt == true)
            .GroupBy(sale => sale.Dept)
            .Select(g => new
            {
              Volume = g.Sum(s => s.Volume ?? 0),
              Sales = g.Sum(s => s.Sales ?? 0),
              ExemptedTax = g.Sum(s => s.ExemptedTax ?? 0),
              Cost = g.Sum(s => s.Cost ?? 0)
            })
            .FirstOrDefault();

        if (statusData == null)
        {
          AddDataToCell(sheet, rowNo, columnNo + 2, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 3, 0); // Hard coded in VB
          AddDataToCell(sheet, rowNo, columnNo + 4, 0); // Hard coded in VB
        }
        else
        {
          AddDataToCell(sheet, rowNo, columnNo + 2, statusData.Volume);
          AddDataToCell(sheet, rowNo, columnNo + 3, statusData.Sales);
          AddDataToCell(sheet, rowNo, columnNo + 4, statusData.ExemptedTax);
        }

        rowTformula += "," + IntToExcelColumn(columnNo + 2) + (rowNo + 1);
        rowDformula += "," + IntToExcelColumn(columnNo + 3) + (rowNo + 1);
        lastColumn = columnNo + 4;
      }

      // row summary
      ApplyCellFormula(sheet, rowNo, lastColumn + 1, $"SUM({rowTformula})");
      ApplyCellFormula(sheet, rowNo, lastColumn + 2, $"SUM({rowDformula})");

      if (taxExemptType == "BITE")
      {
        ApplyCellFormula(sheet, rowNo, lastColumn + 3, $"SUM({rowCformula})");
      }

      lastColumn += 3; // we need to consider row total columns too
      rowNo++;
    }

    int footerRow = rowNo + 1;

    // full footer summary report styling
    lastColumn = lastColRef;

    for (int row = footerSectionStartRow; row <= footerRow; row++)
    {
      for (int col = A; col <= lastColumn; col++)
      {
        string cellDataFormat = null;
        bool isBold = false;
        // cell data format only for footer row
        if (row == footerRow)
        {
          if (taxExemptType == "BITE")
          {
            if (col >= D && col <= lastColumn)
            {
              cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
            }

            isBold = (col >= M && col <= lastColumn);
          }
          else
          {
            if (col >= D && col <= lastColumn)
            {
              if (col < H)
              {
                cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
              else if (col == H || col == J)
              {
                // exempted tax
                cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
              }
              else
              {
                cellDataFormat = (col % 2 == 1) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
            }

            isBold = (col == lastColumn);
          }
        }
        //except footer row
        else
        {
          if (taxExemptType == "BITE")
          {
            if (col >= D && col <= lastColumn)
            {
              cellDataFormat = ((col == D || col == G || col == J || col == M) ? "###,##0;(###,##0)" : "$###,##0.00;($###,##0.00)"); // Hard coded in VB
            }

            isBold = (col >= M && col <= lastColumn);
          }
          else
          {
            if (col >= D && col <= lastColumn)
            {
              if (col < E)
              {
                cellDataFormat = (col % 2 == 0) ? "$###,##0.00;($###,##0.00)" : "###,##0;(###,##0)"; // Hard coded in VB
              }
              else if (col == E || col == J)
              {
                // exempted tax
                cellDataFormat = "$###,##0.00;($###,##0.00)"; // Hard coded in VB
              }
            }

            isBold = (col == lastColumn);
          }
        }

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, IndexedColors.Grey50Percent.Index, 10, null,
                        isBold, false, false, false, false,
                        col >= D && col <= lastColumn ? HorizontalAlignment.Right : null, null,
                        noneBorder, (col == lastColumn ? mediumBorder : noneBorder), (row == footerSectionStartRow ? mediumBorder : noneBorder), (row == footerRow ? mediumBorder : noneBorder),
                        cellDataFormat);
      }
    }


    for (int col = D; col <= lastColumn; col++)
    {
      sheet.AutoSizeColumn(col, true);
    }

    return GetBase64Data(workbook);
  }

  #endregion

  #region Sales Summray Fuel Products Report
  public async Task<FileBase64DataViewModel?> SalesSummaryFuelProductsReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string taxExemptType = excelReportGeneratorViewModel.TaxExemptType;
    string primaryTax = "";
    string secondaryTax = "";

    string url = "TaxMaster/GetTaxes";
    IEnumerable<TaxMasterViewModel> taxes = await _http.GetAndParseAsync<IList<TaxMasterViewModel>>(url);
    var activeTaxes = taxes.Where(tax => tax.Active == true).ToList();

    if (activeTaxes != null)
    {
      primaryTax = activeTaxes[0].Name;

      if (activeTaxes.Count() > 1)
      {
        secondaryTax = activeTaxes[1].Name;
      }
    }

    url = "ExcelReports/GetGradesByFuelType";
    IList<GradeViewModel>? grades = await _http.GetAndParseAsync<IList<GradeViewModel>>(url);

    url = "ExcelReports/GetDistinctDatesFromSaleHead";
    IList<SaleHeadDtoViewModel>? dates = await _http.PostAndParseAsync<IList<SaleHeadDtoViewModel>?>(url, excelReportGeneratorViewModel);

    url = "ExcelReports/GetSummaryFuelProductsReportData";
    IList<SummaryFuelProductsReportDtoViewModel>? data = await _http.PostAndParseAsync<IList<SummaryFuelProductsReportDtoViewModel>?>(url, excelReportGeneratorViewModel);

    url = "CompanySetup/GetCompany";
    company = await _http.GetAndParseAsync<CompanyViewModel>(url);

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("salesSummaryFuelProducts");
    string? startDate = excelReportGeneratorViewModel.StartDate?.ToString("M/d/yyyy");
    string? endDate = excelReportGeneratorViewModel.EndDate?.ToString("M/d/yyyy");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("SALES SUMMARY - FUEL PRODUCTS"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
    BorderStyle noneBorder = BorderStyle.None;
    BorderStyle thickBorder = BorderStyle.Thick;
    BorderStyle mediumBorder = BorderStyle.Medium;

    int processedMonth;
    int processedYear;
    int customerStatus = 0; // 1 - non status, 2 - status no rebate, 3 - status with rebate
    string groupId;
    bool taxExempt;
    int startRow;
    int endRow;
    string processedStock_Code;
    string fuelType;
    int rowNo = 7;
    long totalRowCnt = 8 + (dates.Count() + 3) * (grades.Count() + 3);

    /* :::::::::::::::::::::::::::::::::: Report title section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reporPeriodStr = _localizer.GetString("reportPeriod");
    string reportTitle = company.CompanyName + "   " + reporPeriodStr + " " + startDate + " - " + endDate;

    sheet.AddMergedRegion(new CellRangeAddress(0, 0, A, R));
    AddDataToCell(sheet, 0, 0, reportTitle);

    sheet.AddMergedRegion(new CellRangeAddress(1, 1, A, R));
    AddDataToCell(sheet, 1, 0, reportName);

    /* :::::::::::::::::::::::::::::::::: Report header section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    sheet.AddMergedRegion(new CellRangeAddress(3, 3, D, G));
    AddDataToCell(sheet, 3, D, _localizer.GetString("nonAutochtones").ToString());

    AddDataToCell(sheet, 3, H, _localizer.GetString("autochtones").ToString());
    sheet.AddMergedRegion(new CellRangeAddress(3, 3, H, K));

    sheet.AddMergedRegion(new CellRangeAddress(3, 3, L, P));
    AddDataToCell(sheet, 3, L, _localizer.GetString("autochtonesEtExemption").ToString());

    sheet.AddMergedRegion(new CellRangeAddress(3, 3, Q, R));
    AddDataToCell(sheet, 3, Q, _localizer.GetString("totalFLSR").ToString());

    for (int i = D; i <= O; i += 4)
    {
      AddDataToCell(sheet, 5, i, _localizer.GetString("quantity").ToString());
      AddDataToCell(sheet, 5, i + 1, _localizer.GetString("dollar").ToString());
      AddDataToCell(sheet, 5, i + 2, _localizer.GetString("tps").ToString() + "\n" + (i == D ? _localizer.GetString("incl").ToString() : _localizer.GetString("exempt").ToString()));
      AddDataToCell(sheet, 5, i + 3, _localizer.GetString("tvq").ToString() + "\n" + (i == D ? _localizer.GetString("incl").ToString() : _localizer.GetString("exempt").ToString()));
    }

    AddDataToCell(sheet, 5, P, _localizer.GetString("fuel").ToString() + "\n" + _localizer.GetString("rebate").ToString());
    AddDataToCell(sheet, 5, Q, _localizer.GetString("quantity").ToString());
    AddDataToCell(sheet, 5, R, _localizer.GetString("dollar").ToString());

    // report header styling
    for (int row = 0; row < 7; row++)
    {
      for (int col = A; col <= R; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, (row < 6 ? IndexedColors.Grey50Percent.Index : null), 10, null,
                        true, false, false, true, false,
                        row == 5 ? HorizontalAlignment.Right : HorizontalAlignment.Center, null,
                        (row < 6 ? noneBorder : null), ((col == G || col == K || col == P || (col == R && row != 6)) ? mediumBorder : (row != 6 ? noneBorder : null)), (row != 6 ? noneBorder : null), (row == 5 ? mediumBorder : (row != 6 ? noneBorder : null)),
                        null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report Body section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    foreach (var date in dates)
    {
      int sectionStartRow = rowNo;
      processedMonth = ((int)(date.Month.HasValue ? date.Month : 0));
      processedYear = ((int)(date.Year.HasValue ? date.Year : 0));

      AddDataToCell(sheet, rowNo, 0, DateTime.Parse(processedMonth.ToString("00") + "/01/" + processedYear).ToString("MMM-yy")); // Format(processedMonth, "mmm") & "-" & Format(Date, "yy")
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

      rowNo++;
      string[] arr_Subtotal = new string[14];
      startRow = rowNo;

      for (int idx = 0; idx < grades.Count();)
      {
        var grade = grades[idx];
        fuelType = string.IsNullOrEmpty(grade.FuelType) ? "" : grade.FuelType;

        AddDataToCell(sheet, rowNo, 0, string.IsNullOrEmpty(grade.FullName) ? "" : grade.FullName);

        processedStock_Code = (string.IsNullOrEmpty(grade.StockCode) ? "" : grade.StockCode).Trim();
        var filteredData = data.Where(d => d.Month == processedMonth
                                           && d.Year == processedYear
                                           && d.StockCode == processedStock_Code);

        if (!filteredData.Any())
        {
          for (int i = 3; i < 16; i++)
          {
            AddDataToCell(sheet, rowNo, i, 0);
          }
        }

        foreach (var dataItem in filteredData)
        {
          groupId = string.IsNullOrEmpty(dataItem.GroupID) ? "" : dataItem.GroupID;
          taxExempt = ((bool)(dataItem.TaxExempt.HasValue ? dataItem.TaxExempt : false));

          if (groupId == "" && !taxExempt) customerStatus = 1;
          else if (groupId == "" && taxExempt) customerStatus = 2;
          else if (groupId != "" && taxExempt) customerStatus = 3;

          if (customerStatus == 1)
          {
            if (dataItem.SalesLineTaxName == primaryTax)
            {
              AddDataToCell(sheet, rowNo, 3, ((double)(dataItem.Quantity.HasValue ? dataItem.Quantity : 0)));
              AddDataToCell(sheet, rowNo, 4, ((double)(dataItem.Amount.HasValue ? dataItem.Amount : 0)));
              AddDataToCell(sheet, rowNo, 5, dataItem.SalesLineIncludedTax);
            }
            else if (dataItem.SalesLineTaxName == secondaryTax)
            {
              AddDataToCell(sheet, rowNo, 6, dataItem.SalesLineIncludedTax);
            }
          }
          else if (customerStatus == 2)
          {
            if (dataItem.TaxCreditLineTaxName == primaryTax)
            {
              AddDataToCell(sheet, rowNo, 7, ((double)(dataItem.Quantity.HasValue ? dataItem.Quantity : 0)));
              AddDataToCell(sheet, rowNo, 8, ((double)(dataItem.Amount.HasValue ? dataItem.Amount : 0)));
              AddDataToCell(sheet, rowNo, 9, dataItem.TaxCreditLineIncludedTax);
            }
            else if (dataItem.TaxCreditLineTaxName == secondaryTax)
            {
              AddDataToCell(sheet, rowNo, 10, dataItem.TaxCreditLineIncludedTax);
            }
          }
          else if (customerStatus == 3)
          {
            if (dataItem.TaxCreditLineTaxName == primaryTax)
            {
              AddDataToCell(sheet, rowNo, 11, ((double)(dataItem.Quantity.HasValue ? dataItem.Quantity : 0)));
              AddDataToCell(sheet, rowNo, 12, ((double)(dataItem.Amount.HasValue ? dataItem.Amount : 0)));
              AddDataToCell(sheet, rowNo, 13, dataItem.TaxCreditLineIncludedTax);
              AddDataToCell(sheet, rowNo, 15, ((double)(dataItem.Rebate.HasValue ? dataItem.Rebate : 0)));
            }
            else if (dataItem.TaxCreditLineTaxName == secondaryTax)
            {
              AddDataToCell(sheet, rowNo, 14, dataItem.TaxCreditLineIncludedTax);
            }
          }
        }

        // built the formulas for quantity and dollar totals by row
        ApplyCellFormula(sheet, rowNo, 16, $"SUM(D{rowNo + 1},H{rowNo + 1},L{rowNo + 1})");
        ApplyCellFormula(sheet, rowNo, 17, $"SUM(E{rowNo + 1},I{rowNo + 1},M{rowNo + 1})");

        idx++;

        if (idx >= grades.Count()) rowNo++;
        else
        {
          grade = grades[idx];

          if (grade.FuelType == fuelType) rowNo++;
          else
          {
            endRow = rowNo++;

            // build the formulas for subtotal by date for pump fuel (fuel type "G")
            AddDataToCell(sheet, rowNo, 0, _localizer.GetString("subtotal").ToString());

            for (int i = 3; i < 16; i++)
            {
              ApplyCellFormula(sheet, rowNo, i, $"SUM({IntToExcelColumn(i)}{startRow + 1}:{IntToExcelColumn(i)}{endRow + 1})");
            }

            // built the formulas for quantity and dollar totals by row
            ApplyCellFormula(sheet, rowNo, 16, $"SUM(D{rowNo + 1},H{rowNo + 1},L{rowNo + 1})");
            ApplyCellFormula(sheet, rowNo, 17, $"SUM(E{rowNo + 1},I{rowNo + 1},M{rowNo + 1})");

            startRow = rowNo; // row no is new startRow fro the Total formula
            rowNo++;
          }
        }
      }

      // build the formulas for total by date
      endRow = rowNo - 1;

      AddDataToCell(sheet, rowNo, 0, _localizer.GetString("totalFLSR").ToString() + " " + DateTime.Parse(processedMonth.ToString("00") + "/01/" + processedYear).ToString("MMM-yyyy")); // Format(processedMonth, "mmm-dd-yyyy")
      sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, A, C));

      for (int i = 3; i < 16; i++)
      {
        ApplyCellFormula(sheet, rowNo, i, $"SUM({IntToExcelColumn(i)}{startRow + 1}:{IntToExcelColumn(i)}{endRow + 1})");
      }

      // built the formulas for quantity and dollar totals by row
      ApplyCellFormula(sheet, rowNo, 16, $"SUM(D{rowNo + 1},H{rowNo + 1},L{rowNo + 1})");
      ApplyCellFormula(sheet, rowNo, 17, $"SUM(E{rowNo + 1},I{rowNo + 1},M{rowNo + 1})");

      // body section styling
      for (int row = sectionStartRow; row <= rowNo; row++)
      {
        for (int col = A; col <= R; col++)
        {
          string cellDataFormat = null;

          cellDataFormat = (col == D || col == H || col == L || col == Q) ? "###,##0.00;(###,##0.00)" : "###,##0.00$;(###,##0.00)$";

          CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                          row == sectionStartRow || row == rowNo && col == A ? IndexedColors.Blue.Index : IndexedColors.Black.Index, null, 8, null,
                          (col == Q || col == R), false, false, false, false,
                          col == A ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                          null, ((col == G || col == K || col == P) ? mediumBorder : null), null, (row == rowNo ? thickBorder : null),
                          cellDataFormat);
        }
      }

      rowNo += 2;
    }

    for (int col = A; col <= R; col++)
    {
      sheet.AutoSizeColumn(col);
    }

    return GetBase64Data(workbook);
  }
  #endregion

  #region North 60/YTG Fleet Card Report
  public async Task<FileBase64DataViewModel?> North60OrYTGFleetCardReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    bool isYTG = (excelReportGeneratorViewModel.ReportNo == 7 ? false : true);

    string url = $"ExcelReports/GetNorth60OrYTGFleetCardReportData?isYTG={isYTG}";
    IList<FleetCardReportDtoViewModel>? data = await _http.PostAndParseAsync<IList<FleetCardReportDtoViewModel>?>(url, excelReportGeneratorViewModel);

    url = "CompanySetup/GetCompany";
    company = await _http.GetAndParseAsync<CompanyViewModel>(url);

    url = "ExcelReports/GetAmountFromSaleLine";
    IList<SaleLineViewModel>? amounts = await _http.GetAndParseAsync<IList<SaleLineViewModel>>(url);

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString(isYTG ? "ytgFleetCardReport" : "north60FleetCardReport");
    string sheetName = isYTG ? "YTG Fleet Card Report" : "NORTH 60 FLEET CARD REPORT"; // Hard coded in VB code, Sheet name shouldn't be localized
    string? startDate = excelReportGeneratorViewModel.StartDate?.ToString("M/d/yyyy");
    string? endDate = excelReportGeneratorViewModel.EndDate?.ToString("M/d/yyyy");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet(sheetName);
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

    int rowNo = 9;
    decimal? totalAmount = 0;
    long lngSaleNo = 0;

    // add work sheet style
    sheet.SetColumnWidth(A, 20 * 256); // Set the width of column A (index 0) to 20 characters
    sheet.SetColumnWidth(B, 15 * 256); // Set the width of column B (index 1) to 15 characters
    sheet.SetColumnWidth(C, 12 * 256); // Set the width of column C (index 2) to 12 characters
    sheet.SetColumnWidth(D, 10 * 256); // Set the width of column D (index 3) to 10 characters
    sheet.SetColumnWidth(E, 10 * 256); // Set the width of column E (index 4) to 10 characters
    sheet.SetColumnWidth(G, 10 * 256); // Set the width of column G (index 6) to 10 characters
    sheet.SetColumnWidth(I, 12 * 256); // Set the width of column I (index 8) to 12 characters
    sheet.SetColumnWidth(J, 15 * 256); // Set the width of column J (index 9) to 15 characters
    sheet.SetColumnWidth(L, 15 * 256); // Set the width of column L (index 11) to 15 characters
    sheet.SetColumnWidth(N, 15 * 256); // Set the width of column N (index 13) to 15 characters

    /* :::::::::::::::::::::::::::::::::: Report Header section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    // First Row
    sheet.AddMergedRegion(new CellRangeAddress(0, 0, A, B));
    AddDataToCell(sheet, 0, A, _localizer.GetString(isYTG ? "ytgFleetVehicleAgency" : "north60Petroleum").ToString());

    sheet.AddMergedRegion(new CellRangeAddress(0, 0, C, D));
    AddDataToCell(sheet, 0, C, _localizer.GetString(isYTG ? "ytgAccountNo" : "accountNo").ToString());

    sheet.AddMergedRegion(new CellRangeAddress(0, 0, F, I));
    AddDataToCell(sheet, 0, F, $"{_localizer.GetString("from").ToString()} {_localizer.GetString("yukonTireCentreInc").ToString()}");

    sheet.AddMergedRegion(new CellRangeAddress(0, 0, L, M));
    AddDataToCell(sheet, 0, L, _localizer.GetString("invoiceNo").ToString());

    // Second Row
    sheet.AddMergedRegion(new CellRangeAddress(1, 1, A, B));
    AddDataToCell(sheet, 1, A, _localizer.GetString(isYTG ? "attnSerge" : "attnJenBiggar").ToString());

    sheet.AddMergedRegion(new CellRangeAddress(1, 1, F, I));
    AddDataToCell(sheet, 1, F, _localizer.GetString(isYTG ? "ytgVendorId" : "vendorId").ToString());

    // Third Row
    sheet.AddMergedRegion(new CellRangeAddress(2, 2, A, B));
    AddDataToCell(sheet, 2, A, _localizer.GetString(isYTG ? "ytgBuilding" : "industrialRoad146").ToString());

    sheet.AddMergedRegion(new CellRangeAddress(2, 2, F, I));
    AddDataToCell(sheet, 2, F, _localizer.GetString("industrialRoad107").ToString());

    // Fourth Row
    sheet.AddMergedRegion(new CellRangeAddress(3, 3, A, B));
    AddDataToCell(sheet, 3, A, _localizer.GetString(isYTG ? "quartzRoad" : "whiteHorseV1").ToString());

    sheet.AddMergedRegion(new CellRangeAddress(3, 3, F, I));
    AddDataToCell(sheet, 3, F, _localizer.GetString("whiteHorseT7").ToString());

    // Fifth Row
    sheet.AddMergedRegion(new CellRangeAddress(4, 4, A, B));
    AddDataToCell(sheet, 4, A, _localizer.GetString(isYTG ? "whiteHorseP9" : "jbiggarEmail").ToString());

    // Sixth Row
    AddDataToCell(sheet, 5, A, _localizer.GetString(isYTG ? "ytgFaxNo" : "faxNo").ToString());
    AddDataToCell(sheet, 5, B, _localizer.GetString(isYTG ? "ytgPhoneNo" : "phoneNo").ToString());
    AddDataToCell(sheet, 5, J, _localizer.GetString("discount").ToString());

    // Seventh Row
    sheet.AddMergedRegion(new CellRangeAddress(6, 6, A, N));
    AddDataToCell(sheet, 6, A, _localizer.GetString(isYTG ? "ytgFleetCardInvoiceWorksheet" : "north60FleetCardInvoiceWorksheet").ToString());

    for (int col = A; col <= N; col++)
    {
      sheet.AddMergedRegion(new CellRangeAddress(7, 9, col, col));
    }

    // eighth to tenth Row (column header)
    AddDataToCell(sheet, 7, A, _localizer.GetString("cardNumber").ToString());
    AddDataToCell(sheet, 7, B, _localizer.GetString("creditSlip").ToString());
    AddDataToCell(sheet, 7, C, _localizer.GetString("transactionDate").ToString());
    AddDataToCell(sheet, 7, D, _localizer.GetString("litres").ToString());
    AddDataToCell(sheet, 7, E, _localizer.GetString("dollarOrLitreOfFuel").ToString());
    AddDataToCell(sheet, 7, F, _localizer.GetString("typeOfFuel").ToString());
    AddDataToCell(sheet, 7, G, _localizer.GetString("dollarAmount").ToString());
    AddDataToCell(sheet, 7, H, _localizer.GetString("discAmount").ToString());
    AddDataToCell(sheet, 7, I, _localizer.GetString("gasTotalAfterDisc").ToString());
    AddDataToCell(sheet, 7, J, _localizer.GetString("miscellaneous").ToString());
    AddDataToCell(sheet, 7, K, _localizer.GetString("amt").ToString());
    AddDataToCell(sheet, 7, L, _localizer.GetString("totalAmountOwingAfterDiscount").ToString());
    AddDataToCell(sheet, 7, M, _localizer.GetString("fuelGst").ToString());
    AddDataToCell(sheet, 7, N, _localizer.GetString("totalAfterGstRemoved").ToString());

    // Report header full styling
    for (int row = 0; row <= 9; row++)
    {
      for (int col = A; col <= N; col++)
      {
        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        (row == 4 && !isYTG) ? IndexedColors.Blue.Index : IndexedColors.Black.Index, null, (short?)((row == 0 && col < J) ? 14 : 10), "Calibri",
                        (row != 4 && !isYTG) || isYTG, false, (row == 4 && !isYTG), true, (row == 4 && col == A && !isYTG),
                        (row == 6 || row == 7 || (row == 5 && col == 10) || col == F) ? HorizontalAlignment.Center : HorizontalAlignment.Left, null,
                        null, null, null, null,
                        (row == 5 && col == 10) ? "$0.00;$(0.00)" : null);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report Body section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    rowNo++;
    int bodySectionStartRow = rowNo;

    AddDataToCell(sheet, 5, 10, 0);

    foreach (var item in data)
    {
      for (int i = 0; i < 14; i++)
      {
        if (i == 0)
        {
          AddDataToCell(sheet, rowNo, i, "'" + item.CardNumber.ToString());
        }
        else if (i == 7)
        {
          ApplyCellFormula(sheet, rowNo, 7, $"G{rowNo + 1}-I{rowNo + 1}");
        }
        else if (i == 8)
        {
          ApplyCellFormula(sheet, rowNo, 8, $"(E{rowNo + 1}-K6)*D{rowNo + 1}");
        }
        else if (i == 10)
        {
          totalAmount = (item.SaleHeadAmount == null ? 0 : item.SaleHeadAmount);
          var fuelAmount = amounts
                            .Where(amt => amt.SaleNo == item.SaleNo)
                            .Sum(sl => sl.Amount) ?? 0;

          if ((totalAmount - fuelAmount != 0) && (lngSaleNo == item.SaleNo))
          {
            AddDataToCell(sheet, rowNo, 10, totalAmount - fuelAmount);
          }
          else
          {
            AddDataToCell(sheet, rowNo, 10, 0);
          }
        }
        else if (i == 11)
        {
          ApplyCellFormula(sheet, rowNo, 11, $"I{rowNo + 1}+ K{rowNo + 1}");
        }
        else if (i == 12)
        {
          ApplyCellFormula(sheet, rowNo, 12, $"I{rowNo + 1}- I{rowNo + 1}/1.05"); // Hard coded in VB
        }
        else if (i == 13)
        {
          ApplyCellFormula(sheet, rowNo, 13, $"L{rowNo + 1}- M{rowNo + 1}");
        }
      }

      AddDataToCell(sheet, rowNo, B, item.SaleNo);
      AddDataToCell(sheet, rowNo, C, item.SaleDate?.ToString("dd/MM/yyyy"));
      AddDataToCell(sheet, rowNo, D, item.Quantity == null ? 0 : item.Quantity);
      AddDataToCell(sheet, rowNo, E, item.Price == null ? 0 : item.Price);
      AddDataToCell(sheet, rowNo, F, string.IsNullOrWhiteSpace(item.Descript) ? "" : item.Descript);
      AddDataToCell(sheet, rowNo, G, item.SaleLineAmount == null ? 0 : item.SaleLineAmount);
      AddDataToCell(sheet, rowNo, J, item.Misc);

      rowNo++;
      lngSaleNo = item.SaleNo;
    }

    // display the totals
    for (int i = D; i <= N; i++)
    {
      if (i == E || i == F || i == J) continue;

      ApplyCellFormula(sheet, rowNo + 1, i, $"SUM({IntToExcelColumn(i)}10:{IntToExcelColumn(i)}{rowNo + 1})");
    }

    // Report body styling
    for (int row = bodySectionStartRow; row <= rowNo + 1; row++)
    {
      for (int col = A; col <= N; col++)
      {
        string cellDataFormat = null;

        if (col >= D)
        {
          if (col == D || col == E)
          {
            cellDataFormat = "###,##0.000;(###,##0.000)";
          }
          else if (col != F && col != J)
          {
            cellDataFormat = "###,##0.00;(###,##0.00)";
          }
        }

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, (short?)((row == rowNo + 1) ? 11 : 10), "Calibri",
                        false, false, false, false, false,
                        (col == A || col == C || col == F || col == J) ? HorizontalAlignment.Left : HorizontalAlignment.Right, null,
                        null, null, null, null,
                        cellDataFormat);
      }
    }

    // Get base64data
    return GetBase64Data(workbook);
  }
  #endregion

  #region Sales By Hour Report
  public async Task<FileBase64DataViewModel?> SalesByHourReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string version = excelReportGeneratorViewModel.VersionPolicy;
    excelReportGeneratorViewModel.EndDate = excelReportGeneratorViewModel.StartDate?.AddDays(6);

    string url = $"ExcelReports/GetSalesByHourReport";
    IList<SaleHeadViewModel>? saleHeadData = await _http.PostAndParseAsync<IList<SaleHeadViewModel>?>(url, excelReportGeneratorViewModel);
    saleHeadData.OrderBy(d => d.SaleTime)
                .ToList();

    url = $"CompanySetup/GetCompany";
    company = await _http.GetAndParseAsync<CompanyViewModel>(url);

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("salesByHourReport");
    DateTime? startDate = excelReportGeneratorViewModel.StartDate;
    DateTime? endDate = excelReportGeneratorViewModel.EndDate;

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("SALES BY HOUR REPORT"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

    int maxRow = 99; // hard coded in VB
    int headerRow = 5; // hard coded in VB
    DateTime rangeTimeFrom = DateTime.Today;
    DateTime rangeTimeTo = DateTime.Today.AddDays(1).AddTicks(-1);

    TimeSpan elevenPM = new TimeSpan(23, 0, 0);
    if (rangeTimeTo.TimeOfDay > elevenPM)
    {
      rangeTimeTo = DateTime.Today.Add(elevenPM); // Set to 11:00:00 PM
    }

    DateTime tempHoldTime = rangeTimeFrom;
    DateTime? tempHoldTime2;
    DateTime tempHoldTime3;
    int excelLine = 0;
    int k = 0; // column index

    /* :::::::::::::::::::::::::::::::::: Report Body section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    while (tempHoldTime <= rangeTimeTo)
    {
      excelLine++;
      tempHoldTime3 = tempHoldTime.AddSeconds(3599);
      AddDataToCell(sheet, headerRow + excelLine, 1, tempHoldTime.ToString("HH:mm"));
      k = 1;
      tempHoldTime2 = startDate;

      while (tempHoldTime2 <= endDate)
      {
        k++;

        if (excelLine == 1)
        {
          AddDataToCell(sheet, headerRow, k, _localizer.GetString(tempHoldTime2?.DayOfWeek.ToString().ToLower()).ToString());
          sheet.SetColumnWidth(k, 15 * 256); // Set the width of columns (index k) to 11 characters
        }

        if (version == "MVCC")
        {
          var saleAmtSum = saleHeadData
            .Where(sh => sh.SaleDate?.Date == tempHoldTime2?.Date)
            .Where(sh => sh.SD?.TimeOfDay >= tempHoldTime.TimeOfDay && sh.SD?.TimeOfDay <= tempHoldTime3.TimeOfDay)
            .Where(sh => sh.TType != "PUMPTEST" && sh.TType != "DriveOff")
            .Sum(sh => sh.SaleAmount ?? 0);

          AddDataToCell(sheet, headerRow + excelLine, k, saleAmtSum);
        }
        else
        {
          var saleAmtSum = saleHeadData
            .Where(sh => sh.ShiftDate?.Date == tempHoldTime2?.Date)
            .Where(sh => sh.SaleTime?.TimeOfDay >= tempHoldTime.TimeOfDay && sh.SaleTime?.TimeOfDay <= tempHoldTime3.TimeOfDay)
            .Sum(sh => sh.SaleAmount ?? 0);

          AddDataToCell(sheet, headerRow + excelLine, k, saleAmtSum);
        }

        tempHoldTime2 = tempHoldTime2?.AddDays(1);
      }

      tempHoldTime = tempHoldTime.AddHours(1);
    }

    AddDataToCell(sheet, headerRow, k + 1, _localizer.GetString("totalFLSR").ToString());
    AddDataToCell(sheet, headerRow, k + 3, _localizer.GetString("avgSales").ToString());
    AddDataToCell(sheet, headerRow, k + 4, _localizer.GetString("totalSalesPercentage").ToString());

    int i = 1;

    for (i = (headerRow + 1); i <= (headerRow + excelLine); i++)
    {
      ApplyCellFormula(sheet, i, k + 1, $"SUM(C{i + 1}:{IntToExcelColumn(k)}{i + 1})");
      ApplyCellFormula(sheet, i, k + 3, $"AVERAGE(C{i + 1}:{IntToExcelColumn(k)}{i + 1})");
    }

    ApplyCellFormula(sheet, i + 1, k + 1, $"SUM({IntToExcelColumn(k + 1)}{headerRow + 2}:{IntToExcelColumn(k + 1)}{i})");
    ApplyCellFormula(sheet, i + 1, k + 3, $"AVERAGE(C{i + 2}:{IntToExcelColumn(k)}{i + 2})");

    for (i = headerRow + 1; i <= (headerRow + excelLine); i++)
    {
      ApplyCellFormula(sheet, i, k + 4, $"SUM({IntToExcelColumn(k + 3)}{i + 1}/{IntToExcelColumn(k + 3)}{headerRow + excelLine + 3})");
    }

    i = 1;
    excelLine++;
    tempHoldTime2 = startDate;
    while (tempHoldTime2 <= endDate)
    {
      i++;
      ApplyCellFormula(sheet, headerRow + excelLine + 1, i, $"SUM({IntToExcelColumn(i)}{headerRow + 2}:{IntToExcelColumn(i)}{excelLine + headerRow + 1})");
      tempHoldTime2 = tempHoldTime2?.AddDays(1);
    }

    // set column width
    sheet.SetColumnWidth(0, 3 * 256); // Set the width of column A (index 0) to 3 characters
    sheet.SetColumnWidth(k + 1, 18 * 256); // Set the width of column to 11 characters
    sheet.SetColumnWidth(k + 2, 3 * 256); // Set the width of column to 3 characters
    sheet.SetColumnWidth(k + 3, 12 * 256); // Set the width of column 10 characters
    sheet.SetColumnWidth(k + 4, 13 * 256); // Set the width of column 13 characters

    // report body styling
    for (int row = 6; row < 32; row++)
    {
      for (int col = B; col <= M; col++)
      {
        string cellDataFormat = null;

        if (col > B && col < M)
        {
          cellDataFormat = (version == "MVCC" ? "##,##0.00_);(##,##0.00)" : "$#,##0.00_);($#,##0.00)");
        }
        else if (col == M)
        {
          cellDataFormat = "0.00%";
        }

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, 10, null,
                        col == B || row == 31, false, false, false, false,
                        HorizontalAlignment.Right, null,
                        null, null, null, null,
                        cellDataFormat);
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report Header section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportTitle = _localizer.GetString("salesByHourForThePeriodOf").ToString() + " " + startDate?.DayOfWeek + " " + startDate?.ToString("MMM dd, yyyy") + " to " + endDate?.DayOfWeek + " " + endDate?.ToString("MMM dd, yyyy");
    AddDataToCell(sheet, headerRow - 3, 1, reportTitle);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow - 3, headerRow - 3, 1, k + 4));

    AddDataToCell(sheet, headerRow - 2, 1, _localizer.GetString("store").ToString() + " # " + company.StoreCode);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow - 2, headerRow - 2, 1, k + 4));

    AddDataToCell(sheet, headerRow - 4, 1, company.CompanyName);
    sheet.AddMergedRegion(new CellRangeAddress(headerRow - 4, headerRow - 4, 1, k + 4));

    // Report header styling
    for (int row = 1; row < 6; row++)
    {
      for (int col = B; col <= M; col++)
      {
        short? fontSize = (short?)(row == 1 ? 15 : (row == 3 ? 10 : 9));

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, fontSize, null,
                        true, false, false, true, false,
                        (row == 1 || row == 2) ? HorizontalAlignment.Center : HorizontalAlignment.Left, null,
                        null, null, null, null,
                        null);
      }
    }

    // Get base 64 data
    return GetBase64Data(workbook);
  }
  #endregion

  #region FNGTR Fuel Sales Report
  public async Task<FileBase64DataViewModel?> FNGTRFuelSalesReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string url = $"CompanySetup/GetCompany";
    company = await _http.GetAndParseAsync<CompanyViewModel>(url);
    string storeNumber = (string.IsNullOrEmpty(company.CompanyName) ? "" : company.CompanyName);

    url = $"ExcelReports/GetAllReportData";
    IList<ReportDataViewModel>? reportData = await _http.GetAndParseAsync<IList<ReportDataViewModel>?>(url);

    /* :::::::::::::::::::::::::::::::::: Report section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    string reportName = _localizer.GetString("fngtrFuelSalesReport");

    IWorkbook workbook = new XSSFWorkbook();
    ISheet sheet = workbook.CreateSheet("FNGTR FUEL SALES REPORT"); // Hard coded in VB code, Sheet name shouldn't be localized
    XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();

    int rowNo = 0;
    long lngSaleNo = 0;

    /* :::::::::::::::::::::::::::::::::: Report header section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    // header
    AddDataToCell(sheet, rowNo, 0, _localizer.GetString("site").ToString());
    AddDataToCell(sheet, rowNo, 1, _localizer.GetString("dateOrTime").ToString());
    AddDataToCell(sheet, rowNo, 2, _localizer.GetString("saleNo").ToString());
    AddDataToCell(sheet, rowNo, 3, _localizer.GetString("treatyNo").ToString());
    AddDataToCell(sheet, rowNo, 4, _localizer.GetString("product").ToString());
    AddDataToCell(sheet, rowNo, 5, _localizer.GetString("description").ToString());
    AddDataToCell(sheet, rowNo, 6, _localizer.GetString("price").ToString());
    AddDataToCell(sheet, rowNo, 7, _localizer.GetString("taxExemptPrice").ToString());
    AddDataToCell(sheet, rowNo, 8, _localizer.GetString("quantity").ToString());
    AddDataToCell(sheet, rowNo, 9, _localizer.GetString("exciseTaxExempt").ToString());
    AddDataToCell(sheet, rowNo, 10, _localizer.GetString("saleTotal").ToString());
    AddDataToCell(sheet, rowNo, 11, _localizer.GetString("phoneNoStr").ToString());
    AddDataToCell(sheet, rowNo, 12, _localizer.GetString("reason").ToString());

    // set column width
    sheet.SetColumnWidth(0, 30 * 256); // Set the width of column A (index 0) to 30 characters
    sheet.SetColumnWidth(1, 25 * 256); // Set the width of column B (index 1) to 25 characters
    sheet.SetColumnWidth(2, 10 * 256); // Set the width of column C (index 2) to 10 characters
    sheet.SetColumnWidth(3, 12 * 256); // Set the width of column D (index 3) to 12 characters
    sheet.SetColumnWidth(4, 15 * 256); // Set the width of column E (index 4) to 15 characters
    sheet.SetColumnWidth(5, 15 * 256); // Set the width of column F (index 5) to 15 characters
    sheet.SetColumnWidth(6, 10 * 256); // Set the width of column G (index 6) to 10 characters
    sheet.SetColumnWidth(7, 20 * 256); // Set the width of column H (index 7) to 20 characters
    sheet.SetColumnWidth(8, 10 * 256); // Set the width of column I (index 8) to 10 characters
    sheet.SetColumnWidth(9, 17 * 256); // Set the width of column J (index 9) to 17 characters
    sheet.SetColumnWidth(10, 10 * 256); // Set the width of column K (index 10) to 10 characters
    sheet.SetColumnWidth(11, 12 * 256); // Set the width of column L (index 11) to 12 characters
    sheet.SetColumnWidth(12, 20 * 256); // Set the width of column M (index 12) to 20 characters

    for (int row = 0; row <= reportData.Count(); row++)
    {
      for (int col = A; col <= M; col++)
      {
        ICell cell = GetCell(sheet, row, col);
        cell.CellStyle = style;
      }
    }

    /* :::::::::::::::::::::::::::::::::: Report body section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    foreach (var data in reportData)
    {
      lngSaleNo = (long)data.SaleNo;

      // first column
      AddDataToCell(sheet, rowNo + 1, 0, storeNumber);

      // second column
      AddDataToCell(sheet, rowNo + 1, 1, data.SaleDate.HasValue ? data.SaleDate?.ToString("MM/dd/yyyy hh:mm:ss tt") : "");

      // third column
      AddDataToCell(sheet, rowNo + 1, 2, data.SaleNo.HasValue ? data.SaleNo.ToString() : "");

      // fourth column
      AddDataToCell(sheet, rowNo + 1, 3, string.IsNullOrEmpty(data.TreatyNo) ? data.TreatyNo : "");

      // fifth column
      AddDataToCell(sheet, rowNo + 1, 4, string.IsNullOrEmpty(data.Product) ? data.Product : "");

      // sixth column
      AddDataToCell(sheet, rowNo + 1, 5, string.IsNullOrEmpty(data.Description) ? data.Description : "");

      // seventh column
      AddDataToCell(sheet, rowNo + 1, 6, data.Price.HasValue ? data.Price : 0);

      // eighth column
      AddDataToCell(sheet, rowNo + 1, 7, data.TaxExemptedPrice.HasValue ? data.TaxExemptedPrice : 0);

      // ninth column
      AddDataToCell(sheet, rowNo + 1, 8, data.Quantity.HasValue ? data.Quantity : 0);

      // Tenth column
      AddDataToCell(sheet, rowNo + 1, 9, data.TotalExemptedTax.HasValue ? data.TotalExemptedTax : 0);

      // Eleventh column
      AddDataToCell(sheet, rowNo + 1, 10, data.SaleTotal.HasValue ? data.SaleTotal : 0);

      // Twelfth column
      AddDataToCell(sheet, rowNo + 1, 11, data.PhoneNo.HasValue ? data.PhoneNo : "");

      // Thirteenth column
      AddDataToCell(sheet, rowNo + 1, 12, string.IsNullOrEmpty(data.Reason) ? data.Reason : "");

      rowNo++;
    }

    // Full report styling
    for (int row = 0; row <= rowNo; row++)
    {
      for (int col = A; col <= M; col++)
      {
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
        string cellDataFormat = null;

        if (row == 0)
        {
          horizontalAlignment = HorizontalAlignment.Center;
        }
        else
        {
          // alignment
          if (col == C || (col >= G && col <= L))
          {
            horizontalAlignment = HorizontalAlignment.Right;
          }

          // cell data format
          if (col == A)
          {
            cellDataFormat = "000"; // hard coded in VB
          }
          else if (col == G || col == H)
          {
            cellDataFormat = "###,##0.00"; // hard coded in VB
          }
          else if (col >= I && col <= K)
          {
            cellDataFormat = "###,##0.000"; // hard coded in vB
          }
        }

        CreateCellStyles(workbook, sheet, style, row, col, GetCellValue(sheet, row, col),
                        IndexedColors.Black.Index, null, (short?)(row == 0 ? 11 : 10), "Calibri",
                        row == 0, false, false, false, false,
                        horizontalAlignment, null,
                        null, null, null, null,
                        cellDataFormat);
      }
    }

    // Get base 64 data
    return GetBase64Data(workbook);
  }
  #endregion

  #region Text Reports

  public async Task<FileBase64DataViewModel> ConvertTextReportToExcelAsync(TextReportParamsViewModel textReportParams)
  {
    if (_config.IsBackOffice)
    {
      textReportParams.StoreID = null;
    }

    string policyValue = _policyService.GetPolicy(PolicyName.TIMEFORMAT).ToString();
    timeFormat = policyValue.Equals(AppConstants.TIME_FORMAT_24H) ? AppConstants.HOUR_MINUTE : AppConstants.HOUR_MINUTE_SECOND;

    return textReportParams?.ReportName! switch
    {
      "T0009" => await GetFleetCardSaleReportAsync(textReportParams),
      "T0018" => await GetCustomerSalesDetailReportAsync(textReportParams),
      "T0019" => await GetCustomerSalesDetailReportAsync(textReportParams),
    };
  }

  #region Fleet Card Sale Report

  private async Task<FileBase64DataViewModel> GetFleetCardSaleReportAsync(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetFleetCardSaleReportData";
    var fleetCardSaleData = await _http.PostAndParseAsync<IEnumerable<FleetCardSaleViewModel>?>(url, textReportParams);
    int[] columnPadding = { 10, 15, 30, 16, 15, 13, 13, 10, 15, 30, 13, 12, 15, 15, 15, 16, 10 };
    var workbook = new XSSFWorkbook();
    var sheet = workbook.CreateSheet();

    if (fleetCardSaleData == null || !fleetCardSaleData.Any())
    {
      return GetBase64Data(workbook);
    }


    // Create a cell style for center alignment
    var centerStyle = workbook.CreateCellStyle();
    centerStyle.Alignment = HorizontalAlignment.Center;
    centerStyle.VerticalAlignment = VerticalAlignment.Center;
    var rowIdx = 0;
    var colIdx = 0;

    // Title
    var index = textReportParams.StoreName!.IndexOf('-');
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, textReportParams.StoreName.Remove(index, 1).Insert(index, " "), centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, _localizer.GetString("fleetCardSaleReport").Value, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, $"{_localizer.GetString("startDate").Value}: {textReportParams.StartDate.Value:dd-MMM-yyyy} {_localizer.GetString("endDate").Value}: {textReportParams.EndDate.Value:dd-MMM-yyyy}", centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);

    for (var i = 0; i < rowIdx; i++)
    {
      sheet.AddMergedRegion(new CellRangeAddress(i, i, 0, columnPadding.Length - 1));
    }

    /* :::::::::::::::::::::::::::::::::: Report header section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    // header
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("saleNo").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("cardName").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("cardNumber").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("date").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("time").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("saleAmount").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("tendAmount").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("type").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("line").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("produit").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("qty").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("price").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("net").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("tax").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("charges").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, $"{_localizer.GetString("discount").Value}/{_localizer.GetString("rebate").Value}");
    AddDataToCell(sheet, rowIdx++, colIdx++, _localizer.GetString("total").Value);

    for(var j = 0; j < columnPadding.Length; j++)
    {
      sheet.SetColumnWidth(j, columnPadding[j] * 256);
    }

    decimal tendAmountSum = 0;
    decimal saleAmountSum = 0;
    decimal quantitySum = 0;
    var fleetCardGroupedBySaleNo = fleetCardSaleData.GroupBy(x => x.SaleNo);

    foreach (var fleetCard in fleetCardGroupedBySaleNo)
    {
      var orderedFleetCard = fleetCard.OrderBy(x => x.LineNumber);
      var firstCard = orderedFleetCard.First();
      tendAmountSum += firstCard.TendAmount;
      saleAmountSum += firstCard.SaleAmount;

      foreach (var card in orderedFleetCard)
      {
        colIdx = 0;
        quantitySum += card.Quantity;
        var taxes = card.LineTax + card.SaleKitTax + card.ChargeTax;
        var charges = card.SaleCharge + card.SaleKitCharge;
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.SaleNo.ToString());
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.CardName);
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.CardNumber);
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.TransactionDate.Value, GetDataFormat(workbook, "yyyy-MM-dd"));
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.TransactionTime.Value.ToString("HH:mm:ss"), GetDataFormat(workbook, "HH:mm:ss"));
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.SaleAmount, GetDataFormat(workbook, "#0.00"));
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.TendAmount, GetDataFormat(workbook, "#0.00"));
        AddDataToCell(sheet, rowIdx, colIdx++, firstCard.Type);
        AddDataToCell(sheet, rowIdx, colIdx++, card.LineNumber, GetDataFormat(workbook, "##"));
        AddDataToCell(sheet, rowIdx, colIdx++, card.Description);
        AddDataToCell(sheet, rowIdx, colIdx++, card.Quantity, GetDataFormat(workbook, "#0.000"));
        AddDataToCell(sheet, rowIdx, colIdx++, card.Price, GetDataFormat(workbook, "#0.000"));
        AddDataToCell(sheet, rowIdx, colIdx++, card.Amount, GetDataFormat(workbook, "#0.00"));
        AddDataToCell(sheet, rowIdx, colIdx++, taxes, GetDataFormat(workbook, "#0.00"));
        AddDataToCell(sheet, rowIdx, colIdx++, charges, GetDataFormat(workbook, "#0.00"));
        AddDataToCell(sheet, rowIdx, colIdx++, card.Discount, GetDataFormat(workbook, "#0.00"));
        AddDataToCell(sheet, rowIdx++, colIdx++, card.Amount + taxes + charges - card.Discount, GetDataFormat(workbook, "#0.00"));
      }
    }

    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);

    for(int i = rowIdx; i < rowIdx + 3; i++)
    {
      sheet.AddMergedRegion(new CellRangeAddress(i, i, 0, columnPadding.Length - 1));
    }

    AddDataToCell(sheet, rowIdx++, 0, $"{_localizer.GetString("totalFleetCardTenderedAmount")}: {tendAmountSum.ToString("$#0.00")}", centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, $"{_localizer.GetString("totalFleetCardSaleAmount")}: {saleAmountSum.ToString("$#0.00")}", centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, $"{_localizer.GetString("totalFleetCardSaleVolume")}: {quantitySum.ToString("$#0.00")}", centerStyle);

    // Get base 64 data
    return GetBase64Data(workbook);
  }

  #endregion Fleet Card Sale Report

  #region Customer Sales Detail Report

  private async Task<FileBase64DataViewModel> GetCustomerSalesDetailReportAsync(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetCustomerSalesDetailReportData";
    var customerSalesDetail = await _http.PostAndParseAsync<IEnumerable<CustomerSalesDetailViewModel>>(url, textReportParams);
    int[] columnPadding = { 20, 17, 37, 22, 14, 11, 17, 37, 10, 10, 17, 17, 10, 7 };
    var reportTitle = textReportParams.ReportName == "T0018" ? _localizer.GetString("arCustomerSalesReport") : _localizer.GetString("customerSalesDetailReport");

    var workbook = new XSSFWorkbook();
    var sheet = workbook.CreateSheet();

    if (customerSalesDetail == null || !customerSalesDetail.Any())
    {
      return GetBase64Data(workbook);
    }

    // Create a cell style for center alignment
    var centerStyle = workbook.CreateCellStyle();
    centerStyle.Alignment = HorizontalAlignment.Center;
    centerStyle.VerticalAlignment = VerticalAlignment.Center;
    var rowIdx = 0;
    var colIdx = 0;

    // Title
    var index = textReportParams.StoreName!.IndexOf('-');
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, textReportParams.StoreName.Remove(index, 1).Insert(index, " "), centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, $"{reportTitle} ({textReportParams.ReportName})", centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, $"{_localizer.GetString("printedOn")}: {DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}")}", centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, $"{_localizer.GetString("startDate").Value}: {textReportParams.StartDate.Value:dd-MMM-yyyy} {_localizer.GetString("endDate").Value}: {textReportParams.EndDate.Value:dd-MMM-yyyy}", centerStyle);
    AddDataToCell(sheet, rowIdx++, 0, string.Empty, centerStyle);

    for (var i = 0; i < rowIdx; i++)
    {
      sheet.AddMergedRegion(new CellRangeAddress(i, i, 0, columnPadding.Length - 1));
    }

    /* :::::::::::::::::::::::::::::::::: Report header section start :::::::::::::::::::::::::::::::::::::::::::::::::::::: */
    // header
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("saleNo").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("customerCode").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("customerName").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("cardNumber").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("date").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("time").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("product").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("productName").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("qty").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("taxes").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, $"{_localizer.GetString("discount").Value}/{_localizer.GetString("rebate").Value}");
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("depositAmount").Value);
    AddDataToCell(sheet, rowIdx, colIdx++, _localizer.GetString("amount").Value);
    AddDataToCell(sheet, rowIdx++, colIdx++, _localizer.GetString("type").Value);

    for (var j = 0; j < columnPadding.Length; j++)
    {
      sheet.SetColumnWidth(j, columnPadding[j] * 256);
    }

    foreach (var saleDetail in customerSalesDetail)
    {
      colIdx = 0;
      decimal taxAmount = saleDetail.LineTaxAmount + saleDetail.ChargeTaxAmount + saleDetail.KitTaxAmount;
      decimal depositAmount = saleDetail.SaleAmount + saleDetail.KitAmount;

      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.SaleNumber.ToString());
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.ClientCode);
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.ClientName);
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.CardNumber);
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.SaleDate.Value, GetDataFormat(workbook, "yyyy-MM-dd"));
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.SaleTime.Value.ToString("HH:mm:ss"), GetDataFormat(workbook, "HH:mm:ss"));
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.StockCode);
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.StockName);
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.Quantity, GetDataFormat(workbook, "###0.000"));
      AddDataToCell(sheet, rowIdx, colIdx++, taxAmount, GetDataFormat(workbook, "##0.00"));
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.Discount + saleDetail.DiscountAdjustment, GetDataFormat(workbook, "##0.00"));
      AddDataToCell(sheet, rowIdx, colIdx++, depositAmount, GetDataFormat(workbook, "##0.00"));
      AddDataToCell(sheet, rowIdx, colIdx++, saleDetail.Amount + taxAmount + depositAmount - saleDetail.Discount - saleDetail.DiscountAdjustment, GetDataFormat(workbook, "###0.00"));
      AddDataToCell(sheet, rowIdx++, colIdx++, saleDetail.SaleType);
    }

    // Get base 64 data
    return GetBase64Data(workbook);
  }

  #endregion Customer Sales Detail Report

  private ICellStyle GetDataFormat(IWorkbook workbook, string format)
  {
    var cellStyle = workbook.CreateCellStyle();
    var dataFormat = workbook.CreateDataFormat();
    cellStyle.DataFormat = dataFormat.GetFormat(format);
    return cellStyle;
  }

  #endregion Text Reports
}
