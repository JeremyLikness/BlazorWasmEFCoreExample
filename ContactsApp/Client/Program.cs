using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ContactsApp.BaseRepository;
using ContactsApp.Client.Data;
using ContactsApp.Controls.Grid;
using ContactsApp.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsApp.Client
{
    public class Program
    {
        public const string BaseClient = "ContactsApp.ServerAPI";
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>(nameof(App).ToLowerInvariant());

            builder.Services.AddHttpClient(BaseClient,
                client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp =>
            sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient(BaseClient));

            builder.Services.AddApiAuthorization();

            // client implementation
            builder.Services.AddScoped<IBasicRepository<Contact>, WasmRepository>();

            // references to control filters and sorts
            builder.Services.AddScoped<GridControls>();
            builder.Services.AddScoped<IContactFilters>(sp => 
            sp.GetService<GridControls>());
            builder.Services.AddScoped(sp =>
                new ClaimsPrincipal(new ClaimsIdentity()));

            await builder.Build().RunAsync();
        }
    }
}
