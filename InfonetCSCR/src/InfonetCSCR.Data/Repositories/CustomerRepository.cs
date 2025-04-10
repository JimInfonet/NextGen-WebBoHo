namespace InfonetCSCR.Data.Repositories;

public class CustomerRepository : BaseRepository, ICustomerRepository
{
  public CustomerRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<CustomersDto>> GetAllCustomersAsync(bool orderByCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT CL_Code AS [Code],
                           CL_Name AS [Name],
                           CL_Phone AS [Phone]
                    FROM Client
                   {(orderByCode ? "ORDER BY CL_Code" : "ORDER BY CL_Name")} ;");
    IEnumerable<CustomersDto> customers = await Connection.QueryAsync<CustomersDto>(query.ToString(), null, Transaction);
    return customers;
  }

  public async Task<bool> AddCustomerAsync(Customer customer)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[CLIENT] (
                        [CL_CODE],
                        [CL_NAME],
                        [CL_CATEG],
                        [CL_ADD1],
                        [CL_ADD2],
                        [CL_CITY],
                        [CL_PROV],
                        [CL_COUNTRY],
                        [CL_POSTAL],
                        [CL_ACODE],
                        [CL_PHONE],
                        [CL_OFF_PH],
                        [CL_CELL],
                        [CL_FAX],
                        [CL_TOLL],
                        [CL_EMAIL],
                        [CL_CONT1],
                        [CL_CONT2],
                        [LO_NUM],
                        [LO_POINTS],
                        [CL_ARCUST],
                        [CL_TYPE],
                        [TERMS],
                        [DISCOUNT_DAYS],
                        [Early_Pay_Percent],
                        [CUST_DISC],
                        [PRICE_CODE],
                        [CL_HISTBAL],
                        [CL_HPAID],
                        [CL_LIMIT],
                        [CL_CURBAL],
                        [CL_STATUS],
                        [PARENT_ACCT],
                        [UsePO],
                        [GroupID],
                        [UseFuelRebate],
                        [UseFuelRebateDiscount],
                        [CL_Note],
                        [TaxExempt],
                        [PlateNumber],
                        [MultiUse_PO],
                        [TECardNumber],
                        [ProfileID]
                    )
                    VALUES (
                        @Code,
                        @Name,
                        @Category,
                        @Address1,
                        @Address2,
                        @City,
                        @Province,
                        @Country,
                        @PostalCode,
                        @AreaCode,
                        @Phone,
                        @OfficePhone,
                        @CellPhone,
                        @Fax,
                        @Toll,
                        @Email,
                        @Contact1,
                        @Contact2,
                        @LoyaltyNumber,
                        @LoyaltyPoints,
                        @ARCustomer,
                        @Type,
                        @Terms,
                        @DiscountDays,
                        @EarlyPayPercent,
                        @CustomerDiscount,
                        @PriceCode,
                        @HistoricalBalance,
                        @HistoricalPaid,
                        @CreditLimit,
                        @CurrentBalance,
                        @Status,
                        @ParentAccount,
                        @UsePurchaseOrder,
                        @GroupID,
                        @UseFuelRebate,
                        @UseFuelRebateDiscount,
                        @Note,
                        @TaxExempt,
                        @PlateNumber,
                        @MultiUsePurchaseOrder,
                        @CardNumber,
                        @ProfileID
                    )";

