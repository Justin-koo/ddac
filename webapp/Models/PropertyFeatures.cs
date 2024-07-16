using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
    public class PropertyFeatures
    {
        [Key]
        public int FeatureID { get; set; }
        public string FeatureName { get; set; }
        public string IconClass { get; set; }
        public string Category { get; set; }
    }
}
