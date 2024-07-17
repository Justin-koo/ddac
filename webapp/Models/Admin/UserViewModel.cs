using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace webapp.Models.Admin
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "You must enter the username before submitting your form!")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "You must enter the full name before submitting your form!")]
        [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The full name must only contain alphabetic characters and spaces.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "You must enter the email before submitting your form!")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "The country must only contain alphabetic characters and spaces.")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
        [Display(Name = "State")]
        public string? State { get; set; }

        [StringLength(256, ErrorMessage = "You must enter the value between 2 - 256 chars", MinimumLength = 2)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [Range(5, 99999, ErrorMessage = "The ZIP code must be a valid 5-digit number.")]
        [Display(Name = "Zip")]
        public int? Zip { get; set; }

        [StringLength(1000, ErrorMessage = "You must enter the value between 6 - 1000 chars", MinimumLength = 6)]
        [Display(Name = "About")]
        public string? About { get; set; }

        [Display(Name = "Role")]
        public string SelectedRole { get; set; }


    }
}
