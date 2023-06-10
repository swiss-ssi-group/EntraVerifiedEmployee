using IssuerVerifiableEmployee.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IssuerVerifiableEmployee.Pages.Employees;

public class CreateModel : PageModel
{
    private readonly EmployeeDbContext _context;

    [FromQuery(Name = "id")]
    public string? Id { get; set; }

    public string? UserName { get; set; }

    [BindProperty]
    public Employee Employee { get; set; } = new Employee();

    public CreateModel(EmployeeDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet(string id)
    {
        Employee.DisplayName = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Employee.Issuedby = HttpContext.User?.Identity?.Name;
        Employee.IssuedAt = DateTimeOffset.UtcNow;

        _context.Employees.Add(Employee);
        await _context.SaveChangesAsync();

        return RedirectToPage("./User", new { id = Employee.DisplayName });
    }
}
