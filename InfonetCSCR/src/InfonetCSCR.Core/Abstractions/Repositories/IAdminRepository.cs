namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IAdminRepository
{
  Task<Admin> GetAdminByName(string name);
}
