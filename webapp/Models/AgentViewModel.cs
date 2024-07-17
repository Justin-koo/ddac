namespace webapp.Models
{
	public class AgentViewModel
	{
		public string Id { get; set; }

		public string UserName { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string PhoneNumber { get; set; }

		public string About { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string? FacebookLink { get; set; }

		public string? LinkedInLink { get; set; }

		public string? XLink { get; set; }

		public string? GoogleLink { get; set; }

		public int PropertyCount { get; set; }

		public List<PropertyViewModel> Properties { get; set; }

		public string Location { get; set; }

		public string ProfilePicture { get; set; }
	}
}
