using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfonetCSCR.Reporting.Service
{
    public class DashboardService
    {
        private DBHelper dbHelper;
        public DashboardService()
        {
            dbHelper = new DBHelper();
        }

        public DashboardData GetSalesAndTransData(DateTime date)
        {
            return dbHelper.GetSalesAndTransData(date);
        }

        public DashboardChart GetChartModelData(DateTime date)
        {
            var chartData = dbHelper.GetChartModelData(date);
            return PrepareChartModelData(chartData);
        }

        private DashboardChart PrepareChartModelData(IEnumerable<Sale> chartData)
        {
            List<SalesChartData> salesChartData = new List<SalesChartData>();
            var deptNames = chartData.Select(s => s.Dept).Distinct();

            foreach (var dept in deptNames)
            {
                SalesChartData deptData = new SalesChartData();
                deptData.Label = dept;
                deptData.Data = new List<decimal>();
                for (int i = 0; i < 24; i++)
                {
                    var hourSale = chartData.Where(s => (s.SaleHour == i && s.Dept.Equals(dept)))
                                        .Select(x => (x.SaleAmt)).FirstOrDefault();
                    deptData.Data.Add(hourSale);
                }
                salesChartData.Add(deptData);
            }

            TopCatChartData topCatChartData = GetTopCategoryData(chartData);
            DashboardChart chartModel = new DashboardChart
            {
                SalesChartDatas = salesChartData,
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
}
