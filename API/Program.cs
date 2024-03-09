//Creates a WebApplicationBuilder object responsible for setting up the application's services and configuration.
using API.Extensions;
using API.Middleware;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.ApplicationServices(builder.Configuration);

var app = builder.Build();

//Below all are middleware
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

//Redirect server side error to api controller
app.UseStatusCodePagesWithRedirects("/error/{0}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

//Auto migration on start-up
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<StoreContext>();
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    //do migration
    await context.Database.MigrateAsync();

    //do seeding
    await StoreContextSeed.SeedAsync(context);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
