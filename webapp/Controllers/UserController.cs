using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Helpers;
using webapp.Models;
using Microsoft.CodeAnalysis.Elfie.Extensions;

namespace webapp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EncryptionHelper _encryptionHelper;

        public UserController(UserManager<webappUser> userManager, webappContext context, IWebHostEnvironment webHostEnvironment, EncryptionHelper encryptionHelper)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _encryptionHelper = encryptionHelper;

        }

        [Authorize]
        [HttpPost]
        [Route("account/upload-profile-pic")]
        public async Task<IActionResult> UploadUserPic(List<IFormFile> files)
        {
            var user = await _userManager.GetUserAsync(User);
            var uploadUrls = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Delete the old profile picture if it exists
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "user", user.ProfilePicture);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "user");
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

                    uploadUrls.Add(uniqueFileName);
                }
            }

            if (uploadUrls.Count > 0)
            {
                var uploadedUrl = uploadUrls.First(); // Assuming one file
                user.ProfilePicture = uploadedUrl;
                await _userManager.UpdateAsync(user);

                return Json(new { success = true, fileUrl = uploadedUrl });
            }
            else
            {
                return Json(new { success = false, errors = new[] { "No files uploaded." } });
            }
        }

        [HttpGet]
        [Route("{username}/property-save")]
        public async Task<IActionResult> SavePropertyList(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.UserName != username)
            {
                return RedirectToAction(nameof(SavePropertyList), new { username = currentUser.UserName });
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return NotFound();
            }

            var savedProperties = await _context.PropertySave
                .Where(ps => ps.UserId == user.Id)
                .Include(ps => ps.Property)
                .ThenInclude(p => p.Address)
                .ToListAsync();

            var propertySaveModels = savedProperties.Select(ps => new PropertySaveModel
            {
                EncryptedId = ps.Property != null ? _encryptionHelper.Encrypt(ps.PropertyId.ToString()) : string.Empty,
                Title = ps.Property?.Title ?? "No Title",
                Status = ps.Property?.Status ?? "Unknown",
                PropertyType = ps.Property?.PropertyType ?? "Unknown",
                Price = ps.Property?.Price ?? 0,
                GalleryPath = ps.Property?.GalleryPath?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                ListingDate = ps.Property?.ListingDate ?? DateTime.MinValue,
                AddressLine = ps.Property?.Address?.AddressLine ?? "No Address",
                State = ps.Property?.Address?.State ?? "Unknown",
                GalleryFolder = ps.Property != null ? ps.PropertyId.ToString().ToSHA256String() : string.Empty,
                ListingStatus = ps.Property?.ListingStatus ?? "Unknown"
            }).ToList();

            ViewData["Title"] = "My Property";
            ViewData["User"] = user;
            return View(propertySaveModels);
        }


		[Authorize]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> SaveProperty(int propertyId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized(new { success = false, message = "Unauthorized" });
			}

			// Check if the property is already saved by the user
			var existingPropertySave = await _context.PropertySave
				.FirstOrDefaultAsync(ps => ps.UserId == user.Id && ps.PropertyId == propertyId);

			if (existingPropertySave != null)
			{
				return Json(new { success = false, message = "Property is already saved." });
			}

			var propertySave = new PropertySave
			{
				UserId = user.Id,
				PropertyId = propertyId
			};

			_context.PropertySave.Add(propertySave);
			await _context.SaveChangesAsync();

			return Json(new { success = true, message = "Property saved successfully!" });
		}


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UnsaveProperty(int encryptedId)
        {
            Console.WriteLine(encryptedId);
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Unauthorized" });
            }

            var propertySave = await _context.PropertySave
                .FirstOrDefaultAsync(ps => ps.UserId == user.Id && ps.PropertyId == encryptedId);

            if (propertySave == null)
            {
                return NotFound(new { success = false, message = "Property not found in saved list." });
            }

            _context.PropertySave.Remove(propertySave);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Property unsaved successfully!" });
        }


    }
}
