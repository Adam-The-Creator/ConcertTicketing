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
        private readonly ILoadBalancedConcertTicketingDBContextFactory_Read _readDBContextFactory;
        private readonly ConcertTicketingDBContext_Write _writeDBContext;

        public TicketsController(ILoadBalancedConcertTicketingDBContextFactory_Read readContext, ConcertTicketingDBContext_Write writeContext)
        {
            _readDBContextFactory = readContext;
            _writeDBContext = writeContext;
        }

        // GET: api/tickets/available/5
        [HttpGet("available/{concertId:long}")]
        public async Task<ActionResult<int>> GetAvailableTickets(long concertId)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var availableStatus = await _readDBContext.TicketStatuses
                .Where(ts => ts.Status == "Available")
                .Select(ts => ts.Id)
                .FirstOrDefaultAsync();

            if (availableStatus == 0)
            {
                return NotFound("Available status not found.");
            }

            var count = await _readDBContext.Tickets
                .Where(t => t.ConcertId == concertId && (t.PurchaseDate == null || t.TicketStatusId == availableStatus))
                .CountAsync();

            return Ok(count);
        }

        // GET: api/tickets/price/5
        [HttpGet("price/{concertId:long}")]
        public async Task<ActionResult<decimal>> GetTicketPrice(long concertId)
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var availableStatusId = await _readDBContext.TicketStatuses
                .Where(ts => ts.Status == "Available")
                .Select(ts => ts.Id)
                .FirstOrDefaultAsync();

            if (availableStatusId == 0)
                return NotFound("Available status not found.");

            var ticket = await _readDBContext.Tickets
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
            if (request.StartDate > request.EndDate) return BadRequest("Start date cannot be greater than end date.");

            using var transaction = await _writeDBContext.Database.BeginTransactionAsync();
            try
            {
                var concert = await _writeDBContext.Concerts.FindAsync(request.ConcertId);
                if (concert == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Concert not found.");
                }

                if (request.StartDate < concert.Date)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Start date cannot be smaller than the date of concert.");
                }

                var availableStatus = await _writeDBContext.TicketStatuses
                    .Where(ts => ts.Status == "Available")
                    .Select(ts => ts.Id)
                    .FirstOrDefaultAsync();

                if (availableStatus == 0)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Available status not found.");
                }

                var detail = new TicketDetail
                {
                    Description = string.IsNullOrWhiteSpace(request.Description)
                        ? $"Concert: {concert.ConcertName} - Date: {concert.Date:yyyy-MM-dd}"
                        : request.Description,
                    Price = request.Price ?? 0,
                    StartDate = request.StartDate ?? concert.Date,
                    EndDate = request.EndDate ?? concert.Date,
                    Area = request.Area ?? "",
                    ConcertId = concert.Id
                };

                _writeDBContext.TicketDetails.Add(detail);
                await _writeDBContext.SaveChangesAsync();

                var tickets = new List<Ticket>();
                for (int i = 0; i < request.NumberOfTickets; i++)
                {
                    tickets.Add(new Ticket
                    {
                        Id = Guid.NewGuid(),
                        SerialNumber = Guid.NewGuid().ToString(),
                        ConcertId = concert.Id,
                        TicketStatusId = availableStatus,
                        TicketDetailId = detail.Id,
                        Seat = request.Seat
                    });
                }

                _writeDBContext.Tickets.AddRange(tickets);
                await _writeDBContext.SaveChangesAsync();

                return Ok(new { message = $"{request.NumberOfTickets} tickets created." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public class TicketGenerationRequest
        {
            public long ConcertId { get; set; }
            public int NumberOfTickets { get; set; }
            public string? Description { get; set; }
            public decimal? Price { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Area { get; set; }
            public string? Seat { get; set; }
        }
    }
}
