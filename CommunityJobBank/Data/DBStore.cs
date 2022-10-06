using Azure.Core.GeoJson;
using CommunityJobBank.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Net;

namespace CommunityJobBank.Data
{
    public class DBStore
    {

        string connectionString;
        SqlConnection? cnn;
        SqlCommand? sqlCommand;
        SqlDataReader? dataReader;

        public DBStore(IConfiguration configuration)
        {
            
            connectionString = Environment.GetEnvironmentVariable(configuration["DB:connection_var"]);
        }

        private void OpenDBConnection()
        {

            cnn = new SqlConnection(connectionString);

            try
            {
                cnn.Open();
            }
            catch (Exception e)
            {
                throw new Exception("Cannot open database. Check connection string. " + e.Message);
            }
        }

        private void CloseDBConnection()
        {
            if (cnn != null)
            {
                cnn.Close();
            }
        }

        /*===================================================================*/
        //DB Methods

        public List<JobsInfoItem> GetAllJobInfoItemsForDefaultGeographicLocation(string office)
        {
            OpenDBConnection();

            string sql = "select * from jobs_info jb where exists (select * from Jobs_Info_Geo_Lookup jb2 where jb2.system_id=jb.system_id and jb2.Office_Code=@OFFICECODE)";

            sqlCommand = new SqlCommand(sql, cnn);

            var aParam = new SqlParameter("@OFFICECODE", SqlDbType.BigInt);
            aParam.Value = Convert.ToInt64(office);
            sqlCommand.Parameters.Add(aParam);

            dataReader = sqlCommand.ExecuteReader();

            List<JobsInfoItem> result = new List<JobsInfoItem>();

            while (dataReader.Read())
            {
                var j = new JobsInfoItem();

                j.jobs_id = dataReader["jobs_id"] as Int64? ?? null;
                j.noc_2011 = dataReader["noc_2011"] as string ?? null;
                j.remote_cd = dataReader["remote_cd"] as string ?? null;
                j.title = dataReader["title"] as string ?? null;
                j.noc = dataReader["noc"] as string ?? null;
                j.work_period_cd = dataReader["work_period_cd"] as string ?? null;
                j.job_source_id = dataReader["job_source_id"] as string ?? null;
                j.work_term_cd = dataReader["work_term_cd"] as string ?? null;
                j.num_positions = dataReader["num_positions"] as Int32? ?? null;
                j.file_update_date = Convert.ToDateTime(dataReader["file_update_date"]);
                j.date_posted = Convert.ToDateTime(dataReader["date_posted"]);
                j.display_until = Convert.ToDateTime(dataReader["display_until"]);
                j.city_name = dataReader["city_name"] as string ?? null;
                j.url = dataReader["url"] as string ?? null;
                j.province_cd = dataReader["province_cd"] as string ?? null;
                j.salary = dataReader["salary"] as string ?? null;
                j.employer_name = dataReader["employer_name"] as string ?? null;
                j.add_date = Convert.ToDateTime(dataReader["add_date"]);
                j.mod_date = Convert.ToDateTime(dataReader["mod_date"]);
                j.system_id_flag = dataReader["system_id_flag"] as Int32? ?? null;
                j.system_id = Convert.ToInt32(dataReader["system_id"]);
                j.job_note = dataReader["job_note"] as string ?? null;
                j.email = dataReader["email"] as string ?? null;
                j.address = dataReader["address"] as string ?? null;
                j.postal = dataReader["postal"] as string ?? null;
                j.telephone = dataReader["telephone"] as string ?? null;
                j.fax = dataReader["fax"] as string ?? null;
                j.contact_name = dataReader["contact_name"] as string ?? null;
                j.file_name = dataReader["file_name"] as string ?? null;
                j.file_data = dataReader["file_data"] as byte[] ?? null;
                j.content_type = dataReader["content_type"] as string ?? null;
                j.file_length = dataReader["file_length"] as Int32? ?? null;
                j.office_code = dataReader["office_code"] as Int32? ?? null;
                j.job_type = dataReader["job_type"] as Int16? ?? null;
                
                result.Add(j);
            }

            dataReader.Close();
            sqlCommand.Dispose();

            foreach (JobsInfoItem j in result)
            {
                FillJobItemInfoLocations(j);
            }

            CloseDBConnection();

            return result;
        }

