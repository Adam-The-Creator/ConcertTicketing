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
        private readonly ILoadBalancedConcertTicketingDBContextFactory_Read _readDBContextFactory;
        private readonly ConcertTicketingDBContext_Write _writeDBContext;

        public StatsController(ILoadBalancedConcertTicketingDBContextFactory_Read readContext, ConcertTicketingDBContext_Write writeContext)
        {
            _readDBContextFactory = readContext;
            _writeDBContext = writeContext;
        }

        // GET: /api/stats/events/count
        [HttpGet("events/count")]
        public async Task<IActionResult> GetEventCount()
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var count = await _readDBContext.Concerts.CountAsync();
            return Ok(new { count });
        }

        // GET: /api/stats/tickets/sold
        [HttpGet("tickets/sold")]
        public async Task<IActionResult> GetTotalTicketsSold()
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var total = await _readDBContext.Tickets
                .Include(t => t.TicketStatus)
                .CountAsync(t => t.TicketStatus!.Status == "Paid");

            return Ok(new { total });
        }

        // GET: /api/stats/tickets/sold/today
        [HttpGet("tickets/sold/today")]
        public async Task<IActionResult> GetTicketsSoldToday()
        {
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var today = DateTime.Today;
            var count = await _readDBContext.Tickets
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
            using var _readDBContext = _readDBContextFactory.CreateContext();

            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var income = await _readDBContext.Tickets
                .Include(t => t.TicketStatus)
                .Where(t =>
                    t.TicketStatus!.Status == "Paid" &&
                    t.PurchaseDate >= startOfMonth)
                .SumAsync(t => t.TicketDetail!.Price);

            return Ok(new { amount = income });
        }
    }
}
