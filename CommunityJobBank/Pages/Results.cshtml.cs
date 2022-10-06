using CommunityJobBank.Data;
using CommunityJobBank.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace CommunityJobBank.Pages
{
    public class ResultsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private DBStore _dbStore;
        private string office = "";
        public List<JobsInfoItem>? jobs;
        public string? searchType { get; set; }
        [BindProperty]
        public SearchCriteria searchCriteria { get; set; }

        public ResultsModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _dbStore = new DBStore(_configuration);
        }

        public void OnGet()
        {
            if (searchCriteria is null)
            {
                searchCriteria = new SearchCriteria();
            }

            searchType = "none";
        }

        public void OnPostBrowse()
        {
            searchType = "browse";

            if (searchCriteria is null)
            {
                searchCriteria = new SearchCriteria();
            }

            office = HttpContext.Session.GetString("office") ?? "";

            jobs = _dbStore.GetAllJobInfoItemsForDefaultGeographicLocation(office);
        }

        public void OnPostSearch()
        {
            searchType = "search";

            if (searchCriteria is null)
            {
                searchCriteria = new SearchCriteria();
            }

            office = HttpContext.Session.GetString("office") ?? "";

            jobs = _dbStore.GetAllJobInfoItemsForSearchCriteria(office, searchCriteria);
        }

        public void OnPostSearchAdvanced()
        {
            searchType = "advanced";

            if (searchCriteria is null)
            {
                searchCriteria = new SearchCriteria();
            }

            office = HttpContext.Session.GetString("office") ?? "";

            //Implement advanced search
            //You will need to add a new method to the DBStore class and call it to populate your list of jobs...
            //jobs = _dbStore.GetAllJobInfoItemsForSearchCriteriaAdvanced(office, searchCriteria);
            //See the long comment at the top of the GetAllJobInfoItemsForSearchCriteria method for mappings/logic between the SearchCriteria object's fields and the database.
            //We need "office" in order to get the geographic area to use, if they've selected that option.
        }
    }
}
