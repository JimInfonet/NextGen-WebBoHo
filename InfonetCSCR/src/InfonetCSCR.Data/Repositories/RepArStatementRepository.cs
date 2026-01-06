
using Dapper;
using InfonetCSCR.Core.Dtos;
using InfonetCSCR.Core.Models;
using Microsoft.VisualBasic;
using System;
using System.Transactions;

namespace InfonetCSCR.Data.Repositories;

public class RepArStatRepository : BaseRepository, IRepArStatRepository
{
    public RepArStatRepository(IDbTransaction transaction) : base(transaction) { }

    public async Task<IEnumerable<CustomersDto>> GetAllCustomersAsync(bool orderByCode)
    {
        StringBuilder query = new StringBuilder();
        query.Append($@"SELECT CL_Code AS [Code],
                           CL_Name AS [Name],
                           CL_Phone AS [Phone]
                    FROM Client
                   {(orderByCode ? "ORDER BY CL_Code" : "ORDER BY CL_Name")} ;");
        var result = await Connection.QueryAsync<CustomersDto>(query.ToString(), null, Transaction);
        return result;
    }
    public async Task<ArStatementDto> GetRepArStat(string client, DateTime dDate)
    {
        // Ensure the start and end dates cover the whole month
        DateTime startDate = new DateTime(dDate.Year, dDate.Month, 1);
        DateTime endDate = dDate;

        // Parameterized SQL to avoid SQL injection
        StringBuilder queryComp = new StringBuilder();
        queryComp.Append(@"
                            SELECT
                                Company_Name as comp,
                                Address_1 + ' ' + Address_2 as addr1,
                                City + ', ' + Province + ', ' + Postal_Code as addr2
                                FROM CSCAdmin.dbo.Setup_C");

        StringBuilder querySale = new StringBuilder();
        querySale.Append(@"
                            SELECT
                                CAST(h.T_Type AS varchar(10)) AS T_Type, CAST(h.Sale_No AS varchar(20)) AS Sale_No,
                                CAST(h.Sale_Date AS datetime) AS Sale_Date,
                                CAST(
                                    CASE
                                        WHEN h.T_Type = 'ARPAY' THEN h.Payment
                                        WHEN t.TendClas = 'ACCOUNT' THEN t.AmtTend
                                        ELSE 0
                                    END
                                    AS decimal(18, 2)
                                ) AS Amount,
                                CAST(
                                    CASE
                                        WHEN h.T_Type = 'ARPAY' THEN ''
                                        ELSE ISNULL(t.SERNUM, '')
                                    END
                                    AS varchar(50)
                                ) AS SERNUM,
                                CAST(
                                    CASE
                                        WHEN h.T_Type = 'ARPAY' THEN ''
                                        ELSE ISNULL(t.CCARD_NUM, '')
                                    END
                                    AS varchar(50)
                                ) AS CCARD_NUM
                            FROM CSCTrans.dbo.SaleHead h
                            LEFT JOIN CSCTrans.dbo.SaleTend t ON h.Sale_No = t.Sale_No AND t.TendClas = 'ACCOUNT'
                            WHERE h.Client = @Client
                              AND h.Sale_Date >= @StartDate
                              AND h.Sale_Date<@EndDate
                              AND(h.T_Type = 'ARPAY' OR t.TendClas = 'ACCOUNT')");


        var parameters = new
        {
            Client = client,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = new ArStatementDto();
        try
        {
            var compResult = await Connection.QuerySingleOrDefaultAsync<CompanyDto>(queryComp.ToString(), null, transaction: Transaction);

            result.header = compResult as CompanyDto ?? new CompanyDto();

            var arResult = await Connection.QueryAsync<ArSalesDto>(querySale.ToString(), parameters, transaction: Transaction);

            result.details = arResult  as List<ArSalesDto> ?? new List<ArSalesDto>();
        }
        catch (Exception ex)
        {
            // Return empty list to prevent breaking the frontend
            return result;
        }

        return result;
    }
    public static DateTime ToSmallDateTime(DateTime dt)
    {
        // SQL Server smalldatetime rounds to nearest minute
        if (dt.Second >= 30)
            dt = dt.AddMinutes(1);

        // Truncate seconds and milliseconds
        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

        // Enforce smalldatetime range
        DateTime min = new DateTime(1900, 1, 1);
        DateTime max = new DateTime(2079, 6, 6, 23, 59, 0);

        if (dt < min) return min;
        if (dt > max) return max;

        return dt;
    }
    public async Task<bool> GenMonthClosing(string client, DateTime dDate)
    {
        bool bResult = false;
        DateTime smallDT = ToSmallDateTime(dDate);
        
        try
        {
            // delete all closing  to reset
            var sSQL = @"DELETE FROM CSCTrans.dbo.CloseArCustomer WHERE CustomerCode=@Code and ClosingMonth=@dDate";


            int rowsAffected = await Connection.ExecuteAsync(
                sSQL,
                new { Code = client, dDate = dDate }, // parameter value
                Transaction
            );

            if (rowsAffected > 0)
            {
                Console.WriteLine("Delete successful");
            }
            else
            {
                Console.WriteLine("No records deleted");
            }

            // go through all the client to generate the data
            // Load AR Customers
            var customers = new List<string>();
            using (var cmd = Connection.CreateCommand())
            {
                cmd.Transaction = Transaction;
                cmd.CommandText = "SELECT Cl_Code FROM Client WHERE CL_ARCUST = 1 ORDER BY Cl_Code";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(reader["Cl_Code"].ToString());
                    }
                }
            }

            // Insert into CloseArCustomer
            foreach (var clCode in customers)
            {
                double balance = GetBalance(clCode, dDate, false); // your existing logic

                using (var cmd = Connection.CreateCommand())
                {
                    cmd.Transaction = Transaction;
                    cmd.CommandText = @"
                INSERT INTO CSCTrans.dbo.CloseArCustomer (CustomerCode, ClosingMonth, ClosingBalance) 
                VALUES (@CustomerCode, @ClosingMonth, @ClosingBalance)";

                    var p1 = cmd.CreateParameter();
                    p1.ParameterName = "@CustomerCode";
                    p1.Value = clCode;
                    cmd.Parameters.Add(p1);

                    var p2 = cmd.CreateParameter();
                    p2.ParameterName = "@ClosingMonth";
                    p2.Value = dDate;
                    cmd.Parameters.Add(p2);

                    var p3 = cmd.CreateParameter();
                    p3.ParameterName = "@ClosingBalance";
                    p3.Value = balance;
                    cmd.Parameters.Add(p3);

                    cmd.ExecuteNonQuery();
                }
            }

            // Get last closed date
            DateTime? lastClosedDate = null;
            using (var cmd = Connection.CreateCommand())
            {
                cmd.Transaction = Transaction;
                cmd.CommandText = "SELECT MAX(ClosingMonth) AS ClMonth FROM CSCTrans.dbo.CloseArCustomer";
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    lastClosedDate = Convert.ToDateTime(result);
                }
            }

            bResult = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            bResult = false;
        }
        return bResult;
    }
    public double GetBalance(string customerCode, DateTime dDate, bool printIt = false)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return 0;

        var connection = Connection;

        double dblOpenBalance = 0;
        double dblPayment = 0;
        double dblSale = 0;

        // Calculate closing date (VB6 used DateAdd trick)
        DateTime balOpenDate = new DateTime(dDate.Year, dDate.Month, 1);
        DateTime balClosedDate = dDate;

        // Get previous closing balance
        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = Transaction;
            cmd.CommandText = @"
            SELECT ClosingBalance, ClosingMonth 
            FROM CSCTrans.dbo.CloseARCustomer
            WHERE CustomerCode = @CustomerCode AND ClosingMonth = @ClosingMonth";

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@CustomerCode";
            p1.Value = customerCode;
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@ClosingMonth";
            p2.Value = balClosedDate.ToString("yyyyMMdd");
            cmd.Parameters.Add(p2);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (!(reader["ClosingBalance"] is DBNull))
                        dblOpenBalance = Convert.ToDouble(reader["ClosingBalance"]);
                }
            }
        }

        // Main query combining AR payments and sales
        string strSQL = @"
            SELECT T_Type, Sale_No, Sale_Date, Payment AS Amount, '' AS SERNUM, '' AS CCARD_NUM
            FROM CSCTrans.dbo.SaleHead  
            WHERE Client = @CustomerCode AND T_Type = 'ARPAY'
            AND Sale_Date BETWEEN @StartDate AND @CloseDate
            UNION ALL
            SELECT T_Type, SaleHead.Sale_No, SaleHead.Sale_Date, SaleTend.AmtTend AS Amount, 
                   SaleTend.SERNUM, SaleTend.CCARD_NUM
            FROM CSCTrans.dbo.SaleHead 
            INNER JOIN CSCTrans.dbo.SaleTend ON SaleHead.Sale_No = SaleTend.Sale_No
            WHERE SaleHead.Client = @CustomerCode
              AND SaleTend.TendClas = 'ACCOUNT'
              AND SaleHead.Sale_Date BETWEEN @StartDate AND @CloseDate
            ORDER BY Sale_Date, Sale_No";

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = Transaction;
            cmd.CommandText = strSQL;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@CustomerCode";
            p1.Value = customerCode;
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@StartDate";
            p2.Value = balOpenDate.ToString("yyyyMMdd");
            cmd.Parameters.Add(p2);

            var p3 = cmd.CreateParameter();
            p3.ParameterName = "@CloseDate";
            p3.Value = balClosedDate.ToString("yyyyMMdd");
            cmd.Parameters.Add(p3);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string type = reader["T_Type"].ToString();
                    double amount = Convert.ToDouble(reader["Amount"]);

                    if (type == "SALE" || type == "REFUND" || type == "PATP_APP")
                    {
                        dblSale += amount;
                        if (printIt)
                        {
                            // TODO: instead of VB6 Print, write to StreamWriter for reporting
                            // Example:
                            // sw.WriteLine($"{reader["Sale_Date"]:dd-MMM-yyyy} ... {amount} ... {runningBalance}");
                        }
                    }
                    else if (type == "ARPAY")
                    {
                        dblPayment += amount;
                        if (printIt)
                        {
                            // TODO: write ARPAY line to output
                        }
                    }
                }
            }
        }

        double balance = dblOpenBalance + dblSale - dblPayment;

        if (printIt)
        {
            // Example: write statement to file
            string fileName = Path.Combine(Environment.CurrentDirectory, "CustomerStatement.txt");
            using (var sw = new StreamWriter(fileName, append: false))
            {
                sw.WriteLine($"Statement for {customerCode}");
                sw.WriteLine($"Opening Balance: {dblOpenBalance:0.00}");
                sw.WriteLine($"Sales: {dblSale:0.00}");
                sw.WriteLine($"Payments: {dblPayment:0.00}");
                sw.WriteLine($"Closing Balance: {balance:0.00}");
            }
        }

        return balance;
    }

    public Task DeleteByMonthAsync(string customerCode, DateTime closeDate)
    {
        string sql = @"DELETE FROM CloseArCustomer WHERE ClosingMonth = @CloseMonth";
        return Connection.ExecuteAsync(sql, new { CloseMonth = closeDate }, Transaction);
    }

    public Task InsertCustomerAsync(string customerCode, DateTime closeDate)
    {
        string sql = @"INSERT INTO CloseArCustomer (CustomerCode, ClosingMonth, ClosingBalance)
                       VALUES (@CustomerCode, @closeDate, 200.99)";
        //VALUES(@CustomerCode, @ClosingMonth, @ClosingBalance)";
        return Connection.ExecuteAsync(sql, new
        {
            CustomerCode = customerCode,
            ClosingMonth = closeDate,
            ClosingBalance = 200.99
        }, Transaction);
    }

    public Task<DateTime?> GetLastClosingMonthAsync()
    {
        string sql = @"SELECT MAX(ClosingMonth) AS ClMonth FROM CloseArCustomer";
        return Connection.QuerySingleOrDefaultAsync<DateTime?>(sql, transaction: Transaction);
    }
}
