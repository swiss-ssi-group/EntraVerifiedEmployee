using BffMicrosoftEntraID.Server;
using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace IssuerVerifiableEmployee;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        services.Configure<CredentialSettings>(Configuration.GetSection("CredentialSettings"));
        services.AddScoped<MicrosoftGraphDelegatedClient>();
        services.AddScoped<IssuerService>();

        services.AddDistributedMemoryCache();

        var scopes = new string[] { "user.read" };
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi(scopes)
            .AddMicrosoftGraph()
            .AddDistributedTokenCaches();

        // If using downstream APIs and in memory cache, you need to reset the cookie session if the cache is missing
        // If you use persistent cache, you do not require this.
        // You can also return the 403 with the required scopes, this needs special handling for ajax calls
        // The check is only for single scopes
        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme,
            options => options.Events = new RejectSessionCookieWhenAccountNotInCacheEvents(scopes));

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => false;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddRazorPages()
            .AddMvcOptions(options => { })
            .AddMicrosoftIdentityUI();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSecurityHeaders(SecurityHeadersDefinitions
           .GetHeaderPolicyCollection(env.IsDevelopment()));


        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
        });
    }
}
