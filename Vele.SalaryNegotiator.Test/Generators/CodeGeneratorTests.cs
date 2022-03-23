using NUnit.Framework;
using Vele.SalaryNegotiator.Core.Generators;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;

namespace Vele.SalaryNegotiator.Test.Generators;

[TestFixture]
public class CodeGeneratorTests
{
    private ICodeGenerator _codeGenerator;

    [SetUp]
    public void Setup()
    {
        _codeGenerator = new WordCodeGenerator();
    }

    [Test]
    public void Generate_ReturnsValidCode()
    {
        string code = _codeGenerator.Generate();

        Assert.That(code, Is.Not.Null.And.Not.Empty);
        Assert.That(code, Has.Length.EqualTo(12));
    }
}
