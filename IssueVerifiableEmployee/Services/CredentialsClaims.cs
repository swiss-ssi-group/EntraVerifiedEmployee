using System.Text.Json.Serialization;

namespace IssuerVerifiableEmployee.Services;

/// <summary>
/// Application specific claims used in the payload of the issue request. 
/// When using the id_token for the subject claims, the IDP needs to add the values to the id_token!
/// The claims can be mapped to anything then.
/// </summary>
public class CredentialsClaims
{
    /// <summary>
    /// attribute names need to match a claim from the id_token
    /// </summary>
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
    [JsonPropertyName("revocationId")]
    public string RevocationId { get; set; } = string.Empty;
}
