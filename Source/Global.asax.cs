using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ezyd.Infrastructure;
using System.Threading;
using System.Globalization;
using System.Resources;
using MvcGlobalisationSupport;

namespace ezyd
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LogAttribute());
        }

        /* OLD ONE
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
                new string[] { "ezyd.Controllers" }
            );

        }
        */

        //from http://www.codeproject.com/Articles/207602/Creating-a-Bilingual-ASP-NET-MVC3-Application-Part
        public static void RegisterRoutes(RouteCollection routes)
        {
            const string defautlRouteUrl = "{controller}/{action}/{id}";
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteValueDictionary defaultRouteValueDictionary = new RouteValueDictionary(
                    new { controller = "Home", action = "Index", id = UrlParameter.Optional });
            Route defaultRoute = new Route(defautlRouteUrl,
                  defaultRouteValueDictionary, new MvcRouteHandler());
            routes.Add("DefaultGlobalised",
                       new GlobalisedRoute(defaultRoute.Url, defaultRoute.Defaults));
            routes.Add("Default", new Route(defautlRouteUrl,
                       defaultRouteValueDictionary, new MvcRouteHandler()));
        }


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        //this function is being used
        //from http://www.codeproject.com/Articles/207602/Creating-a-Bilingual-ASP-NET-MVC3-Application-Part
        //modified to pl from ar(abic)
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //Note everything hardcoded, for simplicity!
            if (Request.UserLanguages.Length == 0)
                return;

            string language = Request.UserLanguages[0];
            if (language.Substring(0, 2).ToLower() == "pl")
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl");
        }
    }
}