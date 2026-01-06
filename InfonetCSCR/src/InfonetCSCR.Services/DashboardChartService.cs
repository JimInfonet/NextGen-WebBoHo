namespace InfonetCSCR.Services
{
    public class DashboardChartService : IDashboardChartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationConfig _config;

        public DashboardChartService(IApplicationConfig config)
        {
            _config = config;
            this._unitOfWork = new UnitOfWork(_config, _config.CSCTillsConnectionString);
        }

        async Task<DashboardData> IDashboardChartService.GetSalesAndTransData(DateTime date)
        {
            DashboardData dashboardData = await _unitOfWork.DashboardCharts.SalesAndTrans(date);
            _unitOfWork.Commit();

            return dashboardData;
        }
        DashboardChart IDashboardChartService.GetChartModelData(DateTime date)
        {
            DashboardChart dashboardChart = _unitOfWork.DashboardCharts.ChartModel(date);
            _unitOfWork.Commit();

            return dashboardChart;
        }

    }
}

