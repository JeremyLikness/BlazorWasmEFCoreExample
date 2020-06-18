using System.Collections.Generic;
using System.Threading.Tasks;
using ContactsApp.Model;
using ContactsApp.DataAccess;
using ContactsApp.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContactsApp.BaseRepository;
using System;
using Microsoft.Extensions.DependencyInjection;
using ContactsApp.Controls;

namespace ContactsApp.Server.Controllers
{
    /// <summary>
    /// Controller for queries of <see cref="Contact"/>.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QueryController : ControllerBase
    {
        private readonly IBasicRepository<Contact> _repo;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="QueryController"/>.
        /// </summary>
        /// <param name="repo">The <see cref="IBasicRepository{Contact}"/> repo to use.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> for dependency resolution.</param>
        public QueryController(IBasicRepository<Contact> repo,
            IServiceProvider provider)
        {
            _repo = repo;
            _serviceProvider = provider;
        }

        /// <summary>
        /// This is a POST to take on filter information.
        /// </summary>
        /// <example>POST /api/query</example>
        /// <param name="filter">The <see cref="ContactFilter"/> to apply.</param>
        /// <returns>An <see cref="ICollection{Contact}"/>.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync(
            [FromBody] ContactFilter filter)
        {
            // is the database there?
            // NOTE: this is intended to make the sample app easy to run,
            // and will create and seed the database. This is NOT code to
            // put into production. Instead, look to migrations or another
            // method.
            var seed = _serviceProvider.GetService<SeedContacts>();
            await seed.CheckAndSeedDatabaseAsync(User);

            var adapter = new GridQueryAdapter(filter);
            ICollection<Contact> contacts = null;
            // this call both executes a count to get total items and
            // updates the paging information
            await _repo.QueryAsync(
                async query => contacts = await adapter.FetchAsync(query));
            return new OkObjectResult(new
            {
                PageInfo = filter.PageHelper,
                Contacts = contacts
            });
        }
    }
}
