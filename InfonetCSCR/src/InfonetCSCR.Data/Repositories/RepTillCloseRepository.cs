using InfonetCSCR.Core.Dtos;
using InfonetCSCR.Core.Models;
using System.ComponentModel;
using System.Reflection;

namespace InfonetCSCR.Data.Repositories
{
    public class RepTillCloseRepository : BaseRepository, IRepTillCloseRepository
    {
        private readonly IApplicationConfig _config;
        private IDbTransaction _transaction;
        long CloseNo;
        //int ShiftNo;

        bool loading;
        decimal Float_Amt;
        decimal Drop_Amt;
        decimal Drop_Conv;
        decimal Draw_Amt;
        decimal BonusFloat;
        decimal BonusDraw;
        decimal BonusDrop;
        decimal BonusGiveAway;
        decimal SC_Issued;
        decimal Payments;
        decimal ARPay;
        decimal Payouts;
        decimal Credits_Issued;
        decimal OverPay;
        decimal ChangePaid;
        decimal Penny_Adj;
        decimal Total_Tenders;
        string? StoreID = "031";
        bool FirstTime;
        Till_Close TC = new();
        const int PRINT_WIDTH = 40; // Update the width as needed
        bool polShow_All_Exempt;
        public RepTillCloseRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
        {
            _config = config;
            _transaction = transaction;
        }
        //public async Task<IEnumerable<RepTillClose>> GetRepTillCloses(DateOnly date1)
        public async Task<IEnumerable<RepTillClose>> GetRepTillCloses(DateTime date1, DateTime date2)
        {
            string query;

            query = @"Select Close_Num
                            ,CONVERT(NVARCHAR, Open_Date, 111) As Open_Date
                            ,CONVERT(NVARCHAR, Open_Time, 8) As Open_Time
                            ,CONVERT(NVARCHAR, Close_Date, 111) As Close_Date
                            ,CONVERT(NVARCHAR, Close_Time, 8) As Close_Time
                            ,Till_Num
                            ,CONVERT(NVARCHAR, ShiftDate, 111) As ShiftDate
                            ,ShiftNumber                         
                        FROM [CSCTrans].[dbo].[Close_Head] 
                        WHERE Close_Date between @Date1 and @Date2
                        order by Close_Num desc";
            var parameters = new { Date1 = date1, Date2 = date2 };
            var repTillCloses = await Connection.QueryAsync<RepTillClose>(query, parameters, Transaction);

            return repTillCloses;
        }
        //public async Task<IEnumerable<RepTillClose>> GetRepTillCloses()
        //{
        //    string query;
        //    DateTime date2 = DateTime.Now;
        //    DateTime date1 = date2.AddDays(-5);

        //    query = @"Select Close_Num, Open_Date, Open_Time, Close_Date, 
        //                      Close_Time, Till_Num, ShiftDate, ShiftNumber                         
        //                FROM [CSCTrans].[dbo].[Close_Head] 
        //                WHERE Close_Date between @Date1 and @Date2
        //                order by Close_Num desc";
        //    var parameters = new { Date1 = date1, Date2 = date2 };
        //    var repTillCloses = await Connection.QueryAsync<RepTillClose>(query, parameters, Transaction);

