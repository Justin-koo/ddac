using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
    public class PropertyAddress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AddressLine { get; set; }

        [Required]
        public string City { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "The state is required.")]
        public string State { get; set; }

        [Required]
        public string ZipCode { get; set; }

        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public Property Property { get; set; }
    }
}
