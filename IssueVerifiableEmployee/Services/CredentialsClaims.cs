using System.Text.Json.Serialization;

namespace IssuerVerifiableEmployee.Services;

/// <summary>
/// Verified Employee scheme
/// </summary>
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
