namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IRepository<T> where T : class
{
    Task<int> CreateAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(T entity);
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
}
