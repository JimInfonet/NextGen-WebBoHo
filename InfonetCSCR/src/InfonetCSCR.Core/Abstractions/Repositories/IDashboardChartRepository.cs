namespace InfonetCSCR.Core.Abstractions.Repositories
{
    public interface IDashboardChartRepository
    {
        Task<DashboardData> SalesAndTrans(System.DateTime date);
        DashboardChart ChartModel(System.DateTime date);
    }
}
