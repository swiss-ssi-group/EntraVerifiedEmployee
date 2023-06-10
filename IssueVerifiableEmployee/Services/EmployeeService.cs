using IssuerVerifiableEmployee.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IssuerVerifiableEmployee;

public class EmployeeService
{
    private readonly EmployeeDbContext _employeeDbContext;

    public EmployeeService(EmployeeDbContext employeeDbContext)
    {
        _employeeDbContext = employeeDbContext;
    }

    public async Task<bool> HasIdentityEmployee(string username)
    {
        if (!string.IsNullOrEmpty(username))
        {
            var employee = await _employeeDbContext.Employees.FirstOrDefaultAsync(
                dl => dl.DisplayName == username && dl.Valid == true
            );

            if (employee != null)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<Employee?> GetEmployee(string? username)
    {
        var employee = await _employeeDbContext.Employees.FirstOrDefaultAsync(
            dl => dl.DisplayName == username && dl.Valid == true
        );

        return employee;
    }

    public async Task UpdateEmployee(Employee employee)
    {
        _employeeDbContext.Employees.Update(employee);
        await _employeeDbContext.SaveChangesAsync();
    }
}
