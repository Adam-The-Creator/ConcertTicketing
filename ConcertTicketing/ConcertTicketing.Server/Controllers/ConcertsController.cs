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
        private readonly ILoadBalancedConcertTicketingDBContextFactory_Read _readDBContextFactory;
        private readonly ConcertTicketingDBContext_Write _writeDBContext;

        public ConcertsController(ILoadBalancedConcertTicketingDBContextFactory_Read readContext, ConcertTicketingDBContext_Write writeContext)
        {
            _readDBContextFactory = readContext;
            _writeDBContext = writeContext;
        }

        // POST: api/concerts
        [HttpPost]
        public async Task<IActionResult> CreateConcert([FromBody] CreateConcertDto concertDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (concertDTO.Date <= DateTime.Today) return BadRequest("The date cannot be earlier or today.");

            using var transaction = await _writeDBContext.Database.BeginTransactionAsync();
            try
            {
                var venue = await _writeDBContext.Venues.FirstOrDefaultAsync(v => v.Name == concertDTO.VenueName && v.Location == concertDTO.VenueLocation);

                if (venue == null)
                {
                    venue = new Venue
                    {
                        Name = concertDTO.VenueName,
                        Location = concertDTO.VenueLocation
                    };
                    _writeDBContext.Venues.Add(venue);
                    await _writeDBContext.SaveChangesAsync();
                }

                var status = await _writeDBContext.ConcertStatuses.FirstOrDefaultAsync(s => s.Status == concertDTO.Status);

                if (status == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest($"Invalid status value: {concertDTO.Status}");
                }

                var mainArtist = await _writeDBContext.Artists.FindAsync(concertDTO.MainArtistId);

                var concert = new Concert
                {
                    ConcertName = concertDTO.ConcertName,
                    Description = concertDTO.Description,
                    MainArtist = mainArtist,
                    MainArtistId = mainArtist?.Id,
                    ImageUrl = concertDTO.ImageUrl,
                    Date = concertDTO.Date,
                    VenueId = venue.Id,
                    StatusId = status.Id
                };

                _writeDBContext.Concerts.Add(concert);
                await _writeDBContext.SaveChangesAsync();

                ArtistRole? mainArtistRole = await _writeDBContext.ArtistRoles.FirstOrDefaultAsync(r => r.RoleName == "Main Artist");

                if (mainArtistRole == null)
                {
                    mainArtistRole = new ArtistRole
                    {
                        RoleName = "Main Artist"
                    };
                    _writeDBContext.ArtistRoles.Add(mainArtistRole);
                    await _writeDBContext.SaveChangesAsync();
                }

                if (mainArtist != null)
                {
                    var artistRoleAtConcert = new ArtistRolesAtConcert
                    {
                        ConcertId = concert.Id,
                        ArtistId = mainArtist.Id,
                        RoleId = mainArtistRole.Id
                    };

                    _writeDBContext.ArtistRolesAtConcerts.Add(artistRoleAtConcert);
                    await _writeDBContext.SaveChangesAsync();
                }

                return Ok(new { concert.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public class CreateConcertDto
    {
        public string ConcertName { get; set; } = null!;
        public string? Description { get; set; }
        public long? MainArtistId { get; set; } = null;
        public string? ImageUrl { get; set; } = null;
        public DateTime Date { get; set; }
        public string VenueName { get; set; } = null!;
        public string VenueLocation { get; set; } = null!;
        public string Status { get; set; } = "Upcoming";
        public int? ConcertGroup { get; set; } = null;
    }
}
