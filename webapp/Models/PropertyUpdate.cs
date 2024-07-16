using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
	public class PropertyUpdate
	{
		[Key]
		public int Id { get; set; }
		public DateTime UpdateDate { get; set; }
		public String Status { get; set; }

		[DataType(DataType.Currency)]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal Price { get; set; }

		public int PropertyId { get; set; }
	}
}
