using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IssuerVerifiableEmployee.Persistence;
using Microsoft.Graph;
using IssuerVerifiableEmployee.Services.GraphServices;

namespace IssuerVerifiableEmployee;

public class AdminModel : PageModel
{
    private readonly MicrosoftGraphDelegatedClient _microsoftGraphDelegatedClient;

    public AdminModel(MicrosoftGraphDelegatedClient microsoftGraphDelegatedClient)
    {
        _microsoftGraphDelegatedClient = microsoftGraphDelegatedClient;
    }

    public List<Employee> Employees { get; set; } = new List<Employee>();

    public async Task OnGetAsync()
    {
        //Employees = await _microsoftGraphDelegatedClient.GetUsers().ToListAsync();
    }
}