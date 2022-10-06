using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommunityJobBank.Pages
{
    public class DetailsModel : PageModel
    {
        public int job_id { get; set; }

        public void OnGet(string sid)
        {
            job_id = Convert.ToInt32(sid);
        }
    }
}
