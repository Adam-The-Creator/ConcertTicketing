using ConcertTicketing.Server.Data.Context;
using ConcertTicketing.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ILoadBalancedConcertTicketingDBContextFactory_Read _readDBContextFactory;
        private readonly ConcertTicketingDBContext_Write _writeDBContext;

        public GenresController(ILoadBalancedConcertTicketingDBContextFactory_Read readContext, ConcertTicketingDBContext_Write writeContext)
        {
            _readDBContextFactory = readContext;
            _writeDBContext = writeContext;
        }

        // GET: api/genres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var genres = await _readDBContext.Genres
                .OrderBy(g => g.GenreName)
                .Select(g => new GenreDto
                {
                    Id = g.Id,
                    GenreName = g.GenreName
                })
                .ToListAsync();

            return Ok(genres);
        }

        // POST: api/genres
        [HttpPost]
        public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateGenreDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.GenreName))
            {
                return BadRequest("Genre name is required.");
            }

            var newGenre = new Genre
            {
                GenreName = dto.GenreName.Trim()
            };

            _writeDBContext.Genres.Add(newGenre);
            await _writeDBContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenres), new { id = newGenre.Id }, new GenreDto
            {
                Id = newGenre.Id,
                GenreName = newGenre.GenreName
            });
        }
    }

    public class GenreDto
    {
        public int Id { get; set; }
        public string? GenreName { get; set; }
    }

    public class CreateGenreDto
    {
        public string GenreName { get; set; } = string.Empty;
    }
}
