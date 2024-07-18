namespace webapp.Models.Admin
{
	public class ReportPropertyViewModel
	{
		public string UserName { get; set; }
		public string PropertyId { get; set; }
		public string Reason { get; set; }
		public DateTime ReportDate { get; set; }
		public bool IsCurrentUser { get; set; }
	}
}
