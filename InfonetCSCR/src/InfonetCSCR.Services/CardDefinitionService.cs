namespace InfonetCSCR.Services;

public class CardDefinitionService : ICardDefinitionService
{
  private readonly IUnitOfWork _unitOfWork;

  public CardDefinitionService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public CardDefinitionDto GetCardDefinitionFromCardAndTenderCard(Card card, TenderCard tenderCard)
  {
    var cardDefinition = new CardDefinitionDto();

    // Set properties from the Card object
    cardDefinition.CardCode = card.CardCode;
    cardDefinition.Description = card.Description;
    cardDefinition.CardType = card.CardType;
    cardDefinition.VerifyCheckDigit = card.VerifyCheckDigit;
    cardDefinition.Encode = card.Encode;
    cardDefinition.Negative = card.Negative;
    cardDefinition.Positive = card.Positive;
    cardDefinition.Expiry = card.Expiry;
    cardDefinition.LanguageDigit = card.LanguageDigit;
    cardDefinition.AllowPayment = card.AllowPayment;
    cardDefinition.MinLength = card.MinLength;
    cardDefinition.MaxLength = card.MaxLength;
    cardDefinition.CheckMask = card.CheckMask;
    cardDefinition.UsageIDPosition = card.UsageIDPosition;
    cardDefinition.UsageIDLength = card.UsageIDLength;
    cardDefinition.PromptIDPosition = card.PromptIDPosition;
    cardDefinition.PromptIDLength = card.PromptIDLength;
    cardDefinition.DefaultPromptCode = card.DefaultPromptCode;
    cardDefinition.DefaultUsageCode = card.DefaultUsageCode;
    cardDefinition.UserEnteredRestriction = card.UserEnteredRestriction;
    cardDefinition.ProductPerTransaction = card.ProductPerTransaction;
    cardDefinition.ServiceTypeCode = card.ServiceTypeCode;
    cardDefinition.VerifyThirdPartySw = card.VerifyThirdPartySw;
    cardDefinition.VerifyCardNumber = card.VerifyCardNumber;
    cardDefinition.GiftType = card.GiftType;
    cardDefinition.EncryptCardNumber = card.EncryptCardNumber;
    cardDefinition.AskForDriverID = card.AskForDriverID;

    // Set properties from the TenderCard object
    cardDefinition.TendCode = tenderCard?.TendCode;
    cardDefinition.ReportGroup = tenderCard?.ReportGroup;
    cardDefinition.BankCardID = tenderCard?.BankCardID;
    cardDefinition.Call = tenderCard?.Call ?? false;
    cardDefinition.Signature = tenderCard?.Signature ?? false;
    cardDefinition.DiscountRate = tenderCard?.DiscountRate;
    cardDefinition.DiscountType = tenderCard?.DiscountType;
    cardDefinition.RefundAllowed = tenderCard?.RefundAllowed ?? false;
    cardDefinition.PurchaseLimit = tenderCard?.PurchaseLimit;
    cardDefinition.FloorLimit = tenderCard?.FloorLimit;
    cardDefinition.PrintCopies = tenderCard?.PrintCopies;
    cardDefinition.ReceiptTotalText = tenderCard?.ReceiptTotalText;
    cardDefinition.LimitToSale = tenderCard?.LimitToSale ?? false;
    cardDefinition.MaxCashBack = tenderCard?.MaxCashBack;
    cardDefinition.AllowPayPump = tenderCard?.AllowPayPump;
    cardDefinition.CardProductRestrict = tenderCard?.CardProductRestrict;
    cardDefinition.PrintVechicleNo = tenderCard?.PrintVechicleNo;
    cardDefinition.PrintDriverNo = tenderCard?.PrintDriverNo;
    cardDefinition.PrintIdentificationNo = tenderCard?.PrintIdentificationNo;
    cardDefinition.PrintOdometer = tenderCard?.PrintOdometer;
    cardDefinition.PrintUsage = tenderCard?.PrintUsage;
    cardDefinition.OptionalDataProfilID = tenderCard?.OptionalDataProfilID;
    cardDefinition.TransactionFeeAmount = tenderCard?.TransactionFeeAmount;
    cardDefinition.TransactionFeePercentage = tenderCard?.TransactionFeePercentage;

    return cardDefinition;
  }

