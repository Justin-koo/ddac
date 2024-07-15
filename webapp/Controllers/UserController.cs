using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapp.Areas.Identity.Data;
using webapp.Data;

namespace webapp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(UserManager<webappUser> userManager, webappContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
    }
}
