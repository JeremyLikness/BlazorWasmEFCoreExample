using System.Security.Claims;
using System.Threading.Tasks;
using ContactsApp.BaseRepository;
using ContactsApp.Model;

namespace ContactsApp.Client.Data
{
    /// <summary>
    /// Client <see cref="IUnitOfWork"/> implementation that simply tracks versions.
    /// </summary>
    public class WasmUnitOfWork : IUnitOfWork<Contact>
    {
        /// <summary>
        /// The <see cref="Contact"/> being edited.
        /// </summary>
        public Contact OriginalContact { get => _repo.OriginalContact; }

        /// <summary>
        /// The <see cref="Contact"/> that is in the database.
        /// </summary>
        public Contact DatabaseContact { get => _repo.DatabaseContact; }

        /// <summary>
        /// True if there is a conflict (only exists if that happens).
        /// </summary>
        public bool HasConcurrencyConflict => _repo.DatabaseContact != null;

        /// <summary>
        /// The version of the last read <see cref="Contact"/>.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Repository instance.
        /// </summary>
        private readonly WasmRepository _repo;

        /// <summary>
        /// Expose the <see cref="IBasicRepository{Contact}"/> interface.
        /// </summary>
        public IBasicRepository<Contact> Repo { get => _repo; }

        /// <summary>
        /// Creates a new instance of the <see cref="WasmUnitOfWork"/> class.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Contact}"/> implementation.</param>
        public WasmUnitOfWork(IBasicRepository<Contact> repo)
        {
            _repo = repo as WasmRepository;
        }

        /// <summary>
        /// Time to commit.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task CommitAsync()
        {
            return Repo.UpdateAsync(OriginalContact, null);
        }

        /// <summary>
        /// Get rid of references.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="user"></param>
        public void SetUser(ClaimsPrincipal user)
        {
            throw new System.NotImplementedException();
        }
    }
}
