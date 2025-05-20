namespace RentFlow.DTO
{
    public class ReservationBikeDto
    {
        public int BikeId { get; set; }
        public int Quantity { get; set; }
        public BikeDto Bike { get; set; }
    }
}
