using IssuerVerifiableEmployee.Persistence;
using IssuerVerifiableEmployee.Services;
using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Globalization;
using System.Security.Cryptography;

namespace IssuerVerifiableEmployee;

public class IssuerService
{
    protected readonly CredentialSettings _credentialSettings;
    protected IMemoryCache _cache;
    protected readonly ILogger<IssuerService> _log;
    private readonly MicrosoftGraphDelegatedClient _microsoftGraphDelegatedClient;

    public IssuerService(IOptions<CredentialSettings> credentialSettings,
        MicrosoftGraphDelegatedClient microsoftGraphDelegatedClient,
        IMemoryCache memoryCache,
        ILogger<IssuerService> log)
    {
        _credentialSettings = credentialSettings.Value;
        _credentialSettings ??= new CredentialSettings();

        _cache = memoryCache;
        _log = log;
        _microsoftGraphDelegatedClient = microsoftGraphDelegatedClient;
    }

    public async Task<IssuanceRequestPayload> GetIssuanceRequestPayloadAsync(HttpRequest request)
    {
        var payload = new IssuanceRequestPayload();

        var length = 4;
        var pinMaxValue = (int)Math.Pow(10, length) - 1;
        var randomNumber = RandomNumberGenerator.GetInt32(1, pinMaxValue);
        var newpin = string.Format(CultureInfo.InvariantCulture,
            "{0:D" + length.ToString(CultureInfo.InvariantCulture) + "}", randomNumber);

        payload.Pin.Length = length;
        payload.Pin.Value = newpin;
  
        payload.CredentialsType = "VerifiedEmployee";

        //get the manifest from the appsettings, this is the URL to the Verified Employee credential created in the azure portal. 
        //the ? parameter is needed for the myaccount page to work with the Verified Employee credential. This will force the system
        //to use accept an idtokenhint payload and ignore the accesstoken flow which is the default for employment credentials
        payload.Manifest = $"{_credentialSettings.CredentialManifest}{"?manifestType=claimInjection"}";

        var host = GetRequestHostName(request);
        payload.Callback.State = Guid.NewGuid().ToString();
        payload.Callback.Url = $"{host}/api/issuer/issuanceCallback";
        payload.Callback.Headers.ApiKey = _credentialSettings.VcApiCallbackApiKey;

        payload.Registration.ClientName = "Verifiable Credential Employee";
        payload.Authority = _credentialSettings.IssuerAuthority;

        var oid = request.HttpContext.User.Claims.FirstOrDefault(t => t.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier");
        
        var userData = await _microsoftGraphDelegatedClient
            .GetGraphApiUser(oid!.Value);

        var user = userData.User;

        if (userData.User != null && user != null && userData.Photo != null)
        {
            var employee = new Employee
            {
                GivenName = user.GivenName,
                Surname = user.Surname,
                Mail = user.Mail,
                JobTitle = user.JobTitle,
                //Photo = user.Photo,
                DisplayName = user.DisplayName,
                PreferredLanguage = user.PreferredLanguage,
                RevocationId = user.UserPrincipalName,
                AccountEnabled = user.AccountEnabled.GetValueOrDefault(),
            };

            payload.Claims.GivenName = employee.GivenName;
            payload.Claims.Surname = employee.Surname;
            payload.Claims.Mail = employee.Mail;
            payload.Claims.JobTitle = employee.JobTitle;
            payload.Claims.Photo = userData.Photo;
            payload.Claims.DisplayName = employee.DisplayName;
            payload.Claims.PreferredLanguage = employee.PreferredLanguage;
            payload.Claims.RevocationId = employee.RevocationId;

            return payload;
        }

        throw new ArgumentNullException(nameof(user));
    }

    public async Task<(string Token, string Error, string ErrorDescription)> GetAccessToken()
    {
        // You can run this sample using ClientSecret or Certificate. The code will differ only when instantiating the IConfidentialClientApplication
        var isUsingClientSecret = _credentialSettings.AppUsesClientSecret(_credentialSettings);

        // Since we are using application permissions this will be a confidential client application
        IConfidentialClientApplication app;
        if (isUsingClientSecret)
        {
            app = ConfidentialClientApplicationBuilder.Create(_credentialSettings.ClientId)
                .WithClientSecret(_credentialSettings.ClientSecret)
                .WithAuthority(new Uri(_credentialSettings.Authority))
                .Build();
        }
        else
        {
            var certificate = _credentialSettings.ReadCertificate(_credentialSettings.CertificateName);
            app = ConfidentialClientApplicationBuilder.Create(_credentialSettings.ClientId)
                .WithCertificate(certificate)
                .WithAuthority(new Uri(_credentialSettings.Authority))
                .Build();
        }

        //configure in memory cache for the access tokens. The tokens are typically valid for 60 seconds,
        //so no need to create new ones for every web request
        app.AddDistributedTokenCache(services =>
        {
            services.AddDistributedMemoryCache();
            services.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
        });

        // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
        // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
        // a tenant administrator. 
        var scopes = new string[] { _credentialSettings.VCServiceScope };

        AuthenticationResult? result = null;
        try
        {
            result = await app.AcquireTokenForClient(scopes)
                .ExecuteAsync();
        }
        catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
        {
            // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
            // Mitigation: change the scope to be as expected
            return (string.Empty, "500", "Scope provided is not supported");
            //return BadRequest(new { error = "500", error_description = "Scope provided is not supported" });
        }
        catch (MsalServiceException ex)
        {
            // general error getting an access token
            return (string.Empty, "500", "Something went wrong getting an access token for the client API:" + ex.Message);
            //return BadRequest(new { error = "500", error_description = "Something went wrong getting an access token for the client API:" + ex.Message });
        }

        _log.LogTrace("{AccessToken}", result.AccessToken);
        return (result.AccessToken, string.Empty, string.Empty);
    }

    public string GetRequestHostName(HttpRequest request)
    {
        var scheme = "https";// : this.Request.Scheme;
        var originalHost = request.Headers["x-original-host"];
        if (!string.IsNullOrEmpty(originalHost))
        {
            return $"{scheme}://{originalHost}";
        }
        else
        {
            return $"{scheme}://{request.Host}";
        }
    }
}
