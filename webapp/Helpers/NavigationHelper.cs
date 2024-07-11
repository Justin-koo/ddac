using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

namespace webapp.Helpers
{
    public static class NavigationHelper
    {
        public static bool IsPageActive(ViewContext viewContext, string pageName)
        {
            var currentPage = viewContext.RouteData.Values["page"]?.ToString();
            return string.Equals(currentPage, pageName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsControllerActive(ViewContext viewContext, string controllerName)
        {
            var currentController = viewContext.RouteData.Values["controller"]?.ToString();
            return string.Equals(currentController, controllerName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsControllerAndActionActive(ViewContext viewContext, string controllerName, string actionName)
        {
            var currentController = viewContext.RouteData.Values["controller"]?.ToString();
            var currentAction = viewContext.RouteData.Values["action"]?.ToString();
            return string.Equals(currentController, controllerName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(currentAction, actionName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
