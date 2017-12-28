using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebMatrix.WebData;
using System.Web.Optimization;

namespace PumpVisualizer
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            bool isCreateAuto = ConfigurationManager.AppSettings["EnableCreateAuthTable"] == "1";

            //if(!WebSecurity.Initialized)
            //{
            WebSecurity.InitializeDatabaseConnection("AuthConnection", "users", "UserId", "UserName", isCreateAuto);
            //}


            if (!WebSecurity.UserExists("admin"))
                WebSecurity.CreateUserAndAccount("admin", "admin", new { Description = "super admin" });
            if (!Roles.RoleExists("administrators"))
            {
                Roles.CreateRole("administrators");
                if (WebSecurity.UserExists("admin"))
                {
                    if (Roles.FindUsersInRole("administrators", "admin").Count() == 0)
                        Roles.AddUserToRole("admin", "administrators");
                }
            }


        }
    }
}