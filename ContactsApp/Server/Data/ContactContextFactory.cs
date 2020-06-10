using ContactsApp.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace ContactsApp.Server.Data
{
    /// <summary>
    /// Factory to enable running migrations from the command line
    /// </summary>
    public class ContactContextFactory : IDesignTimeDbContextFactory<ContactContext>
    {
        public ContactContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment")
                ?? "Development";
            var basePath = AppContext.BaseDirectory;

            // grab connection string
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString(ContactContext.BlazorContactsDb);
            var optionsBuilder = new DbContextOptionsBuilder<ContactContext>();
            
            // use SQL Server and place migrations in this assembly
            optionsBuilder.UseSqlServer(connstr, builder =>
            builder.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
            return new ContactContext(optionsBuilder.Options);
        }
    }
}
