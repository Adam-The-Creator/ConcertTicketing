using ConcertTicketing.Server.Data.Context;
using ConcertTicketing.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ConcertTicketingDBContext _context;

        public OrdersController(ConcertTicketingDBContext context) => _context = context;

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseTickets([FromBody] BuyTicketsRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
                if (user == null) return NotFound("User not found.");

                var availableStatusId = await _context.TicketStatuses.Where(ts => ts.Status == "Available").Select(ts => ts.Id).FirstOrDefaultAsync();

                if (availableStatusId == 0) return NotFound("Available status not found.");

                var reservedStatusId = await _context.TicketStatuses.Where(ts => ts.Status == "Reserved").Select(ts => ts.Id).FirstOrDefaultAsync();

                var tickets = await _context.Tickets.Where(t => t.ConcertId == request.ConcertId && t.TicketStatusId == availableStatusId).Take(request.Quantity).ToListAsync();

                if (tickets.Count < request.Quantity) return BadRequest("Not enough available tickets.");

                foreach (var ticket in tickets)
                {
                    ticket.TicketStatusId = reservedStatusId;
                }
                await _context.SaveChangesAsync();

                var ticketDetailId = tickets.First().TicketDetailId;
                var pricePerTicket = await _context.TicketDetails.Where(td => td.Id == ticketDetailId).Select(td => td.Price).FirstOrDefaultAsync();

                var expectedTotal = pricePerTicket * tickets.Count;
                if (expectedTotal != request.TotalPrice)
                {
                    foreach (var ticket in tickets)
                    {
                        ticket.TicketStatusId = availableStatusId;
                    }
                    await _context.SaveChangesAsync();
                    return BadRequest("Invalid total price.");
                }

                var purchaseTime = DateTime.UtcNow;

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    OrderDate = purchaseTime,
                    PreferredDeliveryTime = purchaseTime,
                    Paid = purchaseTime,
                    Sent = purchaseTime,
                    TotalPrice = expectedTotal,
                    DiscountedPrice = expectedTotal,
                    Currency = "USD",
                    DeliveryEmail = user.Email
                };

                _context.Orders.Add(order);

                foreach (var ticket in tickets)
                {
                    _context.OrderTickets.Add(new OrderTicket
                    {
                        OrderId = order.Id,
                        TicketId = ticket.Id
                    });
                }
                await _context.SaveChangesAsync();

                var paidStatusId = await _context.TicketStatuses.Where(ts => ts.Status == "Paid").Select(ts => ts.Id).FirstOrDefaultAsync();

                if (paidStatusId == 0)
                {
                    foreach (var ticket in tickets)
                    {
                        ticket.TicketStatusId = availableStatusId;
                    }
                    await _context.SaveChangesAsync();
                    return NotFound("Paid status not found.");
                }

                foreach (var ticket in tickets)
                {
                    ticket.TicketStatusId = paidStatusId;
                    ticket.PurchaseDate = purchaseTime;
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = $"{tickets.Count} ticket(s) purchased successfully." });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet("mytickets/{userId}")]
        public async Task<IActionResult> GetUserTickets(Guid userId)
        {
            var tickets = await _context.OrderTickets
                .Where(ot => ot.Order.UserId == userId)
                .Include(ot => ot.Ticket)
                .ThenInclude(t => t.Concert)
                .Include(ot => ot.Ticket)
                .ThenInclude(t => t.TicketDetail)
                .Select(ot => new
                {
                    ConcertName = ot.Ticket.Concert.ConcertName,
                    Date = ot.Ticket.Concert.Date,
                    Venue = ot.Ticket.Concert.Venue.Name,
                    Location = ot.Ticket.Concert.Venue.Location,
                    Price = ot.Ticket.TicketDetail.Price,
                    PurchaseDate = ot.Ticket.PurchaseDate,
                    SerialNumber = ot.Ticket.SerialNumber
                })
                .ToListAsync();
            return Ok(tickets);
        }


    }

    public class BuyTicketsRequest
    {
        public long ConcertId { get; set; }
        public Guid UserId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
