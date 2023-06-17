using IssuerVerifiableEmployee.Services.GraphServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IssueVerifiableEmployee.Pages
{
    public class SetPreferredLanguageModel : PageModel
    {
        private readonly MicrosoftGraphDelegatedClient _microsoftGraphDelegatedClient;

        [BindProperty]
        public string PreferredLanguage { get; set; } = "en-US";

        public SetPreferredLanguageModel(MicrosoftGraphDelegatedClient microsoftGraphDelegatedClient)
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
                PreferredLanguage = employeeData.Employee.PreferredLanguage;
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
}
