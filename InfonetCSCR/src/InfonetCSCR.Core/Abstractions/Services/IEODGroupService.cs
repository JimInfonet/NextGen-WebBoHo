namespace InfonetCSCR.Core.Abstractions.Services;

public interface IEODGroupService
{
  Task<bool> CheckGroupIdExists(string eodGroupId);
  Task<bool> AddEODGroup(EodGroup eodGroup);
  Task<IEnumerable<EodGroup>> GetEODGroups();
  Task<bool> DeleteEodGroupById(bool isDeptExistsForEodGroupId, string eodGroupId);
  Task<bool> UpdateEODGroupByDept(string eodGroupId, string dept);
}
