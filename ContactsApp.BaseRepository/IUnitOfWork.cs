using System;
using System.Threading.Tasks;

namespace ContactsApp.BaseRepository
{
    /// <summary>
    /// A unit of work.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Commit the work.
        /// </summary>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        Task CommitAsync();
    }
}
