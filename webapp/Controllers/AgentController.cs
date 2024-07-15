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
using SixLabors.ImageSharp.Formats.Jpeg;
using webapp.Helpers; //used only for the Jpeg encoder below

namespace webapp.Controllers
{
    public class AgentController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EncryptionHelper _encryptionHelper;

        public AgentController(UserManager<webappUser> userManager, webappContext context, IWebHostEnvironment webHostEnvironment, EncryptionHelper encryptionHelper)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _encryptionHelper = encryptionHelper;
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
                    UserName = u.UserName,
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

        [Route("agents/{username}")]
        [HttpGet]
		public async Task<IActionResult> AgentDetails(string username)
		{
			if (string.IsNullOrEmpty(username))
			{
				return RedirectToAction("AgentList");
			}

			var agent = await _userManager.Users
			.FirstOrDefaultAsync(u => u.UserName == username);

			if (agent == null)
			{
				return RedirectToAction("AgentList");
			}

			var isAgent = await _userManager.IsInRoleAsync(agent, "Agent");
			if (!isAgent)
			{
				return RedirectToAction("AgentList");
			}

			var properties = await _context.Properties
			.Where(p => p.AgentId == agent.Id)
			.Select(p => new PropertyViewModel
			{
				Id = p.Id,
				Title = p.Title,
				Status = p.Status,
				Price = p.Price,
				Area = p.Area,
				Bedrooms = p.Bedrooms,
				Bathrooms = p.Bathrooms,
				GalleryPath = p.GalleryPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
				AddressLine = p.Address.AddressLine,
				City = p.Address.City,
				State = p.Address.State,
				ZipCode = p.Address.ZipCode,
				BuildingAge = p.Detail.BuildingAge,
				GalleryFolder = p.Id.ToString().ToSHA256String(),
                ListingStatus = p.ListingStatus,
			})
			.ToListAsync();

			var model = new AgentViewModel
			{
				Id = agent.Id,
				Name = agent.FullName,
				Email = agent.Email,
				PhoneNumber = agent.PhoneNumber,
				About = agent.About,
				City = agent.City,
				State = agent.State,
				FacebookLink = agent.FacebookLink,
				XLink = agent.XLink,
				LinkedInLink = agent.LinkedInLink,
				Properties = properties
				//PropertyCount = _context.Properties.Count(p => p.AgentId == agent.Id)
			};

			ViewData["Title"] = "Agent Details";
			return View(model);
		}

        [HttpGet]
		public async Task<IActionResult> PropertyDetails(int id)
        {
			var property = await _context.Properties
			    .Include(p => p.Address)
			    .Include(p => p.Detail)
			    .FirstOrDefaultAsync(p => p.Id == id);

			if (property == null)
			{
				return NotFound();
			}

			var propertyViewModel = new PropertyViewModel
			{
				Id = property.Id,
				Title = property.Title,
				Status = property.Status,
				PropertyType = property.PropertyType,
				Price = property.Price,
				Area = property.Area,
				Bedrooms = property.Bedrooms,
				Bathrooms = property.Bathrooms,
				GalleryPath = property.GalleryPath?.Split(";").ToList() ?? new List<string>(),
				ListingDate = property.ListingDate,
				AgentId = property.AgentId,
				AddressLine = property.Address.AddressLine,
				City = property.Address.City,
				State = property.Address.State,
				ZipCode = property.Address.ZipCode,
				Description = property.Detail.Description,
				BuildingAge = property.Detail.BuildingAge,
				Garage = property.Detail.Garage,
				Rooms = property.Detail.Rooms,
                ListingStatus = property.ListingStatus
            };

			var propertyUpdates = await _context.PropertyUpdate
	            .Where(p => p.PropertyId == id)  // Assuming 'PropertyId' is the foreign key linking to the property
	            .ToListAsync();


			var viewModel = new PropertyDetailsViewModel
            {
                Property = propertyViewModel,
				Features = await _context.Features.ToListAsync(),
                SelectedFeatures = [.. property.Detail.OtherFeatures.Split(";")],
                PropertyUpdates = propertyUpdates,
		    };

			ViewData["Title"] = property.Title;
			return View(viewModel);
		}


        //[Authorize(Roles = "Agent")]
        [HttpGet]
        public async Task<IActionResult> SubmitProperty()
        {
            var viewModel = new PropertyDetailsViewModel
            {
                Property = new PropertyViewModel(),
                Features = await _context.Features.ToListAsync(),
                SelectedFeatures = new List<string>(),
            };
            ViewData["Title"] = "Submit Property";
            return View(viewModel);
        }

        //[Authorize(Roles = "Agent")]
        [HttpPost]
        public async Task<IActionResult> SubmitProperty(PropertyDetailsViewModel model, List<IFormFile> files)
        {

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Invalid");
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                      .Select(e => e.ErrorMessage)
                                      .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                return View(model);
                //return Json(new
                //{
                //    success = false,
                //    errors
                //});
            }

