namespace InfonetCSCR.Core.Abstractions.Services;

public interface ISecurityLevelService
{
  Task<IEnumerable<SecurityLevel>?> GetSecurityLevels(int loggedInUserSecurityLevel);
}