  public Card GetCardFromCardDefinition(CardDefinitionDto cardDefinition)
  {
    Card card = new Card
    {
      CardCode = cardDefinition.CardCode,
      Description = cardDefinition.Description,
      CardType = cardDefinition.CardType,
      VerifyCheckDigit = cardDefinition.VerifyCheckDigit,
      Encode = cardDefinition.Encode,
      Negative = cardDefinition.Negative,
      Positive = cardDefinition.Positive,
      Expiry = cardDefinition.Expiry,
      LanguageDigit = (int?)(cardDefinition.LanguageDigit?.Round() ?? 0),
      AllowPayment = cardDefinition.AllowPayment,
      MinLength = (int?)(cardDefinition.MinLength?.Round() ?? 0),
      MaxLength = (int?)(cardDefinition.MaxLength?.Round() ?? 0),
      CheckMask = cardDefinition.CheckMask,
      UsageIDPosition = cardDefinition.UsageIDPosition,
      UsageIDLength = cardDefinition.UsageIDLength,
      PromptIDPosition = cardDefinition.PromptIDPosition,
      PromptIDLength = cardDefinition.PromptIDLength,
      DefaultPromptCode = cardDefinition.DefaultPromptCode,
      DefaultUsageCode = cardDefinition.DefaultUsageCode,
      UserEnteredRestriction = cardDefinition.UserEnteredRestriction,
      ProductPerTransaction = cardDefinition.ProductPerTransaction,
      ServiceTypeCode = cardDefinition.ServiceTypeCode,
      VerifyThirdPartySw = cardDefinition.VerifyThirdPartySw,
      VerifyCardNumber = cardDefinition.VerifyCardNumber,
      GiftType = cardDefinition.GiftType,
      EncryptCardNumber = cardDefinition.EncryptCardNumber,
      AskForDriverID = cardDefinition.AskForDriverID
    };

    return card;
  }

  public TenderCard GetTendCardFromCardDefinition(CardDefinitionDto cardDefinition)
  {
    TenderCard tenderCard = new()
    {
      TendCode = cardDefinition.TendCode,
      CardCode = cardDefinition.CardCode,
      ReportGroup = cardDefinition.ReportGroup,
      BankCardID = cardDefinition.BankCardID,
      Call = cardDefinition.Call,
      Signature = cardDefinition.Signature,
      DiscountRate = cardDefinition.DiscountRate,
      DiscountType = cardDefinition.DiscountType,
      RefundAllowed = cardDefinition.RefundAllowed,
      PurchaseLimit = cardDefinition.PurchaseLimit,
      FloorLimit = cardDefinition.FloorLimit,
      PrintCopies = cardDefinition.PrintCopies,
      ReceiptTotalText = cardDefinition.ReceiptTotalText,
      LimitToSale = cardDefinition.LimitToSale,
      MaxCashBack = cardDefinition.MaxCashBack,
      AllowPayPump = cardDefinition.AllowPayPump,
      CardProductRestrict = cardDefinition.CardProductRestrict,
      PrintVechicleNo = cardDefinition.PrintVechicleNo,
      PrintDriverNo = cardDefinition.PrintDriverNo,
      PrintIdentificationNo = cardDefinition.PrintIdentificationNo,
      PrintOdometer = cardDefinition.PrintOdometer,
      PrintUsage = cardDefinition.PrintUsage,
      OptionalDataProfilID = cardDefinition.OptionalDataProfilID,
      TransactionFeeAmount = cardDefinition.TransactionFeeAmount,
      TransactionFeePercentage = cardDefinition.TransactionFeePercentage
    };
    return tenderCard;
  }

  public async Task<IEnumerable<Card>> GetCards()
  {
    IEnumerable<Card> cards = await _unitOfWork.Cards.GetCardsAsync();
    _unitOfWork.Commit();

    return cards;
  }

  public async Task<TenderCard> GetTendCardForCardCode(string cardCode)
  {
    TenderCard tenderCard = await _unitOfWork.TenderCards.GetTenderCardDataByCardCodeAsync(cardCode);
    _unitOfWork.Commit();

    return tenderCard;
  }

  public async Task<Card> GetCardForCardCode(string cardCode)
  {
    Card card = await _unitOfWork.Cards.GetCardDefinitionByCardCodeAsync(cardCode);
    _unitOfWork.Commit();

    return card;
  }

  public async Task<CardDefinitionDto> GetCardDefinitionByCardCode(string cardCode)
  {
    Card card = await _unitOfWork.Cards.GetCardDefinitionByCardCodeAsync(cardCode);
    TenderCard tenderCard = await _unitOfWork.TenderCards.GetTenderCardDataByCardCodeAsync(cardCode);

    if (tenderCard != null && tenderCard.TendCode == null)
    {
      tenderCard.TendCode = card.Description;
    }

    _unitOfWork.Commit();

    CardDefinitionDto cardDefinition = GetCardDefinitionFromCardAndTenderCard(card, tenderCard);
    return cardDefinition;
  }

