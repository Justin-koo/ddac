#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webapp.Models;
using webapp.Areas.Identity.Data;
using System.Threading.Tasks;
using webapp.Data;

namespace webapp.Areas.Identity.Pages.Account.Manage
{
	public class SubmitPropertyModel : PageModel
	{
		private readonly UserManager<webappUser> _userManager;
		private readonly webappContext _context;

		public SubmitPropertyModel(UserManager<webappUser> userManager, webappContext context)
		{
			_userManager = userManager;
			_context = context;
		}

		[BindProperty]
		public PropertyViewModel Property { get; set; }

		public void OnGet()
		{
			ViewData["ActivePage"] = ManageNavPages.SubmitProperty;
		}

		public async Task<IActionResult> OnPostAsync(List<IFormFile> files)
		{
			if (!ModelState.IsValid)
			{
				return Page();
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

			return RedirectToPage("./Index");
		}
	}
}
