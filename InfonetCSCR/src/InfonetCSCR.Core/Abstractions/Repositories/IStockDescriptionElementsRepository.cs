namespace InfonetCSCR.Core.Abstractions.Repositories;
public interface IStockDescriptionElementsRepository
{
  Task<IEnumerable<Warehouse>> GetWarehouseAsync();
  Task<IEnumerable<Grade>> GetGradeAsync(string stockCode);
}