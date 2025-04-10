namespace InfonetCSCR.Core.Abstractions.Services;

public interface IUnitOfMeasureService
{
  Task<IList<UnitOfMeasure>> GetAllUnitOfMeasures();
  Task<bool> AddUnitOfMeasure(UnitOfMeasure unitOfMeasure);
  Task<bool> UpdateUnitOfMeasure(UnitOfMeasure unitOfMeasure);
  Task<bool> DeleteUnitOfMeasure(string unitOfMeasureId);
}
