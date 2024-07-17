using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace webapp.Models
{
    public class PropertyDetailsViewModel
    {
        public PropertyViewModel Property { get; set; }
        public List<PropertyFeatures>? Features { get; set; } = new List<PropertyFeatures>();
        public List<string> SelectedFeatures { get; set; } = new List<string>();
        public List<PropertyUpdate>? PropertyUpdates { get; set; } = new List<PropertyUpdate>();

        public AgentViewModel Agent { get; set; }
        public InquiryModel? InquiryModel { get; set; }
	}
}
