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
        
        var employeeData = await _microsoftGraphDelegatedClient
            .GetEmployee(oid!.Value);

        if (employeeData.Employee != null)
        {
            Employee = new Employee
            {
                DisplayName = employeeData.Employee.DisplayName!,
                GivenName = employeeData.Employee.GivenName!,
                JobTitle = employeeData.Employee.JobTitle!,
                Surname = employeeData.Employee.Surname!,
                PreferredLanguage = employeeData.Employee.PreferredLanguage!,
                Mail = employeeData.Employee.Mail!,
                RevocationId = employeeData.Employee.RevocationId!,
                Photo = employeeData.Employee.Photo,
                AccountEnabled = employeeData.Employee.AccountEnabled
            };
            EmployeeMessage = "Add your employee credentials to your wallet";
            HasEmployee = true;
        }
        else
        {
            EmployeeMessage = $"You have no valid employee, Error: {employeeData.Error}";
        }
    }
}
