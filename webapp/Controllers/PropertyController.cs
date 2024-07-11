﻿using System.Net.NetworkInformation;
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
        public async Task<IActionResult> SubmitProperty(PropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var property = new Property
                {
                    Title = model.Title,
                    Status = model.Status,
                    PropertyType = model.PropertyType,
                    Price = model.Price,
                    Area = model.Area,
                    Bedrooms = model.Bedrooms,
                    Bathrooms = model.Bathrooms,
                    GalleryPath = uniqueFileName,
                    AgentId = model.AgentId,
                    ListingDate = DateTime.Now,
                    Address = new PropertyAddress
                    {
                        AddressLine = model.AddressLine,
                        City = model.City,
                        State = model.State,
                        ZipCode = model.ZipCode
                    },
                    Detail = new PropertyDetail
                    {
                        Description = model.Description,
                        BuildingAge = model.BuildingAge ?? string.Empty,
                        Garage = model.Garage ?? 0,
                        Rooms = model.Rooms ?? 0,
                        OtherFeatures = string.Join(", ", model.Features)
                    }
                };

                // var property = new Property
                // {
                //     Title = model.Title,
                //     Status = model.Status,
                //     PropertyType = model.PropertyType,
                //     Price = model.Price,
                //     Area = model.Area,
                //     Bedrooms = model.Bedrooms,
                //     Bathrooms = model.Bathrooms,
                //     ListingDate = model.ListingDate,
                //     AgentId = model.AgentId,
                //     GalleryPath = uniqueFileName,
                //     // ImagePath = uniqueFileName // Store the unique file name in the database
                // };

                // _context.Properties.Add(property);
                // await _context.SaveChangesAsync();

                // var propertyAddress = new PropertyAddress
                // {
                //     AddressLine = model.AddressLine,
                //     City = model.City,
                //     State = model.State,
                //     ZipCode = model.ZipCode,
                //     PropertyId = property.Id
                // };

                // _context.PropertyAddresses.Add(propertyAddress);
                // await _context.SaveChangesAsync();

                // var propertyDetail = new PropertyDetail
                // {
                //     Description = model.Description,
                //     BuildingAge = model.BuildingAge,
                //     Garage = model.Garage,
                //     Rooms = model.Rooms,
                //     OtherFeatures = model.Features[0],
                //     PropertyId = property.Id
                // };

                // _context.PropertyDetails.Add(propertyDetail);
                // await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uniqueFileName = null;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // // Return the file path to the client (if needed)
                // return Json(new { filePath = "/uploads/" + uniqueFileName });
            }

            return BadRequest("Image upload failed.");
        }
    }
}
