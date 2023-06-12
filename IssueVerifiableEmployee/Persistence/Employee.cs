using System.ComponentModel.DataAnnotations;

namespace IssuerVerifiableEmployee.Persistence;

public class Employee
{
    [Key]
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string RevocationId { get; set; } = string.Empty; // userPrincipalName
    public string Photo { get; set; } = string.Empty;

    public DateTimeOffset IssuedAt { get; set; }
    public bool AccountEnabled { get; set; }
}
