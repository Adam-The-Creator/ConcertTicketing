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
    }

    public class UpcomingEvents
    {
        public long ID { get; set; }
        public string ConcertName { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? ImageUrl { get; set; }
    }
}
