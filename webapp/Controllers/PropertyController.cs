using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Models;
using webapp.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace webapp.Controllers
{
    public class PropertyController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertyController(UserManager<webappUser> userManager, webappContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

		[BindProperty]
		public PropertyViewModel Property { get; set; }

        [Authorize]
        [HttpGet]
		public IActionResult SubmitProperty()
        {
            ViewData["Title"] = "Submit Property";
            return View(new PropertyViewModel());
        }

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
				GalleryFolder = p.Id.ToString().ToSHA256String()
			}).ToList();

			ViewData["Title"] = "My Property";
			return View(propertyViewModels);
        }

        [Authorize]
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

        [Authorize]
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
            foreach (var file in files)
            {
                //Console.WriteLine("In handle file");
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + ".png";
					var uploadsFolder = Path.Combine("wwwroot", "uploads", "property", propertyIdString.ToSHA256String());
					var filePath = Path.Combine(uploadsFolder, uniqueFileName);

					// Ensure the uploads folder exists
					if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    uploadUrls.Add(uniqueFileName); // or a URL if needed
                }
                //else {
                //    Console.WriteLine("No file to handle");
                //}
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
            var redirectUrl = Url.Action("SubmitProperty", "Property");

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
	}
}
