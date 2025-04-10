namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ISecurityLevelRepository
{
  Task<IEnumerable<SecurityLevel>?> GetSecurityLevels(int loggedInUserSecurityLevel);
}
