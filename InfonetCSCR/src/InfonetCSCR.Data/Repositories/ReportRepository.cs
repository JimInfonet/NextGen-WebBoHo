using InfonetCSCR.Core.Abstractions;
using System.Collections.Generic;

namespace InfonetCSCR.Data.Repositories
{
    public class ReportRepository : BaseRepository, IReportRepository
    {
        public ReportRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        //public async Task<IEnumerable<ReportFilterModel>> ReportFilter()
        public async Task<ReportFilter> ReportFilter()
        {
            ReportFilter reportFilter = new ReportFilter();
            StringBuilder query;

            query = new StringBuilder($@"SELECT GID, rep.Report_Name as ID, rep.Report_Title as Name FROM [CSCAdmin].[dbo].[REPORTS] rep, [CSCAdmin].[dbo].[Reports_in_Groups] rig 
                            WHERE rep.Report_Name = rig.Report_Name AND rep.Web=1 order by GID, rep.Report_Name");
            reportFilter.Reports = (List<CategoryFilterModel>?)await Connection.QueryAsync<CategoryFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT [Report_Name],[Filter_Seq],[Field],[Type],[Field_Type] FROM CSCAdmin.dbo.[Report_Filter] ORDER BY Filter_Seq");
            reportFilter.Filters = (List<ReportFilterModel>?)await Connection.QueryAsync<ReportFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT D.Dept as Id, Dept_Name as Name, FD.Dept FuelDept FROM CSCMaster.dbo.DEPT D LEFT JOIN CSCMaster.dbo.[FUELDEPT] FD ON D.DEPT = FD.DEPT ORDER BY D.Dept");
            reportFilter.DeptWithFuel = (List<DeptFilterModel>?)await Connection.QueryAsync<DeptFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT D.Dept as Id, Dept_Name as Name, FD.Dept FuelDept FROM CSCMaster.dbo.DEPT D LEFT JOIN CSCMaster.dbo.[FUELDEPT] FD ON D.DEPT = FD.DEPT WHERE FD.Dept is null ORDER BY D.Dept");
            reportFilter.Dept = (List<DeptFilterModel>?)await Connection.QueryAsync<DeptFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT Sub_Dept as Id, Sub_Name as Name, Dept as DeptId FROM CSCMaster.dbo.SubDept ORDER BY Sub_Name");
            reportFilter.SubDept = (List<StockMovementFilterModel>?)await Connection.QueryAsync<StockMovementFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT SubDetail as Id, SubDetail_Name as Name, Dept as DeptId FROM CSCMaster.dbo.SubDetail ORDER BY SubDetail_Name");
            reportFilter.SubDetail = (List<StockMovementFilterModel>?)await Connection.QueryAsync<StockMovementFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT Stock_Code as Id, Descript as Name, Dept as DeptId FROM CSCMaster.dbo.StockMST ORDER BY Descript");
            reportFilter.StockCode = (List<StockMovementFilterModel>?)await Connection.QueryAsync<StockMovementFilterModel>(query.ToString(), null, Transaction);

            query = new StringBuilder($@"SELECT Code as Id, Name FROM CSCMaster.dbo.Vendors ORDER BY Name");
            reportFilter.Vendor = (List<FilterModel>?)await Connection.QueryAsync<FilterModel>(query.ToString(), null, Transaction);

            return reportFilter;
        }

    }
}
