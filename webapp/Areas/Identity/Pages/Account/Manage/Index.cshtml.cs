﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webapp.Areas.Identity.Data;

namespace webapp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<webappUser> _userManager;
        private readonly SignInManager<webappUser> _signInManager;
        private readonly IWebHostEnvironment _environment;


        public IndexModel(
            UserManager<webappUser> userManager,
            SignInManager<webappUser> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string Email { get; set; }


            [Required(ErrorMessage = "You must enter the full name before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The full name must only contain alphabetic characters and spaces.")]
            [Display(Name = "Your Full Name")] //label
            public string FullName { get; set; }

            [Required(ErrorMessage = "You must enter the Country before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The country must only contain alphabetic characters and spaces.")]
            [Display(Name = "Your Country")] //label
            public string Country { get; set; }

            [Required(ErrorMessage = "You must enter the address before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Address")] //label
            public string Address { get; set; }

            [Required(ErrorMessage = "You must enter the state before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
            [Display(Name = "State")] //label
            public string State { get; set; }

            [Required(ErrorMessage = "You must enter the city before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
            [Display(Name = "City")] //label
            public string City { get; set; }

            [Required(ErrorMessage = "You must enter the zip before submitting your form!")]
            [Range(5, 99999, ErrorMessage = "The ZIP code must be a valid 5-digit number.")]
            [Display(Name = "Zip")] //label
            public int? Zip { get; set; }

            [Required(ErrorMessage = "You must enter the about before submitting your form!")]
            [StringLength(1000, ErrorMessage = "You must enter the value between 6 - 1000 chars", MinimumLength = 6)]
            [Display(Name = "About")] //label
            public string About { get; set; }

            [Required(ErrorMessage = "You must enter the facebook link before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Facebook")] //label
            public string FacebookLink { get; set; }

            [Required(ErrorMessage = "You must enter the X link before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "X")] //label
            public string XLink { get; set; }

            [Required(ErrorMessage = "You must enter the LinkedIn link before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "LinkedIn")] //label
            public string LinkedInLink { get; set; }

            [Required(ErrorMessage = "You must enter the google link before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Google Plus")] //label
            public string GoogleLink { get; set; }

            public string ProfilePicture { get; set; }

        }

        private async Task LoadAsync(webappUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Email = user.Email,
                FullName = user.FullName,
                Country = user.Country,
                Address = user.Address,
                State = user.State,
                City = user.City,
                Zip = user.Zip,
                About = user.About,
                FacebookLink = user.FacebookLink,
                XLink = user.XLink,
                LinkedInLink = user.LinkedInLink,
                GoogleLink = user.GoogleLink,
                ProfilePicture = user.ProfilePicture,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.Email != user.Email)
            {
                user.Email = Input.Email;
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }

            if (Input.Country != user.Country)
            {
                user.Country = Input.Country;
            }

            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
            }

            if (Input.State != user.State)
            {
                user.State = Input.State;
            }

            if (Input.City != user.City)
            {
                user.City = Input.City;
            }

            if (Input.Zip != user.Zip)
            {
                user.Zip = Input.Zip;
            }

            if (Input.About != user.About)
            {
                user.About = Input.About;
            }

            if (Input.FacebookLink != user.FacebookLink)
            {
                user.FacebookLink = Input.FacebookLink;
            }

            if (Input.XLink != user.XLink)
            {
                user.XLink = Input.XLink;
            }

            if (Input.LinkedInLink != user.LinkedInLink)
            {
                user.LinkedInLink = Input.LinkedInLink;
            }

            if (Input.GoogleLink != user.GoogleLink)
            {
                user.GoogleLink = Input.GoogleLink;
            }
            
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
