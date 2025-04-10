namespace InfonetCSCR.Core.Abstractions.Services;

public interface IUigService
{
  Task<IEnumerable<GroupInfoDto>?> GetGroupInfo(string userCode);
  Task<bool> AddUigRecord(Uig uig);
  Task<bool> DeleteUigRecord(Uig uig, bool isGroupIncluded = true);
}
