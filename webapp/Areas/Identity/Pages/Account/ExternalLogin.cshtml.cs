﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using webapp.Areas.Identity.Data;
using webapp.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace webapp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<webappUser> _signInManager;
        private readonly UserManager<webappUser> _userManager;
        private readonly IUserStore<webappUser> _userStore;
        private readonly IUserEmailStore<webappUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly SendEmailService _emailService;

        public ExternalLoginModel(
            SignInManager<webappUser> signInManager,
            UserManager<webappUser> userManager,
            IUserStore<webappUser> userStore,
            ILogger<ExternalLoginModel> logger,
            SendEmailService emailService,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _emailService = emailService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        //public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        //{
        //	returnUrl = returnUrl ?? Url.Content("~/");
        //	if (remoteError != null)
        //	{
        //		ErrorMessage = $"Error from external provider: {remoteError}";
        //		return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        //	}
        //	var info = await _signInManager.GetExternalLoginInfoAsync();
        //	if (info == null)
        //	{
        //		ErrorMessage = "Error loading external login information.";
        //		return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        //	}

        //	// Check if the email claim exists in the external login data
        //	var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //	if (email != null)
        //	{
        //		// Check if a user with this email already exists
        //		var user = await _userManager.FindByEmailAsync(email);
        //		if (user != null)
        //		{
        //			// Check if the user already has this login
        //			var existingLogin = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        //			if (existingLogin == null)
        //			{
        //				// Link the external login to the existing user account if not linked
        //				var linkResult = await _userManager.AddLoginAsync(user, info);
        //				if (linkResult.Succeeded)
        //				{
        //					await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        //					return LocalRedirect(returnUrl);
        //				}
        //				else
        //				{
        //					// Handle errors, such as failed to link accounts
        //				}
        //			}
        //		}
        //	}

        //	// If the user does not have an account, then ask the user to create an account.
        //	ReturnUrl = returnUrl;
        //	ProviderDisplayName = info.ProviderDisplayName;
        //	if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        //	{
        //		Input = new InputModel
        //		{
        //			Email = info.Principal.FindFirstValue(ClaimTypes.Email)
        //		};
        //	}
        //	return Page();
        //}

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
					var user = await _userManager.FindByEmailAsync(email);

					if (user != null)
					{
						// User with this email already exists
						var existingLogin = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
						if (existingLogin == null)
						{
							// Link the external login to the existing user account
							var linkResult = await _userManager.AddLoginAsync(user, info);
							if (linkResult.Succeeded)
							{
								await _signInManager.SignInAsync(user, isPersistent: false);
								return LocalRedirect(returnUrl);
							}
							// Handle error when linking accounts
							ErrorMessage = "Error linking external login.";
							return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
						}
						// The account is already linked, just sign in
						await _signInManager.SignInAsync(user, isPersistent: false);
						return LocalRedirect(returnUrl);
					}
					else
					{
						// No user exists with this email, prepare to register a new user
						Input = new InputModel
						{
							Email = email
						};
					}
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                            string subject = "Confirm your email";
                            string htmlBody = $@"
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <style>
                                        .email-container {{
                                            font-family: Arial, sans-serif;
                                            color: #333;
                                            max-width: 600px;
                                            margin: 0 auto;
                                            padding: 20px;
                                            border: 1px solid #ddd;
                                            border-radius: 5px;
                                        }}
                                        .email-header {{
                                            background-color: #f7f7f7;
                                            padding: 10px 20px;
                                            border-bottom: 1px solid #ddd;
                                            text-align: center;
                                        }}
                                        .email-body {{
                                            padding: 20px;
                                        }}
                                        .email-footer {{
                                            background-color: #f7f7f7;
                                            padding: 10px 20px;
                                            border-top: 1px solid #ddd;
                                            text-align: center;
                                            font-size: 12px;
                                            color: #777;
                                        }}
                                        .confirm-button {{
                                            display: inline-block;
                                            padding: 10px 20px;
                                            margin: 20px 0;
                                            font-size: 16px;
                                            font-weight: bold; /* Added font weight */
                                            color: white !important; /* Font color */
                                            background-color: #28a745;
                                            text-decoration: none;
                                            border-radius: 5px;
                                        }}
                                        .confirm-button:hover {{
                                            background-color: #218838;
                                        }}
                                    </style>
                                </head>
                                <body>
                                    <div class='email-container'>
                                        <div class='email-header'>
                                            <h2>Confirm Your Email</h2>
                                        </div>
                                        <div class='email-body'>
                                            <p>Hi,</p>
                                            <p>Thank you for registering with us. To complete your registration, please confirm your email address by clicking the button below:</p>
                                            <a href='{callbackUrl}' class='confirm-button'>
                                                Confirm Email
                                            </a>
                                            <p>If you did not create an account, please ignore this email.</p>
                                            <p>Best regards,<br>DDAC Property Team</p>
                                        </div>
                                        <div class='email-footer'>
                                            <p>&copy; 2023 DDAC Property. All rights reserved.</p>
                                        </div>
                                    </div>
                                </body>
                                </html>";
                        //await _emailService.SendAsync(user.Email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
                        await _emailService.SendAsync(user.Email, subject, htmlBody);

                        //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private webappUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<webappUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(webappUser)}'. " +
                    $"Ensure that '{nameof(webappUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<webappUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<webappUser>)_userStore;
        }
    }
}
