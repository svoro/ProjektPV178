using System.Web;
using System.Web.Mvc;
/* This class is created as a part of default project.
 */
namespace ExpenseManager
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
