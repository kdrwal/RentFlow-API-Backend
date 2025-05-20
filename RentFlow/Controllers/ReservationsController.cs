using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentFlow.DTO;
using RentFlow.Models;
using RentFlow.Models.Contexts;

namespace RentFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly RentContext _context;

        public ReservationsController(RentContext context)
        {
            _context = context;
        }

        // POST: api/Reservations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostReservation([FromBody] ReservationCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (dto.StartDate.Date < DateTime.Today)
                return BadRequest("Reservation start date cannot be in the past.");

            var reservation = new Reservation
            {
                UserId = userId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Created",
                TotalPrice = 0m
            };

            foreach (var item in dto.Items)
            {
                var bike = await _context.Bikes.FindAsync(item.BikeId);
                if (bike == null) return BadRequest($"Bike {item.BikeId} not found");


                var overlappingQuantity = await _context.ReservationBikes
                    .Where(rb => rb.BikeId == item.BikeId && rb.Reservation.Status != "Cancelled" &&
                    (
                      (dto.StartDate >= rb.Reservation.StartDate && dto.StartDate <= rb.Reservation.EndDate) ||
                      (dto.EndDate != null && dto.EndDate >= rb.Reservation.StartDate && dto.EndDate <= rb.Reservation.EndDate) ||
                      (dto.StartDate <= rb.Reservation.StartDate && (dto.EndDate ?? dto.StartDate) >= rb.Reservation.EndDate)
                    ))
                    .SumAsync(rb => (int?)rb.Quantity) ?? 0;

                if(overlappingQuantity + item.Quantity > bike.Quantity)
                {
                    return BadRequest($" '{bike.Description}' is not available in requested quantity for selected dates.");
                }

                reservation.ReservationBikes.Add(new ReservationBike
                {
                    BikeId = bike.BikeId,
                    Quantity = item.Quantity
                });

                var days = ((dto.EndDate ?? dto.StartDate) - dto.StartDate).Days + 1;
                reservation.TotalPrice += bike.Price * item.Quantity * days;

            }


            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), 
                new { id = reservation.ReservationId },
                new { reservation.ReservationId });
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if(reservation == null || reservation.Status == "Cancelled");
                return NotFound();

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{bikeId}/unavailable")]
        public async Task<ActionResult<List<(DateTime start, DateTime end)>>> GetUnavailable(int bikeId)
        {
            var periods = await _context.ReservationBikes
        .Where(rb => rb.BikeId == bikeId && rb.Reservation.Status != "Cancelled")
        .Select(rb => new { rb.Reservation.StartDate, rb.Reservation.EndDate })
        .ToListAsync();

            return periods
                .Select(p => (p.StartDate.Date, (p.EndDate ?? p.StartDate).Date))
                .ToList();
        }


        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            
            var reservations = await _context.Reservations
                .Include(r => r.ReservationBikes)
                    .ThenInclude(rb => rb.Bike)
                .ToListAsync();

            
            var reservationDtos = reservations.Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                StartDate = r.StartDate,
                
                EndDate = r.EndDate ?? r.StartDate,
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                ReservationBikes = r.ReservationBikes.Select(rb => new ReservationBikeDto
                {
                    BikeId = rb.BikeId,
                    Quantity = rb.Quantity,
                    Bike = rb.Bike != null ? new BikeDto
                    {
                        BikeId = rb.Bike.BikeId,
                        Description = rb.Bike.Description,
                        Brand = rb.Bike.Brand,
                        Color = rb.Bike.Color,
                        Size = rb.Bike.Size,
                        ImageUrl = rb.Bike.ImageUrl,
                        Price = rb.Bike.Price,
                        Category = rb.Bike.Category,
                        Quantity = rb.Bike.Quantity
                    } : null
                }).ToList()
            }).ToList();

            return Ok(reservationDtos);
        }



        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.ReservationBikes).ThenInclude(rb => rb.Bike).FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();
            return reservation;

            
        }

        // PUT: api/Reservations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                return BadRequest();
            }

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }




        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.ReservationBikes).FirstOrDefaultAsync(r => r.ReservationId == id);

            if(reservation == null) return NotFound();

            _context.ReservationBikes.RemoveRange(reservation.ReservationBikes);
            _context.Reservations.Remove(reservation);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
