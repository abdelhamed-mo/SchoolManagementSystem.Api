using Domain.Entities;

namespace Domain.Contracts
{
	public interface IUnitOfWork 
	{
		public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class;
		public Task<int> SaveChangesAsync();

	}
}
