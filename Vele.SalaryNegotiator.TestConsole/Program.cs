using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Serilog;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Services;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;
using Vele.SalaryNegotiator.Core.Generators;
using System;

namespace Vele.SalaryNegotiator.TestConsole;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog(dispose: true))
            .AddSingleton<ICodeGenerator, WordCodeGenerator>()
            .AddSingleton<ISecretGenerator, GuidSecretGenerator>()
            .AddScoped<INegotiationService, NegotiationService>()
            .AddDbContext<SalaryNegotiatorDbContext>(db =>
            {
                db.UseSqlite("Data source=SalaryNegotiator_Test.db");
                db.EnableSensitiveDataLogging();
                db.EnableDetailedErrors();
            })
            .BuildServiceProvider();

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            SalaryNegotiatorDbContext dbContext = scope.ServiceProvider.GetRequiredService<SalaryNegotiatorDbContext>();
            //dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        INegotiationService ns = serviceProvider.GetRequiredService<INegotiationService>();

        return 0;
    }
}