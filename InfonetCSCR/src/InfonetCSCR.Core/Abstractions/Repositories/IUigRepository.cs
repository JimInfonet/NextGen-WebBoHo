namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IUigRepository
{
  Task<IEnumerable<GroupInfoDto>> GetGroupInfoAsync(string userCode);
  Task<bool> AddUigRecordAsync(Uig uig);
  Task<bool> DeleteUigRecordAsync(Uig uig, bool isGroupIncluded = true);
}