    int rowsAffected = await Connection.ExecuteAsync(sql, customer, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateCustomerAsync(Customer customer)
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[CLIENT]
                        SET
                            [CL_CODE] = @Code,
                            [CL_NAME] = @Name,
                            [CL_CATEG] = @Category,
                            [CL_ADD1] = @Address1,
                            [CL_ADD2] = @Address2,
                            [CL_CITY] = @City,
                            [CL_PROV] = @Province,
                            [CL_COUNTRY] = @Country,
                            [CL_POSTAL] = @PostalCode,
                            [CL_ACODE] = @AreaCode,
                            [CL_PHONE] = @Phone,
                            [CL_OFF_PH] = @OfficePhone,
                            [CL_CELL] = @CellPhone,
                            [CL_FAX] = @Fax,
                            [CL_TOLL] = @Toll,
                            [CL_EMAIL] = @Email,
                            [CL_CONT1] = @Contact1,
                            [CL_CONT2] = @Contact2,
                            [LO_NUM] = @LoyaltyNumber,
                            [LO_POINTS] = @LoyaltyPoints,
                            [CL_ARCUST] = @ARCustomer,
                            [CL_TYPE] = @Type,
                            [TERMS] = @Terms,
                            [DISCOUNT_DAYS] = @DiscountDays,
                            [Early_Pay_Percent] = @EarlyPayPercent,
                            [CUST_DISC] = @CustomerDiscount,
                            [PRICE_CODE] = @PriceCode,
                            [CL_HISTBAL] = @HistoricalBalance,
                            [CL_HPAID] = @HistoricalPaid,
                            [CL_LIMIT] = @CreditLimit,
                            [CL_CURBAL] = @CurrentBalance,
                            [CL_STATUS] = @Status,
                            [PARENT_ACCT] = @ParentAccount,
                            [UsePO] = @UsePurchaseOrder,
                            [GroupID] = @GroupID,
                            [UseFuelRebate] = @UseFuelRebate,
                            [UseFuelRebateDiscount] = @UseFuelRebateDiscount,
                            [CL_Note] = @Note,
                            [TaxExempt] = @TaxExempt,
                            [PlateNumber] = @PlateNumber,
                            [MultiUse_PO] = @MultiUsePurchaseOrder,
                            [TECardNumber] = @CardNumber,
                            [ProfileID] = CASE WHEN @ARCustomer = 1 THEN @ProfileID ELSE COALESCE([ProfileID], @ProfileID) END
                        WHERE [CL_CODE] = @Code";

    int rowsAffected = await Connection.ExecuteAsync(sql, customer, Transaction);
    return rowsAffected > 0;
  }

  private string GetCustomerQueryString()
  {
    return $@"SELECT 
                        [CL_CODE] AS Code,
                        [CL_NAME] AS Name,
                        [CL_CATEG] AS Category,
                        [CL_ADD1] AS Address1,
                        [CL_ADD2] AS Address2,
                        [CL_CITY] AS City,
                        [CL_PROV] AS Province,
                        [CL_COUNTRY] AS Country,
                        [CL_POSTAL] AS PostalCode,
                        [CL_ACODE] AS AreaCode,
                        [CL_PHONE] AS Phone,
                        [CL_OFF_PH] AS OfficePhone,
                        [CL_CELL] AS CellPhone,
                        [CL_FAX] AS Fax,
                        [CL_TOLL] AS Toll,
                        [CL_EMAIL] AS Email,
                        [CL_CONT1] AS Contact1,
                        [CL_CONT2] AS Contact2,
                        [LO_NUM] AS LoyaltyNumber,
                        [LO_POINTS] AS LoyaltyPoints,
                        [CL_ARCUST] AS ARCustomer,
                        [CL_TYPE] AS Type,
                        [TERMS] AS Terms,
                        [DISCOUNT_DAYS] AS DiscountDays,
                        [Early_Pay_Percent] AS EarlyPayPercent,
                        [CUST_DISC] AS CustomerDiscount,
                        [PRICE_CODE] AS PriceCode,
                        [CL_HISTBAL] AS HistoricalBalance,
                        [CL_HPAID] AS HistoricalPaid,
                        [CL_LIMIT] AS CreditLimit,
                        [CL_CURBAL] AS CurrentBalance,
                        [CL_STATUS] AS Status,
                        [PARENT_ACCT] AS ParentAccount,
                        [UsePO] AS UsePurchaseOrder,
                        [GroupID] AS GroupID,
                        [UseFuelRebate] AS UseFuelRebate,
                        [UseFuelRebateDiscount] AS UseFuelRebateDiscount,
                        [CL_Note] AS Note,
                        [TaxExempt] AS TaxExempt,
                        [PlateNumber] AS PlateNumber,
                        [MultiUse_PO] AS MultiUsePurchaseOrder,
                        [TECardNumber] AS CardNumber,
                        [ProfileID] AS ProfileID
                    FROM
                        [CSCMaster].[dbo].[CLIENT]";
  }

