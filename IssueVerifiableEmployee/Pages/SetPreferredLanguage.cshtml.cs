using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueVerifiableEmployee.Pages;

public class SetPreferredLanguageModel : PageModel
{
    private readonly MicrosoftGraphDelegatedClient _microsoftGraphDelegatedClient;

    public List<SelectListItem> Languages { get; set; } = new List<SelectListItem>
    {
        new SelectListItem("English", "en-US"),
        new SelectListItem("Deutsch", "de-CH"),
        new SelectListItem("Italiano", "it-CH"),
        new SelectListItem("Français", "fr-CH")
    };

    [BindProperty]
    public string PreferredLanguage { get; set; } = "en-US";

    public SetPreferredLanguageModel(MicrosoftGraphDelegatedClient microsoftGraphDelegatedClient)
    {
        _microsoftGraphDelegatedClient = microsoftGraphDelegatedClient;
    }

    public async Task OnGetAsync()
    {
        var oid = User.Claims.FirstOrDefault(t => t.Type == Consts.OID_TYPE);

        var (Employee, Error) = await _microsoftGraphDelegatedClient
            .GetEmployee(oid!.Value);

        if (Employee != null)
        {
            PreferredLanguage = Employee.PreferredLanguage;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var oid = User.Claims.FirstOrDefault(t => t.Type == Consts.OID_TYPE);

        await _microsoftGraphDelegatedClient
            .SetPreferredLanguage(oid!.Value, PreferredLanguage);

        return Page();
    }
}
