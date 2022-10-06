using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace CommunityJobBank.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string p)
        {
            if (!p.IsNullOrEmpty() && p.Length == 7)
            {
                HttpContext.Session.SetString("office", p);
            }
        }

        public void OnPost()
        {

        }
    }
}