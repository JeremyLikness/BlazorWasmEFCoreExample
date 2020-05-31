using System;
using System.Threading.Tasks;
using ContactsApp.BaseRepository;
using ContactsApp.Model;
using ContactsApp.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactsApp.Repository;
using ContactConcurrencyResolver = ContactsApp.Client.Data.ContactConcurrencyResolver;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsApp.Server.Controllers
{
    /// <summary>
    /// Services the <see cref="Contact"/> C.R.U.D. operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly IBasicRepository<Contact> _repo;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="ContactsController"/>.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Contact}"/> repo to work with.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> for dependency resolution.</param>
        public ContactsController(IBasicRepository<Contact> repo,
            IServiceProvider provider)
        {
            _repo = repo;
            _serviceProvider = provider;
        }

        /// <summary>
        /// Get a <see cref="Contact"/>.
        /// </summary>
        /// <example>GET /api/contacts/1?forUpdate=true</example>
        /// <param name="id">The id of the <see cref="Contact"/>.</param>
        /// <param name="forUpdate"><c>True</c> to fetch additional concurrency info.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id,
            [FromQuery] bool forUpdate = false)
        {
            if (id < 1)
            {
                return new NotFoundResult();
            }
            if (forUpdate)
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork<Contact>>();
                HttpContext.Response.RegisterForDispose(unitOfWork);
                var result = await unitOfWork.Repo.LoadAsync(id, User, true);

                // return version for tracking on client. It is not
                // part of the C# class so it is tracked as a "shadow property"
                var concurrencyResult = new ContactConcurrencyResolver
                {
                    OriginalContact = result,
                    RowVersion = result == null ? null :
                    await unitOfWork.Repo.GetPropertyValueAsync<byte[]>(
                        result, ContactContext.RowVersion)
                };
                return new OkObjectResult(concurrencyResult);
            }
            else
            {
                var result = await _repo.LoadAsync(id, User);
                return result == null ? (IActionResult)new NotFoundResult() :
                    new OkObjectResult(result);
            }
        }

        /// <summary>
        /// Add a new <see cref="Contact"/>.
        /// </summary>
        /// <example>POST /api/contacts</example>
        /// <param name="contact"></param>
        /// <returns>The <see cref="Contact"/> with id.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync(
            [FromBody] Contact contact)
        {
            return contact == null
                ? new BadRequestResult()
                : ModelState.IsValid ?
                new OkObjectResult(await _repo.AddAsync(contact, User)) :
                (IActionResult)new BadRequestObjectResult(ModelState);
        }

        /// <summary>
        /// Update a <see cref="Contact"/>.
        /// </summary>
        /// <example>PUT /api/contacts/1</example>
        /// <param name="id">The id of the <see cref="Contact"/>.</param>
        /// <param name="value">The <see cref="ContactConcurrencyResolver"/> payload.</param>
        /// <returns>An <see cref="IActionResult"/> of OK or Conflict.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id,
            [FromBody] ContactConcurrencyResolver value)
        {
            // I got nothing
            if (value == null || value.OriginalContact == null
                || value.OriginalContact.Id != id)
            {
                return new BadRequestResult();
            }
            if (ModelState.IsValid)
            {
                var unitOfWork = _serviceProvider.GetService<IUnitOfWork<Contact>>();
                HttpContext.Response.RegisterForDispose(unitOfWork);
                unitOfWork.SetUser(User);
                // this gets the contact on the board for EF Core
                unitOfWork.Repo.Attach(value.OriginalContact);
                await unitOfWork.Repo.SetOriginalValueForConcurrencyAsync(
                    value.OriginalContact, ContactContext.RowVersion, value.RowVersion);
                try
                {
                    await unitOfWork.CommitAsync();
                    return new OkResult();
                }
                catch (RepoConcurrencyException<Contact> dbex)
                {
                    // oops it has been updated, so send back the database version
                    // and the new RowVersion in case the user wants to override
                    value.DatabaseContact = dbex.DbEntity;
                    value.RowVersion = dbex.RowVersion;
                    return new ConflictObjectResult(value);
                }
            }
            else
            {
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Delete a <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/> to delete.</param>
        /// <returns>The <see cref="IActionResult"/>.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var result = await _repo.DeleteAsync(id, User);
                return result ?
                    new OkResult() :
                    (IActionResult)new NotFoundResult();
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
