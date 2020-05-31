using ContactsApp.BaseRepository;
using ContactsApp.Model;
using ContactsApp.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ContactsApp.Repository
{
    /// <summary>
    /// Simple wrapper around a <see cref="ContactContext"/>.
    /// </summary>
    public class UnitOfWork : IUnitOfWorkContext<ContactContext>

    {
        /// <summary>
        /// Inject the context.
        /// </summary>
        /// <param name="context">The <see cref="ContactContext"/> to wrap.</param>
        public UnitOfWork(ContactContext context)
        {
            Context = context;
        }

        /// <summary>
        /// <c>True</c> when disposed.
        /// </summary>
        public bool Disposed => Context == null;

        /// <summary>
        /// The wrapped <see cref="ContactContext"/>.
        /// </summary>
        public ContactContext Context { get; private set; }

        /// <summary>
        /// Commit changes.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task CommitAsync()
        {
            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // build the helper exception from the exception data
                var newex = new ContactConcurrencyException(
                    (Contact)ex.Entries[0].Entity, ex);
                var dbValues = ex.Entries[0].GetDatabaseValues();

                // was deleted
                if (dbValues == null)
                {
                    newex.DbContact = null;
                }
                else
                {
                    // update the new row version
                    newex.RowVersion = dbValues
                        .GetValue<byte[]>(ContactContext.RowVersion);
                    // grab the database version
                    newex.DbContact = (Contact)dbValues.ToObject();
                    // move to original so second submit works (unless there is another concurrent edit)
                    ex.Entries[0].OriginalValues.SetValues(dbValues);
                }
                throw newex;
            }
            // only get here if no exceptions
            Dispose();
        }

        /// <summary>
        /// Unit of work is done.
        /// </summary>
        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }
    }
}
