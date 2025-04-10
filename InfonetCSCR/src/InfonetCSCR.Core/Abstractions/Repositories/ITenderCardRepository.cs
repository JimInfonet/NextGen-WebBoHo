namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITenderCardRepository
{
  Task<bool> CheckTendCodeExistAsync(string tendCode);
  Task<TenderCard> GetTenderCardDataByCardCodeAsync(string cardCode);
  Task<bool> AddTenderCardAsync(TenderCard tenderCard);
  Task<bool> UpdateTenderCardAsync(TenderCard tenderCard);
  Task<bool> UpdateCardCodeOnTenderCardAsync(string tendCode, string cardCode);
  Task<bool> DeleteTenderCardByTendCodeAsync(string tendCode);
}
