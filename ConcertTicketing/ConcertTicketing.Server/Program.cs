using ConcertTicketing.Server.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Add authorization services.
            builder.Services.AddAuthorization();

            // Add DB context with SQL Server.
            builder.Services.AddDbContext<ConcertTicketingDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MainSQLServerConnection"))
            );

            // Add controllers
            builder.Services.AddControllers();

            builder.Services.AddControllersWithViews();

            // Support CORS (cross-origin requests)
            var AllowClientOrigins = "AllowClientOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: AllowClientOrigins,
                    policy => policy.WithOrigins("https://localhost:54053").AllowAnyHeader().AllowAnyMethod()
                );
            });

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Enable CORS
            app.UseCors(AllowClientOrigins);

            // Enable static files and default files
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseAuthorization();

            //var summaries = new[]
            //{
            //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            //};

            //app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            //{
            //    var forecast = Enumerable.Range(1, 5).Select(index =>
            //        new WeatherForecast
            //        {
            //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //            TemperatureC = Random.Shared.Next(-20, 55),
            //            Summary = summaries[Random.Shared.Next(summaries.Length)]
            //        })
            //        .ToArray();
            //    return forecast;
            //});

            // Map API controllers
            app.MapControllers();

            // Fallback for SPA or index.html
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
