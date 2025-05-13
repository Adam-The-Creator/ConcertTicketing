using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcertTicketing.Server.Models;
using ConcertTicketing.Server.Data.Context;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConcertTicketing.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly ConcertTicketingDBContext _context;

        public StatsController(ConcertTicketingDBContext context) => _context = context;

        // GET: /api/stats/events/count
        [HttpGet("events/count")]
        public async Task<IActionResult> GetEventCount()
        {
            var count = await _context.Concerts.CountAsync();
            return Ok(new { count });
        }

        // GET: /api/stats/tickets/sold
        [HttpGet("tickets/sold")]
        public async Task<IActionResult> GetTotalTicketsSold()
        {
            var total = await _context.Tickets
                .Include(t => t.TicketStatus)
                .CountAsync(t => t.TicketStatus!.Status == "Paid");

            return Ok(new { total });
        }

        // GET: /api/stats/tickets/sold/today
        [HttpGet("tickets/sold/today")]
        public async Task<IActionResult> GetTicketsSoldToday()
        {
            var today = DateTime.Today;
            var count = await _context.Tickets
                .Include(t => t.TicketStatus)
                .CountAsync(t =>
                    t.TicketStatus!.Status == "Paid" &&
                    t.PurchaseDate.HasValue &&
                    t.PurchaseDate.Value.Date == today);

            return Ok(new { today = count });
        }

        // GET: /api/stats/income/monthly
        [HttpGet("income/monthly")]
        public async Task<IActionResult> GetMonthlyIncome()
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var income = await _context.Tickets
                .Include(t => t.TicketStatus)
                .Where(t =>
                    t.TicketStatus!.Status == "Paid" &&
                    t.PurchaseDate >= startOfMonth)
                .SumAsync(t => t.TicketDetail!.Price);

            return Ok(new { amount = income });
        }
    }
}
