using System.Web;
using System.Web.Mvc;
#pragma warning disable 1591

namespace TextIt
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
