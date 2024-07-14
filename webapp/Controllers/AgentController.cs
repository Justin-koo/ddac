using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg; //used only for the Jpeg encoder below

namespace webapp.Controllers
{
    public class AgentController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AgentController(UserManager<webappUser> userManager, webappContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("agents", Name ="AgentList")]
        [HttpGet]
		public async Task<IActionResult> AgentList(AgentViewModel filters, int page = 1)
		{
			int pageSize = 10; // Set a fixed page size
			var usersInRole = await _userManager.GetUsersInRoleAsync("Agent");
			var userIds = usersInRole.Select(u => u.Id).ToList();

			var query = _userManager.Users.Where(u => userIds.Contains(u.Id)).AsQueryable();

			// Calculate the total number of agents
			var totalAgents = userIds.Count;

			// Apply filters
			if (!string.IsNullOrEmpty(filters.Location))
			{
				query = query.Where(u => u.City.Contains(filters.Location) || u.State.Contains(filters.Location));
			}

			if (!string.IsNullOrEmpty(filters.Name))
			{
				query = query.Where(u => u.FullName.Contains(filters.Name));
			}

			// Calculate the total number of filtered agents
			var totalFilteredAgents = await query.CountAsync();

			var agents = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(u => new AgentViewModel
				{
					Id = u.Id,
					Name = u.FullName,
					Email = u.Email,
					PhoneNumber = u.PhoneNumber,
					About = u.About,
					City = u.City,
					State = u.State,
					FacebookLink = u.FacebookLink,
					XLink = u.XLink,
					LinkedInLink = u.LinkedInLink,
					PropertyCount = _context.Properties.Count(p => p.AgentId == u.Id),
					Location = filters.Location
				})
				.ToListAsync();

			ViewData["Title"] = "Find an Agent";
			ViewBag.Filters = filters;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;
			ViewBag.TotalAgents = totalAgents;
			ViewBag.TotalFilteredAgents = totalFilteredAgents;
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalFilteredAgents / pageSize);

			return View(agents);
		}


		[HttpGet]
        public async Task<JsonResult> GetLocations(string term)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Agent");
            var userIds = usersInRole.Select(u => u.Id).ToList();

            var locations = _userManager.Users
                .Where(u => userIds.Contains(u.Id) && (u.City.Contains(term) || u.State.Contains(term)))
                .Select(u => new
                {
                    label = u.City + ", " + u.State,
                    value = u.City
                })
                .Distinct()
                .ToList();

            return Json(locations);
        }

        [Route("agents/details")]
		[HttpGet]
		public IActionResult AgentDetails()
		{
			ViewData["Title"] = "Agent Details";
			return View();
		}

        [HttpGet]
		public async Task<IActionResult> PropertyDetails(int id)
        {
			var property_details = await _context.Properties
				.Include(p => p.Address)
				.Include(p => p.Detail)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (property_details == null)
			{
				return NotFound();
			}

			var viewModel = new PropertyDetailsViewModel
			{
				Property = property_details
			};

			ViewData["Title"] = property_details.Title;
			return View(viewModel);
		}


        //[Authorize(Roles = "Agent")]
        [HttpGet]
        public IActionResult SubmitProperty()
        {
            ViewData["Title"] = "Submit Property";
            return View(new PropertyViewModel());
        }

        //[Authorize(Roles = "Agent")]
        [HttpPost]
        public async Task<IActionResult> SubmitProperty(PropertyViewModel model, List<IFormFile> files)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                      .Select(e => e.ErrorMessage)
                                      .ToList();

                return Json(new
                {
                    success = false,
                    errors
                });
            }

            var user = await _userManager.GetUserAsync(User);

            var property = new Property
            {
                Title = model.Title,
                Status = model.Status,
                PropertyType = model.PropertyType,
                Price = model.Price,
                Area = model.Area,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                ListingDate = model.ListingDate,
                AgentId = user.Id,
                SubmissionStatus = "Pending",
                // GalleryPath = string.Join(";", model.GalleryPath),
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            var propertyIdString = property.Id.ToString();

            //Console.WriteLine("Files: " + (files != null ? files.Count.ToString() : "null"));
            var uploadUrls = new List<string>();
             var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "property", propertyIdString.ToSHA256String());

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            foreach (var file in files)
            {
                //Console.WriteLine("In handle file");
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + ".jpeg";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var inStream = file.OpenReadStream())
                    using (Image image = Image.Load(inStream))
                    {
                        image.Mutate(x => x.Resize(1920, 1250));

                        var encoder = new JpegEncoder
                        {
                            Quality = 75
                        };

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.SaveAsJpegAsync(fileStream, encoder);
                        }
                    }
                    uploadUrls.Add(uniqueFileName); // or a URL if needed
                }
            }

            model.GalleryPath = uploadUrls;

            // Update property with gallery path
            property.GalleryPath = string.Join(";", model.GalleryPath);
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            var propertyAddress = new PropertyAddress
            {
                AddressLine = model.AddressLine,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                PropertyId = property.Id
            };

            _context.PropertyAddresses.Add(propertyAddress);
            await _context.SaveChangesAsync();

            var propertyDetail = new PropertyDetail
            {
                Description = model.Description,
                BuildingAge = model.BuildingAge,
                Garage = model.Garage,
                Rooms = model.Rooms,
                OtherFeatures = string.Join(";", model.Features),
                PropertyId = property.Id
            };

            _context.PropertyDetails.Add(propertyDetail);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Property created successfully!";
            var redirectUrl = Url.Action("SubmitProperty", "Agent");

            return Json(new
            {
                success = true,
                redirectUrl
            });
            //var errors = ModelState
            //        .Where(ms => ms.Value.Errors.Count > 0)
            //        .Select(ms => new
            //        {
            //            Field = ms.Key,
            //            Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            //        });

            //    // Optionally log the errors
            //    foreach (var error in errors)
            //    {
            //        Console.WriteLine($"Field: {error.Field}");
            //        foreach (var errorMessage in error.Errors)
            //        {
            //            Console.WriteLine($"Error: {errorMessage}");
            //        }
            //    }

            //return View(model);
        }

        [Authorize(Roles = "Agent")]
        [HttpGet]
        public async Task<IActionResult> AgentPropertyList()
        {
            var properties = await _context.Properties
                .Include(p => p.Address)
                .Include(p => p.Detail)
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
                SubmissionStatus = p.SubmissionStatus,
            }).ToList();

            ViewData["Title"] = "My Property";
            return View(propertyViewModels);
        }

        [Authorize(Roles = "Agent")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            try
            {
                var property = await _context.Properties.FindAsync(id);
                if (property == null)
                {
                    TempData["Message"] = "Error: Property not found.";
                    return RedirectToAction("AgentPropertyList");
                }

                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Property deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: Unable to delete property.";
                // Log the exception (ex) here if necessary
            }

            return RedirectToAction("AgentPropertyList");
        }
    }
}
