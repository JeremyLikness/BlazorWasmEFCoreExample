using ContactsApp.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContactsApp.Repository
{
    /// <summary>
    /// Interface for a repository that exposes the <seealso cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="TContext"/> to use.</typeparam>
    /// <typeparam name="TEntity">The <see cref="TEntity"/> this repository works with.</typeparam>
    public interface IRepository<TContext, TEntity>:
        IBasicRepository<TEntity> where TContext : DbContext
    {
        /// <summary>
        /// Create a more specific <see cref="IUnitOfWork"/> with context.
        /// </summary>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The <see cref="IUnitOfWorkContext{TContext}"/> instance.</returns>
        new IUnitOfWorkContext<TContext> CreateUnitOfWork(ClaimsPrincipal user);

        /// <summary>
        /// Load an item in a unit of work.
        /// </summary>
        /// <param name="id">The id of the <see cref="TEntity"/> to load.</param>
        /// <param name="unitOfWork">
        /// The <see cref="IUnitOfWorkContext{TContext}"/>
        /// to load it in.
        /// </param>
        /// <returns>The <see cref="TEntity"/> instance.</returns>
        Task<TEntity> LoadAsync(int id, IUnitOfWorkContext<TContext> unitOfWork);
    }
}
