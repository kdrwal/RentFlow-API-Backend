using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RentFlow.Models;

[Index("Email", Name = "UQ__Users__A9D1053440B03C01", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; }

    [Required]
    [StringLength(200)]
    public string Email { get; set; }

    [Required]
    [StringLength(200)]
    public string PasswordHash { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; }

    [StringLength(500)]
    public string Token { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
