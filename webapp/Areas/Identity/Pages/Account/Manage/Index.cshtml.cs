// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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

        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string UserName { get; set; }

            [Required(ErrorMessage = "You must enter the full name before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The full name must only contain alphabetic characters and spaces.")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "You must enter the Country before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
            [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The country must only contain alphabetic characters and spaces.")]
            [Display(Name = "Country")]
            public string Country { get; set; }

            [Required(ErrorMessage = "You must enter the address before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [Required(ErrorMessage = "You must enter the state before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
            [Display(Name = "State")]
            public string State { get; set; }

            [Required(ErrorMessage = "You must enter the city before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
            [Display(Name = "City")]
            public string City { get; set; }

            [Required(ErrorMessage = "You must enter the zip before submitting your form!")]
            [Range(5, 99999, ErrorMessage = "The ZIP code must be a valid 5-digit number.")]
            [Display(Name = "Zip")]
            public int? Zip { get; set; }

            [Required(ErrorMessage = "You must enter the about before submitting your form!")]
            [StringLength(1000, ErrorMessage = "You must enter the value between 6 - 1000 chars", MinimumLength = 6)]
            [Display(Name = "About")]
            public string About { get; set; }

            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Facebook")]
            public string FacebookLink { get; set; }

            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "X")]
            public string XLink { get; set; }

            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "LinkedIn")]
            public string LinkedInLink { get; set; }

            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Google Plus")]
            public string GoogleLink { get; set; }

            public string ProfilePicture { get; set; }

            [Display(Name = "Specialties")]
            public List<string> Specialties { get; set; }

            [Display(Name = "Languages")]
            public List<string> Languages { get; set; }
        }

        private async Task LoadAsync(webappUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Email = email;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                UserName = user.UserName,
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
                Specialties = user.Specialties?.Split(',').ToList() ?? new List<string>(),
                Languages = user.Languages?.Split(',').ToList() ?? new List<string>(),
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

        public async Task<IActionResult> OnPostAsync(List<IFormFile> profilepic)
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

            if (Input.UserName != user.UserName)
            {
                user.UserName = Input.UserName;
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

            user.Specialties = Input.Specialties != null ? string.Join(',', Input.Specialties) : null;
            user.Languages = Input.Languages != null ? string.Join(',', Input.Languages) : null;

            var uploadUrls = new List<string>();
            foreach (var file in profilepic)
            {
                if (file.Length > 0)
                {
                    // Delete the old profile picture if it exists
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", "user", user.ProfilePicture);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "user");
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
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
