namespace InfonetCSCR.Data.Repositories;
public class DashboardChartRepository : BaseRepository, IDashboardChartRepository
{
    public DashboardChartRepository(IDbTransaction transaction) : base(transaction) { }

    public async Task<DashboardData> SalesAndTrans(DateTime date)
    {
        DashboardData salesAndTrans = await Connection.QuerySingleOrDefaultAsync<DashboardData>
            ("Sproc_TotalSalesAndTransactions", new { SALEDATE = date }, Transaction, commandType: CommandType.StoredProcedure);

        return salesAndTrans;
    }

    private async Task<IEnumerable<Sale>> GetChartModelData(DateTime date)
    {
        IEnumerable<Sale> chartModeldata = await Connection.QueryAsync<Sale>
            ("Sproc_DashboardChartData", new { SALEDATE = date }, Transaction, commandType: CommandType.StoredProcedure);

        return chartModeldata;
    }

    public DashboardChart ChartModel(DateTime date)
    {
        IEnumerable<Sale> chartData = GetChartModelData(date).Result;
        DashboardChart chartModel = PrepareChartModelData(chartData);

        return chartModel;
    }

    private DashboardChart PrepareChartModelData(IEnumerable<Sale> chartData)
    {
        List<SalesChartData>  saleChartData = new List<SalesChartData>();
        var deptNames = chartData.Select(s => s.Dept).Distinct();

        foreach (var dept in deptNames)
        {
            SalesChartData deptData = new SalesChartData();
            deptData.Label = dept;
            deptData.Data = new List<decimal>();
            for (int i = 0; i < 24; i++)
            {
                var hourSale = chartData.Where(s => s.SaleHour == i && s.Dept.Equals(dept))
                                    .Select(x => (x.SaleAmt)).FirstOrDefault();
                deptData.Data.Add(hourSale);
            }
            saleChartData.Add(deptData);
        }

        TopCatChartData topCatChartData = GetTopCategoryData(chartData);
        DashboardChart chartModel = new DashboardChart
        {
            SalesChartDatas = saleChartData,
            TopCatChartData = topCatChartData
        };

        return chartModel;
    }

    private TopCatChartData GetTopCategoryData(IEnumerable<Sale> data)
    {
        var groupScores = from sale in data
                          group sale by sale.Dept into deptGroup
                          select new
                          {
                              Dept = deptGroup.Key,
                              DeptSale = deptGroup.Sum(x => x.SaleAmt),
                          };

        var top3Cat = groupScores.OrderByDescending(x => x.DeptSale).Take(3).ToList();

        TopCatChartData chartData = new TopCatChartData();
        chartData.Labels = top3Cat.Select(x => x.Dept).ToList();
        chartData.Data = top3Cat.Select(x => x.DeptSale).ToList();

        return chartData;
    }

}
