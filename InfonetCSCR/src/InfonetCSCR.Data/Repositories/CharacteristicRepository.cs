namespace InfonetCSCR.Data.Repositories;

public class CharacteristicRepository : BaseRepository, ICharacteristicRepository
{
  public CharacteristicRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Characteristic>> GetAllCharacteristicsAsync()
  {
    const string sql = @"SELECT 
	                        [CharacteristicID] CharacteristicId,
	                        [Description] Description
                        FROM [CSCMaster].[dbo].[Characteristic]  
                        ORDER BY [CSCMaster].[dbo].[Characteristic].CharacteristicID";

    IList<Characteristic> characteristics = (IList<Characteristic>)await Connection.QueryAsync<Characteristic>(sql, null, Transaction);
    return characteristics;
  }

  public async Task<bool> AddCharacteristicAsync(Characteristic characteristic)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Characteristic]
            (
                [CharacteristicID],
                [Description]
            )
            VALUES
            (
                @CharacteristicID,
                @Description
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, characteristic, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateCharacteristicAsync(Characteristic characteristic)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[Characteristic]
                      SET
                          Description = @Description
                      WHERE
                          CharacteristicID = @CharacteristicID";
    int rowsAffected = await Connection.ExecuteAsync(sql, characteristic, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCharacteristicAsync(string characteristicId)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Characteristic]
                        WHERE CharacteristicId = @CharacteristicID";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { CharacteristicID = characteristicId }, Transaction);
    return rowsAffected > 0;
  }
}
