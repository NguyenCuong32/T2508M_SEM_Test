namespace ComicSystem.Models
{
    public class RentalReportViewModel
    {
        public string BookName { get; set; } = string.Empty;
        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
