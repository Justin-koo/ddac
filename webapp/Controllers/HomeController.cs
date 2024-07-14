using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Models;
using webapp.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Mvc.ApplicationModels;


namespace webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
                private readonly webappContext _context;

        public HomeController(ILogger<HomeController> logger, webappContext context)
        {
            _logger = logger;
            _context = context;
        }

        // public IActionResult Index()
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
            .Include(p => p.Address)
            .Include(p => p.Detail)
			.ToListAsync();

			var viewModel = new HomepageViewModel
			{
				HomepagePropertyViewModel = properties.Select(p => new PropertyViewModel
				{
					Id = p.Id,
					Title = p.Title,
					Status = p.Status,
					PropertyType = p.PropertyType,
					Price = p.Price,
					Area = p.Area,
					Bedrooms = p.Bedrooms,
					Bathrooms = p.Bathrooms,
					GalleryPath = p.GalleryPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
					ListingDate = p.ListingDate,
					AgentId = p.AgentId,
					AddressLine = p.Address.AddressLine,
					City = p.Address.City,
					State = p.Address.State,
					ZipCode = p.Address.ZipCode,
					Description = p.Detail.Description,
					BuildingAge = p.Detail.BuildingAge,
					Garage = p.Detail.Garage,
					Rooms = p.Detail.Rooms,
					Features = p.Detail.OtherFeatures?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
					GalleryFolder = p.Id.ToString().ToSHA256String(),
					SubmissionStatus = p.SubmissionStatus,
				}).ToList()
			};

			ViewData["Title"] = "My Property";
            return View(viewModel);
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
