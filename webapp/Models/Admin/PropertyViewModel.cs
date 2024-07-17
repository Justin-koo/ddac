namespace webapp.Models.Admin
{
	public class PropertyViewModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string AddressLine { get; set; }
		public decimal Price { get; set; }
		public DateTime ListingDate { get; set; }
		public string ListingStatus { get; set; }
		public List<string> GalleryPath { get; set; }
		public string GalleryFolder { get; set; }
		public string ListedBy { get; set; }
	}

}
