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
    public class ObjectsController : ApiController
    {

        public ObjectsController()
        {
        }

        /// <summary>
        /// Returns all objects that are "tagged" with specific Getty Art &amp; Architecture Thesaurus terms.
        /// </summary>
        [Route("tms-api/objects/terms/{term_id}")]
        public GetObjectsViewModel GetObjectsTerm(string term_id, string token)
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
                
                // Get objects associated with term
                SqlCommand m_cmd_getObjectIDs = new SqlCommand("procTmsApiTermsObjects", sql_conn_data);
                m_cmd_getObjectIDs.CommandType = CommandType.StoredProcedure;
                m_cmd_getObjectIDs.Parameters.Clear();
                m_cmd_getObjectIDs.Parameters.Add(new SqlParameter(@"@p_termid", SqlDbType.Int) { Value = term_id });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getObjectIDs);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                //sql_conn_data.Close();

                // ObjectIDs from TermID
                DataTable dt_objectids = ds.Tables[0];

                GetObjectsViewModel m_objects_view_model = new GetObjectsViewModel();

                m_objects_view_model.Source = "Your Museum Name Here";
                m_objects_view_model.Language = "EN";
                m_objects_view_model.ResultsCount = dt_objectids.Rows.Count;

                GetObjectViewModel m_object = new GetObjectViewModel();
                List<GetObjectViewModel> m_object_list = new List<GetObjectViewModel>();

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

                        m_object = new GetObjectViewModel();

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

                        // Exhibitions for objects
                        DataTable dt_exhibitions = ds_object.Tables[1];

                        GetExhibitionsViewModel m_exhibitions_view_model = new GetExhibitionsViewModel();

                        m_exhibitions_view_model.ResultsCount = dt_exhibitions.Rows.Count;

                        GetExhibitionViewModel m_exhibition = new GetExhibitionViewModel();
                        List<GetExhibitionViewModel> m_exhibition_list = new List<GetExhibitionViewModel>();

                        foreach (DataRow dr_exhibitions in dt_exhibitions.Rows)
                        {
                            m_exhibition = new GetExhibitionViewModel();

                            m_exhibition.ExhibitionID = (int)dr_exhibitions["ExhibitionID"];
                            m_exhibition.ProjectNumber = dr_exhibitions["ProjectNumber"].ToString();
                            m_exhibition.ExhibitionTitle = dr_exhibitions["ExhTitle"].ToString();
                            m_exhibition.Department = dr_exhibitions["Department"].ToString();
                            m_exhibition.ExhibitionDisplayDate = dr_exhibitions["DisplayDate"].ToString();
                            m_exhibition.ExhibitionBeginDate = dr_exhibitions["BeginISODate"].ToString();
                            m_exhibition.ExhibitionEndDate = dr_exhibitions["EndISODate"].ToString();
                            m_exhibition.ObjectCount = (int)dr_exhibitions["ResultsCount"];

                            m_exhibition_list.Add(m_exhibition);

                        }

                        m_exhibitions_view_model.Exhibitions = m_exhibition_list;

                        m_object.Exhibitions = m_exhibitions_view_model;

                        GetTermViewModel m_term = new GetTermViewModel();
                        List<GetTermViewModel> m_term_list = new List<GetTermViewModel>();

                        // Terms for objects
                        DataTable dt_terms = ds_object.Tables[2];

                        GetTermsViewModel m_terms_view_model = new GetTermsViewModel();

                        m_terms_view_model.ResultsCount = dt_terms.Rows.Count;

                        foreach (DataRow dr_terms in dt_terms.Rows)
                        {
                            m_term = new GetTermViewModel();

                            m_term.TermID = (int)dr_terms["TermID"];
                            m_term.Term = dr_terms["Term"].ToString();
                            m_term.TermType = dr_terms["TermType"].ToString();

                            m_term_list.Add(m_term);

                        }

                        m_terms_view_model.Terms = m_term_list;

                        m_object.Terms = m_terms_view_model;

                        m_object_list.Add(m_object); 

                    }

                    
                    m_objects_view_model.Objects = m_object_list;

                }

                sql_conn_data.Close();

                

                return m_objects_view_model;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }
        }

        /// <summary>
        /// Returns all terms associated with an object. 
        /// </summary>
        [Route("tms-api/objects/terms/")]
        public GetTermsViewModel GetTerms(string token)
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
                SqlCommand m_cmd_getTermData = new SqlCommand("procTmsApiTerms", sql_conn_data);
                m_cmd_getTermData.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getTermData);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                sql_conn_data.Close();

                // Objects data
                DataTable dt_terms = ds.Tables[0];

                GetTermsViewModel m_terms_view_model = new GetTermsViewModel();

                m_terms_view_model.Source = "Your Museum Name Here";
                m_terms_view_model.Language = "EN";
                m_terms_view_model.ResultsCount = dt_terms.Rows.Count;

                GetTermViewModel m_term = new GetTermViewModel();
                List<GetTermViewModel> m_term_list = new List<GetTermViewModel>();

                foreach (DataRow dr_terms in dt_terms.Rows)
                {
                    m_term = new GetTermViewModel();

                    m_term.TermID = (int)dr_terms["TermID"];
                    m_term.Term = dr_terms["Term"].ToString();
                    m_term.TermType = dr_terms["TermType"].ToString();
                    m_term.TermCount = (int)dr_terms["TermCount"];

                    m_term_list.Add(m_term);

                }

                m_terms_view_model.Terms = m_term_list;

                return m_terms_view_model;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }

        }

   
        /// <summary>
        /// Returns a single object.  Requires an ObjectID or ObjectNumber.  If you supply an integer, then the service uses ObjectID otherwise it uses ObjectNumber. 
        /// </summary> 

        [Route("tms-api/objects/{object_id_number:maxlength(42)}")]
        public GetObjectsViewModel GetObject(string object_id_number, string token)
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

                // Check if ObjectID or ObjectNumber supplied. If ObjectNumber, then lookup ObjectID
                string m_object_id_number = "";
                int m_object_id;
                bool m_isobjectid = int.TryParse(object_id_number, out m_object_id);

                if (m_isobjectid == false)
                {
                    SqlCommand m_cmd_getObjectID = new SqlCommand("procTmsGetObjectID", sql_conn_data);
                    m_cmd_getObjectID.CommandType = CommandType.StoredProcedure;
                    m_cmd_getObjectID.Parameters.Clear();
                    m_cmd_getObjectID.Parameters.Add(new SqlParameter(@"@p_objectnumber", SqlDbType.NVarChar) { Value = object_id_number });

                    m_object_id_number = m_cmd_getObjectID.ExecuteScalar().ToString();

                    m_isobjectid = int.TryParse(m_object_id_number, out m_object_id);

                    if (m_isobjectid == false)
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("object_id_number {0} is not valid", object_id_number)));
                    }
                }

                // Should only use ObjectID at this point

                SqlCommand m_cmd_getObjectData = new SqlCommand("procTmsApiObjects", sql_conn_data);
                m_cmd_getObjectData.CommandType = CommandType.StoredProcedure;
                m_cmd_getObjectData.Parameters.Clear();
                m_cmd_getObjectData.Parameters.Add(new SqlParameter(@"@p_objectid", SqlDbType.Int) { Value = m_object_id });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getObjectData);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                sql_conn_data.Close();

                // Objects data
                DataTable dt_objects = ds.Tables[0];
                
                GetObjectsViewModel m_objects_view_model = new GetObjectsViewModel();

                    m_objects_view_model.Source = "Your Museum Name Here";
                    m_objects_view_model.Language = "EN";
                    m_objects_view_model.ResultsCount = dt_objects.Rows.Count;


                    GetObjectViewModel m_object = new GetObjectViewModel();
                    List<GetObjectViewModel> m_object_list = new List<GetObjectViewModel>();

                    GetExhibitionViewModel m_exhibition = new GetExhibitionViewModel();
                    List<GetExhibitionViewModel> m_exhibition_list = new List<GetExhibitionViewModel>();

                    GetTermViewModel m_term = new GetTermViewModel();
                    List<GetTermViewModel> m_term_list = new List<GetTermViewModel>();


                    foreach (DataRow dr_objects in dt_objects.Rows)
                    {

                        m_object = new GetObjectViewModel();

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
                        m_object.LastModifiedDate = DateTime.Parse(dr_objects["LastModifiedDate"].ToString(), null, DateTimeStyles.AdjustToUniversal);  //Convert.ToDateTime(dr_objects["LastModifiedDate"].ToString(),);
                        m_object.ImageID = dr_objects["ImageID"].ToString();
                        m_object.Thumbnail = dr_objects["Thumbnail"].ToString();
                        m_object.FullImage = dr_objects["FullImage"].ToString();

                        // Exhibitions for objects
                        DataTable dt_exhibitions = ds.Tables[1];

                        GetExhibitionsViewModel m_exhibitions_view_model = new GetExhibitionsViewModel();

                        m_exhibitions_view_model.ResultsCount = dt_exhibitions.Rows.Count;

                        foreach (DataRow dr_exhibitions in dt_exhibitions.Rows)
                        {
                            m_exhibition = new GetExhibitionViewModel();

                            m_exhibition.ExhibitionID = (int)dr_exhibitions["ExhibitionID"];
                            m_exhibition.ProjectNumber = dr_exhibitions["ProjectNumber"].ToString();
                            m_exhibition.ExhibitionTitle = dr_exhibitions["ExhTitle"].ToString();
                            m_exhibition.Department = dr_exhibitions["Department"].ToString();
                            m_exhibition.ExhibitionDisplayDate = dr_exhibitions["DisplayDate"].ToString();
                            m_exhibition.ExhibitionBeginDate = dr_exhibitions["BeginISODate"].ToString();
                            m_exhibition.ExhibitionEndDate = dr_exhibitions["EndISODate"].ToString();
                            m_exhibition.ObjectCount = (int)dr_exhibitions["ResultsCount"];

                            m_exhibition_list.Add(m_exhibition);

                        }

                        m_exhibitions_view_model.Exhibitions = m_exhibition_list;

                        m_object.Exhibitions = m_exhibitions_view_model;


                        // Terms for objects
                        DataTable dt_terms = ds.Tables[2];

                        GetTermsViewModel m_terms_view_model = new GetTermsViewModel();

                        m_terms_view_model.ResultsCount = dt_terms.Rows.Count;

                        foreach (DataRow dr_terms in dt_terms.Rows)
                        {
                            m_term = new GetTermViewModel();

                            m_term.TermID = (int)dr_terms["TermID"];
                            m_term.Term = dr_terms["Term"].ToString();
                            m_term.TermType = dr_terms["TermType"].ToString();

                            m_term_list.Add(m_term);

                        }

                        m_terms_view_model.Terms = m_term_list;

                        m_object.Terms = m_terms_view_model;

                        m_object_list.Add(m_object);

                    }

                    m_objects_view_model.Objects = m_object_list;

                    return m_objects_view_model;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }


        }

        /// <summary>
        /// Returns a random object. 
        /// </summary>
        
        // GET api/objects/random
        [Route("tms-api/objects/random")]
        public GetObjectsViewModel GetObjectRandom(string token)
        {

            Random m_random = new Random();
            int m_random_object = m_random.Next(100, 10000);

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

                SqlCommand m_cmd_getObjectData = new SqlCommand("procTmsApiObjects", sql_conn_data);
                m_cmd_getObjectData.CommandType = CommandType.StoredProcedure;
                m_cmd_getObjectData.Parameters.Clear();
                m_cmd_getObjectData.Parameters.Add(new SqlParameter(@"@p_objectid", SqlDbType.Int) { Value = m_random_object });

                SqlDataAdapter sda = new SqlDataAdapter(m_cmd_getObjectData);
                DataSet ds = new DataSet();
                sda.Fill(ds);
                sda.Dispose();
                sql_conn_data.Close();

                // Objects data
                DataTable dt_objects = ds.Tables[0];

                GetObjectsViewModel m_objects_view_model = new GetObjectsViewModel();

                m_objects_view_model.Source = "Your Museum Name Here";
                m_objects_view_model.Language = "EN";
                m_objects_view_model.ResultsCount = dt_objects.Rows.Count;


                GetObjectViewModel m_object = new GetObjectViewModel();
                List<GetObjectViewModel> m_object_list = new List<GetObjectViewModel>();

                GetExhibitionViewModel m_exhibition = new GetExhibitionViewModel();
                List<GetExhibitionViewModel> m_exhibition_list = new List<GetExhibitionViewModel>();

                GetTermViewModel m_term = new GetTermViewModel();
                List<GetTermViewModel> m_term_list = new List<GetTermViewModel>();


                foreach (DataRow dr_objects in dt_objects.Rows)
                {

                    m_object = new GetObjectViewModel();

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
                    m_object.LastModifiedDate = DateTime.Parse(dr_objects["LastModifiedDate"].ToString(), null, DateTimeStyles.AdjustToUniversal);  //Convert.ToDateTime(dr_objects["LastModifiedDate"].ToString(),);
                    m_object.ImageID = dr_objects["ImageID"].ToString();
                    m_object.Thumbnail = dr_objects["Thumbnail"].ToString();
                    m_object.FullImage = dr_objects["FullImage"].ToString();

                    // Exhibitions for objects
                    DataTable dt_exhibitions = ds.Tables[1];

                    GetExhibitionsViewModel m_exhibitions_view_model = new GetExhibitionsViewModel();

                    m_exhibitions_view_model.ResultsCount = dt_exhibitions.Rows.Count;

                    foreach (DataRow dr_exhibitions in dt_exhibitions.Rows)
                    {
                        m_exhibition = new GetExhibitionViewModel();

                        m_exhibition.ExhibitionID = (int)dr_exhibitions["ExhibitionID"];
                        m_exhibition.ProjectNumber = dr_exhibitions["ProjectNumber"].ToString();
                        m_exhibition.ExhibitionTitle = dr_exhibitions["ExhTitle"].ToString();
                        m_exhibition.Department = dr_exhibitions["Department"].ToString();
                        m_exhibition.ExhibitionDisplayDate = dr_exhibitions["DisplayDate"].ToString();
                        m_exhibition.ExhibitionBeginDate = dr_exhibitions["BeginISODate"].ToString();
                        m_exhibition.ExhibitionEndDate = dr_exhibitions["EndISODate"].ToString();
                        m_exhibition.ObjectCount = (int)dr_exhibitions["ResultsCount"];

                        m_exhibition_list.Add(m_exhibition);

                    }

                    m_exhibitions_view_model.Exhibitions = m_exhibition_list;

                    m_object.Exhibitions = m_exhibitions_view_model;


                    // Terms for objects
                    DataTable dt_terms = ds.Tables[2];

                    GetTermsViewModel m_terms_view_model = new GetTermsViewModel();

                    m_terms_view_model.ResultsCount = dt_terms.Rows.Count;

                    foreach (DataRow dr_terms in dt_terms.Rows)
                    {
                        m_term = new GetTermViewModel();

                        m_term.TermID = (int)dr_terms["TermID"];
                        m_term.Term = dr_terms["Term"].ToString();
                        m_term.TermType = dr_terms["TermType"].ToString();

                        m_term_list.Add(m_term);

                    }

                    m_terms_view_model.Terms = m_term_list;

                    m_object.Terms = m_terms_view_model;

                    m_object_list.Add(m_object);

                }

                m_objects_view_model.Objects = m_object_list;

                return m_objects_view_model;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("Token {0} is not valid.", token)));
            }
        }

    }
}
