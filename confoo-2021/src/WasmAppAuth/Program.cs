using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WasmAppAuth.Infrastructure;
using WasmAppAuth.Services;

namespace WasmAppAuth
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            //builder.Services.AddHttpClient<ApiService>(client =>
            //{
            //    client.BaseAddress = new Uri(builder.Configuration["https://localhost:5016"]);
            //}).AddHttpMessageHandler(sp => 
            //{
            //    var handler = sp.GetService<AuthorizationMessageHandler>()
            //        .ConfigureHandler(
            //            authorizedUrls: new[] { "https://localhost:5016" },
            //            scopes: new[] { "companyApi" }
            //         );
            //        return handler;
            //});

            builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5016");
            }).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                options.ProviderOptions.Authority = "https://localhost:5001";
                options.ProviderOptions.ClientId = "wasmappauth-client";
                options.ProviderOptions.ResponseType = "code";

                options.UserOptions.RoleClaim = "role";
            });

            builder.Services.AddAuthorizationCore(config =>
            {
                config.AddPolicy("delete-access",
                    new AuthorizationPolicyBuilder().
                        RequireAuthenticatedUser().
                        RequireRole("Admin").
                        Build()
                    );
            });

            await builder.Build().RunAsync();
        }
    }
}
