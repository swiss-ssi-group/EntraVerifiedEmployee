using IssuerVerifiableEmployee.Services;
using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
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
        
        var (User, Photo, Error) = await _microsoftGraphDelegatedClient
            .GetGraphApiUser(oid!.Value);

        if (User != null && Photo != null)
        {
            payload.Claims.GivenName = User.GivenName;
            payload.Claims.Surname = User.Surname;
            payload.Claims.Mail = User.Mail;
            payload.Claims.JobTitle = User.JobTitle;
            payload.Claims.Photo = Photo;
            payload.Claims.DisplayName = User.DisplayName;
            payload.Claims.PreferredLanguage = User.PreferredLanguage;
            payload.Claims.RevocationId = User.UserPrincipalName;

            return payload;
        }

        throw new ArgumentNullException(nameof(User));
    }

    public async Task<(string Token, string Error, string ErrorDescription)> GetAccessToken()
    {
        var isUsingClientSecret = _credentialSettings.AppUsesClientSecret(_credentialSettings);

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

        // With client credentials flows the scopes is ALWAYS of the shape "resource/.default"
        var scopes = new string[] { _credentialSettings.VCServiceScope };

        AuthenticationResult? result;
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
