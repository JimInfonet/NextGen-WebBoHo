namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICardCodesRepository
{
  Task<IEnumerable<CardRange>> GetCardRangesByCardCodeAsync(string cardCode);
  Task<CardRange> GetCardRangeByLowerLimitAsync(int cardCode, string lowerLimit);
  Task<bool> AddCardRangeAsync(int cardCode, string lowerLimit, string upperLimit);
  Task<bool> DeleteCardRangesByCardCodeDataAsync(string cardCode, string lowerLimit, string upperLimit);
  Task<bool> DeleteCardRangesByCardCodeAsync(string cardCode);
}
