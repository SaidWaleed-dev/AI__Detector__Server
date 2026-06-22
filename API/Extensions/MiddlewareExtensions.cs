using API.Middlewares;
using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace API.Extensions;


public static class MiddlewareExtensions
{
    
    public static WebApplication UseCustomMiddlewarePipeline(this WebApplication app)
    {
        // 1. Global Exception Handler
        app.UseMiddleware<ExceptionMiddleware>();

        // 2. Swagger Documentation in Development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Detector API v1"));
        }

        // 3. Static Files
        app.UseStaticFiles();

        // 4. CORS
        app.UseCors("AllowAll");

        // 5. Routing & Auth
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // 6. Endpoints
        app.MapControllers();

        return app;
    }

    
    public static void ApplyDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            Console.WriteLine(" Database migrations applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error creating/migrating database: {ex.Message}");
        }
    }
}
