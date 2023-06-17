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

        public void OnGet()
        {
        }
    }
}
