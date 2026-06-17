using TaskManagement.Extensions;

namespace TaskManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddApiServices()
                .AddJwtAuthentication(builder.Configuration)
                .AddTaskManagementDbContext(builder.Configuration);

            var app = builder.Build();

            app.ConfigureSwagger(app.Environment)
               .ConfigureApiPipeline();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.Run();
        }
    }
}
