namespace RentFlow.DTO
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public List<ReservationBikeDto> ReservationBikes { get; set; }
    }
}
