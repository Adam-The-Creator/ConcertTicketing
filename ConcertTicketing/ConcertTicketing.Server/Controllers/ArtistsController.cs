using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcertTicketing.Server.Data.Context;
using ConcertTicketing.Server.Models;

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly ILoadBalancedConcertTicketingDBContextFactory_Read _readDBContextFactory;
        private readonly ConcertTicketingDBContext_Write _writeDBContext;

        public ArtistsController(ILoadBalancedConcertTicketingDBContextFactory_Read readContext, ConcertTicketingDBContext_Write writeContext) {
            _readDBContextFactory = readContext;
            _writeDBContext = writeContext;
        }

        // GET: api/Artists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Artist>>> GetArtists()
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var artists = await _readDBContext.Artists
            .Select(a => new { a.Id, a.ArtistName })
            .ToListAsync();

            return Ok(artists);
        }

        // GET: api/Artists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Artist>> GetArtist(long id)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var artist = await _readDBContext.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            return artist;
        }

        // GET: api/artists/{id}/genres
        [HttpGet("{id}/genres")]
        public async Task<ActionResult<IEnumerable<GenresOfArtistDTO>>> GetArtistGenres(long id)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var artist = await _readDBContext.Artists
                .Include(a => a.Genres)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artist == null)
            {
                return NotFound();
            }

            return Ok(artist.Genres.Select(g => g.Id).ToList());
        }

        // PUT: api/Artists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArtist(long id, Artist artist)
        {
            if (id != artist.Id)
            {
                return BadRequest();
            }

            _writeDBContext.Entry(artist).State = EntityState.Modified;

            try
            {
                await _writeDBContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtistExists(id))
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

        // PUT: api/artists/{id}/genres
        [HttpPut("{id}/genres")]
        public async Task<IActionResult> UpdateArtistGenres(long id, GenreUpdateDto genresUpdateDTO)
        {
            if (genresUpdateDTO.GenreIds == null) return BadRequest();

            var artist = await _writeDBContext.Artists
                .Include(a => a.Genres)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artist == null)
            {
                return NotFound();
            }

            var newGenres = await _writeDBContext.Genres
                .Where(g => genresUpdateDTO.GenreIds.Contains(g.Id))
                .ToListAsync();

            artist.Genres.Clear();
            foreach (var genre in newGenres)
            {
                artist.Genres.Add(genre);
            }

            await _writeDBContext.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Artists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Artist>> PostArtist(Artist artist)
        {
            _writeDBContext.Artists.Add(artist);
            await _writeDBContext.SaveChangesAsync();

            return CreatedAtAction("GetArtist", new { id = artist.Id }, artist);
        }

        // DELETE: api/Artists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtist(long id)
        {
            var artist = await _writeDBContext.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            _writeDBContext.Artists.Remove(artist);
            await _writeDBContext.SaveChangesAsync();

            return NoContent();
        }

        private bool ArtistExists(long id)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();
            return _readDBContext.Artists.Any(e => e.Id == id);
        }
    }

    public class GenreUpdateDto
    {
        public List<int>? GenreIds { get; set; } = null;
    }

    public class GenresOfArtistDTO
    {
        public List<int>? GenreIds { get; set; } = null;
        public List<string>? GenreNames { get; set; } = null;
    }
}
