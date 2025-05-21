using ConcertTicketing.Server.Data.Context;
using ConcertTicketing.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ILoadBalancedConcertTicketingDBContextFactory_Read _readDBContextFactory;
        private readonly ConcertTicketingDBContext_Write _writeDBContext;

        public EventsController(ILoadBalancedConcertTicketingDBContextFactory_Read readContext, ConcertTicketingDBContext_Write writeContext)
        {
            _readDBContextFactory = readContext;
            _writeDBContext = writeContext;
        }

        // GET: api/events/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<UpcomingEvents>>> GetUpcomingConcerts()
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var upcomingStatusId = await _readDBContext.ConcertStatuses.Where(s => s.Status == "Upcoming").Select(s => s.Id).FirstOrDefaultAsync();

            var upcomingEvents = await _readDBContext.Concerts
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

        [HttpGet("eventdetails/{eventId}")]
        public async Task<ActionResult<EventDetails>> GetEventDetails(int eventId)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var concert = await _readDBContext.Concerts
                .Include(c => c.Venue)
                .Include(c => c.MainArtist)
                .Where(c => c.Id == eventId)
                .FirstOrDefaultAsync();

            if (concert == null)
                return NotFound();

            var mainArtistId = concert.MainArtistId;

            var artistAtConcert = await _readDBContext.ArtistRolesAtConcerts
                .Include(arac => arac.Artist)
                .ThenInclude(a => a.Genres)
                .Where(arac => arac.ConcertId == eventId)
                .ToListAsync();

            var relatedArtists = artistAtConcert
                .Select(arac => arac.Artist)
                .Distinct()
                .ToList();

            var otherArtists = relatedArtists
                .Where(a => a.Id != mainArtistId)
                .Select(a => a.ArtistName)
                .Distinct()
                .ToList();

            var mainArtistName = concert.MainArtist?.ArtistName;

            var otherArtistGenres = relatedArtists
                .Where(a => a.Id != mainArtistId)
                .SelectMany(a => a.Genres)
                .Select(g => g.GenreName)
                .ToList();

            var mainArtistGenres = concert.MainArtist?.Genres.Select(g => g.GenreName).ToList() ?? [];

            var allGenres = otherArtistGenres
                .Concat(mainArtistGenres)
                .Distinct()
                .ToList();

            var allArtists = new List<string>();
            if (!string.IsNullOrEmpty(mainArtistName)) allArtists.Add(mainArtistName);
            allArtists.AddRange(otherArtists);

            var eventDetails = new EventDetails
            {
                ID = concert.Id,
                ConcertName = concert.ConcertName,
                Description = concert.Description,
                Date = concert.Date,
                ImageUrl = concert.ImageUrl,
                VenueName = concert.Venue.Name,
                VenueLocation = concert.Venue.Location,
                MainArtistName = mainArtistName,
                MainArtistId = mainArtistId ?? 0,
                Artists = allArtists,
                Genres = allGenres
            };

            return Ok(eventDetails);
        }


        // PUT: api/events
        [HttpPut("{eventId}")]
        public async Task<ActionResult<EventDetails>> UpdateEvent(int eventId, [FromBody] EventDetails updatedEvent)
        {
            if (updatedEvent.Date <= DateTime.Today) return BadRequest("The date cannot be earlier or today.");

            using var transaction = await _writeDBContext.Database.BeginTransactionAsync();
            try
            {
                var concert = await _writeDBContext.Concerts
                .Include(c => c.Venue)
                .Include(c => c.MainArtist)
                .Where(c => c.Id == eventId)
                .FirstOrDefaultAsync();

                if (concert == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound($"Concert with ID {eventId} not found.");
                }

                concert.ConcertName = updatedEvent.ConcertName;
                concert.Description = updatedEvent.Description;
                concert.Date = updatedEvent.Date;
                concert.Venue.Name = updatedEvent.VenueName;
                concert.Venue.Location = updatedEvent.VenueLocation;
                concert.MainArtistId = updatedEvent.MainArtistId;

                try
                {
                    await _writeDBContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }

                var updatedConcert = new EventDetails
                {
                    ID = concert.Id,
                    ConcertName = concert.ConcertName,
                    Description = concert.Description,
                    Date = concert.Date,
                    ImageUrl = concert.ImageUrl,
                    VenueName = concert.Venue.Name,
                    VenueLocation = concert.Venue.Location,
                    MainArtistName = concert.MainArtist?.ArtistName,
                    MainArtistId = concert.MainArtist.Id,
                    Artists = concert.ArtistRolesAtConcerts
                                .Select(arac => arac.Artist.ArtistName)
                                .Distinct()
                                .ToList(),
                    Genres = concert.ArtistRolesAtConcerts
                                .SelectMany(arac => arac.Artist.Genres)
                                .Concat(concert.MainArtist.Genres)
                                .Select(g => g.GenreName)
                                .Distinct()
                                .ToList()
                };

                return Ok(updatedConcert);
            }
            catch (DBConcurrencyException)
            {
                if (!EventExists(eventId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool EventExists(long id)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();
            return _readDBContext.Concerts.Any(e => e.Id == id);
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
        public string? MainArtistName { get; set; } = null;
        public long MainArtistId { get; set; }
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
