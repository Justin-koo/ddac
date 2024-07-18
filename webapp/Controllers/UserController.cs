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
        public async Task<IActionResult> savePropertyList(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.UserName != username)
            {
                return RedirectToAction(nameof(savePropertyList), new { username = currentUser.UserName });
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username);

            var properties = await _context.Properties
                .Include(p => p.Address)
                .Include(p => p.Detail)
                //.Where(p => user.Id)
                .ToListAsync();

            var propertySaveModels = properties.Select(p => new PropertySaveModel
            {
                //Id = p.Id,
                EncryptedId = _encryptionHelper.Encrypt(p.Id.ToString()),
                Title = p.Title,
                Status = p.Status,
                PropertyType = p.PropertyType,
                Price = p.Price,
                //Area = p.Area,
                //Bedrooms = p.Bedrooms,
                //Bathrooms = p.Bathrooms,
                GalleryPath = p.GalleryPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                ListingDate = p.ListingDate,
                //AgentId = p.AgentId,
                AddressLine = p.Address.AddressLine,
                //City = p.Address.City,
                State = p.Address.State,
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
            return View(propertySaveModels);
        }
    }
}
