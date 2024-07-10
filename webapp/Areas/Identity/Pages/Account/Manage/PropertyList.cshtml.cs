using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace webapp.Areas.Identity.Pages.Account.Manage
{
    public class PropertyListModel : PageModel
    {
        public void OnGet()
        {
			ViewData["ActivePage"] = ManageNavPages.PropertyList;
		}
    }
}
