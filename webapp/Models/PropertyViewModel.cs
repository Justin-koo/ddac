    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    namespace webapp.Models
    {
    

    public class PropertyViewModel
    {
        [Required(ErrorMessage = "The property title is required.")]
        [StringLength(100, ErrorMessage = "The property title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The status is required.")]
        public string Status { get; set; }  // For Rent or For Sale

        [Required(ErrorMessage = "The property type is required.")]
        public string PropertyType { get; set; }  // Houses, Apartment, Villas, etc.

        //      [Required(ErrorMessage = "The price is required.")]
        //      [DataType(DataType.Currency, ErrorMessage = "The price must be a valid currency.")]
        //      public decimal Price { get; set; }

        //      [Required(ErrorMessage = "The area is required.")]
        //public int Area { get; set; }

        [Required(ErrorMessage = "The price is required.")]
        [DataType(DataType.Currency, ErrorMessage = "The price must be a valid currency.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "The area is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The area must be greater than zero.")]
        public int Area { get; set; }

        [Required(ErrorMessage = "The number of bedrooms is required.")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "The number of bathrooms is required.")]
        public int Bathrooms { get; set; }

        // public string? GalleryPath { get; set; }  // Path to the gallery images
        public List<string> GalleryPath { get; set; } = new List<string>();

        public DateTime ListingDate { get; set; } = DateTime.Now;

        public string? AgentId { get; set; }

        // Address fields
        [Required(ErrorMessage = "The address field is required.")]
        public string AddressLine { get; set; }

        [Required(ErrorMessage = "The city field is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "The state field is required.")]
        public string State { get; set; }

        [Required(ErrorMessage = "The zip code field is required.")]
        public string ZipCode { get; set; }

        // Property details
        [Required(ErrorMessage = "The description is required.")]
        public string Description { get; set; }

        public string? BuildingAge { get; set; }

        public int? Garage { get; set; }

        public int? Rooms { get; set; }

        public List<string> Features { get; set; } = new List<string>();

		public string? GalleryFolder { get; set; }

		public List<string> ImageUrls { get; set; } = new List<string>();

		public string? ListingStatus { get; set; }

		public int Id { get; set; }

        public string? EncryptedId { get; set; }

		public List<int>? SavedPropertyIds { get; set; }

	}
}
