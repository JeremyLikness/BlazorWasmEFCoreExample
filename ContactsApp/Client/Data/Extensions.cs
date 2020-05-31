using ContactsApp.BaseRepository;
using ContactsApp.Model;

namespace ContactsApp.Client.Data
{
    /// <summary>
    /// I get by with a little help from my friends.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Transfers the new page information over.
        /// </summary>
        /// <param name="helper">The <see cref="PageHelper"/> to use.</param>
        /// <param name="newData">The new data to transfer.</param>
        public static void Refresh(this IPageHelper helper, IPageHelper newData)
        {
            helper.PageSize = newData.PageSize;
            helper.PageItems = newData.PageItems;
            helper.Page = newData.Page;
            helper.TotalItemCount = newData.TotalItemCount;
        }

        /// <summary>
        /// Helper to transfer concurrency information from the repo
        /// to the data object.
        /// </summary>
        /// <param name="contact">The <see cref="Contact"/> being resolved.</param>
        /// <param name="repo">The <see cref="WasmRepository"/> holding the concurrency values.</param>
        /// <returns>The <see cref="ContactConcurrencyResolver"/> instance.</returns>
        public static ContactConcurrencyResolver ToConcurrencyResolver(
            this Contact contact, WasmRepository repo)
        {
            return new ContactConcurrencyResolver()
            {
                OriginalContact = contact,
                RowVersion = repo.RowVersion
            };
        }
    }
}
