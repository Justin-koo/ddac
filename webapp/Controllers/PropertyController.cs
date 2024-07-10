using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Models;
using webapp.Areas.Identity.Pages.Account.Manage;

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

        public IActionResult SubmitProperty()
        {
            ViewData["Title"] = "Submit Property";
            ViewData["ActivePage"] = ManageNavPages.SubmitProperty;
            return View(new PropertyViewModel());
        }
    }
}
