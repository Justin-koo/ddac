using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using webapp.Areas.Identity.Data;
using webapp.Models;

namespace webapp.Data;

public class webappContext : IdentityDbContext<webappUser>
{
    public webappContext(DbContextOptions<webappContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyAddress> PropertyAddresses { get; set; }
    public DbSet<PropertyDetail> PropertyDetails { get; set; }
    public DbSet<PropertyFeatures> Features { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Entity<PropertyFeatures>().HasData(
            new PropertyFeatures { FeatureID = 1, FeatureName = "Equipped Kitchen", IconClass = "fa-utensils", Category = "Interior Details" },
            new PropertyFeatures { FeatureID = 2, FeatureName = "Gym", IconClass = "fa-dumbbell", Category = "Interior Details" },
            new PropertyFeatures { FeatureID = 3, FeatureName = "Laundry", IconClass = "fa-jug-detergent", Category = "Interior Details" },
            new PropertyFeatures { FeatureID = 4, FeatureName = "Media Room", IconClass = "fa-chromecast", Category = "Interior Details" },
            new PropertyFeatures { FeatureID = 5, FeatureName = "TV Set", IconClass = "fa-tv", Category = "Interior Details" },
            new PropertyFeatures { FeatureID = 6, FeatureName = "Back yard", IconClass = "fa-canadian-maple-leaf", Category = "Outdoor Details" },
            new PropertyFeatures { FeatureID = 7, FeatureName = "Basketball court", IconClass = "fa-basketball", Category = "Outdoor Details" },
            new PropertyFeatures { FeatureID = 8, FeatureName = "Front yard", IconClass = "fa-seedling", Category = "Outdoor Details" },
            new PropertyFeatures { FeatureID = 9, FeatureName = "Garage Attached", IconClass = "fa-square-parking", Category = "Outdoor Details" },
            new PropertyFeatures { FeatureID = 10, FeatureName = "Hot Bath", IconClass = "fa-shower", Category = "Outdoor Details" },
            new PropertyFeatures { FeatureID = 11, FeatureName = "Pool", IconClass = "fa-water-ladder", Category = "Outdoor Details" },
            new PropertyFeatures { FeatureID = 12, FeatureName = "Central Air", IconClass = "fa-fan", Category = "Utilities" },
            new PropertyFeatures { FeatureID = 13, FeatureName = "Electricity", IconClass = "fa-plug", Category = "Utilities" },
            new PropertyFeatures { FeatureID = 14, FeatureName = "Heating", IconClass = "fa-fire", Category = "Utilities" },
            new PropertyFeatures { FeatureID = 15, FeatureName = "Natural Gas", IconClass = "fa-fire-flame-simple", Category = "Utilities" },
            new PropertyFeatures { FeatureID = 16, FeatureName = "Ventilation", IconClass = "fa-snowflake", Category = "Utilities" },
            new PropertyFeatures { FeatureID = 17, FeatureName = "Water", IconClass = "fa-droplet", Category = "Utilities" },
            new PropertyFeatures { FeatureID = 18, FeatureName = "Chair Accessible", IconClass = "fa-wheelchair", Category = "Other Features" },
            new PropertyFeatures { FeatureID = 19, FeatureName = "Elevator", IconClass = "fa-elevator", Category = "Other Features" },
            new PropertyFeatures { FeatureID = 20, FeatureName = "Fireplace", IconClass = "fa-fire-extinguisher", Category = "Other Features" },
            new PropertyFeatures { FeatureID = 21, FeatureName = "Smoke detectors", IconClass = "fa-smoking", Category = "Other Features" },
            new PropertyFeatures { FeatureID = 22, FeatureName = "Washer and dryer", IconClass = "fa-bacon", Category = "Other Features" },
            new PropertyFeatures { FeatureID = 23, FeatureName = "WiFi", IconClass = "fa-wifi", Category = "Other Features" }
        );
    }
}