        public List<JobsInfoItem> GetAllJobInfoItemsForSearchCriteria(string office, SearchCriteria sc)
        {
            OpenDBConnection();

            //NOTE: The below logic could alternatively be implemented in a stored procedure, of course.
            //      So don't feel constrained to do it this way. Tweak, rework and optimize away!

            /*
            Mappings of search criteria to jobs_info and other tables
                Basic search
                    se_position ==> where jobs_info.title, NOC description, NOC jobs like value
                    se_location
                        geoarea: corresponds to items matching office in Jobs_Info_Geo_Lookup
                        provincecity: requires joining on Jobs_Info_Location
                            se_province ==> where Jobs_Info_Location.province_cd == value (2 char prov code)
                            se_citytown ==> Jobs_Info_Location.city_name
                        Additional values for advanced search
                            virtual: where jobs_info.job_type == 2
                            travelling: where jobs_info.job_type == 3
                    se_sortby
                        bestmatch: order by jobs_info.title, and then location
                        dateposted: order by jobs_info.date_posted
                    se_last30days ==> if "on" then where jobs_info.date_posted > getdate() - 30 days
                Additional fields for advanced search
                    se_employer ==> where jobs_info.employer_name like value
                    se_hours
                        fulltime: where jobs_info.work_period_cd == 'F'
                        parttime: where jobs_info.work_period_cd == 'P' Or jobs_info.work_period_cd == 'L'
                            NOTE: 'L': Part Time Leading to Full Time
                    se_term
                        permanent: where jobs_info.work_term_cd == 'P'
                        temporary: where jobs_info.work_term_cd == 'T'
                        casual: where jobs_info.work_term_cd == 'C'
                        seasonal: where jobs_info.work_term_cd == 'S'
                    se_dateposted
                        last48hours: where jobs_info.date_posted > getdate() - 2 days
                        last30days: where jobs_info.date_posted > getdate() - 30 days
                        morethan30days: where jobs_info.date_posted < getdate() - 30 days
             */

            var needPositionParam = false;
            var needOfficeParam = false;
            var needCityParam = false;
            var needProvinceParam = false;

            string sql = "select ji.* from jobs_info ji where ";

            //position
            if(!string.IsNullOrEmpty(sc.se_position))
            {
                sql += "(title like '%' + @POSITION + '%' or exists (select * from noc n where n.noc_code = ji.noc and n.desc_e like '%' + @POSITION + '%') or exists (select * from job_codes_en job where job.noc = ji.noc and job.job_desc like '%' + @POSITION + '%')) ";
                needPositionParam = true;
            }

            //location
            if (!string.IsNullOrEmpty(sc.se_location))
            {
                if (!string.IsNullOrEmpty(sc.se_position))
                {
                    sql += "and ";                    
                }

                if (sc.se_location.Equals("geoarea"))
                {
                    sql += "exists (select * from Jobs_Info_Geo_Lookup jb2 where jb2.system_id=ji.system_id and jb2.Office_Code = @OFFICE) ";
                    needOfficeParam = true;
;               }
                else if (sc.se_location.Equals("provincecity"))
                {
                    var tSql = "";
                    
                    if(!string.IsNullOrEmpty(sc.se_province))
                    {
                        tSql += "jil.province_cd = @PROVINCE";
                        needProvinceParam = true;
                    }
                    if (!string.IsNullOrEmpty(sc.se_province) && !string.IsNullOrEmpty(sc.se_citytown))
                    {
                        tSql += " and ";
                    }
                    if (!string.IsNullOrEmpty(sc.se_citytown))
                    {
                        tSql += "jil.city_name like '%' + @CITY + '%'";
                        needCityParam = true;
                    }

                    sql += "exists(select * from Jobs_Info_Location jil where jil.system_id = ji.system_id and (" + tSql + ")) ";
                    
                }
            }

            //last30days
            if (!string.IsNullOrEmpty(sc.se_last30days) && sc.se_last30days.Equals("on"))
            {
                if (!string.IsNullOrEmpty(sc.se_position) || !string.IsNullOrEmpty(sc.se_location))
                {
                    sql += "and ";
                }

                sql += "ji.date_posted > DATEADD(DAY, -30, getdate()) ";
            }

            //sortby
            if (!string.IsNullOrEmpty(sc.se_sortby))
            {
                if(sc.se_sortby.Equals("bestmatch"))
                {
                    sql += "order by ji.title"; //just putting this here for now; it needs to be refined
                }
                else if (sc.se_sortby.Equals("dateposted"))
                {
                    sql += "order by ji.date_posted desc";
                }
            }

            sqlCommand = new SqlCommand(sql, cnn);

            if (needPositionParam)
            {
                var aParam = new SqlParameter("@POSITION", SqlDbType.VarChar);
                aParam.Value = sc.se_position;
                sqlCommand.Parameters.Add(aParam);
            }
            if (needOfficeParam)
            {
                var aParam = new SqlParameter("@OFFICE", SqlDbType.BigInt);
                aParam.Value = Convert.ToInt64(office);
                sqlCommand.Parameters.Add(aParam);
            }
            if(needProvinceParam)
            {
                var aParam = new SqlParameter("@PROVINCE", SqlDbType.VarChar);
                aParam.Value = sc.se_province;
                sqlCommand.Parameters.Add(aParam);
            }
            if (needCityParam)
            {
                var aParam = new SqlParameter("@CITY", SqlDbType.VarChar);
                aParam.Value = sc.se_citytown;
                sqlCommand.Parameters.Add(aParam);
            }

            dataReader = sqlCommand.ExecuteReader();

            List<JobsInfoItem> result = new List<JobsInfoItem>();

            while (dataReader.Read())
            {
                var j = new JobsInfoItem();

                j.jobs_id = dataReader["jobs_id"] as Int64? ?? null;
                j.noc_2011 = dataReader["noc_2011"] as string ?? null;
                j.remote_cd = dataReader["remote_cd"] as string ?? null;
                j.title = dataReader["title"] as string ?? null;
                j.noc = dataReader["noc"] as string ?? null;
                j.work_period_cd = dataReader["work_period_cd"] as string ?? null;
                j.job_source_id = dataReader["job_source_id"] as string ?? null;
                j.work_term_cd = dataReader["work_term_cd"] as string ?? null;
                j.num_positions = dataReader["num_positions"] as Int32? ?? null;
                j.file_update_date = Convert.ToDateTime(dataReader["file_update_date"]);
                j.date_posted = Convert.ToDateTime(dataReader["date_posted"]);
                j.display_until = Convert.ToDateTime(dataReader["display_until"]);
                j.city_name = dataReader["city_name"] as string ?? null;
                j.url = dataReader["url"] as string ?? null;
                j.province_cd = dataReader["province_cd"] as string ?? null;
                j.salary = dataReader["salary"] as string ?? null;
                j.employer_name = dataReader["employer_name"] as string ?? null;
                j.add_date = Convert.ToDateTime(dataReader["add_date"]);
                j.mod_date = Convert.ToDateTime(dataReader["mod_date"]);
                j.system_id_flag = dataReader["system_id_flag"] as Int32? ?? null;
                j.system_id = Convert.ToInt32(dataReader["system_id"]);
                j.job_note = dataReader["job_note"] as string ?? null;
                j.email = dataReader["email"] as string ?? null;
                j.address = dataReader["address"] as string ?? null;
                j.postal = dataReader["postal"] as string ?? null;
                j.telephone = dataReader["telephone"] as string ?? null;
                j.fax = dataReader["fax"] as string ?? null;
                j.contact_name = dataReader["contact_name"] as string ?? null;
                j.file_name = dataReader["file_name"] as string ?? null;
                j.file_data = dataReader["file_data"] as byte[] ?? null;
                j.content_type = dataReader["content_type"] as string ?? null;
                j.file_length = dataReader["file_length"] as Int32? ?? null;
                j.office_code = dataReader["office_code"] as Int32? ?? null;
                j.job_type = dataReader["job_type"] as Int16? ?? null;

                result.Add(j);
            }

            dataReader.Close();
            sqlCommand.Dispose();

            foreach (JobsInfoItem j in result)
            {
                FillJobItemInfoLocations(j);
            }

            CloseDBConnection();

            return result;
        }

        private void FillJobItemInfoLocations(JobsInfoItem jii)
        {
            string sql = "select * from Jobs_Info_Location jil where system_id = @SYSTEMID";

            sqlCommand = new SqlCommand(sql, cnn);

            var aParam = new SqlParameter("@SYSTEMID", SqlDbType.Int);
            aParam.Value = jii.system_id;
            sqlCommand.Parameters.Add(aParam);

            dataReader = sqlCommand.ExecuteReader();

            jii.locations = new List<JobsInfoItemLocation>();

            while (dataReader.Read())
            {
                var j = new JobsInfoItemLocation();

                j.jobs_info_location_id = Convert.ToInt32(dataReader["jobs_info_location_id"]);
                j.system_id = Convert.ToInt32(dataReader["system_id"]);
                j.city_name = WebUtility.HtmlDecode((dataReader["city_name"] as string) ?? "").Trim();
                j.province_cd = WebUtility.HtmlDecode((dataReader["province_cd"] as string) ?? "").Trim();
                
                jii.locations.Add(j);
            }

            dataReader.Close();
            sqlCommand.Dispose();
        }

    }
}
