namespace KOL1APP.DTOs
{
    public class CarRentalDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int TotalPrice { get; set; }
        public int Discount { get; set; }
        public CarDTO Car { get; set; }
    }
}