using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IContactFilters _controls;

        private const string ApiPrefix = "/api/";
        private string ApiContacts => $"{ApiPrefix}contacts/";
        private string ApiQuery => $"{ApiPrefix}query/";
        private string ForUpdate => "?forUpdate=true";

        /// <summary>
        /// Contact as loaded then modified by the user.
        /// </summary>
        public Contact OriginalContact { get; set; }

        /// <summary>
        /// Contact on the database.
        /// </summary>
        public Contact DatabaseContact { get; set; }

        /// <summary>
        /// The row version of the last contact loaded.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// This will serialize a response from JSON and return null
        /// if the status code is 404 - not found.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<TEntity> SafeGetFromJsonAsync<TEntity>(string url)
        {
            var result = await _apiClient.GetAsync(url);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadFromJsonAsync<TEntity>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WasmRepository"/>.
        /// </summary>
        /// <param name="clientFactory">The <see cref="IHttpClientFactory"/> for communication with the server.</param>
        /// <param name="controls">The <see cref="ContactFilters"/> to parse queries and filters.</param>
        public WasmRepository(IHttpClientFactory clientFactory, IContactFilters controls)
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
        /// <param name="id">The id of the <see cref="Contact"/> to load.</param>
        /// <param name="_">Unused <see cref="ClaimsPrincipal"/>.</param>
        /// <param name="forUpdate"><c>True</c> when concurrency information should be loaded.</param>
        /// <returns></returns>
        public Task<Contact> LoadAsync(
            int id, 
            ClaimsPrincipal _, 
            bool forUpdate = false)
        {
            if (forUpdate)
            {
                return LoadAsync(id);
            }
            return SafeGetFromJsonAsync<Contact>($"{ApiContacts}{id}");
        }

        /// <summary>
        /// Load a <see cref="Contact"/> for updates.
        /// </summary>
        /// <param name="id">The id of the <see cref="Contact"/> to load.</param>
        /// <returns></returns>
        public async Task<Contact> LoadAsync(int id)
        {
            OriginalContact = null;
            DatabaseContact = null;
            RowVersion = null;

            var result = await SafeGetFromJsonAsync<ContactConcurrencyResolver>
                    ($"{ApiContacts}{id}{ForUpdate}");

            if (result == null)
            {
                return null;
            }

            // our instance
            OriginalContact = result.OriginalContact;

            // save the version
            RowVersion = result.RowVersion;

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
        /// Update a <see cref="Contact"/> with concurrency checks.
        /// </summary>
        /// <param name="item">The <see cref="Contact"/> to update.</param>
        /// <param name="user">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public async Task<Contact>
            UpdateAsync(Contact item, ClaimsPrincipal user)
        {
            // send down the contact with the version we have tracked
            var result = await _apiClient.PutAsJsonAsync(
                $"{ApiContacts}{item.Id}",
                item.ToConcurrencyResolver(this));
            if (result.IsSuccessStatusCode)
            {
                return null;
            }
            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                // concurrency issue, so extract what the updated information is
                var resolver = await
                    result.Content.ReadFromJsonAsync<ContactConcurrencyResolver>();
                DatabaseContact = resolver.DatabaseContact;
                var ex = new RepoConcurrencyException<Contact>(item, new Exception())
                {
                    DbEntity = resolver.DatabaseContact
                };
                RowVersion = resolver.RowVersion; // for override
                throw ex;
            }
            throw new HttpRequestException($"Bad status code: {result.StatusCode}");
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="item">Don't care.</param>
        public void Attach(Contact item)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Throws an <exception cref="NotImplementedException">exception</exception>.
        /// </summary>
        /// <param name="query">The <see cref="IQueryable{Contact}"/> delegate.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task QueryAsync(Func<IQueryable<Contact>, Task> query)
        {
            return GetListAsync();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Task<TPropertyType> GetPropertyValueAsync<TPropertyType>(Contact item, string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task SetOriginalValueForConcurrencyAsync<TPropertyType>(Contact item, string propertyName, TPropertyType value)
        {
            throw new NotImplementedException();
        }
    }
}
