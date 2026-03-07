namespace Contracts.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task<T> CreateAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task<T> DeleteAsync(Guid id);
    }
}
