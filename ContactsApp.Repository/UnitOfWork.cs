using ContactsApp.BaseRepository;
using ContactsApp.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContactsApp.Repository
{
    /// <summary>
    /// Simple wrapper around a <see cref="IBasicRepository{TEntity}"/>.
    /// Persists the repo that persists the context.
    /// </summary>
    public class UnitOfWork<TContext, TEntity> : 
        IUnitOfWork<TEntity>
        where TContext: DbContext, ISupportUser
    {
        /// <summary>
        /// The repo to work with.
        /// </summary>
        private IRepository<TEntity, TContext> _repo;

        /// <summary>
        /// Simple repo reference.
        /// </summary>
        public IBasicRepository<TEntity> Repo
        {
            get => _repo;            
        }

        /// <summary>
        /// Inject the context.
        /// </summary>
        /// <param name="context">The <see cref="ContactContext"/> to wrap.</param>
        public UnitOfWork(
            IRepository<TEntity, TContext> repo, DbContextFactory<TContext> factory)
        {
            repo.PersistedContext = factory.CreateDbContext();
            _repo = repo;
        }        

        /// <summary>
        /// Commit changes.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task CommitAsync()
        {
            try
            {
                await _repo.PersistedContext.SaveChangesAsync();                    
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // build the helper exception from the exception data
                var newex = new RepoConcurrencyException<TEntity>(
                    (TEntity)ex.Entries[0].Entity, ex);
                var dbValues = ex.Entries[0].GetDatabaseValues();

                // was deleted
                if (dbValues == null)
                {
                    newex.DbEntity = default;
                }
                else
                {
                    // update the new row version
                    newex.RowVersion = dbValues
                        .GetValue<byte[]>(ContactContext.RowVersion);
                    // grab the database version
                    newex.DbEntity = (TEntity)dbValues.ToObject();
                    // move to original so second submit works (unless there is another concurrent edit)
                    ex.Entries[0].OriginalValues.SetValues(dbValues);
                }
                throw newex;
            }
        }

        /// <summary>
        /// Unit of work is done.
        /// </summary>
        public void Dispose()
        {
            if (_repo != null)
            {
                _repo.Dispose();
                _repo = null;
            }
        }

        /// <summary>
        /// Sets the <see cref="ClaimsPrincipal"/> for audits.
        /// </summary>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        public void SetUser(ClaimsPrincipal user)
        {
            if (_repo.PersistedContext != null)
            {
                _repo.PersistedContext.User = user;
            }
        }
    }
}
