using System;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;

namespace Vele.SalaryNegotiator.Core.Generators;

public class GuidSecretGenerator : ISecretGenerator
{
    public string GenerateForEmployer() => $"yer.{Guid.NewGuid():n}";

    public string GenerateForEmployee() => $"yee.{Guid.NewGuid():n}";
}
