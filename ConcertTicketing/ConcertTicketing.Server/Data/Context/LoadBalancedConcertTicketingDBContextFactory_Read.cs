using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ConcertTicketing.Server.Data.Context
{
    public interface ILoadBalancedConcertTicketingDBContextFactory_Read
    {
        ConcertTicketingDBContext_Read CreateContext();
    }

    public class LoadBalancedConcertTicketingDBContextFactory_Read : ILoadBalancedConcertTicketingDBContextFactory_Read
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoadBalancedConcertTicketingDBContextFactory_Read> _logger;
        private static readonly Random _random = new();

        public LoadBalancedConcertTicketingDBContextFactory_Read(IConfiguration configuration, ILogger<LoadBalancedConcertTicketingDBContextFactory_Read> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Dummy Load Balancer logic
        public ConcertTicketingDBContext_Read CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConcertTicketingDBContext_Read>();

            var connStrings = new[]
            {
                _configuration.GetConnectionString("SecondarySQLServerConnection"),
                _configuration.GetConnectionString("Secondary2SQLServerConnection")
            };

            if (connStrings.Length == 0) throw new InvalidOperationException("No read-replica connection strings configured.");

            var selectedConn = connStrings[_random.Next(connStrings.Length)];
            optionsBuilder.UseSqlServer(selectedConn);

            _logger.LogInformation("Selected read-replica connection string: {ConnectionString}", selectedConn);

            return new ConcertTicketingDBContext_Read(optionsBuilder.Options);
        }
    }
}
