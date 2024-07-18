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
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        public int Area { get; set; }

        [Required]
        public int Bedrooms { get; set; }

        [Required]
        public int Bathrooms { get; set; }

        public string GalleryPath { get; set; }  // Path to the gallery images

        [Required]
        public DateTime ListingDate { get; set; }

        public string AgentId { get; set; }

        [ForeignKey("AgentId")]
        public webappUser Agent { get; set; }
        public virtual PropertyAddress Address { get; set; }
        public virtual PropertyDetail Detail { get; set; }

        [Required]
        public string ListingStatus { get; set; }

		public Property DeepCopy()
		{
			return new Property
			{
				Id = this.Id, // Value type, copied directly
				Status = new string(this.Status), // Immutable, but explicitly creating a new instance for demonstration
				Price = this.Price, // Value type, copied directly
			};
		}
	}
}
