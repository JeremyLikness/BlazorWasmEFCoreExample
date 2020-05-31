using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContactsApp.BaseRepository
{
    /// <summary>
    /// Interface for a simple repository
    /// </summary>
    /// <typeparam name="TEntity">The class the repository is for.</typeparam>
    public interface IBasicRepository<TEntity>
    {
        /// <summary>
        /// Generate a unit of work.
        /// </summary>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>A new <seealso cref="IUnitOfWork"/>.</returns>
        IUnitOfWork CreateUnitOfWork(ClaimsPrincipal user);

        /// <summary>
        /// Apply a query and return results.
        /// </summary>
        /// <param name="query">A function that applies expressions to the
        ///  <seealso cref="IQueryable{TEntity}"/> instance.
        /// </param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task QueryAsync(Func<IQueryable<TEntity>, Task> query);

        /// <summary>
        /// Simple request for a <see cref="ICollection{TEntity}"/>.
        /// </summary>
        /// <returns>A <seealso cref="Task"/> that will resolve to
        /// an instance of <seealso cref="ICollection{TEntity}"/></returns>
        Task<ICollection<TEntity>> GetListAsync();

        /// <summary>
        /// Load a single item.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>The <seealso cref="TEntity"/> instance.</returns>
        Task<TEntity> LoadAsync(int id);

        /// <summary>
        /// Delete an item.
        /// </summary>
        /// <param name="id">The id of the item to delete.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns><c>True</c> if the item is successfully deleted.</returns>
        Task<bool> DeleteAsync(int id, ClaimsPrincipal user);

        /// <summary>
        /// Add a new item.
        /// </summary>
        /// <param name="item">The <see cref="TEntity"/> to add.</param>
        /// <returns>The <see cref="TEntity"/> instance with id set.</returns>
        Task<TEntity> AddAsync(TEntity item, ClaimsPrincipal user);

        /// <summary>
        /// Update an item.
        /// </summary>
        /// <param name="item">The <see cref="TEntity"/> to update.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The updated <see cref="TEntity"/>.</returns>
        Task<TEntity> UpdateAsync(TEntity item, ClaimsPrincipal user);

        /// <summary>
        /// Update an item in the context of an <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <param name="item">The <see cref="TEntity"/> to update.</param>
        /// <returns>The updated <see cref="TEntity"/>.</returns>
        Task<TEntity> UpdateAsync(TEntity item, IUnitOfWork unitOfWork);

        /// <summary>
        /// Load an item in the context of an <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="TEntity"/> to load.</param>
        /// <param name="unitOfWork">The <see cref="IUnitOfWork"/> to load it in.</param>
        /// <returns>The <see cref="TEntity"/> instance.</returns>
        Task<TEntity> LoadAsync(int id, IUnitOfWork unitOfWork);
    }
}
