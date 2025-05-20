using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentFlow.Models;

public partial class Bike
{
    [Key]
    public int BikeId { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Brand { get; set; }

    [Required]
    [StringLength(50)]
    public string Color { get; set; }

    [Required]
    [StringLength(20)]
    public string Size { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; }

    [Required]
    public int Quantity { get; set; }

    [InverseProperty("Bike")]
    public virtual ICollection<ReservationBike> ReservationBikes { get; set; } = new List<ReservationBike>();
}
