namespace webapp.Models
{
    public class ReportProperty
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PropertyId { get; set; }
        public string Reason { get; set; }
        public DateTime ReportDate { get; set; }
    }
}
