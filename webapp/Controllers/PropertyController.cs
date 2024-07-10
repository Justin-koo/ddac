using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapp.Areas.Identity.Data;
using webapp.Data;

namespace webapp.Controllers
{
    public class PropertyController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;

        public PropertyController(UserManager<webappUser> userManager, webappContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Submit()
        {
            return View();
        }
    }
}
