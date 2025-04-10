namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITextReportsRepository
{
  Task<IEnumerable<TextReport>> GetAllTextReportsListAsync();
  Task<IEnumerable<CardDetails>> GetCardDetailsReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FleetCardFuel>> GetFleetCardFuelReportDataAsync(TextReportParams textReportParams, string fuelDept);
  Task<DiscountDto?> GetCardDiscountFromCardNameAsync(string cardname);
  Task<IEnumerable<TotalizerReading>?> GetTotalizerReadingReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<CreditCardSummary>?> GetCreditCardSummaryReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<ARCustomerSales>?> GetARCustomerSalesReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<DailyFuelDiscount>> GetDailyFuelDiscountReportDataAsync(TextReportParams textReportParams, string fuelDept);
  Task<IEnumerable<FuelSaleVariance>> GetSaleVarianceReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<EmployeesAndShifts>?> GetEmployeesAndShiftsAsync(TextReportParams textReportParams);
  Task<IEnumerable<ShiftAnalysis>?> GetShiftAnalysisReportDataAsync(IEnumerable<EmployeesAndShifts> employeesAndShifts, TextReportParams textReportParams, string fuelDept);
  Task<IEnumerable<FleetCardSale>?> GetFleetCardSaleReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<AffdaSaleDetail>?> GetAffdaSaleDetailReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<AffdaSaleDetail>?> GetAffdaTaxSaleSummaryReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<TankDip>> GetTankDipReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<AdagioDto>> GetAdagioReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<IncompleteTransactionsDto>> GetIncompleteTransactionsReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<EmployeeCommissionDto>> GetEmployeeCommissionReportDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<EmployeeCommission>> GetEmployeeCommissionTableDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<SalesDto>> GetFinancialReportSalesDeptDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<SalesDto>> GetFinancialReportSalesDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<TaxesDto>> GetFinancialReportSaleTaxesDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<TaxesDto>> GetFinancialReportTaxesDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<ChargeDto>> GetFinancialReportSaleChargesDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<ChargeDto>> GetFinancialReportChargesDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<TendersDto>> GetFinancialReportSaleTendersDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<TendersDto>> GetFinancialReportTendersDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportARTendersDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportARPayDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntTendersDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportPaymntDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportPayoutDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportOverPayDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportRefundDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<FinancialReportDto>> GetFinancialReportDelVoidDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<CustomerCntDto>> GetFinancialReportCustomerCntDataAsync(TextReportParams textReportParams);
  Task<IEnumerable<CustomerSalesDetail>> GetCustomerSalesDetailReportDataAsync(TextReportParams textReportParams);
}
