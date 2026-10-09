using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using EXAM_Pram.Data;

namespace EXAM_Pram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configure DbContext with SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<BattleGameDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 3. Add Controllers & JSON settings
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // 4. Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BattleGame API",
                    Version = "v1",
                    Description = "API for BattleGame (Player, Asset, PlayerAsset)"
                });
            });

            var app = builder.Build();

            // 5. Configure HTTP request pipeline
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BattleGame API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();

            // Enable serving static files from wwwroot
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Enable CORS
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
