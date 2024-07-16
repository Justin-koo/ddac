using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webapp.Areas.Identity.Data;
using webapp.Data;
using webapp.Helpers;
using webapp.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("webappContextConnection") ?? throw new InvalidOperationException("Connection string 'webappContextConnection' not found.");

builder.Services.AddDbContext<webappContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<webappUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<webappContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/login";
	options.LogoutPath = "/Identity/Account/Logout";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.SlidingExpiration = true;
	options.ExpireTimeSpan = TimeSpan.FromMinutes(1440);
});

// Configure SMTP settings and email service
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<SendEmailService>();

// Configure Data Protection
builder.Services.AddDataProtection();
builder.Services.AddSingleton<EncryptionHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

var defaultCulture = new CultureInfo("en-MY");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = services.GetRequiredService<UserManager<webappUser>>();
		await SeedRolesAndAdminUser(roleManager, userManager);
	}
	catch (Exception ex)
	{
		// Log errors or handle exceptions as necessary
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}

app.Run();

async Task SeedRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<webappUser> userManager)
{
	// Define the roles, including the new "Manager" role
	string[] roleNames = { "Admin", "User", "Agent" };

	// Create each role if it does not already exist
	foreach (var roleName in roleNames)
	{
		if (!await roleManager.RoleExistsAsync(roleName))
		{
			await roleManager.CreateAsync(new IdentityRole(roleName));
		}
	}

	// Define the admin user
	var adminUser = new webappUser
	{
		UserName = "admin@example.com",
		Email = "admin@example.com",
		EmailConfirmed = true
	};

	string adminPassword = "Admin@123";

	// Check if the admin user already exists
	var user = await userManager.FindByEmailAsync(adminUser.Email);
	if (user == null)
	{
		// Create the admin user with the specified password
		var createAdminUser = await userManager.CreateAsync(adminUser, adminPassword);
		if (createAdminUser.Succeeded)
		{
			// Assign the "Admin" role to the newly created admin user
			await userManager.AddToRoleAsync(adminUser, "Admin");
		}
	}
}