  public async Task<Customer> GetCustomerAsync(string code)
  {
    string sql = GetCustomerQueryString() + " WHERE CL_CODE = @Code";
    Customer customer = await Connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Code = code }, Transaction);
    return customer;
  }

  public async Task<Customer> GetCustomerByLoyaltyNumAsync(string loyaltyNum)
  {
    string sql = GetCustomerQueryString() + " WHERE LO_NUM = @LONum";
    Customer customer = await Connection.QueryFirstOrDefaultAsync<Customer>(sql, new { LONum = loyaltyNum }, Transaction);
    return customer;
  }

  public async Task<bool> DeleteClientCardByCodeAsync(string code)
  {
    const string sql = @"Delete From ClientCard 
                         where CL_Code = @Code";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { Code = code }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteClientAsync(string code)
  {
    string query = @"DELETE FROM CSCMaster.dbo.Client 
                        WHERE CL_CODE = @Code";

    int rowsAffected = await Connection.ExecuteAsync(query, new { Code = code }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<CustomerGroup>> GetAllCustomerGroupsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT [GroupID],
                        [GroupName],
                        [DiscountType],
                        [DiscountRate],
                        [LoyaltyFooter],
                        [DiscountName]
                    FROM 
                        [CSCMaster].[dbo].[ClientGroup];");

    IEnumerable<CustomerGroup> groups = await Connection.QueryAsync<CustomerGroup>(query.ToString(), null, Transaction);
    return groups;
  }

  public async Task<bool> AddClientChangeAsync(ClientChanges clientChange)
  {
    string query = $@"INSERT INTO [CSCTransfer].[dbo].[ClientChanges] (
                        [CL_Code],
                        [OP_Status],
                        [Trans_Flag]
                    )
                     VALUES (
                        @Code,
                        @OPStatus,
                        @TransFlag
                     )";

    int rowsAffected = await Connection.ExecuteAsync(query, clientChange, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteClientChangeAsync(ClientChanges clientChange)
  {
    string query = $@"Delete FROM  [CSCTransfer].[dbo].[ClientChanges]
                    where CL_Code = @Code and OP_Status = @OPStatus and Trans_Flag = @TransFlag;";

    int rowsAffected = await Connection.ExecuteAsync(query, clientChange, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> AddArCustomerAsync(ArCustomer arCustomer)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[ArCustomer] (
                        [CustCode],
                        [StmtEmail],
                        [DateOpen],
                        [LastStmtBal],
                        [LastStmtDate],
                        [LastSaleDate],
                        [HighStmlBal],
                        [HighBalDate],
                        [LastPymtBal],
                        [LastPymtDate]
                    )
                    VALUES (
                        @Code,
                        @StmtEmail,
                        @DateOpen,
                        @LastStmtBalance,
                        @LastStmtDate,
                        @LastSaleDate,
                        @HighStmtBalance,
                        @HighSaleDate,
                        @LastPaymentBalance,
                        @LastPaymentDate
                    );";

    int rowsAffected = await Connection.ExecuteAsync(sql, arCustomer, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateArCustomerAsync(ArCustomer arCustomer)
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[ArCustomer]
                    SET
                        [StmtEmail] = @StmtEmail
                    WHERE
                        [CustCode] = @Code;";

    int rowsAffected = await Connection.ExecuteAsync(sql, arCustomer, Transaction);
    return rowsAffected > 0;
  }

  public async Task<ArCustomer> GetArCustomerAsync(string custCode)
  {
    string sql = $@"SELECT
                        [CustCode] AS Code,
                        [StmtEmail],
                        [DateOpen],
                        [LastStmtBal] AS LastStmtBalance,
                        [LastStmtDate],
                        [LastSaleDate],
                        [HighStmlBal] AS HighStmtBalance,
                        [HighBalDate] AS HighSaleDate,
                        [LastPymtBal] AS LastPaymentBalance,
                        [LastPymtDate] AS LastPaymentDate
                    FROM [CSCMaster].[dbo].[ArCustomer]
                    WHERE [CustCode] = @CustCode;";

    var arCustomer = await Connection.QueryFirstOrDefaultAsync<ArCustomer>(sql, new { CustCode = custCode }, Transaction);
    return arCustomer;
  }

  public async Task<bool> DeleteArCustomerAsync(string custCode)
  {
    string sql = $@"DELETE FROM [CSCMaster].[dbo].[ArCustomer]
                    WHERE [CustCode] = @CustCode;";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { CustCode = custCode }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<string> GetSecurityARAsync()
  {
    string query = $@"
                SELECT TOP (1) [POS_BO_Features]
                FROM [CSCAdmin].[dbo].[Setup_C]";

    string posBoFeatures = await Connection.QueryFirstOrDefaultAsync<string>(query, null, Transaction);
    return posBoFeatures;
  }
}
