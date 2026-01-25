namespace MovieOpinions.Contracts.Models.Interface
{
    public interface IBaseRepository<TEntity, TResult>
    {
        Task<TResult> CreateAsync(TEntity entity);

        Task<TResult> UpdateAsync(TEntity entity);

        Task<TResult> DeleteAsync(Guid id);
    }
}
