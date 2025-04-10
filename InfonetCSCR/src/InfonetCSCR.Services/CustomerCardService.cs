namespace InfonetCSCR.Services;

public class CustomerCardService : ICustomerCardService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;

  public CustomerCardService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _config = applicationConfig;
  }

  public async Task<IEnumerable<CustomerCard>> GetCustomerCardsByCode(string code)
  {
    IEnumerable<CustomerCard> customerCards = await _unitOfWork.CustomerCard.GetCustomerCardsByCodeAsync(code);
    _unitOfWork.Commit();
    return customerCards;
  }

  public async Task<bool> AddCustomerCard(CustomerCardDto customerCard)
  {
    bool success = await _unitOfWork.CustomerCard.AddCustomerCardAsync(customerCard);

    if (success && customerCard.IsSyncCustInfo == true && customerCard.IsArCustomer == true && !_config.IsBackOffice)
    {
      await AddCustomerCardChange(customerCard.CardNumber);
    }

    _unitOfWork.Commit();
    return success;
  }

  public async Task<bool> UpdateCustomerCard(CustomerCardDto customerCard)
  {
    bool success = await _unitOfWork.CustomerCard.UpdateCustomerCardAsync(customerCard);

    if (success && customerCard.IsSyncCustInfo == true && customerCard.IsArCustomer == true && !_config.IsBackOffice)
    {
      await UpdateCustomerCardChange(customerCard.CardNumber);
    }

    _unitOfWork.Commit();
    return success;
  }

  public async Task<bool> DeleteCustomerCardByNumber(CustomerCardDeleteDto customerCardDeleteDto)
  {
    bool success = await _unitOfWork.CustomerCard.DeleteCustomerCardByNumberAsync(customerCardDeleteDto.CustomerCardNumber);

    if (success && customerCardDeleteDto.IsSyncCustInfo == true && customerCardDeleteDto.IsArCustomer == true && !_config.IsBackOffice)
    {
      await DeleteCustomerCardChange(customerCardDeleteDto.CustomerCardNumber);
    }

    _unitOfWork.Commit();
    return success;
  }

  public async Task<bool> DeleteRewardCard(string customerCardNumber, string rewardCardNumbe)
  {
    bool success = await _unitOfWork.CustomerCard.DeleteRewardCardAsync(customerCardNumber, rewardCardNumbe);
    _unitOfWork.Commit();
    return success;
  }

  public async Task<IEnumerable<RewardCard>> GetRewardCardsByCardNumber(string number)
  {
    IEnumerable<RewardCard> rewardCards = await _unitOfWork.CustomerCard.GetRewardCardByNumberAsync(number);
    _unitOfWork.Commit();
    return rewardCards;
  }

  public async Task<bool> IsCustomerCardExist(string custCardNumber)
  {
    bool IsCustomerCardExist = await _unitOfWork.CustomerCard.IsCustomerCardExistAsync(custCardNumber);
    _unitOfWork.Commit();
    return IsCustomerCardExist;
  }

  public async Task<bool> IsRewardCardExistByCustomerCardAndRewardCardNumber(string custCardNumber, string rewardCardNumber)
  {
    bool rewardCard = await _unitOfWork.CustomerCard.IsRewardCardExistByCustomerCardAndRewardCardNumberAsync(custCardNumber, rewardCardNumber);
    _unitOfWork.Commit();
    return rewardCard;
  }

  public async Task<bool> IsRewardCardExistByCustomerCardAndPhone(string custCardNumber, string phoneNumber)
  {
    bool rewardCard = await _unitOfWork.CustomerCard.IsRewardCardExistByCustomerCardAndPhoneAsync(custCardNumber, phoneNumber);
    _unitOfWork.Commit();
    return rewardCard;
  }

  public async Task<CustomerCard> GetCustomerCardByNumber(string custCardNumber)
  {
    CustomerCard rewardCard = await _unitOfWork.CustomerCard.GetCustomerCardByNumberAsync(custCardNumber);
    _unitOfWork.Commit();
    return rewardCard;
  }

  public async Task<bool> AddRewardCard(RewardCard custCardNumber)
  {
    bool rewardCard = await _unitOfWork.CustomerCard.AddRewardCardAsync(custCardNumber);
    _unitOfWork.Commit();
    return rewardCard;
  }

  public async Task<bool> UpdateRewardCard(RewardCard custCardNumber)
  {
    bool rewardCard = await _unitOfWork.CustomerCard.UpdateRewardCardAsync(custCardNumber);
    _unitOfWork.Commit();
    return rewardCard;
  }

  public async Task<bool> IsArCustomer(string code)
  {
    bool isArCsutomer = await _unitOfWork.CustomerCard.IsArCustomerAsync(code);
    _unitOfWork.Commit();
    return isArCsutomer;
  }

  public async Task<int> GetNextSequenceNumber(string customerCode)
  {
    int nextSequenceNumber = await _unitOfWork.CustomerCard.GetNextSequenceNumberAsync(customerCode);
    _unitOfWork.Commit();
    return nextSequenceNumber;
  }

  private async Task<bool> AddCustomerCardChange(string cardNumber)
  {
    ClientCardChange clientCardChange = new ClientCardChange()
    {
      CardNumber = cardNumber,
      OPStatus = "N",
      TransFlag = "N"
    };

    bool success = await _unitOfWork.CustomerCard.AddCustomerCardChangeAsync(clientCardChange);
    return success;
  }

  private async Task<bool> UpdateCustomerCardChange(string cardNumber)
  {
    ClientCardChange clientCardChange = new ClientCardChange()
    {
      CardNumber = cardNumber,
      OPStatus = "U",
      TransFlag = "N"
    };

    bool success = await _unitOfWork.CustomerCard.AddCustomerCardChangeAsync(clientCardChange);
    return success;
  }

  private async Task<bool> DeleteCustomerCardChange(string cardNumber)
  {
    ClientCardChange clientCardChange = new ClientCardChange()
    {
      CardNumber = cardNumber,
      OPStatus = "N",
      TransFlag = "N"
    };

    bool success = await _unitOfWork.CustomerCard.DeleteCustomerCardChangeAsync(clientCardChange);
    if (!success)
    {
      clientCardChange.OPStatus = "D";
      success = await _unitOfWork.CustomerCard.AddCustomerCardChangeAsync(clientCardChange);
    }

    return success;
  }
}
