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
            var oid = User.Claims.FirstOrDefault(t => t.Type ==
            "http://schemas.microsoft.com/identity/claims/objectidentifier");

            var employeeData = await _microsoftGraphDelegatedClient
                .GetEmployee(oid!.Value);

            if(employeeData.Employee!.PreferredLanguage != null)
            {
                PreferredLanguage = employeeData.Employee.PreferredLanguage;
            }
        }

        public async Task OnPostAsync() 
        {
            var oid = User.Claims.FirstOrDefault(t => t.Type ==
            "http://schemas.microsoft.com/identity/claims/objectidentifier");

            await _microsoftGraphDelegatedClient
                .SetPreferredLanguage(oid!.Value, PreferredLanguage);
        }
    }
}
