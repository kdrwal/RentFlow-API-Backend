namespace RentFlow.DTO
{
    public class ReservationCreateDto
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ReservationItemDto> Items { get; set; }
    }
}
