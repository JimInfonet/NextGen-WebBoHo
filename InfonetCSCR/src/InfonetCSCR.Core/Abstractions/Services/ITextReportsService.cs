namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITextReportsService
{
  Task<IEnumerable<TextReport>> GetAllTextReportsListAsync();
  Task<IEnumerable<CardDetails>> GetCardDetailsReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FleetCardFuel>> GetFleetCardFuelReportData(TextReportParams textReportParams);
  Task<DiscountDto?> GetCardDiscountFromCardName(string cardname);
  Task<IEnumerable<TotalizerReading>?> GetTotalizerReadingReportData(TextReportParams textReportParams);
  Task<IEnumerable<CreditCardSummary>?> GetCreditCardSummaryReportData(TextReportParams textReportParams);
  Task<IEnumerable<ARCustomerSales>?> GetARCustomerSalesReportData(TextReportParams textReportParams);
  Task<IEnumerable<DailyFuelDiscount>> GetDailyFuelDiscountReportData(TextReportParams textReportParams);
  Task<IEnumerable<FuelSaleVariance>> GetSaleVarianceReportData(TextReportParams textReportParams);
  Task<ShiftAnalysisDto?> GetShiftAnalysisReportData(TextReportParams textReportParams);
  Task<IEnumerable<FleetCardSale>?> GetFleetCardSaleReportData(TextReportParams textReportParams);
  Task<IEnumerable<AffdaSaleDetail>?> GetAffdaSaleDetailReportData(TextReportParams textReportParams);
  Task<IEnumerable<AffdaSaleDetail>?> GetAffdaTaxSaleSummaryReportData(TextReportParams textReportParams);
  Task<IEnumerable<TankDip>> GetTankDipReportData(TextReportParams textReportParams);
  Task<IEnumerable<AdagioDto>> GetAdagioReportData(TextReportParams textReportParams);
  Task<IEnumerable<IncompleteTransactionsDto>> GetIncompleteTransactionsReportData(TextReportParams textReportParams);
  Task<IEnumerable<EmployeeCommissionDto>> GetEmployeeCommissionReportData(TextReportParams textReportParams);
  Task<IEnumerable<EmployeeCommission>> GetEmployeeCommissionTableData(TextReportParams textReportParams);
  Task<IEnumerable<SalesDto>> GetFinancialReportSalesDeptData(TextReportParams textReportParams);
  Task<IEnumerable<SalesDto>> GetFinancialReportSalesData(TextReportParams textReportParams);
  Task<IEnumerable<TaxesDto>> GetFinancialReportSaleTaxesData(TextReportParams textReportParams);
  Task<IEnumerable<TaxesDto>> GetFinancialReportTaxesData(TextReportParams textReportParams);
  Task<IEnumerable<ChargeDto>> GetFinancialReportSaleChargesData(TextReportParams textReportParams);
  Task<IEnumerable<ChargeDto>> GetFinancialReportChargesData(TextReportParams textReportParams);
  Task<IEnumerable<TendersDto>> GetFinancialReportSaleTendersData(TextReportParams textReportParams);
  Task<IEnumerable<TendersDto>> GetFinancialReportTendersData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportARTendersData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportARPayData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntTendersData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportPayoutData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportOverPayData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportRefundData(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportDelVoidData(TextReportParams textReportParams);
  Task<IEnumerable<CustomerCntDto>> GetFinancialReportCustomerCntData(TextReportParams textReportParams);
  Task<IEnumerable<CustomerSalesDetail>?> GetCustomerSalesDetailReportDataAsync(TextReportParams textReportParams);
}
