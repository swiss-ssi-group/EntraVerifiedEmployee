using IssuerVerifiableEmployee.Persistence;
using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;

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
        var user = await _microsoftGraphDelegatedClient
            .GetGraphApiUser(HttpContext.User?.Identity?.Name);

        if (user != null)
        {
            Employee = new Employee
            {
                DisplayName = user.DisplayName,
                GivenName = user.GivenName,
                JobTitle = user.JobTitle,
                Surname = user.Surname,
                PreferredLanguage = user.PreferredLanguage,
                Valid = user.AccountEnabled.GetValueOrDefault(),
                Mail = user.Mail,
                RevocationId = user.UserPrincipalName
            };
            EmployeeMessage = "Add your employee credentials to your wallet";
            HasEmployee = true;
        }
        else
        {
            EmployeeMessage = "You have no valid employee";
        }
    }
}
