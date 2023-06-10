using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IssuerVerifiableEmployee.Persistence;

namespace IssuerVerifiableEmployee.Pages.Employees;

public class UserModel : PageModel
{
    private readonly EmployeeDbContext _context;

    [FromQuery(Name = "id")]
    public string? UserName { get; set; }

    public UserModel(EmployeeDbContext context)
    {
        _context = context;
    }

    public IList<Employee> Employee { get; set; } = new List<Employee>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (id == null)
        {
            return NotFound();
        }
        UserName = id;

        Employee = await _context.Employees
            .AsQueryable()
            .Where(item => item.DisplayName == id)
            .ToListAsync();

        return Page();
    }
}
