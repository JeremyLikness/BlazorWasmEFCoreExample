using System.Threading.Tasks;
using ContactsApp.BaseRepository;
using ContactsApp.Model;

namespace ContactsApp.Client.Data
{
    /// <summary>
    /// Client <see cref="IUnitOfWork"/> implementation that simply tracks versions.
    /// </summary>
    public class WasmUnitOfWork : IUnitOfWork
    {
        private readonly IBasicRepository<Contact> _repo;

        /// <summary>
        /// The <see cref="Contact"/> being edited.
        /// </summary>
        public Contact OriginalContact { get; set; }

        /// <summary>
        /// The <see cref="Contact"/> that is in the database.
        /// </summary>
        public Contact DatabaseContact { get; set; }

        /// <summary>
        /// True if there is a conflict (only exists if that happens).
        /// </summary>
        public bool HasConcurrencyConflict => DatabaseContact != null;

        /// <summary>
        /// The version of the last read <see cref="Contact"/>.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="WasmUnitOfWork"/> class.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Contact}"/> implementation.</param>
        public WasmUnitOfWork(IBasicRepository<Contact> repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Time to commit.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task CommitAsync()
        {
            return _repo.UpdateAsync(OriginalContact, this);
        }

        /// <summary>
        /// Get rid of references.
        /// </summary>
        public void Dispose()
        {
            RowVersion = null;
            OriginalContact = null;
            DatabaseContact = null;
        }
    }
}
