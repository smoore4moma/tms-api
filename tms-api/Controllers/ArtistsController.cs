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
    public class ArtistsController : ApiController
    {


        public ArtistsController()
        {
        }

        /// <summary>
        /// This service allows you to search for an artist. You can use AND, OR, and AND NOT in the search.
        /// </summary>

        [Route("tms-api/artists")]
        public GetArtistsViewModel GetArtistsSearch(string search, string token)
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

                SqlCommand m_cmd_getArtistData = new SqlCommand("procTmsApiArtistSearch", sql_conn_data);
                m_cmd_getArtistData.CommandType = CommandType.StoredProcedure;
                m_cmd_getArtistData.Parameters.Clear();
                m_cmd_getArtistData.Parameters.Add(new SqlParameter(@"@p_displayname", SqlDbType.NVarChar) { Value = search });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getArtistData);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                sql_conn_data.Close();

                // Objects data
                DataTable dt_artist = ds.Tables[0];

                GetArtistsViewModel m_artists_view_model = new GetArtistsViewModel();

                m_artists_view_model.Source = "Your Museum Name Here";
                m_artists_view_model.Language = "EN";
                m_artists_view_model.ResultsCount = dt_artist.Rows.Count;

                GetArtistViewModel m_artist = new GetArtistViewModel();
                List<GetArtistViewModel> m_artist_list = new List<GetArtistViewModel>();

                foreach (DataRow dr_artist in dt_artist.Rows)
                {
                    m_artist = new GetArtistViewModel();

                    m_artist.ArtistID = (int)dr_artist["ArtistID"];
                    m_artist.AlphaSort = dr_artist["AlphaSort"].ToString();
                    m_artist.DisplayName = dr_artist["DisplayName"].ToString();
                    m_artist.BeginDate = dr_artist["BeginDate"].ToString();
                    m_artist.EndDate = dr_artist["EndDate"].ToString();
                    m_artist.DisplayDate = dr_artist["DisplayDate"].ToString();
                    m_artist.Sex = dr_artist["Sex"].ToString();
                    m_artist.Nationality = dr_artist["Nationality"].ToString();
                    m_artist.ObjectCount = (int)dr_artist["ResultsCount"];

                    m_artist_list.Add(m_artist);

                }

                m_artists_view_model.Artists = m_artist_list;

                return m_artists_view_model;


            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }


        }

        /// <summary>
        /// This service returns data about a single artist, including all objects.  Requires an ArtistID. 
        /// </summary>
        [Route("tms-api/artists/{artist_id}")]
        public GetArtistObjectsViewModel GetArtist(int artist_id, string token)
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
                SqlCommand m_cmd_getObjectIDs = new SqlCommand("procTmsApiArtistObjects", sql_conn_data);
                m_cmd_getObjectIDs.CommandType = CommandType.StoredProcedure;
                m_cmd_getObjectIDs.Parameters.Clear();
                m_cmd_getObjectIDs.Parameters.Add(new SqlParameter(@"@p_artistid", SqlDbType.Int) { Value = artist_id });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getObjectIDs);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                //sql_conn_data.Close();

                // Artist data
                DataTable dt_artist = ds.Tables[0];

                // ObjectIDs from ArtistID
                DataTable dt_objectids = ds.Tables[1];

                GetArtistObjectsViewModel m_artist_view_model = new GetArtistObjectsViewModel();

                DataRow dr_artist = dt_artist.Rows[0];

                m_artist_view_model.ArtistID = (int)dr_artist["ArtistID"];
                m_artist_view_model.AlphaSort = dr_artist["AlphaSort"].ToString();
                m_artist_view_model.DisplayName = dr_artist["DisplayName"].ToString();
                m_artist_view_model.BeginDate = dr_artist["BeginDate"].ToString();
                m_artist_view_model.EndDate = dr_artist["EndDate"].ToString();
                m_artist_view_model.DisplayDate = dr_artist["DisplayDate"].ToString();
                m_artist_view_model.Sex = dr_artist["Sex"].ToString();
                m_artist_view_model.Nationality = dr_artist["Nationality"].ToString();
                m_artist_view_model.ObjectCount = dt_objectids.Rows.Count;

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


                    m_artist_view_model.Objects = m_object_list;

                }

                sql_conn_data.Close();



                return m_artist_view_model;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }
        }

    }
}
