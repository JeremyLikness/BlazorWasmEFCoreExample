using ContactsApp.BaseRepository;
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
    public class ContactRepository : IRepository<ContactContext, Contact>
    {
        /// <summary>
        /// Factory to create contexts
        /// </summary>
        private readonly DbContextFactory<ContactContext> _factory;

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
        /// Adds a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to add.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The <see cref="Contact"/> with id set.</returns>
        public async Task<Contact> AddAsync(Contact item, ClaimsPrincipal user)
        {
            using (var context = _factory.CreateDbContext())
            {
                context.User = user;
                context.Contacts.Add(item);
                await context.SaveChangesAsync();
                return item;
            }
        }

        /// <summary>
        /// Create a <see cref="IUnitOfWork"/> for a longer-lived transaction.
        /// </summary>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>A new instance of <see cref="IUnitOfWorkContext{ContactContext}"/>.</returns>
        public IUnitOfWorkContext<ContactContext> CreateUnitOfWork(ClaimsPrincipal user)
        {
            var unitOfWork = new UnitOfWork(_factory.CreateDbContext());
            unitOfWork.Context.User = user;
            return unitOfWork;
        }

        /// <summary>
        /// Delete a <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">Id of the <see cref="Contact"/> to delete.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns><c>True</c> when found and deleted.</returns>
        public async Task<bool> DeleteAsync(int id, ClaimsPrincipal user)
        {
            using (var context = _factory.CreateDbContext())
            {
                context.User = user;
                var item = await context.Contacts.SingleOrDefaultAsync(c => c.Id == id);
                if (item == null)
                {
                    return false;
                }
                context.Contacts.Remove(item);
                await context.SaveChangesAsync();
                return true;
            }
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
        /// <returns>The <see cref="Contact"/>.</returns>
        public async Task<Contact> LoadAsync(int id)
        {
            using (var context = _factory.CreateDbContext())
            {
                return await context.Contacts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            }
        }

        /// <summary>
        /// Load a <see cref="Contact"/> in a <see cref="IUnitOfWork"/> context.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/> to load.</param>
        /// <param name="unitOfWork">The <see cref="IUnitOfWorkContext{ContactContext}"/> to use.</param>
        /// <returns>The <see cref="Contact"/>.</returns>
        public Task<Contact> LoadAsync(int id, IUnitOfWorkContext<ContactContext> unitOfWork)
        {
            return unitOfWork.Context.Contacts.SingleOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Loads a <see cref="Contact"/> in a <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/> to load.</param>
        /// <param name="unitOfWork">The <see cref="IUnitOfWork"/> to use.</param>
        /// <returns>The <see cref="Contact"/>.</returns>
        public Task<Contact> LoadAsync(int id, IUnitOfWork unitOfWork)
        {
            return LoadAsync(id, unitOfWork as IUnitOfWorkContext<ContactContext>);
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
            using (var context = _factory.CreateDbContext())
            {
                await query(context.Contacts.AsNoTracking().AsQueryable());
            }
        }

        /// <summary>
        /// Update the <see cref="Contact"/> (without a unit of work).
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to update.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public async Task<Contact> UpdateAsync(Contact item, ClaimsPrincipal user)
        {
            using (var context = _factory.CreateDbContext())
            {
                context.User = user;
                context.Contacts.Attach(item);
                await context.SaveChangesAsync();
                return item;
            }
        }

        /// <summary>
        /// Update the <see cref="Contact"/> in a <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to update.</param>
        /// <param name="unitOfWork">The <see cref="IUnitOfWork"/> to use.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public Task<Contact> UpdateAsync(Contact item, IUnitOfWork unitOfWork)
        {
            return UpdateAsync(item, unitOfWork as IUnitOfWorkContext<ContactContext>);
        }

        /// <summary>
        /// Explicit implementation for the underlying interface.
        /// </summary>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>An <see cref="IUnitOfWork"/> instance.</returns>
        IUnitOfWork IBasicRepository<Contact>.CreateUnitOfWork(ClaimsPrincipal user)
        {
            return CreateUnitOfWork(user);
        }
    }
}
