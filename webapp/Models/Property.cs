using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using webapp.Areas.Identity.Data;

namespace webapp.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Status { get; set; }  // For Rent or For Sale

        [Required]
        public string PropertyType { get; set; }  // Houses, Apartment, Villas, etc.

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        public string Area { get; set; }

        [Required]
        public int Bedrooms { get; set; }

        [Required]
        public int Bathrooms { get; set; }

        public string GalleryPath { get; set; }  // Path to the gallery images

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public string BuildingAge { get; set; }  // 0 - 5 Years, 0 - 10 Years, etc.

        public int Garage { get; set; }

        public int Rooms { get; set; }

        public string OtherFeatures { get; set; }  // Comma-separated list of features

        [Required]
        [StringLength(100)]
        public string ContactName { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        public string ContactPhone { get; set; }

        public bool GDPRConsent { get; set; }

        [Required]
        public DateTime ListingDate { get; set; }

        public string AgentId { get; set; }

        [ForeignKey("AgentId")]
        public webappUser Agent { get; set; }
    }
}
