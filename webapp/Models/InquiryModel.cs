using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
	public class InquiryModel
	{
		public string? Date { get; set; }  // Includes both date and time
		public string? Time { get; set; }  // Includes both date and time
		public bool IsInPerson { get; set; }    // True for In Person, False for Video Chat

		[Required(ErrorMessage = "Your name is required.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Description is required.")]
		public string Description { get; set; }
		public string Type { get; set; }
		//public string AgentEmail { get; set; }
		//public int PropertyId { get; set; }
		//public string PropertyTitle { get; set; }
		//public string PropertyAddress { get; set; }
		public string PropertyLink { get; set; }
	}

}
