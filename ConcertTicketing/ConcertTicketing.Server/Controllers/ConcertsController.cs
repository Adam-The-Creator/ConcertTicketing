using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcertTicketing.Server.Models;
using ConcertTicketing.Server.Data.Context;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConcertsController : ControllerBase
    {
        private readonly ConcertTicketingDBContext _context;

        public ConcertsController(ConcertTicketingDBContext context) => _context = context;

        // POST: api/concerts
        [HttpPost]
        public async Task<IActionResult> CreateConcert([FromBody] CreateConcertDto concertDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Name == concertDTO.VenueName && v.Location == concertDTO.VenueLocation);

            if (venue == null)
            {
                venue = new Venue
                {
                    Name = concertDTO.VenueName,
                    Location = concertDTO.VenueLocation
                };
                _context.Venues.Add(venue);
                await _context.SaveChangesAsync();
            }

            var status = await _context.ConcertStatuses.FirstOrDefaultAsync(s => s.Status == concertDTO.Status);

            if (status == null)
            {
                return BadRequest($"Invalid status value: {concertDTO.Status}");
            }

            var concert = new Concert
            {
                ConcertName = concertDTO.ConcertName,
                Description = concertDTO.Description,
                Date = concertDTO.Date,
                VenueId = venue.Id,
                StatusId = status.Id
            };

            _context.Concerts.Add(concert);
            await _context.SaveChangesAsync();

            return Ok(new { concert.Id });
        }
    }

    public class CreateConcertDto
    {
        public string ConcertName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueLocation { get; set; } = null!;
        public string Status { get; set; } = "Upcoming";
    }
}
