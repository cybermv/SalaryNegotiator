using NUnit.Framework;
using Vele.SalaryNegotiator.Core.Generators;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;

namespace Vele.SalaryNegotiator.Test.Generators;

[TestFixture]
public class SecretGeneratorTests
{
    private ISecretGenerator _secretGenerator;

    [SetUp]
    public void Setup()
    {
        _secretGenerator = new GuidSecretGenerator();
    }

    [Test]
    public void Generate_Employer_ReturnsValidSecret()
    {
        string secret = _secretGenerator.GenerateForEmployer();

        Assert.That(secret, Is.Not.Null.And.Not.Empty);
        Assert.That(secret, Contains.Substring("yer"));
    }

    [Test]
    public void Generate_Employee_ReturnsValidSecret()
    {
        string secret = _secretGenerator.GenerateForEmployee();

        Assert.That(secret, Is.Not.Null.And.Not.Empty);
        Assert.That(secret, Contains.Substring("yee"));
    }
}
