using FluentValidation;
using MediatR;
using SalesApp.Application;
using Microsoft.OpenApi.Models;
using SalesApp.Infrastructure;
using SalesApp.Infrastructure.Seed;
using Serilog;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
  .ReadFrom.Configuration(ctx.Configuration)
  .WriteTo.Console());

var conn = builder.Configuration.GetConnectionString("Default");
builder.Services.AddInfrastructure(conn);

builder.Services.AddMediatR(cfg =>
{
  cfg.RegisterServicesFromAssemblies(
    typeof(IAppDb).Assembly,
    typeof(Program).Assembly
  );
}); builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "SalesApp API", Version = "v1" });
});
builder.Services.AddCors(opt =>
{
  opt.AddDefaultPolicy(p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.Migrate();
  DbInitializer.SeedAsync(db).GetAwaiter().GetResult();
}

app.UseSerilogRequestLogging();
app.UseCors();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", ts = DateTime.UtcNow }))
   .WithName("Health");

app.Run();
