using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MyAdvisor.Application.DTOs.Common;
using MyAdvisor.Infrastructure;
using MyAdvisor.Infrastructure.Persistence;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5103", "https://localhost:7192")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

await MigrateAsync(app.Services, app.Logger);

static async Task MigrateAsync(IServiceProvider services, ILogger logger)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            db.Database.Migrate();
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt}/10 failed. Retrying in 3s.", attempt);
            await Task.Delay(3000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    var ex = feature?.Error;

    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode = ex switch
    {
        KeyNotFoundException => StatusCodes.Status404NotFound,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    var body = JsonSerializer.Serialize(new ErrorResponse(ex?.Message ?? "An unexpected error occurred."));
    await ctx.Response.WriteAsync(body);
}));

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
