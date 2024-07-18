using System.Collections.Generic;

namespace webapp.Models
{
    public class SearchResultViewModel
    {
        public List<PropertyViewModel> Properties { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; }
        public string City { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public string types { get; set; }

        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int Garage { get; set; }
        public string BuiltYear { get; set; }
        public List<int> SelectedFeatures { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public List<string> Cities { get; set; } = new List<string>();
        public List<string> states { get; set; } = new List<string>();

        public List<string> PropertyTypes { get; set; } = new List<string>();

        public List<string> statusList { get; set; } = new List<string>();
        public int ActivePropertiesCount { get; set; }


        public List<PropertyFeatures> Features { get; set; } = new List<PropertyFeatures>();
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string PriceRange { get; set; }

    }
}
