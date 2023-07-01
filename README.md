# Issue Microsoft Entra Verified Employee credential

[![.NET](https://github.com/swiss-ssi-group/AzureADVerifiableEmployee/actions/workflows/dotnet.yml/badge.svg)](https://github.com/swiss-ssi-group/AzureADVerifiableEmployee/actions/workflows/dotnet.yml)

[Issue Employee verifiable credentials using Entra Verified ID and ASP.NET Core](https://damienbod.com)

### Local debugging, required for callback

```
ngrok http https://localhost:5001
```

### Verified Employee scheme

```csharp
public class CredentialsClaims
{
    [JsonPropertyName("givenName")]
    public string GivenName { get; set; } = string.Empty;
    [JsonPropertyName("surname")]
    public string Surname { get; set; } = string.Empty;
    [JsonPropertyName("mail")]
    public string Mail { get; set; } = string.Empty;
    [JsonPropertyName("jobTitle")]
    public string JobTitle { get; set; } = string.Empty;
    [JsonPropertyName("photo")] // "type": "image/jpg;base64url",
    public string Photo { get; set; } = string.Empty;
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("preferredLanguage")]
    public string PreferredLanguage { get; set; } = string.Empty;
    [JsonPropertyName("userPrincipalName")]
    public string RevocationId { get; set; } = string.Empty;
}
```

### Update the preferredLanguage

PATCH 

https://graph.microsoft.com/v1.0/users/{oid}
```
{
    "preferredLanguage": "de-CH",
    "jobTitle": "HR"
}
```

### Photo with a license

https://portal.office.com/account/?ref=MeControl#personalinfo

https://learn.microsoft.com/en-us/graph/api/profilephoto-update?view=graph-rest-1.0&tabs=http

https://graph.microsoft.com/v1.0/users/{oid}/photo

### Photo

You can update the profile photo in the Azure AD portal without a license in the users basic profile settings.

## Links

https://learn.microsoft.com/en-us/azure/active-directory/verifiable-credentials/how-to-use-quickstart-multiple

https://github.com/swiss-ssi-group/AzureADVerifiableCredentialsAspNetCore

https://learn.microsoft.com/en-us/azure/active-directory/verifiable-credentials/decentralized-identifier-overview

https://ssi-start.adnovum.com/data

https://github.com/e-id-admin/public-sandbox-trustinfrastructure/discussions/14

https://openid.net/specs/openid-connect-self-issued-v2-1_0.html

https://identity.foundation/jwt-vc-presentation-profile/

https://learn.microsoft.com/en-us/azure/active-directory/verifiable-credentials/verifiable-credentials-standards

https://github.com/Azure-Samples/active-directory-verifiable-credentials-dotnet

https://aka.ms/mysecurityinfo

https://fontawesome.com/

https://developer.microsoft.com/en-us/graph/graph-explorer?tenant=damienbodsharepoint.onmicrosoft.com

https://learn.microsoft.com/en-us/graph/api/overview?view=graph-rest-1.0

https://github.com/Azure-Samples/VerifiedEmployeeIssuance

https://github.com/AzureAD/microsoft-identity-web/blob/jmprieur/Graph5/src/Microsoft.Identity.Web.GraphServiceClient/Readme.md#replace-the-nuget-packages

https://docs.microsoft.com/azure/app-service/deploy-github-actions#configure-the-github-secret

https://issueverifiableemployee.azurewebsites.net/
