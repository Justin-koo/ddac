using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using webapp.Areas.Identity.Data;

namespace webapp.Models
{
	public class PropertySave
	{
		[Key]
		public int Id { get; set; }
		
		public String UserId { get; set; }

		[ForeignKey("UserId")]
		public webappUser user { get; set; }

		public int PropertyId { get; set; }

	}
}
