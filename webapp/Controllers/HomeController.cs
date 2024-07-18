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


namespace webapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
                private readonly webappContext _context;
        private const int PageSize = 10; // Number of items per page


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

			var viewModel = new HomepageViewModel
			{
				HomepagePropertyViewModel = propertyViewModels,
				States = topStates
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
				.Select(p => $"{p.Address.AddressLine}, {p.Address.City}, {p.Address.State}, {p.Address.ZipCode}")
				.Distinct()
				.ToListAsync();

			return Json(locations);
		}

		[HttpGet]
		public async Task<IActionResult> SearchResults(string query, string city, string state, string status, string types, int bedrooms, int bathrooms, int garage, string builtYear, List<int> features, decimal? minPrice, decimal? maxPrice, int page = 1)
		{
			var propertiesQuery = _context.Properties
				.Include(p => p.Address)
				.Include(p => p.Detail)
				.AsQueryable();

			if (!string.IsNullOrEmpty(query))
			{
				var queryTerms = query.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
									  .Select(term => term.Trim())
									  .ToList();

				foreach (var term in queryTerms)
				{
					propertiesQuery = propertiesQuery.Where(p => p.Address.City.Contains(term) ||
																 p.Address.State.Contains(term) ||
																 p.Address.AddressLine.Contains(term));
				}
			}

			if (!string.IsNullOrEmpty(city))
			{
				propertiesQuery = propertiesQuery.Where(p => p.Address.City == city);
			}

            if (!string.IsNullOrEmpty(state))
            {
                propertiesQuery = propertiesQuery.Where(p => p.Address.State == state);
            }

            if (!string.IsNullOrEmpty(status))
			{
				propertiesQuery = propertiesQuery.Where(p => p.Status == status);
			}

            if (!string.IsNullOrEmpty(types))
            {
                propertiesQuery = propertiesQuery.Where(p => p.PropertyType == types);
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

			// Calculate min and max price
			decimal minPriceAvailable = 0;
			decimal maxPriceAvailable = 0;
			if (await propertiesQuery.AnyAsync())
			{
				minPriceAvailable = await propertiesQuery.MinAsync(p => p.Price);
				maxPriceAvailable = await propertiesQuery.MaxAsync(p => p.Price);
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

            // Count active properties
            int activePropertiesCount = await _context.Properties
                .Where(p => p.ListingStatus == "Active")
                .CountAsync();

            var totalProperties = await propertiesQuery.CountAsync();
			var properties = await propertiesQuery
				.Skip((page - 1) * PageSize)
				.Take(PageSize)
				.ToListAsync();

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
				MinPrice = minPriceAvailable,
				MaxPrice = maxPriceAvailable,
				PriceRange = $"{minPrice};{maxPrice}",
                
                states = await _context.Properties.Select(p => p.Address.State).Distinct().ToListAsync(),
                Cities = await _context.Properties.Select(p => p.Address.City).Distinct().ToListAsync(),
                statusList = await _context.Properties.Select(p => p.Status).Distinct().ToListAsync(),
                PropertyTypes = await _context.Properties.Select(p => p.PropertyType).Distinct().ToListAsync(),
                ActivePropertiesCount = activePropertiesCount
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
