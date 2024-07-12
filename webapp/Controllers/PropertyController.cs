﻿using System.Net.NetworkInformation;
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

            //ViewData["Title"] = "My Property";
            return View(properties);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitProperty(PropertyViewModel model, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // Handle the case where the user is not found.
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var filePaths = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot/uploads", Path.GetFileName(file.FileName));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    filePaths.Add(filePath);
                }
            }


            var property = new Property
            {
                Title = Property.Title,
                Status = Property.Status,
                PropertyType = Property.PropertyType,
                Price = Property.Price,
                Area = Property.Area,
                Bedrooms = Property.Bedrooms,
                Bathrooms = Property.Bathrooms,
                GalleryPath = string.Join(";", filePaths),
                AgentId = user.Id,
                ListingDate = DateTime.Now,
                Address = new PropertyAddress
                {
                    AddressLine = Property.AddressLine,
                    City = Property.City,
                    State = Property.State,
                    ZipCode = Property.ZipCode
                },
                Detail = new PropertyDetail
                {
                    Description = Property.Description,
                    BuildingAge = Property.BuildingAge ?? string.Empty,
                    Garage = Property.Garage ?? 0,
                    Rooms = Property.Rooms ?? 0,
                    OtherFeatures = string.Join(", ", Property.Features)
                }
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            //StatusMessage = "Property created successfully.";

            return RedirectToPage("/Property/SubmitProperty");
        }

        [HttpPost]
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