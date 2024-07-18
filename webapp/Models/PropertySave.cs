using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using webapp.Areas.Identity.Data;
using webapp.Models;

public class PropertySave
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public webappUser User { get; set; }

    public int PropertyId { get; set; }

    [ForeignKey("PropertyId")]
    public Property Property { get; set; }
}
