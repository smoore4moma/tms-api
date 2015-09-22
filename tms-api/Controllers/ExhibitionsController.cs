using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Http.Cors;
using System.Globalization;

using tms_api.Models;

namespace tms_api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class ExhibitionsController : ApiController
    {

        public ExhibitionsController()
        {
        }

        /// <summary>
        /// This service allows you to search for exhibitions using the title. You can use AND, OR, and AND NOT in the search.
        /// </summary>

        [Route("tms-api/exhibitions")]
        public GetExhibitionsViewModel GetAllExhibitions(string search, string token)
        {

            // First check the auth token

            // Connect to databases
            string conn_token = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string conn_data = ConfigurationManager.ConnectionStrings["TMSConnectionString"].ConnectionString;

            SqlConnection sql_conn_token = new SqlConnection(conn_token);
            SqlConnection sql_conn_data = new SqlConnection(conn_data);

            sql_conn_token.Open();

            SqlCommand m_cmd_verify_token = new SqlCommand("procValidateToken", sql_conn_token);
            m_cmd_verify_token.CommandType = CommandType.StoredProcedure;
            m_cmd_verify_token.Parameters.Clear();
            m_cmd_verify_token.Parameters.Add(new SqlParameter(@"@p_token", SqlDbType.NVarChar) { Value = token });

            int m_isvalid = Convert.ToInt32(m_cmd_verify_token.ExecuteScalar());

            sql_conn_token.Close();

            // 0 is false, 1 is true.
            if (m_isvalid == 1)
            {

                SqlCommand m_cmd_getExhData = new SqlCommand("procTmsApiExhibitionSearch", sql_conn_data);
                m_cmd_getExhData.CommandType = CommandType.StoredProcedure;
                m_cmd_getExhData.Parameters.Clear();
                m_cmd_getExhData.Parameters.Add(new SqlParameter(@"@p_exhibitiontitle", SqlDbType.NVarChar) { Value = search });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getExhData);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                sql_conn_data.Close();

                // Objects data
                DataTable dt_exh = ds.Tables[0];

                GetExhibitionsViewModel m_exh_view_model = new GetExhibitionsViewModel();

                m_exh_view_model.Source = "Your Museum Name Here";
                m_exh_view_model.Language = "EN";
                m_exh_view_model.ResultsCount = dt_exh.Rows.Count;

                GetExhibitionViewModel m_exh = new GetExhibitionViewModel();
                List<GetExhibitionViewModel> m_exh_list = new List<GetExhibitionViewModel>();

                foreach (DataRow dr_exh in dt_exh.Rows)
                {
                    m_exh = new GetExhibitionViewModel();

                    m_exh.ExhibitionID = (int)dr_exh["ExhibitionID"];
                    m_exh.ProjectNumber = dr_exh["ProjectNumber"].ToString();
                    m_exh.ExhibitionTitle = dr_exh["ExhTitle"].ToString();
                    m_exh.Department = dr_exh["Department"].ToString();
                    m_exh.ExhibitionDisplayDate = dr_exh["DisplayDate"].ToString();
                    m_exh.ExhibitionBeginDate = dr_exh["BeginISODate"].ToString();
                    m_exh.ExhibitionEndDate = dr_exh["EndISODate"].ToString();
                    m_exh.ObjectCount = (int)dr_exh["ResultsCount"];

                    m_exh_list.Add(m_exh);

                }

                m_exh_view_model.Exhibitions = m_exh_list;

                return m_exh_view_model;


            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }



        }

        /// <summary>
        /// This service returns data about a single exhibition, including exhibition objects.  Requires an ExhibitionID. 
        /// </summary>
        [Route("tms-api/exhibitions/{exhibition_id}")]
        public GetExhibitionObjectsViewModel GetExhibition(int exhibition_id, string token)
        {
            // First check the auth token

            // Connect to databases
            string conn_token = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string conn_data = ConfigurationManager.ConnectionStrings["TMSConnectionString"].ConnectionString;

            SqlConnection sql_conn_token = new SqlConnection(conn_token);
            SqlConnection sql_conn_data = new SqlConnection(conn_data);

            sql_conn_token.Open();

            SqlCommand m_cmd_verify_token = new SqlCommand("procValidateToken", sql_conn_token);
            m_cmd_verify_token.CommandType = CommandType.StoredProcedure;
            m_cmd_verify_token.Parameters.Clear();
            m_cmd_verify_token.Parameters.Add(new SqlParameter(@"@p_token", SqlDbType.NVarChar) { Value = token });

            int m_isvalid = Convert.ToInt32(m_cmd_verify_token.ExecuteScalar());

            sql_conn_token.Close();

            // 0 is false, 1 is true.
            if (m_isvalid == 1)
            {
                sql_conn_data.Open();

                // Get objects associated with exhibition
                SqlCommand m_cmd_getObjectIDs = new SqlCommand("procTmsApiExhibitionObjects", sql_conn_data);
                m_cmd_getObjectIDs.CommandType = CommandType.StoredProcedure;
                m_cmd_getObjectIDs.Parameters.Clear();
                m_cmd_getObjectIDs.Parameters.Add(new SqlParameter(@"@p_exhibitionid", SqlDbType.Int) { Value = exhibition_id });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getObjectIDs);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                //sql_conn_data.Close();

                // Exhibition data
                DataTable dt_exhdata = ds.Tables[0];

                // ObjectIDs from ExhibitionID
                DataTable dt_objectids = ds.Tables[1];

                GetExhibitionObjectsViewModel m_exh_view_model = new GetExhibitionObjectsViewModel();

                DataRow dr_exhdata = dt_exhdata.Rows[0];

                m_exh_view_model.ExhibitionID = (int)dr_exhdata["ExhibitionID"];
                m_exh_view_model.ProjectNumber = dr_exhdata["ProjectNumber"].ToString();
                m_exh_view_model.ExhibitionTitle = dr_exhdata["ExhTitle"].ToString();
                m_exh_view_model.Department = dr_exhdata["Department"].ToString();
                m_exh_view_model.ExhibitionDisplayDate = dr_exhdata["DisplayDate"].ToString();
                m_exh_view_model.ExhibitionBeginDate = dr_exhdata["BeginISODate"].ToString();
                m_exh_view_model.ExhibitionEndDate = dr_exhdata["EndISODate"].ToString();
                m_exh_view_model.ObjectCount = dt_objectids.Rows.Count;

                GetAltObjectViewModel m_object = new GetAltObjectViewModel();
                List<GetAltObjectViewModel> m_object_list = new List<GetAltObjectViewModel>();

                foreach (DataRow dr_objectid in dt_objectids.Rows)
                {

                    SqlCommand m_cmd_getObject = new SqlCommand("procTmsApiObjects", sql_conn_data);
                    m_cmd_getObject.CommandType = CommandType.StoredProcedure;
                    m_cmd_getObject.Parameters.Clear();
                    m_cmd_getObject.Parameters.Add(new SqlParameter(@"@p_objectid", SqlDbType.Int) { Value = (int)dr_objectid["ObjectID"] });

                    SqlDataAdapter sda_object = new SqlDataAdapter(m_cmd_getObject);
                    DataSet ds_object = new DataSet();
                    sda_object.Fill(ds_object);
                    sda_object.Dispose();

                    // Objects data
                    DataTable dt_objects = ds_object.Tables[0];

                    foreach (DataRow dr_objects in dt_objects.Rows)
                    {

                        m_object = new GetAltObjectViewModel();

                        m_object.ObjectNumber = dr_objects["ObjectNumber"].ToString();
                        m_object.ObjectID = (int)dr_objects["ObjectID"];
                        m_object.Title = dr_objects["Title"].ToString();
                        m_object.DisplayName = dr_objects["DisplayName"].ToString();
                        m_object.AlphaSort = dr_objects["AlphaSort"].ToString();
                        m_object.ArtistID = (int)dr_objects["ArtistID"];
                        m_object.DisplayDate = dr_objects["DisplayDate"].ToString();
                        m_object.Dated = dr_objects["Dated"].ToString();
                        m_object.DateBegin = (int)dr_objects["DateBegin"];
                        m_object.DateEnd = (int)dr_objects["DateEnd"];
                        m_object.Medium = dr_objects["Medium"].ToString();
                        m_object.Dimensions = dr_objects["Dimensions"].ToString();
                        m_object.Department = dr_objects["Department"].ToString();
                        m_object.Classification = dr_objects["Classification"].ToString();
                        m_object.OnView = (Int16)dr_objects["OnView"];
                        m_object.Provenance = dr_objects["Provenance"].ToString();
                        m_object.Description = dr_objects["Description"].ToString();
                        m_object.ObjectStatusID = (int)dr_objects["ObjectStatusID"];
                        m_object.CreditLine = dr_objects["CreditLine"].ToString();
                        m_object.LastModifiedDate = DateTime.Parse(dr_objects["LastModifiedDate"].ToString(), null, DateTimeStyles.AdjustToUniversal);
                        m_object.ImageID = dr_objects["ImageID"].ToString();
                        m_object.Thumbnail = dr_objects["Thumbnail"].ToString();
                        m_object.FullImage = dr_objects["FullImage"].ToString();

                        m_object_list.Add(m_object);

                    }

                    m_exh_view_model.Objects = m_object_list;

                }

                sql_conn_data.Close();



                return m_exh_view_model;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }


        }



    }
}