            var user = await _userManager.GetUserAsync(User);

            var property = new Property
            {
                Title = model.Property.Title,
                Status = model.Property.Status,
                PropertyType = model.Property.PropertyType,
                Price = model.Property.Price,
                Area = model.Property.Area,
                Bedrooms = model.Property.Bedrooms,
                Bathrooms = model.Property.Bathrooms,
                ListingDate = model.Property.ListingDate,
                AgentId = user.Id,
                ListingStatus = "Active",
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

            // Update property with gallery path
            property.GalleryPath = string.Join(";", uploadUrls);
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            var propertyAddress = new PropertyAddress
            {
                AddressLine = model.Property.AddressLine,
                City = model.Property.City,
                State = model.Property.State,
                ZipCode = model.Property.ZipCode,
                PropertyId = property.Id
            };

            _context.PropertyAddresses.Add(propertyAddress);
            await _context.SaveChangesAsync();

            var propertyDetail = new PropertyDetail
            {
                Description = model.Property.Description,
                BuildingAge = model.Property.BuildingAge,
                Garage = model.Property.Garage,
                Rooms = model.Property.Rooms,
                OtherFeatures = string.Join(";", model.SelectedFeatures),
                PropertyId = property.Id
            };

            _context.PropertyDetails.Add(propertyDetail);
            await _context.SaveChangesAsync();

			var propertyUpdate = new PropertyUpdate
			{
                Status = model.Property.Status,
                Price = model.Property.Price,
				PropertyId = property.Id,
				UpdateDate = property.ListingDate,
			};

			_context.PropertyUpdate.Add(propertyUpdate);
			await _context.SaveChangesAsync();

			TempData["Message"] = "Property created successfully!";
            var redirectUrl = Url.Action("SubmitProperty", "Agent");

            return RedirectToAction(nameof(AgentPropertyList), new { username = user.UserName });

            //return Json(new
            //{
            //    success = true,
            //    redirectUrl
            //});

            //return View(model);
        }

        [Authorize(Roles = "Agent")]
        [HttpGet]
        [Route("{username}/property-listing")]
        public async Task<IActionResult> AgentPropertyList(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.UserName != username)
            {
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username);

            var properties = await _context.Properties
                .Include(p => p.Address)
                .Include(p => p.Detail)
                .Where(p => p.AgentId == user.Id)
                .ToListAsync();

            var propertyViewModels = properties.Select(p => new PropertyViewModel
            {
                //Id = p.Id,
                EncryptedId = _encryptionHelper.Encrypt(p.Id.ToString()),
                Title = p.Title,
                //Status = p.Status,
                //PropertyType = p.PropertyType,
                Price = p.Price,
                //Area = p.Area,
                //Bedrooms = p.Bedrooms,
                //Bathrooms = p.Bathrooms,
                GalleryPath = p.GalleryPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                ListingDate = p.ListingDate,
                //AgentId = p.AgentId,
                AddressLine = p.Address.AddressLine,
                //City = p.Address.City,
                //State = p.Address.State,
                //ZipCode = p.Address.ZipCode,
                //Description = p.Detail.Description,
                //BuildingAge = p.Detail.BuildingAge,
                //Garage = p.Detail.Garage,
                //Rooms = p.Detail.Rooms,
                //Features = p.Detail.OtherFeatures?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                GalleryFolder = p.Id.ToString().ToSHA256String(),
                ListingStatus = p.ListingStatus,
            }).ToList();

