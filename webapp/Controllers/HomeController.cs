using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using webapp.Models;

namespace webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
			return View();
        }

		[Route("about")]
		public IActionResult About()
		{
			return View();
		}

        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("faq")]
        public IActionResult Faq()
		{
			return View();
		}

        [Route("contact-us")]
        public IActionResult Contact()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
