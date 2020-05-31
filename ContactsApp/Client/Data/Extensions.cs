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
        /// Simple cast from <see cref="IUnitOfWork"/> to <see cref="WasmUnitOfWork"/>
        /// implementation.
        /// </summary>
        /// <param name="unitOfWork">The <see cref="IUnitOfWork"/> to cast.</param>
        /// <returns>The <see cref="WasmUnitOfWork"/>.</returns>
        public static WasmUnitOfWork Resolve(this IUnitOfWork unitOfWork)
        {
            return unitOfWork as WasmUnitOfWork;
        }

        /// <summary>
        /// Helper to transfer concurrency information from the work unit
        /// to the data object.
        /// </summary>
        /// <param name="contact">The <see cref="Contact"/> being resolved.</param>
        /// <param name="unitOfWork">The <see cref="WasmUnitOfWork"/> that is tracking the version.</param>
        /// <returns>The <see cref="ContactConcurrencyResolver"/> instance.</returns>
        public static ContactConcurrencyResolver ToConcurrencyResolver(
            this Contact contact, WasmUnitOfWork unitOfWork)
        {
            return new ContactConcurrencyResolver()
            {
                OriginalContact = contact,
                RowVersion = unitOfWork.RowVersion
            };
        }
    }
}
