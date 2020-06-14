using ContactsApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContactsApp.DataAccess
{
    /// <summary>
    /// This class is a helper class to audit <see cref="Contact"/> instances.
    /// </summary>
    public class ContactAuditAdapter
    {
        private static readonly string Unknown = nameof(Unknown);
        /// <summary>
        /// Marks user and timestamp information on entities and generates
        /// the audit log.
        /// </summary>
        /// <param name="currentUser">The <see cref="ClaimsPrincipal"/> logged in.</param>
        /// <param name="context">The <see cref="ContactContext"/> to use.</param>
        /// <param name="saveChangesAsync">A delegate to save the changes.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task<int> ProcessContactChangesAsync(
            ClaimsPrincipal currentUser,
            ContactContext context,
            Func<Task<int>> saveChangesAsync)
        {
            var user = Unknown;

            // grab user identifier
            if (currentUser != null)
            {
                var name = currentUser.Claims.FirstOrDefault(
                    c => c.Type == ClaimTypes.NameIdentifier);

                if (name != null)
                {
                    user = name.Value;
                }
                else if (!string.IsNullOrWhiteSpace(currentUser.Identity.Name))
                {
                    user = currentUser.Identity.Name;
                }
            }

            var audits = new List<ContactAudit>();

            // audit contacts
            foreach (var item in context.ChangeTracker.Entries<Contact>())
            {
                if (item.State == EntityState.Modified ||
                    item.State == EntityState.Added ||
                    item.State == EntityState.Deleted)
                {
                    // set created information for new item
                    if (item.State == EntityState.Added)
                    {
                        item.Property<string>(ContactContext.CreatedBy).CurrentValue =
                            user;
                        item.Property<DateTimeOffset>(ContactContext.CreatedOn).CurrentValue =
                            DateTimeOffset.UtcNow;
                    }

                    Contact dbVal = null;

                    // set modified information for modified item
                    if (item.State == EntityState.Modified)
                    {
                        var db = await item.GetDatabaseValuesAsync();
                        dbVal = db.ToObject() as Contact;
                        item.Property<string>(ContactContext.ModifiedBy).CurrentValue =
                            user;
                        item.Property<DateTimeOffset>(ContactContext.ModifiedOn).CurrentValue =
                          DateTimeOffset.UtcNow;
                    }

                    // parse the changes
                    var changes = new PropertyChanges<Contact>(item, dbVal);
                    var audit = new ContactAudit
                    {
                        ContactId = item.Entity.Id,
                        Action = item.State.ToString(),
                        User = user,
                        Changes = JsonSerializer.Serialize(changes),
                        ContactRef = item.Entity
                    };

                    audits.Add(audit);
                }
            }

            if (audits.Count > 0)
            {
                // save
                context.ContactAudits.AddRange(audits);
            }

            var result = await saveChangesAsync();

            // need a second round to update newly generated keys
            var secondSave = false;

            // attach ids for add operations
            foreach (var audit in audits.Where(a => a.ContactId == 0).ToList())
            {
                secondSave = true;
                audit.ContactId = audit.ContactRef.Id;
                context.Entry(audit).State = EntityState.Modified;
            }

            if (secondSave)
            {
                await saveChangesAsync();
            }

            return result;
        }
    }
}
