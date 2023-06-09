using IssuerVerifiableEmployee.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IssuerVerifiableEmployee.Pages;

public class EmployeeCredentialsModel : PageModel
{
    private readonly EmployeeService _employeeService;

    public string EmployeeMessage { get; set; } = "Loading credentials";
    public bool HasEmployee { get; set; }
    public Employee? Employee { get; set; }

    public EmployeeCredentialsModel(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    public async Task OnGetAsync()
    {
        Employee = await _employeeService.GetEmployee(HttpContext.User?.Identity?.Name);

        if (Employee != null)
        {
            EmployeeMessage = "Add your employee credentials to your wallet";
            HasEmployee = true;
        }
        else
        {
            EmployeeMessage = "You have no valid employee";
        }
    }
}
