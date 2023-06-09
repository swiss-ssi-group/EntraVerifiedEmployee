using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IssuerVerifiableEmployee.Persistence;

namespace IssuerVerifiableEmployee.Pages.Employees;

public class EditModel : PageModel
{
    private readonly EmployeeDbContext _context;

    public EditModel(EmployeeDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public UpdateEmployee? Employee { get; set; } = null;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        Employee = new UpdateEmployee
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            Name = employee.Name,
            UserName = employee.UserName,
            Valid = employee.Valid
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Employee != null)
        {
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == Employee.Id);
            
            if (existingEmployee == null)
                return NotFound();

            existingEmployee.Valid = Employee.Valid;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(Employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./User", new { id = existingEmployee.UserName });
        }

        return BadRequest("No model");
    }

    private bool EmployeeExists(Guid id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}
