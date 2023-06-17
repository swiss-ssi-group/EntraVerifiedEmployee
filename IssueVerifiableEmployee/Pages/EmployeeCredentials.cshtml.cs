using IssuerVerifiableEmployee.Persistence;
using IssuerVerifiableEmployee.Services.GraphServices;
using IssueVerifiableEmployee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace IssuerVerifiableEmployee.Pages;

public class EmployeeCredentialsModel : PageModel
{
    private readonly MicrosoftGraphDelegatedClient _microsoftGraphDelegatedClient;

    public string EmployeeMessage { get; set; } = "Loading credentials";
    public bool HasEmployee { get; set; }
    public Employee? Employee { get; set; }

    [BindProperty]
    public byte[]? Photo { get; set; }

    public EmployeeCredentialsModel(MicrosoftGraphDelegatedClient microsoftGraphDelegatedClient)
    {
        _microsoftGraphDelegatedClient = microsoftGraphDelegatedClient;
    }

    public async Task OnGetAsync()
    {
        var oid = User.Claims.FirstOrDefault(t => t.Type == Consts.OID_TYPE);
        
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
            Photo = Base64UrlEncoder.DecodeBytes(Employee.Photo);
        }
        else
        {
            EmployeeMessage = $"You have no valid employee, Error: {employeeData.Error}";
        }
    }
}
