namespace RentFlow.DTO
{
    public class BikeDto
    {
        public int BikeId { get; set; }
        public string Description { get; set; }

        public string Brand { get; set; }
        public string Color { get; set; }
        public string Size  { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
    }
}
