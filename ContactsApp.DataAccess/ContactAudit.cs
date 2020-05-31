using ContactsApp.Model;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsApp.DataAccess
{
    /// <summary>
    /// Audit for <see cref="Contact"/>.
    /// </summary>
    public class ContactAudit
    {
        /// <summary>
        /// Audit key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Time of audit.
        /// </summary>
        public DateTimeOffset EventTime { get; set; }
            = DateTimeOffset.UtcNow;

        /// <summary>
        /// Id of the <see cref="Contact"/> being audited.
        /// </summary>
        public int ContactId { get; set; }

        /// <summary>
        /// Id of the user who made the change.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// What happened?
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// JSON serialized snapshot of before/after
        /// </summary>
        public string Changes { get; set; }

        /// <summary>
        /// Makes it easier to track updated ids.
        /// </summary>
        [NotMapped]
        public Contact ContactRef { get; set; }
    }
}
