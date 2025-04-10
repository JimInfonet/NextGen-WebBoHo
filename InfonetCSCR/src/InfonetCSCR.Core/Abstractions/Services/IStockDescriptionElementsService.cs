namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockDescriptionElementsService
{
  Task<IEnumerable<Warehouse>> GetWarehouse();
  Task<IEnumerable<Grade>> GetGrade(string stockCode);
}
