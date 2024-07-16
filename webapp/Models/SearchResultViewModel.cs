using System.Collections.Generic;

namespace webapp.Models
{
    public class SearchResultViewModel
    {
        public List<PropertyViewModel> Properties { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
