using ContactsApp.Model;
using System;

namespace ContactsApp.BaseRepository
{
    /// <summary>
    /// Simplify concurrency issues.
    /// </summary>
    public class ContactConcurrencyException : Exception
    {
        /// <summary>
        /// The <see cref="Contact"/> to update.
        /// </summary>
        public Contact Contact { get; private set; }

        /// <summary>
        /// The changed <see cref="Contact"/> in the database.
        /// </summary>
        public Contact DbContact { get; set; }

        /// <summary>
        /// The database row version.
        /// </summary>
        public byte[] RowVersion { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactConcurrencyException"/>
        /// class.
        /// </summary>
        /// <param name="contact">The <see cref="Contact"/> instance with a conflict.</param>
        /// <param name="ex">The exception that was thrown.</param>
        public ContactConcurrencyException(Contact contact, Exception ex) :
            base("A concurrency issue was detected", ex)
        {
            Contact = contact;
        }
    }
}
