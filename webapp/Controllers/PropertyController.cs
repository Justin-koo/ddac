using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Models;
using webapp.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        public IActionResult SubmitProperty()
        {
            ViewData["Title"] = "Submit Property";
            ViewData["ActivePage"] = ManageNavPages.SubmitProperty;
            return View(new PropertyViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SubmitProperty(PropertyViewModel model, IFormFileCollection files)
        {

            if (ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    // Handle the case where the user is not found.
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                Console.WriteLine("Files: " + (files != null ? files.Count.ToString() : "null"));
                var uploadUrls = new List<string>();
                foreach (var file in files)
                {
                    Console.WriteLine("In handle file");
                    if (file.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var uploadsFolder = Path.Combine("wwwroot", "uploads");
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

                        uploadUrls.Add(filePath); // or a URL if needed
                    }
                    else {
                        Console.WriteLine("No file to handle");
                    }
                }

                model.GalleryPath = uploadUrls;

                Console.WriteLine("This is the url ::" + string.Join(";", model.GalleryPath));

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
                    GalleryPath = string.Join(";", model.GalleryPath),
                };

                _context.Properties.Add(property);
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
                    // OtherFeatures = 'hello world',
                    PropertyId = property.Id
                };

                _context.PropertyDetails.Add(propertyDetail);
                await _context.SaveChangesAsync();

                return RedirectToAction("SubmitProperty");
            }
            var errors = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .Select(ms => new
                    {
                        Field = ms.Key,
                        Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    });

                // Optionally log the errors
                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Field}");
                    foreach (var errorMessage in error.Errors)
                    {
                        Console.WriteLine($"Error: {errorMessage}");
                    }
                }

            return View(model);
        }
    }
}
