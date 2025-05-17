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

        // GET: api/tickets/price/5
        [HttpGet("price/{concertId:long}")]
        public async Task<ActionResult<decimal>> GetTicketPrice(long concertId)
        {
            var availableStatusId = await _context.TicketStatuses
                .Where(ts => ts.Status == "Available")
                .Select(ts => ts.Id)
                .FirstOrDefaultAsync();

            if (availableStatusId == 0)
                return NotFound("Available status not found.");

            var ticket = await _context.Tickets
                .Where(t => t.ConcertId == concertId && t.TicketStatusId == availableStatusId)
                .Include(t => t.TicketDetail)
                .FirstOrDefaultAsync();

            if (ticket == null || ticket.TicketDetail == null)
                return NotFound("No available ticket found for this concert.");

            return Ok(ticket.TicketDetail.Price);
        }


        // POST: api/tickets/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateTickets([FromBody] TicketGenerationRequest request)
        {
            var concert = await _context.Concerts.FindAsync(request.ConcertId);
            if (concert == null)
                return NotFound("Concert not found.");

            var availableStatus = await _context.TicketStatuses
                .Where(ts => ts.Status == "Available")
                .Select(ts => ts.Id)
                .FirstOrDefaultAsync();

            if (availableStatus == 0)
                return NotFound("Available status not found.");

            var detail = new TicketDetail
            {
                Description = $"Concert: {concert.ConcertName} - Date: {concert.Date:yyyy-MM-dd}",
                Price = 0,
                StartDate = concert.Date,
                EndDate = concert.Date,
                Area = "",
                ConcertId = concert.Id
            };

            _context.TicketDetails.Add(detail);
            await _context.SaveChangesAsync();

            var tickets = new List<Ticket>();
            for (int i = 0; i < request.NumberOfTickets; i++)
            {
                tickets.Add(new Ticket
                {
                    Id = Guid.NewGuid(),
                    SerialNumber = Guid.NewGuid().ToString(),
                    ConcertId = concert.Id,
                    TicketStatusId = availableStatus,
                    TicketDetailId = detail.Id
                });
            }

            _context.Tickets.AddRange(tickets);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{request.NumberOfTickets} tickets created." });
        }
    }

    public class TicketGenerationRequest
    {
        public long ConcertId { get; set; }
        public int NumberOfTickets { get; set; }
    }
}
