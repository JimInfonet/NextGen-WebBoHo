namespace InfonetCSCR.Services;

public class TendersService : ITendersService
{
  private readonly IUnitOfWork _unitOfWork;

  public TendersService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<Tender>> GetTendersWithCardCodeAsync()
  {
    IEnumerable<Tender> tenders = await _unitOfWork.Tenders.GetTendersWithCardCodeAsync();
    _unitOfWork.Commit();
    return tenders;
  }

  public async Task<Tender> GetSingleTender(string tendCode)
  {
    Tender tender = await _unitOfWork.Tenders.GetSingleTenderAsync(tendCode);
    if(tender.CardCode == 0)
    {
      tender.CardCode = null;
    }
    _unitOfWork.Commit();
    return tender;
  }

  public async Task<IEnumerable<Payout>> GetPayout()
  {
    IEnumerable<Payout> payouts = await _unitOfWork.Tenders.GetPayoutAsync();
    _unitOfWork.Commit();
    return payouts;
  }

  public async Task<IEnumerable<TenderClass>> GetTenderClasses()
  {
    IEnumerable<TenderClass> tenderClasses = await _unitOfWork.Tenders.GetTenderClassesAsync();
    _unitOfWork.Commit();
    return tenderClasses;
  }

  public async Task<bool> UpdateTender(Tender tender)
  {
    bool isUpdated = await _unitOfWork.Tenders.UpdateTenderAsync(tender);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> AddTender(Tender tender)
  {
    bool isAdded = await _unitOfWork.Tenders.AddTenderAsync(tender);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteTender(string tendCode)
  {
    Tender tender = await _unitOfWork.Tenders.GetSingleTenderAsync(tendCode);
    string cardCode = tender.CardCode.ToString();

    if (!string.IsNullOrEmpty(cardCode))
    {
      TenderCard tenderCard = await _unitOfWork.TenderCards.GetTenderCardDataByCardCodeAsync(cardCode);

      if (!string.IsNullOrEmpty(tenderCard.BankCardID))
      {
        string bankCardID = tenderCard.BankCardID.ToString();
        await _unitOfWork.CardProductRestrictions.DeleteCardProductRestrictionByBankCardIDAsync(bankCardID);
      }

      await _unitOfWork.Cards.DeleteCardByCardCodeAsync(cardCode);
      await _unitOfWork.CardCodes.DeleteCardRangesByCardCodeAsync(cardCode);
    }

    await _unitOfWork.Tenders.DeleteTenderAsync(tendCode);
    await _unitOfWork.TenderCards.DeleteTenderCardByTendCodeAsync(tendCode);

    bool isDeleted = _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> IsTenderCodeExist(string tendCode)
  {
    bool isTenderCodeExist = await _unitOfWork.Tenders.IsTenderCodeExistAsync(tendCode);
    _unitOfWork.Commit();
    return isTenderCodeExist;
  }

  public async Task<bool> IsFullNameExist(string fullName)
  {
    bool isFullNameExist = await _unitOfWork.Tenders.IsFullNameExistAsync(fullName);
    _unitOfWork.Commit();
    return isFullNameExist;
  }

  public async Task<bool> ResetSequence()
  {
    IEnumerable<Tender> tenders = await _unitOfWork.Tenders.GetTendersWithCardCodeAsync();
    List<Task<bool>> tasks = new List<Task<bool>>();

    byte seq = 1;
    foreach (Tender tender in tenders)
    {
      tender.Sequence = seq;
      tasks.Add(_unitOfWork.Tenders.UpdateTenderAsync(tender));
      seq++;
    }

    bool[] results = await Task.WhenAll(tasks);
    int numOfRows = 0;

    for(int i=0;i<results.Length; i++)
    {
      if (results[i])numOfRows++;
    }

    if (numOfRows != tenders.Count())
    {
      throw new Exception("Failed to reset sequence");
    }

    _unitOfWork.Commit();
    return true;
  }

  public async Task<IEnumerable<Tender>> GetTendersListAsync()
  {
    var tendersList = await _unitOfWork.Tenders.GetTendersListAsync();
    _unitOfWork.Commit();
    return tendersList;
  }

  public async Task<IEnumerable<Tender>> GetTendersListByClassNameAsync(string tendClass)
  {
    var tendersList = await _unitOfWork.Tenders.GetTendersListByClassNameAsync(tendClass);
    _unitOfWork.Commit();
    return tendersList;
  }
}
