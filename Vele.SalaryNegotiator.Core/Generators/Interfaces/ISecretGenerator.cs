namespace Vele.SalaryNegotiator.Core.Generators.Interfaces;

public interface ISecretGenerator
{
    string GenerateForEmployer();
    string GenerateForEmployee();
}