        //    return repTillCloses;
        //}
        public void showPolicy(GetRepTillCloseDto model)
        {
            Console.WriteLine($"BaseCurr:{model.policies.BaseCurr}");
            Console.WriteLine($"CBonusName:{model.policies.CBonusName}");
            Console.WriteLine($"CBonusTend:{model.policies.CBonusTend}");
            Console.WriteLine($"CashBonus:{model.policies.CashBonus}");
            Console.WriteLine($"CcMode:{model.policies.CcMode}");
            Console.WriteLine($"CombineCr:{model.policies.CombineCr}");
            Console.WriteLine($"CombineFleet:{model.policies.CombineFleet}");
            Console.WriteLine($"CountCash:{model.policies.CountCash}");
            Console.WriteLine($"CountType:{model.policies.CountType}");
            Console.WriteLine($"CloseIncluded:{model.policies.CloseIncluded}");
            Console.WriteLine($"EodGroup:{model.policies.EodGroup}");
            Console.WriteLine($"PrtTillClCol:{model.policies.PrtTillClCol}");
            Console.WriteLine($"ShowAllExempt:{model.policies.ShowAllExempt}");
            Console.WriteLine($"SupportEzi:{model.policies.SupportEzi}");
            Console.WriteLine($"Show_NRGT:{model.policies.Show_NRGT}");
            Console.WriteLine($"ShowCodes:{model.policies.ShowCodes}");
            Console.WriteLine($"TaxRebate:{model.policies.TaxRebate}");
            Console.WriteLine($"TaxExempt:{model.policies.TaxExempt}");
            Console.WriteLine($"Te_Type:{model.policies.Te_Type}");
            Console.WriteLine($"TidelSafe:{model.policies.TidelSafe}");
            Console.WriteLine($"TillCDays:{model.policies.TillCDays}");
            Console.WriteLine($"UseOvrPymnt:{model.policies.UseOvrPymnt}");
            Console.WriteLine($"UseShifts:{model.policies.UseShifts}");

        }
        public async Task<string[]> GetRepByShift(GetRepTillCloseDto model)
        {
            //showPolicy(model);

            string filePath = "TillClose.txt";
            CloseNo = model.closeNo;
            Boolean bFound = false;
            int shift;
            Boolean PrintIt = false;
            String Where_Clause;

            if (model.shiftNo.ToUpper() == "ALL")
            {
                shift = 1;
                TC.AllShifts = true;
            }
            else
            {
                shift = int.Parse(model.shiftNo);
                TC.AllShifts = false;
            }

            Where_Clause = Collect_Close_Data(model, shift, TC.AllShifts, StoreID).Result;

            int n = 0;

            // Load the Entered amounts into the recordset.
            var tenders = (IEnumerable<Tender>)await GetTenders();
            var data = await GetTillClose();
            List<Close_Line> cLines = new();
            foreach (var tc in data)
            {
                Close_Line cl = new();
                n = n + 1;
                cl.Sequence = n;
                if (tc.Tender == "Other Tenders")
                {
                    cl.Short_Name = "Other";
                    cl.Tender_Class = "OTHER";
                    cl.Exchange_Rate = 1;
                }
                else
                {
                    if ((model.policies.CombineCr && tc.Tender == "CRCARD" && model.policies.CcMode != "VALIDATE") ||
                    (model.policies.CombineFleet && tc.Tender == "FLEET" && model.policies.CcMode != "VALIDATE"))
                    {
                        cl.Short_Name = "Other";
                        cl.Tender_Class = "OTHER";
                        cl.Exchange_Rate = 1;
                    }
                    else
                    {
                        if (tc.Tender == "Yukon Tire Fleet")
                        {
                            cl.Short_Name = "YT";
                            cl.Tender_Class = "FLEET";
                            cl.Exchange_Rate = 1;
                        }
                        else
                        {
                            foreach (var td in tenders)
                            {
                                if (tc.Tender == td.FullName)
                                {
                                    cl.Short_Name = td.ShortName;
                                    cl.Tender_Class = td.Class;
                                    cl.Exchange_Rate = (decimal)td.ExchangeRate;
                                    break;
                                }
                            }
                        }
                    }
                }
                cl.Tender_Name = tc.Tender;
                cl.Tender_Count = (int?)tc.Count;
                cl.Entered = (decimal)tc.Entered;
                cl.System = (decimal)tc.System;
                cl.Balance = (decimal)tc.Difference;

                cl.Converted_Balance = (decimal)(tc.System * cl.Exchange_Rate);
                cl.Converted_Entered = (decimal)(tc.Entered * cl.Exchange_Rate);

                //CL.Tender_Count = IIf(IsNull(![Count]), 0, ![Count])
                //CL.Entered = IIf(IsNull(![Entered]), 0, ![Entered])
                //CL.System = ![System]
                //CL.Balance = ![Difference]

                //CL.Converted_System = ![System] * CL.Exchange_Rate
                //CL.Converted_Entered = IIf(IsNull(![Entered]), 0, ![Entered]) * CL.Exchange_Rate
                //CL.Converted_Balance = ![Difference] * CL.Exchange_Rate
                //Me.Add CL
                //.MoveNext


                // Add the new Close_Line object to the list
                cLines.Add(cl);
            } // end of for
            TC.CloseLines = cLines;
            TC.Till_Number = 0;
            TC.ShiftNumber = shift;
            TC.Close_Num = 0;

            TC.Float = 0;
            TC.Draw = Draw_Amt;
            TC.Drop = Drop_Conv - BonusDrop;
            TC.Payments = Payments;
            TC.ARPay = ARPay;
            TC.PayOuts = Payouts;
            TC.OverPay = OverPay;
            TC.Penny_Adj = Penny_Adj;
            TC.BonusFloat = BonusFloat;
            TC.BonusDraw = BonusDraw;
            TC.BonusDrop = BonusDrop;
            TC.BonusGiveAway = BonusGiveAway;

            TC.Credits_Issued = SC_Issued;

            //tc.Close_Till False, Where_Clause, StoreID
            if (Shift_Report_Main(model, PrintIt, Where_Clause, StoreID).Result)
            {
                //Console.WriteLine("File created successfully.");
                // Read the file as binary data
                string[] fileLines = File.ReadAllLines(filePath);

                return fileLines;
            }
            else
            {
                //Console.WriteLine("Error in creating file.");
                return null;
            }
        }
        public async Task<string[]> GetRepByDates(GetRepTillCloseDto model)
        {
            //showPolicy(model);

            string filePath = "TillClose.txt";
            CloseNo = model.closeNo;
            Boolean bFound = false;
            int shift =0;
            Boolean PrintIt = false;
            DateOnly sDate = DateOnly.FromDateTime(model.startDate);
            DateOnly eDate = DateOnly.FromDateTime(model.endDate);
            TimeOnly sTime = TimeOnly.Parse(model.startTime);
            TimeOnly eTime = TimeOnly.Parse(model.endTime);
            DateTime sDateTime = sDate.ToDateTime(sTime);
            DateTime eDateTime = eDate.ToDateTime(eTime);
            String Where_Clause;

            //if (model.shiftNo.ToUpper() == "ALL")
            //{
            //    shift = 1;
            //    TC.AllShifts = true;
            //}
            //else
            //{
            //    shift = int.Parse(model.shiftNo);
            //    TC.AllShifts = false;
            //}

            Where_Clause = Collect_Close_Data(model, shift, TC.AllShifts, StoreID).Result;

            int n = 0;

            // Load the Entered amounts into the recordset.
            var tenders = (IEnumerable<Tender>)await GetTenders();
            var data = await GetTillClose();
            List<Close_Line> cLines = new();
            foreach (var tc in data)
            {
                Close_Line cl = new();
                n = n + 1;
                cl.Sequence = n;
                if (tc.Tender == "Other Tenders")
                {
                    cl.Short_Name = "Other";
                    cl.Tender_Class = "OTHER";
                    cl.Exchange_Rate = 1;
                }
                else
                {
                    if ((model.policies.CombineCr && tc.Tender == "CRCARD" && model.policies.CcMode != "VALIDATE") ||
                    (model.policies.CombineFleet && tc.Tender == "FLEET" && model.policies.CcMode != "VALIDATE"))
                    {
                        cl.Short_Name = "Other";
                        cl.Tender_Class = "OTHER";
                        cl.Exchange_Rate = 1;
                    }
                    else
                    {
                        if (tc.Tender == "Yukon Tire Fleet")
                        {
                            cl.Short_Name = "YT";
                            cl.Tender_Class = "FLEET";
                            cl.Exchange_Rate = 1;
                        }
                        else
                        {
                            foreach (var td in tenders)
                            {
                                if (tc.Tender == td.FullName)
                                {
                                    cl.Short_Name = td.ShortName;
                                    cl.Tender_Class = td.Class;
                                    cl.Exchange_Rate = (decimal)td.ExchangeRate;
                                    break;
                                }
                            }
                        }
                    }
                }
                cl.Tender_Name = tc.Tender;
                cl.Tender_Count = (int?)tc.Count;
                cl.Entered = (decimal)tc.Entered;
                cl.System = (decimal)tc.System;
                cl.Balance = (decimal)tc.Difference;

                cl.Converted_Balance = (decimal)(tc.System * cl.Exchange_Rate);
                cl.Converted_Entered = (decimal)(tc.Entered * cl.Exchange_Rate);

                //CL.Tender_Count = IIf(IsNull(![Count]), 0, ![Count])
                //CL.Entered = IIf(IsNull(![Entered]), 0, ![Entered])
                //CL.System = ![System]
                //CL.Balance = ![Difference]

                //CL.Converted_System = ![System] * CL.Exchange_Rate
                //CL.Converted_Entered = IIf(IsNull(![Entered]), 0, ![Entered]) * CL.Exchange_Rate
                //CL.Converted_Balance = ![Difference] * CL.Exchange_Rate
                //Me.Add CL
                //.MoveNext


                // Add the new Close_Line object to the list
                cLines.Add(cl);
            } // end of for
            TC.CloseLines = cLines;
            TC.Till_Number = 0;
            TC.ShiftNumber = shift;
            TC.Close_Num = 0;

            TC.Float = 0;
            TC.Draw = Draw_Amt;
            TC.Drop = Drop_Conv - BonusDrop;
            TC.Payments = Payments;
            TC.ARPay = ARPay;
            TC.PayOuts = Payouts;
            TC.OverPay = OverPay;
            TC.Penny_Adj = Penny_Adj;
            TC.BonusFloat = BonusFloat;
            TC.BonusDraw = BonusDraw;
            TC.BonusDrop = BonusDrop;
            TC.BonusGiveAway = BonusGiveAway;

            TC.Credits_Issued = SC_Issued;

            //tc.Close_Till False, Where_Clause, StoreID
            if (Date_Report_Main(model, PrintIt, Where_Clause, StoreID).Result)
            {
                //Console.WriteLine("File created successfully.");
                // Read the file as binary data
                string[] fileLines = File.ReadAllLines(filePath);

                return fileLines;
            }
            else
            {
                //Console.WriteLine("Error in creating file.");
                return null;
            }
        }
        public async Task<bool> Shift_Report_Main(GetRepTillCloseDto model, bool PrintIt, string Where_Clause, string StoreID)
        {
            bool bFinished;
            decimal T_Ent;
            decimal T_Sys;
            decimal T_Dif;
            decimal Exc_Adj_E;
            decimal Exc_Adj_S;
            decimal Exc_Adj_D;
            string query, query2, query3;
            int n, nRecordCount = 0;

            const int PRINT_WIDTH = 40; // Update the width as needed
            string filePath = "TillClose.txt";

            // Delete the file if it already exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Create the file and write content to it
            using StreamWriter writer = File.CreateText(filePath);
            if (!_config.IsBackOffice)
                writer.WriteLine(PadC("Store:" + StoreID, PRINT_WIDTH));
            else
                writer.WriteLine(PadC("Store:" + StoreID, PRINT_WIDTH));

            writer.WriteLine(PadC("TILL REPORT BY DATE RANGE", PRINT_WIDTH));
            if (TC.AllShifts)
                writer.WriteLine(PadC("All Shifts On " + model.shiftDate.ToString("MM-dd-yyyy"), PRINT_WIDTH));
            else
                writer.WriteLine(PadC("Shift #" + TC.ShiftNumber + " On " + model.shiftDate.ToString("MM-dd-yyyy"), PRINT_WIDTH));

            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine(PadC("Printed On " + DateTime.Now.ToString("MM-dd-yyyy") + " " + "at " + DateTime.Now.ToString("hh:mm:sstt"), PRINT_WIDTH));

            writer.WriteLine(new string(' ', PRINT_WIDTH));

            if (model.policies.Show_NRGT)
                await printNRGT(writer, model.startDate, model.endDate, StoreID);

            T_Sys = 0;
            Exc_Adj_S = 0;
            n = 0;

            writer.WriteLine(PadR("Tender", PRINT_WIDTH / 2) + PadL("System", PRINT_WIDTH / 2));
            writer.WriteLine(new string('_', PRINT_WIDTH));

            //Search the tender of the close_tenders out of TillCloses
            foreach (var cl in TC.CloseLines)
            {
                writer.WriteLine(PadR(cl.Short_Name, PRINT_WIDTH / 2) + PadL(cl.System.ToString("##,###,##0.00"), PRINT_WIDTH / 2));
                T_Sys += cl.System;
                Exc_Adj_S = Exc_Adj_S - cl.System * (1 - cl.Exchange_Rate);
            }
            writer.WriteLine(new string('_', PRINT_WIDTH));
            writer.WriteLine(PadR("TOTALS", PRINT_WIDTH / 2) + PadL(T_Sys.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Exc_Adj_S != 0)
                writer.WriteLine(PadR("+Exch Adj", PRINT_WIDTH / 2) + PadL(Exc_Adj_S.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Float_Amt != 0)
                writer.WriteLine(PadR("-Float", PRINT_WIDTH / 2) + PadL((-1 * Float_Amt).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Credits_Issued != 0)
                writer.WriteLine(PadR("-Credits", PRINT_WIDTH / 2) + PadL(Credits_Issued.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Payments != 0)
                writer.WriteLine(PadR("-Payments", PRINT_WIDTH / 2) + PadL((-1 * Payments).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (ARPay != 0)
                writer.WriteLine(PadR("-AR Pay", PRINT_WIDTH / 2) + PadL((-1 * ARPay).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (OverPay != 0)
                writer.WriteLine(PadR("-OverPay", PRINT_WIDTH / 2) + PadL((-1 * OverPay).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Draw_Amt != 0)
                writer.WriteLine(PadR("-Draws", PRINT_WIDTH / 2) + PadL((-1 * Draw_Amt).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Drop_Amt != 0)
                writer.WriteLine(PadR("+Drops", PRINT_WIDTH / 2) + PadL(Drop_Amt.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Payouts != 0)
                writer.WriteLine(PadR("+Payouts", PRINT_WIDTH / 2) + PadL(Payouts.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Penny_Adj != 0)
                writer.WriteLine(PadR("+/-Penny Adjustment", PRINT_WIDTH / 2) + PadL((-1 * Penny_Adj).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            T_Sys += Exc_Adj_S + Credits_Issued - Float_Amt - Draw_Amt + Drop_Amt - Payments - ARPay + Payouts - OverPay - Penny_Adj;

            // Totals after all adjustments
            if (Exc_Adj_S != 0 || Credits_Issued != 0 || Float_Amt != 0 || Draw_Amt != 0
                || Drop_Amt != 0 || OverPay != 0 || Penny_Adj != 0 || Payments != 0 || ARPay != 0
                || Payouts != 0)
            {
                writer.WriteLine(PadL("_", PRINT_WIDTH));
                writer.WriteLine(PadR("TOTALS", PRINT_WIDTH / 2), PadL(T_Sys.ToString("##,###,##0.00"), PRINT_WIDTH / 2));
            }

            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine("Transaction Summary");
            writer.WriteLine(new string('_', PRINT_WIDTH));

            if (Where_Clause.Length == 0)
            {
                query = $"SELECT SaleHead.T_Type as [TType], " +
                    $"  Count(*) as [Transactions], " +
                    $"  Sum(SALE_AMT) As [Amount], " +
                    $"  Sum(PAYMENT) as [Pays] " +
                    $"From CSCTrans.dbo.SaleHead " +
                    $"Where  SaleHead.T_Type <> 'CANCEL' and SaleHead.Till={model.tillNo} " +
                    $"Group By SaleHead.T_Type ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT SaleHead.T_Type as [TType], " +
                            $"  Count(*) as [Transactions], " +
                            $"  Sum(SALE_AMT) As [Amount], " +
                            $"  Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                            $"Group By SaleHead.T_Type ";
                    }
                    else
                    {
                        query = $"SELECT SaleHead.T_Type as [TType], " +
                            $"  Count(*) as [Transactions], " +
                            $"  Sum(SALE_AMT) As [Amount], " +
                            $"  Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                            " AND SaleHead.Store_id = {StoreID} " +
                            $"Group By SaleHead.T_Type ";
                    }
                }
                else
                {
                    query = $"SELECT SaleHead.T_Type as [TType], " +
                        $"  Count(*) as [Transactions], " +
                        $"  Sum(SALE_AMT) As [Amount], " +
                        $"  Sum(PAYMENT) as [Pays] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                        $"Group By SaleHead.T_Type ";
                }
            }
            var list = await Connection.QueryAsync<TendData>(query, null, Transaction);

            foreach (var item in list)
            {
                switch (item.TType)
                {
                    case "PATP_APP":
                        writer.WriteLine(PadR("P@P Sale", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "PATP_C":
                        writer.WriteLine(PadR("P@P Void", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "REFUND":
                        writer.WriteLine(PadR("Refund", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "SALE":
                        writer.WriteLine(PadR("Sale", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "VOID":
                        writer.WriteLine(PadR("Void", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "ARPAY":
                        writer.WriteLine(PadR("AR Pay", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "PAYOUT":
                        writer.WriteLine(PadR("Payout", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "BTL RTN":
                        writer.WriteLine(PadR("Bottle Return", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    default:
                        writer.WriteLine(PadR(item.TType!, 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                }
            }
            writer.WriteLine(new string(' ', 20) + new string('_', 8));

            if (Where_Clause.Length == 0)
            {
                query = $"Select Count(*) as [Transactions] " +
                    $"From CSCTrans.dbo.SaleHead " +
                    $"Where  SaleHead.T_Type <> 'CANCEL' and SaleHead.Till={model.tillNo} ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"Select Count(*) as [Transactions] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} ";
                    }
                    else
                    {
                        query = $"Select Count(*) as [Transactions] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                                " AND SaleHead.Store_id = {StoreID} ";
                    }
                }
                else
                {
                    query = $"Select Count(*) as [Transactions] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} ";
                }
            }
            var TransNo = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
            writer.WriteLine(PadR("Total Transactions", 20) + PadL(TransNo.ToString(), 8));

            if (Where_Clause.Length == 0)
            {
                query = $"SELECT distinct Sale_No " +
                    $"From CSCTrans.dbo.SaleHead " +
                    $"WHERE Sale_No in " +
                    $"(SELECT distinct Sale_No " +
                    $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                    $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                    $"                 From CSCTrans.dbo.SaleHead " +
                    $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                    $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                    $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL')))";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT distinct Sale_No  " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"WHERE Sale_No in " +
                            $"(SELECT distinct Sale_No " +
                            $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                            $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                 From CSCTrans.dbo.SaleHead " +
                            $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL') AND {Where_Clause}))";
                    }
                    else
                    {
                        query = $"SELECT distinct Sale_No  " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"WHERE Sale_No in " +
                            $"(SELECT distinct Sale_No " +
                            $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                            $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                 From CSCTrans.dbo.SaleHead " +
                            $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL') AND {Where_Clause})) " +
                            $"                     AND SaleHead.Store_id = {StoreID}) " +
                            $" AND SaleLine.Store_id = {StoreID}) " +
                            $"AND SaleHead.Store_id = {StoreID} ";

                    }
                }
                else
                {
                    query = $"SELECT distinct Sale_No  " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"WHERE Sale_No in " +
                        $"(SELECT distinct Sale_No " +
                        $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                        $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                        $"                 From CSCTrans.dbo.SaleHead " +
                        $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                        $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                        $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL') AND {Where_Clause}))";

                }
            }
            var sale = await Connection.QueryAsync<string>(query, null, Transaction);

            if (sale != null && sale.Any())
            {
                foreach (var sa in sale)
                {
                    nRecordCount++;
                }

                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine("Fuel Transaction Summary");
                writer.WriteLine(new string('_', PRINT_WIDTH));

                if (Where_Clause.Length == 0)
                {
                    query = $"Select SaleHead.T_Type as [Till], " +
                        $"       Count(*) as [Transactions], " +
                        $"       Sum(SALE_AMT) As [Amount], " +
                        $"       Sum(PAYMENT) as [Pays] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where  SaleHead.T_Type <> 'CANCEL' and SaleHead.Till = {model.tillNo} " +
                        $"Group By SaleHead.T_Type ";
                }
                else
                {
                    if (_config.IsBackOffice)
                    {
                        //Pay at the pump Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Till=100 AND SaleHead.T_Type <> 'CANCEL'  AND {Where_Clause}  " +
                            $"Group By SaleHead.Till ";

                        var fuelSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (fuelSale != null)
                            writer.WriteLine(PadR("Pay at the pump Sales", 21) + PadL(fuelSale.Transactions!.Value.ToString("##0"), 6) + PadL(fuelSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Pay at the pump Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Post Pay Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                            $"                           FROM CSCTrans.dbo.SaleLine " +
                            $"                           WHERE GradeID <> 0 AND Prepay = 0 AND ManualFuel = 0 " +
                            $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                                              From CSCTrans.dbo.SaleHead " +
                            $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                            $"Group By SaleHead.Till ";

                        var fSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (fSale != null)
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL(fSale.Transactions!.Value.ToString("##0"), 6) + PadL(fSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Prepay Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                            $"                           FROM CSCTrans.dbo.SaleLine " +
                            $"                           WHERE GradeID <> 0 AND Prepay = 1 " +
                            $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                                              From CSCTrans.dbo.SaleHead " +
                            $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                            $"Group By SaleHead.Till ";

                        var pSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (pSale != null)
                            writer.WriteLine(PadR("Prepay Sales", 21) + PadL(pSale.Transactions!.Value.ToString("##0"), 6) + PadL(pSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Prepay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Manual Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                            $"                           FROM CSCTrans.dbo.SaleLine " +
                            $"                           WHERE GradeID <> 0 AND ManualFuel = 1 " +
                            $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                                              From CSCTrans.dbo.SaleHead " +
                            $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                            $"Group By SaleHead.Till ";

                        var mSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (mSale != null)
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL(mSale.Transactions!.Value.ToString("##0"), 6) + PadL(mSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                    }
                    else //Headoffice
                    {
                        //Post Pay Sales
                        if (StoreID == "ALL")
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 0 AND ManualFuel = 0 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                                $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                                $"Group By SaleHead.Till ";
                        }
                        else
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 0 AND ManualFuel = 0 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                                $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause}) " +
                                $"                                                  AND SaleLine.Store_id = {StoreID}) " +
                                $"  AND SaleHead.Store_id = {StoreID} " +
                                $"Group By SaleHead.Till ";
                        }
                        var fSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (fSale != null)
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL(fSale.Transactions!.Value.ToString("##0"), 6) + PadL(fSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);
                        //Prepay Sales
                        if (StoreID == "ALL")
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                                $"Group By SaleHead.Till ";
                        }
                        else
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause}) " +
                                $"                                                  AND SaleLine.Store_id = {StoreID}) " +
                                $"  AND SaleHead.Store_id = {StoreID} " +
                                $"Group By SaleHead.Till ";
                        }
                        var pSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (pSale != null)
                            writer.WriteLine(PadR("PrePay Sales", 21) + PadL(pSale.Transactions!.Value.ToString("##0"), 6) + PadL(pSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("PrePay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Manual Sales
                        if (StoreID == "ALL")
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND ManualFuel = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') " +
                                $"                                                  AND {Where_Clause})) " +
                                $"Group By SaleHead.Till ";
                        }
                        else
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND ManualFuel = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') " +
                                $"                                                  AND {Where_Clause}) " +
                                $"                                                  AND SaleLine.Store_id = {StoreID}) " +
                                $"  AND SaleHead.Store_id = {StoreID} " +
                                $"Group By SaleHead.Till ";
                        }
                        var mSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (mSale != null)
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL(mSale.Transactions!.Value.ToString("##0"), 6) + PadL(mSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);
                    } // end of headoffice
                }
                writer.WriteLine(new string(' ', 20) + new string('_', 8));
                writer.WriteLine(PadR("Total Fuel Transactions", 23) + PadL(nRecordCount.ToString(), 7));
            }
            //Summarize Sales Voided and Individual Lines Deleted.
            if (!Product_Sales_Report(writer, model, PrintIt, Where_Clause, StoreID).Result)
            {
                //Console.WriteLine("Error in creating file.");
                return false;
            }
            //end of using filestream
            return true;
        }

        public async Task<bool> Date_Report_Main(GetRepTillCloseDto model, bool PrintIt, string Where_Clause, string StoreID)
        {
            bool bFinished;
            decimal T_Ent;
            decimal T_Sys;
            decimal T_Dif;
            decimal Exc_Adj_E;
            decimal Exc_Adj_S;
            decimal Exc_Adj_D;
            string query, query2, query3;
            int n, nRecordCount = 0;
            DateTime sTime, eTime;

            sTime = DateTime.Parse(model.startTime);
            eTime = DateTime.Parse(model.endTime);

            const int PRINT_WIDTH = 40; // Update the width as needed
            string filePath = "TillClose.txt";

            // Delete the file if it already exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Create the file and write content to it
            using StreamWriter writer = File.CreateText(filePath);
            if (!_config.IsBackOffice)
                writer.WriteLine(PadC("Store:" + StoreID, PRINT_WIDTH));
            else
                writer.WriteLine(PadC("Store:" + StoreID, PRINT_WIDTH));

            writer.WriteLine(PadC("TILL REPORT BY DATE RANGE", PRINT_WIDTH));


            writer.WriteLine(PadC("From " + model.startDate.ToString("MM-dd-yyyy") + " AT " + sTime.ToString("hh:mm:sstt"), PRINT_WIDTH));
            writer.WriteLine(PadC("To   " + model.endDate.ToString("MM-dd-yyyy") + " AT " + eTime.ToString("hh:mm:sstt"), PRINT_WIDTH));

            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine(PadC("Printed On " + DateTime.Now.ToString("MM-dd-yyyy") + " " + "at " + DateTime.Now.ToString("hh:mm:sstt"), PRINT_WIDTH));
            writer.WriteLine(new string(' ', PRINT_WIDTH));

            if (model.policies.Show_NRGT)
                await printNRGT(writer, model.startDate, model.endDate, StoreID);

            T_Sys = 0;
            Exc_Adj_S = 0;
            n = 0;

            writer.WriteLine(PadR("Tender", PRINT_WIDTH / 2) + PadL("System", PRINT_WIDTH / 2));
            writer.WriteLine(new string('_', PRINT_WIDTH));

            //Search the tender of the close_tenders out of TillCloses
            foreach (var cl in TC.CloseLines)
            {
                writer.WriteLine(PadR(cl.Short_Name, PRINT_WIDTH / 2) + PadL(cl.System.ToString("##,###,##0.00"), PRINT_WIDTH / 2));
                T_Sys += cl.System;
                Exc_Adj_S = Exc_Adj_S - cl.System * (1 - cl.Exchange_Rate);
            }
            writer.WriteLine(new string('_', PRINT_WIDTH));
            writer.WriteLine(PadR("TOTALS", PRINT_WIDTH / 2) + PadL(T_Sys.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Exc_Adj_S != 0)
                writer.WriteLine(PadR("+Exch Adj", PRINT_WIDTH / 2) + PadL(Exc_Adj_S.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Float_Amt != 0)
                writer.WriteLine(PadR("-Float", PRINT_WIDTH / 2) + PadL((-1 * Float_Amt).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Credits_Issued != 0)
                writer.WriteLine(PadR("-Credits", PRINT_WIDTH / 2) + PadL(Credits_Issued.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Payments != 0)
                writer.WriteLine(PadR("-Payments", PRINT_WIDTH / 2) + PadL((-1 * Payments).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (ARPay != 0)
                writer.WriteLine(PadR("-AR Pay", PRINT_WIDTH / 2) + PadL((-1 * ARPay).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (OverPay != 0)
                writer.WriteLine(PadR("-OverPay", PRINT_WIDTH / 2) + PadL((-1 * OverPay).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Draw_Amt != 0)
                writer.WriteLine(PadR("-Draws", PRINT_WIDTH / 2) + PadL((-1 * Draw_Amt).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Drop_Amt != 0)
                writer.WriteLine(PadR("+Drops", PRINT_WIDTH / 2) + PadL(Drop_Amt.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Payouts != 0)
                writer.WriteLine(PadR("+Payouts", PRINT_WIDTH / 2) + PadL(Payouts.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            if (Penny_Adj != 0)
                writer.WriteLine(PadR("+/-Penny Adjustment", PRINT_WIDTH / 2) + PadL((-1 * Penny_Adj).ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            T_Sys += Exc_Adj_S + Credits_Issued - Float_Amt - Draw_Amt + Drop_Amt - Payments - ARPay + Payouts - OverPay - Penny_Adj;

            // Totals after all adjustments
            if (Exc_Adj_S != 0 || Credits_Issued != 0 || Float_Amt != 0 || Draw_Amt != 0
                || Drop_Amt != 0 || OverPay != 0 || Penny_Adj != 0 || Payments != 0 || ARPay != 0
                || Payouts != 0)
            {
                writer.WriteLine(PadL("_", PRINT_WIDTH));
                writer.WriteLine(PadR("TOTALS", PRINT_WIDTH / 2), PadL(T_Sys.ToString("##,###,##0.00"), PRINT_WIDTH / 2));
            }

            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine("Transaction Summary");
            writer.WriteLine(new string('_', PRINT_WIDTH));

            if (Where_Clause.Length == 0)
            {
                query = $"SELECT SaleHead.T_Type as [TType], " +
                    $"  Count(*) as [Transactions], " +
                    $"  Sum(SALE_AMT) As [Amount], " +
                    $"  Sum(PAYMENT) as [Pays] " +
                    $"From CSCTrans.dbo.SaleHead " +
                    $"Where  SaleHead.T_Type <> 'CANCEL' and SaleHead.Till={model.tillNo} " +
                    $"Group By SaleHead.T_Type ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT SaleHead.T_Type as [TType], " +
                            $"  Count(*) as [Transactions], " +
                            $"  Sum(SALE_AMT) As [Amount], " +
                            $"  Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                            $"Group By SaleHead.T_Type ";
                    }
                    else
                    {
                        query = $"SELECT SaleHead.T_Type as [TType], " +
                            $"  Count(*) as [Transactions], " +
                            $"  Sum(SALE_AMT) As [Amount], " +
                            $"  Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                            " AND SaleHead.Store_id = {StoreID} " +
                            $"Group By SaleHead.T_Type ";
                    }
                }
                else
                {
                    query = $"SELECT SaleHead.T_Type as [TType], " +
                        $"  Count(*) as [Transactions], " +
                        $"  Sum(SALE_AMT) As [Amount], " +
                        $"  Sum(PAYMENT) as [Pays] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                        $"Group By SaleHead.T_Type ";
                }
            }
            var list = await Connection.QueryAsync<TendData>(query, null, Transaction);

            foreach (var item in list)
            {
                switch (item.TType)
                {
                    case "PATP_APP":
                        writer.WriteLine(PadR("P@P Sale", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "PATP_C":
                        writer.WriteLine(PadR("P@P Void", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "REFUND":
                        writer.WriteLine(PadR("Refund", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "SALE":
                        writer.WriteLine(PadR("Sale", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "VOID":
                        writer.WriteLine(PadR("Void", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "ARPAY":
                        writer.WriteLine(PadR("AR Pay", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "PAYOUT":
                        writer.WriteLine(PadR("Payout", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    case "BTL RTN":
                        writer.WriteLine(PadR("Bottle Return", 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                    default:
                        writer.WriteLine(PadR(item.TType!, 19) + PadL(item.Transactions.ToString(), 8) + PadL((item.Amount + item.Pays).ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        break;
                }
            }
            writer.WriteLine(new string(' ', 20) + new string('_', 8));

            if (Where_Clause.Length == 0)
            {
                query = $"Select Count(*) as [Transactions] " +
                    $"From CSCTrans.dbo.SaleHead " +
                    $"Where  SaleHead.T_Type <> 'CANCEL' and SaleHead.Till={model.tillNo} ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"Select Count(*) as [Transactions] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} ";
                    }
                    else
                    {
                        query = $"Select Count(*) as [Transactions] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} " +
                                " AND SaleHead.Store_id = {StoreID} ";
                    }
                }
                else
                {
                    query = $"Select Count(*) as [Transactions] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' and {Where_Clause} ";
                }
            }
            var TransNo = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
            writer.WriteLine(PadR("Total Transactions", 20) + PadL(TransNo.ToString(), 8));

            if (Where_Clause.Length == 0)
            {
                query = $"SELECT distinct Sale_No " +
                    $"From CSCTrans.dbo.SaleHead " +
                    $"WHERE Sale_No in " +
                    $"(SELECT distinct Sale_No " +
                    $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                    $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                    $"                 From CSCTrans.dbo.SaleHead " +
                    $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                    $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                    $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL')))";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT distinct Sale_No  " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"WHERE Sale_No in " +
                            $"(SELECT distinct Sale_No " +
                            $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                            $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                 From CSCTrans.dbo.SaleHead " +
                            $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL') AND {Where_Clause}))";
                    }
                    else
                    {
                        query = $"SELECT distinct Sale_No  " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"WHERE Sale_No in " +
                            $"(SELECT distinct Sale_No " +
                            $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                            $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                 From CSCTrans.dbo.SaleHead " +
                            $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL') AND {Where_Clause})) " +
                            $"                     AND SaleHead.Store_id = {StoreID}) " +
                            $" AND SaleLine.Store_id = {StoreID}) " +
                            $"AND SaleHead.Store_id = {StoreID} ";

                    }
                }
                else
                {
                    query = $"SELECT distinct Sale_No  " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"WHERE Sale_No in " +
                        $"(SELECT distinct Sale_No " +
                        $" FROM CSCTrans.dbo.SaleLine WHERE GradeID <> 0 " +
                        $" AND SALE_NO in (SELECT SaleHead.Sale_No " +
                        $"                 From CSCTrans.dbo.SaleHead " +
                        $"                 JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                        $"                 AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                        $"                 WHERE SaleHead.T_Type NOT IN ('VOID','CANCEL') AND {Where_Clause}))";

                }
            }
            var sale = await Connection.QueryAsync<string>(query, null, Transaction);

            if (sale != null && sale.Any())
            {
                foreach (var sa in sale)
                {
                    nRecordCount++;
                }

                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine("Fuel Transaction Summary");
                writer.WriteLine(new string('_', PRINT_WIDTH));

                if (Where_Clause.Length == 0)
                {
                    query = $"Select SaleHead.T_Type as [Till], " +
                        $"       Count(*) as [Transactions], " +
                        $"       Sum(SALE_AMT) As [Amount], " +
                        $"       Sum(PAYMENT) as [Pays] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where  SaleHead.T_Type <> 'CANCEL' and SaleHead.Till = {model.tillNo} " +
                        $"Group By SaleHead.T_Type ";
                }
                else
                {
                    if (_config.IsBackOffice)
                    {
                        //Pay at the pump Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Till=100 AND SaleHead.T_Type <> 'CANCEL'  AND {Where_Clause}  " +
                            $"Group By SaleHead.Till ";

                        var fuelSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (fuelSale != null)
                            writer.WriteLine(PadR("Pay at the pump Sales", 21) + PadL(fuelSale.Transactions!.Value.ToString("##0"), 6) + PadL(fuelSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Pay at the pump Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Post Pay Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                            $"                           FROM CSCTrans.dbo.SaleLine " +
                            $"                           WHERE GradeID <> 0 AND Prepay = 0 AND ManualFuel = 0 " +
                            $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                                              From CSCTrans.dbo.SaleHead " +
                            $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                            $"Group By SaleHead.Till ";

                        var fSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (fSale != null)
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL(fSale.Transactions!.Value.ToString("##0"), 6) + PadL(fSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Prepay Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                            $"                           FROM CSCTrans.dbo.SaleLine " +
                            $"                           WHERE GradeID <> 0 AND Prepay = 1 " +
                            $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                                              From CSCTrans.dbo.SaleHead " +
                            $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                            $"Group By SaleHead.Till ";

                        var pSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (pSale != null)
                            writer.WriteLine(PadR("Prepay Sales", 21) + PadL(pSale.Transactions!.Value.ToString("##0"), 6) + PadL(pSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Prepay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Manual Sales
                        query = $"Select SaleHead.Till, " +
                            $"       Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                            $"                           FROM CSCTrans.dbo.SaleLine " +
                            $"                           WHERE GradeID <> 0 AND ManualFuel = 1 " +
                            $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                            $"                                              From CSCTrans.dbo.SaleHead " +
                            $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                            $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                            $"Group By SaleHead.Till ";

                        var mSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (mSale != null)
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL(mSale.Transactions!.Value.ToString("##0"), 6) + PadL(mSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                    }
                    else //Headoffice
                    {
                        //Post Pay Sales
                        if (StoreID == "ALL")
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 0 AND ManualFuel = 0 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                                $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                                $"Group By SaleHead.Till ";
                        }
                        else
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 0 AND ManualFuel = 0 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No " +
                                $"                                              AND (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause}) " +
                                $"                                                  AND SaleLine.Store_id = {StoreID}) " +
                                $"  AND SaleHead.Store_id = {StoreID} " +
                                $"Group By SaleHead.Till ";
                        }
                        var fSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (fSale != null)
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL(fSale.Transactions!.Value.ToString("##0"), 6) + PadL(fSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Post Pay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);
                        //Prepay Sales
                        if (StoreID == "ALL")
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause})) " +
                                $"Group By SaleHead.Till ";
                        }
                        else
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND Prepay = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') AND {Where_Clause}) " +
                                $"                                                  AND SaleLine.Store_id = {StoreID}) " +
                                $"  AND SaleHead.Store_id = {StoreID} " +
                                $"Group By SaleHead.Till ";
                        }
                        var pSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (pSale != null)
                            writer.WriteLine(PadR("PrePay Sales", 21) + PadL(pSale.Transactions!.Value.ToString("##0"), 6) + PadL(pSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("PrePay Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);

                        //Manual Sales
                        if (StoreID == "ALL")
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND ManualFuel = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') " +
                                $"                                                  AND {Where_Clause})) " +
                                $"Group By SaleHead.Till ";
                        }
                        else
                        {
                            query = $"Select Till, " +
                                $"       Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From CSCTrans.dbo.SaleHead " +
                                $"Where SaleHead.Sale_No in (SELECT Sale_No " +
                                $"                           FROM CSCTrans.dbo.SaleLine " +
                                $"                           WHERE GradeID <> 0 AND ManualFuel = 1 " +
                                $"                              AND SALE_NO in (SELECT SaleHead.Sale_No " +
                                $"                                              From CSCTrans.dbo.SaleHead " +
                                $"                                              WHERE SaleHead.Till <> 100 AND SaleHead.T_Type NOT IN ('CANCEL') " +
                                $"                                                  AND {Where_Clause}) " +
                                $"                                                  AND SaleLine.Store_id = {StoreID}) " +
                                $"  AND SaleHead.Store_id = {StoreID} " +
                                $"Group By SaleHead.Till ";
                        }
                        var mSale = await Connection.QueryFirstOrDefaultAsync<FuelSale>(query, null, Transaction);

                        if (mSale != null)
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL(mSale.Transactions!.Value.ToString("##0"), 6) + PadL(mSale.Amount!.Value.ToString("##,###,##0.00"), 13), PRINT_WIDTH);
                        else
                            writer.WriteLine(PadR("Manual Sales", 21) + PadL("0", 6) + PadL("0", 13), PRINT_WIDTH);
                    } // end of headoffice
                }
                writer.WriteLine(new string(' ', 20) + new string('_', 8));
                writer.WriteLine(PadR("Total Fuel Transactions", 23) + PadL(nRecordCount.ToString(), 7));
            }
            //Summarize Sales Voided and Individual Lines Deleted.
            if (!Product_Sales_Report(writer, model, PrintIt, Where_Clause, StoreID).Result)
            {
                //Console.WriteLine("Error in creating file.");
                return false;
            }
            //end of using filestream
            return true;
        }
        public async Task<bool> Product_Sales_Report(StreamWriter writer, GetRepTillCloseDto model, bool PrintIt, string Where_Clause, string StoreID)
        {
            #region Product_Sales_Report
            bool polShowCodes;
            string? D_Name;
            decimal S_Sales = 0;
            decimal S_Volume = 0;
            decimal F_Sales = 0;
            decimal F_Vol = 0;
            decimal FtSale;
            decimal Ftvol;
            decimal Avg_Price;
            decimal TotDiscount = 0;
            decimal PrepayRefund = 0;
            decimal Sales_Only;
            decimal Volume_Only;
            decimal AddedTax;
            decimal InclTax;
            decimal Tax_Rebate;
            string query, query2, query3;

            query = "SELECT Dept FROM CSCMaster.dbo.FuelDept ";
            string fuelDept = await Connection.QueryFirstOrDefaultAsync<string>(query, null, Transaction);

            polShowCodes = model.policies.ShowCodes;

            query = "SELECT * FROM CSCMaster.dbo.Dept ";
            IEnumerable<Dept> depts = await Connection.QueryAsync<Dept>(query, null, Transaction);

            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine(PadR("PRODUCT SALES", PRINT_WIDTH));
            writer.WriteLine(PadR("Category", 18) + PadL("Qty", 9) + PadL("Sales", 13));

            writer.WriteLine(new string('_', PRINT_WIDTH));

            foreach (var dt in depts)
            {
                if (dt.DEPT != fuelDept)
                {
                    if (dt.EOD_Detail <= 1 || dt.EOD_Detail > 4)
                    {
                        if (Where_Clause.Length == 0)
                        {
                            query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as Volume, " +
                                    $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as Sales " +
                                    $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                    $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND  SaleLine.Dept = dt.DEPT " +
                                    $" AND SaleLine.Till_Num = {TC.Till_Number})" +
                                    $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                    $" AND SaleLine.Dept = dt.DEPT And SaleLine.Till_Num = {TC.Till_Number}) " +
                                    $"Group By SaleLine.Dept " +
                                    $"Order by SaleLine.Dept";
                        }
                        else
                        {
                            if (!_config.IsBackOffice)
                            {
                                if (StoreID == "ALL")
                                {
                                    query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as Volume, " +
                                            $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as Sales " +
                                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                            $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                            $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') " +
                                            $" AND {Where_Clause} " +
                                            $" AND  SaleLine.Dept = dt.DEPT) " +
                                            $"Group By SaleLine.Dept " +
                                            $"Order by SaleLine.Dept";
                                }
                                else
                                {
                                    query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as Volume, " +
                                            $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as Sales " +
                                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                            $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                            $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') " +
                                            $" AND {Where_Clause} " +
                                            $" AND  SaleLine.Dept = dt.DEPT " +
                                            $" AND SaleHead.Store_id = {StoreID}) " +
                                            $"Group By SaleLine.Dept " +
                                            $"Order by SaleLine.Dept";
                                }
                            }
                            else
                            {
                                query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as Volume, " +
                                        $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as Sales " +
                                        $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                        $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') " +
                                        $" AND {Where_Clause} " +
                                        $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                        $"Group By SaleLine.Dept " +
                                        $"Order by SaleLine.Dept";
                            }
                        }
                        IEnumerable<SaleList> sales = await Connection.QueryAsync<SaleList>(query, null, Transaction);
                        if (sales != null && sales.Any())
                        {
                            foreach (var sl in sales)
                            {
                                if (sl.Sales != null)
                                {
                                    //query = $"SELECT DEPT, DEPT_NAME FROM CSCMaster.dbo.Dept where dept = '{sl.Dept}'";
                                    //var dept = await Connection.QueryFirstOrDefaultAsync<Dept>(query, null, Transaction);
                                    query = "SELECT DEPT, DEPT_NAME FROM CSCMaster.dbo.Dept where dept = @Dept";
                                    var parameters = new { Dept = sl.Dept };
                                    var dept = await Connection.QueryFirstOrDefaultAsync<Dept>(query, parameters, Transaction);

                                    if (dept != null)
                                    {
                                        D_Name = dept.DEPT_NAME;

                                        if (polShowCodes)
                                            writer.WriteLine(PadR(sl.Dept, 5) + PadR(D_Name, 13) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));
                                        else
                                            writer.WriteLine(PadR(D_Name, 18) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                                        S_Sales += sl.Sales;
                                        S_Volume += sl.Volume;
                                    }
                                    else
                                    {
                                        writer.WriteLine(PadR("<No Dept>", 13) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                                        S_Sales += sl.Sales;
                                        S_Volume += sl.Volume;
                                    }
                                }
                            }
                        }
                    }
                    else if (dt.EOD_Detail == 2)
                    {
                        query = "";
                        if (Where_Clause.Length == 0)
                        {
                            query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                    $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                    $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                    $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND  SaleLine.Dept = {dt.DEPT} " +
                                    $" AND SaleLine.Till_Num = {TC.Till_Number})" +
                                    $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                    $" AND SaleLine.Dept = {dt.DEPT} And SaleLine.Till_Num = {TC.Till_Number}) " +
                                    $"Group By SaleLine.Dept " +
                                    $"Order by SaleLine.Dept";
                        }
                        else
                        {
                            if (!_config.IsBackOffice)
                            {
                                if (StoreID == "ALL")
                                {
                                    query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                            $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                            $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                            $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                            $" AND {Where_Clause} " +
                                            $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                            $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                            $" AND {Where_Clause} " +
                                            $" AND SaleLine.Dept = {dt.DEPT}) " +
                                            $"Group By SaleLine.Dept " +
                                            $"Order by SaleLine.Dept";
                                }
                                else
                                {
                                    query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                            $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                            $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                            $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                            $" AND {Where_Clause} " +
                                            $" AND  SaleLine.Dept = {dt.DEPT} " +
                                            $" AND SaleHead.Store_id = {StoreID}) " +
                                            $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                            $" AND {Where_Clause} " +
                                            $" AND SaleLine.Dept = {dt.DEPT} " +
                                            $" AND SaleHead.Store_id = {StoreID}) " +
                                            $"Group By SaleLine.Dept " +
                                            $"Order by SaleLine.Dept";
                                }
                            }
                            else
                            {
                                query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                        $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                        $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                        $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                        $" AND {Where_Clause} " +
                                        $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                        $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                        $" AND {Where_Clause} " +
                                        $" AND SaleLine.Dept = {dt.DEPT}) " +
                                        $"Group By SaleLine.Dept " +
                                        $"Order by SaleLine.Dept";
                            }
                        }
                        IEnumerable<SaleList> sales = await Connection.QueryAsync<SaleList>(query, null, Transaction);
                        if (sales != null)
                        {
                            foreach (var sl in sales)
                            {
                                if (sl.Sales != null)
                                {
                                    query = $"SELECT DEPT, DEPT_NAME FROM CSCMaster.dbo.Dept where dept = '{sl.Dept}'";
                                    var dept = await Connection.QueryFirstOrDefaultAsync<Dept>(query, null, Transaction);

                                    if (dept != null)
                                    {
                                        D_Name = dept.DEPT_NAME;

                                        if (polShowCodes)
                                            writer.WriteLine(PadR(sl.Dept, 5) + PadR(D_Name, 13) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));
                                        else
                                            writer.WriteLine(PadR(D_Name, 18) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                                        S_Sales += sl.Sales;
                                        S_Volume += sl.Volume;
                                    }
                                }
                            }
                        }
                    }
                    else if (dt.EOD_Detail == 3)
                    {
                        query = $"SELECT * FROM CSCMaster.dbo.SubDept where dept = '{dt.DEPT}'";
                        IEnumerable<SubDepartment> subDepts = await Connection.QueryAsync<SubDepartment>(query, null, Transaction);

                        if (subDepts != null)
                        {
                            foreach (var sDt in subDepts)
                            {
                                if (Where_Clause.Length == 0)
                                {
                                    query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                            $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                            $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND  SaleLine.Dept = {dt.DEPT} " +
                                            $" AND SaleLine.Till_Num = {TC.Till_Number})" +
                                            $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                            $" AND SaleLine.Dept = {dt.DEPT} And SaleLine.Till_Num = {TC.Till_Number}) " +
                                            $"Group By SaleLine.Dept " +
                                            $"Order by SaleLine.Dept";
                                }
                                else
                                {
                                    if (!_config.IsBackOffice)
                                    {
                                        if (StoreID == "ALL")
                                        {
                                            query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                                    $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                                    $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                                    $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                                    $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                                    $" AND {Where_Clause} " +
                                                    $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                                    $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                                    $" AND {Where_Clause} " +
                                                    $" AND SaleLine.Dept = {dt.DEPT}) " +
                                                    $"Group By SaleLine.Dept " +
                                                    $"Order by SaleLine.Dept";
                                        }
                                        else
                                        {
                                            query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                                    $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                                    $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                                    $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                                    $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                                    $" AND {Where_Clause} " +
                                                    $" AND  SaleLine.Dept = {dt.DEPT} " +
                                                    $" AND SaleHead.Store_id = {StoreID}) " +
                                                    $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                                    $" AND {Where_Clause} " +
                                                    $" AND SaleLine.Dept = {dt.DEPT} " +
                                                    $" AND SaleHead.Store_id = {StoreID}) " +
                                                    $"Group By SaleLine.Dept " +
                                                    $"Order by SaleLine.Dept";
                                        }
                                    }
                                    else
                                    {
                                        query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                                $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                                $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                                $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                                $" AND {Where_Clause} " +
                                                $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                                $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                                $" AND {Where_Clause} " +
                                                $" AND SaleLine.Dept = {dt.DEPT}) " +
                                                $"Group By SaleLine.Dept " +
                                                $"Order by SaleLine.Dept";
                                    }
                                }
                                IEnumerable<SaleList> sales = await Connection.QueryAsync<SaleList>(query, null, Transaction);
                                if (sales != null)
                                {
                                    foreach (var sl in sales)
                                    {
                                        if (sl.Sales != null)
                                        {
                                            query = $"SELECT DEPT, DEPT_NAME FROM CSCMaster.dbo.Dept where dept = '{sl.Dept}'";
                                            var Dept = await Connection.QueryFirstOrDefaultAsync<Dept>(query, null, Transaction);

                                            if (Dept != null)
                                            {
                                                D_Name = Dept.DEPT_NAME;

                                                if (polShowCodes)
                                                    writer.WriteLine(PadR(sl.Dept, 5) + PadR(D_Name, 13) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));
                                                else
                                                    writer.WriteLine(PadR(D_Name, 18) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                                                S_Sales += sl.Sales;
                                                S_Volume += sl.Volume;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else if (dt.EOD_Detail == 4)
                        {
                            if (Where_Clause.Length == 0)
                            {
                                query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                        $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                        $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                        $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND  SaleLine.Dept = {dt.DEPT} " +
                                        $" AND SaleLine.Till_Num = {TC.Till_Number})" +
                                        $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                        $" AND SaleLine.Dept = {dt.DEPT} And SaleLine.Till_Num = {TC.Till_Number}) " +
                                        $"Group By SaleLine.Dept " +
                                        $"Order by SaleLine.Dept";
                            }
                            else
                            {
                                if (!_config.IsBackOffice)
                                {
                                    if (StoreID == "ALL")
                                    {
                                        query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                                $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                                $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                                $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                                $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                                $" AND {Where_Clause} " +
                                                $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                                $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                                $" AND {Where_Clause} " +
                                                $" AND SaleLine.Dept = {dt.DEPT}) " +
                                                $"Group By SaleLine.Dept " +
                                                $"Order by SaleLine.Dept";
                                    }
                                    else
                                    {
                                        query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                                $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                                $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                                $" AND SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = Salehead.Store_id) " +
                                                $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                                $" AND {Where_Clause} " +
                                                $" AND  SaleLine.Dept = {dt.DEPT} " +
                                                $" AND SaleHead.Store_id = {StoreID}) " +
                                                $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                                $" AND {Where_Clause} " +
                                                $" AND SaleLine.Dept = {dt.DEPT} " +
                                                $" AND SaleHead.Store_id = {StoreID}) " +
                                                $"Group By SaleLine.Dept " +
                                                $"Order by SaleLine.Dept";
                                    }
                                }
                                else
                                {
                                    query = $"SELECT SaleLine.Dept,COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                            $" Sum(COALESCE(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj,0)) as [Sales] " +
                                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No) " +
                                            $"WHERE (SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                            $" AND {Where_Clause} " +
                                            $" AND  SaleLine.Dept = {dt.DEPT}) " +
                                            $" OR (SaleHead.T_Type = 'PATP_APP' " +
                                            $" AND {Where_Clause} " +
                                            $" AND SaleLine.Dept = {dt.DEPT}) " +
                                            $"Group By SaleLine.Dept " +
                                            $"Order by SaleLine.Dept";
                                }
                            }
                            IEnumerable<SaleList> sales = await Connection.QueryAsync<SaleList>(query, null, Transaction);
                            if (sales != null && sales.Any())
                            {
                                foreach (var sl in sales)
                                {
                                    if (sl.Sales != null)
                                    {
                                        query = $"SELECT DEPT, DEPT_NAME FROM CSCMaster.dbo.Dept where dept = '{sl.Dept}'";
                                        var Dept = await Connection.QueryFirstOrDefaultAsync<Dept>(query, null, Transaction);

                                        if (Dept != null)
                                        {
                                            D_Name = Dept.DEPT_NAME;

                                            if (polShowCodes)
                                                writer.WriteLine(PadR(sl.Dept, 5) + PadR(D_Name, 13) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));
                                            else
                                                writer.WriteLine(PadR(D_Name, 18) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                                            S_Sales += sl.Sales;
                                            S_Volume += sl.Volume;
                                        }
                                        else
                                        {
                                            writer.WriteLine(PadR("<No Dept>", 18) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                                            S_Sales += sl.Sales;
                                            S_Volume += sl.Volume;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            //Report for sales that do not have any department coding
            if (Where_Clause.Length == 0)
            {
                query = $"SELECT SaleLine.Dept, " +
                        $"       Sum(COALESCE(SaleLine.Quantity,0)) as [Volume],COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                        $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                        $"        SaleLine.Sale_No = SaleHead.Sale_No) LEFT JOIN CSCMaster.dbo.Dept as [D] ON " +
                        $"        D.Dept = SaleLine.Dept " +
                        $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                        $"       D.Dept IS NULL AND " +
                        $"       SaleLine.Till_Num= {TC.Till_Number} " +
                        $"Group by SaleLine.Dept ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT SaleLine.Dept, " +
                                $"       Sum(COALESCE(SaleLine.Quantity,0)) as [Volume],COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                                $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = {StoreID} " +
                                $"LEFT JOIN CSCMaster.dbo.Dept as [D] ON " +
                                $"        D.Dept = SaleLine.Dept " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                                $"       {Where_Clause} AND " +
                                $"       D.Dept IS NULL  " +
                                $"Group by SaleLine.Dept ";
                    }
                    else
                    {
                        query = $"SELECT SaleLine.Dept, " +
                                $"       Sum(COALESCE(SaleLine.Quantity,0)) as [Volume],COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                                $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = SaleHead.Store_Id " +
                                $"LEFT JOIN CSCMaster.dbo.Dept as [D] ON " +
                                $"        D.Dept = SaleLine.Dept " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                                $"       Saleline.Store_id = {StoreID}) AND " +
                                $"       {Where_Clause} AND " +
                                $"       D.Dept IS NULL  " +
                                $"Group by SaleLine.Dept ";
                    }
                }
                else
                {
                    query = $"SELECT SaleLine.Dept, " +
                            $"       Sum(COALESCE(SaleLine.Quantity,0)) as [Volume],COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                            $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"        SaleLine.Sale_No = SaleHead.Sale_No) " +
                            $"LEFT JOIN CSCMaster.dbo.Dept as [D] ON " +
                            $"        D.Dept = SaleLine.Dept " +
                            $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                            $"       {Where_Clause} AND " +
                            $"       D.Dept IS NULL " +
                            $"Group by SaleLine.Dept ";
                }
            }
            IEnumerable<SaleList> saleList2 = await Connection.QueryAsync<SaleList>(query, null, Transaction);
            if (saleList2 != null && saleList2.Any())
            {
                foreach (var sl in saleList2)
                {
                    if (sl.Sales != null)
                    {
                        query = $"SELECT * FROM CSCMaster.dbo.Dept where dept = '{sl.Dept}'";
                        var Dept = await Connection.QueryFirstOrDefaultAsync<Dept>(query, null, Transaction);

                        if (Dept != null)
                        {
                            D_Name = Dept.DEPT_NAME;
                            writer.WriteLine(PadR(D_Name, 18) + PadL(sl.Volume.ToString("########0"), 9) + PadL(sl.Sales.ToString("#####0.00"), 13));

                            S_Sales += sl.Sales;
                            S_Volume += sl.Volume;
                        }
                    }
                }
            }

            writer.WriteLine(new string('_', PRINT_WIDTH));
            writer.WriteLine(PadR("Total Product Sales", 25) + PadL(S_Sales.ToString("##,###,##0.00"), 15));

            //Report Fuel Sales
            query = "";
            if (Where_Clause.Length == 0)
            {
                query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                        $"       COALESCE(Sum(SaleLine.Quantity),0) as [Volume],COALESCE(Sum(SaleLine.Amount),0) as [Sales], " +
                        $"       COALESCE(Sum(SaleLine.REG_PRICE * SaleLine.QUANTITY),0) as [OrigSales], COALESCE(Sum(-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Discount] " +
                        $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                        $"        SaleLine.Sale_No = SaleHead.Sale_No) " +
                        $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                        $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                        $"       D.Dept = {fuelDept} AND " +
                        $"       SaleLine.Till_Num= {TC.Till_Number} " +
                        $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                        $"Order by SaleLine.Stock_Code ";

                query2 = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                        $"       COALESCE(Sum(S_linetax.Tax_Included_Amount),0) as [Includedtax] " +
                        $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                        $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till) " +
                        $"INNER JOIN CSCTrans.dbo.S_LINETAX ON saleline.Sale_No = s_linetax.Sale_No  " +
                        $"       And saleline.till_num = s_linetax.till_num And saleline.line_num=s_linetax.line_no " +
                        $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                        $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                        $"       D.Dept = {fuelDept} AND " +
                        $"       SaleLine.Till_Num= {TC.Till_Number} " +
                        $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                        $"Order by SaleLine.Stock_Code ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                                $"       COALESCE(Sum(SaleLine.Quantity),0) as [Volume],COALESCE(Sum(SaleLine.Amount),0) as [Sales], " +
                                $"       COALESCE(Sum(SaleLine.REG_PRICE * SaleLine.QUANTITY),0) as [OrigSales], COALESCE(Sum(-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Discount] " +
                                $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = SaleHead.Store_Id) " +
                                $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                                $"       {Where_Clause} AND " +
                                $"       D.Dept = {fuelDept} " +
                                $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                $"Order by SaleLine.Stock_Code ";

                        query2 = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                                $"       COALESCE(Sum(S_linetax.Tax_Included_Amount),0) as [Includedtax] " +
                                $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till) " +
                                $"INNER JOIN CSCTrans.dbo.S_LINETAX ON saleline.Sale_No = s_linetax.Sale_No  " +
                                $"       And saleline.till_num = s_linetax.till_num And saleline.line_num=s_linetax.line_no and saleline.store_id = s_linetax.store " +
                                $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                                $"       {Where_Clause} AND " +
                                $"       D.Dept = {fuelDept} " +
                                $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                $"Order by SaleLine.Stock_Code ";
                    }
                    else
                    {
                        query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                                $"       COALESCE(Sum(SaleLine.Quantity),0) as [Volume],COALESCE(Sum(SaleLine.Amount),0) as [Sales], " +
                                $"       COALESCE(Sum(SaleLine.REG_PRICE * SaleLine.QUANTITY),0) as [OrigSales], COALESCE(Sum(-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Discount] " +
                                $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till and Saleline.Store_id = SaleHead.Store_Id) " +
                                $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                                $"       Saleline.Store_id = {StoreID}) AND " +
                                $"       {Where_Clause} AND " +
                                $"       D.Dept = {fuelDept} " +
                                $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                $"Order by SaleLine.Stock_Code ";

                        query2 = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                                $"       COALESCE(Sum(S_linetax.Tax_Included_Amount),0) as [Includedtax] " +
                                $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till) " +
                                $"INNER JOIN CSCTrans.dbo.S_LINETAX ON saleline.Sale_No = s_linetax.Sale_No  " +
                                $"       And saleline.till_num = s_linetax.till_num And saleline.line_num=s_linetax.line_no and saleline.store_id = s_linetax.store " +
                                $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                                $"       SaleHead.Store_Id = {StoreID} AND " +
                                $"       {Where_Clause} AND " +
                                $"       D.Dept = {fuelDept} " +
                                $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                $"Order by SaleLine.Stock_Code ";
                    }
                }
                else
                {
                    query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                            $"       COALESCE(Sum(SaleLine.Quantity),0) as [Volume],COALESCE(Sum(SaleLine.Amount),0) as [Sales], " +
                            $"       COALESCE(Sum(SaleLine.REG_PRICE * SaleLine.QUANTITY),0) as [OrigSales], COALESCE(Sum(-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Discount] " +
                            $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"      SaleLine.Sale_No = SaleHead.Sale_No) " +
                            $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                            $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                            $"      {Where_Clause} AND " +
                            $"      D.Dept = {fuelDept} " +
                            $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                            $"Order by SaleLine.Stock_Code ";

                    query2 = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name, " +
                            $"       COALESCE(Sum(S_linetax.Tax_Included_Amount),0) as [Includedtax] " +
                            $"FROM   (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"        SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.Till_num = SaleHead.Till) " +
                            $"INNER JOIN CSCTrans.dbo.S_LINETAX ON saleline.Sale_No = s_linetax.Sale_No  " +
                            $"       And saleline.till_num = s_linetax.till_num And saleline.line_num=s_linetax.line_no " +
                            $"INNER JOIN CSCMaster.dbo.Dept as [D] ON D.Dept = SaleLine.Dept " +
                            $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND " +
                            $"       {Where_Clause} AND " +
                            $"       D.Dept = {fuelDept} " +
                            $"Group by SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                            $"Order by SaleLine.Stock_Code ";
                }
            }
            IEnumerable<FuelSaleList> fuelSaleList = await Connection.QueryAsync<FuelSaleList>(query, null, Transaction);
            IEnumerable<FuelTaxList> fuelTaxList = await Connection.QueryAsync<FuelTaxList>(query2, null, Transaction);

            if (fuelSaleList != null && fuelSaleList.Any())
            {
                foreach (var sl in fuelSaleList)
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(PadR("FUEL SALES", PRINT_WIDTH));
                    writer.WriteLine(PadR("Grade", 10) + PadL("AVG $", 5) + PadL("Volume", 12) + PadL("Sales", 13));
                    writer.WriteLine(new string('_', PRINT_WIDTH));

                    TotDiscount = 0;
                    if (sl.Sales != null)
                    {
                        query = $"SELECT DEPT, DEPT_NAME FROM CSCMaster.dbo.Dept where dept = '{sl.Dept}'";
                        var fDept = await Connection.QueryFirstOrDefaultAsync<FuelDept>(query, null, Transaction);

                        if (fDept != null)
                        {
                            D_Name = PadR(sl.Descript, 12);

                            S_Sales += sl.Sales + sl.Discount;
                            S_Volume += sl.Volume;

                            F_Sales += sl.Sales + sl.Discount;

                            F_Vol += sl.Volume;

                            if (sl.Volume != 0)
                                Avg_Price = sl.Sales / sl.Volume;
                            else
                                Avg_Price = 0;

                            writer.WriteLine(PadR(D_Name, 12) + PadL(Avg_Price.ToString("0.000"), 5) +
                                PadL(sl.Volume.ToString("#######0.000"), 10) + PadL(sl.Sales.ToString("##,###,##0.00"), 13));

                            if (model.policies.UseOvrPymnt)
                            {
                                if (D_Name.ToUpper().Substring(0, 6) == "PREPAY")
                                    PrepayRefund = -1 * sl.Sales;
                            }

                            if (sl.Discount != 0)
                            {
                                writer.WriteLine(PadR(" ", 8) + PadC("Discount Included", 22) +
                                    PadL(sl.Discount.ToString("######0.00"), 10));
                                TotDiscount += sl.Discount;
                            }
                        }
                    }
                } /* end of for loop */

                if (F_Vol != 0)
                    Avg_Price = F_Sales / F_Vol;
                else
                    Avg_Price = 0;

                writer.WriteLine(new string('_', PRINT_WIDTH));
                writer.WriteLine(PadR("Total", 12) + PadL(Avg_Price.ToString("0.000"), 5) +
                    PadL(F_Vol.ToString("#######0.000"), 10) + PadL(F_Sales.ToString("##,###,##0.00"), 13));

                FtSale = F_Sales;
                Ftvol = F_Vol;

                if (TotDiscount != 0)
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(PadR("Fuel Discount Total", 27) + PadL(Math.Abs(TotDiscount).ToString("##,###,##0.00"), 13));
                }

                S_Sales += Math.Abs(TotDiscount);
            }
            if (model.policies.UseOvrPymnt)
            {
                if (PrepayRefund > 0)
                {
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(PadR("Prepay Refund Adjustment", 30) + PadL(PrepayRefund.ToString("##,###,##0.00"), 10));
                }
            }

            writer.WriteLine(new string(' ', PRINT_WIDTH));
            writer.WriteLine(new string('_', PRINT_WIDTH));
            if (model.policies.UseOvrPymnt)
                writer.WriteLine(PadR("Total Sales", 25) + PadL((S_Sales + PrepayRefund).ToString("##,###,##0.00"), 15));
            else
                writer.WriteLine(PadR("Total Sales", 25) + PadL(S_Sales.ToString("##,###,##0.00"), 15));

            writer.WriteLine(new string(' ', PRINT_WIDTH));

            /// TaxExempt section

            Sales_Only = S_Sales;
            Volume_Only = S_Volume;

            if (Where_Clause.Length == 0)
            {
                query = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                        $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                        $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                        $"       and SaleChg.TILL_NUM = {TC.Till_Number} " +
                        $" Where  SaleHead.T_Type IN ('SALE','REFUND') ";
                query2 = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                        $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                        $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                        $"       and SaleKitChg.TILL_NUM = {TC.Till_Number} " +
                        $" Where  SaleHead.T_Type IN ('SALE','REFUND') ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                                $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                $" Where  SaleHead.T_Type IN ('SALE','REFUND') AND " +
                                $"       {Where_Clause} ";

                        query2 = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                                $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                $"       AND SaleKitChg.Store_Id = SaleHead.Store_Id " +
                                $" Where  SaleHead.T_Type IN ('SALE','REFUND') AND " +
                                $"       {Where_Clause} ";
                    }
                    else
                    {
                        query = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                                $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                $" Where  SaleHead.T_Type IN ('SALE','REFUND') AND " +
                                $"       SaleHead.Store_Id = {StoreID} AND " +
                                $"       {Where_Clause} ";

                        query2 = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                                $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                $"       AND SaleKitChg.Store_Id = SaleHead.Store_Id " +
                                $" Where  SaleHead.T_Type IN ('SALE','REFUND') AND " +
                                $"       SaleHead.Store_Id = {StoreID} AND " +
                                $"       {Where_Clause} ";
                    }
                }
                else
                {
                    query = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                            $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                            $" Where  SaleHead.T_Type IN ('SALE','REFUND') AND " +
                            $"       {Where_Clause} ";

                    query2 = $"Select COALESCE(Sum(Amount),0) as [Deposits]  " +
                            $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                            $" Where  SaleHead.T_Type IN ('SALE','REFUND') AND " +
                            $"       {Where_Clause} ";
                }
            }
            var dblDeposit = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
            var dblDepositK = await Connection.QueryFirstOrDefaultAsync<decimal>(query2, null, Transaction);

            decimal S_Depos = dblDeposit + dblDepositK;

            if (S_Depos != 0)
            {
                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(PadR("Charges", PRINT_WIDTH));
                writer.WriteLine(PadR("Category", 25) + PadL("Qty", 5) + PadL("Volume", 10));
                writer.WriteLine(new string('_', PRINT_WIDTH));

                if (Where_Clause.Length == 0)
                {
                    query = $"Select  distinct AS_CODE, Description   " +
                            $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                            $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                            $"       and SaleChg.TILL_NUM = {TC.Till_Number} ";

                    query2 = $"Select distinct AS_CODE, Description " +
                            $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                            $" Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                            $"       and SaleKitChg.TILL_NUM = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"Select distinct AS_CODE, Description " +
                                    $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                    $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                    $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                    $"       AND {Where_Clause} ";

                            query2 = $"Select distinct AS_CODE, Description  " +
                                    $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                    $"       AND SaleKitChg.Store_Id = SaleHead.Store_Id " +
                                    $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                    $"       AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"Select  distinct AS_CODE, Description   " +
                                    $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                    $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                    $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                    $"       AND SaleHead.Store_Id = {StoreID} " +
                                    $"       AND {Where_Clause} ";

                            query2 = $"Select distinct AS_CODE, Description   " +
                                    $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                    $"       AND SaleKitChg.Store_Id = SaleHead.Store_Id " +
                                    $" Where SaleHead.T_Type IN ('SALE','REFUND') " +
                                    $"       AND SaleHead.Store_Id = {StoreID} " +
                                    $"       AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"Select  distinct AS_CODE, Description   " +
                                $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                $"       AND {Where_Clause} ";

                        query2 = $"Select distinct AS_CODE, Description   " +
                                $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                $" Where SaleHead.T_Type IN ('SALE','REFUND') " +
                                $"       AND {Where_Clause} ";
                    }
                }

                //WriteToFile(writer,new string('_', PRINT_WIDTH));
                //WriteToFile(writer,PadR("Charges", 25) + PadL("", 5) + PadL(S_Depos.ToString("##,###,##0.00"), 15));
                IEnumerable<DeposType> deposType = await Connection.QueryAsync<DeposType>(query, null, Transaction);
                IEnumerable<KitDepos> kitDepos = await Connection.QueryAsync<KitDepos>(query2, null, Transaction);

                if (deposType != null && deposType.Any())
                {
                    writer.WriteLine(PadR("Sale Charges", PRINT_WIDTH));

                    foreach (var dt in deposType)
                    {
                        if (Where_Clause.Length == 0)
                        {
                            query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                    $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                    $" Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                    $"       And SaleChg.AS_CODE = '{dt.AS_CODE}' " +
                                    $"       AND SaleChg.TILL_NUM = {TC.Till_Number} ";
                        }
                        else
                        {
                            if (!_config.IsBackOffice)
                            {
                                if (StoreID == "ALL")
                                {
                                    query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                            $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                            $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                            $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                            $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                            $"       And SaleChg.AS_CODE = '{dt.AS_CODE}' " +
                                            $"       AND {Where_Clause} ";
                                }
                                else
                                {
                                    query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                            $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                            $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                            $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                            $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                            $"       SaleHead.Store_Id = {StoreID} AND " +
                                            $"       And SaleChg.AS_CODE = '{dt.AS_CODE}' " +
                                            $"       AND {Where_Clause} ";
                                }
                            }
                            else
                            {
                                query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                        $"From   CSCTrans.dbo.SaleChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $"       SaleChg.Sale_No = SaleHead.Sale_No " +
                                        $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                        $"       And SaleChg.AS_CODE = '{dt.AS_CODE}' " +
                                        $"       AND {Where_Clause} ";
                            }
                        }
                        SaleCharge saleCharge = await Connection.QueryFirstOrDefaultAsync<SaleCharge>(query, null, Transaction);

                        if (saleCharge.Amt != 0)
                            writer.WriteLine(PadR(dt.DESCRIPTION, 25) + PadL(saleCharge.Qty.ToString("###0"), 5) + PadL(saleCharge.Amt.ToString("###,##0.00"), 10));
                    } /* end of for */
                    writer.WriteLine(PadR(" ", PRINT_WIDTH));

                    if (kitDepos != null && kitDepos.Any())
                    {
                        writer.WriteLine(PadR("Sale Kit Charges", PRINT_WIDTH));

                        foreach (var dt in kitDepos)
                        {
                            if (Where_Clause.Length == 0)
                            {
                                query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                        $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                        $" Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                        $"       And SaleKitChg.AS_CODE = '{dt.AS_CODE}' " +
                                        $"       AND SaleKitChg.TILL_NUM = {TC.Till_Number} ";
                            }
                            else
                            {
                                if (!_config.IsBackOffice)
                                {
                                    if (StoreID == "ALL")
                                    {
                                        query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                                $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                                $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                                $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                                $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                                $"       And SaleKitChg.AS_CODE = '{dt.AS_CODE}' " +
                                                $"       AND {Where_Clause} ";
                                    }
                                    else
                                    {
                                        query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                                $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                                $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                                $"       AND SaleChg.Store_Id = SaleHead.Store_Id " +
                                                $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                                $"       SaleHead.Store_Id = {StoreID} AND " +
                                                $"       And SaleKitChg.AS_CODE = '{dt.AS_CODE}' " +
                                                $"       AND {Where_Clause} ";
                                    }
                                }
                                else
                                {
                                    query = $"Select COALESCE(Sum(QUANTITY),0) as [Qty], COALESCE(Sum(Amount),0) as [Amt] " +
                                            $"From   CSCTrans.dbo.SaleKitChg INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                            $"       SaleKitChg.Sale_No = SaleHead.Sale_No " +
                                            $"Where  SaleHead.T_Type IN ('SALE','REFUND') " +
                                            $"       And SaleKitChg.AS_CODE = '{dt.AS_CODE}' " +
                                            $"       AND {Where_Clause} ";
                                }
                            }
                            SaleCharge saleKitCharge = await Connection.QueryFirstOrDefaultAsync<SaleCharge>(query, null, Transaction);

                            if (saleKitCharge.Amt != 0)
                                writer.WriteLine(PadR(dt.DESCRIPTION, 25) + PadL(saleKitCharge.Qty.ToString("###0"), 5) + PadL(saleKitCharge.Amt.ToString("###,##0.00"), 10));
                        } /* end of for */
                    }
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(PadR("Total Charges", 25) + PadL(S_Depos.ToString("##,###,##0.00"), 15));
                    writer.WriteLine(PadR("", PRINT_WIDTH));
                }

            }

            //TAXES
            S_Sales += S_Depos;

            if (Where_Clause.Length == 0)
            {
                query = $"Select S_SaleTax.Tax_Name, " +
                        $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                        $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included], " +
                        $"      COALESCE(Sum(Tax_Rebate),0) as [Rebate] " +
                        $"From CSCTrans.dbo.S_SaleTax " +
                        $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleKitChg.Sale_No = SaleHead.Sale_No " +
                        $"Where SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') AND S_SaleTax.TILL_NUM={TC.Till_Number} " +
                        $"Group By S_SaleTax.Tax_Name ";

                query2 = $"Select S_LineTax.Tax_Name, S_LineTax.Tax_Code, S_LineTax.Tax_Included, " +
                        $"      Sum(Tax_Added_Amount) as [Taxes], " +
                        $"      Sum(Tax_Included_Amount) as [Included] " +
                        $"From CSCTrans.dbo.S_LineTax " +
                        $"INNER JOIN CSCTrans.dbo.SaleLine ON S_LineTax.Sale_No=SaleLine.Sale_NO AND S_LineTax.Line_No=SaleLine.Line_Num " +
                        $"Where SaleLine.SALE_TYPE IN ('SALE','REFUND','PATP_APP') AND SaleLine.Dept='{fuelDept}' " +
                        $"AND S_LineTax.TILL_NUM={TC.Till_Number} " +
                        $"Group By S_LineTax.Tax_Name,S_LineTax.Tax_Code,S_LineTax.Tax_Included ";

                query3 = $"Select SaleKitChgTax.Tax_Name, " +
                        $"      Sum(Tax_Added_Amount) as [Taxes], Sum(Tax_Included_Amount) as [Included] " +
                        $"From CSCTrans.dbo.SaleKitChgTax " +
                        $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleKitChgTax.Sale_No = SaleHead.Sale_No " +
                        $"Where SaleHead.T_Type IN ('SALE','REFUND') AND SaleKitChgTax.TILL_NUM={TC.Till_Number} " +
                        $"Group By SaleKitChgTax.Tax_Name ";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"Select S_SaleTax.Tax_Name, " +
                                $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                                $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included], " +
                                $"      COALESCE(Sum(Tax_Rebate),0) as [Rebate] " +
                                $"From CSCTrans.dbo.S_SaleTax " +
                                $"INNER JOIN CSCTrans.dbo.SaleHead ON S_SaleTax.Sale_No = SaleHead.Sale_No " +
                                $"       AND S_SaleTax.Store_Id = SaleHead.Store_Id " +
                                $"Where SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') " +
                                $"      AND {Where_Clause} " +
                                $"Group By S_SaleTax.Tax_Name ";

                        query2 = $"Select S_LineTax.Tax_Name, S_LineTax.Tax_Code, S_LineTax.Tax_Included, " +
                                $"      Sum(Tax_Added_Amount) as [Taxes], " +
                                $"      Sum(Tax_Included_Amount) as [Included] " +
                                $"From CSCTrans.dbo.S_LineTax " +
                                $"INNER JOIN CSCTrans.dbo.SaleLine ON S_LineTax.Sale_No=SaleLine.Sale_NO AND S_LineTax.Line_No=SaleLine.Line_Num " +
                                $"       AND S_LineTax.Store_Id = SaleHead.Store_Id " +
                                $"Where SaleLine.SALE_TYPE IN ('SALE','REFUND','PATP_APP') AND SaleLine.Dept='{fuelDept}' " +
                                $"      AND {Where_Clause} " +
                                $"Group By S_LineTax.Tax_Name,S_LineTax.Tax_Code,S_LineTax.Tax_Included ";

                        query3 = $"Select SaleKitChgTax.Tax_Name, " +
                                $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                                $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included] " +
                                $"From CSCTrans.dbo.SaleKitChgTax " +
                                $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleKitChgTax.Sale_No = SaleHead.Sale_No " +
                                $"       AND SaleKitChgTax.Store_Id = SaleHead.Store_Id " +
                                $"Where SaleHead.T_Type IN ('SALE','REFUND') " +
                                $"      AND {Where_Clause} " +
                                $"Group By SaleKitChgTax.Tax_Name ";

                    }
                    else
                    {
                        query = $"Select S_SaleTax.Tax_Name, " +
                                $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                                $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included], " +
                                $"      COALESCE(Sum(Tax_Rebate),0) as [Rebate] " +
                                $"From CSCTrans.dbo.S_SaleTax " +
                                $"INNER JOIN CSCTrans.dbo.SaleHead ON S_SaleTax.Sale_No = SaleHead.Sale_No " +
                                $"       AND S_SaleTax.Store_Id = SaleHead.Store_Id " +
                                $"Where SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') " +
                                $"      AND SaleHead.Store_Id = {StoreID} " +
                                $"      AND {Where_Clause} " +
                                $"Group By S_SaleTax.Tax_Name ";

                        query2 = $"Select S_LineTax.Tax_Name, S_LineTax.Tax_Code, S_LineTax.Tax_Included, " +
                                $"      Sum(Tax_Added_Amount) as [Taxes], " +
                                $"      Sum(Tax_Included_Amount) as [Included] " +
                                $"From CSCTrans.dbo.S_LineTax " +
                                $"INNER JOIN CSCTrans.dbo.SaleLine ON S_LineTax.Sale_No=SaleLine.Sale_NO AND S_LineTax.Line_No=SaleLine.Line_Num " +
                                $"       AND S_LineTax.Store_Id = SaleHead.Store_Id " +
                                $"Where SaleLine.SALE_TYPE IN ('SALE','REFUND','PATP_APP') AND SaleLine.Dept='{fuelDept}' " +
                                $"      AND SaleHead.Store_Id = {StoreID} " +
                                $"      AND {Where_Clause} " +
                                $"Group By S_LineTax.Tax_Name,S_LineTax.Tax_Code,S_LineTax.Tax_Included ";

                        query3 = $"Select SaleKitChgTax.Tax_Name, " +
                                $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                                $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included] " +
                                $"From CSCTrans.dbo.SaleKitChgTax " +
                                $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleKitChgTax.Sale_No = SaleHead.Sale_No " +
                                $"       AND SaleKitChgTax.Store_Id = SaleHead.Store_Id " +
                                $"Where SaleHead.T_Type IN ('SALE','REFUND') " +
                                $"      AND SaleHead.Store_Id = {StoreID} " +
                                $"      AND {Where_Clause} " +
                                $"Group By SaleKitChgTax.Tax_Name ";
                    }
                }
                else
                {
                    query = $"Select S_SaleTax.Tax_Name, " +
                            $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                            $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included], " +
                            $"      COALESCE(Sum(Tax_Rebate),0) as [Rebate] " +
                            $"From CSCTrans.dbo.S_SaleTax " +
                            $"INNER JOIN CSCTrans.dbo.SaleHead ON S_SaleTax.Sale_No = SaleHead.Sale_No " +
                            $"Where SaleHead.T_Type IN ('SALE','REFUND','PATP_APP') " +
                            $"      AND {Where_Clause} " +
                            $"Group By S_SaleTax.Tax_Name ";

                    query2 = $"Select S_LineTax.Tax_Name, S_LineTax.Tax_Code, S_LineTax.Tax_Included, " +
                            $"      Sum(Tax_Added_Amount) as [Taxes], " +
                            $"      Sum(Tax_Included_Amount) as [Included] " +
                            $"From CSCTrans.dbo.S_LineTax " +
                            $"INNER JOIN CSCTrans.dbo.SaleLine ON S_LineTax.Sale_No=SaleLine.Sale_NO AND S_LineTax.Line_No=SaleLine.Line_Num " +
                            $"Where SaleLine.SALE_TYPE IN ('SALE','REFUND','PATP_APP') AND SaleLine.Dept='{fuelDept}' " +
                            $"      AND {Where_Clause} " +
                            $"Group By S_LineTax.Tax_Name,S_LineTax.Tax_Code,S_LineTax.Tax_Included ";

                    query3 = $"Select SaleKitChgTax.Tax_Name, " +
                            $"      COALESCE(Sum(Tax_Added_Amount),0) as [Taxes], " +
                            $"      COALESCE(Sum(Tax_Included_Amount),0) as [Included] " +
                            $"From CSCTrans.dbo.SaleKitChgTax " +
                            $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleKitChgTax.Sale_No = SaleHead.Sale_No " +
                            $"Where SaleHead.T_Type IN ('SALE','REFUND') " +
                            $"      AND {Where_Clause} " +
                            $"Group By SaleKitChgTax.Tax_Name ";
                }
            }
            IEnumerable<SaleTax> saleTax = await Connection.QueryAsync<SaleTax>(query, null, Transaction);

            if (saleTax != null && saleTax.Any())
            {
                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(PadR("TAXES", PRINT_WIDTH));
                writer.WriteLine(PadR("Name", 7) + PadL("Included", 10) + PadL("Added", 10) + PadL("Total", 13));
                writer.WriteLine(new string('_', PRINT_WIDTH));

                AddedTax = 0;
                InclTax = 0;
                Tax_Rebate = 0;

                foreach (var st in saleTax)
                {
                    if ((st.Taxes != 0) || (st.Included != 0))
                    {
                        writer.WriteLine(PadR(st.Tax_Name, 7) + PadL(st.Included.ToString("###,##0.00"), 10) +
                            PadL(st.Taxes.ToString("###,##0.00"), 10) + PadL((st.Included + st.Taxes).ToString("##,###,##0.00"), 13));

                        if (model.policies.TaxRebate)
                        {
                            writer.WriteLine(PadR("Tax Rebate", 10) + PadL(st.Rebate.ToString("###,##0.00"), 30));
                            Tax_Rebate += st.Rebate;
                        }
                        else
                            Tax_Rebate = 0;

                        AddedTax += st.Taxes;
                        InclTax += st.Included;
                        S_Sales = S_Sales + st.Taxes - Tax_Rebate;
                    }
                }
                writer.WriteLine(new string('_', PRINT_WIDTH));
                writer.WriteLine(PadR("Total", 7) + PadL(InclTax.ToString("###,##0.00"), 10) +
                    PadL(AddedTax.ToString("###,##0.00"), 10) + PadL((InclTax + AddedTax - Tax_Rebate).ToString("###,##0.00"), 13));
            }

            IEnumerable<SaleTax> saleTaxKitC = await Connection.QueryAsync<SaleTax>(query3, null, Transaction);

            if (saleTaxKitC != null && saleTaxKitC.Any())
            {
                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(PadR("TAXES ADDED ON CHARGES", PRINT_WIDTH));
                writer.WriteLine(PadR("Name", 7) + PadL("Included", 10) + PadL("Added", 10) + PadL("Total", 13));
                writer.WriteLine(new string('_', PRINT_WIDTH));

                AddedTax = 0;
                InclTax = 0;
                Tax_Rebate = 0;

                foreach (var st in saleTaxKitC)
                {
                    if ((st.Taxes != 0) || (st.Included != 0))
                    {
                        writer.WriteLine(PadR(st.Tax_Name, 7) + PadL(st.Included.ToString("###,##0.00"), 10) +
                            PadL(st.Taxes.ToString("###,##0.00"), 10) + PadL((st.Included + st.Taxes).ToString("##,###,##0.00"), 13));
                        AddedTax += st.Taxes;
                        InclTax += st.Included;
                        S_Sales = S_Sales + st.Taxes;
                    }
                }
                writer.WriteLine(new string('_', PRINT_WIDTH));
                writer.WriteLine(PadR("Total", 7) + PadL(InclTax.ToString("###,##0.00"), 10) +
                    PadL(AddedTax.ToString("###,##0.00"), 10) + PadL((InclTax + AddedTax).ToString("###,##0.00"), 13));
            }

            writer.WriteLine(new string('_', PRINT_WIDTH));
            writer.WriteLine(PadR("TOTALS", 25) + PadL(S_Sales.ToString("###,##0.00"), 15));

            writer.WriteLine(new string(' ', PRINT_WIDTH));

            Sales_Only = S_Sales;
            Volume_Only = S_Volume;

            ///TaxExempt
            if (model.policies.TaxExempt)
            {
                if (model.policies.ShowAllExempt)
                {
                    Print_AITE_Report(writer, Where_Clause, StoreID);
                    Print_SITE_Report(writer, Where_Clause, StoreID);
                }
                else
                {
                    if (model.policies.Te_Type == "AITE")
                        Print_AITE_Report(writer, Where_Clause, StoreID);
                    else
                        Print_SITE_Report(writer, Where_Clause, StoreID);
                }
            }
            ///Summary for Non-Taxable items section
            query = "";
            if (Where_Clause.Length == 0)
            {
                query = $"SELECT Count(*) AS CNT, COALESCE(sum(SALELINE.QUANTITY),0) AS QTY, " +
                        $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                        $"FROM CSCTrans.dbo.SALELINE " +
                        $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No AND SALELINE.SALE_NO=S_LineTax.Sale_No " +
                        $"WHERE S_LineTax.Tax_Name IS NULL AND SaleLine.Till_Num = {TC.Till_Number} " +
                        $"       And SaleLine.SALE_TYPE IN ('SALE','REFUND','EXCHANGE','PATP_APP')";
            }
            else
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"SELECT Count(*) AS CNT, COALESCE(sum(SALELINE.QUANTITY),0) AS QTY, " +
                                $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                                $"FROM CSCTrans.dbo.SALELINE " +
                                $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No " +
                                $"      AND SALELINE.SALE_NO=S_LineTax.Sale_No " +
                                $"      AND SALELINE.Store_Id = S_LineTax.Store " +
                                $"JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No AND SaleLine.Store_Id = SaleHead.Store_Id " +
                                $"WHERE S_LineTax.Tax_Name IS NULL " +
                                $"      AND SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') " +
                                $"      AND {Where_Clause} ";
                    }
                    else
                    {
                        query = $"SELECT Count(*) AS CNT, COALESCE(sum(SALELINE.QUANTITY),0) AS QTY, " +
                                $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                                $"FROM CSCTrans.dbo.SALELINE " +
                                $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No " +
                                $"      AND SALELINE.SALE_NO=S_LineTax.Sale_No " +
                                $"      AND SALELINE.Store_Id = S_LineTax.Store " +
                                $"JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No AND SaleLine.Store_Id = SaleHead.Store_Id " +
                                $"WHERE S_LineTax.Tax_Name IS NULL " +
                                $"      AND SaleHead.Store_Id = {StoreID} AND SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE','PATP_APP') " +
                                $"      AND {Where_Clause} ";
                    }
                }
                else
                {
                    query = $"SELECT Count(*) AS CNT, COALESCE(sum(SALELINE.QUANTITY),0) AS QTY, " +
                            $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                            $"FROM CSCTrans.dbo.SALELINE " +
                            $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No AND SALELINE.SALE_NO=S_LineTax.Sale_No " +
                            $"JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                            $"WHERE S_LineTax.Tax_Name IS NULL " +
                            $"       And SaleLine.SALE_TYPE IN ('SALE','REFUND','EXCHANGE','PATP_APP') " +
                            $"       AND {Where_Clause} ";
                }
            }
            SaleNoTax saleNoTax = await Connection.QueryFirstOrDefaultAsync<SaleNoTax>(query, null, Transaction);

            if (saleNoTax != null)
            {
                if (saleNoTax.CNT > 0)
                {
                    writer.WriteLine(PadR("", PRINT_WIDTH));
                    writer.WriteLine(PadR("Non-Taxable Product Sales", PRINT_WIDTH));
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(PadR("", 18) + PadL("Qty", 9) + PadL("Sales", 13));

                    query = "";
                    if (Where_Clause.Length == 0)
                    {
                        query = $"SELECT SaleLine.Dept, " +
                                $"      COALESCE(sum(SALELINE.QUANTITY),0) AS QTY " +
                                $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                                $"FROM CSCTrans.dbo.SALELINE " +
                                $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No AND SALELINE.SALE_NO=S_LineTax.Sale_No " +
                                $"WHERE S_LineTax.Tax_Name IS NULL AND SaleLine.Till_Num = {TC.Till_Number} " +
                                $"       And SaleLine.SALE_TYPE IN ('SALE','REFUND','EXCHANGE','PATP_APP') " +
                                $"Group By SaleLine.Dept " +
                                $"Order by SaleLine.Dept";
                    }
                    else
                    {
                        if (!_config.IsBackOffice)
                        {
                            if (StoreID == "ALL")
                            {
                                query = $"SELECT SaleLine.Dept, " +
                                        $"      COALESCE(sum(SALELINE.QUANTITY),0) AS QTY " +
                                        $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                                        $"FROM CSCTrans.dbo.SALELINE " +
                                        $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No " +
                                        $"      AND SALELINE.SALE_NO=S_LineTax.Sale_No AND SALELINE.Store_Id = S_LineTax.Store " +
                                        $"JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No AND SaleLine.Store_Id = SaleHead.Store_Id " +
                                        $"WHERE S_LineTax.Tax_Name IS NULL  " +
                                        $"       And SaleHead.T_TYPE IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND {Where_Clause} " +
                                        $"Group By SaleLine.Dept " +
                                        $"Order by SaleLine.Dept";
                            }
                            else
                            {
                                query = $"SELECT SaleLine.Dept, " +
                                        $"      COALESCE(sum(SALELINE.QUANTITY),0) AS QTY " +
                                        $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                                        $"FROM CSCTrans.dbo.SALELINE " +
                                        $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No " +
                                        $"      AND SALELINE.SALE_NO=S_LineTax.Sale_No AND SALELINE.Store_Id = S_LineTax.Store " +
                                        $"JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No AND SaleLine.Store_Id = SaleHead.Store_Id " +
                                        $"WHERE S_LineTax.Tax_Name IS NULL  " +
                                        $"      AND SaleHead.Store_Id = {StoreID} AND SaleHead.T_TYPE IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND {Where_Clause} " +
                                        $"Group By SaleLine.Dept " +
                                        $"Order by SaleLine.Dept";
                            }
                        }
                        else
                        {
                            query = $"SELECT SaleLine.Dept, " +
                                    $"      COALESCE(sum(SALELINE.QUANTITY),0) AS QTY, " +
                                    $"      COALESCE(SUM(SALELINE.AMOUNT-SaleLine.Discount-SaleLine.Disc_Adj),0) AS AMT " +
                                    $"FROM CSCTrans.dbo.SALELINE " +
                                    $"LEFT JOIN CSCTrans.dbo.S_LineTax ON SALELINE.LINE_NUM=S_LineTax.Line_No " +
                                    $"      AND SALELINE.SALE_NO=S_LineTax.Sale_No " +
                                    $"JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                    $"WHERE S_LineTax.Tax_Name IS NULL  " +
                                    $"      AND SaleHead.T_TYPE IN ('SALE','REFUND','EXCHANGE','PATP_APP') AND {Where_Clause} " +
                                    $"Group By SaleLine.Dept " +
                                    $"Order by SaleLine.Dept";
                        }
                    }
                    IEnumerable<SaleLineTax> saleNoTaxDept = await Connection.QueryAsync<SaleLineTax>(query, null, Transaction);
                    if (saleNoTaxDept != null && saleNoTaxDept.Any())
                    {
                        foreach (var snt in saleNoTaxDept)
                        {
                            if (snt.Dept != "")
                            {
                                query = $"SELECT Dept + ' ' + Dept_Name As DeptName FROM CSCMaster.dbo.Dept WHERE DEPT = {snt.Dept}";
                                string DeptName = await Connection.QueryFirstOrDefaultAsync<string>(query, null, Transaction);

                                if (DeptName != null)
                                {
                                    if (snt.Dept == fuelDept)
                                        writer.WriteLine(PadR(DeptName, 17) + PadL(snt.QTY.ToString("#####0.000"), 10) + PadL(snt.AMT.ToString("##,###,##0.00"), 13));
                                    else
                                        writer.WriteLine(PadR(DeptName, 17) + PadL(snt.QTY.ToString("#########0"), 10) + PadL(snt.AMT.ToString("##,###,##0.00"), 13));
                                }
                            }
                        }
                    }
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(PadR("Total Sales", 17) + PadL(saleNoTax.QTY.ToString("#####0.000"), 10) + PadL(saleNoTax.AMT.ToString("##,###,##0.00"), 13));
                    writer.WriteLine(PadR("", PRINT_WIDTH));
                }
            }
            if (model.policies.Te_Type == "QITE" || model.policies.Te_Type == "AITE")
            {
                writer.WriteLine(PadR("", PRINT_WIDTH));
                Print_TE_TAXES_Report(writer, Where_Clause, StoreID);
            }

            ///EOD Grouping Summary
            bool blGroupSummary = true;
            decimal GroupTotalVolume = 0;
            decimal GroupTotalSales = 0;

            if (model.policies.EodGroup)
            {
                query = "SELECT * FROM CSCMaster.dbo.EodGroups ";
                IEnumerable<EodGroup> eodGroups = await Connection.QueryAsync<EodGroup>(query, null, Transaction);

                query = "";
                if (eodGroups != null && eodGroups.Any())
                {
                    foreach (var eg in eodGroups)
                    {
                        if (Where_Clause.Length == 0)
                        {
                            query = $"SELECT max(saleline.dept) as [Dept], COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                    $"      COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                                    $"FROM CSCTrans.dbo.SaleLine " +
                                    $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                    $"INNER JOIN CSCMaster.dbo.Dept ON Dept.Dept = SaleLine.Dept " +
                                    $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                                    $"      Dept.eodgroup = {eg.EodGroupID} " +
                                    $"      AND  SaleLine.TILL_NUM={TC.Till_Number} ";
                        }
                        else
                        {
                            if (!_config.IsBackOffice)
                            {
                                if (StoreID == "ALL")
                                {
                                    query = $"SELECT max(saleline.dept) as [Dept], COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                            $"      COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                                            $"FROM CSCTrans.dbo.SaleLine " +
                                            $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.store_id = SaleHead.store_id " +
                                            $"INNER JOIN CSCMaster.dbo.Dept ON Dept.Dept = SaleLine.Dept " +
                                            $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                                            $"      Dept.eodgroup = {eg.EodGroupID} " +
                                            $"      AND {Where_Clause} ";
                                }
                                else
                                {
                                    query = $"SELECT max(saleline.dept) as [Dept], COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                            $"      COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                                            $"FROM CSCTrans.dbo.SaleLine " +
                                            $"INNER JOIN SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No and SaleLine.store_id = SaleHead.store_id " +
                                            $"INNER JOIN CSCMaster.dbo.Dept ON Dept.Dept = SaleLine.Dept " +
                                            $"WHERE SaleHead.Store_Id = {StoreID} AND SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                                            $"      Dept.eodgroup = {eg.EodGroupID} " +
                                            $"      AND {Where_Clause} ";
                                }
                            }
                            else
                            {
                                query = $"SELECT max(saleline.dept) as [Dept], COALESCE(Sum(SaleLine.Quantity),0) as [Volume], " +
                                        $"      COALESCE(Sum(SaleLine.Amount-SaleLine.Discount-SaleLine.Disc_Adj),0) as [Sales] " +
                                        $"FROM CSCTrans.dbo.SaleLine " +
                                        $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleLine.Sale_No = SaleHead.Sale_No " +
                                        $"INNER JOIN CSCMaster.dbo.Dept ON Dept.Dept = SaleLine.Dept " +
                                        $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND " +
                                        $"      Dept.eodgroup = {eg.EodGroupID} " +
                                        $"      AND {Where_Clause} ";
                            }
                        }
                        EodGrp eodGrp = await Connection.QueryFirstOrDefaultAsync<EodGrp>(query, null, Transaction);

                        if (eodGrp != null)
                        {
                            if (eodGrp.Volume > 0)
                            {
                                if (blGroupSummary)
                                {
                                    writer.WriteLine(PadR("", PRINT_WIDTH));
                                    writer.WriteLine(PadR("GROUP SUMMARY", PRINT_WIDTH));
                                    writer.WriteLine(new string('_', PRINT_WIDTH));
                                    writer.WriteLine(PadR("Group Category", 18) + PadR("Qty", 9) + PadL("Sales", 13));
                                    writer.WriteLine(PadR("", PRINT_WIDTH));
                                    blGroupSummary = false;
                                }

                                if (eodGrp.Dept == fuelDept)
                                    writer.WriteLine(PadR(eg.EodGroupName, 17) + PadR(eodGrp.Volume.ToString("#####0.000"), 10) + PadL(eodGrp.Sales.ToString("#,###,##0.00"), 13));
                                else
                                    writer.WriteLine(PadR(eg.EodGroupName, 18) + PadR(eodGrp.Volume.ToString("######0"), 9) + PadL(eodGrp.Sales.ToString("#,###,##0.00"), 13));

                                GroupTotalVolume += eodGrp.Volume;
                                GroupTotalSales += eodGrp.Sales;
                            }
                        }
                    }
                    if (GroupTotalVolume > 0 || GroupTotalSales > 0)
                    {
                        if ((Sales_Only - GroupTotalSales) > 0 || (Volume_Only - GroupTotalVolume) > 0)
                            writer.WriteLine(PadR("Total Other Sales", 20) + PadR((Volume_Only - GroupTotalVolume).ToString("####0"), 10) + PadL((Sales_Only - GroupTotalSales).ToString("###0.00"), 10));

                        writer.WriteLine(new string('_', PRINT_WIDTH));
                        writer.WriteLine(PadR("Total Sales", 20) + PadL("", 10) + PadL(Sales_Only.ToString("###0.00"), 10));
                    }
                }
            } /* end of EOD Group */

            ///TaxExempt
            query = "";
            if (model.policies.TaxExempt)
            {
                if (model.policies.Te_Type == "AITE" || model.policies.Te_Type == "QITE")
                {
                    if (Where_Clause.Length == 0)
                    {
                        query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                $"      SaleLine.Dept, D.Dept_Name, " +
                                $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                $"FROM (SaleLine INNER JOIN SaleHead ON " +
                                $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                $"      Saleline.Till_num  = SaleHead.till) " +
                                $"INNER JOIN TaxExemptSaleLine ON " +
                                $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                $"      and TaxExemptSaleLine.Till_Num =Saleline.Till_Num " +
                                $"      and TaxExemptSaleline.Line_num = saleline.line_num " +
                                $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                $"      D.Dept = SaleLine.Dept " +
                                $"WHERE SaleLine.Dept = {fuelDept} and Salehead.Till={TC.Till_Number} " +
                                $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                $"Order by SaleLine.Stock_Code";
                    }
                    else
                    {
                        if (!_config.IsBackOffice)
                        {
                            if (StoreID == "ALL")
                            {
                                query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                        $"      SaleLine.Dept, D.Dept_Name, " +
                                        $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                        $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                        $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                        $"FROM (SaleLine INNER JOIN SaleHead ON " +
                                        $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                        $"      Saleline.Till_num  = SaleHead.till and " +
                                        $"      SaleLine.Store_Id = SaleHead.Store_Id ) " +
                                        $"INNER JOIN TaxExemptSaleLine ON " +
                                        $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                        $"      and TaxExemptSaleLine.Till_Num =Saleline.Till_Num " +
                                        $"      and TaxExemptSaleline.Line_num = saleline.line_num " +
                                        $"      and TaxExemptSaleline.Store_Id = saleline.Store_Id " +
                                        $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                        $"      D.Dept = SaleLine.Dept " +
                                        $"WHERE {Where_Clause} AND SaleLine.Dept = {fuelDept} " +
                                        $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                        $"Order by SaleLine.Stock_Code";
                            }
                            else
                            {
                                query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                        $"      SaleLine.Dept, D.Dept_Name, " +
                                        $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                        $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                        $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                        $"FROM (SaleLine INNER JOIN SaleHead ON " +
                                        $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                        $"      Saleline.Till_num  = SaleHead.till and " +
                                        $"      SaleLine.Store_Id = SaleHead.Store_Id ) " +
                                        $"INNER JOIN TaxExemptSaleLine ON " +
                                        $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                        $"      and TaxExemptSaleLine.Till_NUm =Saleline.Till_Num " +
                                        $"      and TaxExemptSaleline.Line_nuTaxable Fuel Sales m = saleline.line_num " +
                                        $"      and TaxExemptSaleline.Store_Id = saleline.Store_Id " +
                                        $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                        $"      D.Dept = SaleLine.Dept " +
                                        $"WHERE SaleHead.Store_Id = {StoreID} and " +
                                        $"      {Where_Clause} AND SaleLine.Dept = {fuelDept} " +
                                        $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                        $"Order by SaleLine.Stock_Code";
                            }
                        }
                        else
                        {
                            query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                    $"      SaleLine.Dept, D.Dept_Name, " +
                                    $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                    $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                    $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                    $"FROM (SaleLine INNER JOIN SaleHead ON " +
                                    $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                    $"      Saleline.Till_num  = SaleHead.till " +
                                    $"INNER JOIN TaxExemptSaleLine ON " +
                                    $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                    $"      and TaxExemptSaleLine.Till_Num =Saleline.Till_Num " +
                                    $"      and TaxExemptSaleline.Line_num = saleline.line_num " +
                                    $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                    $"      D.Dept = SaleLine.Dept " +
                                    $"WHERE {Where_Clause} AND SaleLine.Dept = {fuelDept} " +
                                    $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                    $"Order by SaleLine.Stock_Code";
                        }
                    }

                }
                //else if (model.policies.Te_Type == "SITE")
                else
                {
                    if (Where_Clause.Length == 0)
                    {
                        query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                $"      SaleLine.Dept, D.Dept_Name, " +
                                $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                $"      Saleline.Till_num  = SaleHead.till " +
                                $"INNER JOIN CSCTrans.dbo.TaxExemptSaleLine ON " +
                                $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                $"      and TaxExemptSaleLine.Till_NUM =Saleline.Till_Num " +
                                $"      and TaxExemptSaleline.Line_NUM = saleline.line_num " +
                                $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                $"      D.Dept = SaleLine.Dept) " +
                                $"WHERE SaleLine.Dept = {fuelDept} and Salehead.Till={TC.Till_Number} " +
                                $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                $"Order by SaleLine.Stock_Code";
                    }
                    else
                    {
                        if (!_config.IsBackOffice)
                        {
                            if (StoreID == "ALL")
                            {
                                query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                        $"      SaleLine.Dept, D.Dept_Name, " +
                                        $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                        $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                        $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                        $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                        $"      Saleline.Till_num  = SaleHead.till " +
                                        $"INNER JOIN CSCTrans.dbo.TaxExemptSaleLine ON " +
                                        $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                        $"      and TaxExemptSaleLine.Till_NUM =Saleline.Till_Num " +
                                        $"      and TaxExemptSaleline.Line_NUM = saleline.line_num " +
                                        $"      and purchaseitem.Store_id = saleline.Store_id " +
                                        $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                        $"      D.Dept = SaleLine.Dept) " +
                                        $"WHERE {Where_Clause} AND SaleLine.Dept = {fuelDept} " +
                                        $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                        $"Order by SaleLine.Stock_Code";
                            }
                            else
                            {
                                query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                        $"      SaleLine.Dept, D.Dept_Name, " +
                                        $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                        $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                        $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                        $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                        $"      Saleline.Till_num  = SaleHead.till " +
                                        $"INNER JOIN CSCTrans.dbo.TaxExemptSaleLine ON " +
                                        $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                        $"      and TaxExemptSaleLine.Till_NUM =Saleline.Till_Num " +
                                        $"      and TaxExemptSaleline.Line_NUM = saleline.line_num " +
                                        $"      and TaxExemptSaleline.Store_id = saleline.Store_id " +
                                        $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                        $"      D.Dept = SaleLine.Dept) " +
                                        $"WHERE SaleHead.Store_Id = {StoreID} and " +
                                        $"      {Where_Clause} AND SaleLine.Dept = {fuelDept} " +
                                        $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                        $"Order by SaleLine.Stock_Code";
                            }
                        }
                        else
                        {
                            query = $"SELECT SaleLine.Stock_Code, SaleLine.Descript, " +
                                    $"      SaleLine.Dept, D.Dept_Name, " +
                                    $"      Sum(TaxExemptSaleLine.Quantity) as [Volume], " +
                                    $"      Sum(TaxExemptSaleLine.Amount) as [Sales], " +
                                    $"      Sum(-SaleLine.Discount-SaleLine.Disc_Adj) as [Discount] " +
                                    $"FROM (CSCTrans.dbo.SaleLine INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $"      SaleLine.Sale_No = SaleHead.Sale_No and " +
                                    $"      Saleline.Till_num  = SaleHead.till " +
                                    $"INNER JOIN CSCTrans.dbo.TaxExemptSaleLine ON " +
                                    $"      TaxExemptSaleLine.Sale_No =Saleline.Sale_no " +
                                    $"      and TaxExemptSaleLine.Till_NUM =Saleline.Till_Num " +
                                    $"      and TaxExemptSaleline.Line_NUM = saleline.line_num " +
                                    $"INNER JOIN CSCMaster.dbo.Dept as [D] ON " +
                                    $"      D.Dept = SaleLine.Dept) " +
                                    $"WHERE {Where_Clause} AND SaleLine.Dept = {fuelDept} " +
                                    $"Group By SaleLine.Stock_Code, SaleLine.Descript, SaleLine.Dept, D.Dept_Name " +
                                    $"Order by SaleLine.Stock_Code";
                        }

                    }

                }
                IEnumerable<TeSaleList> teSaleList = await Connection.QueryAsync<TeSaleList>(query, null, Transaction);

                if (teSaleList != null && teSaleList.Any())
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(PadR("Tax Exempt Fuel Sales", PRINT_WIDTH));
                    writer.WriteLine(PadR("Grade", 10) + PadL("", 5) + PadL("Volume", 12) + PadL("Sales", 13));
                    writer.WriteLine(new string('_', PRINT_WIDTH));

                    foreach (var ts in teSaleList)
                    {
                        D_Name = PadR(ts.Descript, 12);
                        F_Sales += ts.Sales + ts.Discount;
                        F_Vol += ts.Volume;
                        writer.WriteLine(PadR(ts.Descript, 12) + PadL("", 5) + PadL(ts.Volume.ToString("######0.000"), 12) + PadL(ts.Sales.ToString("###,##0.00"), 13));

                        if (ts.Discount != 0)
                        {
                            writer.WriteLine(PadR("", 8) + PadC("Discount Included", 22) + PadL(ts.Discount.ToString("######0.00"), 10));
                        }
                    }
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(PadR("Total", 10) + PadL("", 5) + PadL(F_Vol.ToString("#######0.000"), 12) +
                        PadL(F_Sales.ToString("##,###,##0.00"), 13));

                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                }

                //this section is for Fuel after excluding the included tax and also after reducing taxexempt sales
                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(PadR("FUEL SALES - DETAIL", PRINT_WIDTH));

                if (fuelSaleList != null && fuelSaleList.Any())
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(PadR("Taxable Fuel Sales (Excluding taxes)", PRINT_WIDTH));
                    writer.WriteLine(PadR("Grade", 10) + PadL("Avg $", 5) + PadL("Volume", 12) + PadL("Sales", 13));
                    writer.WriteLine(new string('_', PRINT_WIDTH));

                    F_Sales = 0;
                    F_Vol = 0;

                    foreach (var sl in fuelSaleList)
                    {
                        if (model.policies.TaxExempt)
                        {
                            foreach (var ts in teSaleList)
                            {
                                if (sl.Stock_Code == ts.Stock_Code)
                                {
                                    foreach (var ft in fuelTaxList)
                                    {
                                        if (ts.Stock_Code == ft.Stock_Code)
                                        {
                                            if (model.policies.Te_Type == "AITE" || model.policies.Te_Type == "SITE" || model.policies.Te_Type == "QITE")
                                            {
                                                F_Sales += sl.Sales - ts.Sales + sl.Discount - ts.Discount;
                                                F_Vol += sl.Volume - ts.Volume;

                                                writer.WriteLine(PadR(sl.Descript, 10) + PadL("", 5) + PadL((sl.Volume - ts.Volume).ToString("#######0.000"), 12) + PadL((sl.Sales - ts.Sales).ToString("#######0.000"), 13));
                                            }
                                        }
                                        else
                                        {
                                            F_Sales += sl.Sales - ts.Sales + sl.Discount - ts.Discount;
                                            F_Vol += sl.Volume - ts.Volume;

                                            writer.WriteLine(PadR(sl.Descript, 10) + PadL("", 5) + PadL((sl.Volume - ts.Volume).ToString("#######0.000"), 12) + PadL((sl.Sales - ts.Sales).ToString("#######0.000"), 13));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var ft in fuelTaxList)
                            {
                                F_Sales += sl.Sales + sl.Discount;
                                F_Vol += sl.Volume;

                                sl.Volume = (sl.Volume == 0) ? 1 : sl.Volume;
                                writer.WriteLine(PadR(sl.Descript, 10) + PadL((sl.Sales / sl.Volume).ToString("#0.000"), 5) + PadL(sl.Volume.ToString("#######0.000"), 12) + PadL(sl.Sales.ToString("#######0.000"), 13));

                                if (sl.Discount != 0)
                                {
                                    writer.WriteLine(PadR("", 8) + PadC("Discount Included", 22) + PadL((sl.Discount).ToString("######0.00"), 10));
                                }
                            }
                        }
                    } /* FuelSale Loop */
                } /* End of if FuleSale */
            } /* End of if model.policies.TaxExempt */
            /* End of Product_Sales_Report */
            #endregion

            return true;
        }

        public async Task<bool> printNRGT(StreamWriter writer, DateTime SDate, DateTime EDate, string StoreID)
        {
            string sSQL;
            NRGTReport r = new();
            DateOnly sDate, eDate;
            sDate = DateOnly.FromDateTime(SDate);
            eDate = DateOnly.FromDateTime(EDate);

            if (!_config.IsBackOffice)
            {
                //sSQL = $"SELECT  SUM(Value) AS SVal, SUM(TrainValue) AS TVal FROM NRGT " +
                //    " WHERE Store_id = {StoreID} and ShiftDate < convert(varchar(10), SDate, 112)"; //112: yyyyMMDD
                sSQL = $"SELECT  COALESCE(SUM(Value),0) AS SVal, COALESCE(SUM(TrainValue),0) AS TVal FROM CSCTrans.dbo.NRGT " +
                       $" WHERE Store_id = {StoreID} and ShiftDate < {sDate} ";
            }
            else
            {
                sSQL = $"SELECT  COALESCE(SUM(Value),0) AS SVal, COALESCE(SUM(TrainValue),0) AS TVal FROM CSCTrans.dbo.NRGT " +
                       $" WHERE ShiftDate < {sDate} ";
            }
            var data = await Connection.QueryFirstOrDefaultAsync<NRGTData>(sSQL, null, Transaction);

            if (data != null)
            {
                r.ClosingTotal = data.SVal;
                r.ClosingTrainTotal = data.TVal;
            }

            writer.WriteLine(PadR("Closing NRGT", PRINT_WIDTH / 2) + PadL(r.ClosingTotal.ToString("##,###,##0.00"), PRINT_WIDTH / 2));
            writer.WriteLine(PadR("Opening NRGT", PRINT_WIDTH / 2) + PadL(r.OpeningTotal.ToString("##,###,##0.00"), PRINT_WIDTH / 2));

            writer.WriteLine(new string('_', PRINT_WIDTH));

            writer.WriteLine(PadR("Total Sales", PRINT_WIDTH / 2) + PadL((r.ClosingTotal - r.OpeningTotal).ToString("##,###,##0.00"), PRINT_WIDTH / 2));
            writer.WriteLine(new string(' ', PRINT_WIDTH));

            writer.WriteLine(PadR("Closing NRGT-Training", PRINT_WIDTH / 2) + PadL(r.ClosingTrainTotal.ToString("##,###,##0.00"), PRINT_WIDTH / 2 - 1));
            writer.WriteLine(PadR("Opening NRGT-Training", PRINT_WIDTH / 2) + PadL(r.OpeningTrainTotal.ToString("##,###,##0.00"), PRINT_WIDTH / 2 - 1));

            writer.WriteLine(new string('_', PRINT_WIDTH));

            writer.WriteLine(PadR("Total Sales-Training", PRINT_WIDTH / 2) + PadL((r.ClosingTrainTotal - r.OpeningTrainTotal).ToString("##,###,##0.00"), PRINT_WIDTH / 2));
            writer.WriteLine(new string(' ', PRINT_WIDTH));

            return true;
        }

        public async Task<IEnumerable<int>> GetShiftNos(DateTime shiftdate)
        {
            string query;
            if (!_config.IsBackOffice)
            {
                query = @"
                        SELECT ShiftNumber
                        FROM [CSCTrans].[dbo].[Close_Head]
                        WHERE ShiftDate = @ShiftDate
                        ORDER BY Close_Num DESC";
            }
            else
            {
                query = @"
                        SELECT ShiftNumber
                        FROM [CSCMaster].[dbo].[ShiftStore]
                        ORDER BY ShiftNumber";
            }

            // Convert DateOnly to DateTime for compatibility
            var parameters = new { ShiftDate = shiftdate };
            var shiftNos = await Connection.QueryAsync<int>(query, parameters, Transaction);

            return shiftNos;
        }
        public async Task<string[]?> GetRepByTillNo(GetRepTillCloseDto model)
        {
            polShow_All_Exempt = model.policies.ShowAllExempt;
            Boolean bFound = false;
            CloseNo = model.closeNo;
            //ShiftNo = model.shiftNo;

            string filePath = "TillClose.txt";
            Boolean PrintIt = false;
            String Where_Clause;

            CloseHead closeHead = await GetCloseHead();

            Float_Amt = (closeHead.Float) ?? 0;
            Draw_Amt = (closeHead.Draw) ?? 0;

            BonusFloat = (closeHead.BonusFloat) ?? 0;
            BonusDraw = (closeHead.BonusDraw) ?? 0;
            BonusDrop = (closeHead.BonusDrop) ?? 0;
            BonusGiveAway = (closeHead.BonusGiveAway) ?? 0;

            SC_Issued = (closeHead.ScIssued) ?? 0;

            if (model.policies.UseShifts)
                Where_Clause = Collect_Close_Data(model, 0, false, StoreID).Result;
            else
                Where_Clause = Collect_Close_Data(model, 0).Result;

            // Load the Entered amounts into the recordset.
            var data = (IEnumerable<Close_Line>)await GetCloseTenders();
            if (model.policies.CountType.ToUpper() == "EACH TENDER")
            {
                //Search the tender of the close_tenders out of TillCloses
                foreach (var cl in data)
                {
                    bFound = false;
                    var tillCloses = (IEnumerable<TillClose>?)await GetTillClose();
                    if (tillCloses != null)
                    {
                        foreach (var tc in tillCloses)
                        {
                            if (tc.Tender == cl.Tender_Name)
                            {
                                tc.Entered = cl.Entered;
                                tc.System = cl.System;
                                tc.Difference = cl.Entered - cl.System;
                                if (!await UpdateTender(cl.Entered, cl.System, cl.Tender_Name))
                                {
                                    Console.WriteLine($"Error in UpdateTender");
                                }
                                bFound = true;
                                break;
                            }
                        }
                        if (!bFound)
                        {
                            TillClose tc = new();
                            tc.Tender = cl.Tender_Name;
                            tc.Entered = cl.Entered;
                            tc.System = cl.System;
                            tc.Difference = cl.Entered - cl.System;
                            if (!await AddTillClose(tc))
                            {
                                Console.WriteLine($"Error in AddTillClose");
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var cl in data)
                {
                    if (!await UpdateTender(cl.Entered, cl.System, cl.Tender_Name))
                    {
                        Console.WriteLine($"Error in UpdateTender");
                    }
                }

            }
            TC.Till_Number = closeHead.Till_Num;
            TC.Close_Num = closeHead.Close_Num;
            TC.User = closeHead.User;

            TC.CloseLines = data;
            TC.GroupNumber = (closeHead.GroupNumber) ?? 0;
            TC.GroupType = (closeHead.GroupType) ?? "";

            TC.Open_Date = closeHead.Open_Date.HasValue ? DateOnly.FromDateTime((DateTime)closeHead.Open_Date) : null;
            TC.Open_Time = closeHead.Open_Time.HasValue ? TimeOnly.FromDateTime((DateTime)closeHead.Open_Time) : null;
            TC.Close_Date = closeHead.Close_Date.HasValue ? DateOnly.FromDateTime((DateTime)closeHead.Close_Date) : null;
            TC.Close_Time = closeHead.Close_Time.HasValue ? TimeOnly.FromDateTime((DateTime)closeHead.Close_Time) : null;

            TC.ShiftNumber = closeHead.ShiftNumber;
            //TC.ShiftDate = closeHead.ShiftDate.HasValue ? DateOnly.FromDateTime((DateTime)closeHead.ShiftDate) : null;
            TC.ShiftDate = DateOnly.FromDateTime(closeHead.ShiftDate);
            TC.Float = Float_Amt;
            TC.Draw = Draw_Amt;
            TC.Drop = Drop_Conv - BonusDrop;
            TC.Payments = Payments;
            TC.ARPay = ARPay;
            TC.PayOuts = Payouts;
            TC.OverPay = OverPay;
            TC.Penny_Adj = Penny_Adj;
            TC.BonusFloat = BonusFloat;
            TC.BonusDraw = BonusDraw;
            TC.BonusDrop = BonusDrop;
            TC.BonusGiveAway = BonusGiveAway;

            TC.Credits_Issued = SC_Issued;
            TC.Close_Num = CloseNo;

            //tc.Close_Till False, Where_Clause, StoreID
            if (printTillCloseReport(model, PrintIt, Where_Clause, StoreID).Result)
            {
                //Console.WriteLine("File created successfully.");
                // Read the file as binary data
                string[] fileLines = File.ReadAllLines(filePath);

                return fileLines;
            }
            else
            {
                //Console.WriteLine("Error in creating file.");
                return null;
            }
        } // End of Product_Sales_Report

        public async Task<bool> UpdateTender(decimal? amount, decimal? system, string? tendName)
        {
            string query = $@"UPDATE CSCTrans.dbo.TillClose " +
                    $"Set [Entered] = [Entered] + @amount, Difference = [Entered] - @system " +
                    $"WHERE Tender = @tendName";
            using SqlCommand cmd = new(query, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
            cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = amount;
            cmd.Parameters.Add("@system", SqlDbType.Decimal).Value = system;
            cmd.Parameters.Add("@tendName", SqlDbType.VarChar, 20).Value = tendName;

            cmd.CommandType = CommandType.Text;
            int rowAffected = cmd.ExecuteNonQuery();

            return await Task.FromResult(rowAffected > 0);
        }

        public async Task<bool> UpdateTender(decimal? amount, string tendName)
        {
            string query = $@"UPDATE CSCTrans.dbo.TillClose " +
                    $"Set [System] = [System] + @amount, Difference = (-1)* ([System] + @amount) " +
                    $"WHERE Tender = @tendName";

            using SqlCommand cmd = new(query, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
            cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = amount;
            cmd.Parameters.Add("@tendName", SqlDbType.VarChar, 20).Value = tendName;

            cmd.CommandType = CommandType.Text;
            int rowAffected = cmd.ExecuteNonQuery();

            return await Task.FromResult(rowAffected > 0);
        }
        public async Task<CloseHead> GetCloseHead()
        {
            string query = $"Select * From CSCTrans.dbo.Close_Head Where  Close_Head.Close_Num = {CloseNo}";
            var closeHead = await Connection.QueryFirstOrDefaultAsync<CloseHead>(query, null, Transaction);

            return closeHead;
        }

        public async Task<IEnumerable<Close_Line>> GetCloseTenders()
        {
            List<Close_Line> closeTenders = new();

            string query = $"Select * From CSCTrans.dbo.Close_Tenders Where Close_Num = {CloseNo}";

            try
            {
                var data = await Connection.QueryAsync<CloseLine>(query, null, Transaction);

                foreach (var cl in data)
                {
                    var close_Line = new Close_Line();

                    close_Line.Tender_Name = cl.TENDNAME;
                    close_Line.Tender_Class = cl.TENDCLASS;
                    close_Line.Tender_Count = cl.TENDCOUNT;
                    close_Line.Sequence = cl.Sequence;
                    close_Line.Entered = cl.AMOUNT;
                    close_Line.System = cl.SYSTEM;
                    close_Line.Balance = cl.BALANCE;
                    close_Line.Exchange_Rate = cl.Exchange_Rate;
                    close_Line.Converted_System = cl.CONV_SYS;
                    close_Line.Converted_Entered = cl.CONV_AMT;
                    close_Line.Converted_Balance = cl.CONV_SYS;

                    closeTenders.Add(close_Line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCloseTenders: {ex.Message}");
                throw;
            }

            return (IEnumerable<Close_Line>)closeTenders;
        }
        public async Task<bool> GetTillCloseByDesc(string sDesc)
        {
            string query = $"Select Count(*) As Cnt From CSCTrans.dbo.TillClose WHERE Tender = '{sDesc}'";

            try
            {
                var Cnt = await Connection.QueryFirstOrDefaultAsync<int>(query, null, Transaction);
                return (Cnt > 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTillClose: {ex.Message}");
                //throw;
                return false;
            }
        }

        public async Task<IEnumerable<PayoutData>?> GetPayoutData()
        {
            string query = $"select * from CSCMaster.dbo.reasons where type = 'P' order by code";

            try
            {
                var payoutData = await Connection.QueryFirstOrDefaultAsync<PayoutData>(query, null, Transaction);
                return (IEnumerable<PayoutData>)payoutData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPayoutData: {ex.Message}");
                //throw;
                return null;
            }
        }

        public async Task<IEnumerable<TillClose>?> GetTillClose()
        {
            string query = $"Select * From CSCTrans.dbo.TillClose";

            try
            {
                var tillCloses = await Connection.QueryAsync<TillClose>(query, null, Transaction);
                return (IEnumerable<TillClose>)tillCloses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTillClose: {ex.Message}");
                //throw;
                return null;
            }
        }

        public async Task<IEnumerable<Tender>> GetTenders()
        {
            string sql = @$"SELECT  Tendmast.TendCode as [Code],
                            Tendmast.ShortDesc as [ShortName],                   
                            Tendmast.TendDesc  as [FullName],
                            Tendmast.TendClass as [Class], 
                            Tendmast.Exchange as [ExchangeRate], 
                            Tendmast.DisplaySeq as [Sequence],
                            TendCard.CardCode as [CardCode],
                            Tendmast.EXACTCHG AS ExactChange,
                            Tendmast.GIVECHG AS GiveChange,
                            Tendmast.AcceptAsPayment AS AcceptAsPayment,
                            Tendmast.GIVEASREF AS GiveAsRefund,
                            Tendmast.CANADJUST AS CanAdjust,
                            Tendmast.BANKABLE AS Bankable,
                            Tendmast.IncludeInClose AS IncludeInClose,
                            Tendmast.Put_In_Drawer AS PutInDrawer,
                            Tendmast.Signature AS Signature,
                            Tendmast.Inactive AS Inactive,
                            Tendmast.MINAMOUNT AS MinAmount,
                            Tendmast.MAXAMOUNT AS MaxAmount,
                            Tendmast.SMALLUNIT AS SmallestUnit,
                            Tendmast.PrintCopies AS PrintCopies,
                            Tendmast.INSTANCES AS Instances,
                            Tendmast.PayoutCode AS PayoutCode
                        FROM CSCMaster.dbo.TendMast LEFT JOIN CSCMaster.dbo.TendCard 
                        ON TendMast.TendCode = TendCard.TendCode 
                        ORDER BY Tendmast.DisplaySeq";

            IEnumerable<Tender> tenders = await Connection.QueryAsync<Tender>(sql, null, Transaction);
            return tenders;
        }

        public async Task<bool> AddTillClose(TillClose tc)
        {
            string query;
            if (!_config.IsBackOffice)
                query = $"INSERT CSCTrans.dbo.TillClose([StoreId],[Tender],[Count],[Entered],[System],[Difference]) " +
                        $"VALUES (@StoreId,@Tender,@Count,@Entered,@System,@Difference)";
            else
                query = $"INSERT CSCTrans.dbo.TillClose([Tender],[Count],[Entered],[System],[Difference]) " +
                        $"VALUES (@Tender,@Count,@Entered,@System,@Difference)";

            using SqlCommand cmd = new(query, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
            if (!_config.IsBackOffice)
                cmd.Parameters.Add("@StoreId", SqlDbType.VarChar, 5).Value = tc.StoreID;
            cmd.Parameters.Add("@Tender", SqlDbType.VarChar, 20).Value = tc.Tender;
            cmd.Parameters.Add("@Count", SqlDbType.Int).Value = tc.Count;
            cmd.Parameters.Add("@Entered", SqlDbType.Decimal).Value = tc.Entered;
            cmd.Parameters.Add("@System", SqlDbType.Decimal).Value = tc.System;
            cmd.Parameters.Add("@Difference", SqlDbType.Decimal).Value = tc.Difference;

            cmd.CommandType = CommandType.Text;
            int rowAffected = cmd.ExecuteNonQuery();

            return await Task.FromResult(rowAffected > 0);
        }
        public async Task<IEnumerable<TenderData>> GetSaleTendData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select SaleTend.TendName as [Tender], Count(*) as [Count], " +
                            $"Sum(SaleTend.AmtTend) as [Amount], " +
                            $"Sum(SaleTend.AmtUsed) as [Used] " +
                            $"From CSCTrans.dbo.SaleTend " +
                            $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleTend.Sale_No = SaleHead.Sale_No and SaleTend.store_id = SaleHead.store_id " +
                            $"Where (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"AND SaleHead.T_Type NOT IN ('VOID','CANCEL') AND " +
                            $"{Where_Clause} " +
                            $"Group By SaleTend.TendName";
                        var list = await Connection.QueryAsync<TenderData>(query, null, Transaction);
                        return list;
                    }
                    else
                    {
                        string query = $"Select SaleTend.TendName as [Tender], Count(*) as [Count], " +
                            $"Sum(SaleTend.AmtTend) as [Amount], " +
                            $"Sum(SaleTend.AmtUsed) as [Used] " +
                            $"From CSCTrans.dbo.SaleTend " +
                            $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleTend.Sale_No = SaleHead.Sale_No and SaleTend.store_id = SaleHead.store_id " +
                            $"Where (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                            $"AND SaleHead.T_Type NOT IN ('VOID','CANCEL') AND " +
                            $"{Where_Clause} and SaleHead.Store_id = {StoreID} " +
                            $"Group By SaleTend.TendName";
                        var list = await Connection.QueryAsync<TenderData>(query, null, Transaction);
                        return list;

                    }
                }
                else
                {
                    string query = $"Select SaleTend.TendName as [Tender], Count(*) as [Count], " +
                        $"Sum(SaleTend.AmtTend) as [Amount], " +
                        $"Sum(SaleTend.AmtUsed) as [Used] " +
                        $"From CSCTrans.dbo.SaleTend " +
                        $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleTend.Sale_No = SaleHead.Sale_No " +
                        $"Where (SaleTend.TendName <> 'Store Credit' OR SaleTend.AmtTend > 0) " +
                        $"AND SaleHead.T_Type NOT IN ('VOID','CANCEL') AND " +
                        $"{Where_Clause} " +
                        $"Group By SaleTend.TendName";
                    var list = await Connection.QueryAsync<TenderData>(query, null, Transaction);
                    return list;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetTendData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return null;
            }
        }

        public async Task<IEnumerable<TendMast>> GetInclTendData()
        {
            try
            {
                string query = $"Select TendDesc " +
                    $"From CSCMaster.dbo.TendMast " +
                    $"Where TendMast.IncludeInClose = 1";
                var list = await Connection.QueryAsync<TendMast>(query, null, Transaction);
                return list;
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetInclTendData: {ex.Message}");
                throw; // Rethrow the exception if you want to propagate it.                
            }
        }
        public async Task<decimal> GetPaymentData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.Payment), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type = 'PAYMENT' AND  " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.Payment), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type = 'PAYMENT' AND SaleHead.store_Id = {StoreID} " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;

                    }
                }
                else
                {
                    string query = $"Select COALESCE(SUM(SaleHead.Payment), 0) AS [Used] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where SaleHead.T_Type = 'PAYMENT' AND  " +
                        $"{Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetPaymentData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }

        public async Task<decimal> GetArPaymentData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.Payment), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type = 'ARPAY' AND  " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.Payment), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type = 'ARPAY' AND SaleHead.store_Id = {StoreID} " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;

                    }
                }
                else
                {
                    string query = $"Select COALESCE(SUM(SaleHead.Payment), 0) AS [Used] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where SaleHead.T_Type = 'ARPAY' AND  " +
                        $"{Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetArPaymentData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }

        public async Task<decimal> GetPennyAdjustData(string Where_Clause)
        {
            try
            {
                if (_config.IsBackOffice)
                {
                    string query = $"Select COALESCE(SUM(SaleHead.Penny_Adj), 0) AS [PennyAdj] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where + {Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetPennyAdjustData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
            return 0;
        }

        public async Task<decimal> GetPayoutsData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.PayOut), 0) AS[Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type = 'PAYOUT' AND  " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.PayOut), 0) AS[Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type = 'PAYOUT' AND SaleHead.store_Id = {StoreID} " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;

                    }
                }
                else
                {
                    string query = $"Select COALESCE(SUM(SaleHead.PayOut), 0) AS[Used] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where SaleHead.T_Type = 'PAYOUT' AND  " +
                        $"{Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetPayoutsData: {ex.Message}");
                throw; // Rethrow the exception if you want to propagate it.                
            }
        }

        public async Task<decimal> GetOverPaymentData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.OverPayment), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type IN ('SALE','REFUND')  AND  " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.OverPayment), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type IN ('SALE','REFUND') AND SaleHead.store_Id = {StoreID} " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;

                    }
                }
                else
                {
                    string query = $"Select COALESCE(SUM(SaleHead.OverPayment), 0) AS [Used] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where SaleHead.T_Type IN ('SALE','REFUND') AND  " +
                        $"{Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetOverPaymentData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }

        public async Task<decimal> GetChangePaidData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.Change), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type <> 'VOID' AND  " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(SUM(SaleHead.Change), 0) AS [Used] " +
                            $"From CSCTrans.dbo.SaleHead " +
                            $"Where SaleHead.T_Type <> 'VOID' AND SaleHead.store_Id = {StoreID} " +
                            $"{Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;

                    }
                }
                else
                {
                    string query = $"Select COALESCE(SUM(SaleHead.Change), 0) AS [Used] " +
                        $"From CSCTrans.dbo.SaleHead " +
                        $"Where SaleHead.T_Type <> 'VOID' AND  " +
                        $"{Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetChangePaidData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }
        public async Task<CashDraw?> GetCashDrawData(string StoreID, string Where_Clause, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                if (CloseNo > 0)
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            string query = $"Select Count(*) as [nDraw] " +
                                $", Sum(CashDraw.Amount) as [Draw_Total] " +
                                $", sum(CashDraw.CashBonus) as [CashBonus] " +
                                $"From CSCTrans.dbo.CashDraw " +
                                $"Where {Where_Clause} ";
                            var data = await Connection.QueryFirstOrDefaultAsync<CashDraw>(query, null, Transaction);
                            return data;
                        }
                        else
                        {
                            string query = $"Select Count(*) as [nDraw] " +
                                $", Sum(CashDraw.Amount) as [Draw_Total] " +
                                $", sum(CashDraw.CashBonus) as [CashBonus] " +
                                $"From CSCTrans.dbo.CashDraw " +
                                $"Where CashDraw.store_Id = {StoreID} AND {Where_Clause} ";
                            var data = await Connection.QueryFirstOrDefaultAsync<CashDraw>(query, null, Transaction);
                            return data;

                        }
                    }
                    else
                    {
                        string query = $"Select Count(*) as [nDraw] " +
                            $", Sum(CashDraw.Amount) as [Draw_Total] " +
                            $", sum(CashDraw.CashBonus) as [CashBonus] " +
                            $"From CSCTrans.dbo.CashDraw " +
                            $"Where {Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<CashDraw>(query, null, Transaction);
                        return data;
                    }
                }
                else
                {
                    string query = $"Select Count(*) as [nDraw] " +
                        $", Sum(CashDraw.Amount) as [Draw_Total] " +
                        $", sum(CashDraw.CashBonus) as [CashBonus] " +
                        $"From CSCTrans.dbo.CashDraw " +
                        $"Where  Draw_Date >= {DateOnly.FromDateTime(StartDate)} AND " +
                        $"       Draw_Date <= {DateOnly.FromDateTime(EndDate)} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<CashDraw>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetCashDrawData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return null;
            }
        }

        public async Task<decimal?> GetBonusGiveAwayData(GetRepTillCloseDto model, string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(sum(DiscountAmount),0) as [Bonus] " +
                            $"From CSCTrans.dbo.DiscountTender  inner join  CSCTrans.dbo.Salehead " +
                            $"ON Salehead.sale_no = Discounttender.Sale_NO AND " +
                            $"Salehead.Till= Discounttender.Till_num " +
                            $"where DiscountType = 'B' AND {Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(sum(DiscountAmount),0) as [Bonus] " +
                            $"From CSCTrans.dbo.DiscountTender  inner join  CSCTrans.dbo.Salehead " +
                            $"ON Salehead.sale_no = Discounttender.Sale_NO AND " +
                            $"Salehead.Till= Discounttender.Till_num " +
                            $"where DiscountType = 'B' AND " +
                            $"store_Id = {StoreID} AND {Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                }
                else
                {
                    string query = $"Select COALESCE(sum(DiscountAmount),0) as [Bonus] " +
                        $"From CSCTrans.dbo.DiscountTender  inner join  CSCTrans.dbo.Salehead " +
                        $"ON Salehead.sale_no = Discounttender.Sale_NO AND " +
                        $"Salehead.Till= Discounttender.Till_num " +
                        $"where DiscountType = 'B' AND {Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetBonusGiveAwayData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }

        public async Task<decimal?> GetBonusDropData(GetRepTillCloseDto model, string StoreID, string Where_Clause, DateTime StartDate, DateTime EndDate)
        {
            try
            {

                if (CloseNo > 0)
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            string query = $"Select DropLines.Tender_Name, " +
                                $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                                $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv]  " +
                                $"From CSCTrans.dbo.DropLines " +
                                $"Where Tender_name = '{model.policies.CBonusTend}'  AND {Where_Clause} " +
                                $"Group By  DropLines.Tender_Name ";
                            var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                            return data;
                        }
                        else
                        {
                            string query = $"Select DropLines.Tender_Name, " +
                                $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                                $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv]  " +
                                $"From CSCTrans.dbo.DropLines " +
                                $"Where Tender_name = '{model.policies.CBonusTend}' AND " +
                                $"DropLines.store_Id = {StoreID} AND {Where_Clause} " +
                                $"Group By DropLines.Tender_Name ";
                            var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                            return data;
                        }
                    }
                    else
                    {
                        string query = $"Select DropLines.Tender_Name, " +
                            $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                            $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv]  " +
                            $"From CSCTrans.dbo.DropLines " +
                            $"Where Tender_name = '{model.policies.CBonusTend}' AND {Where_Clause} " +
                            $"Group By DropLines.Tender_Name ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                }
                else
                {
                    string query = $"Select DropLines.Tender_Name, " +
                        $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                        $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv]  " +
                        $"From CSCTrans.dbo.DropLines " +
                        $"Where Tender_name = {model.policies.CBonusTend}  AND DropDate >= {StartDate} AND DropDate <= {EndDate} " +
                        $"Group By DropLines.Tender_Name ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetBonusDropData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }

        public async Task<decimal> GetTotalChangePaidData(string StoreID, string Where_Clause)
        {
            try
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        string query = $"Select COALESCE(Sum(SaleHead.Change),0) as [SumChg]  " +
                            $"From   CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'VOID' AND {Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                    else
                    {
                        string query = $"Select COALESCE(Sum(SaleHead.Change),0) as [SumChg]  " +
                            $"From   CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'VOID' " +
                            $" SaleHead.store_Id = {StoreID} AND {Where_Clause} ";
                        var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                        return data;
                    }
                }
                else
                {
                    string query = $"Select COALESCE(Sum(SaleHead.Change),0) as [SumChg]  " +
                        $"From   CSCTrans.dbo.SaleHead " +
                        $"Where  SaleHead.T_Type <> 'VOID' AND {Where_Clause} ";
                    var data = await Connection.QueryFirstOrDefaultAsync<decimal>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetTotalChangePaidData: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return 0;
            }
        }

        public async Task<IEnumerable<DropData>> GetDropLines(GetRepTillCloseDto model, string StoreID, string Where_Clause, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                if (CloseNo > 0)
                {

                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            string query = $"Select DropLines.Tender_Name,  " +
                                $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                                $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv] " +
                                $"From CSCTrans.dbo.DropLines " +
                                $"Where {Where_Clause} " +
                                $"Group By DropLines.Tender_Name ";
                            var data = await Connection.QueryAsync<DropData>(query, null, Transaction);
                            return data;
                        }
                        else
                        {
                            string query = $"Select DropLines.Tender_Name,  " +
                                $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                                $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv] " +
                                $"From CSCTrans.dbo.DropLines " +
                                $"Where store_Id = {StoreID} AND {Where_Clause} " +
                                $"Group By DropLines.Tender_Name ";
                            var data = await Connection.QueryAsync<DropData>(query, null, Transaction);
                            return data;
                        }
                    }
                    else
                    {
                        string query = $"Select DropLines.Tender_Name,  " +
                            $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                            $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv] " +
                            $"From CSCTrans.dbo.DropLines " +
                            $"Where {Where_Clause} " +
                            $"Group By DropLines.Tender_Name ";
                        var data = await Connection.QueryAsync<DropData>(query, null, Transaction);
                        return data;
                    }
                }
                else
                {
                    string query = $"Select DropLines.Tender_Name,  " +
                        $"COALESCE(Sum(Amount),0)      as [Drop_Amt], " +
                        $"COALESCE(Sum(Conv_Amount),0) as [Drop_Conv] " +
                        $"From CSCTrans.dbo.DropLines " +
                        $"Where DropDate >= '{StartDate}' AND DropDate <= '{EndDate}' " +
                    $"Group By DropLines.Tender_Name ";
                    var data = await Connection.QueryAsync<DropData>(query, null, Transaction);
                    return data;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log it appropriately.
                Console.WriteLine($"Error in GetDropLines: {ex.Message}");
                //throw; // Rethrow the exception if you want to propagate it.
                return null;
            }
        }
        public async Task<string> GetTerminalId()
        {
            string query = $"SELECT TerminalID FROM CSCAdmin.dbo.TerminalIds where TerminalType='Ezipin'";
            var data = await Connection.QueryFirstOrDefaultAsync<string>(query, null, Transaction);
            return data;
        }

        public async Task<bool> printTillCloseReport(GetRepTillCloseDto model, bool PrintIt, string Where_Clause, string StoreID)
        {
            bool bFinished = false;
            decimal T_Ent;
            decimal T_Sys;
            decimal T_Dif;
            decimal Exc_Adj_E;
            decimal Exc_Adj_S;
            decimal Exc_Adj_D;
            decimal Amount;
            string query, query2, query3;
            string fuelDept;

            const int PRINT_WIDTH = 40; // Update the width as needed
            string filePath = "TillClose.txt";

            // Delete the file if it already exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Create the file and write content to it
            using (StreamWriter writer = File.CreateText(filePath))
            {
                writer.WriteLine(PadC("Store:" + StoreID + "    INFONET TECHNOLOGY", PRINT_WIDTH));
                writer.WriteLine(PadC("TILL CLOSE #" + TC.Close_Num + " FOR TILL #" + TC.Till_Number, PRINT_WIDTH));
                writer.WriteLine(PadC("Opened on " + TC.Open_Date.Value.ToString("MM-dd-yyyy") + " at " + TC.Open_Time.Value.ToString("hh:mm:sstt"), PRINT_WIDTH));
                writer.WriteLine(PadC("Closed on " + TC.Close_Date.Value.ToString("MM-dd-yyyy") + " at " + TC.Close_Time.Value.ToString("hh:mm:sstt"), PRINT_WIDTH));
                if (model.policies.UseShifts)
                    writer.WriteLine(PadC("Shift " + TC.ShiftNumber + " On " + TC.ShiftDate.Value.ToString("MM-dd-yyyy"), PRINT_WIDTH));

                if (model.tillNo != 100)
                    writer.WriteLine(PadC("Cashier " + TC.User + " : " + TC.UserName, PRINT_WIDTH));

                if (Where_Clause.Length > 0)
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(PadC("REPRINT ON : " + DateTime.Now.ToString("MM-dd-yyyy") + " " + "at " + DateTime.Now.ToString("hh:mm:sstt"), PRINT_WIDTH));
                }

                writer.WriteLine(new string('_', PRINT_WIDTH));
                if (model.policies.SupportEzi)
                {
                    string TerminalId = GetTerminalId().Result;
                    writer.WriteLine(PadC("Ezipin Terminal ID: " + TerminalId, PRINT_WIDTH));
                }
                writer.WriteLine(new string(' ', PRINT_WIDTH));

                #region Tender
                if (model.policies.CountCash)
                    writer.WriteLine(PadR("Tender", 7) + PadL("Cnt", 4) + PadL("Entered", 10) + PadL("System", 10) + PadL("Diff", 9));
                else
                    writer.WriteLine(PadR("Tender", 7) + PadL("Cnt", 4) + PadL(" ", 10) + PadL("System", 9));

                writer.WriteLine(new string('_', PRINT_WIDTH));

                //var tillCloses = (IEnumerable<Close_Line>?)await GetCloseTenders();

                T_Ent = 0;
                T_Sys = 0;
                T_Dif = 0;
                Exc_Adj_E = 0;
                Exc_Adj_S = 0;
                Exc_Adj_D = 0;

                if (TC.CloseLines != null)
                {
                    foreach (var cl in TC.CloseLines)
                    {
                        if (model.policies.CountCash)
                            writer.WriteLine(PadR(cl.Tender_Name ?? 0.ToString(), 7) + PadL(cl.Tender_Count!.Value.ToString(), 4) +
                            //PadL(cl.ENTERED!.Value.ToString("######0.00"), 10) +
                            PadL(cl.Entered.ToString("######0.00"), 10) +
                            PadL(cl.System.ToString("######0.00"), 10) +
                            PadL(cl.Balance.ToString("######0.00"), 9));
                        else
                        {
                            //WriteToFile(writer,PadR(cl.TENDNAME!.ToString(), 7) + PadL(cl.TENDCOUNT!.Value.ToString(), 4) +
                            //PadL(" ", 10) +
                            //PadL(cl.SYSTEM!.Value.ToString("######0.00"), 9));
                            string cName;
                            if (cl.Tender_Name!.Length > 7)
                                cName = cl.Tender_Name.Substring(0, 6);
                            else
                                cName = cl.Tender_Name!.ToString();
                            writer.WriteLine(PadR(cName, 7) + PadL(cl.Tender_Count!.Value.ToString(), 4) +
                            PadL(" ", 10) +
                            PadL(cl.System.ToString("######0.00"), 9));

                        }
                        //T_Ent += cl.ENTERED!.Value;
                        T_Ent += cl.Entered;
                        T_Sys += cl.System;
                        T_Dif += cl.Balance;

                        //Exc_Adj_E -= cl.ENTERED!.Value * (1 - cl.Exchange_Rate!.Value);
                        Exc_Adj_E -= cl.Entered * (1 - cl.Exchange_Rate);
                        Exc_Adj_S -= cl.System * (1 - cl.Exchange_Rate);
                        Exc_Adj_D -= cl.Balance * (1 - cl.Exchange_Rate);
                    }
                }
                writer.WriteLine(new string('_', PRINT_WIDTH));

                // These are the raw totals of the face values of the tenders.
                if (model.policies.CountCash)
                {
                    writer.WriteLine(PadR("TOTALS", 11) +
                        PadL(T_Ent.ToString("######0.00"), 10) +
                        PadL(T_Sys.ToString("######0.00"), 10) +
                        PadL(T_Dif.ToString("######0.00"), 9));

                    // Show Exchange Adjustments (may be positive or negative)
                    if (Exc_Adj_E != 0 || Exc_Adj_S != 0 || Exc_Adj_D != 0)
                    {
                        writer.WriteLine(PadR("_EXCH_ADJ", 11) +
                            PadL(Exc_Adj_E.ToString("######0.00"), 10) +
                            PadL(Exc_Adj_S.ToString("######0.00"), 10) +
                            PadL(Exc_Adj_D.ToString("######0.00"), 9));
                    }

                    // Subtract the float
                    if (Float_Amt != 0)
                    {
                        Amount = (-1) * Float_Amt;
                        writer.WriteLine(PadR("-Float", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }
                    // Remove Store Credits issued
                    if (Credits_Issued != 0)
                    {
                        Amount = Credits_Issued;
                        writer.WriteLine(PadR("-Credits", 11) +
                            PadL(Credits_Issued.ToString("######0.00"), 10) +
                            PadL(Credits_Issued.ToString("######0.00"), 10));
                    }

                    // Subtract Payments Received
                    if (Payments != 0)
                    {
                        Amount = (-1) * Payments;
                        writer.WriteLine(PadR("-Payments", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Subtract AR Payments Received
                    if (ARPay != 0)
                    {
                        Amount = (-1) * ARPay;
                        writer.WriteLine(PadR("-AR Pay", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Subtract AR Payments Received
                    if (OverPay != 0)
                    {
                        Amount = (-1) * OverPay;
                        writer.WriteLine(PadR("-OverPay", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // remove the till Draws
                    if (Draw_Amt != 0)
                    {
                        Amount = (-1) * Draw_Amt;
                        writer.WriteLine(PadR("-Draws", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Add the Till Drops to the Safe
                    if (Drop_Amt != 0)
                    {
                        Amount = Drop_Amt;
                        writer.WriteLine(PadR("+Drops", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Add PayOuts Made
                    if (Payouts != 0)
                    {
                        Amount = Payouts;
                        writer.WriteLine(PadR("Payouts", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Add Penny Adjustments
                    if (Penny_Adj != 0)
                    {
                        Amount = (-1) * Penny_Adj;
                        writer.WriteLine(PadR("Penny Adjustment", 11) +
                            PadL(Amount.ToString("######0.00"), 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    if (model.policies.CashBonus)
                    {
                        // Show Exchange Adjustments (may be positive or negative)
                        if (BonusFloat != 0 || BonusDraw != 0 || BonusDrop != 0)
                        {
                            writer.WriteLine(model.policies.CBonusName + ":");
                        }
                        // For Cash bonus float
                        if (BonusFloat != 0)
                        {
                            Amount = (-1) * BonusFloat;
                            writer.WriteLine(PadR("BonusFloat", 11) +
                                PadL(Amount.ToString("######0.00"), 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                        // Remove Cash bonus draws
                        if (BonusDraw != 0)
                        {
                            Amount = (-1) * BonusDraw;
                            writer.WriteLine(PadR("BonusDraw", 1) +
                                PadL(Amount.ToString("######0.00"), 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                        // Add cash Bonus Drops to the safe
                        if (BonusDrop != 0)
                        {
                            Amount = BonusDrop;
                            writer.WriteLine(PadR("BonusDrop", 11) +
                                PadL(Amount.ToString("######0.00"), 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                        // Add cash Bonus Giveaway to the safe
                        if (BonusGiveAway != 0)
                        {
                            Amount = BonusGiveAway;
                            writer.WriteLine(PadR("BonusGiveAway", 11) +
                                PadL(Amount.ToString("######0.00"), 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                    }
                }
                else
                {
                    // Totals
                    writer.WriteLine(PadR("TOTALS", 11) +
                        PadL(" ", 10) +
                        PadL(T_Sys.ToString("######0.00"), 10));

                    // Show Exchange Adjustments (may be positive or negative)
                    if (Exc_Adj_E != 0 || Exc_Adj_S != 0 || Exc_Adj_D != 0)
                    {
                        writer.WriteLine(PadR("_EXCH_ADJ", 11) +
                            PadL(" ", 10) +
                            PadL(Exc_Adj_S.ToString("######0.00"), 10));
                    }
                    // Subtract the float
                    if (Float_Amt != 0)
                    {
                        Amount = (-1) * Float_Amt;
                        writer.WriteLine(PadR("-Float", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }
                    // Remove Store Credits issued
                    if (Credits_Issued != 0)
                    {
                        Amount = Credits_Issued;
                        writer.WriteLine(PadR("-Credits", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Subtract Payments Received
                    if (Payments != 0)
                    {
                        Amount = (-1) * Payments;
                        writer.WriteLine(PadR("-Payments", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Subtract AR Payments Received
                    if (ARPay != 0)
                    {
                        Amount = (-1) * ARPay;
                        writer.WriteLine(PadR("-AR Pay", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Subtract AR Payments Received
                    if (OverPay != 0)
                    {
                        Amount = (-1) * OverPay;
                        writer.WriteLine(PadR("-OverPay", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // remove the till Draws
                    if (Draw_Amt != 0)
                    {
                        Amount = (-1) * Draw_Amt;
                        writer.WriteLine(PadR("-Draws", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Add the Till Drops to the Safe
                    if (Drop_Amt != 0)
                    {
                        Amount = Drop_Amt;
                        writer.WriteLine(PadR("+Drops", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Add PayOuts Made
                    if (Payouts != 0)
                    {
                        Amount = Payouts;
                        writer.WriteLine(PadR("Payouts", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    // Add Penny Adjustments
                    if (Penny_Adj != 0)
                    {
                        Amount = (-1) * Penny_Adj;
                        writer.WriteLine(PadR("Penny Adjustment", 11) +
                            PadL(" ", 10) +
                            PadL(Amount.ToString("######0.00"), 10));
                    }

                    if (model.policies.CashBonus)
                    {
                        // Show Exchange Adjustments (may be positive or negative)
                        if (BonusFloat != 0 || BonusDraw != 0 || BonusDrop != 0)
                        {
                            writer.WriteLine(model.policies.CBonusName + ":");
                        }
                        // For Cash bonus float
                        if (BonusFloat != 0)
                        {
                            Amount = (-1) * BonusFloat;
                            writer.WriteLine(PadR("BonusFloat", 11) +
                                PadL(" ", 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                        // Remove Cash bonus draws
                        if (BonusDraw != 0)
                        {
                            Amount = (-1) * BonusDraw;
                            writer.WriteLine(PadR("BonusDraw", 11) +
                                PadL(" ", 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                        // Add cash Bonus Drops to the safe
                        if (BonusDrop != 0)
                        {
                            Amount = BonusDrop;
                            writer.WriteLine(PadR("BonusDrop", 11) +
                                PadL(" ", 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                        // Add cash Bonus Giveaway to the safe
                        if (BonusGiveAway != 0)
                        {
                            Amount = BonusGiveAway;
                            writer.WriteLine(PadR("BonusGiveAway", 11) +
                                PadL(" ", 10) +
                                PadL(Amount.ToString("######0.00"), 10));
                        }
                    }
                }

                // To include cash Bonus
                T_Ent = T_Ent + Exc_Adj_E + Credits_Issued - Float_Amt - BonusFloat - Draw_Amt - BonusDraw +
                    Drop_Amt + BonusDrop + BonusGiveAway - Payments - ARPay + Payouts - OverPay - Penny_Adj;

                T_Sys = T_Sys + Exc_Adj_S + Credits_Issued - Float_Amt - BonusFloat - Draw_Amt - BonusDraw +
                    Drop_Amt + BonusDrop + BonusGiveAway - Payments - ARPay + Payouts - OverPay - Penny_Adj;

                T_Dif += Exc_Adj_D;

                // Totals after all adjustments
                if (Exc_Adj_E != 0 || Exc_Adj_S != 0 || Exc_Adj_D != 0 || +
                    Credits_Issued != 0 || OverPay != 0 || +
                    Float_Amt != 0 || Draw_Amt != 0 || Drop_Amt != 0 || +
                    BonusFloat != 0 || BonusDraw != 0 || BonusDrop != 0 || BonusGiveAway != 0)
                {
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    if (model.policies.CountCash)
                    {
                        writer.WriteLine(PadR("TOTALS", 11) +
                                    PadL(T_Ent.ToString("######0.00"), 10) +
                                    PadL(T_Sys.ToString("######0.00"), 10) +
                                    PadL(T_Dif.ToString("######0.00"), 9));
                    }
                    else
                    {
                        writer.WriteLine(PadR("TOTALS", 25) +
                                    PadL(T_Sys.ToString("##,###,##0.00"), 13));
                    }
                }
                #endregion Tender

                #region Transaction
                // Summarize the transactions by type
                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(PadR("Transaction Summary", 20));
                writer.WriteLine(new string('_', PRINT_WIDTH));

                if (Where_Clause.Length == 0)
                {
                    query = $"Select SaleHead.T_Type as [TType], Count(*) as [Transactions], " +
                            $"       Sum(SALE_AMT) As [Amount], " +
                            $"       Sum(PAYMENT) as [Pays] " +
                            $"From   CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' " +
                            $" and SaleHead.Till = {TC.Till_Number} " +
                            $" Group By SaleHead.T_Type ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"Select SaleHead.T_Type as [TType], Count(*) as [Transactions], " +
                                    $"       Sum(SALE_AMT) As [Amount], " +
                                    $"       Sum(PAYMENT) as [Pays] " +
                                    $"From   CSCTrans.dbo.SaleHead " +
                                    $"Where  SaleHead.T_Type <> 'CANCEL' AND {Where_Clause} " +
                                    $" Group By SaleHead.T_Type ";
                        }
                        else
                        {
                            query = $"Select SaleHead.T_Type as [TType], Count(*) as [Transactions], " +
                                    $"       Sum(SALE_AMT) As [Amount], " +
                                    $"       Sum(PAYMENT) as [Pays] " +
                                    $"From   CSCTrans.dbo.SaleHead " +
                                    $"Where  SaleHead.T_Type <> 'CANCEL' AND " +
                                    $" SaleHead.Store_Id = {StoreID} AND {Where_Clause} " +
                                    $" Group By SaleHead.T_Type ";
                        }
                    }
                    else
                    {
                        query = $"Select SaleHead.T_Type as [TType], Count(*) as [Transactions], " +
                                $"       Sum(SALE_AMT) As [Amount], " +
                                $"       Sum(PAYMENT) as [Pays] " +
                                $"From   CSCTrans.dbo.SaleHead " +
                                $"Where  SaleHead.T_Type <> 'CANCEL' AND {Where_Clause} " +
                                $" Group By SaleHead.T_Type ";
                    }
                }
                var list = await Connection.QueryAsync<SaleData>(query, null, Transaction);

                foreach (var item in list)
                {
                    string tName;
                    switch (item.TTYPE)
                    {
                        case "PATP_APP":
                            tName = "P@P Sale";
                            break;
                        case "PATP_C":
                            tName = "P@P Void";
                            break;
                        //case "REFUND"
                        //    break;
                        //case "SALE":
                        //    break;
                        //case "VOID":
                        //    break;
                        //case "ARPAY":
                        //    break;
                        //case "PAYOUT":
                        //    break;
                        case "BTL RTN":
                            tName = "Bottle Return";
                            break;
                        //case "RUNAWAY":
                        //    break;
                        //case "PUMPTEST":
                        //    break;
                        default:
                            tName = item.TTYPE;
                            break;
                    }

                    writer.WriteLine(PadR(tName, 20) +
                                PadL(item.Transactions.ToString("######0"), 8) +
                                PadL((item.Amount + item.Pays).ToString("######0.00"), 12));
                }

                // Summarize the transactions by type
                if (Where_Clause.Length == 0)
                {
                    query = $"Count(*) as [Transactions] " +
                            $"From   CSCTrans.dbo.SaleHead " +
                            $"Where  SaleHead.T_Type <> 'CANCEL' " +
                            $" and SaleHead.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"Select Count(*) as [Transactions] " +
                                    $"From   CSCTrans.dbo.SaleHead " +
                                    $"Where  SaleHead.T_Type <> 'CANCEL' AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"Select Count(*) as [Transactions] " +
                                    $"From   CSCTrans.dbo.SaleHead " +
                                    $"Where  SaleHead.T_Type <> 'CANCEL' AND " +
                                    $" SaleHead.Store_Id = {StoreID} AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"Select Count(*) as [Transactions] " +
                                $"From   CSCTrans.dbo.SaleHead " +
                                $"Where  SaleHead.T_Type <> 'CANCEL' AND {Where_Clause} ";
                    }
                }
                var NoTrans = await Connection.QueryFirstOrDefaultAsync<int>(query, null, Transaction);

                writer.WriteLine(new string('_', PRINT_WIDTH));
                writer.WriteLine(PadR("Total Transaction", 20) +
                    PadL(NoTrans.ToString("######0"), 8));
                #endregion Transaction

                #region Cash Balance section
                double dblTotalCash;
                double dblTotalChange;
                double? dblTotalPayout;
                double? dblTotalDrop;
                double? dblTotalBottleReturn;
                double dblComputedBalance;
                double dblActualbalance;
                double dblTotalPayment;
                double dblTotalARPay;
                double? dblTotalDraw;
                double dblBonusDraw;
                double dblBonusDrop;
                double? dblBonusCash;
                double dblComputedBonusBalance;
                double dblActualBonusBalance;

                writer.WriteLine(new string(' ', PRINT_WIDTH));
                writer.WriteLine(PadR("Cash Balance Summary", PRINT_WIDTH));
                writer.WriteLine(new string('_', PRINT_WIDTH));

                if (Float_Amt != 0)
                {
                    writer.WriteLine(PadR("Till Open Cash: (+)", 30) +
                        PadL(Float_Amt.ToString("######0.00"), 10));
                }
                dblTotalCash = 0;
                dblTotalChange = 0;
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                            $"FROM CSCTrans.dbo.SaleTend INNER JOIN SaleHead ON CSCTrans.dbo.SaleTend.Sale_No = SaleHead.Sale_No " +
                            $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND tendname = '{model.policies.BaseCurr}' " +
                            $" AND SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $" AND Saletend.Store_Id = Salehead.Store_Id " +
                                    $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND tendname = '{model.policies.BaseCurr}' " +
                                    $" AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $" AND Saletend.Store_Id = Salehead.Store_Id " +
                                    $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND tendname = '{model.policies.BaseCurr}' " +
                                    $" AND SaleHead.Store_Id = {StoreID} AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON SaleTend.Sale_No = SaleHead.Sale_No " +
                                $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') AND tendname = '{model.policies.BaseCurr}' " +
                                $" AND {Where_Clause} ";
                    }
                }
                dblTotalCash = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction) ?? default;

                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT sum(COALESCE(CHANGE,0)) as [SumChange] " +
                            $"FROM CSCTrans.dbo.SaleHead " +
                            $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                            $" AND SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT sum(COALESCE(CHANGE,0)) as [SumChange] " +
                                    $"FROM CSCTrans.dbo.SaleHead " +
                                    $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                    $" AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT sum(COALESCE(CHANGE,0)) as [SumChange] " +
                                    $"FROM CSCTrans.dbo.SaleHead " +
                                    $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                    $" AND SaleHead.Store_Id = {StoreID} AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT sum(COALESCE(CHANGE,0)) as [SumChange] " +
                                $"FROM CSCTrans.dbo.SaleHead " +
                                $"WHERE SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                $" AND {Where_Clause} ";
                    }
                }
                dblTotalChange = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction) ?? default;

                writer.WriteLine(PadR("Cash Sales: (+)", 30) +
                    PadL((dblTotalCash + dblTotalChange).ToString("######0.00"), 10));
                #endregion Cash Balance section

                #region Cash Payment
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                            $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $" SaleTend.Sale_No = SaleHead.Sale_No " +
                            $"WHERE  SaleHead.T_Type = 'PAYMENT'  and tendname = '{model.policies.BaseCurr}' " +
                            $" AND SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $" AND Saletend.Store_Id = Salehead.Store_Id " +
                                    $"WHERE  SaleHead.T_Type = 'PAYMENT'  and tendname = '{model.policies.BaseCurr}' " +
                                    $" AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $" AND Saletend.Store_Id = Salehead.Store_Id " +
                                    $"WHERE  SaleHead.T_Type = 'PAYMENT'  and tendname = '{model.policies.BaseCurr}' " +
                                    $" AND SaleHead.Store_Id = {StoreID} AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                                $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                $"WHERE  SaleHead.T_Type = 'PAYMENT'  and tendname = '{model.policies.BaseCurr}' " +
                                $" AND {Where_Clause} ";
                    }
                }
                CashPymnt cashPymnt = await Connection.QueryFirstOrDefaultAsync<CashPymnt>(query, null, Transaction);
                double dblCashPymnt = cashPymnt.sumAmount + cashPymnt.sumChange;
                if (dblCashPymnt != 0)
                {
                    writer.WriteLine(PadR("Cash Payment: (+)", 30) +
                        PadL(dblCashPymnt.ToString("######0.00"), 10));
                }
                #endregion Cash Payment

                #region Cash AR Payment
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                            $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                            $" SaleTend.Sale_No = SaleHead.Sale_No " +
                            $"WHERE SaleHead.T_Type = 'ARPAY' AND tendname = {model.policies.BaseCurr} " +
                            $" AND SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $" AND Saletend.Store_Id = Salehead.Store_Id " +
                                    $"WHERE SaleHead.T_Type = 'ARPAY' AND tendname = {model.policies.BaseCurr} " +
                                    $" AND {Where_Clause} ";

                        }
                        else
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $" AND Saletend.Store_Id = Salehead.Store_Id " +
                                    $"WHERE SaleHead.T_Type = 'ARPAY' AND tendname = {model.policies.BaseCurr} " +
                                    $" AND SaleHead.Store_Id = {StoreID} AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT sum(COALESCE(amttend,0)) as [sumAmount], sum(COALESCE(Change,0)) as [sumChange] " +
                                $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                $"WHERE SaleHead.T_Type = 'ARPAY' AND tendname = '{model.policies.BaseCurr}' " +
                                $" AND {Where_Clause} ";
                    }
                }
                CashPymnt cashArPymnt = (CashPymnt)await Connection.QueryFirstOrDefaultAsync<CashPymnt>(query, null, Transaction);

                double dblCashArPymnt = cashArPymnt.sumAmount + cashArPymnt.sumChange;

                if (dblCashArPymnt != 0)
                {
                    writer.WriteLine(PadR("Cash AR Pay: (+)", 30) +
                        PadL(dblCashArPymnt.ToString("######0.00"), 10));
                }
                #endregion Cash AR Pay

                #region Cash Draw
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                            $"FROM CSCTrans.dbo.Cashdraw " +
                            $"WHERE SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.Cashdraw " +
                                    $"WHERE {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.Cashdraw " +
                                    $"WHERE {Where_Clause} AND SaleHead.Store_Id = {StoreID} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                                $"FROM CSCTrans.dbo.Cashdraw " +
                                $"WHERE {Where_Clause} ";
                    }
                }
                dblTotalDraw = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction);

                if (dblTotalDraw != 0)
                {
                    writer.WriteLine(PadR("Cash Draw: (+)", 30) +
                        PadL(dblTotalDraw!.Value.ToString("######0.00"), 10));
                }
                #endregion Cash Draw

                #region Cash Payout
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT   SUM(COALESCE(PayOut,0)) AS [sumAmount] " +
                            $"FROM CSCTrans.dbo.SaleHead " +
                            $"WHERE SaleHead.T_Type = 'PAYOUT' and SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.SaleHead " +
                                    $"WHERE SaleHead.T_Type = 'PAYOUT' AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.SaleHead " +
                                    $"WHERE SaleHead.T_Type = 'PAYOUT' AND SaleHead.Store_Id = {StoreID} " +
                                    $"AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                $"FROM CSCTrans.dbo.SaleHead " +
                                $"WHERE SaleHead.T_Type = 'PAYOUT' " +
                                $"AND {Where_Clause} ";
                    }
                }
                dblTotalPayout = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction);

                if (dblTotalPayout != 0)
                {
                    writer.WriteLine(PadR("Cash Payouts: (-)", 30) +
                        PadL(dblTotalPayout!.Value.ToString("######0.00"), 10));
                }
                #endregion Cash Payout

                #region Cash Drop
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT SUM(COALESCE(amount,0)) AS [sumAmount] " +
                            $"FROM CSCTrans.dbo.DropLines " +
                            $"WHERE Tender_name = '{model.policies.BaseCurr}' and SaleTend.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.DropLines " +
                                    $"WHERE Tender_name = '{model.policies.BaseCurr}' " +
                                    $"AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.DropLines " +
                                    $"WHERE Tender_name = '{model.policies.BaseCurr}' AND SaleHead.Store_Id = {StoreID} " +
                                    $"AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT COALESCE(Sum(amount),0) AS [sumAmount] " +
                                $"FROM CSCTrans.dbo.DropLines " +
                                $"WHERE Tender_name = '{model.policies.BaseCurr}' " +
                                $"AND {Where_Clause} ";
                    }
                }
                dblTotalDrop = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction);

                if (dblTotalDrop != 0)
                {
                    writer.WriteLine(PadR("Cash Drop: (-)", 30) +
                        PadL(dblTotalDrop!.Value.ToString("######0.00"), 10));
                }
                #endregion Cash Drop

                #region Bottle Return
                if (Where_Clause.Length == 0)
                {
                    query = $"SELECT COALESCE(Sum(SALE_AMT),0) AS [sumAmount] " +
                            $"FROM CSCTrans.dbo.SaleHead " +
                            $"WHERE SaleHead.T_Type = 'BTL RTN' " +
                            $" AND SaleHead.Till = {TC.Till_Number} ";
                }
                else
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT COALESCE(Sum(SALE_AMT),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.SaleHead " +
                                    $"WHERE SaleHead.T_Type = 'BTL RTN' " +
                                    $" AND {Where_Clause} ";
                        }
                        else
                        {
                            query = $"SELECT COALESCE(Sum(SALE_AMT),0) AS [sumAmount] " +
                                    $"FROM CSCTrans.dbo.SaleHead " +
                                    $"WHERE SaleHead.T_Type = 'BTL RTN' " +
                                    $" AND SaleHead.Store_Id = {StoreID} " +
                                    $" AND {Where_Clause} ";
                        }
                    }
                    else
                    {
                        query = $"SELECT COALESCE(Sum(SALE_AMT),0) AS [sumAmount] " +
                                $"FROM CSCTrans.dbo.SaleHead " +
                                $"WHERE SaleHead.T_Type = 'BTL RTN' " +
                                $"AND {Where_Clause} ";
                    }
                }
                dblTotalBottleReturn = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction);

                if (dblTotalBottleReturn != 0)
                {
                    writer.WriteLine(PadR("Bottle Return", 30) +
                        PadL(dblTotalBottleReturn!.Value.ToString("######0.00"), 10));
                }

                dblComputedBalance = Convert.ToDouble(Float_Amt) + (dblTotalCash + dblTotalChange) +
                    dblCashArPymnt + dblTotalDraw!.Value - dblTotalPayout!.Value - dblTotalDrop!.Value + dblTotalBottleReturn!.Value;

                writer.WriteLine(PadR("Computed BALANCE:", 30) +
                    PadL(dblComputedBalance.ToString("######0.00"), 10));
                dblActualbalance = 0;
                if (TC.CloseLines != null)
                {
                    foreach (var CL in TC.CloseLines)
                    {
                        if (model.policies.CountCash)
                        {
                            if (CL.Tender_Name == model.policies.BaseCurr)
                            {
                                dblActualbalance = Convert.ToDouble(CL.Entered);
                                break;
                            }
                        }
                    }
                }
                writer.WriteLine(PadR("Actual Till Close Cash: (-)", 30) +
                                PadL(dblActualbalance.ToString("######0.00"), 10));
                if ((dblComputedBalance - dblActualbalance) > 0)
                {
                    writer.WriteLine(PadR("Cash Short:", 30) +
                                    PadL(Math.Abs(dblComputedBalance - dblActualbalance).ToString("(######0.00)"), 10));
                }
                else if ((dblComputedBalance - dblActualbalance) < 0)
                {
                    writer.WriteLine(PadR("Cash Over:", 30) +
                                    PadL(Math.Abs(dblComputedBalance - dblActualbalance).ToString("######0.00"), 10));
                }

                writer.WriteLine(new string('_', PRINT_WIDTH));
                #endregion Bottle Return

                #region Cash Bonus
                if (model.policies.CashBonus)
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(PadR("Cash Bonus Balance Summary:", PRINT_WIDTH));
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    if (TC.BonusFloat != 0)
                    {
                        writer.WriteLine(PadR("Till Open Cash Bonus: (+)", 30) +
                                        PadL(TC.BonusFloat.ToString("######0.00"), 10));
                    }
                    dblBonusCash = 0;
                    if (Where_Clause.Length == 0)
                    {
                        query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                $" SaleTend.Sale_No = SaleHead.Sale_No  " +
                                $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                $" AND tendname = '{model.policies.CBonusTend}' " +
                                $" AND SaleHead.Till = {TC.Till_Number} ";
                    }
                    else
                    {
                        if (!_config.IsBackOffice)
                        {
                            if (StoreID == "ALL")
                            {
                                query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                        $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $" SaleTend.Sale_No = SaleHead.Sale_No AND SaleTend.Store_ID = SaleHead.Store_ID " +
                                        $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                        $" AND tendname = '{model.policies.CBonusTend}' " +
                                        $" AND {Where_Clause} ";
                            }
                            else
                            {
                                query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                        $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $" SaleTend.Sale_No = SaleHead.Sale_No AND SaleTend.Store_ID = SaleHead.Store_ID " +
                                        $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                        $" AND tendname = '{model.policies.CBonusTend}' " +
                                        $" AND SaleHead.Store = {StoreID} " +
                                        $" AND {Where_Clause} ";
                            }
                        }
                        else
                        {
                            query = $"SELECT sum(COALESCE(amttend,0)) as [SumCash] " +
                                    $"FROM CSCTrans.dbo.SaleTend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                    $" SaleTend.Sale_No = SaleHead.Sale_No " +
                                    $"WHERE  SaleHead.T_Type IN ('SALE','REFUND','EXCHANGE') " +
                                    $" AND tendname = '{model.policies.CBonusTend}' " +
                                    $" AND {Where_Clause} ";
                        }
                    }
                    dblBonusCash = await Connection.QueryFirstOrDefaultAsync<double?>(query, null, Transaction) ?? default;

                    writer.WriteLine(PadR("Cash Bonus Sales: (+)", 30) +
                        PadL(dblBonusCash!.Value.ToString("######0.00"), 10));

                    if (TC.BonusDraw != 0)
                    {
                        writer.WriteLine(PadR("Cash Bonus Draw: (+)", 30) +
                            PadL(TC.BonusDraw.ToString("######0.00"), 10));
                    }
                    if (TC.BonusDrop != 0)
                    {
                        writer.WriteLine(PadR("Cash Bonus Drop: (-)", 30) +
                            PadL(TC.BonusDrop.ToString("######0.00"), 10));
                    }
                    if (TC.BonusGiveAway != 0)
                    {
                        writer.WriteLine(PadR("Cash Bonus Given: (-)", 30) +
                            PadL(TC.BonusGiveAway.ToString("######0.00"), 10));

                    }
                    dblComputedBonusBalance = Convert.ToDouble(TC.BonusFloat) + dblBonusCash!.Value + Convert.ToDouble(TC.BonusDraw) - Convert.ToDouble(TC.BonusDrop) - Convert.ToDouble(TC.BonusGiveAway);

                    writer.WriteLine(PadR("Computed BALANCE:", 30) +
                        PadL(dblComputedBonusBalance.ToString("######0.00"), 10));

                    dblActualBonusBalance = 0;

                    if (TC.CloseLines != null)
                    {
                        foreach (var CL in TC.CloseLines)
                        {
                            if (model.policies.CountCash)
                            {
                                if (CL.Tender_Name == model.policies.BaseCurr)
                                {
                                    dblActualBonusBalance = Convert.ToDouble(CL.Entered);
                                    break;
                                }
                            }
                        }
                    }
                    writer.WriteLine(PadR("Actual Till Close Cash: (-)", 30) +
                                    PadL(dblActualBonusBalance.ToString("######0.00"), 10));
                    if ((dblComputedBonusBalance - dblActualBonusBalance) > 0)
                    {
                        writer.WriteLine(PadR("Cash Short:", 30) +
                                        PadL(Math.Abs(dblComputedBonusBalance - dblActualBonusBalance).ToString("(######0.00)"), 10));
                    }
                    else if ((dblComputedBonusBalance - dblActualBonusBalance) < 0)
                    {
                        writer.WriteLine(PadR("Cash Over:", 30) +
                                        PadL(Math.Abs(dblComputedBonusBalance - dblActualBonusBalance).ToString("######0.00"), 10));
                    }
                    writer.WriteLine(new string('_', PRINT_WIDTH));

                }
                #endregion Cash Bonus

                #region Payouts;
                double dblPayout;
                double dblTendPayout;
                int intTendCount;
                int intPayoutCount;
                double PayoutTotal = 0;
                double PayoutCount = 0;
                int cnt = 0;

                query = "SELECT * FROM CSCMaster.dbo.reasons WHERE type = 'P' ORDER BY code ";
                IEnumerable<Reason> reasons = await Connection.QueryAsync<Reason>(query, null, Transaction);

                foreach (Reason rl in reasons)
                {
                    if (Where_Clause.Length == 0)
                    {
                        query = $"SELECT   count(COALESCE(payout,0)) as [Cnt] ,COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                $"FROM     CSCTrans.dbo.SaleHead " +
                                $"WHERE    SaleHead.T_Type = 'PAYOUT' AND reason_type = 'P' AND reason = '{rl.Code}' " +
                                $" AND Till= {TC.Till_Number} ";
                    }
                    else
                    {
                        if (!_config.IsBackOffice)
                        {
                            if (StoreID == "ALL")
                            {
                                query = $"SELECT   count(COALESCE(payout,0)) as [Cnt],COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                        $"FROM     CSCTrans.dbo.SaleHead " +
                                        $"WHERE    SaleHead.T_Type = 'PAYOUT' AND reason_type = 'P' AND reason = '{rl.Code}' " +
                                        $" AND {Where_Clause} ";
                            }
                            else
                            {
                                query = $"SELECT   count(COALESCE(payout,0)) as [Cnt],COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                        $"FROM     CSCTrans.dbo.SaleHead " +
                                        $"WHERE    SaleHead.T_Type = 'PAYOUT' AND reason_type = 'P' AND reason = '{rl.Code}' " +
                                        $" AND SaleHead.Store_Id = {StoreID} " +
                                        $" AND {Where_Clause} ";
                            }
                        }
                        else
                        {
                            query = $"SELECT   count(COALESCE(payout,0)) as [Cnt],COALESCE(Sum(PayOut),0) AS [sumAmount] " +
                                    $"FROM     CSCTrans.dbo.SaleHead " +
                                    $"WHERE    SaleHead.T_Type = 'PAYOUT' AND reason_type = 'P' AND reason = '{rl.Code}' " +
                                    $" AND {Where_Clause} ";
                        }
                    }
                    Payout? payout = (Payout?)await Connection.QueryFirstOrDefaultAsync<Payout>(query, null, Transaction);
                    if (payout != null)
                    {
                        dblPayout = payout.sumAmount;
                        intPayoutCount = payout.Cnt;
                        if (intPayoutCount > 0)
                            cnt += 1;
                    }
                    else
                    {
                        intPayoutCount = 0;
                        dblPayout = 0;
                    }
                    dblTendPayout = 0;
                    intTendCount = 0;

                    if (cnt == 1)
                    {
                        writer.WriteLine(new string(' ', PRINT_WIDTH));
                        writer.WriteLine(PadR("Payout Summary", PRINT_WIDTH));
                        writer.WriteLine(new string('_', PRINT_WIDTH));
                        cnt += 1;
                    }

                    query = $"SELECT tenddesc from CSCMaster.dbo.tendmast where payoutcode = {rl.Code} ";
                    IEnumerable<string> tends = await Connection.QueryAsync<string>(query, null, Transaction);

                    if (tends != null && tends.Any())
                    {
                        foreach (string desc in tends)
                        {
                            if (Where_Clause.Length == 0)
                            {
                                query = $"SELECT count(amttend) as [CntPayout], COALESCE(Sum(amttend),0) AS [sumAmount] " +
                                        $"FROM CSCTrans.dbo.Saletend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                        $"SaleTend.Sale_No = SaleHead.Sale_No  where  tendname = '{desc}' " +
                                        $" AND Till= {TC.Till_Number} ";
                            }
                            else
                            {
                                if (!_config.IsBackOffice)
                                {
                                    if (StoreID == "ALL")
                                    {
                                        query = $"SELECT count(amttend) as [CntPayout], COALESCE(Sum(amttend),0) AS [sumAmount] " +
                                                $"FROM CSCTrans.dbo.Saletend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                                $"SaleTend.Sale_No = SaleHead.Sale_No  where  tendname = '{desc}' " +
                                                $" AND Till= {TC.Till_Number}  AND {Where_Clause} ";
                                    }
                                    else
                                    {
                                        query = $"SELECT count(amttend) as [CntPayout], COALESCE(Sum(amttend),0) AS [sumAmount] " +
                                                $"FROM CSCTrans.dbo.Saletend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                                $"SaleTend.Sale_No = SaleHead.Sale_No  where  tendname = '{desc}' " +
                                                $" AND SaleHead.Store_Id = {StoreID} " +
                                                $" AND {Where_Clause} ";
                                    }
                                }
                                else
                                {
                                    query = $"SELECT count(amttend) as [CntPayout], COALESCE(Sum(amttend),0) AS [sumAmount] " +
                                            $"FROM CSCTrans.dbo.Saletend INNER JOIN CSCTrans.dbo.SaleHead ON " +
                                            $"SaleTend.Sale_No = SaleHead.Sale_No  where  tendname = '{desc}' " +
                                            $" AND {Where_Clause} ";
                                }
                            }
                            payout = (Payout?)await Connection.QueryFirstOrDefaultAsync<Payout>(query, null, Transaction);
                            if (payout != null)
                            {
                                dblTendPayout += payout.sumAmount;
                                intTendCount += payout.Cnt;
                                if (intTendCount > 0)
                                    cnt += 1;

                                if (cnt == 1)
                                {
                                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                                    writer.WriteLine(PadR("Payout Summary", PRINT_WIDTH));
                                    writer.WriteLine(new string('_', PRINT_WIDTH));
                                    cnt += 1;
                                }
                            }
                            PayoutTotal += dblTendPayout;
                            PayoutCount += intTendCount;
                        }
                        PayoutTotal += (dblPayout + dblTendPayout);
                        PayoutCount += (intPayoutCount + intTendCount);

                        if ((intPayoutCount + intTendCount) != 0)
                            writer.WriteLine(PadR(rl.Desc, 25) + PadL((intPayoutCount + intTendCount).ToString("###0"), 5) + PadL((dblPayout + dblTendPayout).ToString("###0.00"), 10));
                    }

                    if (cnt > 0)
                    {
                        writer.WriteLine(new string(' ', PRINT_WIDTH));
                        writer.WriteLine(PadR("Total Payouts", 25) + PadL((intPayoutCount + intTendCount).ToString("###0"), 5) + PadL((dblPayout + dblTendPayout).ToString("###0.00"), 10));
                        writer.WriteLine(new string('_', PRINT_WIDTH));
                    }
                }
                #endregion

                #region Voided
                //Summarize Sales Voided and Individual Lines Deleted.
                if (Where_Clause.Length > 0)
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "ALL")
                        {
                            query = $"SELECT   DelLines.Del_Action as [Action], Count(*) as [Transactions] " +
                                    $"FROM     CSCTrans.dbo.DelLines " +
                                    $"WHERE    {Where_Clause} " +
                                    $"Group By DelLines.Del_Action ";
                        }
                        else
                        {
                            query = $"SELECT   DelLines.Del_Action as [Action], Count(*) as [Transactions] " +
                                    $"FROM     CSCTrans.dbo.DelLines " +
                                    $"WHERE    Store_Id = {StoreID} " +
                                    $" AND {Where_Clause} " +
                                    $"Group By DelLines.Del_Action ";
                        }
                    }
                    else
                    {
                        query = $"SELECT   DelLines.Del_Action as [Action], Count(*) as [Transactions] " +
                                $"FROM     CSCTrans.dbo.DelLines " +
                                $"WHERE {Where_Clause} " +
                                $"Group By DelLines.Del_Action ";
                    }
                    IEnumerable<DelData> delLines = await Connection.QueryAsync<DelData>(query, null, Transaction);
                    if (delLines != null && delLines.Any())
                    {
                        writer.WriteLine(new string(' ', PRINT_WIDTH));
                        writer.WriteLine(PadR("Void & Deleted Lines", PRINT_WIDTH));
                        writer.WriteLine(new string('_', PRINT_WIDTH));

                        foreach (var dl in delLines)
                        {
                            writer.WriteLine(PadR((dl.Action == "D") ? "Line Deleted" : "Voided Lines", 30) + PadL(dl.Transactions.ToString(), 10));
                        }
                    }
                }
                else
                {
                    query = "SELECT   DelLines.Del_Action as [Action], Count(*) as [Transactions] " +
                            "FROM     CSCTrans.dbo.DelLines " +
                            "WHERE Till_Num= @TillNumber " +
                            "Group By DelLines.Del_Action ";
                    IEnumerable<DelData> delLines = await Connection.QueryAsync<DelData>(query, new { TillNumber = TC.Till_Number }, Transaction);
                    if (delLines != null && delLines.Any())
                    {
                        writer.WriteLine(new string(' ', PRINT_WIDTH));
                        writer.WriteLine(PadR("Void & Deleted Lines", PRINT_WIDTH));
                        writer.WriteLine(new string('_', PRINT_WIDTH));

                        foreach (var dl in delLines)
                        {
                            writer.WriteLine(PadR((dl.Action == "D") ? "Line Deleted" : "Voided Lines", 30) + PadL(dl.Transactions.ToString(), 10));
                        }
                    }
                }
                #endregion Voided

                if (!Product_Sales_Report(writer, model, PrintIt, Where_Clause, StoreID).Result)
                {
                    //Console.WriteLine("Error in creating file.");
                    return false;
                }

                #region Tidel
                if (model.policies.TidelSafe)
                {
                    if (Where_Clause.Length == 0)
                    {
                        query = $"SELECT distinct ProcessType, sum(CashValue) as TotalCash, " +
                                $"sum(CashCount)as CashCnt,  Count(processtype)  as Cnt " +
                                $"FROM  CSCTrans.dbo.TidelTrans " +
                                $"WHERE  Response in ('OK', 'TRUE') " +
                                $"AND Till_num= {TC.Till_Number} Group by Processtype ";
                    }
                    else
                    {
                        if (!_config.IsBackOffice)
                        {
                            if (StoreID == "ALL")
                            {
                                query = $"SELECT distinct ProcessType, sum(CashValue) as TotalCash, " +
                                        $"sum(CashCount)as CashCnt,  Count(processtype)  as Cnt " +
                                        $"FROM  CSCTrans.dbo.TidelTrans " +
                                        $"WHERE  Response in ('OK', 'TRUE') " +
                                        $"AND {Where_Clause}  Group by Processtype ";
                            }
                            else
                            {
                                query = $"SELECT distinct ProcessType, sum(CashValue) as TotalCash, " +
                                        $"sum(CashCount)as CashCnt,  Count(processtype)  as Cnt " +
                                        $"FROM  CSCTrans.dbo.TidelTrans " +
                                        $"WHERE  Response in ('OK', 'TRUE') " +
                                        $" AND store_Id = {StoreID} AND {Where_Clause}  Group by Processtype ";
                            }
                        }
                        else
                        {
                            query = $"SELECT distinct ProcessType, sum(CashValue) as TotalCash, " +
                                    $"sum(CashCount)as CashCnt,  Count(processtype)  as Cnt " +
                                    $"FROM  CSCTrans.dbo.TidelTrans " +
                                    $"WHERE  Response in ('OK', 'TRUE') " +
                                    $" AND {Where_Clause}  Group by Processtype ";
                        }
                    }
                    IEnumerable<TidelData> tidelLines = await Connection.QueryAsync<TidelData>(query, null, Transaction);
                    if (tidelLines != null && tidelLines.Any())
                    {
                        writer.WriteLine(new string(' ', PRINT_WIDTH));
                        writer.WriteLine(PadR("TIDEL SAFE TRANSACTIONS", PRINT_WIDTH));
                        writer.WriteLine(new string('_', PRINT_WIDTH));

                        writer.WriteLine(PadR("Type", 11) + PadL("Cash Value", 11) + PadL("Cash Cnt", 11) + PadL("Trans #", 7));
                        writer.WriteLine(new string('_', PRINT_WIDTH));

                        foreach (var tl in tidelLines)
                        {
                            writer.WriteLine(PadR(tl.ProcessType, 11) + PadL(tl.TotalCash.ToString(), 10) + PadL(tl.CashCnt.ToString(), 11) + PadL(tl.Cnt.ToString(), 7));
                        }
                    }
                }
                #endregion Tidel

                #region Pay@pump incomplete transactions
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "ALL")
                    {
                        query = $"Select Sale_Date,Sale_Time,SaleHead.Sale_No,CompletionAmount " +
                                $"From CSCPayPumpHist.dbo.InComplete " +
                                $"INNER JOIN CSCTRANS.dbo.SaleHead SaleHead " +
                                $" ON SaleHead.Sale_No =InComplete.InvoiceNumber " +
                                $" INNER JOIN  CSCTRANS.dbo.CardTenders CardTenders ON SALEHEAD.Sale_no = CardTenders.SALE_NO " +
                                $" AND SALEHEAD.Till = CardTenders.Till_NUM " +
                                $" AND Salehead.Till= 100 " +
                                $" AND {Where_Clause} ORDER BY SaleHead.ShiftDate,Salehead.sale_no ";
                    }
                    else
                    {
                        query = $"Select Sale_Date,Sale_Time,SaleHead.Sale_No,CompletionAmount " +
                                $"From CSCPayPumpHist.dbo.InComplete " +
                                $"INNER JOIN CSCTRANS.dbo.SaleHead SaleHead " +
                                $" ON SaleHead.Sale_No =InComplete.InvoiceNumber " +
                                $" INNER JOIN  CSCTRANS.dbo.CardTenders CardTenders ON SALEHEAD.Sale_no = CardTenders.SALE_NO " +
                                $" AND SALEHEAD.Till = CardTenders.Till_NUM " +
                                $" AND Salehead.Till= 100 " +
                                $" AND store_Id = {StoreID} AND {Where_Clause} ORDER BY SaleHead.ShiftDate,Salehead.sale_no ";
                    }
                }
                else
                {
                    if (Where_Clause.Length > 0)
                    {
                        query = $"Select Sale_Date,Sale_Time,SaleHead.Sale_No,CompletionAmount " +
                                $"From CSCPayPumpHist.dbo.InComplete " +
                                $"INNER JOIN CSCTRANS.dbo.SaleHead SaleHead " +
                                $" ON SaleHead.Sale_No =InComplete.InvoiceNumber " +
                                $" INNER JOIN  CSCTRANS.dbo.CardTenders CardTenders ON SALEHEAD.Sale_no = CardTenders.SALE_NO " +
                                $" AND SALEHEAD.Till = CardTenders.Till_NUM " +
                                $" AND Salehead.Till= 100 " +
                                $" AND {Where_Clause} ORDER BY SaleHead.ShiftDate,Salehead.sale_no ";
                    }
                    else
                    {
                        query = $"Select Sale_Date,Sale_Time,SaleHead.Sale_No,CompletionAmount " +
                                $"From CSCPayPumpHist.dbo.InComplete " +
                                $"INNER JOIN CSCTRANS.dbo.SaleHead SaleHead " +
                                $" ON SaleHead.Sale_No =InComplete.InvoiceNumber " +
                                $" INNER JOIN  CSCTRANS.dbo.CardTenders CardTenders ON SALEHEAD.Sale_no = CardTenders.SALE_NO " +
                                $" AND SALEHEAD.Till = CardTenders.Till_NUM " +
                                $" AND Salehead.Till= 100 " +
                                $" ORDER BY SaleHead.ShiftDate,Salehead.sale_no ";
                    }
                }

                IEnumerable<IncompData> incomps = await Connection.QueryAsync<IncompData>(query, null, Transaction);
                if ((incomps != null && incomps.Any()) && (Where_Clause != "") && (TC.Till_Number == 100))
                {
                    writer.WriteLine(PadR("----Pay@Pump Incomplete Transactions----", PRINT_WIDTH));
                    writer.WriteLine(new string('_', PRINT_WIDTH));

                    writer.WriteLine(PadR("Sale No", 10) + PadL("Sale Time", 15) + PadL("Amount", 10));
                    writer.WriteLine(new string('_', PRINT_WIDTH));

                    foreach (var ic in incomps)
                    {
                        writer.WriteLine(PadR(ic.Sale_No, 10) + PadL(ic.Sale_Time.ToString("HH:mm:ss"), 15) + PadL(ic.CompletionAmount.ToString(), 10));
                    }
                }
                #endregion Pay@pump incomplete transactions
            }
            //writer.Close();

            bFinished = true;
            return bFinished;
        }
        public async Task<bool> Print_AITE_Report(StreamWriter writer, string Where_Clause, string StoreID)
        {
            string query;
            decimal vAmount;
            decimal vTotalTaxSaved;

            query = "select * from CSCMaster.dbo.Category order by ID";
            IEnumerable<Category> category = await Connection.QueryAsync<Category>(query, null, Transaction);

            vAmount = 0;
            vTotalTaxSaved = 0;

            foreach (var ct in category)
            {
                if (Where_Clause.Length > 0)
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "All")
                        {
                            query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                                    $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                                    $"      COALESCE(Sum(ExemptedTax),0) as [sTotalTaxSaved]  " +
                                    $"From CSCTrans.dbo.TaxExemptSaleLine INNER JOIN CSCTrans.dbo.SaleHead ON TaxExemptSaleLine.Sale_No = SaleHead.Sale_No " +
                                    $"      AND TaxExemptSaleLine.Store_id = SaleHead.Store_id  " +
                                    $"Where ProductType= {ct.Id} AND {Where_Clause} " +
                                    $"Group By ProductType";
                        }
                        else
                        {
                            query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                                    $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                                    $"      COALESCE(Sum(ExemptedTax),0) as [sTotalTaxSaved]  " +
                                    $"From CSCTrans.dbo.TaxExemptSaleLine INNER JOIN CSCTrans.dbo.SaleHead ON TaxExemptSaleLine.Sale_No = SaleHead.Sale_No " +
                                    $"      AND TaxExemptSaleLine.Store_id = SaleHead.Store_id  " +
                                    $"Where SaleHead.Store_id = {StoreID} AND ProductType= {ct.Id} AND {Where_Clause} " +
                                    $"Group By ProductType";
                        }
                    }
                    else
                    {
                        query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                                $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                                $"      COALESCE(Sum(ExemptedTax),0) as [sTotalTaxSaved]  " +
                            $"From CSCTrans.dbo.TaxExemptSaleLine INNER JOIN CSCTrans.dbo.SaleHead ON TaxExemptSaleLine.Sale_No = SaleHead.Sale_No " +
                                $"Where ProductType= {ct.Id} AND {Where_Clause} " +
                                $"Group By ProductType";
                    }
                }
                else
                {
                    query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                            $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                            $"      COALESCE(Sum(ExemptedTax),0) as [sTotalTaxSaved]  " +
                            $"From CSCTrans.dbo.TaxExemptSaleLine INNER JOIN CSCTrans.dbo.SaleHead ON TaxExemptSaleLine.Sale_No = SaleHead.Sale_No " +
                            $"Where ProductType= {ct.Id} AND TILL_NUM = {TC.Till_Number} " +
                            $"Group By ProductType";
                }
                IEnumerable<TaxExemptLine> teLines = await Connection.QueryAsync<TaxExemptLine>(query, null, Transaction);

                if (teLines != null && teLines.Any())
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(new string(' ', PRINT_WIDTH));

                    if (polShow_All_Exempt)
                        writer.WriteLine(PadR("Tax Exempt Sales - (AITE) ", 25));
                    else
                        writer.WriteLine(PadR("Tax Exempt Sales", 20));

                    writer.WriteLine(new string(' ', 32) + PadR("Exempted", 8));

                    writer.WriteLine(PadR("Category", 10) + PadL("Qty", 11) + PadL("Sales", 10) + PadL("Tax", 9));
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    
                    foreach (var tl in teLines)
                    {
                        writer.WriteLine(PadR(ct.Name, 11) + PadL(tl.sQuantity.ToString("####0.000"), 10) + PadL(tl.sAmount.ToString("######0.00"), 10)
                            + PadL(tl.sTotalTaxSaved.ToString("####0.00"), 9));

                        vAmount += tl.sAmount;
                        vTotalTaxSaved += tl.sTotalTaxSaved;
                    }

                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(PadR("Total Sales", 21) + PadL(vAmount.ToString("######0.00"), 10) + PadL(vTotalTaxSaved.ToString("######0.00"), 9));
                }

            } /* end of for */

            return true;
        } /* end of function */

        public async Task<bool> Print_SITE_Report(StreamWriter writer, string Where_Clause, string StoreID)
        {
            string query;
            decimal vAmount;
            decimal vTotalTaxSaved;
            bool HeadPrn = false;

            query = "select * from CSCMaster.dbo.Category order by ID";
            IEnumerable<Category> category = await Connection.QueryAsync<Category>(query, null, Transaction);

            vAmount = 0;
            vTotalTaxSaved = 0;

            foreach (var ct in category)
            {
                if (Where_Clause.Length > 0)
                {
                    if (!_config.IsBackOffice)
                    {
                        if (StoreID == "All")
                        {
                            query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                                    $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                                    $"      COALESCE(Sum(TotalTaxSaved),0) as [sTotalTaxSaved]  " +
                                    $"From CSCTrans.dbo.PurchaseItem INNER JOIN CSCTrans.dbo.SaleHead ON PurchaseItem.Sale_No = SaleHead.Sale_No " +
                                    $"      AND PurchaseItem.Store_id = SaleHead.Store_id  " +
                                    $"Where CategoryCodeFK= {ct.Id} AND {Where_Clause} " +
                                    $"Group By CategoryCodeFK";
                        }
                        else
                        {
                            query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                                    $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                                    $"      COALESCE(Sum(TotalTaxSaved),0) as [sTotalTaxSaved]  " +
                                    $"From CSCTrans.dbo.PurchaseItem INNER JOIN CSCTrans.dbo.SaleHead ON PurchaseItem.Sale_No = SaleHead.Sale_No " +
                                    $"      AND PurchaseItem.Store_id = SaleHead.Store_id  " +
                                    $"Where SaleHead.Store_id = {StoreID} AND CategoryCodeFK= {ct.Id} AND {Where_Clause} " +
                                    $"Group By CategoryCodeFK";
                        }
                    }
                    else
                    {
                        query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                                $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                                $"      COALESCE(Sum(TotalTaxSaved),0) as [sTotalTaxSaved]  " +
                            $"From CSCTrans.dbo.PurchaseItem INNER JOIN CSCTrans.dbo.SaleHead ON PurchaseItem.Sale_No = SaleHead.Sale_No " +
                                $"Where CategoryCodeFK= {ct.Id} AND {Where_Clause} " +
                                $"Group By CategoryCodeFK";
                    }
                }
                else
                {
                    query = $"Select COALESCE(Sum(Quantity),0) as [sQuantity], " +
                            $"      COALESCE(Sum(Amount),0) as [sAmount], " +
                            $"      COALESCE(Sum(TotalTaxSaved),0) as [sTotalTaxSaved]  " +
                            $"From CSCTrans.dbo.PurchaseItem INNER JOIN CSCTrans.dbo.SaleHead ON PurchaseItem.Sale_No = SaleHead.Sale_No " +
                            $"Where CategoryCodeFK= {ct.Id} AND TILL_NUM = {TC.Till_Number} " +
                            $"Group By CategoryCodeFK";
                }
                IEnumerable<TaxExemptLine> teLines = await Connection.QueryAsync<TaxExemptLine>(query, null, Transaction);

                if (teLines != null && teLines.Any())
                {
                    writer.WriteLine(new string(' ', PRINT_WIDTH));
                    writer.WriteLine(new string(' ', PRINT_WIDTH));

                    if (polShow_All_Exempt)
                        writer.WriteLine(PadR("Tax Exempt Sales - (SITE) ", 25));
                    else
                        writer.WriteLine(PadR("Tax Exempt Sales", 20));

                    writer.WriteLine(new string(' ', 32) + PadR("Exempted", 8));

                    writer.WriteLine(PadR("Category", 10) + PadL("Qty", 11) + PadL("Sales", 10) + PadL("Tax", 9));
                    writer.WriteLine(new string('_', PRINT_WIDTH));
                    writer.WriteLine(new string(' ', PRINT_WIDTH));

                    if (!HeadPrn)
                    {
                        if (polShow_All_Exempt)
                        {
                            writer.WriteLine(PadR("Tax Exempt Sales - (SITE)", 25));
                        }
                        else
                        {
                            writer.WriteLine(PadR("Tax Exempt Sales", 20));
                        }

                        writer.WriteLine(PadR("", 33) + PadL("Exempted", 8));
                        writer.WriteLine(PadR("Category", 13) + PadL("Qty", 9) + PadL("Sales", 10) + PadL("Tax", 8));
                        writer.WriteLine(new string('_', PRINT_WIDTH));
                        HeadPrn = true;
                    }
                    foreach (var tl in teLines)
                    {
                        writer.WriteLine(PadR(ct.Name, 13) + PadL(tl.sQuantity.ToString("####0.000"), 9) + PadL(tl.sAmount.ToString("######0.00"), 10)
                            + PadL(tl.sTotalTaxSaved.ToString("####0.00"), 8));

                        vAmount += tl.sAmount;
                        vTotalTaxSaved += tl.sTotalTaxSaved;
                    }

                    if (HeadPrn)
                    {
                        writer.WriteLine(new string('_', PRINT_WIDTH));
                        writer.WriteLine(PadR("Total Sales", 22) + PadL(vAmount.ToString("######0.00"), 10) + PadL(vTotalTaxSaved.ToString("######0.00"), 8));
                        writer.WriteLine(new string(' ', PRINT_WIDTH));
                    }
                }

            } /* end of for */

            return true;
        } /* end of function */
        public async Task<bool> Print_TE_TAXES_Report(StreamWriter writer, string Where_Clause, string StoreID)
        {
            string query;
            decimal totAdded = 0;
            decimal totIncluded = 0;

            if (Where_Clause.Length > 0)
            {
                if (!_config.IsBackOffice)
                {
                    if (StoreID == "All")
                    {
                        query = $"SELECT DISTINCT TaxCredit.Tax_Name, " +
                                $"      COALESCE(SUM(TaxCredit.Tax_Added_Amount),0) as [Added_Tax], " +
                                $"      COALESCE(SUM(TaxCredit.Tax_Included_Amount),0) as [Included_Tax]  " +
                                $"From CSCTrans.dbo.TaxCredit " +
                                $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleHead.TILL = TaxCredit.TILL_NUM AND SaleHead.SALE_NO = TaxCredit.Sale_No " +
                                $"      AND SaleHead.Store_id = SaleHead.Store_id  " +
                                $"Where {Where_Clause} " +
                                $"Group By TaxCredit.Tax_Name ";
                    }
                    else
                    {
                        query = $"SELECT DISTINCT TaxCredit.Tax_Name, " +
                                $"      COALESCE(SUM(TaxCredit.Tax_Added_Amount),0) as [Added_Tax], " +
                                $"      COALESCE(SUM(TaxCredit.Tax_Included_Amount),0) as [Included_Tax]  " +
                                $"From CSCTrans.dbo.TaxCredit " +
                                $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleHead.TILL = TaxCredit.TILL_NUM AND SaleHead.SALE_NO = TaxCredit.Sale_No " +
                                $"      AND SaleHead.Store_id = SaleHead.Store_id  " +
                                $"Where SaleHead.Store_id = {StoreID} AND {Where_Clause} " +
                                $"Group By TaxCredit.Tax_Name ";
                    }
                }
                else
                {
                    query = $"SELECT DISTINCT TaxCredit.Tax_Name, " +
                            $"      COALESCE(SUM(TaxCredit.Tax_Added_Amount),0) as [Added_Tax], " +
                            $"      COALESCE(SUM(TaxCredit.Tax_Included_Amount),0) as [Included_Tax]  " +
                            $"From CSCTrans.dbo.TaxCredit " +
                            $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleHead.TILL = TaxCredit.TILL_NUM AND SaleHead.SALE_NO = TaxCredit.Sale_No " +
                            $"Where {Where_Clause} " +
                            $"Group By TaxCredit.Tax_Name ";
                }
            }
            else
            {
                query = $"SELECT DISTINCT TaxCredit.Tax_Name, " +
                        $"      COALESCE(SUM(TaxCredit.Tax_Added_Amount),0) as [Added_Tax], " +
                        $"      COALESCE(SUM(TaxCredit.Tax_Included_Amount),0) as [Included_Tax]  " +
                        $"From CSCTrans.dbo.TaxCredit " +
                        $"INNER JOIN CSCTrans.dbo.SaleHead ON SaleHead.TILL = TaxCredit.TILL_NUM AND SaleHead.SALE_NO = TaxCredit.Sale_No " +
                        $"Where TILL_NUM = {TC.Till_Number} " +
                        $"Group By TaxCredit.Tax_Name ";
            }
            IEnumerable<TaxCredit> taxCredits = await Connection.QueryAsync<TaxCredit>(query, null, Transaction);

            if (taxCredits != null && taxCredits.Any())
            {
                writer.WriteLine(PadR("EXEMPTED TAXES (Added & Included only)", PRINT_WIDTH));
                writer.WriteLine(PadR("Name", 20) + PadL("Added", 10) + PadL("Included", 10));
                writer.WriteLine(new string('_', PRINT_WIDTH));

                foreach (var tc in taxCredits)
                {
                    writer.WriteLine(PadR(tc.Tax_Name, 11) + PadL(tc.Added_Tax.ToString("#####0.00"), 10) + PadL(tc.Included_Tax.ToString("#####0.00"), 10));

                    totAdded += tc.Added_Tax;
                    totIncluded += tc.Included_Tax;
                }
            }

            writer.WriteLine(new string('_', PRINT_WIDTH));
            writer.WriteLine(PadR("Total Sales", 21) + PadL(totAdded.ToString("######0.00"), 10) + PadL(totIncluded.ToString("######0.00"), 9));

            return true;
        } /* end of function */

        public static string PadC(string input, int width)

        {
            return input.PadLeft((width + input.Length) / 2).PadRight(width);
        }

        public static string PadL(string input, int width)
        {
            return input.PadLeft(width);
        }

        public static string PadR(string input, int width)
        {
            return input.PadRight(width);
        }

        public void DeleteTillClose()
        {
            //const string sql = $"DELETE From CSCTrans.dbo.TillClose";

            //int rowsAffected = await Connection.ExecuteAsync(sql, null, Transaction);
            //return rowsAffected > 0;
            //--
            const string sql = $"DELETE From CSCTrans.dbo.TillClose";

            using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);

            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
        }

        public async Task<string> Collect_Close_Data(GetRepTillCloseDto model, int shift = 0, bool AllShifts = false, string StoreID = "")
        {
            string Where_Clause;
            decimal Sum_Other = 0;
            int cnt_Other = 0;
            int cnt_Totals = 0;
            string polCountType = model.policies.CountType;
            bool polIncludInClose = model.policies.CloseIncluded;
            string polBonusTend = model.policies.CBonusTend;
            string query;
            DateOnly shiftDate = DateOnly.FromDateTime(model.shiftDate);

            loading = true;
            #region Getting a where_clause
            if (model.closeNo == 0 && shift == 0)
            {
                //' Where Clause for a range of dates
                Where_Clause = $" " +
                    $"SaleHead.ShiftDate BETWEEN '{model.startDate}' AND '{model.endDate}' ";
            }
            else if (shift > 0)
            {
                //' Where Clause for reports by shift
                if (AllShifts)
                    // All shifts on a date.
                    Where_Clause = $" " +
                        $"SaleHead.ShiftDate = '{shiftDate}' ";
                else
                    // Selected shift on a date.
                    Where_Clause = $" " +
                        $"SaleHead.ShiftDate = '{shiftDate}' and SaleHead.Shift = {shift}";
            }
            else
            {
                // Where Clause for a close number
                Where_Clause = $" " +
                    $"Close_Num = {model.closeNo}";
            }
            #endregion

            var payments = GetPaymentData(StoreID, Where_Clause).Result;
            Payments = Convert.ToDecimal(payments);

            var arPay = GetArPaymentData(StoreID, Where_Clause).Result;
            ARPay = Convert.ToDecimal(arPay);

            var penny_Adj = GetPennyAdjustData(Where_Clause).Result;
            Penny_Adj = Convert.ToDecimal(penny_Adj);

            var payouts = GetPayoutsData(StoreID, Where_Clause).Result;
            Payouts = Convert.ToDecimal(payouts);

            var overPay = GetOverPaymentData(StoreID, Where_Clause).Result;
            OverPay = Convert.ToDecimal(overPay);

            var changePaid = GetChangePaidData(StoreID, Where_Clause).Result;
            ChangePaid = Convert.ToDecimal(changePaid);

            var cashDraw = GetCashDrawData(StoreID, Where_Clause, model.startDate, model.endDate).Result;
            if (cashDraw != null)
            {
                Draw_Amt = cashDraw.Draw_Total;
                BonusDraw = cashDraw.CashBonus;
            }

            //' Clear out the TillClose file before reloading.
            DeleteTillClose();

            if (model.policies.CashBonus && (polBonusTend.Length > 0))
            {
                var bonusGiveAway = GetBonusGiveAwayData(model, StoreID, Where_Clause).Result;
                BonusGiveAway = Convert.ToDecimal(bonusGiveAway);

                var bonusDrop = GetBonusDropData(model, StoreID, Where_Clause, model.startDate, model.endDate).Result;
                BonusDrop = Convert.ToDecimal(bonusDrop);
            }

            decimal total_Changes = Convert.ToDecimal(GetTotalChangePaidData(StoreID, Where_Clause).Result);
            Total_Tenders = total_Changes + Draw_Amt + Float_Amt;

            Drop_Conv = 0;
            IEnumerable<DropData> dropLines = await GetDropLines(model, StoreID, Where_Clause, model.startDate, model.endDate);

            IEnumerable<TenderData> rsSaleTends = await GetSaleTendData(StoreID, Where_Clause);

            IEnumerable<TendMast> rsInclTends = await GetInclTendData();

            if (dropLines != null)
            {
                foreach (var dl in dropLines)
                {
                    if (polCountType.ToUpper() == "EACH TENDER")
                    {
                        foreach (var st in rsSaleTends)
                        {
                            if (polIncludInClose)
                            {
                                bool bFound = false;

                                foreach (var it in rsInclTends)
                                {
                                    if (it.TendDesc == st.Tender)
                                    {
                                        bFound = true;

                                        TillClose tc = new();
                                        if (st.Tender == model.policies.BaseCurr)
                                            tc.System = -dl.Drop_Amt + Float_Amt + total_Changes + Draw_Amt;
                                        else if ((st.Tender == polBonusTend) && (model.policies.CashBonus))
                                            tc.System = -dl.Drop_Amt + BonusFloat + BonusDraw - BonusGiveAway;
                                        else
                                            tc.System = -dl.Drop_Amt;

                                        tc.StoreID = StoreID;
                                        tc.Tender = st.Tender;
                                        tc.Entered = 0;
                                        tc.Count = st.Count;
                                        tc.Difference = (decimal)-tc.System;

                                        if (!await AddTillClose(tc))
                                        {
                                            Console.WriteLine($"Error in AddTillClose");
                                        }

                                        break;
                                    }
                                }
                                if (!bFound)
                                {
                                    Sum_Other += st.Used;
                                    cnt_Other += st.Count;

                                    TillClose tc = new();
                                    if (st.Tender == model.policies.BaseCurr)
                                        tc.System = -dl.Drop_Amt + Float_Amt + total_Changes + Draw_Amt;
                                    else if ((st.Tender == polBonusTend) && (model.policies.CashBonus))
                                        tc.System = -dl.Drop_Amt + BonusFloat + BonusDraw - BonusGiveAway;
                                    else
                                        tc.System = -dl.Drop_Amt;

                                    tc.StoreID = StoreID;
                                    tc.Tender = "Other Tenders";
                                    tc.Entered = 0;
                                    tc.Count = st.Count;
                                    tc.Difference = -tc.System;

                                    if (!await AddTillClose(tc))
                                    {
                                        Console.WriteLine($"Error in AddTillClose");
                                    }
                                }
                            }
                            else // !policies.CloseIncluded
                            {
                                TillClose tc = new();
                                tc.StoreID = StoreID;
                                tc.Tender = dl.Tender_Name;
                                tc.Entered = 0;
                                if (st.Tender == model.policies.BaseCurr)
                                    tc.System = -dl.Drop_Amt + Float_Amt + total_Changes + Draw_Amt;
                                else if ((st.Tender == polBonusTend) && (model.policies.CashBonus))
                                    tc.System = -dl.Drop_Amt + BonusFloat + BonusDraw - BonusGiveAway;
                                else
                                    tc.System = -dl.Drop_Amt;

                                tc.Difference = -tc.System;
                                tc.Count = st.Count;

                                if (!await AddTillClose(tc))
                                {
                                    Console.WriteLine($"Error in AddTillClose");
                                }
                            }
                        }
                    }
                    else // policies.CountType != "EACH TENDER"
                        Total_Tenders -= Convert.ToDecimal(dl.Drop_Conv);

                    Drop_Conv += Convert.ToDecimal(dl.Drop_Conv);
                } //end of dropLines loop
            }
            foreach (var td in rsSaleTends)
            {
                if (!GetTillCloseByDesc(td.Tender).Result) // INSERT as No Same Tend exist yet
                {
                    if (polCountType.ToUpper() == "EACH TENDER")
                    {
                        if (polIncludInClose)
                        {
                            bool bFound = false;

                            foreach (var it in rsInclTends)
                            {
                                if (it.TendDesc == td.Tender)
                                {
                                    bFound = true;

                                    TillClose tc = new();
                                    tc.StoreID = StoreID;
                                    tc.Tender = td.Tender;
                                    if (td.Tender == model.policies.BaseCurr)
                                        tc.System = td.Amount + Float_Amt + total_Changes + Draw_Amt;
                                    else if ((td.Tender == polBonusTend) && (model.policies.CashBonus))
                                        tc.System = td.Amount + Float_Amt + BonusDraw - BonusGiveAway;
                                    else
                                        tc.System = td.Amount - Drop_Amt;

                                    tc.Difference = -tc.System;
                                    tc.Count = td.Count;
                                    tc.Entered = 0;

                                    if (!await AddTillClose(tc))
                                    {
                                        Console.WriteLine($"Error in AddTillClose");
                                    }
                                    //break;
                                }
                            }
                            if ((!bFound) && (td.Tender != polBonusTend))
                            {
                                Sum_Other += td.Used;
                                cnt_Other += td.Count;
                            }
                            //break;
                        }
                        else // !policies.CloseIncluded
                        {
                            TillClose tc = new();
                            tc.StoreID = StoreID;
                            tc.Tender = td.Tender;
                            if (td.Tender == model.policies.BaseCurr)
                                tc.System = td.Amount + Float_Amt + total_Changes + Draw_Amt;
                            else if ((td.Tender == polBonusTend) && (model.policies.CashBonus))
                                tc.System = td.Amount + BonusFloat + BonusDraw - BonusGiveAway;
                            else
                                tc.System = td.Amount - Drop_Amt;

                            tc.Difference = -tc.System;
                            tc.Entered = 0;
                            tc.Count = td.Count;

                            if (!await AddTillClose(tc))
                            {
                                Console.WriteLine($"Error in AddTillClose");
                            }
                        }
                    } // end of Each Tender
                    else
                    {
                        Total_Tenders += Convert.ToDecimal(td.Used);
                        cnt_Totals += td.Count;
                    } // end of All Tender
                }
                else // Update as the Same TendDesc already exist
                {
                    if (polCountType.ToUpper() == "EACH TENDER")
                    {
                        if (polIncludInClose)
                        {
                            bool bFound = false;

                            foreach (var it in rsInclTends)
                            {
                                if (it.TendDesc == td.Tender)
                                {
                                    bFound = true;

                                    if (!await UpdateTender(td.Amount, td.Tender))
                                    {
                                        Console.WriteLine($"Error in UpdateTender");
                                    }
                                    break;
                                }
                            }
                            if ((!bFound) && (td.Tender != polBonusTend))
                            {
                                Sum_Other += td.Used;
                                cnt_Other += td.Count;
                            }
                            break;
                        }
                        else // !policies.CloseIncluded
                        {
                            decimal? System_Amt = 0;

                            System_Amt = td.Amount;

                            if (!await UpdateTender(td.Amount, td.Tender))
                            {
                                Console.WriteLine($"Error in UpdateTender");
                            }
                            Total_Tenders += Convert.ToDecimal(td.Used);
                            cnt_Totals += td.Count;
                        }
                    } // end of Each Tender                }
                } // end of rsSaleTends loop

                if (polCountType.ToUpper() == "EACH TENDER")
                {
                    bool bFound = false;
                    foreach (var st in rsSaleTends)
                    {
                        if (st.Tender == model.policies.BaseCurr)
                        {
                            bFound = true;
                        }
                    }
                    if ((!bFound) && (Math.Abs(Convert.ToInt32(Float_Amt)) + Math.Abs(Convert.ToInt32(ChangePaid))
                        + Math.Abs(Convert.ToInt32(Draw_Amt)) + Math.Abs(Convert.ToInt32(Payouts)) > 0))
                    {
                        TillClose tc = new();
                        tc.StoreID = StoreID;
                        tc.Tender = model.policies.BaseCurr;
                        tc.Count = 0;
                        tc.Entered = 0;
                        tc.System = Float_Amt + ChangePaid + Draw_Amt;
                        tc.Difference = -tc.System;

                        if (!await AddTillClose(tc))
                        {
                            Console.WriteLine($"Error in AddTillClose");
                        }
                    }

                    if (model.policies.CashBonus && (polBonusTend.Length > 0))
                    {
                        bFound = false;
                        foreach (var st in rsSaleTends)
                        {
                            if (st.Tender == polBonusTend)
                            {
                                bFound = true;
                            }
                        }
                        if ((!bFound) && (Math.Abs(Convert.ToInt32(BonusFloat)) + Math.Abs(Convert.ToInt32(BonusDraw))
                            + Math.Abs(Convert.ToInt32(BonusDraw)) + Math.Abs(Convert.ToInt32(BonusGiveAway)) > 0))
                        {
                            TillClose tc = new();
                            tc.StoreID = StoreID;
                            tc.Tender = polBonusTend;
                            tc.Count = 0;
                            tc.Entered = 0;
                            tc.System = BonusFloat + BonusDraw - BonusGiveAway;
                            tc.Difference = -tc.System;

                            if (!await AddTillClose(tc))
                            {
                                Console.WriteLine($"Error in AddTillClose");
                            }
                        }
                    }
                }

                if (polCountType == "All Tenders")
                {
                    TillClose tc = new();
                    tc.StoreID = StoreID;
                    tc.Tender = "All Tenders";
                    tc.Count = cnt_Totals;
                    tc.Entered = 0;
                    tc.System = Total_Tenders;
                    tc.Difference = -tc.System;

                    if (!await AddTillClose(tc))
                    {
                        Console.WriteLine($"Error in AddTillClose");
                    }
                }
                else if ((polIncludInClose) && (Sum_Other != 0))
                {
                    TillClose tc = new();
                    tc.StoreID = StoreID;
                    tc.Tender = "Other Tenders";
                    tc.Count = cnt_Totals;
                    tc.Entered = 0;
                    tc.System = Sum_Other;
                    tc.Difference = -tc.System;

                    if (!await AddTillClose(tc))
                    {
                        Console.WriteLine($"Error in AddTillClose");
                    }
                }
            }
            loading = false;

            return Where_Clause;
        }
    }
}
