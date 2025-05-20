using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentFlow.Models;

public partial class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalPrice { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; }

    [InverseProperty("Reservation")]
    public virtual ICollection<ReservationBike> ReservationBikes { get; set; } = new List<ReservationBike>();

    [ForeignKey("UserId")]
    [InverseProperty("Reservations")]
    public virtual User User { get; set; }
}
