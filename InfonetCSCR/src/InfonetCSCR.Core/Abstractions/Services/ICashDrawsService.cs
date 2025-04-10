namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICashDrawsService
{
  Task<IList<DrawTypes>> GetAllDrawTypesData();
  Task<bool> InsertDrawType(DrawTypes drawTypes);
  Task<bool> UpdateDrawType(DrawTypes drawTypes);
  Task<bool> DeleteDrawType(DrawTypes drawTypes);
}
