using IssuerVerifiableEmployee.Persistence;
using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IssuerVerifiableEmployee.Pages;

public class EmployeeCredentialsModel : PageModel
{
    private readonly MicrosoftGraphDelegatedClient _microsoftGraphDelegatedClient;

    public string EmployeeMessage { get; set; } = "Loading credentials";
    public bool HasEmployee { get; set; }
    public Employee? Employee { get; set; }

    public EmployeeCredentialsModel(MicrosoftGraphDelegatedClient microsoftGraphDelegatedClient)
    {
        _microsoftGraphDelegatedClient = microsoftGraphDelegatedClient;
    }

    public async Task OnGetAsync()
    {
        var oid = User.Claims.FirstOrDefault(t => t.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier");
        var userData = await _microsoftGraphDelegatedClient
            .GetGraphApiUser(oid!.Value);

        var user = userData.User;

        if (userData.User != null && user != null && userData.Photo != null)
        {
            Employee = new Employee
            {
                DisplayName = user.DisplayName,
                GivenName = user.GivenName,
                JobTitle = user.JobTitle,
                Surname = user.Surname,
                PreferredLanguage = user.PreferredLanguage,
                Mail = user.Mail,
                RevocationId = user.UserPrincipalName,
                Photo = userData.Photo,
                AccountEnabled = user.AccountEnabled.GetValueOrDefault()
            };
            EmployeeMessage = "Add your employee credentials to your wallet";
            HasEmployee = true;
        }
        else
        {
            EmployeeMessage = $"You have no valid employee, userData.Photo: {userData.Photo == null}";
        }
    }
}
