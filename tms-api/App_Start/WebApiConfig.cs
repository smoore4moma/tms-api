using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;


namespace tms_api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // This will enable cross-ownership requests SMOORE
            // Use NuGet to add this: http://www.nuget.org/packages/Microsoft.AspNet.WebApi.Cors
            config.EnableCors();

            // Use camel case for JSON data.
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // These two lines remove XML as the default (instead uses JSON).  You can still force XML with Accept: text/xml in the request. SMOORE
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("type", "json", new MediaTypeHeaderValue("application/json")));

            config.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("type", "xml", new MediaTypeHeaderValue("application/xml")));

            // Web API routes
            config.MapHttpAttributeRoutes();

            // ******************* IMPORTANT ********************//

            // Rename your routeTemplate
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
			
			// Alexa route with validation handler
			config.Routes.MapHttpRoute(
            name: "AlexaApi",
            routeTemplate: "api/alexa/{id}",
            defaults: null,
            constraints: null,
            handler: new Handlers.AlexaRequestValidationHandler()
			

        }
    }
}
