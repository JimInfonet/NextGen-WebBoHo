namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IUnitOfMeasureRepository
{
  Task<IList<UnitOfMeasure>> GetAllUnitOfMeasuresAsync();
  Task<bool> AddUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure);
  Task<bool> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure);
  Task<bool> DeleteUnitOfMeasureAsync(string unitOfMeasureId);
}
