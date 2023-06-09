using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IssuerVerifiableEmployee.Persistence;

namespace IssuerVerifiableEmployee;

public class AdminModel : PageModel
{
    private readonly EmployeeDbContext _context;

    public AdminModel(EmployeeDbContext context)
    {
        _context = context;
    }

    public List<Employee> Employees { get; set; } = new List<Employee>();

    public async Task OnGetAsync()
    {
        Employees = await _context.Employees.ToListAsync();
    }
}