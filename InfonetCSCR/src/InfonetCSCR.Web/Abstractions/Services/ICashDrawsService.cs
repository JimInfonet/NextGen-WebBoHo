namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICashDrawsService
{
  Task<List<DrawTypesViewModel>?> GetAllDrawTypesData();
  Task<bool> InsertDrawType(DrawTypesViewModel drawTypes);
  Task<bool> UpdateDrawType(DrawTypesViewModel drawTypes);
  Task<bool> DeleteDrawType(DrawTypesViewModel drawTypes);
}