  public async Task<List<string>> GetOptionalProfileIDs()
  {
    List<string> profileIDs = await _unitOfWork.OptionalDatas.GetOptionalProfileIDsAsync();
    _unitOfWork.Commit();

    return profileIDs;
  }

  public async Task<bool> AddCardForUpdateTender(Card card)
  {
    bool isCardCodeExist = await _unitOfWork.Cards.CheckCardCodeExistAsync(card.CardCode.ToString());

    if (isCardCodeExist == false)
    {
      await _unitOfWork.Cards.AddCardAsync(card);
    }
    bool response = _unitOfWork.Commit();

    return response;
  }

  public async Task<bool> AddOrUpdateTenderCardForUpdateTender(TenderCard tenderCard)
  {
    bool isTendCodeExist = await _unitOfWork.TenderCards.CheckTendCodeExistAsync(tenderCard.TendCode);

    if (isTendCodeExist == false)
    {
      await _unitOfWork.TenderCards.AddTenderCardAsync(tenderCard);
    }
    else
    {
      await _unitOfWork.TenderCards.UpdateCardCodeOnTenderCardAsync(tenderCard.TendCode, tenderCard.CardCode.ToString());
    }
    bool response = _unitOfWork.Commit();

    return response;
  }

  public async Task<bool> AddCardDefinitionForUpdateTender(CardDefinitionDto cardDefinition)
  {
    Card card = GetCardFromCardDefinition(cardDefinition);
    TenderCard tenderCard = GetTendCardFromCardDefinition(cardDefinition);

    bool isCardCodeExist = await _unitOfWork.Cards.CheckCardCodeExistAsync(card.CardCode.ToString());
    bool isTendCodeExist = await _unitOfWork.TenderCards.CheckTendCodeExistAsync(tenderCard.TendCode);

    if (isCardCodeExist == false)
    {
      await _unitOfWork.Cards.AddCardAsync(card);
    }

    if (isTendCodeExist == false)
    {
      await _unitOfWork.TenderCards.AddTenderCardAsync(tenderCard);
    }
    else
    {
      await _unitOfWork.TenderCards.UpdateCardCodeOnTenderCardAsync(tenderCard.TendCode, card.CardCode.ToString());
    }
    bool response = _unitOfWork.Commit();

    return response;
  }

  public async Task<bool> UpdateCardDefinitionByCardCode(CardDefinitionDto cardDefinition)
  {
    Card card = GetCardFromCardDefinition(cardDefinition);
    TenderCard tenderCard = GetTendCardFromCardDefinition(cardDefinition);

    await _unitOfWork.Cards.UpdateCardAsync(card);
    await _unitOfWork.TenderCards.UpdateTenderCardAsync(tenderCard);

    bool response = _unitOfWork.Commit();
    return response;
  }

  public async Task<bool> DeleteCardDefinitionByCardCode(string cardCode)
  {
    await _unitOfWork.Cards.DeleteCardByCardCodeAsync(cardCode);
    await _unitOfWork.CardCodes.DeleteCardRangesByCardCodeAsync(cardCode);

    bool response = _unitOfWork.Commit();
    return response;
  }

  public async Task<bool> DeleteCardRangesByCardCodeData(string cardCode, string lowerLimit, string upperLimit)
  {
    await _unitOfWork.CardCodes.DeleteCardRangesByCardCodeDataAsync(cardCode, lowerLimit, upperLimit);

    bool response = _unitOfWork.Commit();
    return response;
  }

  public async Task<IEnumerable<CardRange>> GetCardRangesByCardCode(string cardCode)
  {
    IEnumerable<CardRange> cardRanges = await _unitOfWork.CardCodes.GetCardRangesByCardCodeAsync(cardCode);
    _unitOfWork.Commit();
    return cardRanges;
  }

  public async Task<bool> AddCardRange(int cardCode, string lowerLimit, string upperLimit)
  {
    CardRange cardRange = await _unitOfWork.CardCodes.GetCardRangeByLowerLimitAsync(cardCode, lowerLimit);
    if (cardRange != null)
    {
      throw new HttpRequestException("Such lower limit alreay exists for this card code.", null, HttpStatusCode.Conflict);
    }

    await _unitOfWork.CardCodes.AddCardRangeAsync(cardCode, lowerLimit, upperLimit);
    _unitOfWork.Commit();
    return true;
  }
}
