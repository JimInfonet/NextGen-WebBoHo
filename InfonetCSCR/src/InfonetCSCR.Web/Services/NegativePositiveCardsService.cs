namespace InfonetCSCR.Web.Services;

public class NegativePositiveCardsService : INegativePositiveCardsService
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _appConfig;

  public NegativePositiveCardsService(IHttpService http, IApplicationConfig appConfig)
  {
    _http = http;
    _appConfig = appConfig;
  }

  #region Private Methods

  // File type NegativeCardLatest is NCLatest.txt file.
  // The data formate will be like : CardNumber,Status,EntryDate,ExpiryDate
  // Example: "127",A,03/14/2024,03/14/2024,
  // Here Status A means add and D means remove
  private async Task<ReadNegativeCardLogViewModel> HandleFileTypeNegativeCardLatest(string filePath)
  {
    ReadNegativeCardLogViewModel readNegativeCardLog = new ReadNegativeCardLogViewModel();
    readNegativeCardLog.FilePath = filePath;

    Dictionary<string, (string, int, DateTime?, DateTime?)> negativeCardDataDictionary = new();
    Dictionary<string, (string, bool, string)> positiveCardDataDictionary = new();
    IList<NegativeCardViewModel>? negativeCards = await GetAllNegativeCardsData();
    IList<PositiveCardViewModel>? positiveCards = await GetAllPositiveCardsData();
    bool isDeleteAllNegativeCards = false;
    bool isFirstIteration = true;

    using (StreamReader sr = new StreamReader(filePath))
    {
      while (!sr.EndOfStream)
      {
        string textline = sr.ReadLine();

        if (textline == null || textline.Length == 0)
        {
          readNegativeCardLog.NumberOfLinesErrorFromFile++;
          readNegativeCardLog.NumberOfLines++;
          continue;
        }

        if (textline == "*,\"D\"" && isFirstIteration)
        {
          negativeCards = null;
          isDeleteAllNegativeCards = true;
          continue;
        }
        else
        {
          string[] splitText = textline.Split(',');
          string operationType = "";
          string cardNumber = "";

          if (splitText.Length <= 1)
          {
            readNegativeCardLog.NumberOfLinesErrorFromFile++;
            readNegativeCardLog.NumberOfLines++;
            continue;
          }

          if (splitText[1].Length >= 2)
          {
            operationType = splitText[1].Substring(1, splitText[1].Length - 2);
          }
          else
          {
            operationType = splitText[1];
          }

          if (splitText[0].Length >= 2)
          {
            splitText[0] = splitText[0].Substring(1, splitText[0].Length - 2);
            cardNumber = splitText[0];
          }
          else
          {
            readNegativeCardLog.NumberOfLinesErrorFromFile++;
            readNegativeCardLog.NumberOfLines++;
            continue;
          }

          if (operationType != "A" && operationType != "D") // Here A means Add and D means remove
          {
            readNegativeCardLog.NumberOfLinesErrorFromFile++;
            readNegativeCardLog.NumberOfLines++;
            continue;
          }

          if (operationType == "A")
          {
            bool isNegativeCardExists = CheckNegativeCardAlreadyExistsForAdd(negativeCardDataDictionary, negativeCards, cardNumber);

            if (!isNegativeCardExists)
            {
              AddNegativeCardDataInDictionary(negativeCardDataDictionary, readNegativeCardLog, splitText, false);
            }
            else
            {
              readNegativeCardLog.NumberOfLinesCannotAddedToNegativeCard++;
            }

            bool isPositiveCardExists = CheckPositiveCardAlreadyExistsForRemove(positiveCardDataDictionary, positiveCards, cardNumber);

            if (isPositiveCardExists)
            {
              string cardNumberFromFile = splitText[0];
              positiveCardDataDictionary.Add(cardNumberFromFile,
                ("D", false, ""));

              readNegativeCardLog.NumberOfLinesDeletedFromPositiveCard++;
            }
            else
            {
              readNegativeCardLog.NumberOfLinesCannotDeletedFromPositiveCard++;
            }
          }
          else
          {
            bool isCardExists = CheckNegativeCardAlreadyExistsForRemove(negativeCardDataDictionary, negativeCards, cardNumber);

            if (isCardExists)
            {
              string cardNumberFromFile = splitText[0];

              negativeCardDataDictionary.Add(cardNumberFromFile,
                ("D", 0, null, null));
              readNegativeCardLog.NumberOfLinesDeletedFromNegativeCard++;
            }
            else
            {
              readNegativeCardLog.NumberOfLinesCannotDeletedFromNegativeCard++;
            }

            bool isPositiveCardExists = CheckPositiveCardAlreadyExistsForAdd(positiveCardDataDictionary, positiveCards, cardNumber);

            if (!isPositiveCardExists)
            {
              string cardNumberFromFile = splitText[0];

              if (cardNumberFromFile.Length >= 1 && cardNumberFromFile.Length <= 50)
              {
                positiveCardDataDictionary.Add(cardNumberFromFile,
                  ("A", false, ""));

                readNegativeCardLog.NumberOfLinesAddedToPositiveCard++;
              }
              else
              {
                readNegativeCardLog.NumberOfLinesCannotAddedToPositiveCard++;
              }
            }
            else
            {
              readNegativeCardLog.NumberOfLinesCannotAddedToPositiveCard++;
            }
          }
        }

        readNegativeCardLog.NumberOfLines++;
        isFirstIteration = false;
      }
    }

    if (isDeleteAllNegativeCards)
    {
      await DeleteAllNegativeCards();
    }

    await AddOrDeleteNegativeCardData(negativeCardDataDictionary);
    await AddOrDeletePositiveCardData(positiveCardDataDictionary);

    return readNegativeCardLog;
  }

  // File type U is Uyymmdd.NCF file.
  // File type R is Ryymmdd.NCF file.
  // The data formate will be like : Status,CardNumber,MessageCode,EntryDate,ExpiryDate
  // Example: +,11,3,03/14/2024,03/14/2024,
  // Here Status + means add and - means remove
  private async Task<ReadNegativeCardLogViewModel> HandleFileTypeUOrR(string filePath)
  {
    ReadNegativeCardLogViewModel readNegativeCardLog = new ReadNegativeCardLogViewModel();
    readNegativeCardLog.FilePath = filePath;

    Dictionary<string, (string, int, DateTime?, DateTime?)> negativeCardDataDictionary = new();
    IList<NegativeCardViewModel>? negativeCards = await GetAllNegativeCardsData();
    bool isDeleteAllNegativeCards = false;
    bool isFirstIteration = true;

    using (StreamReader sr = new StreamReader(filePath))
    {
      while (!sr.EndOfStream)
      {
        string? textline = sr.ReadLine();
        if (textline == null)
        {
          readNegativeCardLog.NumberOfLines++;
          readNegativeCardLog.NumberOfLinesErrorFromFile++;
          continue;
        }

        if (textline == "-,*" && isFirstIteration)
        {
          negativeCards = null;
          isDeleteAllNegativeCards = true;
          continue;
        }
        else
        {
          string[] splitText = textline.Split(',');
          if (splitText[0] != "+" && splitText[0] != "-") // Here + means Add and - means remove
          {
            readNegativeCardLog.NumberOfLines++;
            readNegativeCardLog.NumberOfLinesErrorFromFile++;
            continue;
          }

          if (splitText[0] == "+")
          {
            if (splitText.Length < 3)
            {
              readNegativeCardLog.NumberOfLines++;
              readNegativeCardLog.NumberOfLinesErrorFromFile++;
              continue;
            }

            string cardNumber = splitText[1];
            bool isCardExists = CheckNegativeCardAlreadyExistsForAdd(negativeCardDataDictionary, negativeCards, cardNumber);

            if (!isCardExists)
            {
              AddNegativeCardDataInDictionary(negativeCardDataDictionary, readNegativeCardLog, splitText, true);
            }
            else
            {
              readNegativeCardLog.NumberOfLinesCannotAddedToNegativeCard++;
            }
          }
          else
          {
            string cardNumber = splitText[1];
            bool isCardExists = CheckNegativeCardAlreadyExistsForRemove(negativeCardDataDictionary, negativeCards, cardNumber);

            if (isCardExists)
            {
              string cardNumberFromFile = splitText[1];

              negativeCardDataDictionary.Add(cardNumberFromFile,
                ("-", 0, null, null));
              readNegativeCardLog.NumberOfLinesDeletedFromNegativeCard++;
            }
            else
            {
              readNegativeCardLog.NumberOfLinesCannotDeletedFromNegativeCard++;
            }
          }
        }

        readNegativeCardLog.NumberOfLines++;
        isFirstIteration = false;
      }
    }

    if (isDeleteAllNegativeCards)
    {
      await DeleteAllNegativeCards();
    }

    await AddOrDeleteNegativeCardData(negativeCardDataDictionary);

    return readNegativeCardLog;
  }

  private void AddNegativeCardDataInDictionary(
    Dictionary<string, (string, int, DateTime?, DateTime?)> negativeCardDataDictionary,
    ReadNegativeCardLogViewModel readNegativeCardLog,
    string[] splitText,
    bool isFileTypeUOrR)
  {
    string cardNumberFromFile = isFileTypeUOrR ? splitText[1] : splitText[0];

    if (isFileTypeUOrR && !int.TryParse(splitText[2], out int parsedInt))
    {
      splitText[2] = "0";
    }

    int messageCodeFromFile = isFileTypeUOrR ? int.Parse(splitText[2]) : 0;
    DateTime? entryDateFromFile = null;
    DateTime? expiryDateFromFile = null;

    // For file type U or R, the data formate will be like : Status,CardNumber,MessageCode,EntryDate,ExpiryDate
    // Example: +,11,3,03/14/2024,03/14/2024,
    // So to extract EntryDate the list length must be at least 4
    // And to extract ExpiryDate the list length must be at least 5

    // For file type nclatest.txt, the data formate will be like : CardNumber,Status,EntryDate,ExpiryDate
    // Example: "127",A,03/14/2024,03/14/2024,
    // So to extract EntryDate the list length must be at least 3
    // And to extract ExpiryDate the list length must be at least 4
    int limitForEntryDate = isFileTypeUOrR ? 4 : 3;
    int limitForExpiryDate = isFileTypeUOrR ? 5 : 4;

    string status = isFileTypeUOrR ? "+" : "A";

    bool isDataValid = false;

    isDataValid = (cardNumberFromFile.Length >= 1 && cardNumberFromFile.Length <= 20);
    isDataValid = (messageCodeFromFile >= short.MinValue && messageCodeFromFile <= short.MaxValue) && isDataValid;

    DateTime minDate = new DateTime(1900, 1, 1);
    DateTime maxDate = new DateTime(2079, 6, 6);
    string[] formats = { "MM-dd-yyyy", "MM/dd/yyyy" };

    bool isAddDate = true;
    if (!isFileTypeUOrR && splitText.Length < limitForExpiryDate)
    {
      isAddDate = false;
    }

    if (splitText.Length >= limitForEntryDate && isAddDate && DateTime.TryParseExact(splitText[limitForEntryDate - 1], formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
    {
      entryDateFromFile = DateTime.ParseExact(splitText[limitForEntryDate - 1], formats, CultureInfo.InvariantCulture);
      isDataValid = (entryDateFromFile >= minDate && entryDateFromFile <= maxDate) && isDataValid;
    }

    if (splitText.Length >= limitForExpiryDate && isAddDate && DateTime.TryParseExact(splitText[limitForExpiryDate - 1], formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
    {
      expiryDateFromFile = DateTime.ParseExact(splitText[limitForExpiryDate - 1], formats, CultureInfo.InvariantCulture);
      isDataValid = (expiryDateFromFile >= minDate && expiryDateFromFile <= maxDate) && isDataValid;
    }

    if (!isDataValid)
    {
      readNegativeCardLog.NumberOfLinesCannotAddedToNegativeCard++;
      return;
    }

    negativeCardDataDictionary.Add(cardNumberFromFile,
    (status, messageCodeFromFile, entryDateFromFile, expiryDateFromFile));
    readNegativeCardLog.NumberOfLinesAddedToNegativeCard++;
  }

  private bool CheckNegativeCardAlreadyExistsForRemove(
    Dictionary<string, (string, int, DateTime?, DateTime?)> negativeCardDataDictionary,
    IList<NegativeCardViewModel>? negativeCards,
    string cardNumber)
  {
    bool isCardExists = false;
    if (negativeCardDataDictionary.TryGetValue(cardNumber, out (string, int, DateTime?, DateTime?) cardData))
    {
      if (cardData.Item1 == "+" || cardData.Item1 == "A")
      {
        negativeCardDataDictionary.Remove(cardNumber);
        isCardExists = true;
      }
    }
    else
    {
      NegativeCardViewModel? negativeCardFromDB = null;
      if (negativeCards != null)
      {
        negativeCardFromDB = negativeCards.FirstOrDefault(card => card.CardNumber == cardNumber);
      }
      if (negativeCardFromDB != null)
      {
        isCardExists = true;
      }
    }

    return isCardExists;
  }

  private bool CheckNegativeCardAlreadyExistsForAdd(
    Dictionary<string, (string, int, DateTime?, DateTime?)> negativeCardDataDictionary,
    IList<NegativeCardViewModel>? negativeCards,
    string cardNumber)
  {
    bool isCardExists = false;
    if (negativeCardDataDictionary.TryGetValue(cardNumber, out (string, int, DateTime?, DateTime?) cardData))
    {
      if (cardData.Item1 == "-" || cardData.Item1 == "D")
      {
        negativeCardDataDictionary.Remove(cardNumber);
      }
      else
      {
        isCardExists = true;
      }
    }
    else
    {
      NegativeCardViewModel? negativeCardFromDB = null;
      if (negativeCards != null)
      {
        negativeCardFromDB = negativeCards.FirstOrDefault(card => card.CardNumber == cardNumber);
      }
      if (negativeCardFromDB != null)
      {
        isCardExists = true;
      }
    }

    return isCardExists;
  }

  private bool CheckPositiveCardAlreadyExistsForRemove(
    Dictionary<string, (string, bool, string)> positiveCardDataDictionary,
    IList<PositiveCardViewModel>? positiveCards,
    string cardNumber)
  {
    bool isPositiveCardExists = false;
    if (positiveCardDataDictionary.TryGetValue(cardNumber, out (string, bool, string) positiveCard))
    {
      if (positiveCard.Item1 == "A" || positiveCard.Item1 == "+")
      {
        positiveCardDataDictionary.Remove(cardNumber);
        isPositiveCardExists = true;
      }
    }
    else
    {
      PositiveCardViewModel? positiveCardFromDB = null;
      if (positiveCards != null)
      {
        positiveCardFromDB = positiveCards.FirstOrDefault(card => card.CardNumber == cardNumber);
      }

      if (positiveCardFromDB != null)
      {
        isPositiveCardExists = true;
      }
    }

    return isPositiveCardExists;
  }

  private bool CheckPositiveCardAlreadyExistsForAdd(
    Dictionary<string, (string, bool, string)> positiveCardDataDictionary,
    IList<PositiveCardViewModel>? positiveCards,
    string cardNumber)
  {
    bool isPositiveCardExists = false;

    if (positiveCardDataDictionary.TryGetValue(cardNumber, out (string, bool, string) cardDataTmp))
    {
      if (cardDataTmp.Item1 == "D" || cardDataTmp.Item1 == "-")
      {
        positiveCardDataDictionary.Remove(cardNumber);
      }
      else
      {
        isPositiveCardExists = true;
      }
    }
    else
    {
      PositiveCardViewModel? positiveCardFromDB = null;
      if (positiveCards != null)
      {
        positiveCardFromDB = positiveCards.FirstOrDefault(card => card.CardNumber == cardNumber);
      }

      if (positiveCardFromDB != null)
      {
        isPositiveCardExists = true;
      }
    }
    return isPositiveCardExists;
  }

  private async Task<bool> AddOrDeleteNegativeCardData(
    Dictionary<string, (string, int, DateTime?, DateTime?)> negativeCardDataDictionary)
  {
    IList<NegativeCardViewModel> negativeCardsForAdd = new List<NegativeCardViewModel>();
    IList<NegativeCardViewModel> negativeCardsForDelete = new List<NegativeCardViewModel>();

    foreach (var negativeCardData in negativeCardDataDictionary)
    {
      string cardNumber = negativeCardData.Key;

      (string status, int messageCode, DateTime? entryDate, DateTime? expiryDate) = negativeCardData.Value;

      NegativeCardViewModel negativeCard = new NegativeCardViewModel
      {
        CardNumber = cardNumber,
        MessageCode = messageCode,
        EntryDate = entryDate,
        ExpiryDate = expiryDate
      };

      if (status == "+" || status == "A")
      {
        negativeCardsForAdd.Add(negativeCard);
        negativeCardsForDelete.Add(negativeCard);
      }
      else
      {
        negativeCardsForDelete.Add(negativeCard);
      }
    }

    await DeleteNegativeCardsBatch(negativeCardsForDelete);
    await InsertNegativeCardsBatch(negativeCardsForAdd);

    return true;
  }

  private async Task<bool> AddOrDeletePositiveCardData(
    Dictionary<string, (string, bool, string)> positiveCardDataDictionary)
  {
    IList<PositiveCardViewModel> positiveCardsForAdd = new List<PositiveCardViewModel>();
    IList<PositiveCardViewModel> positiveCardsForDelete = new List<PositiveCardViewModel>();

    foreach (var positiveCard in positiveCardDataDictionary)
    {
      string cardNumber = positiveCard.Key;

      (string status, bool active, string pin) = positiveCard.Value;

      PositiveCardViewModel positiveCardTmp = new PositiveCardViewModel
      {
        CardNumber = cardNumber,
        Active = active,
        PIN = pin
      };

      if (status == "A" || status == "+")
      {
        positiveCardsForAdd.Add(positiveCardTmp);
        positiveCardsForDelete.Add(positiveCardTmp);
      }
      else
      {
        positiveCardsForDelete.Add(positiveCardTmp);
      }
    }

    await DeletePositiveCardsBatch(positiveCardsForDelete);
    await InsertPositiveCardsBatch(positiveCardsForAdd);

    return true;
  }

  #endregion

  public async Task<IList<PositiveCardViewModel>?> GetAllPositiveCardsData()
  {
    string url = $"NegativePositiveCards/GetAllPositiveCardsData";
    IList<PositiveCardViewModel>? positiveCards = await _http.GetAndParseAsync<IList<PositiveCardViewModel>?>(url);
    return positiveCards;
  }

  public async Task<bool> InsertPositiveCard(PositiveCardViewModel positiveCard)
  {
    string url = $"NegativePositiveCards/InsertPositiveCard";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, positiveCard);
    return isAdded;
  }

  public async Task<bool> DeletePositiveCard(PositiveCardViewModel positiveCard)
  {
    string url = $"NegativePositiveCards/DeletePositiveCard";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, positiveCard);
    return isDeleted;
  }

  public async Task<IList<NegativeCardViewModel>?> GetAllNegativeCardsData()
  {
    string url = $"NegativePositiveCards/GetAllNegativeCardsData";
    IList<NegativeCardViewModel>? negativeCards = await _http.GetAndParseAsync<IList<NegativeCardViewModel>?>(url);
    return negativeCards;
  }

  public async Task<bool> InsertNegativeCard(NegativeCardViewModel negativeCard)
  {
    string url = $"NegativePositiveCards/InsertNegativeCard";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, negativeCard);
    return isAdded;
  }

  public async Task<bool> DeleteNegativeCard(NegativeCardViewModel negativeCard)
  {
    string url = $"NegativePositiveCards/DeleteNegativeCard";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, negativeCard);
    return isDeleted;
  }

  public async Task<IList<CardMessageViewModel>?> GetAllCardMessagesData()
  {
    string url = $"NegativePositiveCards/GetAllCardMessagesData";
    IList<CardMessageViewModel>? cardMessages = await _http.GetAndParseAsync<IList<CardMessageViewModel>?>(url);
    return cardMessages;
  }

  public async Task<bool> InsertCardMessage(CardMessageViewModel cardMessage)
  {
    string url = $"NegativePositiveCards/InsertCardMessage";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, cardMessage);
    return isAdded;
  }

  public async Task<bool> DeleteCardMessage(CardMessageViewModel cardMessage)
  {
    string url = $"NegativePositiveCards/DeleteCardMessage";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, cardMessage);
    return isDeleted;
  }

  public async Task<bool> InsertNegativeCardsBatch(IList<NegativeCardViewModel> negativeCards)
  {
    string url = $"NegativePositiveCards/InsertNegativeCardsBatch";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, negativeCards);
    return isAdded;
  }

  public async Task<bool> DeleteNegativeCardsBatch(IList<NegativeCardViewModel> negativeCards)
  {
    string url = $"NegativePositiveCards/DeleteNegativeCardsBatch";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, negativeCards);
    return isDeleted;
  }

  public async Task<bool> DeleteAllNegativeCards()
  {
    string url = $"NegativePositiveCards/DeleteAllNegativeCards";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url);
    return isDeleted;
  }


  public async Task<bool> InsertPositiveCardsBatch(IList<PositiveCardViewModel> positiveCards)
  {
    string url = $"NegativePositiveCards/InsertPositiveCardsBatch";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, positiveCards);
    return isAdded;
  }

  public async Task<bool> DeletePositiveCardsBatch(IList<PositiveCardViewModel> positiveCards)
  {
    string url = $"NegativePositiveCards/DeletePositiveCardsBatch";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, positiveCards);
    return isDeleted;
  }

  public async Task<ReadNegativeCardLogViewModel> ReadNegativeCard()
  {
    string folderPath = _appConfig.IsBackOffice ?
      _appConfig.FileConfig.BackOfficeFolderPath : _appConfig.FileConfig.HeadOfficeFolderPath;

    string negativeFilePath = Path.Combine(folderPath, "U");
    negativeFilePath += DateTime.Now.ToString("yyMMdd") + ".NCF";
    ReadNegativeCardLogViewModel? readNegativeCardLog = new();

    if (File.Exists(negativeFilePath))
    {
      readNegativeCardLog = await HandleFileTypeUOrR(negativeFilePath);
      return readNegativeCardLog;
    }

    negativeFilePath = Path.Combine(folderPath, "R");
    negativeFilePath += DateTime.Now.ToString("yyMMdd") + ".NCF";

    if (File.Exists(negativeFilePath))
    {
      readNegativeCardLog = await HandleFileTypeUOrR(negativeFilePath);
      return readNegativeCardLog;
    }

    negativeFilePath = Path.Combine(folderPath, "nclatest.txt");


    if (File.Exists(negativeFilePath))
    {
      readNegativeCardLog = await HandleFileTypeNegativeCardLatest(negativeFilePath);
      return readNegativeCardLog;
    }

    string errorFilePath = folderPath + "\\U" + DateTime.Now.ToString("yyMMdd") + ".NCF";
    errorFilePath += "/R" + DateTime.Now.ToString("yyMMdd") + ".NCF";
    errorFilePath += "/nclatest.txt";

    readNegativeCardLog.ErrorFilePath = errorFilePath;

    return readNegativeCardLog;
  }

  // File type U is Uyymmdd.PCF file.
  // File type R is Ryymmdd.PCF file.
  // The data formate will be like : Status,CardNumber,Active,PIN
  // Example: +,11,1,12
  // Here Status + means add and - means remove
  private async Task<ReadPositiveCardLogViewModel> HandleFileTypeUOrRForPositiveCard(string filePath)
  {
    ReadPositiveCardLogViewModel readPositiveCardLog = new()
    {
      FilePath = filePath
    };

    Dictionary<string, (string, bool, string)> positiveCardDataDictionary = new();
    IList<PositiveCardViewModel>? positiveCards = await GetAllPositiveCardsData();
    bool isDeleteAllPositiveCards = false;
    bool isFirstIteration = true;

    using (StreamReader sr = new StreamReader(filePath))
    {
      while (!sr.EndOfStream)
      {
        string? textline = sr.ReadLine();
        if (textline == null)
        {
          readPositiveCardLog.NumberOfLines++;
          readPositiveCardLog.NumberOfLinesErrorFromFile++;
          continue;
        }

        if (textline == "-,*" && isFirstIteration)
        {
          positiveCards = null;
          isDeleteAllPositiveCards = true;
          continue;
        }
        else
        {
          string[] splitText = textline.Split(',');
          if (splitText[0] != "+" && splitText[0] != "-") // Here + means Add and - means remove
          {
            readPositiveCardLog.NumberOfLines++;
            readPositiveCardLog.NumberOfLinesErrorFromFile++;
            continue;
          }

          if (splitText.Length < 2)
          {
            readPositiveCardLog.NumberOfLines++;
            readPositiveCardLog.NumberOfLinesErrorFromFile++;
            continue;
          }

          if (splitText[0] == "+")
          {
            string cardNumber = splitText[1];

            bool active = false;
            if (splitText.Length >= 3)
            {
              active = !(string.IsNullOrEmpty(splitText[2]) || splitText[2].Trim().ToUpper() == "0");
            }

            string pin = "";
            if (splitText.Length >= 4)
            {
              pin = splitText[3];
              if (pin.Length < 1 || pin.Length > 15)
              {
                pin = "";
              }
            }

            if (cardNumber.Length >= 1 && cardNumber.Length <= 50)
            {
              if (positiveCardDataDictionary.ContainsKey(cardNumber))
              {
                positiveCardDataDictionary[cardNumber] = ("+", active, pin);
              }
              else
              {
                positiveCardDataDictionary.Add(cardNumber, ("+", active, pin));
              }

              readPositiveCardLog.NumberOfLinesAddedToPositiveCard++;
            }
            else
            {
              readPositiveCardLog.NumberOfLinesCannotAddedToPositiveCard++;
            }
          }
          else
          {
            bool isPositiveCardExists = CheckPositiveCardAlreadyExistsForRemove(positiveCardDataDictionary, positiveCards, splitText[1]);

            if (isPositiveCardExists)
            {
              string cardNumberFromFile = splitText[1];
              positiveCardDataDictionary.Add(cardNumberFromFile,
                ("-", false, ""));
              readPositiveCardLog.NumberOfLinesDeletedFromPositiveCard++;
            }
            else
            {
              readPositiveCardLog.NumberOfLinesCannotDeletedFromPositiveCard++;
            }
          }
        }

        readPositiveCardLog.NumberOfLines++;
        isFirstIteration = false;
      }
    }

    if (isDeleteAllPositiveCards)
    {
      await DeleteAllPositiveCards();
    }

    await AddOrDeletePositiveCardData(positiveCardDataDictionary);

    return readPositiveCardLog;
  }

  public async Task<ReadPositiveCardLogViewModel> ReadPositiveCard()
  {
    string folderPath = _appConfig.IsBackOffice ?
      _appConfig.FileConfig.BackOfficeFolderPath : _appConfig.FileConfig.HeadOfficeFolderPath;

    string positiveFilePath = Path.Combine(folderPath, "U");
    positiveFilePath += DateTime.Now.ToString("yyMMdd") + ".PCF";
    ReadPositiveCardLogViewModel? readPositiveCardLog = new();

    if (File.Exists(positiveFilePath))
    {
      readPositiveCardLog = await HandleFileTypeUOrRForPositiveCard(positiveFilePath);
      return readPositiveCardLog;
    }

    positiveFilePath = Path.Combine(folderPath, "R");
    positiveFilePath += DateTime.Now.ToString("yyMMdd") + ".PCF";

    if (File.Exists(positiveFilePath))
    {
      readPositiveCardLog = await HandleFileTypeUOrRForPositiveCard(positiveFilePath);
      return readPositiveCardLog;
    }

    string errorFilePath = "U" + DateTime.Now.ToString("yyMMdd") + ".PCF";
    errorFilePath += "/R" + DateTime.Now.ToString("yyMMdd") + ".PCF";

    readPositiveCardLog.ErrorFilePath = errorFilePath;

    return readPositiveCardLog;
  }

  public async Task<bool> DeleteAllPositiveCards()
  {
    string url = $"NegativePositiveCards/DeleteAllPositiveCards";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url);
    return isDeleted;
  }
}
