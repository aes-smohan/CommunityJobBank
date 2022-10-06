namespace CommunityJobBank.Models
{	
    public class JobsInfoItem
    {
		public Int64? jobs_id { get; set; }
        public string? noc_2011 { get; set; }	
		public string? remote_cd { get; set; }	
		public string? title { get; set; }
		public string? noc { get; set; }
		public string? work_period_cd { get; set; }
		public string? job_source_id { get; set; }
        public string? work_term_cd { get; set; }
        public Int32? num_positions { get; set; }
		public DateTime file_update_date { get; set; }
        public DateTime date_posted { get; set; }
        public DateTime display_until { get; set; }
        public string? city_name { get; set; }
        public string? url { get; set; }
        public string? province_cd { get; set; }
        public string? salary { get; set; }
        public string? employer_name { get; set; }
        public DateTime add_date{ get; set; }
        public DateTime mod_date { get; set; }
        public Int32? system_id_flag { get; set; }
        public Int32 system_id { get; set; }
        public string? job_note { get; set; }
        public string? email { get; set; }
        public string? address { get; set; }
        public string? postal { get; set; }
        public string? telephone { get; set; }
        public string? fax { get; set; }
        public string? contact_name { get; set; }
        public string? file_name { get; set; }
        public byte[]? file_data { get; set; }
        public string? content_type { get; set; }
        public Int32? file_length { get; set; }
        public Int32? office_code { get; set; }
        public Int16? job_type { get; set; }

        public List<JobsInfoItemLocation>? locations { get; set; }

        public JobsInfoItem()
        {

        }

    }
}
