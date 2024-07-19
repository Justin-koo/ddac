using System.ComponentModel.DataAnnotations;

namespace webapp.Models.Admin
{
	public class UserListModel
	{
		[Required]
		public string Id { get; set; }

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
		public string PhoneNumber { get; set; }

		[Display(Name = "Role")]
		public string SelectedRole { get; set; }
	}
}
