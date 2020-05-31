using ContactsApp.Model;

namespace ContactsApp.Client.Data
{
    /// <summary>
    /// This class helps track concurrency issues for client/server
    /// scenarios. 
    /// </summary>
    public class ContactConcurrencyResolver
    {
        /// <summary>
        /// The latest database version.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// The <see cref="Contact"/> being updated.
        /// </summary>
        public Contact OriginalContact { get; set; }

        /// <summary>
        /// The <see cref="Contact"/> as it exists in the database.
        /// </summary>
        public Contact DatabaseContact { get; set; }
    }
}
