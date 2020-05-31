using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContactsApp.Server.Data;
using ContactsApp.Server.Models;
using ContactsApp.DataAccess;
using ContactsApp.Repository;
using ContactsApp.Model;
using ContactsApp.BaseRepository;

namespace ContactsApp.Server
{
    public class Startup
    {
        private static readonly string DefaultConnection 
            = nameof(DefaultConnection);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationAuditDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString(DefaultConnection)));

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationAuditDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationAuditDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddDbContextFactory<ContactContext>(opt =>
                opt.UseSqlServer(
                    Configuration.GetConnectionString(ContactContext.BlazorContactsDb))
                .EnableSensitiveDataLogging());

            // add the repository
            services.AddScoped<IRepository<Contact, ContactContext>, 
                ContactRepository>();
            services.AddScoped<IBasicRepository<Contact>>(sp =>
                sp.GetService<IRepository<Contact, ContactContext>>());
            services.AddScoped<IUnitOfWork<Contact>, UnitOfWork<ContactContext, Contact>>();

            // for seeding the first time
            services.AddScoped<SeedContacts>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
