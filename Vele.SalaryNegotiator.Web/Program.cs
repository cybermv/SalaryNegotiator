using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text.Json.Serialization;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Generators;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;
using Vele.SalaryNegotiator.Core.Services;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console());

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        JsonConverter enumConverter = new JsonStringEnumConverter();
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddSingleton<ICodeGenerator, WordCodeGenerator>()
    .AddSingleton<ISecretGenerator, GuidSecretGenerator>()
    .AddScoped<INegotiationService, NegotiationService>()
    .AddDbContext<SalaryNegotiatorDbContext>(db =>
    {
        db.UseSqlite("Data source=SalaryNegotiator_Web.db");
        db.EnableSensitiveDataLogging();
        db.EnableDetailedErrors();
    });

WebApplication app = builder.Build();

// Create the database if it doesn't exist.
using (IServiceScope scope = app.Services.CreateScope())
{
    SalaryNegotiatorDbContext dbContext = scope.ServiceProvider.GetRequiredService<SalaryNegotiatorDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
