
using Dapper;
using InfonetCSCR.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Web.Razor.Tokenizer;

namespace InfonetCSCR.Reporting.Repository
{
    public static class ReportGlobal
    {
        public static string LanguageUsed = "English";

        //public static String GetLanguage()
        //{
        //    string Language = CSCAdminDB.Query<string>
        //        ("SELECT TOP(1) Language FROM SETUP_C")
        //        .FirstOrDefault();
        //    return Language;
        //}
    }

    public static class WriteLog
    {
        public static void WriteLine(System.Exception e)
        {
            WriteLine("Message: " + e.Message + System.Environment.NewLine + "Stack trace: " + e.StackTrace);
        }

        public static void WriteLine(string TextToWrite)
        {
            WriteLine(TextToWrite, "P");
        }

        public static void WriteLine(string TextToWrite, string TypeStr)
        {
            WriteLine(TextToWrite, TypeStr, "LogMain.txt");
        }

        public static void WriteLine(string TextToWrite, string TypeStr, string LogFileName)
        {
            string logPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Log"), LogFileName);

            if (!File.Exists(logPath))
            {
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath.Substring(0, logPath.LastIndexOf("\\")));

                FileStream fs1 = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write);
                fs1.Close();
            }


            FileInfo fi = new FileInfo(logPath);
            if ((fi.Length / 1024) > 1000)
            {
                string strTime = DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() +
                    DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString(); // +DateTime.Now.Millisecond.ToString();            
                string NewLogName = logPath.Substring(logPath.LastIndexOf('\\') + 1, (logPath.LastIndexOf('.') - logPath.LastIndexOf('\\') - 1));
                string NewPath = logPath.Substring(0, logPath.LastIndexOf('\\'));
                File.Move(logPath, NewPath + "\\" + NewLogName + "_" + strTime + ".txt");
            }

            if (TypeStr != "E") TextToWrite = DateTime.Now.ToString() + " " + TextToWrite;

