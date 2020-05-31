using ContactsApp.DataAccess;
using ContactsApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContactsApp.Repository
{
    /// <summary>
    /// Implementation of repository for <see cref="ContactContext"/>.
    /// </summary>
    public class ContactRepository : IRepository<Contact, ContactContext>
    {
        /// <summary>
        /// Factory to create contexts.
        /// </summary>
        private readonly DbContextFactory<ContactContext> _factory;
        private bool disposedValue;

        /// <summary>
        /// For longer tracked work.
        /// </summary>
        public ContactContext PersistedContext { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="ContactRepository"/> class.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="DbContextFactory{ContactContext}"/>
        /// to use.
        /// </param>
        public ContactRepository(DbContextFactory<ContactContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Performs some work, either using the peristed context or
        /// by generating a new context for the operation.
        /// </summary>
        /// <param name="work">The work to perform (passed a <see cref="ContactContext"/>).</param>
        /// <param name="user">The current <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="saveChanges"><c>True</c> to save changes when done.</param>
        /// <returns></returns>
        private async Task WorkInContextAsync(
            Func<ContactContext, Task> work, 
            ClaimsPrincipal user,
            bool saveChanges = false)
        {
            if (PersistedContext != null)
            {
                if (user != null)
                {
                    PersistedContext.User = user;
                }
                // do some work. Save changes flag is ignored because this will be
                // committed later.
                await work(PersistedContext);
            }
            else
            {
                using (var context = _factory.CreateDbContext())
                {
                    context.User = user;
                    await work(context);
                    if (saveChanges)
                    {
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Attaches an item to the <see cref="ContactContext"/>.
        /// </summary>
        /// <param name="item">The instance of the <see cref="Contact"/>.</param>
        public void Attach(Contact item)
        {
            if (PersistedContext == null)
            {
                throw new InvalidOperationException("Only valid in a unit of work.");
            }
            PersistedContext.Attach(item);
        }

        /// <summary>
        /// Adds a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to add.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The <see cref="Contact"/> with id set.</returns>
        public async Task<Contact> AddAsync(Contact item, ClaimsPrincipal user)
        {
            await WorkInContextAsync(context =>
            {
                context.Contacts.Add(item);
                return Task.CompletedTask;
            }, user, true);
            return item;
        }

        /// <summary>
        /// Delete a <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Contact"/> to delete.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns><c>True</c> when found and deleted.</returns>
        public async Task<bool> DeleteAsync(int id, ClaimsPrincipal user)
        {
            bool? result = null;
            await WorkInContextAsync(async context =>
            {
                var item = await context.Contacts.SingleOrDefaultAsync(c => c.Id == id);
                if (item == null)
                {
                    result = false;
                }
                else
                {
                    context.Contacts.Remove(item);
                }
            }, user, true);
            if (!result.HasValue)
            {
                result = true;
            }
            return result.Value;
        }

        /// <summary>
        /// Not implemented here (see the Blazor WebAssembly client).
        /// </summary>
        /// <returns>The <see cref="ICollection{Contact}"/>.</returns>
        public Task<ICollection<Contact>> GetListAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load a <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/> to load.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="forUpdate"><c>True</c> to keep tracking on.</param>
        /// <returns>The <see cref="Contact"/>.</returns>
        public async Task<Contact> LoadAsync(
            int id, 
            ClaimsPrincipal user,
            bool forUpdate = false)
        {
            Contact contact = null;
            await WorkInContextAsync(async context =>
            {
                var contactRef = context.Contacts;
                if (forUpdate)
                {
                    contactRef.AsNoTracking();
                }
                contact = await contactRef
                    .SingleOrDefaultAsync(c => c.Id == id);
            }, user);
            return contact;
        }

        /// <summary>
        /// Query the <see cref="Contact"/> database.
        /// </summary>
        /// <param name="query">
        /// A delegate that provides an <see cref="IQueryable{Contact}"/>
        /// to build on.
        /// </param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task QueryAsync(Func<IQueryable<Contact>, Task> query)
        {
            await WorkInContextAsync(async context =>
            {
                await query(context.Contacts.AsNoTracking().AsQueryable());
            }, null);
        }

        /// <summary>
        /// Update the <see cref="Contact"/> (without a unit of work).
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to update.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public async Task<Contact> UpdateAsync(Contact item, ClaimsPrincipal user)
        {
            await WorkInContextAsync(context =>
            {
                context.Contacts.Attach(item);
                return Task.CompletedTask;
            }, user, true);
            return item;
        }

        /// <summary>
        /// Grabs the value of a property. Useful for shadow properties.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="item">The <see cref="Contact"/> the property is on.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        public async Task<TPropertyType> GetPropertyValueAsync<TPropertyType>(
            Contact item, string propertyName)
        {
            TPropertyType value = default;
            await WorkInContextAsync(context =>
            {
                value = context.Entry(item)
                .Property<TPropertyType>(propertyName).CurrentValue;
                return Task.CompletedTask;
            }, null);
            return value;
        }

        /// <summary>
        /// Sets original value. This is useful to check concurrency if you have
        /// disconnected entities and are re-attaching to update.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="item">The <see cref="Contact"/> being tracked.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task SetOriginalValueForConcurrencyAsync<TPropertyType>(
            Contact item, 
            string propertyName,
            TPropertyType value)
        {
            await WorkInContextAsync(context =>
            {
                var tracked = context.Entry(item);
                // we tell EF Core what version we loaded
                tracked.Property<TPropertyType>(propertyName).OriginalValue =
                        value;
                // we tell EF Core to modify entity
                tracked.State = EntityState.Modified;
                return Task.CompletedTask;
            }, null);            
        }

        /// <summary>
        /// Implements the dipose pattern.
        /// </summary>
        /// <param name="disposing"><c>True</c> when disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (PersistedContext != null)
                    {
                        PersistedContext.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Implement <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
