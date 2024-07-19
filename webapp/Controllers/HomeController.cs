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
using System.Drawing.Printing;
using Mono.TextTemplating;
using static NuGet.Packaging.PackagingConstants;


namespace webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly webappContext _context;
        private readonly UserManager<webappUser> _userManager;

        private const int PageSize = 10; // Number of items per page


        public HomeController(ILogger<HomeController> logger, webappContext context, UserManager<webappUser> userManager)
        {
			_userManager = userManager;
			_logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // public IActionResult Index()
        public async Task<IActionResult> Index()
        {
			var savedPropertyIds = new List<int>();
			var user = await _userManager.GetUserAsync(User);


			if (user != null)
			{
				savedPropertyIds = await _context.PropertySave
					.Where(ps => ps.UserId == user.Id)
					.Select(ps => ps.PropertyId)
					.ToListAsync();
			}

			var properties = await _context.Properties
            .Include(p => p.Address)
            .Include(p => p.Detail)
			.Where(p => p.ListingStatus == "Active")
			.OrderByDescending(p => p.ListingDate) // Sort by ListingDate in descending order
		    .Take(8)
			.ToListAsync();

			var propertyViewModels = properties.Select(p => new PropertyViewModel
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
				ListingStatus = p.ListingStatus,
			}).ToList();

			var topStates = properties
		        .GroupBy(p => p.Address.State)
		        .OrderByDescending(g => g.Count())
		        .Take(4)
		        .Select(g => new StateViewModel
		        {
			        State = g.Key,
			        PropertyCount = g.Count()
		        })
		        .ToList();

            var usersInRole = await _userManager.GetUsersInRoleAsync("Agent");
            var userIds = usersInRole.Select(u => u.Id).ToList();
            var query = _userManager.Users.Where(u => userIds.Contains(u.Id)).AsQueryable();

            var agents = await query
                .Select(u => new AgentViewModel
                {
                    //Id = u.Id,
                    UserName = u.UserName,
                    Name = u.FullName,
                    //Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    About = u.About,
                    City = u.City,
                    State = u.State,
                    //FacebookLink = u.FacebookLink,
                    //XLink = u.XLink,
                    //LinkedInLink = u.LinkedInLink,
                    //GoogleLink = u.GoogleLink,
                    PropertyCount = _context.Properties.Count(p => p.AgentId == u.Id),
                    Location = u.City + ", " + u.State,
                    ProfilePicture = u.ProfilePicture,
                })
				.Take(8)
                .ToListAsync();

            var viewModel = new HomepageViewModel
			{
				HomepagePropertyViewModel = propertyViewModels,
				States = topStates,
                Agents = agents,
                SavedPropertyIds = savedPropertyIds
            };

			ViewData["Title"] = "My Property";
            return View(viewModel);
        }


		[HttpGet]
		public async Task<IActionResult> GetLocationSuggestions(string query)
		{
			var locations = await _context.Properties
				.Include(p => p.Address)
				.Where(p => p.Address.City.Contains(query) ||
							p.Address.State.Contains(query) ||
							p.Address.AddressLine.Contains(query))
				.Select(p => $"{p.Address.AddressLine}, {p.Address.City}, {p.Address.State}")
				.Distinct()
				.ToListAsync();

			return Json(locations);
		}

        [HttpGet]
        public async Task<IActionResult> SearchResults(string query, string liststatus, string city, string state, string status, string types, int bedrooms, int bathrooms, int garage, string builtYear, List<int> features, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            var savedPropertyIds = new List<int>();
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                savedPropertyIds = await _context.PropertySave
                    .Where(ps => ps.UserId == user.Id)
                    .Select(ps => ps.PropertyId)
                    .ToListAsync();
            }

            var propertiesQuery = _context.Properties
                .Include(p => p.Address)
                .Include(p => p.Detail)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                var queryTerms = query.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(term => term.Trim().ToLower())
                                      .ToList();

                foreach (var term in queryTerms)
                {
                    propertiesQuery = propertiesQuery.Where(p => p.Address.City.ToLower().Contains(term) ||
                                                                 p.Address.State.ToLower().Contains(term) ||
                                                                 p.Address.AddressLine.ToLower().Contains(term));
                }
            }

            if (!string.IsNullOrEmpty(city))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Address.City.ToLower() == city.ToLower());
            }

            if (!string.IsNullOrEmpty(state))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Address.State.ToLower() == state.ToLower());
            }

            if (!string.IsNullOrEmpty(status))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Status.ToLower() == status.ToLower());
            }

            if (!string.IsNullOrEmpty(types))
            {
                propertiesQuery = propertiesQuery.Where(p => p.PropertyType.ToLower() == types.ToLower());
            }

            if (bedrooms > 0)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Bedrooms >= bedrooms);
            }

            if (bathrooms > 0)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Bathrooms >= bathrooms);
            }

            if (garage > 0)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Detail.Garage == garage);
            }

            if (!string.IsNullOrEmpty(builtYear))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Detail.BuildingAge == builtYear);
            }

            if (features != null && features.Count > 0)
            {
                propertiesQuery = propertiesQuery.Where(p => features.All(f => p.Detail.OtherFeatures.Contains(f.ToString())));
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.Price <= maxPrice.Value);
            }

            propertiesQuery = propertiesQuery.Where(p => p.ListingStatus == "Active");

            // Get the total count of active properties based on the filters
            var activePropertiesCount = await propertiesQuery.CountAsync();

            var totalProperties = await propertiesQuery.CountAsync();
            var properties = await propertiesQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Ensure the view model is constructed even if there are no properties
            var viewModel = new SearchResultViewModel
            {
                Properties = properties.Select(p => new PropertyViewModel
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
                    GalleryFolder = p.Id.ToString().ToSHA256String(),
                    ListingStatus = p.ListingStatus,
                }).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalProperties / (double)PageSize),
                Query = query,
                City = city,
                state = state,
                status = status,
                types = types,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                Garage = garage,
                BuiltYear = builtYear,
                SelectedFeatures = features ?? new List<int>(),
                Features = await _context.Features.ToListAsync(),
                MinPrice = propertiesQuery.Any() ? await propertiesQuery.MinAsync(p => p.Price) : 0,
                MaxPrice = propertiesQuery.Any() ? await propertiesQuery.MaxAsync(p => p.Price) : 0,
                PriceRange = $"{minPrice};{maxPrice}",
                SavedPropertyIds = savedPropertyIds,
                states = await _context.Properties.Select(p => p.Address.State).Distinct().ToListAsync(),
                Cities = await _context.Properties.Select(p => p.Address.City).Distinct().ToListAsync(),
                statusList = await _context.Properties.Select(p => p.Status).Distinct().ToListAsync(),
                PropertyTypes = await _context.Properties.Select(p => p.PropertyType).Distinct().ToListAsync(),
                ActivePropertiesCount = activePropertiesCount,
            };
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
