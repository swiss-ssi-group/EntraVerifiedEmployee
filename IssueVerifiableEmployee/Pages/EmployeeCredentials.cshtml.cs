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
        var oid = User.Claims.FirstOrDefault(t => t.Type == 
            "http://schemas.microsoft.com/identity/claims/objectidentifier");
        
        var userData = await _microsoftGraphDelegatedClient
            .GetGraphApiUser(oid!.Value);

        if (userData.User != null && userData.Photo != null)
        {
            Employee = new Employee
            {
                DisplayName = userData.User.DisplayName,
                GivenName = userData.User.GivenName,
                JobTitle = userData.User.JobTitle,
                Surname = userData.User.Surname,
                PreferredLanguage = userData.User.PreferredLanguage,
                Mail = userData.User.Mail,
                RevocationId = userData.User.UserPrincipalName,
                Photo = userData.Photo,
                AccountEnabled = userData.User.AccountEnabled.GetValueOrDefault()
            };
            EmployeeMessage = "Add your employee credentials to your wallet";
            HasEmployee = true;
        }
        else
        {
            EmployeeMessage = $"You have no valid employee, Error: {userData.Error}";
        }
    }
}
