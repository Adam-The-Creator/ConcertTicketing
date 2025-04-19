using ConcertTicketing.Server.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add authorization services (if needed for your app)
            builder.Services.AddAuthorization();

            // Add DB context with SQL Server
            builder.Services.AddDbContext<ConcertTicketingDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MainSQLServerConnection"))
            );

            // Add controllers
            builder.Services.AddControllers();
            
            builder.Services.AddControllersWithViews();

            // Support CORS (cross-origin requests)
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll",
            //        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            //});

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Enable CORS
            //app.UseCors("AllowAll");

            // Enable static files and default files
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseRouting();

            // Enable authorization
            app.UseAuthorization();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            });

            // Map API controllers
            app.MapControllers();

            // Fallback for SPA or index.html
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
