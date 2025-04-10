namespace InfonetCSCR.Data.Repositories;

public class RegistersRepository : BaseRepository, IRegistersRepository
{

  public RegistersRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<int>> GetRegistersAsync()
  {
    string sql = @$"SELECT DISTINCT REG_NO from [CSCAdmin].[dbo].[REGISTER]";

    var registers = await Connection.QueryAsync<int>(sql, null, Transaction);
    return registers;
  }

  public async Task<IEnumerable<Register>> GetRegistersByRegNoAsync(int regNo)
  {
      string sql = @$"SELECT
                      [REG_NO] AS RegNo,
                      [DeviceID] AS DeviceID,
                      [Active] AS Active,
                      [UseOPOS] AS UseOPOS,
                      [DeviceName] AS DeviceName,
                      [PortNum] AS PortNum,
                      [PortSetting] AS PortSetting,
                      [DriverName] AS DriverName,
                      [FontName] AS FontName,
                      [FontSize] AS FontSize,
                      [WritePosLog] AS WritePosLog
                    FROM [CSCAdmin].[dbo].[Register]
                    WHERE REG_NO = @RegNo";

      var registers = await Connection.QueryAsync<Register>(sql, new { RegNo = regNo }, Transaction);
      return registers;
    }

  public async Task<IEnumerable<CustomerDisplay>> GetCustomerDisplayAsync()
  {
    string sql = @$"SELECT REG_DIS_Code AS REGDISCode, Description FROM [CSCAdmin].[dbo].[CustomerDisplay]";

    var customerDisplays = await Connection.QueryAsync<CustomerDisplay>(sql, null, Transaction);
    return customerDisplays;
  }

  public async Task<IEnumerable<Device>> GetDevicesAsync()
  {
    string sql = @$"SELECT * FROM [CSCAdmin].[dbo].[Device]";

    var devices = await Connection.QueryAsync<Device>(sql, null, Transaction);
    return devices;
  }

  public async Task<bool> AddRegisterAsync(List<Register> register)
  {
    string sql = @$"INSERT INTO [CSCAdmin].[dbo].[REGISTER] (
                        [REG_NO],
                        [DeviceID],
                        [Active],
                        [UseOPOS],
                        [DeviceName],
                        [PortNum],
                        [PortSetting],
                        [DriverName],
                        [FontName],
                        [FontSize],
                        [WritePosLog]
                    ) VALUES (
                        @RegNo,
                        @DeviceID,
                        @Active,
                        @UseOPOS,
                        @DeviceName,
                        @PortNum,
                        @PortSetting,
                        @DriverName,
                        @FontName,
                        @FontSize,
                        @WritePosLog
                      )";

    int rowsAffected = await Connection.ExecuteAsync(sql, register, Transaction);
    return rowsAffected == 8;               // Will effect 8 rows, one for each device
  }

  public async Task<bool> UpdateRegisterAsync(List<Register> register)
  {
    string sql = @$"UPDATE [CSCAdmin].[dbo].[REGISTER]
                    SET
                        [Active] = @Active,
                        [UseOPOS] = @UseOPOS,
                        [DeviceName] = COALESCE(@DeviceName, [DeviceName]),
                        [PortNum] = COALESCE(@PortNum, [PortNum]),
                        [PortSetting] = COALESCE(@PortSetting, [PortSetting]),
                        [DriverName] = COALESCE(@DriverName, [DriverName]),
                        [FontName] = COALESCE(@FontName, [FontName]),
                        [FontSize] = COALESCE(@FontSize, [FontSize]),
                        [WritePosLog] = @WritePosLog
                    WHERE
                        [REG_NO] = @RegNo
                    AND
                        [DeviceID] = @DeviceID";

    int rowsAffected = await Connection.ExecuteAsync(sql, register, Transaction);
    return rowsAffected == 8;               // Will effect 8 rows, one for each device
  }

  public async Task<bool> DeleteRegisterAsync(int regNo)
  {
    string sql = @"DELETE FROM [CSCAdmin].[dbo].[REGISTER] WHERE [REG_NO] = @RegNo";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { RegNo = regNo }, Transaction);
    return rowsAffected == 8;               // Will effect 8 rows, one for each device
  }

  public async Task<bool> IsRegisterExistsAsync(int regNo)
  {
    string sql = @"SELECT COUNT(*) FROM [CSCAdmin].[dbo].[REGISTER] WHERE [REG_NO] = @RegNo";

    int count = await Connection.ExecuteScalarAsync<int>(sql, new { RegNo = regNo }, Transaction);
    return count > 0;
  }
}
