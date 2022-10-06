using Microsoft.AspNetCore.Mvc;

namespace CommunityJobBank.Models
{
    public class SearchCriteria
    {
        //Basic search fields
        [BindProperty]
        public string se_position { get; set; } = ""; //user entered string
        [BindProperty]
        public string se_location { get; set; } = ""; //geoarea or provincecity (additional values for advanced search: virtual and travelling
        [BindProperty]
        public string se_province { get; set; } = ""; //2 char prov code
        [BindProperty]
        public string se_citytown { get; set; } = ""; //user entered string
        [BindProperty]
        public string se_sortby { get; set; } = ""; //bestmatch or dateposted
        [BindProperty]
        public string se_last30days { get; set; } = ""; //will be "on" if checked. Otherwise won't have a value.
        //Additional fields in advanced search
        [BindProperty]
        public string se_employer { get; set; } = ""; //user entered string
        [BindProperty]
        public string se_hours { get; set; } = ""; //fulltime or parttime
        [BindProperty]
        public string se_term { get; set; } = ""; //permanent, temporary, casual, seasonal
        [BindProperty]
        public string se_dateposted { get; set; } = ""; //last48hours, last30days, morethan30days

        public SearchCriteria()
        {

        }
    }
}
