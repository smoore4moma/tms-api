using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

using moma_tms_api.Models;

namespace moma_tms_api.Controllers
{
    public class AlexaController : ApiController
    {
        private const string ApplicationId = "amzn1.ask.skill.youramazonskillidhere"; 

        string m_image_art;

        [HttpPost, Route("api/alexa/moma")]
        public dynamic Moma(AlexaRequest alexaRequest)
        {

            if (alexaRequest.Session.Application.ApplicationId != ApplicationId)
                throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));

            var totalSeconds = (DateTime.UtcNow - alexaRequest.Request.Timestamp).TotalSeconds;
            if (totalSeconds <= 0 || totalSeconds > 150)
                throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));

            var request = new Requests().Create(new moma_tms_api.Request
            {

                MemberId = (alexaRequest.Session.Attributes == null) ? 0 : alexaRequest.Session.Attributes.MemberId,
                Timestamp = alexaRequest.Request.Timestamp,
                Intent = (alexaRequest.Request.Intent == null) ? "" : alexaRequest.Request.Intent.Name,
                AppId = alexaRequest.Session.Application.ApplicationId,
                RequestId = alexaRequest.Request.RequestId,
                SessionId = alexaRequest.Session.SessionId,
                UserId = alexaRequest.Session.User.UserId,
                IsNew = alexaRequest.Session.New,
                Version = alexaRequest.Version,
                Type = alexaRequest.Request.Type,
                Reason = alexaRequest.Request.Reason,
                SlotsList = alexaRequest.Request.Intent.GetSlots(),
                DateCreated = DateTime.UtcNow

            });

            AlexaResponse response = null;

            switch (request.Type)
            {
                case "LaunchRequest":
                    response = LaunchRequestHandler(request);
                    break;
                case "IntentRequest":
                    response = IntentRequestHandler(request);
                    break;
                case "SessionEndedRequest":
                    response = SessionEndedRequestHandler(request);
                    break;
            }

            return response;
        }


        private AlexaResponse IntentRequestHandler(Request request)
        {
            AlexaResponse response = null;

            switch (request.Intent)
            {

                case "ExhibitionsIntent":
                    response = ExhibitionsIntentHandler(request);
                    break;
                case "EventsIntent":
                    response = EventsIntentHandler(request);
                    break;
                case "FilmsIntent":
                    response = FilmsIntentHandler(request);
                    break;
                case "ArtistsIntent":
                    response = ArtistsIntentHandler(request);
                    break;
                case "ArtIntent":
                    response = ArtIntentHandler(request);
                    break;
                case "BathroomsIntent":
                    response = BathroomsIntentHandler(request);
                    break;
                case "HoursIntent":
                    response = HoursIntentHandler(request);
                    break;
                case "DirectionsIntent":
                    response = DirectionsIntentHandler(request);
                    break;
                case "FoodIntent":
                    response = FoodIntentHandler(request);
                    break;
                case "StoresIntent":
                    response = StoresIntentHandler(request);
                    break;
                case "AMAZON.CancelIntent":
                case "AMAZON.StopIntent":
                    response = CancelOrStopIntentHandler(request);
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntent(request);
                    break;
            }

            return response;
        }

        private AlexaResponse LaunchRequestHandler(Request request)
        {
            var response = new AlexaResponse("welcome to mow mah.  Please tell me what you would like to hear about.");
            response.Session.MemberId = request.MemberId;
            response.Response.Card.Title = "MoMA";
            response.Response.Card.Text = "Welcome to MoMA.\nYou must be a Modern person!\n\nPlease tell me what you would like to hear about.";
            response.Response.Reprompt.OutputSpeech.Text = "i'm sorry.  i did not understand.";
            response.Response.ShouldEndSession = false;

            return response;
        }

        private AlexaResponse HelpIntent(Request request)
        {
            var response = new AlexaResponse("You can say, Alexa, tell me about mow mah.  You can also say, Alexa, stop or Alexa, cancel, at any time to exit.", false);
            response.Response.Card.Title = "MoMA";
            response.Response.Card.Text = "Welcome to MoMA.\nYou must be a Modern person!\n\nPlease tell me what you would like to hear about.";

            response.Response.Reprompt.OutputSpeech.Text = "OK, I didn't quite get that.  Would you like to try again? Please say Yes or No.";

            if (request.Intent == "AMAZON.YesIntent")
            {
                response = LaunchRequestHandler(request);
            }

            if (request.Intent == "AMAZON.NoIntent")
            {
                response.Response.OutputSpeech.Text = "OK, i'll be quiet now.  have a nice day!";
                response.Response.ShouldEndSession = true;
            }

            return response;
        }

        private AlexaResponse CancelOrStopIntentHandler(Request request)
        {
            return new AlexaResponse("Thanks for listening, we look forward to seeing you in our galleries soon.", true);
        }

        private AlexaResponse SessionEndedRequestHandler(Request request)
        {
            return null;
        }

        // Below is where we get MoMA data and return it to Alexa as a response

        // Exhibitions
        private AlexaResponse ExhibitionsIntentHandler(Request request)
        {
            string dateValue = "";
            if (request.SlotsList.FirstOrDefault(s => s.Key == "Date").Value != null)
                dateValue = request.SlotsList.FirstOrDefault(s => s.Key == "Date").Value.ToString();

            string conn_data = ConfigurationManager.ConnectionStrings["TMSConnectionString"].ConnectionString;
            SqlConnection sql_conn_data = new SqlConnection(conn_data);
            sql_conn_data.Open();

            // Get exhibitions
            SqlCommand getAlexaExhibitions = new SqlCommand("procMomaAlexaExhibitions", sql_conn_data);
            getAlexaExhibitions.CommandType = CommandType.StoredProcedure;
            getAlexaExhibitions.Parameters.Clear();
            getAlexaExhibitions.Parameters.Add(new SqlParameter(@"@p_alexa_date", SqlDbType.VarChar, 50) { Value = dateValue.ToString() });

            SqlDataAdapter sda = new SqlDataAdapter(getAlexaExhibitions);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            sda.Dispose();

            DataTable dt_exhs = ds.Tables[0];
            int m_dt_exhs_ct = dt_exhs.Rows.Count;

            var output = new StringBuilder("Here are the exhibitions you asked for. ");
            var outputCard = new StringBuilder("Here are the exhibitions you asked for.\n");

            //if (m_dt_exhs_ct >= 5)
            //{
            //foreach (DataRow dr_exh in dt_exhs.Rows.Cast<DataRow>().Take(5))
            foreach (DataRow dr_exh in dt_exhs.Rows)
            {
                outputCard.Append(dr_exh["ExhTitle"].ToString() + ", " + dr_exh["DisplayDate"].ToString() + "\n");
                output.Append(dr_exh["ExhTitle"].ToString() + ". ");
            }


            sql_conn_data.Close();

            var response = new AlexaResponse(output.ToString());
            response.Response.Card.Title = "Exhibitions";
            response.Response.Card.Type = "Standard";
            response.Response.Card.Text = outputCard.ToString();

            return response;
        }

        // Events
        private AlexaResponse EventsIntentHandler(Request request)
        {
            var response = new AlexaResponse("I'm still working on events. Please try again later.");
            response.Response.Card.Title = "Events";
            response.Response.Card.Type = "Simple";
            response.Response.Card.Content = "Sorry! I'm still working on events. Please try again later.";

            return response;
        }

        // Films
        private AlexaResponse FilmsIntentHandler(Request request)
        {
            var response = new AlexaResponse("Same-day film tickets are free for members or with a general admission receipt.  For daily screening information, visit the film desk on the first floor, or check mowmah dot org slash film.");
            response.Response.Card.Title = "Films";
            response.Response.Card.Type = "Simple";
            response.Response.Card.Content = "Same-day film tickets are free for members or with a general admission receipt.\nAdults $12; Seniors (65 and over with ID) $10; Students (full-time with ID) $8; Children (16 and under) free.\nFor daily screening information, visit the film desk on the first floor or check moma.org/film.";

            return response;
        }

        // Artist
        private AlexaResponse ArtistsIntentHandler(Request request)
        {
            string artistValue = "";
            string m_name1 = "";
            string m_name2 = "";

            // Names are done this way to allow for more precise results.

            if (request.SlotsList.FirstOrDefault(s => s.Key == "StartName").Value != null)
                m_name1 = request.SlotsList.FirstOrDefault(s => s.Key == "StartName").Value.ToString();

            if (request.SlotsList.FirstOrDefault(s => s.Key == "EndName").Value != null)
                m_name2 = request.SlotsList.FirstOrDefault(s => s.Key == "EndName").Value.ToString();

            if (String.IsNullOrEmpty(m_name2))
            {
                artistValue = m_name1;
            }
            else
            {
                artistValue = m_name1 + " AND " + m_name2;
            }

            string conn_data = ConfigurationManager.ConnectionStrings["TMSConnectionString"].ConnectionString;
            SqlConnection sql_conn_data = new SqlConnection(conn_data);
            sql_conn_data.Open();

            // Get artists
            SqlCommand getAlexaArtist = new SqlCommand("procMomaApiArtistSearch", sql_conn_data);
            getAlexaArtist.CommandType = CommandType.StoredProcedure;
            getAlexaArtist.Parameters.Clear();
            getAlexaArtist.Parameters.Add(new SqlParameter(@"@p_displayname", SqlDbType.VarChar, 255) { Value = artistValue.ToString() });

            SqlDataAdapter sda = new SqlDataAdapter(getAlexaArtist);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            sda.Dispose();

            DataTable dt_artists = ds.Tables[0];
            int m_dt_artists_ct = dt_artists.Rows.Count;

            var output = new StringBuilder();
            var outputCard = new StringBuilder();

            if (m_dt_artists_ct == 1)
            {
                foreach (DataRow dr in dt_artists.Rows)
                {
                    output.Append(dr["DisplayName"].ToString() + ", " + dr["DisplayDate"].ToString() + " has " + dr["ResultsCount"].ToString() + " works in the collection.");
                    outputCard.Append(dr["DisplayName"].ToString() + ", " + dr["DisplayDate"].ToString() + " has " + dr["ResultsCount"].ToString() + " works in the collection.");
                }
            }
            else if (m_dt_artists_ct > 1)
            {
                output.Append("There are multiple artists with a similar sounding name. ");
                outputCard.Append("There are multiple artists with a similar sounding name.\n");

                foreach (DataRow dr in dt_artists.Rows)
                {
                    output.Append(dr["DisplayName"].ToString() + ", " + dr["DisplayDate"].ToString() + " has " + dr["ResultsCount"].ToString() + " works in the collection. ");
                    outputCard.Append(dr["DisplayName"].ToString() + ", " + dr["DisplayDate"].ToString() + " has " + dr["ResultsCount"].ToString() + " works in the collection.\n ");
                }
            }

            sql_conn_data.Close();

            var response = new AlexaResponse(output.ToString());
            response.Response.Card.Title = "Artists";
            response.Response.Card.Type = "Standard";
            response.Response.Card.Image.SmallImageUrl = "https://api.moma.org/Content/Images/logo2.jpg";
            response.Response.Card.Text = outputCard.ToString();

            return response;
        }

        // Art
        private AlexaResponse ArtIntentHandler(Request request)
        {
            string artValue = "";
            string m_title = "";
            //string m_title2 = "";

            //// Names are done this way to allow for more precise results.

            //if (request.SlotsList.FirstOrDefault(s => s.Key == "StartTitle").Value != null)
            //    m_title1 = request.SlotsList.FirstOrDefault(s => s.Key == "StartTitle").Value.ToString();

            //if (request.SlotsList.FirstOrDefault(s => s.Key == "EndTitle").Value != null)
            //    m_title2 = request.SlotsList.FirstOrDefault(s => s.Key == "EndTitle").Value.ToString();

            //if (String.IsNullOrEmpty(m_title2))
            //{
            //    artValue = m_title1;
            //}
            //else
            //{
            //    artValue = m_title1 + " AND " + m_title2;
            //}

            if (request.SlotsList.FirstOrDefault(s => s.Key == "Title").Value != null)
                m_title = request.SlotsList.FirstOrDefault(s => s.Key == "Title").Value.ToString();

			// This exists because we use SQL Server Full-Text Index search
            artValue = m_title.Replace(" and ", " ").Replace(" ", " AND ");

            string conn_data = ConfigurationManager.ConnectionStrings["TMSConnectionString"].ConnectionString;
            SqlConnection sql_conn_data = new SqlConnection(conn_data);
            sql_conn_data.Open();

            // Get art.  OnView only.
            SqlCommand getAlexaArt = new SqlCommand("procMomaApiObjectSearch", sql_conn_data);
            getAlexaArt.CommandType = CommandType.StoredProcedure;
            getAlexaArt.Parameters.Clear();
            getAlexaArt.Parameters.Add(new SqlParameter(@"@p_searchterm", SqlDbType.VarChar, 255) { Value = artValue.ToString() });
            getAlexaArt.Parameters.Add(new SqlParameter(@"@p_searchtype", SqlDbType.VarChar, 255) { Value = "title" });
            getAlexaArt.Parameters.Add(new SqlParameter(@"@p_onview", SqlDbType.TinyInt) { Value = 1 });

            SqlDataAdapter sda = new SqlDataAdapter(getAlexaArt);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            sda.Dispose();

            DataTable dt_art = ds.Tables[0];
            int m_dt_art_ct = dt_art.Rows.Count;

            var output = new StringBuilder();
            var outputCard = new StringBuilder();
            string m_location;
            string m_current_location;

            if (m_dt_art_ct == 1)
            {
                foreach (DataRow dr in dt_art.Rows)
                {
                    m_current_location = dr["CurrentLocation"].ToString();

                    if (m_current_location == " ")
                    {
                        m_location = "The location is not available. ";
                    }
                    else {
                        m_location = "It is located on the " + m_current_location;
                    }

                    output.Append(dr["Title"].ToString() + " by " + dr["DisplayName"].ToString() + " dated " + dr["Dated"].ToString() + " is a " + dr["Classification"].ToString() + ". The medium is " + dr["Medium"].ToString() + ". " + dr["Creditline"].ToString() + ". " + m_location);  //"It is located on the " + dr["CurrentLocation"].ToString());
                    outputCard.Append(dr["Title"].ToString() + " by " + dr["DisplayName"].ToString() + " dated " + dr["Dated"].ToString() + " is a " + dr["Classification"].ToString() + ". The medium is " + dr["Medium"].ToString() + ". " + dr["Creditline"].ToString() + ". " + m_location + ".\n");  // It is located on the " + dr["CurrentLocation"].ToString());
                    m_image_art = dr["Thumbnail"].ToString();
                }
            }
            else if (m_dt_art_ct > 1)
            {
                outputCard.Append("There are multiple art works with a similar sounding name. \n ");

                foreach (DataRow dr in dt_art.Rows)
                {
                    m_current_location = dr["CurrentLocation"].ToString();
                     
                    if (m_current_location == " ")
                    {
                        m_location = "The location is not available. ";
                    }
                    else
                    {
                        m_location = "It is located on the " + m_current_location;
                    }

                    output.Append(dr["Title"].ToString() + " by " + dr["DisplayName"].ToString() + " dated " + dr["Dated"].ToString() + " is a " + dr["Classification"].ToString() + ". The medium is " + dr["Medium"].ToString() + ". " + dr["Creditline"].ToString() + ". " + m_location);
                    outputCard.Append(dr["Title"].ToString() + " by " + dr["DisplayName"].ToString() + " dated " + dr["Dated"].ToString() + " is a " + dr["Classification"].ToString() + ". The medium is " + dr["Medium"].ToString() + ". " + dr["Creditline"].ToString() + ". " + m_location + "\n");
                }
            }

            sql_conn_data.Close();

            var response = new AlexaResponse(output.ToString());
            response.Response.Card.Title = "Art";
            response.Response.Card.Type = "Standard";
            response.Response.Card.Image.SmallImageUrl = m_image_art;
            response.Response.Card.Text = outputCard.ToString();

            return response;
        }


        // Bathrooms
        private AlexaResponse BathroomsIntentHandler(Request request)
        {
            var response = new AlexaResponse("Restrooms are located on floors two, three, four, five, and six.  Family restrooms are located on the fifth floor and in Theater one.");
            response.Response.Card.Title = "Restrooms";
            response.Response.Card.Content = "Restrooms are located on floors 2, 3, 4, 5 and 6.  Family restrooms are located on the 5th floor and in Theater One.";
            response.Response.Card.Type = "Simple";

            return response;
        }

        // Hours
        private AlexaResponse HoursIntentHandler(Request request)
        {
            var response = new AlexaResponse("mow mah is open every day from ten thirty to five thirty with extended hours friday to eight pm.  admission is free on friday from four to eight pm.  please check our web site for special openings and exceptions.");
            response.Response.Card.Title = "Hours";
            response.Response.Card.Type = "Simple";
            response.Response.Card.Content = "MoMA is open every day from 10:30 AM to 5:30 PM with extended hours on Friday to 8 PM. Admission is free on Friday from 4 to 8 PM.  Please check our web site for special openings and exceptions.";

            return response;
        }

        // Directions
        private AlexaResponse DirectionsIntentHandler(Request request)
        {
            var response = new AlexaResponse("mow mah is located at 11 West Fifty-third Street, between Fifth and Sixth avenues. By subway. From the east side of Manhattan take the six train to fifty-first Street, transfer to the E or M train.  one stop to fifty-third Street and Fifth Avenue. From the west side of Manhattan, take the E or M train to fifty-third Street and Fifth Avenue, or B, D, or F train to Rockefeller Center");
            response.Response.Card.Title = "Directions";
            response.Response.Card.Type = "Simple";
            response.Response.Card.Content = "11 West 53rd Street, between 5th and 6th Avenue.\nBy subway.  From the east side of Manhattan take the 6 train to 51st Street, transfer to the E or M train.  one stop to 53rd Street and Fifth Avenue. From the west side of Manhattan, take the E or M train to 53rd Street and Fifth Avenue, or B, D, or F train to Rockefeller Center";

            return response;
        }

        // Food
        private AlexaResponse FoodIntentHandler(Request request)
        {
            var response = new AlexaResponse("There are cafes with a full menu on the second floor and the fifth floor.  For upscale dining, try the modern restaurant on the first floor.");
            response.Response.Card.Title = "Food";
            response.Response.Card.Type = "Simple";
            response.Response.Card.Content = "Cafe 2 & Expresso Bar is located on the 2nd floor.\nTerrace 5 Café is located on the 5th floor\nFor upscale dining, try The Modern restaurant located on the first floor.";

            return response;
        }

        // Stores
        private AlexaResponse StoresIntentHandler(Request request)
        {
            var response = new AlexaResponse("Our main stores are located on the first floor in the lobby and across the street at 44 west fifty-third street. We also have books on the second floor and a special exhibitions store on the sixth floor. You can also try our design store in soho at eighty-one spring street. Or visit us any time online at store dot mowmah dot org.");
            response.Response.Card.Title = "Shop";
            response.Response.Card.Type = "Simple";
            response.Response.Card.Content = "Design & Book Store - Floor 1\nBooks - Floor 2\nSpecial Exhibitions Store - Floor 6\nDesign Store - 44 West 53 Street\nDesign Store, Soho - 81 Spring Street\nOnline - store.moma.org";

            return response;
        }

    }

}

