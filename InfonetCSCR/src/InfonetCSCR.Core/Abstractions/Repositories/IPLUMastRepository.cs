namespace InfonetCSCR.Core.Abstractions.Repositories;
public interface IPLUMastRepository
{
  Task<bool> CheckPLUCodeExistsForPLUPrimeAsync(string pluPrime, string pluCode);
  Task<PLUMast?> GetPLUMastByPLUCodeAsync(string pluCode);
  Task<IEnumerable<PLUMast>?> GetPLUCodesByPLUPrimeAsync(string pluPrime);
  Task<bool> AddPLUMastAsync(PLUMast pluMast);
  Task<bool> DeletePLUMastByPLUPrimAsync(string pluPrim);
  Task<bool> DeletePLUMastByPLUCodeAsync(string pluCode);
  Task<IEnumerable<PLUMast?>> GetPLUMast();
  Task<IEnumerable<PLUMast>> GetPluMastsForStockTakes();
}

