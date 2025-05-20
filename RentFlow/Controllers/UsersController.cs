using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using RentFlow.DTO;
using RentFlow.Helpers;
using RentFlow.Models;
using RentFlow.Models.Contexts;

namespace RentFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class UsersController : ControllerBase
    {
        private readonly RentContext _context;

        public UsersController(RentContext context)
        {
            _context = context;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
                return BadRequest();

            var user = await _context.Users.FirstOrDefaultAsync(item 
                => item.Email == loginDto.Email);

            if (user == null)
                return NotFound(new { Message = "User Not Found!" });

            if(!PasswordHasher.VerifyPassword(loginDto.PasswordHash, user.PasswordHash))
            {
                return BadRequest(new { Message = "Passwrod is Incorrect" });
            }

            user.Token = CreateJwtToken(user);

            return Ok(new
            {
                Token = user.Token,
                user = new 
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                },
                Message = "Login Success!"
            });

        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest();

            //Validation
            //Check email
            if (await CheckUserEmailExistAsync(registerDto.Email))
                return BadRequest(new { Message = "Email already exists" });

            //Check password
            var password = CheckPasswordStrength(registerDto.PasswordHash);
            if (!string.IsNullOrEmpty(password))
                return BadRequest(new { Message = password.ToString() });

            var userObj = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = PasswordHasher.HashPassword(registerDto.PasswordHash),
                Role = "User",
                Token = "",
                IsActive = true
            };

            

            await _context.Users.AddAsync(userObj);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User Registered!"
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Where(item => item.UserId == userId && item.IsActive)
                .FirstOrDefaultAsync();
            if (user == null) return NotFound();

            return new UserProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                
            };
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if(user==null || !user.IsActive) return NotFound();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            if(!string.IsNullOrWhiteSpace(dto.PasswordHash)) 
                user.PasswordHash = PasswordHasher.HashPassword(dto.PasswordHash);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var u = await _context.Users.FindAsync(userId);
            if (u == null) return NotFound();

            u.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var reservations = await _context.Reservations.Where(r => r.UserId == userId).Include(r => r.ReservationBikes).ThenInclude(rb => rb.Bike).ToListAsync();

            var result = reservations.Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                UserId = r.UserId,
                StartDate = r.StartDate,
                EndDate = r.EndDate ?? default(DateTime), // Explicitly handle nullable DateTime
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                ReservationBikes = r.ReservationBikes.Select(rb => new ReservationBikeDto
                {
                    BikeId = rb.BikeId,
                    Quantity = rb.Quantity,
                    Bike = new BikeDto
                    {
                        BikeId = rb.Bike.BikeId,
                        Description = rb.Bike.Description,
                        Brand = rb.Bike.Brand,
                        Color = rb.Bike.Color,
                        Size = rb.Bike.Size,
                        ImageUrl = rb.Bike.ImageUrl,
                        Price = rb.Bike.Price,
                        Category = rb.Bike.Category
                    }
                }).ToList()
            });

            return Ok(result);
        }


        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        //METHODS
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private Task<bool> CheckUserEmailExistAsync(string email)
                => _context.Users.AnyAsync(item => item.Email == email);

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 6)
                sb.Append("The password must consist of at least six individual characters" + Environment.NewLine);

            if (!(Regex.IsMatch(password, "[a-z]")
                && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("The password must contain numbers and uppercase letter " + Environment.NewLine);

            return sb.ToString();

        }

        private string CreateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("AReallyLongAndSecureSecretKey12345678901234567890");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            });

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256
                );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

    }
}
