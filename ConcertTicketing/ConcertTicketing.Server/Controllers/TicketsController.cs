using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcertTicketing.Server.Models;
using ConcertTicketing.Server.Data.Context;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ConcertTicketingDBContext _context;

        public TicketsController(ConcertTicketingDBContext context) => _context = context;

        // GET: api/tickets/available/5
        [HttpGet("available/{concertId:long}")]
        public async Task<ActionResult<int>> GetAvailableTickets(long concertId)
        {
            var availableStatus = await _context.TicketStatuses
                .Where(ts => ts.Status == "Available")
                .Select(ts => ts.Id)
                .FirstOrDefaultAsync();

            if (availableStatus == 0)
            {
                return NotFound("Available status not found.");
            }

            var count = await _context.Tickets
                .Where(t => t.ConcertId == concertId && (t.PurchaseDate == null || t.TicketStatusId == availableStatus))
                .CountAsync();

            return Ok(count);
        }
    }
}
