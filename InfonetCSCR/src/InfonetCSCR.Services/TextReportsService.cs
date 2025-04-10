namespace InfonetCSCR.Services;

public class TextReportsService : ITextReportsService
{
  private readonly IUnitOfWork _unitOfWork;

  public TextReportsService(IApplicationConfig applicationConfig)
  {
    _unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
  }

  public async Task<IEnumerable<TextReport>> GetAllTextReportsListAsync()
  {
    var textReportsList = await _unitOfWork.TextReports.GetAllTextReportsListAsync();
    _unitOfWork.Commit();
    return textReportsList;
  }

  public async Task<IEnumerable<CardDetails>> GetCardDetailsReportDataAsync(TextReportParams textReportParams)
  {
    var cardDetails = await _unitOfWork.TextReports.GetCardDetailsReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return cardDetails;
  }

  public async Task<IEnumerable<FleetCardFuel>> GetFleetCardFuelReportData(TextReportParams textReportParams)
  {
    string fuelDept = await _unitOfWork.Fuels.GetFuelDepartmentAsync() ?? string.Empty;
    var fleetCardFuels = await _unitOfWork.TextReports.GetFleetCardFuelReportDataAsync(textReportParams, fuelDept);
    _unitOfWork.Commit();
    return fleetCardFuels;
  }

  public async Task<DiscountDto?> GetCardDiscountFromCardName(string cardname)
  {
    var discountProperties = await _unitOfWork.TextReports.GetCardDiscountFromCardNameAsync(cardname);
    _unitOfWork.Commit();
    return discountProperties;
  }

  public async Task<IEnumerable<TotalizerReading>?> GetTotalizerReadingReportData(TextReportParams textReportParams)
  {
    var totalizerReadings = await _unitOfWork.TextReports.GetTotalizerReadingReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return totalizerReadings;
  }

  public async Task<IEnumerable<CreditCardSummary>?> GetCreditCardSummaryReportData(TextReportParams textReportParams)
  {
    var creditCardSummary = await _unitOfWork.TextReports.GetCreditCardSummaryReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return creditCardSummary;
  }

  public async Task<IEnumerable<ARCustomerSales>?> GetARCustomerSalesReportData(TextReportParams textReportParams)
  {
    var creditCardSummary = await _unitOfWork.TextReports.GetARCustomerSalesReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return creditCardSummary;
  }

  public async Task<IEnumerable<DailyFuelDiscount>> GetDailyFuelDiscountReportData(TextReportParams textReportParams)
  {
    string fuelDept = await _unitOfWork.Fuels.GetFuelDepartmentAsync() ?? string.Empty;
    var creditCardSummary = await _unitOfWork.TextReports.GetDailyFuelDiscountReportDataAsync(textReportParams, fuelDept);
    _unitOfWork.Commit();
    return creditCardSummary;
  }

  public async Task<IEnumerable<FuelSaleVariance>> GetSaleVarianceReportData(TextReportParams textReportParams)
  {
    var creditCardSummary = await _unitOfWork.TextReports.GetSaleVarianceReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return creditCardSummary;
  }

  public async Task<ShiftAnalysisDto?> GetShiftAnalysisReportData(TextReportParams textReportParams)
  {
    var employeesAndShifts = await _unitOfWork.TextReports.GetEmployeesAndShiftsAsync(textReportParams);

    var employeeIDs = employeesAndShifts.Select(vm => vm.Employee).Where(e => !string.IsNullOrEmpty(e)).Distinct().ToList();

    string fuelDept = await _unitOfWork.Fuels.GetFuelDepartmentAsync() ?? string.Empty;
    var shiftAnalysisReportData = await _unitOfWork.TextReports.GetShiftAnalysisReportDataAsync(employeesAndShifts, textReportParams, fuelDept);
    _unitOfWork.Commit();

    var shiftAnalysisDto = new ShiftAnalysisDto { EmployeesAndShifts = employeesAndShifts, ShiftAnalysis = shiftAnalysisReportData };
    return shiftAnalysisDto;
  }

  public async Task<IEnumerable<FleetCardSale>?> GetFleetCardSaleReportData(TextReportParams textReportParams)
  {
    var fleetCardSaleReportData = await _unitOfWork.TextReports.GetFleetCardSaleReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return fleetCardSaleReportData;
  }

  public async Task<IEnumerable<AffdaSaleDetail>?> GetAffdaSaleDetailReportData(TextReportParams textReportParams)
  {
    var affdaSaleDetailReportData = await _unitOfWork.TextReports.GetAffdaSaleDetailReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return affdaSaleDetailReportData;
  }

  public async Task<IEnumerable<AffdaSaleDetail>?> GetAffdaTaxSaleSummaryReportData(TextReportParams textReportParams)
  {
    var affdaSaleSummaryReportData = await _unitOfWork.TextReports.GetAffdaTaxSaleSummaryReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return affdaSaleSummaryReportData;
  }

  public async Task<IEnumerable<TankDip>> GetTankDipReportData(TextReportParams textReportParams)
  {
    var tankDips = await _unitOfWork.TextReports.GetTankDipReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return tankDips;
  }

  public async Task<IEnumerable<AdagioDto>> GetAdagioReportData(TextReportParams textReportParams)
  {
    var adagioReportData = await _unitOfWork.TextReports.GetAdagioReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return adagioReportData;
  }

