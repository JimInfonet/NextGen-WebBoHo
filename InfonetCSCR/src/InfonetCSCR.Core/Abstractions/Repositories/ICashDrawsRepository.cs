namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICashDrawsRepository
{
  Task<IList<DrawTypes>> GetAllDrawTypesDataAsync();
  Task<bool> InsertDrawTypeAsync(DrawTypes drawType);
  Task<bool> UpdateDrawTypeAsync(DrawTypes drawType);
  Task<bool> DeleteDrawTypeAsync(DrawTypes drawType);
}
