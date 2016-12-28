using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Test
{
    using System.Data.SqlClient;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            using (var sqlconn = new SqlConnection("Data Source=g1sa681vaf.database.chinacloudapi.cn,1433;Initial Catalog=sampledb1;Persist Security Info=True;User ID=micdev;Password=mic@2016;MultipleActiveResultSets=True;Encrypt=True"))
            {
                if (sqlconn.State != System.Data.ConnectionState.Open)
                {
                    sqlconn.Open();
                }
            }
        }
    }
}
