using ConcertTicketing.Server.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ConcertTicketingDBContext _context;

        public EventsController(ConcertTicketingDBContext context) => _context = context;

        // GET: api/events/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<UpcomingEvents>>> GetUpcomingConcerts()
        {
            var upcomingStatusId = await _context.ConcertStatuses.Where(s => s.Status == "Upcoming").Select(s => s.Id).FirstOrDefaultAsync();

            var upcomingEvents = await _context.Concerts
                .Where(c => c.StatusId == upcomingStatusId && c.Date >= DateTime.Now)
                .OrderBy(c => c.Date)
                .Select(c => new UpcomingEvents
                {
                    ID = c.Id,
                    ConcertName = c.ConcertName,
                    Date = c.Date,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();

            return Ok(upcomingEvents);
        }

        // GET: api/events/eventdetails/{eventId}
        [HttpGet("eventdetails/{eventId}")]
        public async Task<ActionResult<EventDetails>> GetEventDetails(int eventId)
        {
            var concert = await _context.Concerts
                .Include(c => c.Venue)
                .Include(c => c.MainArtist)
                .Include(c => c.ArtistRolesAtConcerts)
                .ThenInclude(arac => arac.Artist)
                .ThenInclude(a => a.Genres)
                .Where(c => c.Id == eventId)
                .FirstOrDefaultAsync();
            
            if (concert == null)
            {
                return NotFound();
            }

            var allArtists = concert.ArtistRolesAtConcerts
                .Select(arac => arac.Artist.ArtistName)
                .Distinct()
                .ToList();

            var allGenres = concert.ArtistRolesAtConcerts
                .SelectMany(arac => arac.Artist.Genres)
                .Concat(concert.MainArtist.Genres)
                .Select(g => g.GenreName)
                .Distinct()
                .ToList();

            var eventDetails = new EventDetails
            {
                ID = concert.Id,
                ConcertName = concert.ConcertName,
                Description = concert.Description,
                Date = concert.Date,
                ImageUrl = concert.ImageUrl,
                VenueName = concert.Venue.Name,
                VenueLocation = concert.Venue.Location,
                MainArtistName = concert.MainArtist.ArtistName,
                Artists = allArtists,
                Genres = allGenres
            };

            return Ok(eventDetails);
        }

    }

    public class EventDetails
    {
        public long ID { get; set; }
        public string ConcertName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? ImageUrl { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueLocation { get; set; } = null!;
        public string MainArtistName { get; set; } = null!;
        public List<string> Artists { get; set; } = [];
        public List<string> Genres { get; set; } = [];
    }

    public class UpcomingEvents
    {
        public long ID { get; set; }
        public string ConcertName { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? ImageUrl { get; set; }
    }
}
