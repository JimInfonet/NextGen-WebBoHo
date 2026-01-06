namespace InfonetCSCR.Core.Abstractions.Services
{
    public interface IDashboardChartService
    {
        Task<DashboardData> GetSalesAndTransData(DateTime date);
        DashboardChart GetChartModelData(DateTime date);
    }
}
