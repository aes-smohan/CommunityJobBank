namespace CommunityJobBank.Models
{
    public class JobsInfoItemLocation
    {
        public Int32 jobs_info_location_id { get; set; }
        public Int32 system_id { get; set; }
        public string city_name { get; set; } = "";
        public string province_cd { get; set; } = "";

        public JobsInfoItemLocation()
        {

        }
    }
}
