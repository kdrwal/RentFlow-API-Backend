using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentFlow.Models;

[PrimaryKey("ReservationId", "BikeId")]
public partial class ReservationBike
{
    [Key]
    public int ReservationId { get; set; }

    [Key]
    public int BikeId { get; set; }

    public int Quantity { get; set; }

    [ForeignKey("BikeId")]
    [InverseProperty("ReservationBikes")]
    public virtual Bike Bike { get; set; }

    [ForeignKey("ReservationId")]
    [InverseProperty("ReservationBikes")]
    public virtual Reservation Reservation { get; set; }
}