  public async Task<IEnumerable<IncompleteTransactionsDto>> GetIncompleteTransactionsReportData(TextReportParams textReportParams)
  {
    var incompleteTransactionsReportData = await _unitOfWork.TextReports.GetIncompleteTransactionsReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return incompleteTransactionsReportData;
  }

  public async Task<IEnumerable<EmployeeCommissionDto>> GetEmployeeCommissionReportData(TextReportParams textReportParams)
  {
    var employeeCommissionReportData = await _unitOfWork.TextReports.GetEmployeeCommissionReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return employeeCommissionReportData;
  }

  public async Task<IEnumerable<EmployeeCommission>> GetEmployeeCommissionTableData(TextReportParams textReportParams)
  {
    var employeeCommissionTableData = await _unitOfWork.TextReports.GetEmployeeCommissionTableDataAsync(textReportParams);
    _unitOfWork.Commit();
    return employeeCommissionTableData;
  }

  public async Task<IEnumerable<SalesDto>> GetFinancialReportSalesDeptData(TextReportParams textReportParams)
  {
    var financialReportSalesDeptData = await _unitOfWork.TextReports.GetFinancialReportSalesDeptDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportSalesDeptData;
  }

  public async Task<IEnumerable<SalesDto>> GetFinancialReportSalesData(TextReportParams textReportParams)
  {
    var financialReportSalesData = await _unitOfWork.TextReports.GetFinancialReportSalesDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportSalesData;
  }

  public async Task<IEnumerable<TaxesDto>> GetFinancialReportSaleTaxesData(TextReportParams textReportParams)
  {
    var financialReportSaleTaxesData = await _unitOfWork.TextReports.GetFinancialReportSaleTaxesDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportSaleTaxesData;
  }

  public async Task<IEnumerable<TaxesDto>> GetFinancialReportTaxesData(TextReportParams textReportParams)
  {
    var financialReportTaxesData = await _unitOfWork.TextReports.GetFinancialReportTaxesDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportTaxesData;
  }

  public async Task<IEnumerable<ChargeDto>> GetFinancialReportSaleChargesData(TextReportParams textReportParams)
{
    var financialReportSaleChargesData = await _unitOfWork.TextReports.GetFinancialReportSaleChargesDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportSaleChargesData;
  }

  public async Task<IEnumerable<ChargeDto>> GetFinancialReportChargesData(TextReportParams textReportParams)
{
    var financialReportChargesData = await _unitOfWork.TextReports.GetFinancialReportChargesDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportChargesData;
  }

  public async Task<IEnumerable<TendersDto>> GetFinancialReportSaleTendersData(TextReportParams textReportParams)
{
    var financialReportSaleTendersData = await _unitOfWork.TextReports.GetFinancialReportSaleTendersDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportSaleTendersData;
  }

  public async Task<IEnumerable<TendersDto>> GetFinancialReportTendersData(TextReportParams textReportParams)
{
    var financialReportTendersData = await _unitOfWork.TextReports.GetFinancialReportTendersDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportTendersData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportARTendersData(TextReportParams textReportParams)
  {
    var financialReportARTendersData = await _unitOfWork.TextReports.GetFinancialReportARTendersDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportARTendersData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportARPayData(TextReportParams textReportParams)
  {
    var financialReportARPayData = await _unitOfWork.TextReports.GetFinancialReportARPayDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportARPayData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntTendersData(TextReportParams textReportParams)
  {
    var financialReportPaymntTendersData = await _unitOfWork.TextReports.GetFinancialReportPaymntTendersDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportPaymntTendersData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntData(TextReportParams textReportParams)
  {
    var financialReportPaymntData = await _unitOfWork.TextReports.GetFinancialReportPaymntDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportPaymntData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportPayoutData(TextReportParams textReportParams)
  {
    var financialReportPayoutData = await _unitOfWork.TextReports.GetFinancialReportPayoutDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportPayoutData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportOverPayData(TextReportParams textReportParams)
  {
    var financialReportOverPayData = await _unitOfWork.TextReports.GetFinancialReportOverPayDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportOverPayData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportRefundData(TextReportParams textReportParams)
  {
    var financialReportRefundData = await _unitOfWork.TextReports.GetFinancialReportRefundDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportRefundData;
  }

  public async Task<IEnumerable<FinancialReportDto>> GetFinancialReportDelVoidData(TextReportParams textReportParams)
  {
    var financialReportDelVoidData = await _unitOfWork.TextReports.GetFinancialReportDelVoidDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportDelVoidData;
  }

  public async Task<IEnumerable<CustomerCntDto>> GetFinancialReportCustomerCntData(TextReportParams textReportParams)
  {
    var financialReportCustomerCntData = await _unitOfWork.TextReports.GetFinancialReportCustomerCntDataAsync(textReportParams);
    _unitOfWork.Commit();
    return financialReportCustomerCntData;
  }

  public async Task<IEnumerable<CustomerSalesDetail>?> GetCustomerSalesDetailReportDataAsync(TextReportParams textReportParams)
  {
    var customerSalesDetailReportData = await _unitOfWork.TextReports.GetCustomerSalesDetailReportDataAsync(textReportParams);
    _unitOfWork.Commit();
    return customerSalesDetailReportData;
  }
}
