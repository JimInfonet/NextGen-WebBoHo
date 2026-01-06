using InfonetCSCR.Core.Models;

namespace InfonetCSCR.Web.Services;

public class TextReportsService : ITextReportsService
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IStringLocalizer<TextReportsService> _localizer;
  private readonly string timeFormat;
  public TextReportsService(IHttpService http, IApplicationConfig config, IStringLocalizer<TextReportsService> localizer, IPolicyService policy)
  {
    _http = http;
    _localizer = localizer;
    _config = config;
    timeFormat = string.Equals(policy.GetPolicy(PolicyName.TIMEFORMAT).ToString(), AppConstants.TIME_FORMAT_24H) ?
                                                                                    AppConstants.HOUR_MINUTE : AppConstants.HOUR_MINUTE_SECOND;
  }

  public async Task<SelectList> GetAllTextReportsListAsync()
  {
    const string url = "TextReports/GetAllTextReportsList";
    var textReportsList = await _http.GetAndParseAsync<IEnumerable<TextReportViewModel>>(url);

    if (_config.IsBackOffice)
    {
      textReportsList = textReportsList.Where(textReport => textReport.ReportCode != "T0021");
    }
    else
    {
      textReportsList = textReportsList.Where(textReport => textReport.ReportCode != "T0007" && textReport.ReportCode != "T0008");
    }

    foreach (var textReport in textReportsList)
    {
      textReport.ReportTitle = $"{textReport.ReportCode}-{textReport.ReportTitle}";
    }

    return new SelectList(textReportsList, "ReportCode", "ReportTitle");
  }

  public async Task<SelectList> GetAllStoresListAsync()
  {
    if (_config.IsBackOffice)
    {
      return await GetCompanyAsync();
    }

    const string url = "Store/GetAllStoresList";
    var allStoresList = await _http.GetAndParseAsync<IEnumerable<StoreViewModel>>(url);

    foreach (var store in allStoresList)
    {
      store.Store_Desc = $"{store.Store_ID}-{store.Store_Desc}";
    }

    return new SelectList(allStoresList, "Store_ID", "Store_Desc");
  }

  public async Task<string> GetTextReport(TextReportParamsViewModel textReportParams)
  {
    if (_config.IsBackOffice)
    {
      textReportParams.StoreID = null;
    }

    return textReportParams?.ReportName! switch
    {
      "T0001" => await GetTotalizerReadingReport(textReportParams!),
      "T0002" => await GetCardDetailsReportAsync(textReportParams),
      "T0003" => await GetFleetCardFuelReport(textReportParams),
      "T0004" => await GetCreditCardSummaryReport(textReportParams),
      "T0005" => await GetARCustomerSalesReport(textReportParams),
      "T0006" => await GetDailyFuelDiscountReport(textReportParams),
      "T0007" => await GetSaleVarianceReport(textReportParams),
      "T0008" => await GetShiftAnalysisReport(textReportParams),
      "T0009" => await GetFleetCardSaleReportData(textReportParams),
      "T0010" => await GetAffdaSaleDetailReportData(textReportParams),
      "T0011" => await GetAffdaTaxSaleSummaryReportData(textReportParams),
      "T0012" => await GetStockItemsDetailReportData(textReportParams),
      "T0013" => await GetTankDipReport(textReportParams),
      "T0014" => await GetAdagioReportData(textReportParams),
      "T0015" => await GetIncompleteTransactionsReportData(textReportParams),
      "T0016" => await GetFinancialReportData(textReportParams),
      "T0017" => await GetEmployeeCommissionReportData(textReportParams),
      "T0018" => await GetCustomerSalesDetailReportDataAsync(textReportParams),
      "T0019" => await GetCustomerSalesDetailReportDataAsync(textReportParams),
      _ => "No Data",
    };
  }

  #region T0002

  private async Task<string> GetCardDetailsReportAsync(TextReportParamsViewModel textReportParams)
  {
    const int LONG_PRINT_WIDTH = 152;
    const string url = "TextReports/GetCardDetailsReportData";
    var cardDetails = await _http.PostAndParseAsync<IEnumerable<CardDetailsViewModel>>(url, textReportParams);

    if (cardDetails == null || !cardDetails.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("cardReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("cardName"), 20));
    str.Append(PadR(_localizer.GetString("cardNumber"), 21));
    str.Append(PadR(_localizer.GetString("expDate"), 19));
    str.Append(PadR(_localizer.GetString("auth"), 16));
    str.Append(PadR(_localizer.GetString("date"), 11));
    str.Append(PadR(_localizer.GetString("time"), 9));
    str.Append(PadR(_localizer.GetString("saleNo"), 18));
    str.Append(PadR(_localizer.GetString("termID"), 21));
    str.Append(PadR(_localizer.GetString("amount"), 10));
    str.AppendLine(PadR(_localizer.GetString("type"), 7));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    foreach (var card in cardDetails)
    {
      str.Append(PadR(card.CardName, 20));
      str.Append(PadR(GetMaskedCardNumber(card.CardNumber, card.CardType), 21));
      str.Append(PadR(card.ExpiryDate, 19));
      str.Append(PadR(card.ApprovalCode, 16));
      str.Append(PadR(card.TransactionDate.ToString("yyyy-MM-dd"), 11));
      str.Append(PadR(card.TransactionTime.ToString("HH:mm:ss"), 9));
      str.Append(PadR(card.SaleNo.ToString(), 18));
      str.Append(PadR(card.TerminalID, 21));
      str.Append(PadR(card.Amount.ToString("$##0.00"), 10));

      if (card.TransactionType.Trim().ToUpper() == "PATP_APP")
      {
        str.AppendLine(PadR("P@P", 7));
      }
      else
      {
        str.AppendLine(PadR(card.TransactionType, 7));
      }
    }

    return str.ToString();
  }

  #endregion T0002

  #region T0003

  private async Task<string> GetFleetCardFuelReport(TextReportParamsViewModel textReportParams)
  {
    const int LONG_PRINT_WIDTH = 132;
    const string url = "TextReports/GetFleetCardFuelReportData";
    int[] columnPadding = { 17, 22, 14, 11, 17, 17, 17, 10, 7 };
    var fleetCardFuels = await _http.PostAndParseAsync<IEnumerable<FleetCardFuelViewModel>>(url, textReportParams);

    if (fleetCardFuels == null || !fleetCardFuels.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    var groupedByCardName = fleetCardFuels.GroupBy(m => m.CardName);
    int i = 0;
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("fleetCardFuelSaleReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("cardNumber"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("date"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("time"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("product"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("qty"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("discount"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("amount"), columnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("type"), columnPadding[i]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    decimal totalThirdPartyDiscount = 0;
    foreach (var group in groupedByCardName)
    {
      str.AppendLine();
      str.AppendLine(PadR(group.Key!, 20));

      foreach (var fuelCard in group)
      {
        i = 0;
        str.Append(PadR(fuelCard.SaleNo.ToString(), columnPadding[i++]));
        str.Append(PadR(GetMaskedCardNumber(fuelCard.CardNumber, fuelCard.CardType), columnPadding[i++]));
        str.Append(PadR(fuelCard.TransactionDate.ToString("yyyy-MM-dd"), columnPadding[i++]));
        str.Append(PadR(fuelCard.TransactionTime.ToString("HH:mm:ss"), columnPadding[i++]));
        str.Append(PadR(fuelCard.StockCode, columnPadding[i++]));
        str.Append(PadR(fuelCard.Quantity.ToString("##0.000"), columnPadding[i++]));
        str.Append(PadR(fuelCard.Discount.ToString("$##0.00"), columnPadding[i++]));
        str.Append(PadR(fuelCard.Amount.ToString("$##0.00"), columnPadding[i++]));
        str.Append(PadR(fuelCard.SaleType, columnPadding[i]));
        str.AppendLine();
      }

      decimal quantitySum = group.Sum(m => m.Quantity);
      decimal discountSum = group.Sum(m => m.Discount);
      decimal amountSum = group.Sum(m => m.Amount);

      // Total section
      str.AppendLine();
      str.AppendLine(new string('-', 30));
      str.Append(new string(' ', 25));
      str.Append(PadR($"{_localizer.GetString("totalOf")} {group.Key}", 56));

      str.Append(PadR(quantitySum.ToString("##0.000"), columnPadding[5]));
      str.Append(PadR(discountSum.ToString("$##0.00"), columnPadding[6]));
      str.AppendLine(PadR(amountSum.ToString("$##0.00"), columnPadding[7]));

      // Discount section
      DiscountViewModel? discountProperties = await GetCardDiscountFromCardName(group.Key);

      if (discountProperties == null)
        continue;

      string discountType = discountProperties.DiscountType.ToString().ToLower();

      if (discountProperties.DiscountRate != 0 && !string.IsNullOrEmpty(discountType))
      {
        str.Append(new string(' ', 18));
        str.Append(PadR($"{_localizer.GetString("thirdPartyDiscountOf")} {group.Key}", 47));
        decimal thirdPartyDiscount = 0;
        if (discountType == "t")
        {
          thirdPartyDiscount = (group.Count() * discountProperties.DiscountRate);
          str.Append(PadR($"{_localizer.GetString("totalSalesX")} {discountProperties.DiscountRate.ToString("##0.00")}", 50));
          str.Append(PadR($"{thirdPartyDiscount:$##0.00}", 31));
        }
        else if (discountType == "p")
        {
          thirdPartyDiscount = (amountSum * discountProperties.DiscountRate / 100);
          str.Append(PadR($"{_localizer.GetString("totalSalesX")} {discountProperties.DiscountRate.ToString("##0.00")}%", 50));
          str.Append(PadR($"{thirdPartyDiscount:$##0.00}", 31));
        }

        totalThirdPartyDiscount += thirdPartyDiscount;
        str.AppendLine();
      }
    }

    str.AppendLine();
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(new string(' ', 20));
    str.Append(PadR(_localizer.GetString("totalThirdPartyCards"), 61));


    decimal thirdPartyQuantitySum = fleetCardFuels.Sum(m => m.Quantity);
    decimal thirdPartyDiscountSum = fleetCardFuels.Sum(m => m.Discount);
    decimal thirdPartyAmountSum = fleetCardFuels.Sum(m => m.Amount);

    str.Append(PadR(thirdPartyQuantitySum.ToString("##0.000"), columnPadding[5]));
    str.Append(PadR(thirdPartyDiscountSum.ToString("$##0.00"), columnPadding[6]));
    str.AppendLine(PadR(thirdPartyAmountSum.ToString("$##0.00"), columnPadding[7]));

    str.Append(new string(' ', 18));
    str.Append(PadR(_localizer.GetString("totalThirdPartyDiscount"), 97));

    str.AppendLine(PadR(totalThirdPartyDiscount.ToString("$##0.00"), columnPadding[5]));

    return str.ToString();
  }

  #endregion T0003

  #region T0001

  private async Task<string> GetTotalizerReadingReport(TextReportParamsViewModel textReportParams)
  {
    const int LONG_PRINT_WIDTH = 50;
    const string url = "TextReports/GetTotalizerReadingReport";
    int[] columnPadding = { 15, 15, 20 };
    var totalizerReadings = await _http.PostAndParseAsync<IEnumerable<TotalizerReadingViewModel>>(url, textReportParams);

    if (totalizerReadings == null || !totalizerReadings.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    DateTime lastReading = totalizerReadings.Max(tr => tr.Date);
    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    int i = 0;
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("totalizerReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("lastReadings").Value}: {lastReading.ToString($"dd-MMM-yyyy {timeFormat}")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("pump/Grade"), columnPadding[i++]));
    str.Append(_localizer.GetString("volume").ToString().PadLeftFixed(columnPadding[i++]));
    str.AppendLine(_localizer.GetString("amount").ToString().PadLeftFixed(columnPadding[i]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    foreach (var totalizerReading in totalizerReadings)
    {
      i = 0;
      str.Append(PadR($"{totalizerReading.PumpID}/{totalizerReading.FullName}", columnPadding[i++]));
      str.Append(totalizerReading.Volume.ToString("##0.000").PadLeftFixed(columnPadding[i++]));
      str.AppendLine(totalizerReading.Dollars.ToString("##0.000").PadLeftFixed(columnPadding[i]));
    }

    str.AppendLine(new string('_', LONG_PRINT_WIDTH));

    var totalVolume = totalizerReadings.Sum(x => x.Volume);
    var totalAmount = totalizerReadings.Sum(x => x.Dollars);

    i = 0;
    str.Append(PadC($"  {_localizer.GetString("totalArticles")} :", columnPadding[i++]));
    str.Append(totalVolume.ToString("##0.000").PadLeftFixed(columnPadding[i++]));
    str.AppendLine(totalAmount.ToString("##0.000").PadLeftFixed(columnPadding[i]));

    str.AppendLine(new string('=', LONG_PRINT_WIDTH));

    var groupedData = totalizerReadings
            .GroupBy(x => new { x.Grade, x.FullName })
            .Select(g => new
            {
              g.Key.Grade,
              g.Key.FullName,
              TotalVolume = g.Sum(x => x.Volume),
              TotalAmount = g.Sum(x => x.Dollars)
            })
            .OrderBy(g => g.Grade)
            .ToList();

    str.AppendLine(PadR(_localizer.GetString("summaryByGrade"), LONG_PRINT_WIDTH));
    str.AppendLine();

    foreach (var group in groupedData)
    {
      if (string.IsNullOrEmpty(group.FullName))
        continue;

      i = 0;
      str.Append(group.FullName.PadLeftFixed(columnPadding[i++]));
      str.Append(group.TotalVolume.ToString("##0.000").PadLeftFixed(columnPadding[i++]));
      str.AppendLine(group.TotalAmount.ToString("##0.000").PadLeftFixed(columnPadding[i]));
    }

    str.AppendLine(new string('_', LONG_PRINT_WIDTH));

    i = 0;
    str.Append($"{_localizer.GetString("totalArticles")} :".PadLeftFixed(columnPadding[i++]));
    str.Append(totalVolume.ToString("##0.000").PadLeftFixed(columnPadding[i++]));
    str.AppendLine(totalAmount.ToString("##0.000").PadLeftFixed(columnPadding[i]));

    return str.ToString();
  }

  #endregion T0001

  #region T0004
  private async Task<string?> GetCreditCardSummaryReport(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetCreditCardSummaryReport";

    int[] columnPadding = { 22, 10, 10, 15, 15, 15 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var creditCardSummary = await _http.PostAndParseAsync<IEnumerable<CreditCardSummaryViewModel>>(url, textReportParams);

    if (creditCardSummary == null || !creditCardSummary.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var groupedByDescription = creditCardSummary.GroupBy(c => c.Description);

    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    #region Header
    int i = 0;
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("creditCardSummaryReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("saleDate").Value}: {textReportParams.SaleDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("cardType"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("class"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("till"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("date"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("transactions"), columnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("amount"), columnPadding[i]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    #endregion Header

    var insertedKeys = new HashSet<string>();

    decimal totalCreditDiscount = 0;
    decimal totalDebitDiscount = 0;

    string cardType = "";

    foreach (var group in groupedByDescription)
    {
      foreach (var card in group)
      {
        i = 0;

        if (insertedKeys.Contains(group.Key!))
        {
          str.Append(PadR("", columnPadding[i++]));
          str.Append(PadR("", columnPadding[i++]));
        }
        else
        {
          str.Append(PadR(group.Key!.ToString(), columnPadding[i++]));

          if (card.CardType! == "C") str.Append(PadR(_localizer.GetString("credit"), columnPadding[i++]));
          if (card.CardType! == "D") str.Append(PadR(_localizer.GetString("debit"), columnPadding[i++]));

          cardType = card.CardType!;
        }

        str.Append(PadR(card.Till.ToString(), columnPadding[i++]));
        str.Append(PadR(card.ShiftDate.ToString("MM/dd/yy"), columnPadding[i++]));
        str.Append(PadR(card.Transactions.ToString(), columnPadding[i++]));
        str.AppendLine(PadR(card.TotalAmount.ToString("$##0.00"), columnPadding[i]));

        insertedKeys.Add(group.Key!);
      }

      // Discount section
      DiscountViewModel? discountProperties = await GetCardDiscountFromCardName(group.Key);

      if (discountProperties == null)
        continue;

      string discountType = discountProperties.DiscountType.ToString().ToLower();

      if (discountProperties.DiscountRate != 0 && !string.IsNullOrEmpty(discountType))
      {
        decimal totSum = group.Sum(g => g.TotalAmount);

        str.AppendLine(new string('-', columnPadding[5]).PadLeftFixed(LONG_PRINT_WIDTH));

        decimal discount = 0;
        if (discountType == "t")
        {
          discount = (group.Count() * discountProperties.DiscountRate);
          str.Append(PadC($"{_localizer.GetString("totalArticles")} {group.Key!} {_localizer.GetString("transactions")} x {discountProperties.DiscountRate.ToString("##0.00")}%", LONG_PRINT_WIDTH - columnPadding.Last()));
          str.Append(PadR($"{discount:$##0.00}", columnPadding.Last()));
        }
        else if (discountType == "p")
        {
          discount = (totSum * discountProperties.DiscountRate / 100);
          str.Append(PadC($"{_localizer.GetString("totalArticles")} {group.Key!} {_localizer.GetString("dollars")} x {discountProperties.DiscountRate.ToString("##0.00")}%", LONG_PRINT_WIDTH - columnPadding.Last()));
          str.Append(PadR($"{discount:$##0.00}", columnPadding.Last()));
        }

        if (cardType == "C") totalCreditDiscount += discount;
        if (cardType == "D") totalDebitDiscount += discount;

        str.AppendLine();
        str.AppendLine();
      }
    }

    var totalDebitAmount = creditCardSummary.Where(card => card.CardType == "D").Sum(card => card.TotalAmount);
    var totalCreditAmount = creditCardSummary.Where(card => card.CardType == "C").Sum(card => card.TotalAmount);
    var totalDebitTransactions = creditCardSummary.Where(card => card.CardType == "D").Sum(card => card.Transactions);
    var totalCreditTransactions = creditCardSummary.Where(card => card.CardType == "C").Sum(card => card.Transactions);

    str.AppendLine();
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    //Total Credit
    i = 0;
    str.Append(PadR(_localizer.GetString("totalArticles"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("credit"), columnPadding[i++]));
    str.Append(PadR("", columnPadding[i++]));
    str.Append(PadR("", columnPadding[i++]));
    str.Append(PadR(totalCreditTransactions.ToString(), columnPadding[i++]));
    str.AppendLine(PadR(totalCreditAmount.ToString("$##0.00"), columnPadding[i]));

    //Total Debit
    i = 0;
    str.Append(PadR(_localizer.GetString("totalArticles"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("debit"), columnPadding[i++]));
    str.Append(PadR("", columnPadding[i++]));
    str.Append(PadR("", columnPadding[i++]));
    str.Append(PadR(totalDebitTransactions.ToString(), columnPadding[i++]));
    str.AppendLine(PadR(totalDebitAmount.ToString("$##0.00"), columnPadding[i]));
    str.AppendLine();

    //Credit Debit Discount
    str.Append(PadR($"{_localizer.GetString("creditDiscount")}", LONG_PRINT_WIDTH - columnPadding.Last()));
    str.AppendLine(PadR(totalCreditDiscount.ToString("$##0.00"), columnPadding.Last()));
    str.Append(PadR($"{_localizer.GetString("debitDiscount")}", LONG_PRINT_WIDTH - columnPadding.Last()));
    str.AppendLine(PadR(totalDebitDiscount.ToString("$##0.00"), columnPadding.Last()));

    return str.ToString();
  }
  #endregion T0004

  #region T0005
  private async Task<string?> GetARCustomerSalesReport(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetARCustomerSalesReport";
    int[] columnPadding = { 17, 22, 16, 11, 6, 10, 8, 15, 15, 1, 8 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var arCustomerSales = await _http.PostAndParseAsync<IEnumerable<ARCustomerSalesViewModel>>(url, textReportParams);

    if (arCustomerSales == null || !arCustomerSales.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var groupedByClient = arCustomerSales.GroupBy(c => new { c.ClientCode, c.ClientName });
    var str = new StringBuilder();

    int groupCount = groupedByClient.Count();
    int currentIndex = 0;

    foreach (var group in groupedByClient)
    {
      int index = textReportParams.StoreName!.IndexOf('-');

      #region Header
      int i = 0;
      str.AppendLine();
      str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
      str.AppendLine();
      str.AppendLine(PadC(_localizer.GetString("arCustomerSalesReport").Value, LONG_PRINT_WIDTH));
      str.AppendLine();
      str.AppendLine(PadC($"{_localizer.GetString("customerCode")} : {group.Key.ClientCode}", LONG_PRINT_WIDTH));
      str.AppendLine(PadC($"{_localizer.GetString("customerName")} : {group.Key.ClientName}", LONG_PRINT_WIDTH));
      str.AppendLine();
      str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
      str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
      str.AppendLine();
      str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[i++]));
      str.Append(PadR(_localizer.GetString("cardNumber"), columnPadding[i++]));
      str.Append(PadR(_localizer.GetString("po"), columnPadding[i++]));
      str.Append(PadR(_localizer.GetString("date"), columnPadding[i++]));
      str.Append(PadR(_localizer.GetString("time"), columnPadding[i++]));
      str.Append(PadR(_localizer.GetString("product"), columnPadding[i++]));
      str.Append(_localizer.GetString("qty").ToString().PadLeftFixed(columnPadding[i++]));
      str.Append(_localizer.GetString("discount").ToString().PadLeftFixed(columnPadding[i++]));
      str.Append(_localizer.GetString("amount").ToString().PadLeftFixed(columnPadding[i++]));
      str.Append(new string(' ', columnPadding[i++]));
      str.AppendLine(PadR(_localizer.GetString("type"), columnPadding[i]));
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      #endregion Header

      var totalDiscount = 0.00m;
      var totalAmount = 0.00m;

      int totalNumberOfSales = 0;
      foreach (var arCustomerSale in group)
      {
        if (arCustomerSale.SaleNo == null)
          continue;

        totalAmount += arCustomerSale.Amount.GetValueOrDefault(0m);
        totalDiscount += arCustomerSale.Discount.GetValueOrDefault(0m);
        totalNumberOfSales++;
        i = 0;
        str.Append(PadR(arCustomerSale.SaleNo.ToString(), columnPadding[i++]));
        str.Append(PadR(arCustomerSale.CardNumber!, columnPadding[i++]));
        str.Append(PadR(arCustomerSale.SerNum!, columnPadding[i++]));
        string formattedDate = arCustomerSale.SaleDate.HasValue ? arCustomerSale.SaleDate.Value.ToString("yyyy-MM-dd") : "";
        string formattedTime = arCustomerSale.SaleTime.HasValue ? arCustomerSale.SaleTime.Value.ToString($"{timeFormat}") : "";

        str.Append(PadR(formattedDate, columnPadding[i++]));
        str.Append(PadR(formattedTime, columnPadding[i++]));
        str.Append(PadR(arCustomerSale.StockCode!, columnPadding[i++]));
        str.Append((arCustomerSale.Quantity ?? 0).ToString("##0.00").PadLeftFixed(columnPadding[i++]));
        str.Append((arCustomerSale.Discount ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[i++]));
        str.Append((arCustomerSale.Amount ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[i++]));
        str.Append(new string(' ', columnPadding[i++]));
        str.AppendLine(PadR(arCustomerSale.SaleType!, columnPadding[i]));
      }

      if (totalNumberOfSales > 0)
        str.AppendLine(new string('-', LONG_PRINT_WIDTH));

      str.Append($"{_localizer.GetString("totalNumberOfSales")}: ".ToString().PadLeftFixed(40));
      str.Append(PadR(totalNumberOfSales.ToString(), 50));

      str.Append(totalDiscount.ToString("$##0.00").PadLeftFixed(columnPadding[7]));
      str.AppendLine(totalAmount.ToString("$##0.00").PadLeftFixed(columnPadding[8]));
      currentIndex++;

      if (currentIndex < groupCount)
      {
        str.AppendLine(PadC($"{_localizer.GetString("page")} - {currentIndex}", LONG_PRINT_WIDTH));
        str.AppendLine("NewPage");
      }

      str.AppendLine();
    }

    return str.ToString();
  }
  #endregion T0005

  #region T0006
  private async Task<string?> GetDailyFuelDiscountReport(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetDailyFuelDiscountReport";
    int[] columnPadding = { 30, 30 };
    int LONG_PRINT_WIDTH = 80;
    var dailyFuelDiscount = await _http.PostAndParseAsync<IEnumerable<DailyFuelDiscountViewModel>>(url, textReportParams);

    if (dailyFuelDiscount == null || !dailyFuelDiscount.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var fuelDiscountPayAtPump = dailyFuelDiscount.Where(c => c.SaleType == "PATP_APP").ToList();
    var fuelDiscountPayInside = dailyFuelDiscount.Where(c => c.SaleType != "PATP_APP").ToList();
    var str = new StringBuilder();

    int index = textReportParams.StoreName!.IndexOf('-');

    #region Header
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("dailyFuelDiscountReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("saleDate").Value}: {textReportParams.SaleDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    #endregion Header

    decimal? totalPayAtPumpDiscount = 0, totalPayInsideDiscount = 0;

    if (fuelDiscountPayAtPump != null && fuelDiscountPayAtPump.Any())
    {
      str.Append($"{_localizer.GetString("saleType")} : ");
      str.AppendLine(_localizer.GetString("payPump"));
      str.AppendLine();
      str.Append(_localizer.GetString("produit").ToString().PadLeftFixed(columnPadding[0]));
      str.AppendLine(_localizer.GetString("discount").ToString().PadLeftFixed(columnPadding[1]));
      str.AppendLine();

      var fuelDiscountPayAtPumpGroupedByStockCode = fuelDiscountPayAtPump.GroupBy(x => x.StockCode);

      foreach (var group in fuelDiscountPayAtPumpGroupedByStockCode)
      {
        var data = group.First();
        str.Append(data.StockCode.PadLeftFixed(columnPadding[0]));
        str.AppendLine((group.Sum(x => x.Discount) ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[1]));
      }

      totalPayAtPumpDiscount = fuelDiscountPayAtPump.Sum(x => x.Discount);
      str.AppendLine();
      str.Append(PadC(_localizer.GetString("totalDiscount"), columnPadding[0]));
      str.AppendLine((totalPayAtPumpDiscount ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[1]));
      str.AppendLine();
    }

    if (fuelDiscountPayInside.Any())
    {
      str.Append($"{_localizer.GetString("saleType")} : ");
      str.AppendLine(_localizer.GetString("payInside"));
      str.AppendLine();
      str.Append(_localizer.GetString("produit").ToString().PadLeftFixed(columnPadding[0]));
      str.AppendLine(_localizer.GetString("discount").ToString().PadLeftFixed(columnPadding[1]));
      str.AppendLine();

      var fuelDiscountPayInsideGroupedByStockCode = fuelDiscountPayInside.GroupBy(x => x.StockCode);

      foreach (var group in fuelDiscountPayInsideGroupedByStockCode)
      {
        var data = group.First();
        str.Append(data.StockCode.PadLeftFixed(columnPadding[0]));
        str.AppendLine((group.Sum(x => x.Discount) ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[1]));
      }

      totalPayInsideDiscount = fuelDiscountPayInside.Sum(x => x.Discount);
      str.AppendLine();
      str.Append(PadC(_localizer.GetString("totalDiscount"), columnPadding[0]));
      str.AppendLine((totalPayInsideDiscount ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[1]));
      str.AppendLine();
    }

    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.Append(PadC(_localizer.GetString("totalDiscount"), columnPadding[0]));
    str.AppendLine((totalPayAtPumpDiscount + totalPayInsideDiscount ?? 0).ToString("$##0.00").PadLeftFixed(columnPadding[1]));

    return str.ToString();
  }
  #endregion T0007

  #region T0007
  private async Task<string?> GetSaleVarianceReport(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetSaleVarianceReport";
    int[] columnPadding = { 20, 10, 15, 15, 15, 15, 15 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var saleVarianceData = await _http.PostAndParseAsync<IEnumerable<FuelSaleVarianceViewModel>>(url, textReportParams);

    if (saleVarianceData == null || !saleVarianceData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var str = new StringBuilder();

    int index = textReportParams.StoreName!.IndexOf('-');
    int i = 0;

    #region Header
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("fuelSaleVarianceReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("saleDate").Value}: {textReportParams.SaleDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(new string(' ', 20));
    str.Append(PadR(_localizer.GetString("saleProcessed"), 30));
    str.Append(PadR(_localizer.GetString("totalizerSales"), 35));
    str.AppendLine(PadR(_localizer.GetString("variance"), 25));

    str.Append(PadR(_localizer.GetString("pump/Product"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("dollars"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("volumes"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("dollars"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("volumes"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("dollars"), columnPadding[i++]));
    str.AppendLine(PadL(_localizer.GetString("volumes"), columnPadding[i]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    #endregion Header

    if (textReportParams.IsSummaryByGrade)
    {
      var groupedByGrade = saleVarianceData.GroupBy(x => x.GradeName);

      foreach (var group in groupedByGrade)
      {
        string gradeName = "";
        str.AppendLine();
        foreach (var saleVariance in group)
        {
          i = 0;
          str.Append(PadR($"{saleVariance.PumpID}/{saleVariance.GradeName}", columnPadding[i++]));
          str.Append(PadL(saleVariance.SumOfExt.ToString("##0.000"), columnPadding[i++]));
          str.Append(PadL(saleVariance.SumOfQty.ToString("##0.000"), columnPadding[i++]));
          str.Append(PadL(saleVariance.SumOfSalesDollars.ToString("##0.000"), columnPadding[i++]));
          str.Append(PadL(saleVariance.SumOfSalesLiters.ToString("##0.000"), columnPadding[i++]));
          str.Append(PadL((saleVariance.SumOfExt - saleVariance.SumOfSalesDollars).ToString("##0.000"), columnPadding[i++]));
          str.AppendLine(PadL((saleVariance.SumOfQty - saleVariance.SumOfSalesLiters).ToString("##0.000"), columnPadding[i]));
          gradeName = saleVariance.GradeName!;
        }

        i = 0;
        str.AppendLine(new string('.', LONG_PRINT_WIDTH));
        str.Append(PadR(gradeName, columnPadding[i++]));
        str.Append(PadL(group.Sum(x => x.SumOfExt).ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL(group.Sum(x => x.SumOfQty).ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL(group.Sum(x => x.SumOfSalesDollars).ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL(group.Sum(x => x.SumOfSalesLiters).ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL((group.Sum(x => x.SumOfExt) - group.Sum(x => x.SumOfSalesDollars)).ToString("##0.000"), columnPadding[i++]));
        str.AppendLine(PadL((group.Sum(x => x.SumOfQty) - group.Sum(x => x.SumOfSalesLiters)).ToString("##0.000"), columnPadding[i]));
      }
    }
    else
    {
      str.AppendLine();
      foreach (var saleVariance in saleVarianceData)
      {
        i = 0;
        str.Append(PadR($"{saleVariance.PumpID}/{saleVariance.GradeName}", columnPadding[i++]));
        str.Append(PadL(saleVariance.SumOfExt.ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL(saleVariance.SumOfQty.ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL(saleVariance.SumOfSalesDollars.ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL(saleVariance.SumOfSalesLiters.ToString("##0.000"), columnPadding[i++]));
        str.Append(PadL((saleVariance.SumOfExt - saleVariance.SumOfSalesDollars).ToString("##0.000"), columnPadding[i++]));
        str.AppendLine(PadL((saleVariance.SumOfQty - saleVariance.SumOfSalesLiters).ToString("##0.000"), columnPadding[i]));
      }
    }

    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    return str.ToString();
  }
  #endregion T0007

  #region T0008
  private async Task<string?> GetShiftAnalysisReport(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetShiftAnalysisReportData";
    int[] columnPadding = { 20, 18, 15, 15, 20, 15, 25 };
    int[] headerColumnPadding = { 25, 30, 25, 25 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var shiftAnalysisReportData = await _http.PostAndParseAsync<ShiftAnalysisViewModel?>(url, textReportParams);
    var employeesAndShifts = shiftAnalysisReportData?.EmployeesAndShifts;
    var shiftAnalysis = shiftAnalysisReportData?.ShiftAnalysis;

    if (shiftAnalysisReportData == null || employeesAndShifts == null || !employeesAndShifts.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var employeeIdToEmployeeMap = employeesAndShifts
        .Where(e => e.EmployeeID != null && e.Employee != null)
        .GroupBy(e => e.EmployeeID)
        .ToDictionary(g => g.Key!, g => g.Last().Employee!);

    var company = await GetCompanyInfoAsync();
    int index = textReportParams.StoreName!.IndexOf('-');

    int i = 0;
    var str = new StringBuilder();
    #region Header
    str.AppendLine();
    str.Append(new string(' ', 45));
    str.AppendLine(PadR(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH - 45));
    str.Append(new string(' ', 45));
    str.AppendLine(PadR($"{company.Address1} , {company.City}", LONG_PRINT_WIDTH - 45));
    str.AppendLine();
    str.AppendLine(PadC(_localizer.GetString("shiftAnalysis").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR($"{_localizer.GetString("startDate").Value}", headerColumnPadding[i++]));
    str.Append(PadR(textReportParams.StartDate.Value.ToString("dd-MMM-yyyy"), headerColumnPadding[i++]));
    str.Append(PadR($"{_localizer.GetString("endDate").Value}", headerColumnPadding[i++]));
    str.Append(PadR(textReportParams.EndDate.Value.ToString("dd-MMM-yyyy"), headerColumnPadding[i]));
    str.AppendLine();
    str.AppendLine();
    #endregion Header

    var shiftAnalysisGroupedByEmployee = shiftAnalysis.GroupBy(s => s.EmployeeID);

    foreach (var shift in shiftAnalysisGroupedByEmployee)
    {
      i = 0;
      str.AppendLine(new string('_', LONG_PRINT_WIDTH));
      str.AppendLine();
      str.Append(PadR(_localizer.GetString("employeeID"), headerColumnPadding[i++]));
      str.Append(PadR(shift.Key.ToString(), headerColumnPadding[i++]));

      if (employeeIdToEmployeeMap.TryGetValue(shift.Key, out var employeeName))
        str.Append(PadR(employeeName, headerColumnPadding[i]));

      str.AppendLine();

      var shiftAnalysisGroupedByDate = shift.GroupBy(x => x.ShiftDate);
      foreach (var entry in shiftAnalysisGroupedByDate)
      {
        if (entry.Key == null)
          continue;

        i = 0;
        var firstEntry = entry.First();
        str.AppendLine();
        str.Append(PadR(_localizer.GetString("date"), headerColumnPadding[i++]));
        str.Append(PadR(entry.Key.Value.ToString("dd-MMM-yy"), headerColumnPadding[i++]));
        str.Append(PadR(_localizer.GetString("shiftID"), headerColumnPadding[i++]));
        str.Append(PadR(firstEntry.Shift!.ToString(), headerColumnPadding[i]));
        str.AppendLine();
        str.AppendLine();

        str.Append(PadR("", headerColumnPadding[0]));
        str.Append(PadR(_localizer.GetString("openCashDrawer"), headerColumnPadding[1]));
        str.Append(PadL((firstEntry.CashDrawerCount ?? 0).ToString(), headerColumnPadding[2]));
        str.AppendLine();

        str.Append(PadR("", headerColumnPadding[0]));
        str.Append(PadR(_localizer.GetString("transactionCount"), headerColumnPadding[1]));
        str.Append(PadL((firstEntry.SalesCount ?? 0).ToString(), headerColumnPadding[2]));
        str.AppendLine();

        str.Append(PadR("", headerColumnPadding[0]));
        str.Append(PadR(_localizer.GetString("deletedLinesCount"), headerColumnPadding[1]));
        str.Append(PadL((firstEntry.DeletedLinesCount ?? 0).ToString("F0"), headerColumnPadding[2]));
        str.AppendLine();

        str.Append(PadR("", headerColumnPadding[0]));
        str.Append(PadR(_localizer.GetString("deletedLinesTotalAmount"), headerColumnPadding[1]));

        if (firstEntry.DeletedLinesTotalAmount == null)
          str.Append(PadL((firstEntry.DeletedLinesTotalAmount ?? 0).ToString("F0"), headerColumnPadding[2]));
        else
          str.Append(PadL((firstEntry.DeletedLinesTotalAmount ?? 0).ToString("$#,##0.00"), headerColumnPadding[2]));

        str.AppendLine();

        str.Append(PadR("", headerColumnPadding[0]));
        str.Append(PadR(_localizer.GetString("averageSaleAmount"), headerColumnPadding[1]));
        str.Append(PadL((firstEntry.AvgSaleAmount ?? 0).ToString("$##0.00"), headerColumnPadding[2]));
        str.AppendLine();

        str.Append(PadR("", headerColumnPadding[0]));
        str.AppendLine(PadL(_localizer.GetString("excludingFuel"), headerColumnPadding[1]));
        str.AppendLine();

        var stockCodes = entry.GroupBy(x => new { x.SaleNo, x.StockCode });
        i = 0;
        bool headerPrinted = false;
        foreach (var stockCode in stockCodes)
        {
          if (stockCode.Key.StockCode == null)
            continue;

          if (!headerPrinted)
          {
            headerPrinted = true;
            str.AppendLine(PadR(_localizer.GetString("refundsPurchaseCorrections"), LONG_PRINT_WIDTH));
            str.AppendLine();

            str.Append(PadR(_localizer.GetString("invoiceID"), columnPadding[i++]));
            str.Append(PadR(_localizer.GetString("invoiceDate"), columnPadding[i++]));
            str.Append(PadR(_localizer.GetString("reference"), columnPadding[i++]));
            str.Append(PadR(_localizer.GetString("productUPC"), columnPadding[i++]));
            str.Append(PadR(_localizer.GetString("description"), columnPadding[i++]));
            str.Append(PadL(_localizer.GetString("quantity"), columnPadding[i++]));
            str.AppendLine(PadL(_localizer.GetString("totalRetail"), columnPadding[i++]));
            str.AppendLine();
          }

          foreach (var product in stockCode)
          {
            i = 0;
            str.Append(PadR((product.SaleNo ?? 0).ToString(), columnPadding[i++]));
            str.Append(PadR((product.ShiftDate!.Value.ToString("dd-MMM-yy")), columnPadding[i++]));
            str.Append(PadR(product.VoidNo?.ToString(), columnPadding[i++])); // reference
            str.Append(PadR(product.StockCode!, columnPadding[i++]));
            str.Append(PadR(product.Description!, columnPadding[i++]));
            str.Append(PadL((product.Quantity ?? 0).ToString(), columnPadding[i++]));
            str.Append(PadL((product.Amount ?? 0).ToString("$##0.00"), columnPadding[i++]));
            str.AppendLine();
          }
        }
      }
    }

    str.AppendLine(new string('_', LONG_PRINT_WIDTH));
    return str.ToString();
  }
  #endregion T0008

  #region T0009
  private async Task<string?> GetFleetCardSaleReportData(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetFleetCardSaleReportData";
    int[] columnPadding = { 20, 33, 25, 15, 15, 20, 20, 10 };
    int[] innerLineColumnPadding = { 15, 38, 13, 12, 15, 15, 15, 20, 10 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();

    var fleetCardSaleData = await _http.PostAndParseAsync<IEnumerable<FleetCardSaleViewModel>?>(url, textReportParams);

    if (fleetCardSaleData == null || !fleetCardSaleData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int index = textReportParams.StoreName!.IndexOf('-');
    var str = new StringBuilder();
    int i = 0;

    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("fleetCardSaleReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("cardName"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("cardNumber"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("date"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("time"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("saleAmount"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("tendAmount"), columnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("type"), columnPadding[i++]));
    i = 0;
    str.Append(PadR(_localizer.GetString("line"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("produit"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("qty"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("price"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("net"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("tax"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("charges"), innerLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("discount/Rebate"), innerLineColumnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("total"), innerLineColumnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    var fleetCardGroupedBySaleNo = fleetCardSaleData.GroupBy(x => x.SaleNo);

    decimal tendAmountSum = 0;
    decimal saleAmountSum = 0;
    decimal quantitySum = 0;

    foreach (var fleetCard in fleetCardGroupedBySaleNo)
    {
      var orderedFleetCard = fleetCard.OrderBy(x => x.LineNumber);
      var firstCard = orderedFleetCard.First();
      tendAmountSum += firstCard.TendAmount;
      saleAmountSum += firstCard.SaleAmount;

      i = 0;
      str.Append(PadR(firstCard.SaleNo.ToString(), columnPadding[i++]));
      str.Append(PadR(firstCard.CardName!, columnPadding[i++]));
      str.Append(PadR(firstCard.CardNumber!, columnPadding[i++]));
      str.Append(PadR(firstCard.TransactionDate.Value.ToString("yyyy-MM-dd"), columnPadding[i++]));
      str.Append(PadR(firstCard.TransactionTime.Value.ToString("HH:mm:ss"), columnPadding[i++]));
      str.Append(PadR(firstCard.SaleAmount.ToString("$#0.00"), columnPadding[i++]));
      str.Append(PadR(firstCard.TendAmount.ToString("$#0.00"), columnPadding[i++]));
      str.AppendLine(PadR(firstCard.Type, columnPadding[i++]));

      foreach (var card in orderedFleetCard)
      {
        quantitySum += card.Quantity;

        i = 0;
        var taxes = card.LineTax + card.SaleKitTax + card.ChargeTax;
        var charges = card.SaleCharge + card.SaleKitCharge;

        str.Append(PadC(card.LineNumber.ToString(), innerLineColumnPadding[i++]));
        str.Append(PadR(card.Description, innerLineColumnPadding[i++]));
        str.Append(PadR(card.Quantity.ToString("#0.000"), innerLineColumnPadding[i++]));
        str.Append(PadR(card.Price.ToString("#0.000"), innerLineColumnPadding[i++]));
        str.Append(PadR(card.Amount.ToString("#0.00"), innerLineColumnPadding[i++]));
        str.Append(PadR(taxes.ToString("#0.00"), innerLineColumnPadding[i++]));
        str.Append(PadR(charges.ToString("#0.00"), innerLineColumnPadding[i++]));
        str.Append(PadR(card.Discount.ToString("#0.00"), innerLineColumnPadding[i++]));
        str.AppendLine(PadR((card.Amount + taxes + charges - card.Discount).ToString("#0.00"), innerLineColumnPadding[i++]));
      }
    }

    str.AppendLine(PadC(new string('-', 70), LONG_PRINT_WIDTH));
    str.Append(PadL($"{_localizer.GetString("totalFleetCardTenderedAmount")}: ", 95));
    str.AppendLine(tendAmountSum.ToString("$#0.00"));
    str.Append(PadL($"{_localizer.GetString("totalFleetCardSaleAmount")}: ", 95));
    str.AppendLine(saleAmountSum.ToString("$#0.00"));
    str.Append(PadL($"{_localizer.GetString("totalFleetCardSaleVolume")}: ", 95));
    str.AppendLine(quantitySum.ToString("$#0.00"));
    str.AppendLine();

    return str.ToString();
  }
  #endregion T0009

  #region T0010
  private async Task<string?> GetAffdaSaleDetailReportData(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetAffdaSaleDetailReportData";
    int[] columnPadding = { 18, 35, 15, 20, 12, 12, 10, 10, 10, 10 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var affdaSaleDetailReportData = await _http.PostAndParseAsync<IEnumerable<AffdaSaleDetailViewModel>?>(url, textReportParams);

    if (affdaSaleDetailReportData == null || !affdaSaleDetailReportData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int index = textReportParams.StoreName!.IndexOf('-');

    var str = new StringBuilder();
    int i = 0;
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("affdaSaleDetailReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("produit"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("account"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("customer"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("date/Time"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString(""), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("qty"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("price"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("rebate"), columnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("total"), columnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    foreach (var data in affdaSaleDetailReportData)
    {
      i = 0;
      str.Append(PadR(data.SaleNo.ToString(), columnPadding[i++]));
      str.Append(PadR(data.Description!, columnPadding[i++]));
      str.Append(PadR(data.CustomerCode!, columnPadding[i++]));
      str.Append(PadR(data.CustomerName!, columnPadding[i++]));
      str.Append(PadR(data.TransactionDate.Value.ToString("yyyy-MM-dd"), columnPadding[i++]));
      str.Append(PadR(data.TransactionTime.Value.ToString("HH:mm:ss"), columnPadding[i++]));
      str.Append(PadR(data.Quantity.ToString("#0.000"), columnPadding[i++]));
      str.Append(PadR(data.Price.ToString("#0.000"), columnPadding[i++]));
      str.Append(PadR(data.Rebate.ToString("#0.00"), columnPadding[i++]));
      str.AppendLine(PadR((data.Price * data.Quantity).ToString("#0.00"), columnPadding[i++]));
    }

    decimal totalRebate = affdaSaleDetailReportData.Sum(x => x.Rebate);
    str.AppendLine(PadL(new string('-', 45), LONG_PRINT_WIDTH - columnPadding.Last() - 6));
    str.AppendLine(PadL($"{_localizer.GetString("totalFuelRebate")}:{new string(' ', 15)}{totalRebate.ToString("$#0.00")}", LONG_PRINT_WIDTH - columnPadding.Last() - 6));

    return str.ToString();
  }
  #endregion T0010

  #region T0011
  private async Task<string?> GetAffdaTaxSaleSummaryReportData(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetAffdaTaxSaleSummaryReportData";
    int[] columnPadding = { 20, 15, 40, 12, 15, 12, 12 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var affdaSaleSummaryReportData = await _http.PostAndParseAsync<IEnumerable<AffdaSaleDetailViewModel>?>(url, textReportParams);

    if (affdaSaleSummaryReportData == null || !affdaSaleSummaryReportData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int index = textReportParams.StoreName!.IndexOf('-');
    int i = 0;
    var str = new StringBuilder();
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("affdaSaleSummaryReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("customer"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("stock"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("productName"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("qty"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("net"), columnPadding[i++]));
    str.Append(PadL(_localizer.GetString("rebate"), columnPadding[i++]));
    str.AppendLine(PadL(_localizer.GetString("total"), columnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    var groupByCustomer = affdaSaleSummaryReportData.GroupBy(x => x.CustomerCode);

    foreach (var group in groupByCustomer)
    {
      str.AppendLine(PadR($"{group.First().CustomerCode} - {group.First().CustomerName}", 40));

      foreach (var data in group)
      {
        i = 0;
        str.Append(PadR("", columnPadding[i++]));
        str.Append(PadR(data.StockCode!, columnPadding[i++]));
        str.Append(PadR(data.Description!, columnPadding[i++]));
        str.Append(PadL(data.Quantity.ToString("#0.000"), columnPadding[i++]));
        str.Append(PadL(data.Price.ToString("#0.00"), columnPadding[i++]));
        str.Append(PadL(data.Rebate.ToString("#0.00"), columnPadding[i++]));
        str.AppendLine(PadL((data.Price - data.Rebate).ToString("#0.00"), columnPadding[i++]));
      }
    }

    decimal totalRebate = affdaSaleSummaryReportData.Sum(x => x.Rebate);
    str.AppendLine(PadL(new string('-', 35), LONG_PRINT_WIDTH - columnPadding.Last()));
    str.AppendLine(PadL($"{_localizer.GetString("totalFuelRebate")}:{new string(' ', 15)}{totalRebate.ToString("$#0.00")}", LONG_PRINT_WIDTH - columnPadding.Last()));

    return str.ToString();
  }
  #endregion T0011


  #region T0012
  private async Task<string?> GetStockItemsDetailReportData(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetStockMasterInfo";
    int[] columnPadding = { 50, 50, 50 };
    int[] secondLineColumnPadding = { 2, 40, 10, 15, 10, 2, 35, 15, 20 };
    int LONG_PRINT_WIDTH = secondLineColumnPadding.Sum();
    var stockItems = await _http.PostAndParseAsync<IEnumerable<StockMasterViewModel>?>(url, textReportParams);

    if (stockItems == null || !stockItems.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int index = textReportParams.StoreName!.IndexOf('-');

    int i = 0;
    var str = new StringBuilder();
    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("stockItemsDetailReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("department"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("subDepartment"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("subDetail"), columnPadding[i++]));
    str.AppendLine();

    i = 0;
    str.Append(PadR("", secondLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("stockCode/Desc"), secondLineColumnPadding[i++]));
    str.Append(PadL(_localizer.GetString("price"), secondLineColumnPadding[i++]));
    str.Append(PadL(_localizer.GetString("margin"), secondLineColumnPadding[i++]));
    str.Append(PadL(_localizer.GetString("cost"), secondLineColumnPadding[i++]));
    str.Append(PadL(_localizer.GetString(""), secondLineColumnPadding[i++]));
    str.Append(PadC(_localizer.GetString("vendor"), secondLineColumnPadding[i++]));
    str.Append(PadR(_localizer.GetString("taxes"), secondLineColumnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("charges2"), secondLineColumnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    List<string> stockCodes = stockItems.Select(item => item.StockCode).ToList() ?? new List<string>();

    var stockCostInfoBatchParameter = new StockCostInfoBatchParameterDto
    {
      VendorId = "",
      StockCodes = stockCodes,
      OrderNo = 0,
      StoreID = textReportParams.StoreID
    };

    url = "TextReports/GetStockCostInfoBatch";
    var stockCost = await _http.PostAndParseAsync<IEnumerable<StockCostInfoViewModel?>>(url, stockCostInfoBatchParameter);

    url = "TextReports/GetStockPriceInfoBatch";
    var stockPrice = await _http.PostAndParseAsync<IEnumerable<StockPriceInfoDto?>>(url, stockCost);

    url = $"TextReports/GetStockTaxInfoBatch?storeID={textReportParams.StoreID}";
    var stockTax = await _http.PostAndParseAsync<IEnumerable<StockTaxViewModel?>>(url, stockCodes);

    url = $"TextReports/GetStockChargeInfoBatch?storeID={textReportParams.StoreID}";
    var stockCharges = await _http.PostAndParseAsync<IEnumerable<StockChargeViewModel?>>(url, stockCodes);

    Dictionary<string, StockCostInfoViewModel?> stockCostDict = stockCost?
        .Where(item => item?.StockCode != null)
        .GroupBy(item => item!.StockCode!)
        .ToDictionary(group => group.Key, group => group.First()) ?? new Dictionary<string, StockCostInfoViewModel?>();

    Dictionary<string, StockPriceInfoDto?> stockPriceDict = stockPrice?
        .Where(item => item?.StockCode != null)
        .GroupBy(item => item!.StockCode!)
        .ToDictionary(group => group.Key, group => group.First()) ?? new Dictionary<string, StockPriceInfoDto?>();

    Dictionary<string, StockTaxViewModel?> stockTaxDict = stockTax?
        .Where(item => item?.StockCode != null)
        .GroupBy(item => item!.StockCode!)
        .ToDictionary(group => group.Key, group => group.First()) ?? new Dictionary<string, StockTaxViewModel?>();

    Dictionary<string, List<StockChargeViewModel?>> stockChargeDict = stockCharges?
        .Where(item => item?.StockCode != null)
        .GroupBy(item => item!.StockCode!)
        .ToDictionary(group => group.Key, group => group.ToList()) ?? new Dictionary<string, List<StockChargeViewModel?>>();

    var stockItemsGroupedByDept = stockItems.GroupBy(x => new { x.Department, x.SubDepartment, x.SubDetail });

    foreach (var stockGroup in stockItemsGroupedByDept)
    {
      i = 0;
      var firstStock = stockGroup.First();

      if (!string.IsNullOrEmpty(firstStock.Department))
      {
        str.Append(PadR($"{firstStock.Department}-{firstStock.DepartmentName}", columnPadding[i++]));

        if (!string.IsNullOrEmpty(firstStock.SubDepartment))
        {
          str.Append(PadR($"{firstStock.SubDepartment}-{firstStock.SubDepartmentName}", columnPadding[i++]));

          if (!string.IsNullOrEmpty(firstStock.SubDetail))
            str.Append(PadR($"{firstStock.SubDetail}-{firstStock.SubDetailName}", columnPadding[i++]));
        }

        str.AppendLine();
      }
      else
        str.AppendLine(PadR(_localizer.GetString("noDept"), columnPadding[i++]));

      foreach (var stock in stockGroup)
      {
        decimal cost = 0;
        decimal price = 0;
        decimal margin = 0;
        decimal taxRate = 0;
        string? vendorID = null;
        string? vendorInfo = null;
        string? vendorName = null;
        string? taxNameAndCode = null;
        string? chargeInfo = null;

        if (stockCostDict.ContainsKey(stock.StockCode))
        {
          cost = stockCostDict[stock.StockCode].Cost ?? 0;
          vendorID = stockCostDict[stock.StockCode].VendorId;
          vendorName = stockCostDict[stock.StockCode].Name ?? "";
        }

        if (stockPriceDict.ContainsKey(stock.StockCode))
        {
          price = stockPriceDict[stock.StockCode].Price ?? 0;
          margin = stockPriceDict[stock.StockCode].Margin ?? 0;
        }

        if (stockTaxDict.ContainsKey(stock.StockCode))
        {
          taxRate = stockTaxDict[stock.StockCode].TaxRate ?? 0;
          taxNameAndCode = $"{stockTaxDict[stock.StockCode].TaxName}-{stockTaxDict[stock.StockCode].TaxCode}";
        }

        if (stockChargeDict.ContainsKey(stock.StockCode))
        {
          foreach (var stockCharge in stockChargeDict[stock.StockCode])
          {
            if (stockCharge.AssocCode != null)
            {
              chargeInfo += stockCharge.AssocCode.ToString();

              if (stockCharge.AssocPrice != null)
                chargeInfo += "-" + (stockCharge.AssocPrice ?? 0).ToString("#0.0") + " ";
            }
          }
        }

        vendorInfo = string.IsNullOrEmpty(vendorID) ? "" : $"{vendorID}-{vendorName}";

        i = 0;
        str.Append(PadR("", secondLineColumnPadding[i++]));
        str.Append(PadR($"{stock.StockCode}-{stock.Description}", secondLineColumnPadding[i++]));

        str.Append(PadL(price.ToString("#0.00"), secondLineColumnPadding[i++]));
        str.Append(PadL(margin.ToString("#0.00"), secondLineColumnPadding[i++]));
        str.Append(PadL(cost.ToString("#0.00"), secondLineColumnPadding[i++]));
        str.Append(PadR("", secondLineColumnPadding[i++]));
        str.Append(PadR(vendorInfo, secondLineColumnPadding[i++]));

        if (taxNameAndCode == null)
          str.Append(PadR("", secondLineColumnPadding[i++]));
        else
          str.Append(PadR($"{taxNameAndCode} {taxRate:#0.00}%", secondLineColumnPadding[i++]));

        str.AppendLine(PadR(chargeInfo, secondLineColumnPadding[i++]));
      }

      str.AppendLine();
    }

    return str.ToString();
  }
  #endregion T0012

  #region T0013

  private async Task<string> GetTankDipReport(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetTankDipReportData";
    int[] columnPadding = { 20, 20, 20, 20 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var tankDips = await _http.PostAndParseAsync<IEnumerable<TankDipViewModel>>(url, textReportParams);

    if (tankDips == null || !tankDips.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int i = 0;
    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("tankDipReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("dipDate"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("tankId"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("gradeId"), columnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("dipValue"), columnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.AppendLine();

    var groupedTankDips = tankDips.GroupBy(t => t.DipDate);

    foreach (var group in groupedTankDips)
    {

      str.AppendLine(PadR(group.Key.ToString("MM/dd/yyyy"), 20));

      foreach (var tankDip in group)
      {
        i = 0;
        str.Append(PadR("", columnPadding[i++]));
        str.Append(PadR(tankDip.TankID.ToString(), columnPadding[i++]));
        str.Append(PadR(tankDip.Grade.ToString(), columnPadding[i++]));
        str.AppendLine(PadR(tankDip.DipValue.ToString(), columnPadding[i++]));
      }

      str.AppendLine();
    }

    return str.ToString();
  }

  #endregion T0013

  #region T0014

  private async Task<string> GetAdagioReportData(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetAdagioReportData";
    int[] columnPadding = { 15, 35, 18, 12, 10, 12 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    var adagioReportData = await _http.PostAndParseAsync<IEnumerable<AdagioDtoViewModel>>(url, textReportParams);

    if (adagioReportData == null || !adagioReportData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int i = 0;
    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    str.AppendLine();
    str.AppendLine(PadC(_config.IsBackOffice ? textReportParams.StoreName.Remove(index, 1).Insert(index, " ") : _localizer.GetString("gasinfonetheadoffice"), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("adagioReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.Append(PadR(_localizer.GetString("customerCode"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("customerName"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("date"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("time"), columnPadding[i++]));
    str.AppendLine(PadR(_localizer.GetString("amount"), columnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.AppendLine();

    foreach (var adagio in adagioReportData)
    {
      i = 0;
      str.Append(PadR(adagio.Client.ToString(), columnPadding[i++]));
      str.Append(PadR(adagio.CL_NAME.ToString(), columnPadding[i++]));
      str.Append(PadR(adagio.Sale_No.ToString(), columnPadding[i++]));
      str.Append(PadR(adagio.SALE_DATE.HasValue ? adagio.SALE_DATE.Value.ToString("yyyy-MM-dd") : "", columnPadding[i++]));
      str.Append(PadR(adagio.SALE_TIME.HasValue ? adagio.SALE_TIME.Value.ToString("HH:mm:ss") : "", columnPadding[i++]));
      str.AppendLine(PadR($"${Convert.ToDecimal(adagio.AMTTEND):F2}", columnPadding[i++]));
    }

    return str.ToString();
  }

  #endregion T0014

  #region T0015

  private async Task<string> GetIncompleteTransactionsReportData(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetIncompleteTransactionsReportData";

    int[] columnPadding = { 12, 22, 18, 20, 18, 14, 15, 12 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();

    string timePattern = "hh:mm:ss tt";

    if (_config.IsBackOffice)
    {
      timePattern = "HH:mm:ss";
    }

    var incompleteTransactionsReportData = await _http.PostAndParseAsync<IEnumerable<IncompleteTransactionsDtoViewModel>>(url, textReportParams);

    if (incompleteTransactionsReportData == null || !incompleteTransactionsReportData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    int i = 0;
    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("payPumpIncompleteTransactions").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.Append(PadR(_localizer.GetString("saleDate"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("saleTime"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("cardNumber"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("expiryDate"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("cardType"), columnPadding[i++]));
    str.Append(PadR(_localizer.GetString("auth"), columnPadding[i++]));
    str.AppendLine(PadL(_localizer.GetString("saleAmount"), columnPadding[i++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.AppendLine();

    foreach (var incompleteTransactions in incompleteTransactionsReportData)
    {
      i = 0;
      str.Append(PadR(incompleteTransactions.SaleDate.HasValue ? incompleteTransactions.SaleDate.Value.ToString("dd-MMM-yyyy") : "", columnPadding[i++]));
      str.Append(PadR(incompleteTransactions.SaleTime.HasValue ? incompleteTransactions.SaleTime.Value.ToString(timePattern) : "", columnPadding[i++]));
      str.Append(PadR(incompleteTransactions.SaleNo.ToString(), columnPadding[i++]));
      str.Append(PadR(incompleteTransactions.CardNumber.ToString(), columnPadding[i++]));
      str.Append(PadR(incompleteTransactions.ExpiryDate.ToString(), columnPadding[i++]));
      str.Append(PadR(incompleteTransactions.CardType.ToString(), columnPadding[i++]));
      str.Append(PadR(incompleteTransactions.ApprovalNum.ToString(), columnPadding[i++]));
      str.AppendLine(PadL(incompleteTransactions.CompletionAmount.ToString("0.00"), columnPadding[i++]));
    }

    return str.ToString();
  }

  #endregion T0015

  #region T0016

  private async Task<string> GetFinancialReportData(TextReportParamsViewModel textReportParams)
  {
    string url = "TextReports/GetFinancialReportSalesDeptData";

    var salesDeptData = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

    url = "TextReports/GetFinancialReportSalesData";

    int[] columnPadding = { 30, 20, 20, 18, 18, 18, 18 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();

    int i = 0;
    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');

    decimal dailyQtyTotal = 0;
    decimal dailyAmtTotal = 0;
    decimal monthlyQtyTotal = 0;
    decimal monthlyAmtTotal = 0;
    decimal yearlyQtyTotal = 0;
    decimal yearlyAmtTotal = 0;

    decimal dailyNet = 0;
    decimal monthlyNet = 0;
    decimal yearlyNet = 0;

    if (salesDeptData.Any())
    {
      str.AppendLine();
      str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
      str.AppendLine(Environment.NewLine);
      str.AppendLine(PadC(_localizer.GetString("financialReport").Value, LONG_PRINT_WIDTH));
      str.AppendLine();
      str.AppendLine(PadC(DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}"), LONG_PRINT_WIDTH));
      str.AppendLine(PadC($"{_localizer.GetString("selectedDate").Value}: {textReportParams.SaleDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
      str.AppendLine();
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.Append(PadR(_localizer.GetString("entry"), columnPadding[i++]));
      str.Append(PadL(_localizer.GetString("dayQuantity"), columnPadding[i++]));
      str.Append(PadL(_localizer.GetString("dayAmount"), columnPadding[i++]));
      str.Append(PadL(_localizer.GetString("monthQuantity"), columnPadding[i++]));
      str.Append(PadL(_localizer.GetString("monthAmount"), columnPadding[i++]));
      str.Append(PadL(_localizer.GetString("yearQuantity"), columnPadding[i++]));
      str.AppendLine(PadL(_localizer.GetString("yearAmount"), columnPadding[i++]));
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.AppendLine();

      dailyQtyTotal = 0;
      dailyAmtTotal = 0;
      monthlyQtyTotal = 0;
      monthlyAmtTotal = 0;
      yearlyQtyTotal = 0;
      yearlyAmtTotal = 0;

      dailyNet = 0;
      monthlyNet = 0;
      yearlyNet = 0;

      str.AppendLine(_localizer.GetString("productSales"));
      str.AppendLine();

      textReportParams.NullDept = 1;
      textReportParams.Period = 0;

      var dailyReportData = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

      textReportParams.NullDept = 1;
      textReportParams.Period = 1;

      var monthlyReportData = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

      textReportParams.NullDept = 1;
      textReportParams.Period = 2;

      var yearlyReportData = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

      var totalAmount = yearlyReportData.Sum(x => x.Amount);

      if (totalAmount != 0)
      {
        var dailyItem = dailyReportData.FirstOrDefault();
        var monthlyItem = monthlyReportData.FirstOrDefault();
        var yearlyItem = yearlyReportData.FirstOrDefault();
        i = 0;
        str.Append(PadR(_localizer.GetString("noDept"), columnPadding[i++]));
        str.Append(PadR(dailyItem.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadR(dailyItem.Amount?.ToString("0.00") ?? "0.00", columnPadding[i++]));
        str.Append(PadR(monthlyItem.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadR(monthlyItem.Amount?.ToString("0.00") ?? "0.00", columnPadding[i++]));
        str.Append(PadR(yearlyItem.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.AppendLine(PadR(yearlyItem.Amount?.ToString("0.00") ?? "0.00", columnPadding[i++]));

        dailyQtyTotal += dailyItem.TotQty ?? 0;
        dailyAmtTotal += dailyItem.Amount ?? 0;
        monthlyQtyTotal += monthlyItem.TotQty ?? 0;
        monthlyAmtTotal += monthlyItem.Amount ?? 0;
        yearlyQtyTotal += yearlyItem.TotQty ?? 0;
        yearlyAmtTotal += yearlyItem.Amount ?? 0;
      }

      textReportParams.NullDept = 0;
      textReportParams.Period = 0;

      var dailyReportData1 = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

      textReportParams.NullDept = 0;
      textReportParams.Period = 1;

      var monthlyReportData1 = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

      textReportParams.NullDept = 0;
      textReportParams.Period = 2;

      var yearlyReportData1 = await _http.PostAndParseAsync<IEnumerable<SalesDtoViewModel>>(url, textReportParams);

      foreach (var dept in salesDeptData)
      {
        var dailyItem = dailyReportData1.FirstOrDefault(x => x.Dept == dept.Dept);
        var monthlyItem = monthlyReportData1.FirstOrDefault(x => x.Dept == dept.Dept);
        var yearlyItem = yearlyReportData1.FirstOrDefault(x => x.Dept == dept.Dept);
        i = 0;
        str.Append(PadR(dept.Dept + "-" + dept.Name, columnPadding[i++]));

        if (dailyItem == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(dailyItem.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.Append(PadL(dailyItem.Amount?.ToString("0.00") ?? "0.00", columnPadding[i++]));
          dailyQtyTotal += dailyItem.TotQty ?? 0;
          dailyAmtTotal += dailyItem.Amount ?? 0;
        }

        if (monthlyItem == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(monthlyItem.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.Append(PadL(monthlyItem.Amount?.ToString("0.00") ?? "0.00", columnPadding[i++]));
          monthlyQtyTotal += monthlyItem.TotQty ?? 0;
          monthlyAmtTotal += monthlyItem.Amount ?? 0;
        }

        if (yearlyItem == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.AppendLine(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(yearlyItem.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.AppendLine(PadL(yearlyItem.Amount?.ToString("0.00") ?? "0.00", columnPadding[i++]));
          yearlyQtyTotal += yearlyItem.TotQty ?? 0;
          yearlyAmtTotal += yearlyItem.Amount ?? 0;
        }
      }
      i = 0;
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.Append(PadC("", columnPadding[i++]));
      str.Append(PadL(dailyQtyTotal.ToString("0.000"), columnPadding[i++]));
      str.Append(PadL(dailyAmtTotal.ToString("0.00"), columnPadding[i++]));
      str.Append(PadL(monthlyQtyTotal.ToString("0.000"), columnPadding[i++]));
      str.Append(PadL(monthlyAmtTotal.ToString("0.00"), columnPadding[i++]));
      str.Append(PadL(yearlyQtyTotal.ToString("0.000"), columnPadding[i++]));
      str.AppendLine(PadL(yearlyAmtTotal.ToString("0.00"), columnPadding[i++]));
      str.AppendLine();

      dailyNet += dailyAmtTotal;
      monthlyNet += monthlyAmtTotal;
      yearlyNet += yearlyAmtTotal;

    }

    dailyQtyTotal = 0;
    dailyAmtTotal = 0;
    monthlyQtyTotal = 0;
    monthlyAmtTotal = 0;
    yearlyQtyTotal = 0;
    yearlyAmtTotal = 0;

    string fA = "0.00";

    url = "TextReports/GetFinancialReportSaleTaxesData";

    var saleTaxesData = await _http.PostAndParseAsync<IEnumerable<TaxesDtoViewModel>>(url, textReportParams);

    url = "TextReports/GetFinancialReportTaxesData";
    textReportParams.Period = 0;

    var dailyTaxesReportData = await _http.PostAndParseAsync<IEnumerable<TaxesDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyTaxesReportData = await _http.PostAndParseAsync<IEnumerable<TaxesDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyTaxesReportData = await _http.PostAndParseAsync<IEnumerable<TaxesDtoViewModel>>(url, textReportParams);

    if (saleTaxesData.Any())
    {

      str.AppendLine(_localizer.GetString("taxesCapital"));
      str.AppendLine();

      foreach (var tax in saleTaxesData)
      {
        var taxName = tax.TaxName;
        var taxCode = tax.TaxCode;

        var dailyTaxData = dailyTaxesReportData.Where(x => x.TaxName == taxName && x.TaxCode == taxCode).FirstOrDefault();
        var monthlyTaxData = monthlyTaxesReportData.Where(x => x.TaxName == taxName && x.TaxCode == taxCode).FirstOrDefault();
        var yearlyTaxData = yearlyTaxesReportData.Where(x => x.TaxName == taxName && x.TaxCode == taxCode).FirstOrDefault();

        i = 0;
        str.Append(PadR($"{taxName} - {taxCode} ({tax.TaxCode_Desc})", columnPadding[i++]));

        if (dailyTaxData == null)
        {
          str.Append(PadL("", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL("", columnPadding[i++]));

          if (tax.Included == true)
          {
            str.Append(PadL((dailyTaxData.InclTax ?? 0).ToString(fA), columnPadding[i++]));
            dailyAmtTotal += dailyTaxData.InclTax ?? 0;
          }
          else
          {
            str.Append(PadL((dailyTaxData.AddedTax ?? 0).ToString(fA), columnPadding[i++]));
            dailyAmtTotal += dailyTaxData.AddedTax ?? 0;
            dailyNet += dailyTaxData.AddedTax ?? 0;
          }
        }

        if (monthlyTaxData == null)
        {
          str.Append(PadL("", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL("", columnPadding[i++]));

          if (tax.Included == true)
          {
            str.Append(PadL((monthlyTaxData.InclTax ?? 0).ToString(fA), columnPadding[i++]));
            monthlyAmtTotal += monthlyTaxData.InclTax ?? 0;
          }
          else
          {
            str.Append(PadL((monthlyTaxData.AddedTax ?? 0).ToString(fA), columnPadding[i++]));
            monthlyAmtTotal += monthlyTaxData.AddedTax ?? 0;
            monthlyNet += monthlyTaxData.AddedTax ?? 0;
          }
        }

        
        if (yearlyTaxData == null)
        {
          str.Append(PadL("", columnPadding[i++]));
          str.AppendLine(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL("", columnPadding[i++]));

          if (tax.Included == true)
          {
            str.AppendLine(PadL((yearlyTaxData.InclTax ?? 0).ToString(fA), columnPadding[i++]));
            yearlyAmtTotal += yearlyTaxData.InclTax ?? 0;
          }
          else
          {
            str.AppendLine(PadL((yearlyTaxData.AddedTax ?? 0).ToString(fA), columnPadding[i++]));
            yearlyAmtTotal += yearlyTaxData.AddedTax ?? 0;
            yearlyNet += yearlyTaxData.AddedTax ?? 0;
          }
        }
      }

      i = 0;
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.Append(PadC("", columnPadding[i++]));
      str.Append(PadL("", columnPadding[i++]));
      str.Append(PadL(dailyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL("", columnPadding[i++]));
      str.Append(PadL(monthlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL("", columnPadding[i++]));
      str.AppendLine(PadL(yearlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.AppendLine();
    }

    dailyQtyTotal = 0;
    dailyAmtTotal = 0;
    monthlyQtyTotal = 0;
    monthlyAmtTotal = 0;
    yearlyQtyTotal = 0;
    yearlyAmtTotal = 0;

    url = "TextReports/GetFinancialReportSaleChargesData";

    var saleChargeData = await _http.PostAndParseAsync<IEnumerable<ChargeDtoViewModel>>(url, textReportParams);

    url = "TextReports/GetFinancialReportChargesData";
    textReportParams.Period = 0;

    var dailyChargeReportData = await _http.PostAndParseAsync<IEnumerable<ChargeDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyChargeReportData = await _http.PostAndParseAsync<IEnumerable<ChargeDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyChargeReportData = await _http.PostAndParseAsync<IEnumerable<ChargeDtoViewModel>>(url, textReportParams);


    if (saleChargeData.Any())
    {

      str.AppendLine(_localizer.GetString("charges_"));
      str.AppendLine();

      foreach (var charge in saleChargeData)
      {
        var depName = charge.DepName;
        var depCode = charge.DepCode;

        var dailyDepositData = dailyChargeReportData.FirstOrDefault(x => x.DepName == depName && x.DepCode == depCode);
        var monthlyDepositData = monthlyChargeReportData.FirstOrDefault(x => x.DepName == depName && x.DepCode == depCode);
        var yearlyDepositData = yearlyChargeReportData.FirstOrDefault(x => x.DepName == depName && x.DepCode == depCode);

        i = 0;
        str.Append(PadR(depName, columnPadding[i++]));

        if (dailyDepositData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(dailyDepositData.Qty?.ToString("0.000") ?? "0.00", columnPadding[i++]));
          str.Append(PadL(dailyDepositData.Deposit?.ToString(fA) ?? "0.00", columnPadding[i++]));
          dailyQtyTotal += dailyDepositData.Qty ?? 0;
          dailyAmtTotal += dailyDepositData.Deposit ?? 0;
          dailyNet += dailyDepositData.Deposit ?? 0;
        }

        if (monthlyDepositData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(monthlyDepositData.Qty?.ToString("0.000") ?? "0.00", columnPadding[i++]));
          str.Append(PadL(monthlyDepositData.Deposit?.ToString(fA) ?? "0.00", columnPadding[i++]));
          monthlyQtyTotal += monthlyDepositData.Qty ?? 0;
          monthlyAmtTotal += monthlyDepositData.Deposit ?? 0;
          monthlyNet += monthlyDepositData.Deposit ?? 0;
        }

        if (yearlyDepositData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.AppendLine(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(yearlyDepositData.Qty?.ToString("0.000") ?? "0.00", columnPadding[i++]));
          str.AppendLine(PadL(yearlyDepositData.Deposit?.ToString(fA) ?? "0.00", columnPadding[i++]));
          yearlyQtyTotal += yearlyDepositData.Qty ?? 0;
          yearlyAmtTotal += yearlyDepositData.Deposit ?? 0;
          yearlyNet += yearlyDepositData.Deposit ?? 0;
        }
      }

      i = 0;
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.Append(PadC("", columnPadding[i++]));
      str.Append(PadL(dailyQtyTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(dailyAmtTotal.ToString("0.000"), columnPadding[i++]));
      str.Append(PadL(monthlyQtyTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(monthlyAmtTotal.ToString("0.000"), columnPadding[i++]));
      str.Append(PadL(yearlyQtyTotal.ToString(fA), columnPadding[i++]));
      str.AppendLine(PadL(yearlyAmtTotal.ToString("0.000"), columnPadding[i++]));
      str.AppendLine();
    }

    if (yearlyNet != 0)
    {
      i = 0;
      str.Append(PadR(_localizer.GetString("NET1(Sales+Disc+Tax+Charges)"), columnPadding[i++]));
      str.Append(PadL("", columnPadding[i++]));

      str.Append(PadL(dailyNet.ToString(fA), columnPadding[i++]));
      str.Append(PadL("", columnPadding[i++]));

      str.Append(PadL(monthlyNet.ToString(fA), columnPadding[i++]));
      str.Append(PadL("", columnPadding[i++]));

      str.AppendLine(PadL(yearlyNet.ToString(fA), columnPadding[i++]));

      str.AppendLine();
    }

    dailyQtyTotal = 0;
    dailyAmtTotal = 0;
    monthlyQtyTotal = 0;
    monthlyAmtTotal = 0;
    yearlyQtyTotal = 0;
    yearlyAmtTotal = 0;

    dailyNet = 0;
    monthlyNet = 0;
    yearlyNet = 0;

    url = "TextReports/GetFinancialReportSaleTendersData";

    var saleTendersData = await _http.PostAndParseAsync<IEnumerable<TendersDtoViewModel>>(url, textReportParams);

    url = "TextReports/GetFinancialReportTendersData";
    textReportParams.Period = 0;

    var dailyTendersReportData = await _http.PostAndParseAsync<IEnumerable<TendersDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyTendersReportData = await _http.PostAndParseAsync<IEnumerable<TendersDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyTendersReportData = await _http.PostAndParseAsync<IEnumerable<TendersDtoViewModel>>(url, textReportParams);

    if (saleTendersData.Any())
    {
      str.AppendLine(_localizer.GetString("Tenders"));
      str.AppendLine();

      foreach (var tender in saleTendersData)
      {
        var tenderName = tender.Tender?.ToString();

        var dailyTenderData = dailyTendersReportData.FirstOrDefault(x => x.Tender.ToString().ToLower() == tenderName?.ToLower());
        var monthlyTenderData = monthlyTendersReportData.FirstOrDefault(x => x.Tender.ToString().ToLower() == tenderName?.ToLower());
        var yearlyTenderData = yearlyTendersReportData.FirstOrDefault(x => x.Tender.ToString().ToLower() == tenderName?.ToLower());


        i = 0;
        str.Append(PadR(tenderName, columnPadding[i++]));

        if (dailyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(dailyTenderData.TotQty == null ? "0.000" : Convert.ToDecimal(dailyTenderData.TotQty).ToString("0.000"), columnPadding[i++]));
          str.Append(PadL(dailyTenderData.Amount == null ? "0.00" : Convert.ToDecimal(dailyTenderData.Amount).ToString(fA), columnPadding[i++]));
          dailyQtyTotal += dailyTenderData.TotQty == null ? 0 : Convert.ToDecimal(dailyTenderData.TotQty);
          dailyAmtTotal += dailyTenderData.Amount == null ? 0 : Convert.ToDecimal(dailyTenderData.Amount);
        }

        if (monthlyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(monthlyTenderData.TotQty == null ? "0.000" : Convert.ToDecimal(monthlyTenderData.TotQty).ToString("0.000"), columnPadding[i++]));
          str.Append(PadL(monthlyTenderData.Amount == null ? "0.00" : Convert.ToDecimal(monthlyTenderData.Amount).ToString(fA), columnPadding[i++]));
          monthlyQtyTotal += monthlyTenderData.TotQty == null ? 0 : Convert.ToDecimal(monthlyTenderData.TotQty);
          monthlyAmtTotal += monthlyTenderData.Amount == null ? 0 : Convert.ToDecimal(monthlyTenderData.Amount);
        }

        if (yearlyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.AppendLine(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(yearlyTenderData.TotQty == null ? "0.000" : Convert.ToDecimal(yearlyTenderData.TotQty).ToString("0.000"), columnPadding[i++]));
          str.AppendLine(PadL(yearlyTenderData.Amount == null ? "0.00" : Convert.ToDecimal(yearlyTenderData.Amount).ToString(fA), columnPadding[i++]));
          yearlyQtyTotal += yearlyTenderData.TotQty == null ? 0 : Convert.ToDecimal(yearlyTenderData.TotQty);
          yearlyAmtTotal += yearlyTenderData.Amount == null ? 0 : Convert.ToDecimal(yearlyTenderData.Amount);
        }
      }

      i = 0;

      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.Append(PadC("", columnPadding[i++]));
      str.Append(PadL(dailyQtyTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(dailyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(monthlyQtyTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(monthlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(yearlyQtyTotal.ToString(fA), columnPadding[i++]));
      str.AppendLine(PadL(yearlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.AppendLine();

      dailyNet += dailyAmtTotal;
      monthlyNet += monthlyAmtTotal;
      yearlyNet += yearlyAmtTotal;
    }

    dailyQtyTotal = 0;
    dailyAmtTotal = 0;
    monthlyQtyTotal = 0;
    monthlyAmtTotal = 0;
    yearlyQtyTotal = 0;
    yearlyAmtTotal = 0;

    url = "TextReports/GetFinancialReportARTendersData";

    var saleARTendersData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    url = "TextReports/GetFinancialReportARPayData";
    textReportParams.Period = 0;

    var dailyARPayReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyARPayReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyARPayReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    if (saleARTendersData.Any())
    {
      str.AppendLine(_localizer.GetString("-arPay"));
      str.AppendLine();

      foreach (var tender in saleARTendersData)
      {
        var tenderName = tender.Tender;

        var dailyTenderData = dailyARPayReportData.FirstOrDefault(x => x.Tender == tenderName);
        var monthlyTenderData = monthlyARPayReportData.FirstOrDefault(x => x.Tender == tenderName);
        var yearlyTenderData = yearlyARPayReportData.FirstOrDefault(x => x.Tender == tenderName);

        i = 0;
        str.Append(PadR(tenderName, columnPadding[i++]));

        if (dailyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(dailyTenderData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.Append(PadL(dailyTenderData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
          dailyQtyTotal += dailyTenderData.TotQty ?? 0;
          dailyAmtTotal += dailyTenderData.Amount ?? 0;
        }

        if (monthlyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(monthlyTenderData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.Append(PadL(monthlyTenderData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
          monthlyQtyTotal += monthlyTenderData.TotQty ?? 0;
          monthlyAmtTotal += monthlyTenderData.Amount ?? 0;
        }

        if (yearlyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.AppendLine(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(yearlyTenderData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.AppendLine(PadL(yearlyTenderData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
          yearlyQtyTotal += yearlyTenderData.TotQty ?? 0;
          yearlyAmtTotal += yearlyTenderData.Amount ?? 0;
        }
      }

      i = 0;
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.Append(PadC("", columnPadding[i++]));
      str.Append(PadL(dailyQtyTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(dailyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(monthlyQtyTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(monthlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(yearlyQtyTotal.ToString(fA), columnPadding[i++]));
      str.AppendLine(PadL(yearlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.AppendLine();

      dailyNet -= dailyAmtTotal;
      monthlyNet -= monthlyAmtTotal;
      yearlyNet -= yearlyAmtTotal;
    }

    dailyQtyTotal = 0;
    dailyAmtTotal = 0;
    monthlyQtyTotal = 0;
    monthlyAmtTotal = 0;
    yearlyQtyTotal = 0;
    yearlyAmtTotal = 0;

    url = "TextReports/GetFinancialReportPaymntTendersData";

    var saleReportPaymntData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    url = "TextReports/GetFinancialReportPaymntData";
    textReportParams.Period = 0;

    var dailyReportPaymntReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyReportPaymntReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyReportPaymntReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    if (saleReportPaymntData.Any())
    {
      str.AppendLine(_localizer.GetString("-Payments"));
      str.AppendLine();

      foreach (var tender in saleReportPaymntData)
      {
        var tenderName = tender.Tender;

        var dailyTenderData = dailyReportPaymntReportData.FirstOrDefault(x => x.Tender == tenderName);
        var monthlyTenderData = monthlyReportPaymntReportData.FirstOrDefault(x => x.Tender == tenderName);
        var yearlyTenderData = yearlyReportPaymntReportData.FirstOrDefault(x => x.Tender == tenderName);

        i = 0;
        str.Append(PadR(tenderName, columnPadding[i++]));

        if (dailyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(dailyTenderData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.Append(PadL(dailyTenderData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
          dailyQtyTotal += dailyTenderData.TotQty ?? 0;
          dailyAmtTotal += dailyTenderData.Amount ?? 0;
        }

        if (monthlyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.Append(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(monthlyTenderData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.Append(PadL(monthlyTenderData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
          monthlyQtyTotal += monthlyTenderData.TotQty ?? 0;
          monthlyAmtTotal += monthlyTenderData.Amount ?? 0;
        }

        if (yearlyTenderData == null)
        {
          str.Append(PadL("0.000", columnPadding[i++]));
          str.AppendLine(PadL("0.00", columnPadding[i++]));
        }
        else
        {
          str.Append(PadL(yearlyTenderData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
          str.AppendLine(PadL(yearlyTenderData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
          yearlyQtyTotal += yearlyTenderData.TotQty ?? 0;
          yearlyAmtTotal += yearlyTenderData.Amount ?? 0;
        }
      }

      i = 0;
      str.AppendLine(PadC(new string('-', LONG_PRINT_WIDTH), LONG_PRINT_WIDTH - 20));
      str.Append(PadC("", columnPadding[i++]));
      str.Append(PadL(dailyQtyTotal.ToString("0.000"), columnPadding[i++]));
      str.Append(PadL(dailyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(monthlyQtyTotal.ToString("0.000"), columnPadding[i++]));
      str.Append(PadL(monthlyAmtTotal.ToString(fA), columnPadding[i++]));
      str.Append(PadL(yearlyQtyTotal.ToString("0.000"), columnPadding[i++]));
      str.AppendLine(PadL(yearlyAmtTotal.ToString(fA), columnPadding[i++]));

      dailyNet -= dailyAmtTotal;
      monthlyNet -= monthlyAmtTotal;
      yearlyNet -= yearlyAmtTotal;
    }

    url = "TextReports/GetFinancialReportPayoutData";
    textReportParams.Period = 0;

    var dailyPayoutReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyPayoutReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyPayoutReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);


    if (yearlyPayoutReportData != null && yearlyPayoutReportData.Any(y => y.Amount != null && y.Amount != 0))
    {
      i = 0;
      str.Append(PadR(_localizer.GetString("cashPayouts:(-)"), columnPadding[i++]));

      var dailyPayoutData = dailyPayoutReportData.FirstOrDefault();
      var monthlyPayoutData = monthlyPayoutReportData.FirstOrDefault();
      var yearlyPayoutData = yearlyPayoutReportData.FirstOrDefault();

      if (dailyPayoutData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(dailyPayoutData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(dailyPayoutData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
        dailyNet += dailyPayoutData.Amount ?? 0;
      }

      if (monthlyPayoutData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(monthlyPayoutData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(monthlyPayoutData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
        monthlyNet += monthlyPayoutData.Amount ?? 0;
      }

      if (yearlyPayoutData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.AppendLine(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(yearlyPayoutData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.AppendLine(PadL(yearlyPayoutData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
        yearlyNet += yearlyPayoutData.Amount ?? 0;
      }
      str.AppendLine();
    }

    url = "TextReports/GetFinancialReportOverPayData";
    textReportParams.Period = 0;

    var dailyOverPayReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyOverPayReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyOverPayReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    if (yearlyOverPayReportData != null && yearlyOverPayReportData.Any(y => y.Amount != null && y.Amount != 0))
    {
      i = 0;
      str.Append(PadR(_localizer.GetString("cashOverpay:(-)"), columnPadding[i++]));

      var dailyOverPayData = dailyOverPayReportData.FirstOrDefault();
      var monthlyOverPayData = monthlyOverPayReportData.FirstOrDefault();
      var yearlyOverPayData = yearlyOverPayReportData.FirstOrDefault();

      if (dailyOverPayData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(dailyOverPayData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(dailyOverPayData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
        dailyNet -= dailyOverPayData.Amount ?? 0;
      }

      if (monthlyOverPayData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(monthlyOverPayData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(monthlyOverPayData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
        monthlyNet -= monthlyOverPayData.Amount ?? 0;
      }

      if (yearlyOverPayData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.AppendLine(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(yearlyOverPayData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.AppendLine(PadL(yearlyOverPayData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
        yearlyNet -= yearlyOverPayData.Amount ?? 0;
      }
      str.AppendLine();
    }

    if (yearlyNet != 0)
    {
      i = 0;
      str.AppendLine(PadR(_localizer.GetString("NET2(Tends+POs-RAs)"), columnPadding[i++]) +
          PadL("", columnPadding[i++]) +
          PadL(dailyNet.ToString("0.00"), columnPadding[i++]) +
          PadL("", columnPadding[i++]) +
          PadL(monthlyNet.ToString("0.00"), columnPadding[i++]) +
          PadL("", columnPadding[i++]) +
          PadL(yearlyNet.ToString("0.00"), columnPadding[i++])
      );
      str.AppendLine();
    }

    url = "TextReports/GetFinancialReportRefundData";
    textReportParams.Period = 0;

    var dailyRefundReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyRefundReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyRefundReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    if (yearlyRefundReportData != null && yearlyRefundReportData.Any(y => y.Amount != null && y.Amount != 0))
    {
      i = 0;

      str.Append(PadR(_localizer.GetString("refund"), columnPadding[i++]));

      var dailyRefundData = dailyRefundReportData.FirstOrDefault();
      var monthlyRefundData = monthlyRefundReportData.FirstOrDefault();
      var yearlyRefundData = yearlyRefundReportData.FirstOrDefault();

      if (dailyRefundData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(dailyRefundData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(dailyRefundData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
      }

      if (monthlyRefundData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(monthlyRefundData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(monthlyRefundData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
      }

      if (yearlyRefundData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.AppendLine(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(yearlyRefundData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.AppendLine(PadL(yearlyRefundData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
      }
      str.AppendLine();
    }

    url = "TextReports/GetFinancialReportDelVoidData";
    textReportParams.Period = 0;

    var dailyDelVoidReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyDelVoidReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyDelVoidReportData = await _http.PostAndParseAsync<IEnumerable<FinancialReportDtoViewModel>>(url, textReportParams);

    if (yearlyDelVoidReportData != null && yearlyDelVoidReportData.Any())
    {
      foreach (var yearlyDelVoid in yearlyDelVoidReportData)
      {
        i = 0;
        if (yearlyDelVoid.Action == "D")
        {
          str.Append(PadR(_localizer.GetString("deletedlines"), 20));
        }
        else if (yearlyDelVoid.Action == "V")
        {
          str.Append(PadR(_localizer.GetString("voidedlines"), 20));
        }

        var dailyDelVoidData = dailyDelVoidReportData.FirstOrDefault(d => d.Action == yearlyDelVoid.Action);
        var monthlyDelVoidData = monthlyDelVoidReportData.FirstOrDefault(m => m.Action == yearlyDelVoid.Action);

        if (dailyDelVoidData == null)
        {
          str.Append(PadL("0.000", 15)).Append(PadL("0.00", 15));
        }
        else
        {
          str.Append(PadL(dailyDelVoidData.TotQty?.ToString("0.000") ?? "0.000", 15))
             .Append(PadL(dailyDelVoidData.Amount?.ToString(fA) ?? "0.00", 15));
        }

        if (monthlyDelVoidData == null)
        {
          str.Append(PadL("0.000", 15)).Append(PadL("0.00", 15));
        }
        else
        {
          str.Append(PadL(monthlyDelVoidData.TotQty?.ToString("0.000") ?? "0.000", 15))
             .Append(PadL(monthlyDelVoidData.Amount?.ToString(fA) ?? "0.00", 15));
        }

        if (yearlyDelVoid == null)
        {
          str.Append(PadL("0.000", 15)).AppendLine(PadL("0.00", 15));
        }
        else
        {
          str.Append(PadL(yearlyDelVoid.TotQty?.ToString("0.000") ?? "0.000", 15))
             .AppendLine(PadL(yearlyDelVoid.Amount?.ToString(fA) ?? "0.00", 15));
        }
      }
      str.AppendLine();
    }


    url = "TextReports/GetFinancialReportCustomerCntData";
    textReportParams.Period = 0;

    var dailyCustomerCntReportData = await _http.PostAndParseAsync<IEnumerable<CustomerCntDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 1;

    var monthlyCustomerCntReportData = await _http.PostAndParseAsync<IEnumerable<CustomerCntDtoViewModel>>(url, textReportParams);

    textReportParams.Period = 2;

    var yearlyCustomerCntReportData = await _http.PostAndParseAsync<IEnumerable<CustomerCntDtoViewModel>>(url, textReportParams);

    if (yearlyCustomerCntReportData != null && yearlyCustomerCntReportData.Any(y => y.Amount != null && y.Amount != 0))
    {
      i = 0;
      str.Append(PadR(_localizer.GetString("customercount"), columnPadding[i++]));

      var dailyCustomerCntData = dailyCustomerCntReportData.FirstOrDefault();
      var monthlyCustomerCntData = monthlyCustomerCntReportData.FirstOrDefault();
      var yearlyCustomerCntData = yearlyCustomerCntReportData.FirstOrDefault();

      if (dailyCustomerCntData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(dailyCustomerCntData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(dailyCustomerCntData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
      }

      if (monthlyCustomerCntData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.Append(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(monthlyCustomerCntData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.Append(PadL(monthlyCustomerCntData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
      }

      if (yearlyCustomerCntData == null)
      {
        str.Append(PadL("0.000", columnPadding[i++]));
        str.AppendLine(PadL("0.00", columnPadding[i++]));
      }
      else
      {
        str.Append(PadL(yearlyCustomerCntData.TotQty?.ToString("0.000") ?? "0.000", columnPadding[i++]));
        str.AppendLine(PadL(yearlyCustomerCntData.Amount?.ToString(fA) ?? "0.00", columnPadding[i++]));
      }
      str.AppendLine();
    }


    return str.ToString();
  }

  #endregion T0016

  #region T0017

  private async Task<string> GetEmployeeCommissionReportData(TextReportParamsViewModel textReportParams)
  {
    const string url = "TextReports/GetEmployeeCommissionReportData";
    const string urlForEmployeeCommissionTableData = "TextReports/GetEmployeeCommissionTableData";
    int[] columnPadding = { 15, 20, 15, 15, 12, 12, 10, 12 };
    int LONG_PRINT_WIDTH = columnPadding.Sum();
    int[] singleEmployeeInfoColumnPadding = { 20, 20, LONG_PRINT_WIDTH - 40 };
    var company = await GetCompanyInfoAsync();
    var employeeCommissionReportData = await _http.PostAndParseAsync<IEnumerable<EmployeeCommissionDtoViewModel>>(url, textReportParams);
    var employeeCommissionTableData = await _http.PostAndParseAsync<IEnumerable<EmployeeCommissionViewModel>>(urlForEmployeeCommissionTableData, textReportParams);

    if (employeeCommissionReportData == null || !employeeCommissionReportData.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');
    str.AppendLine();

    if (_config.IsBackOffice)
    {
      str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    }
    else
    {
      str.AppendLine(PadC($"{company.CompanyName} ({textReportParams.StoreID})", LONG_PRINT_WIDTH));
    }

    str.AppendLine(PadC($"{company.Address1} , {company.City}", LONG_PRINT_WIDTH));
    str.AppendLine(Environment.NewLine);
    str.AppendLine(PadC(_localizer.GetString("employeeCommissionReport").Value, LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value.ToString("dd-MMM-yyyy")} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value.ToString("dd-MMM-yyyy")}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    int i = 0;
    int employeeNumber = 0;
    string all = _localizer.GetString("all");
    decimal totalCommission = 0;

    foreach (var employeeCommission in employeeCommissionReportData)
    {
      if (employeeCommission.RowNumber == 1)
      {
        if (employeeNumber != 0)
        {
          str.AppendLine();
          str.AppendLine(PadR($"Total Commission: {totalCommission:$0.00;$-0.00}", LONG_PRINT_WIDTH));
          str.AppendLine(new string('-', LONG_PRINT_WIDTH));
          str.AppendLine();
        }

        employeeNumber++;
        totalCommission = 0;
        i = 0;
        str.Append(PadR(_localizer.GetString("employeeID"), singleEmployeeInfoColumnPadding[i++]));
        str.Append(PadR(employeeCommission.Code.ToString(), singleEmployeeInfoColumnPadding[i++]));
        str.AppendLine(PadR(employeeCommission.Name?.ToString(), singleEmployeeInfoColumnPadding[i++]));

        i = 0;
        str.Append(PadR(_localizer.GetString("stockCode"), columnPadding[i++]));
        str.Append(PadR(_localizer.GetString("description"), columnPadding[i++]));
        str.Append(PadR(_localizer.GetString("department"), columnPadding[i++]));
        str.Append(PadR(_localizer.GetString("subDept"), columnPadding[i++]));
        str.Append(PadR(_localizer.GetString("subDetail"), columnPadding[i++]));
        str.Append(PadR(_localizer.GetString("quantity"), columnPadding[i++]));
        str.Append(PadR(_localizer.GetString("amount"), columnPadding[i++]));
        str.AppendLine(PadR(_localizer.GetString("commission"), columnPadding[i++]));
      }

      var employeeByStockCode = employeeCommissionTableData
        .FirstOrDefault(e => e.ByStockCode == true &&
                    e.EmployeeID == employeeCommission.Code &&
                    e.StockCode == employeeCommission.StockCode &&
                    e.Dept != all &&
                    e.SubDept != all &&
                    e.SubDetail != all);

      if (employeeByStockCode!=null)
      {
        if (employeeByStockCode.Apply == true)
        {
          if (!string.IsNullOrEmpty(employeeByStockCode.Commission.ToString()) && employeeByStockCode.Commission != 0.00m)
          {
            var commissionAmount = employeeCommission.Amount * (employeeByStockCode.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = employeeByStockCode.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var employeeByDeptSubDeptSubDetail = employeeCommissionTableData
        .FirstOrDefault(c => c.ByStockCode == false &&
                    c.EmployeeID == employeeCommission.Code &&
                    c.Dept == (string.IsNullOrEmpty(employeeCommission.Dept) ? all : employeeCommission.Dept) &&
                    c.SubDept == (string.IsNullOrEmpty(employeeCommission.SubDept) ? all : employeeCommission.SubDept) &&
                    c.SubDetail == (string.IsNullOrEmpty(employeeCommission.SubDetail) ? all : employeeCommission.SubDetail) &&
                    c.StockCode == employeeCommission.StockCode);

      if (employeeByDeptSubDeptSubDetail != null)
      {
        if (employeeByDeptSubDeptSubDetail.Apply == true)
        {
          if (employeeByDeptSubDeptSubDetail.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (employeeByDeptSubDeptSubDetail.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = employeeByDeptSubDeptSubDetail.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var employeeByDeptSubDept = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.EmployeeID == employeeCommission.Code &&
            e.Dept == (string.IsNullOrEmpty(employeeCommission.Dept) ? all : employeeCommission.Dept) &&
            e.SubDept == (string.IsNullOrEmpty(employeeCommission.SubDept) ? all : employeeCommission.SubDept) &&
            e.SubDetail == all &&
            e.StockCode == employeeCommission.StockCode
        );

      if (employeeByDeptSubDept != null)
      {
        if (employeeByDeptSubDept.Apply == true)
        {
          if (employeeByDeptSubDept.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (employeeByDeptSubDept.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = employeeByDeptSubDept.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var employeeByDept = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.EmployeeID == employeeCommission.Code &&
            e.Dept == employeeCommission.Dept &&
            e.SubDept == all &&
            e.SubDetail == all &&
            e.StockCode == employeeCommission.StockCode
        );

      if (employeeByDept != null)
      {
        if (employeeByDept.Apply == true)
        {
          if (employeeByDept.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (employeeByDept.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = employeeByDeptSubDept.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var getAllForEmployee = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.EmployeeID == employeeCommission.Code &&
            e.Dept == all &&
            e.SubDept == all &&
            e.SubDetail == all &&
            e.StockCode == employeeCommission.StockCode
        );

      if (getAllForEmployee != null)
      {
        if (getAllForEmployee.Apply == true)
        {
          if (getAllForEmployee.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (getAllForEmployee.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = getAllForEmployee.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var allEmployeeByStockCode = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.ByStockCode == true &&
            e.EmployeeID == all &&
            e.StockCode == employeeCommission.StockCode &&
            e.Dept != all &&
            e.SubDept != all &&
            e.SubDetail != all
        );

      if (allEmployeeByStockCode != null)
      {
        if (allEmployeeByStockCode.Apply == true)
        {
          if (allEmployeeByStockCode.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (allEmployeeByStockCode.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = allEmployeeByStockCode.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var allEmployeeByDeptSubDeptSubDetail = employeeCommissionTableData
       .FirstOrDefault(e =>
           e.ByStockCode == false &&
           e.EmployeeID == all &&
           e.Dept == (string.IsNullOrEmpty(employeeCommission.Dept) ? all : employeeCommission.Dept) &&
           e.SubDept == (string.IsNullOrEmpty(employeeCommission.SubDept) ? all : employeeCommission.SubDept) &&
           e.SubDetail == (string.IsNullOrEmpty(employeeCommission.SubDetail) ? all : employeeCommission.SubDetail) &&
           e.StockCode == employeeCommission.StockCode
       );

      if (allEmployeeByDeptSubDeptSubDetail != null)
      {
        if (allEmployeeByDeptSubDeptSubDetail.Apply == true)
        {
          if (allEmployeeByDeptSubDeptSubDetail.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (allEmployeeByDeptSubDeptSubDetail.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = allEmployeeByDeptSubDeptSubDetail.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var allEmployeeByDeptSubDept = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.EmployeeID == all &&
            e.Dept == employeeCommission.Dept &&
            e.SubDept == (string.IsNullOrEmpty(employeeCommission.SubDept) ? all : employeeCommission.SubDept) &&
            e.SubDetail == all &&
            e.StockCode == employeeCommission.StockCode
        );

      if (allEmployeeByDeptSubDept != null)
      {
        if (allEmployeeByDeptSubDept.Apply == true)
        {
          if (allEmployeeByDeptSubDept.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (allEmployeeByDeptSubDept.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = allEmployeeByDeptSubDept.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var allEmployeeByDept = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.EmployeeID == all &&
            e.Dept == employeeCommission.Dept &&
            e.SubDept == all &&
            e.SubDetail == all &&
            e.StockCode == employeeCommission.StockCode
          );

      if (allEmployeeByDept != null)
      {
        if (allEmployeeByDept.Apply == true)
        {
          if (allEmployeeByDept.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (allEmployeeByDept.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = allEmployeeByDept.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }

      var allemployeeByDefault = employeeCommissionTableData
        .FirstOrDefault(e =>
            e.EmployeeID == all &&
            e.Dept == all &&
            e.SubDept == all &&
            e.SubDetail == all &&
            e.StockCode == employeeCommission.StockCode
        );

      if (allemployeeByDefault != null)
      {
        if (allemployeeByDefault.Apply == true)
        {
          if (allemployeeByDefault.Commission != 0m)
          {
            var commissionAmount = employeeCommission.Amount * (allemployeeByDefault.Commission / 100.00M);
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
          else
          {
            var commissionAmount = allemployeeByDefault.CommissionByDol * employeeCommission.Quantity;
            str.Append(PrintCommissionDetails(employeeCommission, commissionAmount, columnPadding));
            totalCommission += commissionAmount ?? 0m;
          }
        }

        continue;
      }
    }

    if (employeeNumber != 0)
    {
      str.AppendLine();
      str.AppendLine(PadR($"Total Commission: {totalCommission:$0.00;$-0.00}", LONG_PRINT_WIDTH));
      str.AppendLine(new string('-', LONG_PRINT_WIDTH));
      str.AppendLine();
    }

    return str.ToString();
  }

  #endregion T0017

  #region T0018 && T0019

  private async Task<string> GetCustomerSalesDetailReportDataAsync(TextReportParamsViewModel textReportParams)
  {
    const int LONG_PRINT_WIDTH = 158;
    const string url = "TextReports/GetCustomerSalesDetailReportData";
    int[] columnPadding = { 20, 22, 14, 11, 17, 37, 10, 10, 10, 7 };
    int padIdx = 0;
    var customerCode = string.Empty;
    int saleSum = 0;
    decimal totalAmount = 0;
    int totalSaleNumber = 0;
    decimal totalTaxes = 0;
    decimal totalDiscount = 0;
    decimal totalDeposit = 0;
    decimal totalOverpayment = 0;
    decimal customerTotalAmount = 0;
    int customerTotalSaleNumber = 0;
    decimal customerTotalTaxes = 0;
    decimal customerTotalDiscount = 0;
    decimal customerTotalDeposit = 0;
    decimal customerTotalOverpayment = 0;
    var customerSalesDetail = await _http.PostAndParseAsync<IEnumerable<CustomerSalesDetailViewModel>>(url, textReportParams);

    if (customerSalesDetail == null || !customerSalesDetail.Any())
    {
      return Environment.NewLine + _localizer.GetString("noData").Value.PadC(LONG_PRINT_WIDTH) + Environment.NewLine + Environment.NewLine;
    }

    var str = new StringBuilder();
    int index = textReportParams.StoreName!.IndexOf('-');
    var reportTitle = textReportParams.ReportName == "T0018" ? _localizer.GetString("arCustomerSalesReport") : _localizer.GetString("customerSalesDetailReport");

    str.AppendLine();
    str.AppendLine(PadC(textReportParams.StoreName.Remove(index, 1).Insert(index, " "), LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{reportTitle} ({textReportParams.ReportName})", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.AppendLine(PadC($"{_localizer.GetString("printedOn")}: {DateTime.Now.ToString($"dd-MMM-yyyy {timeFormat}")}", LONG_PRINT_WIDTH));
    str.AppendLine(PadC($"{_localizer.GetString("startDate").Value} {textReportParams.StartDate.Value:dd-MMM-yyyy} {_localizer.GetString("endDate").Value} {textReportParams.EndDate.Value:dd-MMM-yyyy}", LONG_PRINT_WIDTH));
    str.AppendLine();
    str.Append(PadR(_localizer.GetString("saleNo"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("cardNumber"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("date"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("time"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("product"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("productName"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("qty"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("taxes"), columnPadding[padIdx++]));
    str.Append(PadR(_localizer.GetString("amount"), columnPadding[padIdx++]));
    str.AppendLine(PadR(_localizer.GetString("type"), columnPadding[padIdx++]));
    str.AppendLine(new string('-', LONG_PRINT_WIDTH));

    foreach(var saleDetail in customerSalesDetail)
    {
      padIdx = 0;
      decimal taxAmount = saleDetail.LineTaxAmount + saleDetail.ChargeTaxAmount + saleDetail.KitTaxAmount;
      decimal depositAmount = saleDetail.SaleAmount + saleDetail.KitAmount;

      if (!saleDetail.ClientCode.Equals(customerCode, StringComparison.OrdinalIgnoreCase))
      {
        if (!string.IsNullOrEmpty(customerCode))
        {
          str.AppendLine(new string('-', LONG_PRINT_WIDTH));
          str.Append(PadR($"{_localizer.GetString("totalNumberOfSales")}: {customerTotalSaleNumber}", 40));
          str.Append(PadR($"{_localizer.GetString("totalTaxes")}: {customerTotalTaxes:####0.00}", 20));
          str.Append(PadR($"{_localizer.GetString("totalCharges")}: {customerTotalDeposit:####0.00}", 30));
          str.AppendLine(PadR($"{_localizer.GetString("totalDiscount")}: {customerTotalDiscount:####0.00}", 22));
          str.Append(PadR($"{_localizer.GetString("overpayment")}: {customerTotalOverpayment:####0.00}", 23));
          str.AppendLine(PadR($"{_localizer.GetString("totalAmount")}: {customerTotalAmount:####0.00}", 23));
          str.AppendLine();

          totalSaleNumber += customerTotalSaleNumber;
          totalDiscount += customerTotalDiscount;
          totalAmount += customerTotalAmount;
          totalTaxes += customerTotalTaxes;
          totalDeposit += customerTotalDeposit;
          totalOverpayment += customerTotalOverpayment;
          customerTotalAmount = 0;
          customerTotalSaleNumber = 0;
          customerTotalTaxes = 0;
          customerTotalDiscount = 0;
          customerTotalDeposit = 0;
          customerTotalOverpayment = 0;
        }

        customerCode = saleDetail.ClientCode;
        str.AppendLine(PadR($"{_localizer.GetString("customer")}: {saleDetail.ClientCode} -{saleDetail.ClientName}", LONG_PRINT_WIDTH));
      }

      str.Append(PadR(saleDetail.SaleNumber.ToString(), columnPadding[padIdx++]));
      str.Append(PadR(saleDetail.CardNumber, columnPadding[padIdx++]));
      str.Append(PadR(saleDetail.SaleDate?.ToString("yyyy-MM-dd"), columnPadding[padIdx++]));
      str.Append(PadR(saleDetail.SaleTime?.ToString("HH:mm:ss"), columnPadding[padIdx++]));
      str.Append(PadR(saleDetail.StockCode, columnPadding[padIdx++]));
      str.Append(PadR(saleDetail.StockName, columnPadding[padIdx++]));
      str.Append(PadR(saleDetail.Quantity.ToString("##0.000"), columnPadding[padIdx++]));
      str.Append(PadR(taxAmount.ToString("#0.00"), columnPadding[padIdx++]));
      str.Append(PadR((saleDetail.Amount + taxAmount - saleDetail.Discount - saleDetail.DiscountAdjustment + depositAmount).ToString("###0.00"), columnPadding[padIdx++]));
      str.AppendLine(PadR(saleDetail.SaleType, columnPadding[padIdx++]));

      if (saleDetail.SaleNumber != saleSum)
      {
        saleSum = saleDetail.SaleNumber;
        customerTotalSaleNumber++;
      }

      customerTotalTaxes += taxAmount;
      customerTotalDiscount += saleDetail.Discount + saleDetail.DiscountAdjustment;
      customerTotalDeposit += depositAmount;
      customerTotalOverpayment += saleDetail.Overpayment;
      customerTotalAmount += saleDetail.Amount + taxAmount - saleDetail.Discount - saleDetail.DiscountAdjustment + depositAmount + saleDetail.Overpayment;
    }

    str.AppendLine(new string('-', LONG_PRINT_WIDTH));
    str.Append(PadR($"{_localizer.GetString("totalNumberOfSales")}: {customerTotalSaleNumber}", 40));
    str.Append(PadR($"{_localizer.GetString("totalTaxes")}: {customerTotalTaxes:####0.00}", 20));
    str.Append(PadR($"{_localizer.GetString("totalCharges")}: {customerTotalDeposit:####0.00}", 30));
    str.AppendLine(PadR($"{_localizer.GetString("totalDiscount")}: {customerTotalDiscount:####0.00}", 22));
    str.Append(PadR($"{_localizer.GetString("overpayment")}: {customerTotalOverpayment:####0.00}", 23));
    str.AppendLine(PadR($"{_localizer.GetString("totalAmount")}: {customerTotalAmount:####0.00}", 23));
    str.AppendLine();

    totalSaleNumber += customerTotalSaleNumber;
    totalDiscount += customerTotalDiscount;
    totalAmount += customerTotalAmount;
    totalTaxes += customerTotalTaxes;
    totalDeposit += customerTotalDeposit;
    totalOverpayment += customerTotalOverpayment;

    str.AppendLine(new string('=', LONG_PRINT_WIDTH));
    str.Append(PadR($"{_localizer.GetString("totalNumberOfSales")}: {totalSaleNumber}", 40));
    str.Append(PadR($"{_localizer.GetString("totalTaxes")}: {totalTaxes:####0.00}", 20));
    str.Append(PadR($"{_localizer.GetString("totalCharges")}: {totalDeposit:####0.00}", 30));
    str.AppendLine(PadR($"{_localizer.GetString("totalDiscount")}: {totalDiscount:####0.00}", 22));
    str.Append(PadR($"{_localizer.GetString("overpayment")}: {totalOverpayment:####0.00}", 23));
    str.AppendLine(PadR($"{_localizer.GetString("totalAmount")}: {totalAmount:####0.00}", 23));
    str.AppendLine();

    return str.ToString();
  }

  #endregion

  #region Private Methods

  private async Task<SelectList> GetCompanyAsync()
  {
    const string url = "CompanySetup/GetCompany";
    var company = await _http.GetAndParseAsync<Company>(url);
    company.CompanyName = $"{company.StoreCode}-{company.CompanyName}";
    var companyList = new List<Company> { company };
    return new SelectList(companyList, "StoreCode", "CompanyName");
  }

  private async Task<CompanyViewModel?> GetCompanyInfoAsync()
  {
    const string url = "CompanySetup/GetCompany";
    return await _http.GetAndParseAsync<CompanyViewModel?>(url);
  }

  private string GetMaskedCardNumber(string cardNumber, string cardType)
  {
    if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length <= 10)
    {
      return cardNumber;
    }

    if (!cardType.Equals("C", StringComparison.OrdinalIgnoreCase) && !cardType.Equals("D", StringComparison.OrdinalIgnoreCase))
    {
      return cardNumber;
    }

    return cardNumber.Substring(0, 6) + new string('*', cardNumber.Length - 10) + cardNumber.Substring(cardNumber.Length - 4);
  }

  private async Task<DiscountViewModel?> GetCardDiscountFromCardName(string cardName)
  {
    string url = $"TextReports/GetCardDiscountFromCardName?cardName={cardName}";

    var discountProperties = await _http.GetAndParseAsync<DiscountViewModel?>(url);
    return discountProperties;
  }

  private string PadC(string str, int length, char paddingChar = ' ')
  {
    str ??= string.Empty;
    return str.PadC(length, paddingChar);
  }

  private string PadR(string str, int length)
  {
    str ??= string.Empty;
    return str.PadRightFixed(length);
  }

  private string PadL(string str, int length)
  {
    str ??= string.Empty;
    return str.PadLeftFixed(length);
  }

  private string? PrintCommissionDetails(EmployeeCommissionDtoViewModel employeeCommission, decimal? commissionAmount, int[] columnPadding)
  {
    var str = new StringBuilder();
    int i = 0;
    str.Append(PadR(employeeCommission.StockCode.ToString(), columnPadding[i++]));
    str.Append(PadR(employeeCommission.Description.ToString(), columnPadding[i++]));
    str.Append(PadR(employeeCommission.Dept.ToString(), columnPadding[i++]));
    str.Append(PadR(employeeCommission.SubDept.ToString(), columnPadding[i++]));
    str.Append(PadR(employeeCommission.SubDetail.ToString(), columnPadding[i++]));
    str.Append(PadR(employeeCommission.Quantity.ToString(), columnPadding[i++]));
    str.Append(PadR(employeeCommission.Amount.Value.ToString("$#0.00;$-#0.00"), columnPadding[i++]));
    str.AppendLine(PadR(commissionAmount.Value.ToString("$0.00;$-0.00"), columnPadding[i++]));

    return str.ToString();
  }
  #endregion Private Methods
}
