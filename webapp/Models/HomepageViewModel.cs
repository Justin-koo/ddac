using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace webapp.Models
{
    public class HomepageViewModel
    {
        public List<PropertyViewModel> HomepagePropertyViewModel { get; set; }
        // public List<Property> SimilarProperties { get; set; }
    }
}
