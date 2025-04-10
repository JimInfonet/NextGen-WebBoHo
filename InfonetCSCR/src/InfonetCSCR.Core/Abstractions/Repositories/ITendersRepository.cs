namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITendersRepository
{
  Task<IEnumerable<Tender>> GetTendersWithCardCodeAsync();
  Task<IEnumerable<Payout>> GetPayoutAsync();
  Task<bool> UpdateTenderAsync(Tender tender);
  Task<bool> DeleteTenderAsync(string tendCode);
  Task<bool> AddTenderAsync(Tender tender);
  Task<IEnumerable<TenderClass>> GetTenderClassesAsync();
  Task<Tender> GetSingleTenderAsync(string tendCode);
  Task<bool> IsTenderCodeExistAsync(string tendCode);
  Task<bool> IsFullNameExistAsync(string fullName);
  Task<IEnumerable<Tender>> GetTendersListAsync();
  Task<IEnumerable<Tender>> GetTendersListByClassNameAsync(string tendClass);
}
