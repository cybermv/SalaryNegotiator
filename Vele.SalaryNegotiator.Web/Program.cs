using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Generators;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;
using Vele.SalaryNegotiator.Core.Services;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using Vele.SalaryNegotiator.Web;
using Vele.SalaryNegotiator.Web.Exceptions;
using Vele.SalaryNegotiator.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Hour));

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

// builder.Services.AddGrpc(configuration =>
// {
//     configuration.Interceptors.Add<GrpcExceptionInterceptor>();
// });

builder.Services
    .AddSingleton<ICodeGenerator, WordCodeGenerator>()
    .AddSingleton<ISecretGenerator, GuidSecretGenerator>()
    .AddScoped<INegotiationService, NegotiationService>()
    .AddScoped<IAdminService, AdminService>()
    .AddDbContext<SalaryNegotiatorDbContext>(db =>
    {
        // db.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
        //db.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
        db.UseSqlite("Data source=SalaryNegotiator_Web.db");
        //db.EnableSensitiveDataLogging();
        //db.EnableDetailedErrors();
    });

builder.Services.AddAutoMapper(typeof(AutomapperConfiguration).Assembly);

WebApplication app = builder.Build();

// Create the database if it doesn't exist.
using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        SalaryNegotiatorDbContext dbContext = scope.ServiceProvider.GetRequiredService<SalaryNegotiatorDbContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger>().Error(ex, "Error occurred while attempting to migrate database");
        throw;
    }
}

// Build the middleware pipeline.
app.UseDeveloperExceptionPage();

app.UseFileServer();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseCors(x => x.WithOrigins("http://localhost:4200")
    .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
}

app.UseAuthorization();

app.MapControllers();

// app.MapGrpcService<NegotiationServiceImpl>();
// app.MapGrpcService<AdminServiceImpl>();

await app.RunAsync();
