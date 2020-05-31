using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="forUpdate"><c>True</c> to load concurrency information.</param>
        /// <returns>The <seealso cref="TEntity"/> instance.</returns>
        Task<TEntity> LoadAsync(int id, ClaimsPrincipal user, bool forUpdate = false);

        /// <summary>
        /// Delete an item.
        /// </summary>
        /// <param name="id">The id of the item to delete.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns><c>True</c> if the item is successfully deleted.</returns>
        Task<bool> DeleteAsync(int id, ClaimsPrincipal user);

        /// <summary>
        /// Attach an entity to the database context for tracking.
        /// </summary>
        /// <param name="item">The <see cref="TEntity"/> instance to attach.</param>
        void Attach(TEntity item);

        /// <summary>
        /// Add a new item.
        /// </summary>
        /// <param name="item">The <see cref="TEntity"/> to add.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
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
        /// Get a property from the instance. Mainly used for shadow properties.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="item">The <see cref="TEntity"/> instance the property is on.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value.</returns>
        Task<TPropertyType> GetPropertyValueAsync<TPropertyType>(
            TEntity item, string propertyName);

        /// <summary>
        /// Set the original value to trigger concurrency checks.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="item">The <see cref="TEntity"/> to set the original value for.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Task SetOriginalValueForConcurrencyAsync<TPropertyType>(
            TEntity item, string propertyName, TPropertyType value);
    }
}
