using ContactsApp.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace ContactsApp.Repository
{
    /// <summary>
    /// A more specific <see cref="IUnitOfWork"/> that also exposes
    /// the underlying <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="TContext"/> to work with.</typeparam>
    public interface IUnitOfWorkContext<TContext>: IUnitOfWork
        where TContext: DbContext
    {
        /// <summary>
        /// Access to the <see cref="TContext"/>.
        /// </summary>
        TContext Context { get; }
    }
}
