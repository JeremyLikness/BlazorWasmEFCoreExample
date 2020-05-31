using ContactsApp.Server.Models;
using System;

namespace ContactsApp.Server.Data
{
    public class UserAudit
    {
        public UserAudit()
        {
            EventTime = DateTimeOffset.UtcNow;
        }

        public UserAudit(string action, ApplicationUser user) : this()
        {
            UserId = user.Id;
            Username = user.UserName;
            Action = action;
        }
        public int Id { get; set; }

        public string UserId { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public string Action { get; set; }
        public string Username { get; set; } 
    }
}
