namespace InfonetCSCR.Services;

public class FuelPriceChangeService : IFuelPriceChangeService
{
  private readonly IUnitOfWork _unitOfWork;

  public FuelPriceChangeService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCPumpConnectionString);
  }

  private StoreDataInHoDbRecord FillStoreIdsThatCannotBeAddedField(StoreDataInHoDbRecord storeDataInHoDbRecord, IList<string> storeIdsToRemove)
  {
    if (storeDataInHoDbRecord.StoreIdsThatCannotBeAdded == null)
    {
      storeDataInHoDbRecord.StoreIdsThatCannotBeAdded = storeIdsToRemove;
    }
    else
    {
      storeDataInHoDbRecord.StoreIdsThatCannotBeAdded =
      storeDataInHoDbRecord.StoreIdsThatCannotBeAdded.Concat(storeIdsToRemove).ToList();
    }

    return storeDataInHoDbRecord;
  }

  public async Task<StoreDataInHoDbRecord> CheckStoreDataInHeadOfficeDatabase(FuelPriceChangeStoreCheckRecord fuelPriceChangeStoreCheckRecord)
  {
    IList<string>? storeIds = fuelPriceChangeStoreCheckRecord.Stores;
    StoreDataInHoDbRecord storeDataInHoDbRecord = new();
    IList<Grade>? grades = await _unitOfWork.Grades.GetGradeByStoreIdsAsync(storeIds);
    IList<string> storeIdsInGrades = grades.Where(g => g.StoreId != null).Select(g => g.StoreId).Distinct().ToList();
    IList<string> storeIdsToRemove = storeIds.Where(id => !storeIdsInGrades.Contains(id)).ToList();

    storeIds = storeIdsInGrades;
    storeDataInHoDbRecord.NotExistsInGradeTable = storeIdsToRemove;
    storeDataInHoDbRecord = FillStoreIdsThatCannotBeAddedField(storeDataInHoDbRecord, storeIdsToRemove);

    IList<Tier>? tiersByStoreIds = await _unitOfWork.Tiers.GetTiersByStoreIdsAsync(storeIds);
    IList<string> storeIdsInTiers = tiersByStoreIds.Where(g => g.StoreId != null).Select(g => g.StoreId).Distinct().ToList();
    storeIdsToRemove = storeIds.Where(id => !storeIdsInTiers.Contains(id)).ToList();

    storeIds = storeIdsInTiers;
    storeDataInHoDbRecord.NotExistsInTierTable = storeIdsToRemove;
    storeDataInHoDbRecord = FillStoreIdsThatCannotBeAddedField(storeDataInHoDbRecord, storeIdsToRemove);

    IList<ServiceLevel>? serviceLevelsByStoreIds = await _unitOfWork.ServiceLevels.GetServiceLevelsByStoreIdsAsync(storeIds);
    IList<string> storeIdsInServiceLevels = serviceLevelsByStoreIds.Where(g => g.StoreId != null).Select(g => g.StoreId).Distinct().ToList();
    storeIdsToRemove = storeIds.Where(id => !storeIdsInServiceLevels.Contains(id)).ToList();

    storeIds = storeIdsInServiceLevels;
    storeDataInHoDbRecord.NotExistsInServiceLevelTable = storeIdsToRemove;
    storeDataInHoDbRecord = FillStoreIdsThatCannotBeAddedField(storeDataInHoDbRecord, storeIdsToRemove);

    IList<TierLevelPriceDifference>? tierLevelPriceDifferences = await _unitOfWork.TierLevelPriceDifferences.GetTierLevelPriceDifferencesByStoreIdsAsync(storeIds);
    IList<string> storeIdsTierLevelPriceDifferences = tierLevelPriceDifferences.Where(g => g.StoreId != null).Select(g => g.StoreId).Distinct().ToList();
    storeIdsToRemove = storeIds.Where(id => !storeIdsTierLevelPriceDifferences.Contains(id)).ToList();

    storeDataInHoDbRecord.NotExistsInTierLevelPriceDiffTable = storeIdsToRemove;
    storeDataInHoDbRecord = FillStoreIdsThatCannotBeAddedField(storeDataInHoDbRecord, storeIdsToRemove);

    storeDataInHoDbRecord.InSameTierLevel = true;
    if (fuelPriceChangeStoreCheckRecord.IsFuelGP == false)
    {
      IList<Tier>? tiers = await _unitOfWork.Tiers.GetAllDistinctIdAndDescriptionFromTierTableAsync();
      int maxTierId = 0;
      if (tiers.Any())
      {
        maxTierId = tiers.Max(tier => tier.Id);
      }

      storeDataInHoDbRecord.InSameTierLevel = (maxTierId == tiers.Count());
    }

    IList<ServiceLevel>? serviceLevels = await _unitOfWork.ServiceLevels.GetAllDistinctIdAndDescriptionFromServiceLevelAsync();
    int maxServiceLevelId = 0;
    if (serviceLevels.Any())
    {
      maxServiceLevelId = serviceLevels.Max(serviceLevel => serviceLevel.Id);
    }

    storeDataInHoDbRecord.InSameServiceLevel = (maxServiceLevelId == serviceLevels.Count());

    _unitOfWork.Commit();
    return storeDataInHoDbRecord;
  }

  public async Task<IList<GradeTierServiceLevelTableData>?> GetDataFromGradeTierServiceLevelTable(GradeGridRecord gradeGridRecord)
  {
    IList<GradeTierServiceLevelTableData>? gradeTierServiceLevelTableData =
      await _unitOfWork.Grades.GetDataFromGradeTierServiceLevelTableAsync(gradeGridRecord.StoreId);

    int maxReportId = await _unitOfWork.FuelPriceHistory.GetMaximumReportIdByStoreId(gradeGridRecord.StoreId);

    IList<FuelPriceHistory> fuelPriceHistories = await _unitOfWork.FuelPriceHistory.GetAllFuelPriceHistoryByStoreIdAndReportId(gradeGridRecord.StoreId, maxReportId);

    Dictionary<(int?, int?, int?), FuelPriceHistory> fuelPriceHistoryDictionary = fuelPriceHistories
            .GroupBy(fph => (fph.GradeID, fph.TierID, fph.LevelID))
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault()
            );

    foreach (GradeTierServiceLevelTableData gradeJoinTierJoinServiceLevel in gradeTierServiceLevelTableData)
    {
      var key = (
        GradeID: gradeJoinTierJoinServiceLevel.GradeId,
        TierID: gradeJoinTierJoinServiceLevel.TierId,
        LevelID: gradeJoinTierJoinServiceLevel.ServiceLevelId);

      gradeJoinTierJoinServiceLevel.LoadRow = (
        gradeJoinTierJoinServiceLevel.TierId == 1 && gradeJoinTierJoinServiceLevel.ServiceLevelId == 1);
      if (fuelPriceHistoryDictionary.TryGetValue(key, out FuelPriceHistory fuelPriceHistory))
      {
        gradeJoinTierJoinServiceLevel.DateTime = fuelPriceHistory.DateTime;
        gradeJoinTierJoinServiceLevel.CashPrice = fuelPriceHistory.CashPrice;
        if (gradeGridRecord.IsFuelCp == true)
        {
          gradeJoinTierJoinServiceLevel.CreditPrice = fuelPriceHistory.CreditPrice;
        }
        else
        {
          gradeJoinTierJoinServiceLevel.CreditPrice = fuelPriceHistory.CashPrice;
        }
      }
      else
      {
        gradeJoinTierJoinServiceLevel.CashPrice = 0;
        gradeJoinTierJoinServiceLevel.CreditPrice = 0;
      }
    }

    return gradeTierServiceLevelTableData;
  }

  public async Task<bool> TransferFuelPriceChange(IList<FuelPriceChangeTransferRecord> storesData)
  {
    if (!storesData.Any())
    {
      return false;
    }

    IList<FuelPriceChangeTransferRecord> storeIds = new List<FuelPriceChangeTransferRecord>();

    foreach (FuelPriceChangeTransferRecord store in storesData)
    {
      storeIds.Add(store);
    }

    IList<FuelPriceChangeTransferRecord> uniqueFPCTransferData = storeIds.GroupBy(store => store.StoreId).Select(item => item.First()).ToList();

    await _unitOfWork.FuelPriceChange.DeleteStoreAsync(uniqueFPCTransferData);

    int fuelPriceReportId = await _unitOfWork.FuelPriceChange.GetReportIdAsync(DBTableNames.FuelPrice);
    int fuelPriceHistReportId = await _unitOfWork.FuelPriceChange.GetReportIdAsync(DBTableNames.FuelPriceHist);
    int reportId = Math.Max(fuelPriceReportId, fuelPriceHistReportId) + 1;

    foreach (var storeData in storesData)
    {
      storeData.ReportId = reportId;
      storeData.ReportDateTime = DateTime.Now;
    }

    // add store to the FuelPrice table
    await _unitOfWork.FuelPriceChange.AddNewFuelPricesAsync(storesData);

    // add store to the FuelPriceHist table
    IList<FuelPriceHistory> fuelPriceHistories = await _unitOfWork.FuelPriceChange.GetAllFuelPriceHistoryByStoreIdAndReportIdAsync(uniqueFPCTransferData);

    Dictionary<(string?, int?, int?, int?, int?), FuelPriceHistory> fuelPriceHistoryDictionary = fuelPriceHistories
            .GroupBy(fph => (fph.StoreID, fph.GradeID, fph.TierID, fph.LevelID, fph.ReportID))
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault()
    );

    IList<FuelPriceChangeTransferRecord> newStoresData = new List<FuelPriceChangeTransferRecord>();

    foreach (FuelPriceChangeTransferRecord store in storesData)
    {
      var key = (
        StoreID: store.StoreId,
        GradeID: store.GradeId,
        TierID: store.TierId,
        LevelID: store.ServiceLevelId,
        ReportID: store.ReportId);

      if (!fuelPriceHistoryDictionary.TryGetValue(key, out FuelPriceHistory fuelPriceHistory))
      {
        newStoresData.Add(store);
      }
    }

    await _unitOfWork.FuelPriceChange.AddNewFuelPriceHistsAsync(newStoresData);

    bool isOk = _unitOfWork.Commit();
    return isOk;
  }

  public async Task<IList<GradeTierServiceLevelTableData>?> GetFuelPriceChangeReportData(IList<string> storeIds, bool isFuelCp)
  {
    var newStoreIds = await _unitOfWork.FuelPriceChange.GetMaxReportIdsAsync(storeIds);
    IList<FuelPriceHistory>? fuelPriceHistories = await _unitOfWork.FuelPriceChange.GetAllFuelPriceHistoryByStoreIdAndReportIdAsync(newStoreIds.ToList());

    IList<GradeTierServiceLevelTableData>? gradeTierServiceLevelTableData =
      await _unitOfWork.FuelPriceChange.GetFuelPriceChangeReportDataAsync(storeIds);

    Dictionary<(int?, int?, int?), FuelPriceHistory> fuelPriceHistoryDictionary = fuelPriceHistories
            .GroupBy(fph => (fph.GradeID, fph.TierID, fph.LevelID))
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault()
            );

    foreach (GradeTierServiceLevelTableData gradeJoinTierJoinServiceLevel in gradeTierServiceLevelTableData)
    {
      var key = (
        GradeID: gradeJoinTierJoinServiceLevel.GradeId,
        TierID: gradeJoinTierJoinServiceLevel.TierId,
        LevelID: gradeJoinTierJoinServiceLevel.ServiceLevelId);

      gradeJoinTierJoinServiceLevel.LoadRow = (
        gradeJoinTierJoinServiceLevel.TierId == 1 && gradeJoinTierJoinServiceLevel.ServiceLevelId == 1);
      if (fuelPriceHistoryDictionary.TryGetValue(key, out FuelPriceHistory fuelPriceHistory))
      {
        gradeJoinTierJoinServiceLevel.DateTime = fuelPriceHistory.DateTime;
        gradeJoinTierJoinServiceLevel.CashPrice = fuelPriceHistory.CashPrice;
        if (isFuelCp)
        {
          gradeJoinTierJoinServiceLevel.CreditPrice = fuelPriceHistory.CreditPrice;
        }
        else
        {
          gradeJoinTierJoinServiceLevel.CreditPrice = fuelPriceHistory.CashPrice;
        }
      }
      else
      {
        gradeJoinTierJoinServiceLevel.DateTime = fuelPriceHistory.DateTime;
        gradeJoinTierJoinServiceLevel.CashPrice = 0;
        gradeJoinTierJoinServiceLevel.CreditPrice = 0;
      }
    }

    return gradeTierServiceLevelTableData;
  }
}
