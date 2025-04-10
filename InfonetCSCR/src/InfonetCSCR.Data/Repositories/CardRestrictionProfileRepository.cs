namespace InfonetCSCR.Data.Repositories;

public class CardRestrictionProfileRepository : BaseRepository, ICardRestrictionProfileRepository
{
  public CardRestrictionProfileRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<CardRestrictionProfile>> GetAllRestrictionProfilesAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT *
                    FROM CardRestrProfiles
                    ORDER BY ProfileID;");
    IEnumerable<CardRestrictionProfile> restrictionProfiles = await Connection.QueryAsync<CardRestrictionProfile>(query.ToString(), null, Transaction);
    return restrictionProfiles;
  }

  public async Task<IEnumerable<CardProfileTimeLimit>> GetTimeOfPurchaseRestrictionAsync(string? profileId)
  {
    string query = $@"SELECT *
                    FROM CardProfileTimeLimit
                    WHERE ProfileID = @ProfileID
                    ORDER BY DayOfWeek;";

    IEnumerable<CardProfileTimeLimit> restrictionProfiles = await Connection.QueryAsync<CardProfileTimeLimit>(query.ToString(), new { ProfileID = profileId }, Transaction);
    return restrictionProfiles;
  }

  public async Task<IEnumerable<ProfilePromptDto>> GetAllProfilePromptsAsync(string? profileId)
  {
    string query = $@"SELECT A.profileID, A.promptid,A.promptseq, B.PromptMessage, A.Type
                    FROM CardProfilePrompts A
                    INNER JOIN CardFuelPrompts B
                    ON A.PromptID = B.PromptID AND A.Type=B.Type
                    WHERE A.ProfileID = @ProfileID
                    AND A.Type='F'
                    ORDER BY PromptSeq;";

    IEnumerable<ProfilePromptDto> restrictionProfiles = await Connection.QueryAsync<ProfilePromptDto>(query.ToString(), new { ProfileID = profileId }, Transaction);
    return restrictionProfiles;
  }

  public async Task<CardRestrictionProfile?> GetCardRestrictionProfileAsync(string? profileId)
  {
    string query = $@"SELECT *
                      FROM [CSCMaster].[dbo].[CardRestrProfiles]
                      WHERE ProfileID = @ProfileID;";

    CardRestrictionProfile? restrictionProfile = await Connection.QueryFirstOrDefaultAsync<CardRestrictionProfile>(query, new { ProfileID = profileId }, Transaction);
    return restrictionProfile;
  }

  public async Task<ProfilePromptDto?> GetProfilePromptByPromptSequenceAsync(ProfilePromptDto profilePrompt)
  {
    string query = $@"SELECT *
                      FROM [CSCMaster].[dbo].[cardprofileprompts]
                      WHERE ProfileID = @ProfileID
                      AND PromptSeq = @PromptSeq
                      AND Type = @Type;";

    ProfilePromptDto? existingProfilePrompt = await Connection.QueryFirstOrDefaultAsync<ProfilePromptDto>(query, profilePrompt, Transaction);
    return existingProfilePrompt;
  }

  public async Task<bool> AddProfilePromptAsync(ProfilePromptDto profilePrompt)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[CardProfilePrompts] (
                    [ProfileID],
                    [PromptSeq],
                    [PromptID],
                    [Type]
                  ) VALUES (
                    @ProfileID,
                    @PromptSeq,
                    @PromptID,
                    @Type
                  );";

    int rowsAffected = await Connection.ExecuteAsync(query, profilePrompt, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteProfilePromptAsync(ProfilePromptDto profilePrompt)
  {
    StringBuilder query = new();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].[CardProfilePrompts]
                      WHERE ProfileID = @ProfileID ");

    if (profilePrompt.PromptID != null)
      query.Append($@"AND PromptID = @PromptID ");

    query.Append($@"AND Type = @Type;");

    try
    {
      await Connection.ExecuteAsync(query.ToString(), profilePrompt, Transaction);
    }
    catch
    {
      return false;
    }

    return (true);
  }

  public async Task<bool> AddCardRestrictionProfileAsync(CardRestrictionProfile cardRestrictionProfile)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[CardRestrProfiles] (
                    [ProfileID],
                    [ProfileName],
                    [SngleTransaction],
                    [DailyTransaction],
                    [MonthlyTransaction],
                    [TransactionsPerDay],
                    [AskForPO],
                    [PromptForFuel],
                    [LimitTimeofPurchase],
                    [RestrictProducts]
                  ) VALUES (
                    @ProfileID,
                    @ProfileName,
                    @SngleTransaction,
                    @DailyTransaction,
                    @MonthlyTransaction,
                    @TransactionsPerDay,
                    @AskForPO,
                    @PromptForFuel,
                    @LimitTimeofPurchase,
                    @RestrictProducts
                  );";

    int rowsAffected = await Connection.ExecuteAsync(query, cardRestrictionProfile, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateCardRestrictionProfileAsync(CardRestrictionProfile cardRestrictionProfile)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[CardRestrProfiles]
                    SET
                      [ProfileName] = @ProfileName,
                      [SngleTransaction] = @SngleTransaction,
                      [DailyTransaction] = @DailyTransaction,
                      [MonthlyTransaction] = @MonthlyTransaction,
                      [TransactionsPerDay] = @TransactionsPerDay,
                      [AskForPO] = @AskForPO,
                      [PromptForFuel] = @PromptForFuel,
                      [LimitTimeofPurchase] = @LimitTimeofPurchase,
                      [RestrictProducts] = @RestrictProducts
                    WHERE [ProfileID] = @ProfileID";

    int rowsAffected = await Connection.ExecuteAsync(query, cardRestrictionProfile, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> AddTimeOfPurchaseRestrictionAsync(CardProfileTimeLimit cardProfileTimeLimit)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[CardProfileTimeLimit]
                      (
                      ProfileID,
                      DayOfWeek,
                      AllowPurchase,
                      TimeRestriction,
                      StartTime,
                      EndTime
                      )
                      VALUES
                      (
                      @ProfileID,
                      @DayOfWeek,
                      @AllowPurchase,
                      @TimeRestriction,
                      @StartTime,
                      @EndTime
                      );";

    int rowsAffected = await Connection.ExecuteAsync(query, cardProfileTimeLimit, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateTimeOfPurchaseRestrictionAsync(CardProfileTimeLimit cardProfileTimeLimit)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[CardProfileTimeLimit]
                      SET
                          AllowPurchase = @AllowPurchase,
                          TimeRestriction = @TimeRestriction,
                          StartTime = @StartTime,
                          EndTime = @EndTime
                      WHERE
                          ProfileID = @ProfileID AND
                          DayOfWeek = @DayOfWeek";

    int rowsAffected = await Connection.ExecuteAsync(query, cardProfileTimeLimit, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteTimeOfPurchaseRestrictionAsync(string profileId)
  {
    string query = $@"DELETE FROM [CSCMaster].[dbo].[CardProfileTimeLimit]
                      WHERE ProfileID = @ProfileID";

    int rowsAffected = await Connection.ExecuteAsync(query, new { ProfileID = profileId }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCardRestrictionProfileAsync(string profileId)
  {
    string query = $@"DELETE FROM [CSCMaster].[dbo].[CardRestrProfiles]
                      WHERE ProfileID = @ProfileID";

    int rowsAffected = await Connection.ExecuteAsync(query, new { ProfileID = profileId }, Transaction);
    return rowsAffected > 0;

  }

  public async Task<IEnumerable<ProfilePromptDto>?> GetPromptMessageByProfileIDAsync(string profileId)
  {
    string query = $@"SELECT promptid, promptmessage
                      FROM [CSCMaster].[dbo].cardFuelPrompts
                      WHERE Type='F' AND promptid not in
                        (select promptid FROM [CSCMaster].[dbo].cardprofileprompts
                        WHERE Type='F'
                        AND profileid = @ProfileID)";

    IEnumerable<ProfilePromptDto>? promptMessages = await Connection.QueryAsync<ProfilePromptDto>(query, new { ProfileID = profileId }, Transaction);
    return promptMessages;
  }
}
