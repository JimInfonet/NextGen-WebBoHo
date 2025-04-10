namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IEODGroupRepository
{
  Task<bool> CheckGroupIdExistsAsync(string eodGroupId);
  Task<bool> AddEODGroupAsync(EodGroup eodGroup);
  Task<IEnumerable<EodGroup>> GetGroupsAsync();
  Task<bool> DeleteEodGroupByEodGroupId(string eodGroupId);
}
