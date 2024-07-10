using Microsoft.AspNetCore.Mvc;

namespace webapp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToPage("Identity/Account/Manage/Index");
        }
    }
}
