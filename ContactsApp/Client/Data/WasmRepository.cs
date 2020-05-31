using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using ContactsApp.BaseRepository;
using ContactsApp.Controls.Grid;
using ContactsApp.Model;

namespace ContactsApp.Client.Data
{
    /// <summary>
    /// Client implementation of the <see cref="IBasicRepository{Contact}"/>.
    /// </summary>
    public class WasmRepository : IBasicRepository<Contact>
    {
        private readonly HttpClient _apiClient;
        private readonly GridControls _controls;

        private const string ApiPrefix = "/api/";
        private string ApiContacts => $"{ApiPrefix}contacts/";
        private string ApiQuery => $"{ApiPrefix}query/";
        private string ForUpdate => "?forUpdate=true";

        /// <summary>
        /// Creates a new instance of the <see cref="WasmRepository"/>.
        /// </summary>
        /// <param name="clientFactory">The <see cref="IHttpClientFactory"/> for communication with the server.</param>
        /// <param name="controls">The <see cref="GridControls"/> to parse queries and filters.</param>
        public WasmRepository(IHttpClientFactory clientFactory, GridControls controls)
        {
            _apiClient = clientFactory.CreateClient(Program.BaseClient);
            _controls = controls;
        }

        /// <summary>
        /// Add a contact
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to add.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The added <see cref="Contact"/>.</returns>
        public async Task<Contact> AddAsync(Contact item, ClaimsPrincipal user)
        {
            var result = await _apiClient.PostAsJsonAsync(ApiContacts, item);
            return await result.Content.ReadFromJsonAsync<Contact>();
        }

        /// <summary>
        /// Create a new <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <returns>The <see cref="WasmUnitOfWork"/> implementation.</returns>
        public IUnitOfWork CreateUnitOfWork(ClaimsPrincipal user)
        {
            return new WasmUnitOfWork(this);
        }

        /// <summary>
        /// Delete a <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/>.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param
        /// <returns><c>True</c> when successfully deleted.</returns>
        public async Task<bool> DeleteAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                await _apiClient.DeleteAsync($"{ApiContacts}{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load a <see cref="Contact"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Contact> LoadAsync(int id)
        {
            try
            {
                return _apiClient.GetFromJsonAsync<Contact>($"{ApiContacts}{id}");
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// Load a <see cref="Contact"/> for updates, with a <see cref="IUnitOfWork"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/> to load.</param>
        /// <param name="unitOfWork">The <see cref="IUnitOfWork"/>.</param>
        /// <returns></returns>
        public async Task<Contact> LoadAsync(int id, IUnitOfWork unitOfWork)
        {
            var result = await _apiClient.GetFromJsonAsync<ContactConcurrencyResolver>
                ($"{ApiContacts}{id}{ForUpdate}");

            // get a typed instance to work with
            var wasmUnitOfWork = unitOfWork.Resolve();

            // our instance
            wasmUnitOfWork.OriginalContact = result.OriginalContact;

            // save the version
            wasmUnitOfWork.RowVersion = result.RowVersion;
            return result.OriginalContact;
        }

        /// <summary>
        /// Gets a page of <see cref="Contact"/> items.
        /// </summary>
        /// <returns>The result <see cref="ICollection{Contact}"/>.</returns>
        public async Task<ICollection<Contact>> GetListAsync()
        {
            var result = await _apiClient.PostAsJsonAsync(
                ApiQuery, _controls);
            var queryInfo = await result.Content.ReadFromJsonAsync<QueryResult>();

            // transfer page information
            _controls.PageHelper.Refresh(queryInfo.PageInfo);
            return queryInfo.Contacts;
        }

        /// <summary>
        /// This will throw an <exception cref="NotImplementedException">exception</exception>.
        /// </summary>
        /// <param name="item">A <see cref="Contact"/>.</param>
        /// <param name="user">The logged in <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>A <see cref="Contact"/>.</returns>
        /// 
        public Task<Contact> UpdateAsync(Contact item, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update a <see cref="Contact"/> with concurrency checks.
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to update.</param>
        /// <param name="uow">The <see cref="IUnitOfWork"/> to update from.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public async Task<Contact>
            UpdateAsync(Contact item, IUnitOfWork uow)
        {
            // send down the contact with the version we have tracked
            var unitOfWork = uow.Resolve();
            var result = await _apiClient.PutAsJsonAsync(
                $"{ApiContacts}{item.Id}",
                item.ToConcurrencyResolver(unitOfWork.Resolve()));
            if (result.IsSuccessStatusCode)
            {
                return null;
            }
            if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // concurrency issue, so extract what the updated information is
                var resolver = await
                    result.Content.ReadFromJsonAsync<ContactConcurrencyResolver>();
                unitOfWork.DatabaseContact = resolver.DatabaseContact;
                var ex = new ContactConcurrencyException(item, new Exception())
                {
                    DbContact = resolver.DatabaseContact
                };
                unitOfWork.RowVersion = resolver.RowVersion; // for override
                throw ex;
            }
            throw new HttpRequestException($"Bad status code: {result.StatusCode}");
        }

        /// <summary>
        /// Throws an <exception cref="NotImplementedException">exception</exception>.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable{Contact}"/> delegate.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task QueryAsync(Func<IQueryable<Contact>, Task> query)
        {
            throw new NotImplementedException();
        }
    }
}
