
using Google.GenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using SUNRISEHOTEL.Data;
using SUNRISEHOTEL.Repositories;
using SUNRISEHOTEL.Services;

namespace SUNRISEHOTEL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<SunriseHotelDbContext>(option => option.UseSqlServer(connectionString));

            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                var client = sp.GetRequiredService<Client>();

                var configuration = sp.GetRequiredService<IConfiguration>();

                var model = configuration["Gemini:Model"]
                            ?? "gemini-3.8-flash";

                return client.Models
                    .AsIChatClient(model)
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Build();
            });

            builder.Services.AddSingleton<Client>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var apiKey = configuration["Gemini:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new InvalidOperationException(
                        "Missing Gemini:ApiKey configuration."
                    );
                }

                return new Client(apiKey: apiKey);
            });

            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IHotelAssistantService, HotelAssistantService>();
            // Services
            builder.Services.AddScoped<IGeminiService, GeminiService>();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
