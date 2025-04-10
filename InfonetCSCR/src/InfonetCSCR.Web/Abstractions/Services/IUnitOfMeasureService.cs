namespace InfonetCSCR.Web.Abstractions.Services;

public interface IUnitOfMeasureService
{
  Task<IList<UnitOfMeasureViewModel>?> GetAllUnitOfMeasures();
  Task<bool> AddUnitOfMeasure(UnitOfMeasureViewModel unitOfMeasure);
  Task<bool> UpdateUnitOfMeasure(UnitOfMeasureViewModel unitOfMeasure);
  Task<bool> DeleteUnitOfMeasure(string unitOfMeasureId);
}
