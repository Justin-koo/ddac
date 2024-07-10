using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
    public class PropertyDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public string BuildingAge { get; set; }  // 0 - 5 Years, 0 - 10 Years, etc.

        public int Garage { get; set; }

        public int Rooms { get; set; }

        public string OtherFeatures { get; set; }  // Comma-separated list of features

        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public Property Property { get; set; }
    }
}
