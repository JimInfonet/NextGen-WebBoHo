namespace InfonetCSCR.Data.Repositories;

//CSCAdmin Connection
public class CompanySetupRepository : BaseRepository, ICompanySetupRepository
{
    private Company? _company;
    private readonly IApplicationConfig _config;
    private IDbTransaction _transaction;

    public CompanySetupRepository(IApplicationConfig _applicationConfig, IDbTransaction transaction) : base(transaction)
    {
        _config = _applicationConfig;
        _transaction = transaction;
    }

    public async Task<Company?> GetCompanyAsync()
    {
        if (_company != null)
            return _company;

        string sql = (_config.IsBackOffice) ?
                            @"SELECT TOP 1
													[Company_Name] CompanyName,
													[Address_1] Address1,
													[Address_2] Address2,
													[City] City,
													[Province] Province,
													[Postal_code] PostalCode,
													[Tel_Num] TelephoneNumber,
													[Fax_Num] FaxNumber,
													[Country] Country,
													[Reg_Name] RegistrationName,
													[Reg_Num] RegistrationNumber,
													[Security_Key] SecurityKey,
													[Install_Date] InstallDate,
													[Language] Language,
													[Store] StoreCode,
													[Color] Color,
													[POS_BO_Features] POSBackOfficeFeatures,
													[Pump_Features] PumpFeatures,
													[NIC_Number] NICNumber,
													[Num_OF_POS] NumberOfPOS,
													[MaxConcurrentPOS] MaxConcurrentPOS,
													[Sec_Reg_Name] SecondaryRegistrationName,
													[Sec_Reg_Num] SecondaryRegistrationNumber,
													[AgentNumber] AgentNumber
												FROM [CSCAdmin].[dbo].[Setup_C]" :
                            @"SELECT TOP 1
                          [Company_Name] CompanyName,
                          [Address_1] Address1,
                          [Address_2] Address2,
                          [City] City,
                          [Province] Province,
                          [Postal_code] PostalCode,
                          [Tel_Num] TelephoneNumber,
                          [Fax_Num] FaxNumber,
                          [Country] Country,
                          [Language] Language,
                          [MaxConcurrentPOS] MaxConcurrentPOS,
                          [Sec_Reg_Name] SecondaryRegistrationName,
                          [Sec_Reg_Num] SecondaryRegistrationNumber,
                          [AgentNumber] AgentNumber,
                          [Company_Short_Name] CompanyShortName,
                          [KeyNumber] KeyNumber,
                          [Parent_ID] ParentID,
                          [Company_ID] CompanyID,
                          [Store] StoreCode
                        FROM [CSCAdmin].[dbo].[Setup_C]";

        _company = await Connection.QuerySingleOrDefaultAsync<Company?>(sql, null, Transaction);
        return _company;
    }
}
