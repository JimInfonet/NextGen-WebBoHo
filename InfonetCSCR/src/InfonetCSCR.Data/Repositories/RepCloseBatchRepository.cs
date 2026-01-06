using InfonetCSCR.Core.Models;
using System.ComponentModel;
using System.Reflection;

namespace InfonetCSCR.Data.Repositories
{
    public class RepCloseBatchRepository : BaseRepository, IRepCloseBatchRepository
    {
        Till_Close TC = new();
        public RepCloseBatchRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
        {
        }
        public async Task<IEnumerable<RepCloseBatch>> GetRepCloseBatches(DateTime date1, DateTime date2)
        {
            string query;

            //,CloseBatch.Report
            //,CONVERT(NVARCHAR, CloseBatch.BatchDate, 111) As BatchDate
            //, CONVERT(NVARCHAR, CloseBatch.BatchTime, 8) As BatchTime
                            //,CloseBatch.BatchDate As BatchDate
                            //,CloseBatch.BatchTime As BatchTime
            query = @"Select CloseBatch.ID 
                            ,CONVERT(NVARCHAR, CloseBatch.BatchDate, 111) As BatchDate
                            ,CONVERT(NVARCHAR, CloseBatch.BatchTime, 8) As BatchTime
                            ,CloseBatch.TerminalID
                            ,CloseBatch.BatchNumber
                        FROM [CSCTrans].[dbo].[CloseBatch] 
                        WHERE CloseBatch.BatchDate between @Date1 and @Date2
                        order by BatchDate desc, BatchTime desc";
            var parameters = new { Date1 = date1, Date2 = date2 };
            var repCloseBatches= await Connection.QueryAsync<RepCloseBatch>(query, parameters, Transaction);

            return repCloseBatches;
        }
        public async Task<string?> GetReportById(string id)
        {
            string query;
            query = @"Select CloseBatch.Report
                        FROM [CSCTrans].[dbo].[CloseBatch] 
                        WHERE CloseBatch.ID = @id";
            var parameters = new { id = id };
            var report = await Connection.QuerySingleOrDefaultAsync<string?>(query, parameters, Transaction);

            return report;
        }

        public void showPolicy(Policies pol)
        {
            Console.WriteLine($"BaseCurr:{pol.BaseCurr}");
            Console.WriteLine($"CBonusName:{pol.CBonusName}");
            Console.WriteLine($"CBonusTend:{pol.CBonusTend}");
            Console.WriteLine($"CashBonus:{pol.CashBonus}");
            Console.WriteLine($"CcMode:{pol.CcMode}");
            Console.WriteLine($"CombineCr:{pol.CombineCr}");
            Console.WriteLine($"CombineFleet:{pol.CombineFleet}");
            Console.WriteLine($"CountCash:{pol.CountCash}");
            Console.WriteLine($"CountType:{pol.CountType}");
            Console.WriteLine($"CloseB_Days:{pol.CloseBDays}");
            Console.WriteLine($"CloseIncluded:{pol.CloseIncluded}");
            Console.WriteLine($"EodGroup:{pol.EodGroup}");
            Console.WriteLine($"PrtTillClCol:{pol.PrtTillClCol}");
            Console.WriteLine($"ShowAllExempt:{pol.ShowAllExempt}");
            Console.WriteLine($"SupportEzi:{pol.SupportEzi}");
            Console.WriteLine($"Show_NRGT:{pol.Show_NRGT}");
            Console.WriteLine($"ShowCodes:{pol.ShowCodes}");
            Console.WriteLine($"TaxRebate:{pol.TaxRebate}");
            Console.WriteLine($"TaxExempt:{pol.TaxExempt}");
            Console.WriteLine($"Te_Type:{pol.Te_Type}");
            Console.WriteLine($"TidelSafe:{pol.TidelSafe}");
            Console.WriteLine($"TillCDays:{pol.TillCDays}");
            Console.WriteLine($"UseOvrPymnt:{pol.UseOvrPymnt}");
            Console.WriteLine($"UseShifts:{pol.UseShifts}");

        }
        public async Task<DataTable> GenerateReportData(string SaleNo, string BatchNumber, string TerminalID)
        {
            string query;

            query = $"Select * " +
                        $"From [CSCTrans].[dbo].[Cardtenders] " +
                        $"WHERE Sale_No =  {SaleNo} " +
                        $"AND BatchNumber = {BatchNumber} " +
                        $"AND TerminalId = {TerminalID} " +
                        $"Order by Cardtenders.Card_Name";

            return (DataTable)await Connection.QueryAsync(query, null, Transaction);
        }
    }
}
