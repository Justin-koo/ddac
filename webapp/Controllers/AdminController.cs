using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mono.TextTemplating;
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

			ViewData["IsAdminPage"] = true;
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
                    Id = user.Id,
                    UserName = user.UserName,
					Email = user.Email,
					FullName = user.FullName,
                    SelectedRole = role
				});
			}

			ViewData["IsAdminPage"] = true;
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

			ViewData["IsAdminPage"] = true;
			return View(propertyList);
		}


		[HttpGet]
        public async Task<IActionResult> CreateUser()
        {
			ViewData["IsAdminPage"] = true;
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

			bool AgentCheck = false;

            if (model.SelectedRole == "Agent")
            {
				if (string.IsNullOrEmpty(model.Country))
				{
					ModelState.AddModelError("Country", "Country is required for agents.");
					AgentCheck = true;
                }
				if (string.IsNullOrEmpty(model.Address))
				{
					ModelState.AddModelError("Address", "Address is required for agents.");
                    AgentCheck = true;
                }

                if (string.IsNullOrEmpty(model.State))
				{
                    ModelState.AddModelError("State", "State is required for agents.");
                    AgentCheck = true;
                }
                if (string.IsNullOrEmpty(model.City))
				{
                    ModelState.AddModelError("City", "City is required for agents.");
                    AgentCheck = true;
                }
                if (!model.Zip.HasValue)
				{
                    ModelState.AddModelError("Zip", "Zip is required for agents.");
                    AgentCheck = true;
                }
                if (string.IsNullOrEmpty(model.About))
				{
                    ModelState.AddModelError("About", "About is required for agents.");
                    AgentCheck = true;
                }

				if (AgentCheck)
				{
                    return View(model);
                }
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
                About = model.About,
				EmailConfirmed = true
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

			ViewData["IsAdminPage"] = true;
			return View(model);
        }

        //[Route("/admin/{username}")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
            {
                TempData["Message"] = "Error: User not found.";
                return RedirectToAction(nameof(UserList));
            }

            var roles = await _userManager.GetRolesAsync(user);
            string selectedRole = roles.FirstOrDefault();

            var model = new UserEditModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName, // Assuming you have a FullName property
                PhoneNumber = user.PhoneNumber, // Make sure the user object has a PhoneNumber property
                Country = user.Country, // Assuming there is a Country property
                Address = user.Address, // Assuming there is an Address property
                State = user.State, // Assuming there is a State property
                City = user.City, // Assuming there is a City property
                Zip = user.Zip, // Assuming there is a Zip property
                About = user.About,
                SelectedRole = selectedRole
            };

			ViewData["IsAdminPage"] = true;
			return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
				var user = await _userManager.FindByIdAsync(model.Id);
				if (user == null)
                {
                    TempData["Message"] = "Error: User not found.";
                    return RedirectToAction(nameof(UserList));
                }

                //Update other fields
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.FullName = model.FullName;

                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentPrimaryRole = currentRoles.FirstOrDefault();

                if (model.SelectedRole != currentPrimaryRole)
                {
                    if (!string.IsNullOrEmpty(currentPrimaryRole))
                    {
                        var removeResult = await _userManager.RemoveFromRoleAsync(user, currentPrimaryRole);
                        if (!removeResult.Succeeded)
                        {
                            ModelState.AddModelError("", "Failed to remove old role.");
                            return View(model);
                        }
                    }

                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
                        if (!addResult.Succeeded)
                        {
                            ModelState.AddModelError("", "Failed to add the user to the new role.");
                            return View(model);
                        }
                    }
                }

                if (model.SelectedRole == "Agent")
                {
                    user.Country = model.Country;
                    user.Address = model.Address;
                    user.State = model.State;
                    user.City = model.City; // Assuming there is a City property
                    user.Zip = model.Zip; // Assuming there is a Zip property
                    user.About = model.About;
                }

				if (model.ChangePassword && !string.IsNullOrWhiteSpace(model.Password))
				{
					// Generate a password reset token
					var token = await _userManager.GeneratePasswordResetTokenAsync(user);

					// Reset the password using the token
					var resetPassResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
					if (!resetPassResult.Succeeded)
					{
						foreach (var error in resetPassResult.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
						return View(model);
					}
				}

				await _userManager.UpdateAsync(user);
                TempData["Message"] = "User edited successfully!";

                return RedirectToAction(nameof(UserList));
            }

			ViewData["IsAdminPage"] = true;
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
			ViewData["IsAdminPage"] = true;
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
			ViewData["IsAdminPage"] = true;
			return RedirectToAction(nameof(PropertyList));
		}

		[HttpGet]
		public IActionResult ChangePassword()
		{
			ViewData["IsAdminPage"] = true;
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

			ViewData["IsAdminPage"] = true;
			return View(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> PropertyReport()
        //{
        //	var currentUser = await _userManager.GetUserAsync(User);
        //	var currentUserId = currentUser?.Id;

        //	var reports = await _context.ReportProperty.ToListAsync();
        //	var userIds = reports.Select(r => r.UserId).Distinct();
        //	var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName);

        //	Console.WriteLine("Null before");

        //	var reportViewModels = reports.Select(report => new ReportPropertyViewModel
        //	{
        //		Reason = report.Reason,
        //		ReportDate = report.ReportDate,
        //		PropertyId = report.PropertyId,
        //		UserName = report.UserId == currentUserId ? "Me" : _userManager.FindByIdAsync(report.UserId).Result.UserName,
        //		IsCurrentUser = report.UserId == currentUserId  // Set this property
        //	}).ToList();

        //          Console.WriteLine("Null now");

        //          if (!reportViewModels.Any())
        //	{
        //		Console.WriteLine("Null");
        //	}
        //	else {
        //              Console.WriteLine("Not Null");
        //          }

        //	return View(reportViewModels);
        //}


        [HttpGet]
        public async Task<IActionResult> PropertyReport()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var currentUserId = currentUser.Id;

            var reports = await _context.ReportProperty.ToListAsync();
            if (reports == null || !reports.Any())
            {
                return View("NoReports"); // or any other appropriate action
            }

            var userIds = reports.Select(r => r.UserId).Distinct();
            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName);

            Console.WriteLine("Null before");

            var reportViewModels = new List<ReportPropertyViewModel>();

            foreach (var report in reports)
            {
                try
                {
                    if (report == null)
                    {
                        Console.WriteLine("Report is null");
                        continue;
                    }

                    if (string.IsNullOrEmpty(report.UserId))
                    {
                        Console.WriteLine($"Report UserId is null or empty for PropertyId: {report.PropertyId}");
                        continue;
                    }

                    string userName;
                    if (report.UserId == currentUserId)
                    {
                        userName = "Me";
                    }
                    else if (users.ContainsKey(report.UserId))
                    {
                        userName = users[report.UserId];
                    }
                    else
                    {
                        var user = await _userManager.FindByIdAsync(report.UserId);
                        userName = user?.UserName ?? "Unknown User";
                    }

                    reportViewModels.Add(new ReportPropertyViewModel
                    {
                        Reason = report.Reason,
                        ReportDate = report.ReportDate,
                        PropertyId = report.PropertyId,
                        UserName = userName,
                        IsCurrentUser = report.UserId == currentUserId
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred for Report with PropertyId: {report?.PropertyId}, UserId: {report?.UserId}. Exception: {ex}");
                }
            }

            Console.WriteLine("Null now");

            if (!reportViewModels.Any())
            {
                Console.WriteLine("ReportViewModels is empty");
            }
            else
            {
                Console.WriteLine($"ReportViewModels count: {reportViewModels.Count}");
                foreach (var viewModel in reportViewModels)
                {
                    Console.WriteLine($"PropertyId: {viewModel.PropertyId}, UserName: {viewModel.UserName}");
                }
            }

            return View(reportViewModels);
        }
    }
}
