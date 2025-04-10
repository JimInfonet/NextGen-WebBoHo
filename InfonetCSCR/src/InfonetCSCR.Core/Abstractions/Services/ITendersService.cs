namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITendersService
{
  Task<IEnumerable<Tender>> GetTendersWithCardCodeAsync();
  Task<IEnumerable<Payout>> GetPayout();
  Task<bool> UpdateTender(Tender tender);
  Task<bool> DeleteTender(string tendCode);
  Task<bool> AddTender(Tender tender);
  Task<IEnumerable<TenderClass>> GetTenderClasses();
  Task<Tender> GetSingleTender(string tendCode);
  Task<bool> IsTenderCodeExist(string tendCode);
  Task<bool> IsFullNameExist(string fullName);
  Task<bool> ResetSequence();
  Task<IEnumerable<Tender>> GetTendersListAsync();
  Task<IEnumerable<Tender>> GetTendersListByClassNameAsync(string tendClass);
}
