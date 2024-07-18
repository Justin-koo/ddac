using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Helpers;
using webapp.Models.Admin;

namespace webapp.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly webappContext _context;
		private readonly UserManager<webappUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SignInManager<webappUser> _signInManager;

        public AdminController(webappContext context, UserManager<webappUser> userManager, SignInManager<webappUser> signInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
			_context = context;
			_userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var allUsers = _userManager.Users.ToList();
			var totalUsers = allUsers.Count;

			// Exclude agents and admins
			var agentRoleId = (await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Agent"))?.Id;
			var adminRoleId = (await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin"))?.Id;
			var agentUserIds = _context.UserRoles.Where(ur => ur.RoleId == agentRoleId).Select(ur => ur.UserId);
			var adminUserIds = _context.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId);
			var userRoleIds = allUsers.Select(u => u.Id).Except(agentUserIds).Except(adminUserIds);

			var registeredUsersCount = allUsers.Count(u => userRoleIds.Contains(u.Id));

			var activePropertiesCount = await _context.Properties.CountAsync(p => p.ListingStatus == "Active");
			var soldPropertiesCount = await _context.Properties.CountAsync(p => p.ListingStatus == "Sold");

			var recentlySoldProperties = await _context.Properties
				.Where(p => p.ListingStatus == "Sold")
				.Include(p => p.Address)
				.OrderByDescending(p => p.ListingDate)
				.Take(7)
				.ToListAsync();

			var recentAgents = await _userManager.Users
				.Where(u => agentUserIds.Contains(u.Id))
				.OrderByDescending(u => u.Id) // Assuming Id is incremented, otherwise use a proper date field
				.Take(5)
				.ToListAsync();

			ViewBag.RegisteredUsersCount = registeredUsersCount;
			ViewBag.ActivePropertiesCount = activePropertiesCount;
			ViewBag.SoldPropertiesCount = soldPropertiesCount;
			ViewBag.RecentlySoldProperties = recentlySoldProperties;
			ViewBag.RecentAgents = recentAgents;

			return View();
		}

		[HttpGet]
        public async Task<IActionResult> UserList()
        {
			var currentUserId = _userManager.GetUserId(User);
			var users = _userManager.Users.Where(u => u.Id != currentUserId).ToList();
			var userList = new List<UserViewModel>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();
                userList.Add(new UserViewModel
				{
					//Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FullName = user.FullName,
                    SelectedRole = role
				});
			}

            return View(userList);
        }

		[HttpGet]
		public async Task<IActionResult> PropertyList()
		{
			var properties = await _context.Properties
				.Include(p => p.Address) // Include related data if needed
				.ToListAsync();

			var users = await _userManager.Users.ToListAsync();

			var propertyList = properties.Select(property => new PropertyViewModel
			{
				Id = property.Id,
				Title = property.Title,
				AddressLine = property.Address.AddressLine,
				Price = property.Price,
				ListingDate = property.ListingDate,
				ListingStatus = property.ListingStatus,
				GalleryPath = property.GalleryPath?.Split(';').ToList() ?? new List<string>(),
				GalleryFolder = property.Id.ToString().ToSHA256String(), // Assuming this method exists for generating folder name
				ListedBy = users.FirstOrDefault(u => u.Id == property.AgentId)?.UserName
			}).ToList();

			return View(propertyList);
		}


		[HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model, List<IFormFile> profilepic)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
				{
					var key = state.Key;
					var errors = state.Value.Errors;
					foreach (var error in errors)
					{
						Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
					}
				}

				return View(model);
            }

            var emailExists = await _userManager.FindByEmailAsync(model.Email);
            if (emailExists != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already taken.");
                return View(model);
            }

            if (model.SelectedRole == "Agent")
            {
				if (string.IsNullOrEmpty(model.Country))
					ModelState.AddModelError("Country", "Country is required for agents.");
				if (string.IsNullOrEmpty(model.Address))
					ModelState.AddModelError("Address", "Address is required for agents.");
				if (string.IsNullOrEmpty(model.State))
					ModelState.AddModelError("State", "State is required for agents.");
				if (string.IsNullOrEmpty(model.City))
					ModelState.AddModelError("City", "City is required for agents.");
				if (!model.Zip.HasValue)
					ModelState.AddModelError("Zip", "Zip is required for agents.");
				if (string.IsNullOrEmpty(model.About))
					ModelState.AddModelError("About", "About is required for agents.");

				return View(model);
			}

			var user = new webappUser
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Country = model.Country,
                Address = model.Address,
                State = model.State,
                City = model.City,
                Zip = model.Zip,
                About = model.About
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                TempData["Message"] = "User created successfully!";
                return RedirectToAction(nameof(UserList));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

		[HttpPost]
		public async Task<IActionResult> DeleteUser(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			if (user == null)
			{
				TempData["Message"] = "Error: User not found.";
				return RedirectToAction(nameof(UserList));
			}

			var result = await _userManager.DeleteAsync(user);
			if (!result.Succeeded)
			{
				TempData["Message"] = "Error: Something went wrong.";
				return RedirectToAction(nameof(UserList));
			}

			TempData["Message"] = "User deleted successfully!";
			return RedirectToAction(nameof(UserList));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> BlockProperty(int id)
		{
			var property = await _context.Properties.FindAsync(id);
			if (property == null)
			{
				TempData["Message"] = "Error: Property not found.";
				return RedirectToAction(nameof(PropertyList));
			}

			if (property.ListingStatus == "Sold")
			{
				TempData["Message"] = "Error: Property cannot be blocked.";
				return RedirectToAction(nameof(PropertyList));
			}

			property.ListingStatus = "Blocked";
			_context.Properties.Update(property);
			await _context.SaveChangesAsync();

			TempData["Message"] = "Property blocked successfully!";
			return RedirectToAction(nameof(PropertyList));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UnblockProperty(int id)
		{
			var property = await _context.Properties.FindAsync(id);
			if (property == null)
			{
				TempData["Message"] = "Error: Property not found.";
				return RedirectToAction(nameof(PropertyList));
			}

			if (property.ListingStatus != "Blocked")
			{
				TempData["Message"] = "Error: Property is not blocked.";
				return RedirectToAction(nameof(PropertyList));
			}

			property.ListingStatus = "Active";
			_context.Properties.Update(property);
			await _context.SaveChangesAsync();

			TempData["Message"] = "Property unblocked successfully!";
			return RedirectToAction(nameof(PropertyList));
		}

		[HttpGet]
		public IActionResult ChangePassword()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["message"] = "Your password has been changed successfully.";
				return View();
			}

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

    }
}