            ViewData["Title"] = "My Property";
			ViewData["User"] = user;
			return View(propertyViewModels);
        }

        [Authorize(Roles = "Agent")]
        [HttpGet]
        [Route("{username}/edit/{encryptedId}")]
        public async Task<IActionResult> EditProperty(string username, string encryptedId)
		{
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.UserName != username)
            {
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            int id;
			try
			{
				id = int.Parse(_encryptionHelper.Decrypt(encryptedId));
			}
			catch
			{
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username);

            var property = await _context.Properties
                .Include(p => p.Address)
                .Include(p => p.Detail)
                .SingleOrDefaultAsync(p => p.Id == id && p.AgentId == user.Id);

            if (property == null)
            {
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            var viewModel = new PropertyDetailsViewModel
			{
				Property = new PropertyViewModel
                {
                    //Id = property.Id,
                    EncryptedId = encryptedId,
                    Title = property.Title,
                    Status = property.Status,
                    PropertyType = property.PropertyType,
                    Price = property.Price,
                    Area = property.Area,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = property.Bathrooms,
                    GalleryPath = property.GalleryPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    //ListingDate = property.ListingDate,
                    AddressLine = property.Address.AddressLine,
                    City = property.Address.City,
                    State = property.Address.State,
                    ZipCode = property.Address.ZipCode,
                    Description = property.Detail.Description,
                    BuildingAge = property.Detail.BuildingAge,
                    Garage = property.Detail.Garage,
                    Rooms = property.Detail.Rooms,
                    Features = property.Detail.OtherFeatures?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                    GalleryFolder = property.Id.ToString().ToSHA256String(),
                    //ListingStatus = property.ListingStatus,
                },
                Features = await _context.Features.ToListAsync(),
                SelectedFeatures = property.Detail.OtherFeatures?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            };

			ViewData["Title"] = "Edit Property";
            ViewData["User"] = user;
            return View(viewModel);
		}

        [Authorize(Roles = "Agent")]
        [HttpPost]
        [Route("{username}/edit/{encryptedId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(string username, string encryptedId, PropertyDetailsViewModel viewModel, List<IFormFile> GalleryFiles)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.UserName != username)
            {
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            int id;
            try
            {
                id = int.Parse(_encryptionHelper.Decrypt(encryptedId));
            }
            catch
            {
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            if (!ModelState.IsValid)
            {
                // Reload features if the model state is not valid
                viewModel.Features = await _context.Features.ToListAsync();
                return View(viewModel);
            }

            var property = await _context.Properties
                .Include(p => p.Address)
                .Include(p => p.Detail)
                .SingleOrDefaultAsync(p => p.Id == id && p.AgentId == currentUser.Id);

            if (property == null)
            {
                return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });
            }

            property.Title = viewModel.Property.Title;
            property.Price = viewModel.Property.Price;
            property.Status = viewModel.Property.Status;
            property.PropertyType = viewModel.Property.PropertyType;
            property.Price = viewModel.Property.Price;
            property.Area = viewModel.Property.Area;
            property.Bedrooms = viewModel.Property.Bedrooms;
            property.Bathrooms = viewModel.Property.Bathrooms;
            property.Address.AddressLine = viewModel.Property.AddressLine;
            property.Address.City = viewModel.Property.City;
            property.Address.State = viewModel.Property.State;
            property.Address.ZipCode = viewModel.Property.ZipCode;
            property.Detail.Description = viewModel.Property.Description;
            property.Detail.BuildingAge = viewModel.Property.BuildingAge;
            property.Detail.Garage = viewModel.Property.Garage;
            property.Detail.Rooms = viewModel.Property.Rooms;
            property.Detail.OtherFeatures = string.Join(";", viewModel.SelectedFeatures);

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "property", property.Id.ToString().ToSHA256String());
            var existingGalleryPaths = property.GalleryPath?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var newGalleryPaths = new List<string>(existingGalleryPaths);

            // Handle file uploads
            if (GalleryFiles != null && GalleryFiles.Count > 0)
            {
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var formFile in GalleryFiles)
                {
                    if (formFile.Length > 0 && !existingGalleryPaths.Contains(formFile.FileName))
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }

                        newGalleryPaths.Add(uniqueFileName);
                    }
                }
            }

            // Identify files to delete
            var currentGalleryPaths = GalleryFiles?.Select(f => f.FileName).ToList() ?? new List<string>();
            var filesToDelete = existingGalleryPaths.Except(currentGalleryPaths).ToList();

            // Delete removed files
            foreach (var fileToDelete in filesToDelete)
            {
                var filePath = Path.Combine(uploadsFolder, fileToDelete);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Remove the deleted file from the new gallery paths
                newGalleryPaths.Remove(fileToDelete);
            }

            // Update the gallery path
            property.GalleryPath = string.Join(";", newGalleryPaths);

            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            var propertyUpdate = new PropertyUpdate
            {
                Status = property.Status,
                Price = property.Price,
                PropertyId = property.Id,
                UpdateDate = DateTime.Now,
			};

			_context.PropertyUpdate.Add(propertyUpdate);
			await _context.SaveChangesAsync();

			TempData["Message"] = "Property updated successfully.";
            return RedirectToAction(nameof(AgentPropertyList), new { username = currentUser.UserName });

            //viewModel.Features = await _context.Features.ToListAsync();
            ////viewModel.Property.Id = property.Id;
            //viewModel.Property.EncryptedId = encryptedId; // Preserve the encrypted ID

            //ViewData["Title"] = "Edit Property";
            //ViewData["User"] = currentUser;
            //return View(viewModel);
        }

        [HttpPost]
		public async Task<IActionResult> UnlistProperty(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			var property = await _context.Properties
				.Where(p => p.Id == id && p.AgentId == user.Id)
				.FirstOrDefaultAsync();

			if (property == null)
			{
				return NotFound("Property not found or you do not have permission to unlist this property.");
			}

			//property.ListingStatus = false;
			//_context.Update(property);
			await _context.SaveChangesAsync();

			return RedirectToAction("AgentPropertyList");
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
