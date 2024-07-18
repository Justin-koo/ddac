using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Models;
using webapp.Services;
using System.Text.Json;
using System.Composition;


namespace webapp.Controllers
{
    public class PropertyController : Controller
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly webappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly SendEmailService _emailService;

		public PropertyController(UserManager<webappUser> userManager, webappContext context, IWebHostEnvironment webHostEnvironment, SendEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
			_emailService = emailService;
		}

		[BindProperty]
		public PropertyViewModel Property { get; set; }

		[HttpGet]
		public async Task<IActionResult> PropertyDetails(int id)
		{
			var property = await _context.Properties
				.Include(p => p.Address)
				.Include(p => p.Detail)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (property == null)
			{
				return NotFound();
			}

			var savedPropertyIds = new List<int>();
			var users = await _userManager.GetUserAsync(User);


			if (users != null)
			{
				savedPropertyIds = await _context.PropertySave
					.Where(ps => ps.UserId == users.Id)
					.Select(ps => ps.PropertyId)
					.ToListAsync();
			}

			var propertyViewModel = new PropertyViewModel
			{
				Id = property.Id,
				Title = property.Title,
				Status = property.Status,
				PropertyType = property.PropertyType,
				Price = property.Price,
				Area = property.Area,
				Bedrooms = property.Bedrooms,
				Bathrooms = property.Bathrooms,
				GalleryPath = property.GalleryPath?.Split(";").ToList() ?? new List<string>(),
				ListingDate = property.ListingDate,
				AgentId = property.AgentId,
				AddressLine = property.Address.AddressLine,
				City = property.Address.City,
				State = property.Address.State,
				ZipCode = property.Address.ZipCode,
				Description = property.Detail.Description,
				BuildingAge = property.Detail.BuildingAge,
				Garage = property.Detail.Garage,
				Rooms = property.Detail.Rooms,
				ListingStatus = property.ListingStatus,
				SavedPropertyIds = savedPropertyIds,

			};

			var propertyUpdates = await _context.PropertyUpdate
				.Where(p => p.PropertyId == id)  // Assuming 'PropertyId' is the foreign key linking to the property
				.ToListAsync();

			var user = await _userManager.Users
				.FirstOrDefaultAsync(u => u.Id == property.AgentId);

			var agent = new AgentViewModel
			{
				Id = user.Id,
				UserName = user.UserName,
				Name = user.FullName,
				Email = user.Email,
				PhoneNumber = user.PhoneNumber,
				About = user.About,
				City = user.City,
				State = user.State,
				FacebookLink = user.FacebookLink,
				XLink = user.XLink,
				LinkedInLink = user.LinkedInLink,
				GoogleLink = user.GoogleLink,
				PropertyCount = _context.Properties.Count(p => p.AgentId == user.Id),
				Location = user.City + ", " + user.State,
				ProfilePicture = user.ProfilePicture,
			};


			var viewModel = new PropertyDetailsViewModel
			{
				Property = propertyViewModel,
				Features = await _context.Features.ToListAsync(),
				SelectedFeatures = [.. property.Detail.OtherFeatures.Split(";")],
				PropertyUpdates = propertyUpdates,
				Agent = agent,
			};

			ViewData["Title"] = property.Title;
			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> ScheduleTour(PropertyDetailsViewModel Model)
		{
			var agent = Model.Agent;

			if (agent != null)
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(agent, options);
				Console.WriteLine(json);
			}
			else
			{
				Console.WriteLine("Agent is null");
			}
			
			var property = Model.Property;

			if (property != null)
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(property, options);
				Console.WriteLine(json);
			}
			else
			{
				Console.WriteLine("Proeprty is null");
			}

			var inquiry = Model.InquiryModel;

			if (inquiry != null)
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string json = JsonSerializer.Serialize(inquiry, options);
				Console.WriteLine(json);
			}
			else
			{
				Console.WriteLine("Proeprty is null");
			}

			if (ModelState.IsValid)
			{
				string tourType = Model.InquiryModel.IsInPerson ? "In-Person Tour" : "Virtual Tour via Video Chat";
				string subject = "Inquiry/Tour Request";
				string emailBody = $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <style>
                            body {{ font-family: Arial, sans-serif; }}
                            .email-container {{ max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ccc; border-radius: 5px; }}
                            .header {{ background-color: #004a99; color: white; padding: 10px; text-align: center; }}
                            .content-block {{ padding: 20px; }}
                            .footer {{ background-color: #f3f3f3; padding: 10px; text-align: center; color: grey; }}
                            .button {{ background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                            .button:hover {{ background-color: #218838; }}
                        </style>
                    </head>
                    <body>
                        <div class='email-container'>
                            <div class='header'>
                                <h1>Tour/Inquiry Request Received</h1>
                            </div>
                            <div class='content-block'>
                                <h2>Details of the Request</h2>
                                <p><strong>Name:</strong> {Model.InquiryModel.Name}</p>
                                <p><strong>Email:</strong> {Model.InquiryModel.Email}</p>
                                <p><strong>Phone Number:</strong> {Model.InquiryModel.Phone}</p>
                                <p><strong>Type of Tour:</strong> {tourType}</p>
                                <p><strong>Date and Time:</strong> {Model.InquiryModel.Date} at {Model.InquiryModel.Time}</p>
                                <p><strong>Description:</strong> {Model.InquiryModel.Description}</p>
                                <a href='#' class='button'>View More Details</a>
                            </div>
                            <div class='footer'>
                                <p>Thank you for using DDAC Property Services.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";
				await _emailService.SendAsync(Model.Agent.Email, subject, emailBody, Model.InquiryModel.Email);
				// Process the form (e.g., send an email, save to database)
				TempData["Message"] = "Email send!";
				return RedirectToAction(nameof(PropertyDetails), new { id = Model.Property.Id }); // Redirect to a confirmation page
			}

			foreach (var entry in ModelState)
			{
				if (entry.Value.Errors.Any())
				{
					// Log each error to the console
					foreach (var error in entry.Value.Errors)
					{
						Console.WriteLine($"Error in {entry.Key}: {error.ErrorMessage}");
					}
				}
			}
			TempData["Message"] = "Error";
			return BadRequest("Invalid request data."); // Return with validation errors
		}

		[HttpPost]
		public async Task<IActionResult> ReportListing(string selectedReasonRadio, string propertyIdRadio)
		{
			if (string.IsNullOrEmpty(selectedReasonRadio))
			{
				TempData["Report Message"] = "Error! Please select a reason for reporting.";
				return RedirectToAction("PropertyDetails", new { id = propertyIdRadio });
			}

			var userId = _userManager.GetUserId(User);

			if (userId == null)
			{
				TempData["Report Message"] = "Error! You must be logged in to report a property.";
				return RedirectToAction("PropertyDetails", new { id = propertyIdRadio });
			}

			// Save to database
			var report = new ReportProperty
			{
				UserId = userId,
				PropertyId = propertyIdRadio,
				Reason = selectedReasonRadio,
				ReportDate = DateTime.Now
			};

			_context.ReportProperty.Add(report);
			await _context.SaveChangesAsync();

			TempData["Report Message"] = "Thank you for your report. We will investigate the issue promptly.";
			return RedirectToAction("PropertyDetails", new { id = propertyIdRadio });
		}
	}
}