            StreamWriter sw = File.AppendText(logPath);
            sw.WriteLine(TextToWrite);
            sw.Close();
        }

    } //  End Class WriteLog

    public class DBHelper
    {
        private IDbConnection CSCAdminDB, CSCMasterDB, CSCTransDB;
        private bool isHeadOffice;
        public DBHelper()
        {
            CSCAdminDB = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCAdminConnectionString"].ConnectionString);
            CSCMasterDB = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCMasterConnectionString"].ConnectionString);
            CSCTransDB = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString);

            bool.TryParse(ConfigurationManager.AppSettings["IsHeadOffice"], out isHeadOffice);
        }

        #region Authentication

        public User GetUser(string code)
        {
            var user = CSCAdminDB.Query<User>
                ("SELECT U_CODE AS CODE, EPW AS PASSWORD FROM [USER] WHERE U_CODE = @CODE",
                new { CODE = code })
                .FirstOrDefault();
            return user;
        }

        public string GetCompnayName()
        {

            string companyName = CSCAdminDB.Query<string>
                ("SELECT TOP(1) COMPANY_NAME FROM SETUP_C")
                .FirstOrDefault();
            return companyName;
        }

        public bool HasAccessRight(string pName, User user)
        {
            bool hasAccess = CSCAdminDB.Query<bool>
                              ("IF EXISTS( SELECT * FROM P_COMP WHERE P_NAME = @P_NAME AND P_ACTIVE = 1) BEGIN IF EXISTS( SELECT * FROM P_SET WHERE P_NAME = @P_NAME AND P_SET = 'YES' AND ((P_LEVEL = 'USER' AND P_VALUE = @USERNAME) OR (P_LEVEL = 'UGROUP' AND P_VALUE = (SELECT P_VALUE FROM UIG WHERE[USER] = @USERNAME)))) BEGIN SELECT 'TRUE' END ELSE SELECT 'FALSE' END ELSE SELECT 'TRUE' ",
                              new { P_NAME = pName, USERNAME = user.Code })
                              .FirstOrDefault();
            return hasAccess;
        }

        #endregion

        #region Dashboard

        public DashboardData GetSalesAndTransData(DateTime date)
        {
            DashboardData salesAndTrans = CSCTransDB.Query<DashboardData>("Sproc_TotalSalesAndTransactions", new { SALEDATE = date },
                                              commandType: CommandType.StoredProcedure).FirstOrDefault();
            return salesAndTrans;
        }

        public IEnumerable<Sale> GetChartModelData(DateTime date)
        {
            IEnumerable<Sale> chartdata = CSCTransDB.Query<Sale>("Sproc_DashboardChartData", new { SALEDATE = date },
                                              commandType: CommandType.StoredProcedure);

            return chartdata;
        }

        #endregion

        #region Report

        public List<ReportResModel> Fetch_ReportResource(string strReport, string strPH)
        {
            string sqlCommand = "";

            try
            {
                sqlCommand = "SELECT[Report_Name],[Field_Name],[Res_Number] FROM [Report_Res] WHERE [Report_Name] = '" + strReport + "' AND [Field_Name] = '" + strPH + "' ";

                //List<ReportResModel> dataSet = SqlHelper.ExecuteDataset(conString, CommandType.Text, sqlCommand);
                return CSCMasterDB.Query<ReportResModel>(sqlCommand).ToList();
            }
            catch (Exception ex)
            {
                WriteLog.WriteLine("Error : " + ex.Message);
                return null;
            }
        }

        public List<FilterModel> GetStoreList()
        {
            return CSCMasterDB.Query<FilterModel>("SELECT Store_Id as Id, Store_Desc as Name from Store ORDER BY Store_Id").ToList();
        }

        public List<ReportFilterModel> GetReportFilterList()
        {
            return CSCAdminDB.Query<ReportFilterModel>("SELECT [Report_Name],[Filter_Seq],[Field],[Type],[Field_Type] FROM [Report_Filter] ORDER BY Filter_Seq").ToList();
        }

        public List<ReportResModel> GetReportResourceList(String strReport)
        {
            return CSCAdminDB.Query<ReportResModel>("SELECT [Report_Name],[Field_Name],[Res_Number] FROM [Report_Res] WHERE [Report_Name] = '" + strReport + "' " + " ORDER BY Field_Name").ToList();
        }

        public string GetReportTitle(String strReport)
        {
            string Title = CSCAdminDB.Query<string>("SELECT [Report_Title] FROM [Reports] WHERE [Report_Name] = '" + strReport + "'").FirstOrDefault();
            return Title;
        }


        public ReportFilter GetReportFilter()
        {
            ReportFilter reportFilter = new ReportFilter
            {
                DeptWithFuel = CSCMasterDB.Query<DeptFilterModel>("SELECT D.Dept as Id, Dept_Name as Name, FD.Dept FuelDept FROM DEPT D LEFT JOIN [FUELDEPT] FD ON D.DEPT = FD.DEPT ORDER BY D.Dept").ToList(),
                SubDept = CSCMasterDB.Query<StockMovementFilterModel>("SELECT Sub_Dept as Id, Sub_Name as Name, Dept as DeptId from SubDept ORDER BY Sub_Name").ToList(),
                SubDetail = CSCMasterDB.Query<StockMovementFilterModel>("SELECT SubDetail as Id, SubDetail_Name as Name, Dept as DeptId from SubDetail ORDER BY SubDetail_Name").ToList(),
                StockCode = CSCMasterDB.Query<StockMovementFilterModel>("SELECT Stock_Code as Id, Descript as Name, Dept as DeptId from StockMST ORDER BY Descript").ToList(),
                Vendor = CSCMasterDB.Query<FilterModel>("SELECT Code as Id, Name from Vendors ORDER BY Name").ToList(),
                Reports = CSCAdminDB.Query<CategoryFilterModel>("SELECT GID, rep.Report_Name as ID, rep.Report_Title as Name FROM [CSCAdmin].[dbo].[REPORTS] rep, [CSCAdmin].[dbo].[Reports_in_Groups] rig WHERE rep.Report_Name = rig.Report_Name AND rep.Web = 1 order by GID, rep.Report_Name").ToList(),
                Filters = CSCAdminDB.Query<ReportFilterModel>("SELECT [Report_Name],[Filter_Seq],[Field],[Type],[Field_Type] FROM [Report_Filter] ORDER BY Filter_Seq").ToList(),
                //ReportRes = CSCAdminDB.Query<ReportResModel>("SELECT Report_name, Field_name, Res_Number FROM [CSCAdmin].[dbo].[Report_Res] ORDER BY Field_Name").ToList()
            };

            reportFilter.Dept = reportFilter.DeptWithFuel.Where(x => x.FuelDept is null).ToList();

            return reportFilter;
        }
        //Reports = CSCAdminDB.Query<CategoryFilterModel>("SELECT GID, rep.Report_Name as ID, rep.Report_Title as Name " +
        //                                                        "FROM [CSCAdmin].[dbo].[REPORTS] rep, [CSCAdmin].[dbo].[Reports_in_Groups] rig " +
        //                                                        "WHERE rep.Report_Name = rig.Report_Name order by GID, rep.Report_Name").ToList(),

        public SalesReportViewModel GetSalesReportData(DateTime date, string store)
        {
            var reader = store == "" ?
                CSCTransDB.QueryMultiple("Sproc_DailySalesReportData", new { SALEDATE = date }, commandType: CommandType.StoredProcedure) :
                CSCTransDB.QueryMultiple("Sproc_DailySalesReportData", new { SALEDATE = date, STORE = store }, commandType: CommandType.StoredProcedure);

            List<FuelSalesModel> fuelSalesData = reader.Read<FuelSalesModel>().ToList();
            List<CStoreSalesModel> CStoreSalesData = reader.Read<CStoreSalesModel>().ToList();
            List<SalesTax> salesTaxesData = reader.Read<SalesTax>().ToList();
            List<SaleCharge> salesChargesData = reader.Read<SaleCharge>().ToList();
            List<SaleReceipt> salesReceipt = reader.Read<SaleReceipt>().ToList();

            SalesReportViewModel salesReportViewModel = new SalesReportViewModel
            {
                FuelSales = fuelSalesData,
                CStoreSales = CStoreSalesData,
                SalesTaxes = salesTaxesData,
                SalesCharges = salesChargesData,
                SalesReceipts = salesReceipt,
            };

            return salesReportViewModel;
        }

        public List<SalesDetails> GetSalesDetails(DateTime date, string dept, string stockCode, string store)
        {

            IEnumerable<SalesDetails> salesDetails;
            SqlBuilder builder = new SqlBuilder();
            SqlBuilder.Template selector;

            if (stockCode == "CSTORE")
            {
                selector = builder.AddTemplate(@"SELECT 
	                SL.SALE_NO SALENO, 
	                SL.TILL_NUM TillNum, 
	                SL.LINE_NUM LINENUM, 
	                SL.STOCK_CODE STOCKCODE, 
	                SL.DESCRIPT DESCR, 
	                SL.QUANTITY QTY, 
	                SL.PRICE PRICE, 
	                SL.AMOUNT AMOUNT
                FROM [SALEHEAD] SH /**innerjoin**/ /**where**/");

                if (store == "")
                {
                    builder.InnerJoin(@"[SALELINE] SL 
		                ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
	                LEFT JOIN [CSCMASTER].[DBO].[FUELDEPT] FD 
		                ON SL.DEPT = FD.DEPT
	                LEFT JOIN [CSCMASTER].[DBO].[DEPT] D 
		                ON D.DEPT = SL.DEPT");
                }
                else
                {
                    builder.InnerJoin(@"[SALELINE] SL 
		                ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM AND SH.Store_Id = SL.Store_Id
	                LEFT JOIN [CSCMASTER].[DBO].[FUELDEPT] FD 
		                ON SL.DEPT = FD.DEPT
	                LEFT JOIN [CSCMASTER].[DBO].[DEPT] D 
		                ON D.DEPT = SL.DEPT");
                }

                if (dept == "No ID")
                {
                    builder.Where(@"FD.DEPT IS NULL
                        AND (D.DEPT IS NULL OR SL.DEPT IS NULL)
                        AND SH.T_TYPE IN( 'SALE', 'REFUND', 'PATP_APP') 
		                AND SH.SHIFTDATE = @DATE",
                       new { DATE = date });
                }
                else
                {
                    builder.Where(@"FD.DEPT IS NULL
                        AND SH.T_TYPE IN( 'SALE', 'REFUND', 'PATP_APP') 
		                AND SH.SHIFTDATE = @DATE
                        AND SL.DEPT = @DEPT", new { DATE = date, DEPT = dept });
                }
            }
            else
            {
                selector = builder.AddTemplate(@"SELECT  
                        SL.SALE_NO AS SALENO, 
	                    SL.TILL_NUM AS TILLNUM, 
	                    SL.LINE_NUM AS LINENUM, 
	                    SL.STOCK_CODE AS STOCKCODE, 
	                    SL.DESCRIPT AS DESCR, 
	                    SL.QUANTITY AS QTY, 
	                    SL.PRICE AS PRICE, 
	                    SL.AMOUNT AS AMOUNT
                    FROM[SALEHEAD] SH /**innerjoin**/ /**where**/");

                if (store == "")
                {
                    builder.InnerJoin(@"[SALELINE] SL
                            ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM
                        INNER JOIN[CSCMASTER].[DBO].[FUELDEPT] FD
                            ON SL.DEPT = FD.DEPT
                        INNER JOIN[CSCMASTER].[DBO].[DEPT] D
                            ON D.DEPT = SL.DEPT");
                }
                else
                {
                    builder.InnerJoin(@"[SALELINE] SL
                            ON SH.SALE_NO = SL.SALE_NO AND SH.TILL = SL.TILL_NUM AND SH.Store_Id = SL.Store_Id
                        INNER JOIN[CSCMASTER].[DBO].[FUELDEPT] FD
                            ON SL.DEPT = FD.DEPT
                        INNER JOIN[CSCMASTER].[DBO].[DEPT] D
                            ON D.DEPT = SL.DEPT");
                }

                builder.Where(@"SH.T_TYPE IN( 'SALE', 'REFUND', 'PATP_APP')
                        AND SH.SHIFTDATE = @DATE
				        AND D.DEPT = @DEPT
                        AND SL.STOCK_CODE = @STOCKCODE", new { DATE = date, DEPT = dept, STOCKCODE = stockCode });
            }

            if (store != "")
            {
                builder.Where(@"(@STORE = 'All' OR SH.Store_Id = @STORE)", new { STORE = store });
            }

            salesDetails = CSCTransDB.Query<SalesDetails>(selector.RawSql, selector.Parameters);

            return salesDetails.ToList();
        }

        public List<SalesTaxDetails> GetSalesTaxesDetails(DateTime date, string taxName, string store)
        {
            SqlBuilder builder = new SqlBuilder();
            SqlBuilder.Template selector;

            selector = builder.AddTemplate(@"SELECT 
	                            ST.Sale_no AS SALENO, 
	                            Till_num AS TILLNUM, 
	                            Tax_Name AS TAXNAME, 
	                            Tax_added_amount AS ADDED, 
	                            Tax_Included_amount AS INCLUDED
                              FROM [S_SaleTax] ST /**innerjoin**/ /**where**/");

            if (store == "")
            {
                builder.InnerJoin(@"[SALEHEAD] SH 
	                            ON ST.Sale_No = SH.SALE_NO AND ST.TILL_NUM = SH.TILL");
            }
            else
            {
                builder.InnerJoin(@"[SALEHEAD] SH 
	                            ON ST.Sale_No = SH.SALE_NO AND ST.TILL_NUM = SH.TILL AND ST.Store_Id = SH.Store_Id");
            }

            builder.Where(@"SH.ShiftDate = @DATE
		                            AND ST.Tax_Name = @TAXNAME", new { DATE = date, TAXNAME = taxName });

            if (store != "")
            {
                builder.Where(@"(@STORE = 'All' OR SH.Store_Id = @STORE)", new { STORE = store });
            }

            IEnumerable<SalesTaxDetails> saleTaxDetails = CSCTransDB.Query<SalesTaxDetails>(selector.RawSql, selector.Parameters);

            return saleTaxDetails.ToList();
        }

        public List<SaleChargeDetails> GetSalesChargesDetails(DateTime date, string chargeNmae, string store)
        {
            SqlBuilder builder = new SqlBuilder();
            SqlBuilder.Template selector;

            if (chargeNmae == "Sale Charges")
            {
                selector = builder.AddTemplate(@"SELECT 
                            SC.SALE_NO AS SALENO,
	                        SC.TILL_NUM AS TILLNUM,
	                        SC.LINE_NO AS LINENUM, 
	                        SC.[DESCRIPTION] AS DESCR,
                            AMOUNT AS AMOUNT
                          FROM [SALECHG] SC /**innerjoin**/ /**where**/");

                if (store == "")
                {
                    builder.InnerJoin(@"[SALEHEAD] SH
                              ON SC.SALE_NO = SH.SALE_NO AND SC.TILL_NUM = SH.TILL");
                }
                else
                {
                    builder.InnerJoin(@"[SALEHEAD] SH
                              ON SC.SALE_NO = SH.SALE_NO AND SC.TILL_NUM = SH.TILL AND SC.Store_Id = SH.Store_Id");
                }
            }
            else
            {
                selector = builder.AddTemplate(@"SELECT 
	                        SC.Sale_no AS SALENO,
	                        SC.Till_num AS TILLNUM,
	                        SC.LINE_NO AS LINENUM, 
	                        SC.[Description] AS DESCR, 
	                        Amount AS AMOUNT
                          FROM [SaleKitChg] SC /**innerjoin**/ /**where**/");

                if (store == "")
                {
                    builder.InnerJoin(@"[SALEHEAD] SH
                              ON SC.SALE_NO = SH.SALE_NO AND SC.TILL_NUM = SH.TILL");
                }
                else
                {
                    builder.InnerJoin(@"[SALEHEAD] SH
                              ON SC.SALE_NO = SH.SALE_NO AND SC.TILL_NUM = SH.TILL AND SC.Store_Id = SH.Store_Id");
                }
            }

            builder.Where(@"SH.ShiftDate =  @DATE", new { DATE = date });

            if (store != "")
            {
                builder.Where(@"(@STORE = 'All' OR SH.Store_Id = @STORE)", new { STORE = store });
            }

            IEnumerable<SaleChargeDetails> saleChargeDetails = CSCTransDB.Query<SaleChargeDetails>(selector.RawSql, selector.Parameters);

            return saleChargeDetails.ToList();
        }

        public List<SaleReceiptDetails> GetSalesReceiptDetails(DateTime date, string tendName, string store)
        {
            SqlBuilder builder = new SqlBuilder();
            SqlBuilder.Template selector;

            IEnumerable<SaleReceiptDetails> saleReceiptDetails;
            if (tendName == "Payouts" || tendName == "Overpayments")
            {
                if (tendName == "Payouts")
                {
                    selector = builder.AddTemplate(@"SELECT 
	                        TILL AS TILLNUM,
	                        SH.SALE_NO AS SALENO,
	                        'Payouts' AS TENDNAME,
	                        ISNULL(TEND_AMT,0) AS AMOUNT
                        FROM [SALEHEAD] SH /**where**/");

                    builder.Where(@"SH.ShiftDate = @DATE
	                        AND SH.T_TYPE = 'PAYOUTS'", new { DATE = date });
                }
                else
                {
                    selector = builder.AddTemplate(@"SELECT 
	                        TILL AS TILLNUM,
	                        SH.SALE_NO AS SALENO,
	                        'Overpayments' AS TENDNAME,
	                        ISNULL(OverPayment,0) AS AMOUNT
                        FROM [SALEHEAD] SH /**where**/");

                    builder.Where(@"SH.ShiftDate = @DATE
	                        AND SH.T_TYPE = 'OVERPAYMENTS'", new { DATE = date });
                }

                if (store != "")
                {
                    builder.Where(@"(@STORE = 'All' OR SH.Store_Id = @STORE)", new { STORE = store });
                }

                saleReceiptDetails = CSCTransDB.Query<SaleReceiptDetails>(selector.RawSql, selector.Parameters);
            }
            else
            {
                selector = builder.AddTemplate(@"SELECT 
	                            TILL_NUM AS TILLNUM,
	                            ST.SALE_NO AS SALENO,
	                            SEQUENCE AS SEQUENCE, 
	                            TENDNAME AS TENDNAME,
	                            AMTTEND AS AMOUNT
	                        FROM [SALETEND] ST /**innerjoin**/ /**where**/");

                if (store == "")
                {
                    builder.InnerJoin(@"[SALEHEAD] SH
			                  ON SH.SALE_NO = ST.SALE_NO AND ST.TILL_NUM = SH.TILL");
                }
                else
                {
                    builder.InnerJoin(@"[SALEHEAD] SH
                              ON SH.SALE_NO = ST.SALE_NO AND ST.TILL_NUM = SH.TILL AND ST.Store_Id = SH.Store_Id");
                }

                builder.Where(@"SH.ShiftDate = @DATE
	                        AND ST.TENDNAME = @TENDNAME", new { DATE = date, TENDNAME = tendName });

                if (store != "")
                {
                    builder.Where(@"(@STORE = 'All' OR SH.Store_Id = @STORE)", new { STORE = store });
                }

                saleReceiptDetails = CSCTransDB.Query<SaleReceiptDetails>(selector.RawSql, selector.Parameters);
            }

            return saleReceiptDetails.ToList();
        }
        //public DataTable GetShelfLabelReport(string reportName, string dept, string subdept, string vendor)
        //{
        //    DataTable dt = new DataTable();

        //    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand("Sproc_" + reportName.Substring(0, 6), con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Connection = con;
        //        cmd.CommandTimeout = 0;
        //        cmd.Parameters.Add("@Dept", SqlDbType.NVarChar);
        //        cmd.Parameters.Add("@SubDept", SqlDbType.NVarChar);
        //        cmd.Parameters.Add("@Vendor", SqlDbType.NVarChar);

        //        cmd.Parameters["@Dept"].Value = dept;
        //        cmd.Parameters["@SubDept"].Value = subdept;
        //        cmd.Parameters["@Vendor"].Value = vendor;
        //        // con.Open();
        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        da.Fill(dt);
        //        //  con.Close();
        //    }
        //    return dt;
        //}
        public DataTable GetShelfLabelReport(string dept, string subdept, string vendor, string codetype)
        {
            DataTable dt = new DataTable();
            string sSQL = "";
            if (codetype == "1")
                sSQL = "Sproc_ShelfLabelReportData";
            else
                sSQL = "Sproc_ShelfLabelReportDataCode39";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCMasterConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sSQL, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                cmd.CommandTimeout = 0;
                cmd.Parameters.Add("@Dept", SqlDbType.NVarChar);
                cmd.Parameters.Add("@SubDept", SqlDbType.NVarChar);
                cmd.Parameters.Add("@Vendor", SqlDbType.NVarChar);

                cmd.Parameters["@Dept"].Value = dept;
                cmd.Parameters["@SubDept"].Value = subdept;
                cmd.Parameters["@Vendor"].Value = vendor;
                // con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                //  con.Close();
            }
            return dt;
        }
        public DataTable GetStockMovementReport(string startDate, string endDate, string dept, string subDept, string subDetail, string stockCode, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_StockMovementReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@endDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@Dept", SqlDbType.NVarChar);
                cmd.Parameters.Add("@SubDept", SqlDbType.NVarChar);
                cmd.Parameters.Add("@SubDetail", SqlDbType.NVarChar);
                cmd.Parameters.Add("@StockCode", SqlDbType.NVarChar);

                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@endDate"].Value = endDate;
                cmd.Parameters["@Dept"].Value = dept;
                cmd.Parameters["@SubDept"].Value = subDept;
                cmd.Parameters["@SubDetail"].Value = subDetail;
                cmd.Parameters["@StockCode"].Value = stockCode;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        public DataTable GetNoStockMovementReport(string startDate, string endDate, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_NoStockMovementReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@endDate", SqlDbType.DateTime);

                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@endDate"].Value = endDate;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        public DataTable GetHistoricalInventoryLevelReport(string date, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_HistoricalInventoryLevelReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@InvDate", SqlDbType.DateTime);
                cmd.Parameters["@InvDate"].Value = date;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        public DataTable GetDepartmentSalesReport(string startDate, string endDate, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_DepartmentSalesReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@endDate", SqlDbType.DateTime);

                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@endDate"].Value = endDate;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        public DataTable GetDataTable(string reportName, string date1, string date2, string sdate1, string sdate2, string dept1, string dept2,
             string subdept1, string subdept2, string subdetail1, string subdetail2,
             string stock1, string stock2, string class1, string class2, string cust1, string cust2, string emp1, string emp2,
             string vendor1, string vendor2, string till1, string till2, string tax1, string tax2, string orderno, string qtyoh)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
                {
                    //SqlCommand cmd = new SqlCommand("Sproc_" + reportName, con);
                    SqlCommand cmd = new SqlCommand("Sproc_" + reportName.Substring(0, 6), con);

                    cmd.CommandType = CommandType.StoredProcedure;
                    if (!String.IsNullOrEmpty(date1))
                    {
                        cmd.Parameters.Add("@date1", SqlDbType.DateTime);
                        cmd.Parameters["@date1"].Value = date1;
                    }
                    if (!String.IsNullOrEmpty(date2))
                    {
                        cmd.Parameters.Add("@date2", SqlDbType.DateTime);
                        cmd.Parameters["@date2"].Value = date2;
                    }
                    if (!String.IsNullOrEmpty(sdate1))
                    {
                        cmd.Parameters.Add("@sdate1", SqlDbType.DateTime);
                        cmd.Parameters["@sdate1"].Value = sdate1;
                    }
                    if (!String.IsNullOrEmpty(sdate2))
                    {
                        cmd.Parameters.Add("@sdate2", SqlDbType.DateTime);
                        cmd.Parameters["@sdate2"].Value = sdate2;
                    }
                    if (!String.IsNullOrEmpty(dept1))
                    {
                        cmd.Parameters.Add("@dept1", SqlDbType.VarChar);
                        cmd.Parameters["@dept1"].Value = dept1;
                    }
                    if (!String.IsNullOrEmpty(dept2))
                    {
                        cmd.Parameters.Add("@dept2", SqlDbType.VarChar);
                        cmd.Parameters["@dept2"].Value = dept2;
                    }
                    if (!String.IsNullOrEmpty(subdept1))
                    {
                        cmd.Parameters.Add("@subdept1", SqlDbType.VarChar);
                        cmd.Parameters["@subdept1"].Value = subdept1;
                    }
                    if (!String.IsNullOrEmpty(subdept2))
                    {
                        cmd.Parameters.Add("@subdept2", SqlDbType.VarChar);
                        cmd.Parameters["@subdept2"].Value = subdept2;
                    }
                    if (!String.IsNullOrEmpty(subdetail1))
                    {
                        cmd.Parameters.Add("@subdetail1", SqlDbType.VarChar);
                        cmd.Parameters["@subdetail1"].Value = subdetail1;
                    }
                    if (!String.IsNullOrEmpty(subdetail2))
                    {
                        cmd.Parameters.Add("@subdetail2", SqlDbType.VarChar);
                        cmd.Parameters["@subdetail2"].Value = subdetail2;
                    }
                    if (!String.IsNullOrEmpty(stock1))
                    {
                        cmd.Parameters.Add("@stock1", SqlDbType.VarChar);
                        cmd.Parameters["@stock1"].Value = stock1;
                    }
                    if (!String.IsNullOrEmpty(stock2))
                    {
                        cmd.Parameters.Add("@stock2", SqlDbType.VarChar);
                        cmd.Parameters["@stock2"].Value = stock2;
                    }
                    if (!String.IsNullOrEmpty(class1))
                    {
                        cmd.Parameters.Add("@class1", SqlDbType.VarChar);
                        cmd.Parameters["@class1"].Value = class1;
                    }
                    if (!String.IsNullOrEmpty(class2))
                    {
                        cmd.Parameters.Add("@class2", SqlDbType.VarChar);
                        cmd.Parameters["@class2"].Value = class2;
                    }
                    if (!String.IsNullOrEmpty(cust1))
                    {
                        cmd.Parameters.Add("@cust1", SqlDbType.VarChar);
                        cmd.Parameters["@cust1"].Value = cust1;
                    }
                    //else cmd.Parameters["@cust1"].Value = DBNull.Value;

                    if (!String.IsNullOrEmpty(cust2))
                    {
                        cmd.Parameters.Add("@cust2", SqlDbType.VarChar);
                        cmd.Parameters["@cust2"].Value = cust2;
                    }
                    if (!String.IsNullOrEmpty(emp1))
                    {
                        cmd.Parameters.Add("@emp1", SqlDbType.VarChar);
                        cmd.Parameters["@emp1"].Value = emp1;
                    }

                    if (!String.IsNullOrEmpty(emp2))
                    {
                        cmd.Parameters.Add("@emp2", SqlDbType.VarChar);
                        cmd.Parameters["@emp2"].Value = emp2;
                    }

                    if (!String.IsNullOrEmpty(vendor1))
                    {
                        cmd.Parameters.Add("@vendor1", SqlDbType.VarChar);
                        cmd.Parameters["@vendor1"].Value = vendor1;
                    }

                    if (!String.IsNullOrEmpty(vendor2))
                    {
                        cmd.Parameters.Add("@vendor2", SqlDbType.VarChar);
                        cmd.Parameters["@vendor2"].Value = vendor2;
                    }
                    if (!String.IsNullOrEmpty(till1))
                    {
                        cmd.Parameters.Add("@till1", SqlDbType.VarChar);
                        cmd.Parameters["@till1"].Value = till1;
                    }

                    if (!String.IsNullOrEmpty(till2))
                    {
                        cmd.Parameters.Add("@till2", SqlDbType.VarChar);
                        cmd.Parameters["@till2"].Value = till2;
                    }
                    if (!String.IsNullOrEmpty(tax1))
                    {
                        cmd.Parameters.Add("@tax1", SqlDbType.VarChar);
                        cmd.Parameters["@tax1"].Value = tax1;
                    }

                    if (!String.IsNullOrEmpty(tax2))
                    {
                        cmd.Parameters.Add("@tax2", SqlDbType.VarChar);
                        cmd.Parameters["@tax2"].Value = tax2;
                    }
                    if (!String.IsNullOrEmpty(orderno))
                    {
                        cmd.Parameters.Add("@orderno", SqlDbType.VarChar);
                        cmd.Parameters["@orderno"].Value = orderno;
                    }
                    if (!String.IsNullOrEmpty(qtyoh))
                    {
                        cmd.Parameters.Add("@qtyoh", SqlDbType.VarChar);
                        cmd.Parameters["@qtyoh"].Value = qtyoh;
                    }

                    //else cmd.Parameters["@cust2"].Value = DBNull.Value;

                    //if (store != "")
                    //{
                    //    cmd.Parameters.Add("@store", SqlDbType.NVarChar);
                    //    cmd.Parameters["@store"].Value = store;
                    //}

                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                WriteLog.WriteLine("Error: " + ex.Message);

            }
            return dt;
        }


        public DataTable GetHourlySalesReport(string startDate, string endDate, string dept, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_HourlySalesReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@startDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@endDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@dept", SqlDbType.NVarChar);

                cmd.Parameters["@startDate"].Value = startDate;
                cmd.Parameters["@endDate"].Value = endDate;
                cmd.Parameters["@dept"].Value = dept;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        public DataTable GetQuantityLeaderReport(string startDate, string endDate, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_QuantityLeaderReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime);
                cmd.Parameters.Add("@endDate", SqlDbType.DateTime);

                cmd.Parameters["@StartDate"].Value = startDate;
                cmd.Parameters["@endDate"].Value = endDate;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        public DataTable GetDeptSalesLastSixWeeksReport(string today, string store)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CSCTransConnectionString"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Sproc_DepartmentSalesLast6WeeksReportData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@today", SqlDbType.DateTime);

                cmd.Parameters["@today"].Value = today;

                if (store != "")
                {
                    cmd.Parameters.Add("@Store", SqlDbType.NVarChar);
                    cmd.Parameters["@Store"].Value = store;
                }

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
            }

            return dt;
        }

        #endregion
    }
}
