namespace IssuerVerifiableEmployee.Pages.Employees;

public class UpdateEmployee
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public bool Valid { get; set; }
}
